using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;
using UniformGrid = System.Windows.Controls.Primitives.UniformGrid;
using Key = System.Windows.Input.Key;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyboardFocusChangedEventArgs = System.Windows.Input.KeyboardFocusChangedEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace PrayerAutomation.UI;

public partial class MainWindow : Window
{
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");
    private static readonly string[] WeekdayShortNames = ["Sen", "Sel", "Rab", "Kam", "Jum", "Sab", "Min"];
    private static readonly CalculationMethod[] CalculationMethods =
    [
        new(20, "Indonesia — Kementerian Agama (Kemenag)", "Asia/Jakarta", 20, 18),
        new(99, "Indonesia — Muhammadiyah (hisab)", "Asia/Jakarta", 18, 18),
        new(17, "Malaysia — JAKIM", "Asia/Kuala_Lumpur", 20, 18),
        new(11, "Singapura — MUIS", "Asia/Singapore", 20, 18),
        new(4, "Arab Saudi — Umm al-Qura", "Asia/Riyadh", 18.5, 18),
        new(13, "Turki — Diyanet", "Europe/Istanbul", 18, 17),
        new(3, "Muslim World League", "Asia/Jakarta", 18, 17),
        new(2, "Amerika Utara — ISNA", "America/New_York", 15, 15),
    ];

    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _saveSettings;
    private readonly Action<string, Exception> _logSettingsError;
    private readonly Action<bool, bool> _setStartupEnabled;
    private readonly Func<bool> _isStartupEnabled;
    private readonly Func<LockScreenSafetyInfo> _inspectLockScreenSafety;
    private readonly Action<string> _applyThemePreview;
    private readonly Action _refreshDependentViews;
    private readonly Action _restoreWidgetAppearancePreview;
    private readonly Action<bool, string, string, string> _applyWidgetAppearancePreview;
    private readonly Action<bool, string> _applyTaskbarAppearancePreview;
    private readonly Action _restoreTaskbarAppearancePreview;
    private readonly LocationDetectionService _locationDetectionService = new();
    private readonly DeviceLocationService _deviceLocationService = new();
    private int _locationRequestVersion;
    private int _locationSearchVersion;
    private readonly DispatcherTimer _locationSearchTimer = new(DispatcherPriority.Background)
    {
        Interval = TimeSpan.FromMilliseconds(180)
    };


    private readonly DispatcherTimer _clockTimer;
    private readonly Dictionary<DateTime, Button> _gregorianDateButtons = [];
    private readonly Dictionary<DateTime, CalendarObservance[]> _gregorianObservances = [];
    private bool _loadingSettings;
    private bool _allowClose;
    private object?[]? _savedFormBaseline;
    private bool _resolvingUnsavedChanges;
    private bool _savingSettings;
    private WindowState _lastNonMinimizedWindowState = WindowState.Normal;
    private Func<bool, UnsavedChangesChoice> _promptUnsavedChanges;
    private int _selectedGregorianYear;
    private int _selectedHijriYear;
    private DateTime _selectedGregorianDate;
    private DateTime _lastCalendarToday;
    // A full year in each calendar creates hundreds of WPF controls.  Keep that
    // work out of the startup path so the initially visible controls accept input
    // as soon as the window is shown.
    private bool _calendarViewsLoaded;

    internal MainWindow(AppSettings settings)
        : this(settings, SettingsStore.Save, StartupRegistration.SetEnabled, StartupRegistration.IsEnabled)
    {
    }

    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled)
        : this(settings, saveSettings, setStartupEnabled, isStartupEnabled, null) { }

    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled,
        Func<LockScreenSafetyInfo>? inspectLockScreenSafety)
        : this(settings, saveSettings, setStartupEnabled, isStartupEnabled, inspectLockScreenSafety, null) { }

    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled,
        Func<LockScreenSafetyInfo>? inspectLockScreenSafety, Action<string, Exception>? logSettingsError)
    {
        _settings = settings;
        _logSettingsError = logSettingsError ?? AppLogger.Write;
        _promptUnsavedChanges = ShowUnsavedChangesDialog;
        _saveSettings = saveSettings ?? throw new ArgumentNullException(nameof(saveSettings));
        _setStartupEnabled = setStartupEnabled ?? throw new ArgumentNullException(nameof(setStartupEnabled));
        _isStartupEnabled = isStartupEnabled ?? throw new ArgumentNullException(nameof(isStartupEnabled));
        _inspectLockScreenSafety = inspectLockScreenSafety ?? LockScreenSafetyProbe.Inspect;
        _applyThemePreview = id => ((App)Application.Current).ApplyThemePreview(id);
        _refreshDependentViews = () => ((App)Application.Current).RefreshDependentViews();
        _restoreWidgetAppearancePreview = () => ((App)Application.Current).RestoreWidgetAppearancePreview();
        _applyWidgetAppearancePreview = (enabled, mode, motion, position) =>
            ((App)Application.Current).ApplyWidgetAppearancePreview(enabled, mode, motion, position);
        _applyTaskbarAppearancePreview = (enabled, mode) =>
            ((App)Application.Current).ApplyTaskbarAppearancePreview(enabled, mode);
        _restoreTaskbarAppearancePreview = () =>
            ((App)Application.Current).RestoreTaskbarAppearancePreview();
        _settings.Normalize();
        _locationSearchTimer.Tick += LocationSearchTimer_Tick;
        _loadingSettings = true;
        InitializeComponent();
        MoveCalendarCorrectionEditorsBesideDates();
        LoadLockScreenPreview();
        AttachCorrectionEditors();
        Icon = App.LoadApplicationIcon();
        Closing += OnClosing;
        StateChanged += (_, _) =>
        {
            if (WindowState != WindowState.Minimized)
            {
                _lastNonMinimizedWindowState = WindowState;
                return;
            }
            // A modal must not be owned by a minimized window. Also reject a
            // second hide/minimize while its nested dispatcher is active.
            if (_resolvingUnsavedChanges)
            {
                WindowState = _lastNonMinimizedWindowState;
                return;
            }
            if (!ConfirmUnsavedSettingsChanges()) return;
            RestoreSavedSettingsPreview();
            Hide();
        };

        InitializeYearSelectors();
        PopulateSettingsForm();
        _lastCalendarToday = GetCalendarToday();
        _clockTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (_, _) =>
        {
            UpdateClock();

            DateTime today = GetCalendarToday();
            if (today != _lastCalendarToday)
            {
                _lastCalendarToday = today;
                if (_calendarViewsLoaded)
                    RefreshCalendarViews();
                RefreshOverview();
            }
        };
        _clockTimer.Start();
        Closed += (_, _) => _clockTimer.Stop();

        RefreshOverview();
        if (!string.IsNullOrWhiteSpace(SettingsStore.LoadWarning))
            SettingsStatusLabel.Text = SettingsStore.LoadWarning;
    }

    internal void AllowClose() => _allowClose = true;

    internal void SelectCalendarTab()
    {
        MainTabs.SelectedIndex = 1;
        EnsureCalendarViewsLoaded();
        Activate();
    }

    internal void SelectSettingsTab()
    {
        MainTabs.SelectedIndex = 2;
        Activate();
    }

    internal void RefreshThemeDependentViews()
    {
        if (_calendarViewsLoaded)
            RefreshCalendarViews();
        RefreshOverview();
        RefreshSettingsSchedulePreview();
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source == MainTabs && MainTabs.SelectedIndex == 1)
            EnsureCalendarViewsLoaded();
    }

    internal void RefreshOverview()
    {
        UpdateClock();
        DateTime calendarToday = GetCalendarToday();
        DateTime scheduleDate = GetPrayerScheduleDate();
        var todayLabel = $"{calendarToday.ToString("dddd, d MMMM yyyy", IndonesianCulture)}";
        try
        {
            NativeCalendarDate hijri = HijriForCalendarDate(calendarToday);
            todayLabel += $"  ·  {hijri.Day} {NativePrayerCore.HijriMonthName(hijri.Month)} {hijri.Year} H";
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex))
        {
            AppLogger.Write("overview.hijri", ex);
        }
        TodayLabel.Text = todayLabel;
        LocationSummary.Text = $"{_settings.LocationName}  ·  {_settings.TimeZoneId}  ·  {_settings.CalculationStandard}";
        OverviewMethodLabel.Text = _settings.CalculationStandard;
        OverviewCalculationNote.Text = $"Jadwal {scheduleDate.ToString("d MMMM yyyy", IndonesianCulture)} dihitung tanpa internet (metode {_settings.MethodId}). Sudut dan sumber mengikuti metode yang dipilih. Koreksi menit dapat diatur.";
        OverviewReminderStatus.Text = _settings.RemindersEnabled
            ? $"Pengingat teks: aktif — {_settings.ReminderMinutes} menit sebelumnya (tanpa suara)."
            : "Pengingat teks: nonaktif.";
        OverviewCalendarStatus.Text = "Kalender Masehi dan Hijriyah sipil dapat dikoreksi tanpa mengubah jadwal sholat. Hari besar adalah perkiraan, bukan hasil rukyat atau SKB resmi.";

        ScheduleOverviewGrid.Children.Clear();
        try
        {
            IReadOnlyList<PrayerEntry> entries = PrayerScheduleService.ForDate(scheduleDate, _settings);
            RenderScheduleEntries(ScheduleOverviewGrid, entries, columns: 1);
            FooterStatus.Text = "Aktif di area notifikasi sistem. Tutup jendela untuk menyembunyikannya; gunakan Keluar untuk menutup aplikasi.";
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            AppLogger.Write("overview.schedule", ex);
            ResetScheduleGrid(ScheduleOverviewGrid, 1);
            TextBlock message = CreateScheduleErrorText(
                "Jadwal belum dapat dihitung. Periksa lokasi, zona waktu, metode, dan mesin perhitungan.");
            Grid.SetColumnSpan(message, 1);
            ScheduleOverviewGrid.Children.Add(message);
            FooterStatus.Text = "Jadwal belum tersedia; rincian teknis disimpan di log aplikasi.";
        }
    }

    private TextBlock CreateScheduleErrorText(string text) => new()
    {
        Text = text,
        Style = (Style)FindResource("FormHelpText"),
        Foreground = MakeBrush(ThemeManager.Current.ErrorText)
    };

    private static void ResetScheduleGrid(Grid target, int columns)
    {
        target.Children.Clear();
        target.RowDefinitions.Clear();
        target.ColumnDefinitions.Clear();
        for (int column = 0; column < columns; column++)
            target.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    }

    private static void RenderScheduleEntries(Grid target, IReadOnlyList<PrayerEntry> entries, int columns,
        Func<PrayerEntry, UIElement?>? footerFactory = null)
    {
        columns = Math.Max(1, columns);
        ResetScheduleGrid(target, columns);
        int rowCount = (entries.Count + columns - 1) / columns;
        for (int row = 0; row < rowCount; row++)
            target.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        IReadOnlyList<GridCellPosition> positions = PrayerUiLogic.ColumnFirstPositions(entries.Count, columns);
        for (int index = 0; index < entries.Count; index++)
        {
            UIElement item = CreatePrayerScheduleCard(entries[index], footerFactory?.Invoke(entries[index]));
            Grid.SetColumn(item, positions[index].Column);
            Grid.SetRow(item, positions[index].Row);
            target.Children.Add(item);
        }
    }

    private static UIElement CreatePrayerScheduleCard(PrayerEntry entry, UIElement? footer = null)
    {
        ThemePalette palette = ThemeManager.Current;
        string prayerColor = ThemeCatalog.PrayerColor(palette, entry.Key);
        PrayerPresentation presentation = PrayerUiLogic.PresentationFor(entry.Key, entry.Label) with
        {
            Foreground = prayerColor,
            Background = palette.SurfaceAlt
        };
        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        UIElement icon = CreatePrayerIcon(entry.Key, presentation);
        Grid.SetColumn(icon, 0);
        content.Children.Add(icon);
        var label = new TextBlock
        {
            Text = entry.Label,
            Margin = new Thickness(5, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
            Foreground = MakeBrush(palette.PrimaryText)
        };
        Grid.SetColumn(label, 1);
        content.Children.Add(label);
        var time = new TextBlock
        {
            Text = FormatTime(entry.Time),
            Margin = new Thickness(2, 0, 1, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.SemiBold,
            Foreground = MakeBrush(presentation.Foreground)
        };
        Grid.SetColumn(time, 2);
        content.Children.Add(time);

        var card = new Border
        {
            Margin = new Thickness(1),
            Padding = new Thickness(4, 3, 4, 3),
            MinHeight = 30,
            Background = MakeBrush(presentation.Background),
            BorderBrush = MakeBrush(palette.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(palette.CornerRadius),
            Child = content
        };
        AutomationProperties.SetAutomationId(card, $"PrayerTimeCard-{entry.Key}");
        AutomationProperties.SetName(card, $"{entry.Label}, {FormatTime(entry.Time)}");
        AutomationProperties.SetHelpText(card, presentation.AccessibleDescription);
        if (footer is null) return card;
        if (footer is FrameworkElement footerElement)
            footerElement.HorizontalAlignment = HorizontalAlignment.Center;
        // Stretch the card to the full grid column so the prayer indicators stay wide and
        // nearly touch each other; the correction row is centered underneath.
        var item = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(2, 0, 2, 0) };
        card.Margin = new Thickness(0);
        item.Children.Add(card);
        item.Children.Add(footer);
        return item;
    }

    private void MoveCalendarCorrectionEditorsBesideDates()
    {
        MoveToHost(CalendarDateCorrectionBox.Parent as FrameworkElement, GregorianDateCorrectionHost);
        MoveToHost(HijriDateCorrectionBox.Parent as FrameworkElement, HijriDateCorrectionHost);
    }

    private static void MoveToHost(FrameworkElement? editor, System.Windows.Controls.Panel host)
    {
        if (editor is null || editor.Parent is not System.Windows.Controls.Panel parent) return;
        parent.Children.Remove(editor);
        host.Children.Add(editor);
    }

    private UIElement? CreatePrayerCorrectionEditor(PrayerEntry entry)
    {
        StackPanel? editor = entry.Key switch
        {
            "Fajr" => CorrectionFajrBox.Parent as StackPanel,
            "Sunrise" => CorrectionSunriseBox.Parent as StackPanel,
            // Duha is derived from sunrise, so expose its single meaningful
            // adjustment rather than showing two competing inputs.
            "Dhuha" => DhuhaAfterSunriseBox.Parent as StackPanel,
            "Dhuhr" => CorrectionDhuhrBox.Parent as StackPanel,
            "Asr" => CorrectionAsrBox.Parent as StackPanel,
            "Maghrib" => CorrectionMaghribBox.Parent as StackPanel,
            "Isha" => CorrectionIshaBox.Parent as StackPanel,
            _ => null
        };
        if (editor is null) return null;
        if (editor.Parent is System.Windows.Controls.Panel parent) parent.Children.Remove(editor);

        foreach (TextBlock label in editor.Children.OfType<TextBlock>())
            label.Visibility = Visibility.Collapsed;
        editor.HorizontalAlignment = HorizontalAlignment.Center;
        editor.Margin = new Thickness(0, 1, 0, 0);
        return editor;
    }

    private static UIElement CreatePrayerIcon(string key, PrayerPresentation presentation)
    {
        var icon = new System.Windows.Controls.Image
        {
            Source = PrayerWidgetIcons.Create(key, MakeBrush(presentation.Foreground)),
            Width = 24,
            Height = 24,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = presentation.IconKind == PrayerIconKind.Moon ? new Thickness(0) : new Thickness(0, 0, 2, 0)
        };
        AutomationProperties.SetName(icon, presentation.AccessibleDescription);
        return icon;
    }

    private void InitializeYearSelectors()
    {
        DateTime today = GetCalendarToday();
        int gregorianStart = Math.Max(1, today.Year - 100);
        CalendarYearBox.ItemsSource = Enumerable.Range(gregorianStart, today.Year + 100 - gregorianStart + 1).ToArray();
        _selectedGregorianYear = today.Year;
        CalendarYearBox.SelectedItem = _selectedGregorianYear;
        _selectedGregorianDate = today;

        int currentHijriYear = 1448;
        try
        {
            currentHijriYear = HijriForCalendarDate(today).Year;
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.initial-hijri-year", ex);
            try { currentHijriYear = new UmAlQuraCalendar().GetYear(today.AddDays(_settings.HijriDateCorrectionDays)); }
            catch (ArgumentOutOfRangeException) { currentHijriYear = Math.Max(1, today.Year - 579); }
        }
        currentHijriYear = Math.Clamp(currentHijriYear, 1, 9665);
        int hijriStart = Math.Max(1, currentHijriYear - 100);
        int hijriEnd = Math.Min(9666, currentHijriYear + 100);
        HijriYearBox.ItemsSource = Enumerable.Range(hijriStart, hijriEnd - hijriStart + 1).ToArray();
        _selectedHijriYear = currentHijriYear;
        HijriYearBox.SelectedItem = currentHijriYear;
    }

    private void RefreshCalendarViews()
    {
        DateTime correctedToday = GetCalendarToday();
        _lastCalendarToday = correctedToday;
        if (CalendarYearBox.ItemsSource is IEnumerable<int> gregorianYears && !gregorianYears.Contains(correctedToday.Year))
        {
            int start = Math.Max(1, correctedToday.Year - 100);
            CalendarYearBox.ItemsSource = Enumerable.Range(start, correctedToday.Year + 100 - start + 1).ToArray();
        }
        _selectedGregorianYear = correctedToday.Year;
        _selectedGregorianDate = correctedToday;
        CalendarYearBox.SelectedItem = _selectedGregorianYear;

        try
        {
            _selectedHijriYear = Math.Clamp(HijriForCalendarDate(correctedToday).Year, 1, 9666);
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.refresh-hijri-year", ex);
        }
        HijriYearBox.SelectedItem = _selectedHijriYear;
        RenderGregorianYear(_selectedGregorianYear);
        RenderHijriYear(_selectedHijriYear);
    }

    private void EnsureCalendarViewsLoaded()
    {
        if (_calendarViewsLoaded) return;
        _calendarViewsLoaded = true;
        RefreshCalendarViews();
    }

    private void RenderGregorianYear(int year)
    {
        CalendarGrid.Children.Clear();
        _gregorianDateButtons.Clear();
        _gregorianObservances.Clear();
        if (_selectedGregorianDate.Year != year)
            _selectedGregorianDate = year == GetCalendarToday().Year ? GetCalendarToday() : new DateTime(year, 1, 1);
        try
        {
            DateTime today = GetCalendarToday();
            for (int month = 1; month <= 12; month++)
            {
                var monthPanel = new StackPanel { Margin = new Thickness(5) };
                var monthCard = new Border
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    Background = MakeBrush(ThemeManager.Current.Surface),
                    BorderBrush = MakeBrush(ThemeManager.Current.Border),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(ThemeManager.Current.CornerRadius),
                    Child = monthPanel
                };
                monthPanel.Children.Add(new TextBlock
                {
                    Text = $"{IndonesianCulture.DateTimeFormat.GetMonthName(month)} {year}",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = MakeBrush(ThemeManager.Current.Accent),
                    Margin = new Thickness(2, 1, 2, 5)
                });
                var weekdayGrid = new UniformGrid { Columns = 7, Rows = 1 };
                for (int weekday = 0; weekday < 7; weekday++)
                {
                    var header = new TextBlock
                    {
                        Text = WeekdayShortNames[weekday],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = MakeBrush(ThemeManager.Current.MutedText),
                        Background = MakeBrush(ThemeManager.Current.SurfaceAlt),
                        Margin = new Thickness(1),
                        Padding = new Thickness(2, 4, 2, 4)
                    };
                    AutomationProperties.SetAutomationId(header, $"GregorianWeekdayHeader-{month:00}-{weekday}");
                    AutomationProperties.SetName(header, WeekdayShortNames[weekday]);
                    weekdayGrid.Children.Add(header);
                }
                monthPanel.Children.Add(weekdayGrid);

                var daysGrid = new UniformGrid { Columns = 7, Rows = 6 };
                int monthLength = DateTime.DaysInMonth(year, month);
                int firstWeekday = MondayBasedWeekday(new DateTime(year, month, 1));
                for (int slot = 0; slot < 42; slot++)
                {
                    int day = slot - firstWeekday + 1;
                    if (day < 1 || day > monthLength)
                    {
                        daysGrid.Children.Add(new Border { Margin = new Thickness(1), MinHeight = 28 });
                        continue;
                    }
                    DateTime date = new(year, month, day);
                    CalendarObservance[] observances = NativePrayerCore.CalendarObservances(date, GetHijriDateCorrectionDays()).ToArray();
                    bool isSelected = date == _selectedGregorianDate;
                    _gregorianObservances[date] = observances;
                    Button dateButton = CreateGregorianDateButton(date, today, observances, isSelected);
                    _gregorianDateButtons[date] = dateButton;
                    daysGrid.Children.Add(dateButton);
                }
                monthPanel.Children.Add(daysGrid);
                CalendarGrid.Children.Add(monthCard);
            }
            UpdateGregorianSelection(_selectedGregorianDate);
            FooterStatus.Text = $"Kalender Masehi {year}: 12 bulan dimuat. Penanda membedakan hari besar, puasa sunnah, dan hari tidak berpuasa; tanggal Hijriyah bersifat perkiraan sipil.";
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            AppLogger.Write("calendar.gregorian.render", ex);
            CalendarGrid.Children.Clear();
            _gregorianDateButtons.Clear();
            _gregorianObservances.Clear();
            CalendarGrid.Children.Add(new TextBlock
            {
                Text = "Kalender belum dapat dimuat karena native core tidak tersedia atau tanggal tidak valid.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = MakeBrush(ThemeManager.Current.ErrorText),
                Margin = new Thickness(10)
            });
            SelectedDateDetail.Text = "Periksa berkas prayer_core.dll dan log aplikasi.";
            FooterStatus.Text = "Kalender Gregorian gagal dimuat; rincian teknis disimpan di log aplikasi.";
        }
    }

    private Button CreateGregorianDateButton(DateTime date, DateTime today, IReadOnlyList<CalendarObservance> observances, bool isSelected)
    {
        CalendarMarker markerStyle = PrayerUiLogic.CalendarMarkerFor(date, today, isSelected, observances);
        CalendarThemeColors themeColors = ThemeCatalog.CalendarColorsFor(ThemeManager.Current, markerStyle);
        string[] names = observances.Select(item => item.Name).ToArray();
        var content = new Grid();
        var dayText = new TextBlock
        {
            Text = date.Day.ToString("00", CultureInfo.InvariantCulture),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11,
            FontWeight = markerStyle.IsToday ? FontWeights.Bold : FontWeights.Normal,
            Foreground = MakeBrush(themeColors.Foreground)
        };
        content.Children.Add(dayText);
        var todayRing = new Ellipse
        {
            Width = 23,
            Height = 23,
            Stroke = MakeBrush(themeColors.Border),
            StrokeThickness = themeColors.BorderThickness,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = markerStyle.IsToday ? Visibility.Visible : Visibility.Collapsed,
            IsHitTestVisible = false
        };
        AutomationProperties.SetAutomationId(todayRing, $"TodayRing-{date:yyyy-MM-dd}");
        AutomationProperties.SetName(todayRing, "Penanda tanggal hari ini");
        content.Children.Add(todayRing);
        if (observances.Count > 0)
        {
            var marker = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = MakeBrush(themeColors.Marker),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 2, 0)
            };
            AutomationProperties.SetAutomationId(marker, $"CalendarMarker-{date:yyyy-MM-dd}");
            AutomationProperties.SetName(marker, string.Join("; ", names));
            content.Children.Add(marker);
        }
        var button = new Button
        {
            Style = (Style)FindResource("CalendarDayButton"),
            Content = content,
            Background = MakeBrush(themeColors.Background),
            BorderBrush = MakeBrush(themeColors.Border),
            BorderThickness = new Thickness(themeColors.BorderThickness),
            MinHeight = 28,
            Padding = new Thickness(1),
            Tag = date
        };
        string weekday = date.ToString("dddd", IndonesianCulture);
        string eventDescription = names.Length == 0 ? "" : " — " + string.Join("; ", names);
        string todayDescription = markerStyle.IsToday ? " — Hari ini" : "";
        AutomationProperties.SetAutomationId(button, $"DateCell-{date:yyyy-MM-dd}");
        AutomationProperties.SetName(button, $"{weekday}, {date:dd MMMM yyyy}{todayDescription}{eventDescription}");
        AutomationProperties.SetHelpText(button, (markerStyle.IsToday ? "Penanda tanggal hari ini. " : "") +
            (names.Length == 0 ? "Tidak ada penanda khusus." : "Penanda kalender: " + string.Join("; ", names)));
        button.Click += (_, _) => SelectGregorianDate((DateTime)button.Tag);
        return button;
    }

    private void RenderHijriYear(int year)
    {
        HijriCalendarGrid.Children.Clear();
        try
        {
            DateTime calendarToday = GetCalendarToday();
            NativeCalendarDate active = HijriForCalendarDate(calendarToday);
            int totalDays = 0;
            for (int month = 1; month <= 12; month++)
            {
                NativeHijriMonth nativeMonth = NativePrayerCore.HijriMonth(year, month);
                DateTime uncorrectedStart = new(nativeMonth.GregorianYear, nativeMonth.GregorianMonth, nativeMonth.GregorianDay);
                DateTime displayStart = uncorrectedStart.AddDays(-GetHijriDateCorrectionDays());
                int firstWeekday = MondayBasedWeekday(displayStart);
                totalDays += nativeMonth.DaysInMonth;

                var monthPanel = new StackPanel { Margin = new Thickness(5) };
                var monthCard = new Border
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    Background = MakeBrush(ThemeManager.Current.Surface),
                    BorderBrush = MakeBrush(ThemeManager.Current.Border),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(ThemeManager.Current.CornerRadius),
                    Child = monthPanel
                };
                var monthTitle = new TextBlock
                {
                    Text = $"{NativePrayerCore.HijriMonthName(month)} {year} H",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = MakeBrush(ThemeManager.Current.Accent),
                    Margin = new Thickness(2, 1, 2, 5)
                };
                AutomationProperties.SetAutomationId(monthTitle, $"HijriMonth-{month:00}");
                AutomationProperties.SetName(monthTitle, monthTitle.Text);
                monthPanel.Children.Add(monthTitle);
                var weekdayGrid = new UniformGrid { Columns = 7, Rows = 1 };
                for (int weekday = 0; weekday < 7; weekday++)
                {
                    var header = new TextBlock
                    {
                        Text = WeekdayShortNames[weekday],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = MakeBrush(ThemeManager.Current.MutedText),
                        Background = MakeBrush(ThemeManager.Current.SurfaceAlt),
                        Margin = new Thickness(1),
                        Padding = new Thickness(2, 4, 2, 4)
                    };
                    AutomationProperties.SetAutomationId(header, $"HijriWeekdayHeader-{month:00}-{weekday}");
                    AutomationProperties.SetName(header, WeekdayShortNames[weekday]);
                    weekdayGrid.Children.Add(header);
                }
                monthPanel.Children.Add(weekdayGrid);
                var datesGrid = new UniformGrid { Columns = 7, Rows = 6 };
                for (int slot = 0; slot < 42; slot++)
                {
                    int day = slot - firstWeekday + 1;
                    if (day < 1 || day > nativeMonth.DaysInMonth)
                    {
                        datesGrid.Children.Add(new Border { Margin = new Thickness(1), MinHeight = 28 });
                        continue;
                    }
                    DateTime gregorian = displayStart.AddDays(day - 1);
                    bool isActiveDate = active.Year == year && active.Month == month && active.Day == day;
                    datesGrid.Children.Add(CreateHijriDateButton(year, month, day, gregorian, calendarToday, isActiveDate));
                }
                monthPanel.Children.Add(datesGrid);
                HijriCalendarGrid.Children.Add(monthCard);
            }
            HijriYearLabel.Text = $"Tahun {year} H — 12 bulan dimuat ({totalDays} hari). Kalender Hijriyah sipil/tabular; tanggal resmi dapat berbeda menurut rukyat.";
            HijriSelectedDateDetail.Text = "Pilih tanggal Hijriyah untuk melihat padanan Masehi.";
            FooterStatus.Text = $"Kalender Hijriyah {year} H: 12 bulan dan {totalDays} tanggal dimuat.";
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            AppLogger.Write("calendar.hijri.render", ex);
            HijriCalendarGrid.Children.Clear();
            HijriYearLabel.Text = $"Tahun {year} H dipilih, tetapi data bulan tidak dapat dimuat.";
            HijriSelectedDateDetail.Text = "Periksa berkas prayer_core.dll dan log aplikasi.";
            FooterStatus.Text = "Kalender Hijriyah gagal dimuat; rincian teknis disimpan di log aplikasi.";
        }
    }

    private Button CreateHijriDateButton(int year, int month, int day, DateTime gregorianDate, DateTime calendarToday, bool isActiveDate)
    {
        IReadOnlyList<CalendarObservance> observances = NativePrayerCore.CalendarObservances(gregorianDate, GetHijriDateCorrectionDays());
        CalendarMarker markerStyle = PrayerUiLogic.CalendarMarkerFor(gregorianDate, calendarToday, isActiveDate, observances);
        CalendarThemeColors themeColors = ThemeCatalog.CalendarColorsFor(ThemeManager.Current, markerStyle);
        string[] names = observances.Select(item => item.Name).ToArray();
        var content = new Grid();
        var dayText = new TextBlock
        {
            Text = day.ToString("00", CultureInfo.InvariantCulture),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11,
            FontWeight = markerStyle.IsToday ? FontWeights.Bold : FontWeights.Normal,
            Foreground = MakeBrush(themeColors.Foreground)
        };
        content.Children.Add(dayText);
        var todayRing = new Ellipse
        {
            Width = 23,
            Height = 23,
            Stroke = MakeBrush(themeColors.Border),
            StrokeThickness = themeColors.BorderThickness,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = markerStyle.IsToday ? Visibility.Visible : Visibility.Collapsed,
            IsHitTestVisible = false
        };
        AutomationProperties.SetAutomationId(todayRing, $"HijriTodayRing-{year}-{month:00}-{day:00}");
        AutomationProperties.SetName(todayRing, "Penanda tanggal Hijriyah hari ini");
        content.Children.Add(todayRing);
        if (observances.Count > 0)
        {
            var marker = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = MakeBrush(themeColors.Marker),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 2, 0)
            };
            AutomationProperties.SetAutomationId(marker, $"HijriCalendarMarker-{year}-{month:00}-{day:00}");
            AutomationProperties.SetName(marker, string.Join("; ", names));
            content.Children.Add(marker);
        }
        var button = new Button
        {
            Style = (Style)FindResource("CalendarDayButton"),
            Content = content,
            Background = MakeBrush(themeColors.Background),
            BorderBrush = MakeBrush(themeColors.Border),
            BorderThickness = new Thickness(themeColors.BorderThickness),
            MinHeight = 28,
            Padding = new Thickness(1),
            Tag = gregorianDate
        };
        AutomationProperties.SetAutomationId(button, $"HijriDateCell-{year}-{month:00}-{day:00}");
        string todayDescription = markerStyle.IsToday ? " — Hari ini" : "";
        string eventDescription = names.Length == 0 ? "" : " — " + string.Join("; ", names);
        AutomationProperties.SetName(button,
            $"{NativePrayerCore.HijriMonthName(month)} {day:00} {year} H — {gregorianDate.ToString("dddd, d MMMM yyyy", IndonesianCulture)} M{todayDescription}{eventDescription}");
        AutomationProperties.SetHelpText(button, (markerStyle.IsToday ? "Penanda tanggal Hijriyah hari ini. " : "") +
            (names.Length == 0 ? "Tanggal Hijriyah sipil/tabular dan padanan tanggal Masehi." : "Penanda kalender: " + string.Join("; ", names)));
        button.Click += (_, _) => SelectHijriDate(year, month, day, gregorianDate);
        return button;
    }

    private void SelectGregorianDate(DateTime date)
    {
        _selectedGregorianDate = date.Date;
        DateTime today = GetCalendarToday();
        foreach ((DateTime cellDate, Button button) in _gregorianDateButtons)
            UpdateGregorianDateButton(cellDate, button, today);
        UpdateGregorianSelection(_selectedGregorianDate);
    }

    private void UpdateGregorianDateButton(DateTime date, Button button, DateTime today)
    {
        bool isSelected = date == _selectedGregorianDate;
        _gregorianObservances.TryGetValue(date, out CalendarObservance[]? observances);
        observances ??= [];
        CalendarMarker markerStyle = PrayerUiLogic.CalendarMarkerFor(date, today, isSelected, observances);
        CalendarThemeColors themeColors = ThemeCatalog.CalendarColorsFor(ThemeManager.Current, markerStyle);
        button.Background = MakeBrush(themeColors.Background);
        button.BorderBrush = MakeBrush(themeColors.Border);
        button.BorderThickness = new Thickness(themeColors.BorderThickness);
        if (button.Content is Grid content)
        {
            TextBlock? dayText = content.Children.OfType<TextBlock>().FirstOrDefault();
            if (dayText is not null)
            {
                dayText.FontWeight = markerStyle.IsToday ? FontWeights.Bold : FontWeights.Normal;
                dayText.Foreground = MakeBrush(themeColors.Foreground);
            }
            foreach (Ellipse ellipse in content.Children.OfType<Ellipse>())
            {
                string automationId = AutomationProperties.GetAutomationId(ellipse);
                if (automationId.StartsWith("TodayRing-", StringComparison.Ordinal))
                {
                    ellipse.Stroke = MakeBrush(themeColors.Border);
                    ellipse.StrokeThickness = themeColors.BorderThickness;
                    ellipse.Visibility = markerStyle.IsToday ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (automationId.StartsWith("CalendarMarker-", StringComparison.Ordinal))
                {
                    ellipse.Fill = MakeBrush(themeColors.Marker);
                }
            }
        }
    }

    private void UpdateGregorianSelection(DateTime date)
    {
        try
        {
            string weekday = date.ToString("dddd", IndonesianCulture);
            string caption = $"{weekday}, {date.ToString("d MMMM yyyy", IndonesianCulture)}";
            CalendarObservance[] observances = NativePrayerCore.CalendarObservances(date, GetHijriDateCorrectionDays()).ToArray();
            if (observances.Length > 0)
                caption += "\nPenanda: " + string.Join("; ", observances.Select(item => item.Name));
            NativeCalendarDate hijri = HijriForCalendarDate(date);
            caption += $"\nHijriyah: {hijri.Day} {NativePrayerCore.HijriMonthName(hijri.Month)} {hijri.Year} H\nPerkiraan kalender sipil; dapat berbeda dari rukyat atau otoritas setempat.";
            SelectedDateDetail.Text = caption;
            AutomationProperties.SetName(SelectedDateDetail, caption);
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.gregorian.selection", ex);
            SelectedDateDetail.Text = $"{date.ToString("dddd, d MMMM yyyy", IndonesianCulture)}\nDetail Hijriyah tidak tersedia.";
        }
    }

    private void SelectHijriDate(int year, int month, int day, DateTime gregorianDate)
    {
        string name = NativePrayerCore.HijriMonthName(month);
        IReadOnlyList<CalendarObservance> observances = NativePrayerCore.CalendarObservances(gregorianDate, GetHijriDateCorrectionDays());
        string observanceDescription = observances.Count == 0 ? "" : "\nPenanda: " + string.Join("; ", observances.Select(item => item.Name));
        HijriSelectedDateDetail.Text = $"{day} {name} {year} H — {gregorianDate.ToString("dddd, d MMMM yyyy", IndonesianCulture)} M\nPerkiraan kalender sipil; dapat berbeda dari rukyat atau otoritas setempat.{observanceDescription}";
        AutomationProperties.SetName(HijriSelectedDateDetail, HijriSelectedDateDetail.Text);
    }

    private NativeCalendarDate HijriForCalendarDate(DateTime date) =>
        NativePrayerCore.HijriDate(date.Date.AddDays(GetHijriDateCorrectionDays()));

    private DateTime GetLocationNow()
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
    }

    private DateTime GetCalendarToday()
    {
        DateTime now;
        try { now = GetLocationNow(); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            now = DateTime.Now;
        }
        return PrayerUiLogic.CalendarDisplayDate(now, GetCalendarDateCorrectionDays());
    }

    private int GetCalendarDateCorrectionDays() =>
        !_loadingSettings && CorrectionInputBehavior.TryParseDateCorrection(CalendarDateCorrectionBox?.Text, out int days)
            ? days
            : _settings.CalendarDateCorrectionDays;

    private int GetHijriDateCorrectionDays() =>
        !_loadingSettings && CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox?.Text, out int days)
            ? days
            : _settings.HijriDateCorrectionDays;

    private DateTime GetPrayerScheduleDate()
    {
        DateTime now;
        try { now = GetLocationNow(); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            now = DateTime.Now;
        }
        return PrayerUiLogic.PrayerScheduleDate(now);
    }

    private static DateTime GetPrayerScheduleDate(AppSettings settings)
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(settings.TimeZoneId);
        DateTime locationLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        return PrayerUiLogic.PrayerScheduleDate(locationLocalNow);
    }

    private static DateTime GetCalendarToday(AppSettings settings)
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(settings.TimeZoneId);
        DateTime locationLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        return PrayerUiLogic.CalendarDisplayDate(locationLocalNow, settings.CalendarDateCorrectionDays);
    }

    private static int MondayBasedWeekday(DateTime date) => ((int)date.DayOfWeek + 6) % 7;

    private void UpdateClock()
    {
        DateTime now;
        try { now = GetLocationNow(); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            now = DateTime.Now;
        }
        ClockLabel.Text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private void PopulateSettingsForm()
    {
        _locationRequestVersion++;
        CloseLocationSuggestions();
        OfflineReverseGeocodeStatus.Text = string.Empty;
        _loadingSettings = true;
        try
        {
            LocationNameBox.Text = _settings.LocationName;
            LatitudeBox.Text = _settings.Latitude.ToString("G", CultureInfo.InvariantCulture);
            LongitudeBox.Text = _settings.Longitude.ToString("G", CultureInfo.InvariantCulture);
            TimeZoneBox.ItemsSource = TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames
                .Append(_settings.TimeZoneId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray();
            TimeZoneBox.Text = _settings.TimeZoneId;
            var options = CalculationMethods.ToList();
            if (!options.Any(item => item.Id == _settings.MethodId))
                options.Add(new CalculationMethod(_settings.MethodId, $"{_settings.CalculationStandard} (ID {_settings.MethodId})", _settings.TimeZoneId, 18, 18));
            MethodBox.ItemsSource = options;
            MethodBox.SelectedValue = _settings.MethodId;
            CorrectionFajrBox.Text = Correction("Fajr");
            CorrectionSunriseBox.Text = Correction("Sunrise");
            CorrectionDhuhaBox.Text = Correction("Dhuha");
            CorrectionDhuhrBox.Text = Correction("Dhuhr");
            CorrectionAsrBox.Text = Correction("Asr");
            CorrectionMaghribBox.Text = Correction("Maghrib");
            CorrectionIshaBox.Text = Correction("Isha");
            ReminderEnabledCheckBox.IsChecked = _settings.RemindersEnabled;
            ReminderMinutesBox.Text = _settings.ReminderMinutes.ToString(CultureInfo.InvariantCulture);
            ReminderMessageBox.Text = _settings.ReminderMessage;
            WidgetEnabledCheckBox.IsChecked = _settings.WidgetEnabled;
            TaskbarWidgetModeBox.SelectedItem = TaskbarWidgetModeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag as string == _settings.TaskbarWidgetMode);
            WidgetModeBox.SelectedItem = WidgetModeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetMode, StringComparison.OrdinalIgnoreCase));
            WidgetMotionBox.SelectedItem = WidgetMotionBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetMotion, StringComparison.OrdinalIgnoreCase));
            WidgetPositionBox.SelectedItem = WidgetPositionBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetPosition, StringComparison.OrdinalIgnoreCase));
            ThemeBox.SelectedItem = ThemeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.ThemeId, StringComparison.OrdinalIgnoreCase));
            DhuhaAfterSunriseBox.Text = _settings.DhuhaAfterSunriseMinutes.ToString(CultureInfo.InvariantCulture);
            CalendarDateCorrectionBox.Text = _settings.CalendarDateCorrectionDays.ToString(CultureInfo.InvariantCulture);
            HijriDateCorrectionBox.Text = _settings.HijriDateCorrectionDays.ToString(CultureInfo.InvariantCulture);
            StartupEnabledCheckBox.IsChecked = _settings.StartupEnabled;
            StartHiddenCheckBox.IsChecked = _settings.StartHidden;

            AutoLockEnabledCheckBox.IsChecked = _settings.AutoLockEnabled;
            LockScreenCheckBox.IsChecked = _settings.LockScreenEnabled && _settings.LockScreenOptIn;
            LockScreenDurationBox.Text = _settings.LockScreenDurationMinutes.ToString(CultureInfo.InvariantCulture);
            RefreshLockScreenSafety();
            UpdateTaskbarWidgetAvailability();
        }
        finally { _loadingSettings = false; }
        UpdateWidgetOptionStates();
        UpdateTaskbarWidgetModeHint();
        UpdateStartupOptionStates();
        RefreshSettingsSchedulePreview();
        _savedFormBaseline = CaptureSettingsFormDraft();
    }

    private string Correction(string key) => _settings.Corrections.TryGetValue(key, out int value)
        ? value.ToString(CultureInfo.InvariantCulture)
        : "0";

    private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || MethodBox.SelectedItem is not CalculationMethod) return;
        RefreshSettingsSchedulePreview();
    }

    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || ThemeBox.SelectedItem is not ComboBoxItem themeItem ||
            themeItem.Tag is not string themeId)
            return;

        _applyThemePreview(themeId);
    }

    private void WidgetModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings) return;
        UpdateWidgetOptionStates();
        ApplyWidgetAppearancePreview();
    }

    private void WidgetPreviewOptions_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings)
            ApplyWidgetAppearancePreview();
    }

    private void ApplyWidgetAppearancePreview()
    {
        string mode = (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetMode;
        string motion = (WidgetMotionBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetMotion;
        string position = (WidgetPositionBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetPosition;
        _applyWidgetAppearancePreview(
            WidgetEnabledCheckBox.IsChecked == true, mode, motion, position);
    }

    private void StartupEnabled_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings)
            UpdateStartupOptionStates();
    }

    private void UpdateStartupOptionStates()
    {
        if (StartHiddenCheckBox is not null && StartupEnabledCheckBox is not null)
            StartHiddenCheckBox.IsEnabled = true;
    }

    private void LoadLockScreenPreview()
    {
        if (LockScreenPreviewImage is null) return;
        string path = System.IO.Path.Combine(AppContext.BaseDirectory, "sedang_sholat.png");
        if (!File.Exists(path)) return;
        try
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            LockScreenPreviewImage.Source = image;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ArgumentException or FileFormatException)
        {
            AppLogger.Write("lock-screen.preview", ex);
        }
    }

    private void RefreshLockScreenSafety(string? status = null)
    {
        if (LockScreenCheckBox is null || LockScreenSafetyText is null || LockScreenStatus is null)
            return;

        LockScreenSafetyInfo safety = _inspectLockScreenSafety();
        bool alreadyEnabled = _settings.LockScreenOptIn && _settings.LockScreenEnabled;
        LockScreenCheckBox.IsEnabled = safety.CanChangeTemporarily || alreadyEnabled;
        LockScreenSafetyText.Text = safety.Message;
        LockScreenStatus.Text = status ?? safety.Message;
        LockScreenStatus.Visibility = status is null ? Visibility.Collapsed : Visibility.Visible;
    }

    internal void SetLockScreenStatus(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => SetLockScreenStatus(message));
            return;
        }
        RefreshLockScreenSafety(message);
    }

    internal void SetTaskbarWidgetRuntimeStatus(string message) => TaskbarWidgetStatus.Text = message;

    private void TaskbarWidgetModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings) return;
        UpdateTaskbarWidgetModeHint();
        ApplyTaskbarAppearancePreview();
    }

    private void TaskbarPreviewOptions_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings) ApplyTaskbarAppearancePreview();
    }

    private void ApplyTaskbarAppearancePreview()
    {
        if ((TaskbarWidgetModeBox.SelectedItem as ComboBoxItem)?.Tag is not string mode ||
            mode != "popup") return;
        _applyTaskbarAppearancePreview(TaskbarWidgetEnabledCheckBox.IsChecked == true, mode);
    }

    private void UpdateTaskbarWidgetModeHint()
    {
        if (TaskbarWidgetModeHint is null) return;
        TaskbarWidgetModeHint.Text = "Windows 10 versi 2004 (build 19041) atau lebih baru; Windows 11.";
    }

    private void UpdateWidgetOptionStates()
    {
        if (WidgetMotionBox is null || WidgetPositionBox is null ||
            WidgetMotionLabel is null || WidgetPositionLabel is null)
            return;

        string selectedMode = (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "essential";
        string mode = WidgetSettingsLogic.NormalizeMode(selectedMode);
        bool isEssential = mode == "essential";

        WidgetMotionBox.IsEnabled = isEssential;
        WidgetMotionLabel.IsEnabled = isEssential;
        WidgetPositionBox.IsEnabled = true;
        WidgetPositionLabel.IsEnabled = true;
    }

    private void SettingsPreviewInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_loadingSettings)
        {
            if (sender == LocationNameBox || sender == LatitudeBox || sender == LongitudeBox || sender == TimeZoneBox)
                _locationRequestVersion++;
            if (sender == LocationNameBox)
                QueueLocationSearch();
            else if (sender == LatitudeBox || sender == LongitudeBox || sender == TimeZoneBox)
                CloseLocationSuggestions();
            if ((sender == LatitudeBox || sender == LongitudeBox)
                && TryDouble(LatitudeBox.Text, out double latitude) && double.IsFinite(latitude) && latitude is >= -90 and <= 90
                && TryDouble(LongitudeBox.Text, out double longitude) && double.IsFinite(longitude) && longitude is >= -180 and <= 180)
            {
                _loadingSettings = true;
                try
                {
                    TimeZoneBox.Text = GeoTimeZone.TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
                    try
                    {
                        OfflineLocationResult? place = OfflineReverseGeocoder.Bundled.FindNearest(latitude, longitude);
                        LocationNameBox.Text = place?.DisplayName ?? "Lokasi manual";
                        OfflineReverseGeocodeStatus.Text = place is null
                            ? "Tidak ada permukiman dalam radius 75 km. Isi nama sendiri; koordinat tetap lokal dan belum disimpan."
                            : $"Nama otomatis dari GeoNames lokal: {place.DisplayName}, sekitar {place.DistanceKm:0.0} km. Perkiraan, bukan alamat; belum disimpan dan tidak dikirim ke internet.";
                    }
                    catch (Exception)
                    {
                        LocationNameBox.Text = "Lokasi manual";
                        OfflineReverseGeocodeStatus.Text = "Dataset lokal tidak dapat dibaca. Isi nama sendiri; koordinat dan zona waktu tetap di draf, tidak dikirim ke internet.";
                    }
                }
                finally { _loadingSettings = false; }
            }
            RefreshSettingsSchedulePreview();
        }
    }

    private void CalendarCorrectionInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (_loadingSettings ||
            !CorrectionInputBehavior.TryParseDateCorrection(CalendarDateCorrectionBox.Text, out _) ||
            !CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out _))
            return;

        RefreshOverview();
        if (_calendarViewsLoaded)
            RefreshCalendarViews();
        RefreshSettingsSchedulePreview();
    }

    private void QueueLocationSearch()
    {
        CloseLocationSuggestions();
        if (LocationNameBox.Text.Trim().Length >= 2)
            _locationSearchTimer.Start();
    }

    private async void LocationSearchTimer_Tick(object? sender, EventArgs e)
    {
        _locationSearchTimer.Stop();
        int version = _locationSearchVersion;
        string query = LocationNameBox.Text.Trim();
        if (_loadingSettings || query.Length < 2) return;
        try
        {
            IReadOnlyList<OfflineLocationSuggestion> results = await Task.Run(() =>
                OfflineReverseGeocoder.Bundled.Search(query, maxResults: 8));
            CompleteLocationSearch(version, query, results);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            if (version != _locationSearchVersion) return;
            OfflineReverseGeocodeStatus.Text = "Data lokasi lokal tidak dapat dibaca. Nama boleh diisi sendiri; koordinat tidak diubah dan tidak dikirim ke internet.";
        }
    }

    private void CompleteLocationSearch(int version, string query, IReadOnlyList<OfflineLocationSuggestion> results)
    {
        if (_loadingSettings || _allowClose || version != _locationSearchVersion ||
            !string.Equals(query, LocationNameBox.Text.Trim(), StringComparison.Ordinal)) return;
        LocationSuggestionsList.ItemsSource = results;
        LocationSuggestionsList.SelectedIndex = -1;
        LocationSuggestionsPopup.IsOpen = results.Count > 0 && LocationNameBox.IsKeyboardFocusWithin;
        OfflineReverseGeocodeStatus.Text = results.Count > 0
            ? "Pilihan nama dari GeoNames lokal. Pilih untuk mengisi koordinat dan zona waktu; perubahan masih draf sampai Simpan."
            : "Tidak ada nama yang cocok dalam data kota/permukiman lokal. Coba nama lain atau isi nama dan koordinat sendiri; tidak ada pencarian internet.";
    }

    private void ApplyLocationSuggestion(OfflineLocationSuggestion suggestion)
    {
        if (!LocationSuggestionsList.Items.Contains(suggestion)) return;
        string timezone;
        try
        {
            timezone = GeoTimeZone.TimeZoneLookup.GetTimeZone(suggestion.Latitude, suggestion.Longitude).Result;
            _ = PrayerScheduleService.ResolveTimeZone(timezone);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            OfflineReverseGeocodeStatus.Text = "Zona waktu lokasi ini belum dapat dicocokkan. Koordinat dan nama belum diubah; isi secara manual.";
            return;
        }
        _locationRequestVersion++;
        CloseLocationSuggestions();
        _loadingSettings = true;
        try
        {
            LocationNameBox.Text = suggestion.DisplayName;
            LatitudeBox.Text = suggestion.Latitude.ToString("G", CultureInfo.InvariantCulture);
            LongitudeBox.Text = suggestion.Longitude.ToString("G", CultureInfo.InvariantCulture);
            TimeZoneBox.Text = timezone;
        }
        finally { _loadingSettings = false; }
        OfflineReverseGeocodeStatus.Text = $"Dipilih dari GeoNames lokal: {suggestion.DisplayName}. Koordinat mewakili permukiman, bukan alamat persis; zona waktu dicocokkan lokal. Belum disimpan.";
        RefreshSettingsSchedulePreview();
        if (IsVisible && IsActive) LocationNameBox.Focus();
    }

    private void CloseLocationSuggestions()
    {
        _locationSearchVersion++;
        _locationSearchTimer.Stop();
        LocationSuggestionsPopup.IsOpen = false;
        LocationSuggestionsList.ItemsSource = null;
    }

    private void LocationSuggestionsPopup_Closed(object? sender, EventArgs e) => CloseLocationSuggestions();

    private void LocationNameBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (!LocationNameBox.IsKeyboardFocusWithin && !LocationSuggestionsList.IsKeyboardFocusWithin)
                CloseLocationSuggestions();
        }));
    }

    private void LocationNameBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { CloseLocationSuggestions(); e.Handled = true; }
        else if (e.Key == Key.Down && LocationSuggestionsPopup.IsOpen && LocationSuggestionsList.Items.Count > 0)
        {
            LocationSuggestionsList.SelectedIndex = 0;
            LocationSuggestionsList.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && LocationSuggestionsPopup.IsOpen &&
                 LocationSuggestionsList.SelectedItem is OfflineLocationSuggestion suggestion)
        {
            ApplyLocationSuggestion(suggestion);
            e.Handled = true;
        }
    }

    private void LocationSuggestionsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { CloseLocationSuggestions(); e.Handled = true; }
        else if (e.Key == Key.Enter && LocationSuggestionsList.SelectedItem is OfflineLocationSuggestion suggestion)
        {
            ApplyLocationSuggestion(suggestion);
            e.Handled = true;
        }
    }

    private void LocationSuggestionsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject origin &&
            ItemsControl.ContainerFromElement(LocationSuggestionsList, origin) is System.Windows.Controls.ListBoxItem item &&
            item.DataContext is OfflineLocationSuggestion suggestion)
        {
            ApplyLocationSuggestion(suggestion);
            e.Handled = true;
        }
    }

    private void RefreshSettingsSchedulePreview()
    {
        if (_loadingSettings) return;
        if (!TryReadSchedulePreviewSettings(out AppSettings? preview, out string error))
        {
            ResetScheduleGrid(SettingsSchedulePreviewGrid, 7);
            SettingsScheduleDateLabel.Text = "Estimasi belum tersedia.";
            SettingsScheduleHijriDateLabel.Text = string.Empty;
            SettingsSchedulePreviewStatus.Text = error;
            return;
        }

        try
        {
            AppSettings previewSettings = preview!;
            DateTime scheduleDate = GetPrayerScheduleDate(previewSettings);
            DateTime calendarDate = GetCalendarToday(previewSettings);
            IReadOnlyList<PrayerEntry> entries = PrayerScheduleService.ForDate(scheduleDate, previewSettings);
            RenderScheduleEntries(SettingsSchedulePreviewGrid, entries, columns: 7, CreatePrayerCorrectionEditor);

            SettingsScheduleDateLabel.Text = $"{calendarDate.ToString("dddd, d MMMM yyyy", IndonesianCulture)} ({previewSettings.TimeZoneId})";
            NativeCalendarDate hijri = NativePrayerCore.HijriDate(calendarDate.AddDays(previewSettings.HijriDateCorrectionDays));
            SettingsScheduleHijriDateLabel.Text = $"Hijriyah: {hijri.Day} {NativePrayerCore.HijriMonthName(hijri.Month)} {hijri.Year} H";

            string methodName = (MethodBox.SelectedItem as CalculationMethod)?.Name ?? previewSettings.CalculationStandard;
            SettingsSchedulePreviewStatus.Text = $"Metode {methodName}.";
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            AppLogger.Write("settings.schedule-preview", ex);
            ResetScheduleGrid(SettingsSchedulePreviewGrid, 7);
            TextBlock message = CreateScheduleErrorText(
                "Perkiraan belum dapat dihitung. Periksa lokasi, zona waktu, metode, dan mesin perhitungan.");
            Grid.SetColumnSpan(message, 7);
            SettingsSchedulePreviewGrid.Children.Add(message);
            SettingsScheduleDateLabel.Text = "Estimasi belum tersedia.";
            SettingsScheduleHijriDateLabel.Text = string.Empty;
            SettingsSchedulePreviewStatus.Text = "Rincian teknis disimpan di log aplikasi.";
        }
    }

    private bool TryReadSchedulePreviewSettings(out AppSettings? candidate, out string error)
    {
        candidate = null;
        error = "";
        if (MethodBox.SelectedItem is not CalculationMethod method)
        {
            error = "Pilih metode perhitungan untuk melihat estimasi.";
            return false;
        }
        if (!TryDouble(LatitudeBox.Text, out double latitude) || latitude is < -90 or > 90)
        {
            error = "Latitude harus berupa angka antara −90 dan 90.";
            return false;
        }
        if (!TryDouble(LongitudeBox.Text, out double longitude) || longitude is < -180 or > 180)
        {
            error = "Longitude harus berupa angka antara −180 dan 180.";
            return false;
        }

        string timezone = TimeZoneBox.Text.Trim();
        try { _ = PrayerScheduleService.ResolveTimeZone(timezone); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            error = "Masukkan ID timezone yang valid, misalnya Asia/Jakarta.";
            return false;
        }
        if (!TryInt(DhuhaAfterSunriseBox.Text, 0, 180, out int dhuhaAfterSunrise))
        {
            error = "Jarak Duha setelah terbit harus berupa bilangan bulat 0–180 menit.";
            return false;
        }

        if (!CorrectionInputBehavior.TryParseDateCorrection(CalendarDateCorrectionBox.Text, out int calendarCorrection) ||
            !CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out int hijriCorrection))
        {
            error = "Koreksi tanggal harus berada pada rentang ±7 hari.";
            return false;
        }

        var corrections = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, TextBox box) in CorrectionBoxes())
        {
            if (!CorrectionInputBehavior.TryParseCorrection(box.Text, out int value))
            {
                error = $"Koreksi {key} harus berupa bilangan bulat −30 sampai +30 menit.";
                return false;
            }
            corrections[key] = value;
        }

        candidate = CloneSettings(_settings);
        candidate.Latitude = latitude;
        candidate.Longitude = longitude;
        candidate.TimeZoneId = timezone;
        candidate.MethodId = method.Id;
        candidate.CalculationStandard = method.Name;
        candidate.DhuhaAfterSunriseMinutes = dhuhaAfterSunrise;
        candidate.Corrections = corrections;
        candidate.CalendarDateCorrectionDays = calendarCorrection;
        candidate.HijriDateCorrectionDays = hijriCorrection;
        candidate.LocationName = LocationNameBox.Text.Trim();
        candidate.Normalize();
        return true;
    }

    private async void DetectNetworkLocation_Click(object sender, RoutedEventArgs e)
    {
        Button button = DetectNetworkLocationButton;
        object? originalContent = button.Content;
        button.IsEnabled = false;
        button.Content = "Memeriksa dua penyedia…";
        NetworkLocationStatus.Text = "Mencari perkiraan lokasi dari jaringan…";
        int requestVersion = ++_locationRequestVersion;
        try
        {
            NetworkLocationResult result = await _locationDetectionService.DetectNetworkLocationAsync();
            if (requestVersion != _locationRequestVersion) return;
            if (!result.ProvidersAgree)
            {
                NetworkLocationStatus.Text = "Hasil jaringan tidak cukup selaras; lokasi tidak diisi. Isi secara manual atau coba lagi.";
                return;
            }

            string locationName = CombineLocationName(result.IpApi, result.IpWho);
            (double latitude, double longitude) = LocationDetectionService.Midpoint(result.IpApi, result.IpWho);
            bool wasLoading = _loadingSettings;
            _loadingSettings = true;
            try
            {
                LocationNameBox.Text = locationName;
                LatitudeBox.Text = latitude.ToString("G", CultureInfo.InvariantCulture);
                LongitudeBox.Text = longitude.ToString("G", CultureInfo.InvariantCulture);
                TimeZoneBox.Text = result.IpApi.TimeZoneId!;
            }
            finally { _loadingSettings = wasLoading; }

            NetworkLocationStatus.Text = $"Lokasi perkiraan diisi ({result.IpApi.CountryCode}); belum disimpan.";
            RefreshSettingsSchedulePreview();
        }
        catch (LocationLookupException ex)
        {
            if (requestVersion != _locationRequestVersion) return;
            AppLogger.Write("location.network-detection", ex);
            NetworkLocationStatus.Text = ex.Message;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or HttpRequestException or TaskCanceledException)
        {
            if (requestVersion != _locationRequestVersion) return;
            AppLogger.Write("location.network-detection", ex);
            NetworkLocationStatus.Text = "Deteksi lokasi jaringan gagal. Periksa koneksi internet atau isi lokasi secara manual.";
        }
        finally
        {
            button.Content = originalContent;
            button.IsEnabled = true;
        }
    }

    private async void DetectGpsLocation_Click(object sender, RoutedEventArgs e)
    {
        Button button = DetectGpsLocationButton;
        object? originalContent = button.Content;
        button.IsEnabled = false;
        button.Content = "Memeriksa lokasi perangkat…";
        GpsLocationStatus.Text = "Meminta lokasi dari Windows…";
        int requestVersion = ++_locationRequestVersion;
        try
        {
            DeviceLocationResult result = await _deviceLocationService.DetectAsync();
            if (requestVersion != _locationRequestVersion) return;
            if (!result.IsSuccess)
            {
                GpsLocationStatus.Text = result.Status switch
                {
                    DeviceLocationStatus.PermissionDenied => "Izin lokasi Windows ditolak.",
                    DeviceLocationStatus.PermissionUnspecified => "Lokasi Windows belum tersedia.",
                    DeviceLocationStatus.TimedOut => "Lokasi belum didapat; coba lagi.",
                    DeviceLocationStatus.InvalidPosition => "Koordinat perangkat tidak valid.",
                    _ => "Lokasi perangkat tidak tersedia."
                };
                return;
            }

            bool wasLoading = _loadingSettings;
            _loadingSettings = true;
            try
            {
                TimeZoneBox.Text = GeoTimeZone.TimeZoneLookup.GetTimeZone(
                    result.Latitude!.Value, result.Longitude!.Value).Result;
                LocationNameBox.Text = DeviceLocationLogic.FormatLocalLocationName(
                    result.Source, result.Latitude!.Value, result.Longitude!.Value);
                LatitudeBox.Text = result.Latitude!.Value.ToString("G", CultureInfo.InvariantCulture);
                LongitudeBox.Text = result.Longitude!.Value.ToString("G", CultureInfo.InvariantCulture);
            }
            finally { _loadingSettings = wasLoading; }
            RefreshSettingsSchedulePreview();

            GpsLocationStatus.Text = $"Lokasi perangkat diisi ({DeviceLocationLogic.SourceLabel(result.Source)}); belum disimpan.";
            OfflineLocationResult? offlinePlace = null;
            bool offlineLookupFailed = false;
            try
            {
                offlinePlace = await Task.Run(() => OfflineReverseGeocoder.Bundled.FindNearest(
                    result.Latitude!.Value, result.Longitude!.Value));
            }
            catch (Exception)
            {
                offlineLookupFailed = true;
            }

            if (requestVersion != _locationRequestVersion) return;
            if (offlinePlace is not null)
            {
                LocationNameBox.Text = offlinePlace.DisplayName;
                GpsLocationStatus.Text += $" Nama terdekat: {offlinePlace.DisplayName}.";
                OfflineReverseGeocodeStatus.Text = $"Nama lokasi perangkat dicocokkan secara lokal dengan GeoNames: {offlinePlace.DisplayName}, sekitar {offlinePlace.DistanceKm:0.0} km. Tidak ada permintaan internet.";
            }
            else if (offlineLookupFailed)
            {
                OfflineReverseGeocodeStatus.Text = "Dataset GeoNames offline tidak dapat dibaca; koordinat GPS tetap di form dan tidak dikirim ke internet.";
            }
            else
            {
                GpsLocationStatus.Text += " Nama lokal dipertahankan.";
                OfflineReverseGeocodeStatus.Text = "Tidak ada permukiman GeoNames dalam radius 75 km; label lokal awal dipertahankan. Tidak ada permintaan internet.";
            }
            RefreshSettingsSchedulePreview();
        }
        catch (Exception ex)
        {
            if (requestVersion != _locationRequestVersion) return;
            string sErrMessage = ex.HResult == unchecked((int)0x80070005)
                ? "Akses ditolak oleh pengaturan privasi lokasi Windows."
                : "Windows tidak dapat membaca lokasi perangkat. Periksa layanan dan izin lokasi.";
            GpsLocationStatus.Text = sErrMessage;
        }
        finally
        {
            button.Content = originalContent;
            button.IsEnabled = true;
        }
    }

    private void UpdateTaskbarWidgetAvailability()
    {
        TaskbarWidgetAvailability availability = TaskbarWidgetSupport.Inspect();
        TaskbarWidgetEnabledCheckBox.IsEnabled = availability.CanEnable;
        TaskbarWidgetEnabledCheckBox.IsChecked = availability.CanEnable && _settings.TaskbarWidgetEnabled;
        TaskbarWidgetEnabledCheckBox.ToolTip = availability.Message;
        TaskbarWidgetModeBox.IsEnabled = availability.CanEnable;
        TaskbarWidgetStatus.Text = availability.CanEnable
            ? (Application.Current as App)?.TaskbarWidgetRuntimeStatus ?? availability.Message
            : availability.Message;
    }

    private static string DescribeLocationProvider(ProviderLocation provider)
    {
        string place = string.Join(", ", new[] { provider.City, provider.Region, provider.CountryCode }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return $"{provider.ProviderName}: {(string.IsNullOrWhiteSpace(place) ? "lokasi tidak tersedia" : place)}";
    }

    private static string CombineLocationName(ProviderLocation first, ProviderLocation second)
    {
        string? city = string.IsNullOrWhiteSpace(first.City) ? second.City : first.City;
        string? region = string.IsNullOrWhiteSpace(first.Region) ? second.Region : first.Region;
        string? country = string.IsNullOrWhiteSpace(first.Country) ? second.Country : first.Country;
        string result = string.Join(", ", new[] { city, region, country }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(result) ? "Perkiraan lokasi jaringan" : result;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e) => TrySaveSettings();

    private bool TrySaveSettings()
    {
        if (_savingSettings) return false;
        _savingSettings = true;
        try
        {
            if (!TryReadSettings(out AppSettings? candidate, out string error))
            {
                SettingsStatusLabel.Text = error;
                return false;
            }
            try
            {
                _saveSettings(candidate!);
                CopySettings(candidate!, _settings);
                string startupStatus;
                try
                {
                    _setStartupEnabled(_settings.StartupEnabled, _settings.StartHidden);
                    bool actualStartup = _isStartupEnabled();
                    startupStatus = actualStartup == _settings.StartupEnabled
                        ? "Startup Windows terverifikasi."
                        : "Status startup Windows belum dapat diverifikasi.";
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
                {
                    _logSettingsError("settings.startup", ex);
                    startupStatus = "Pengaturan tersimpan, tetapi registrasi startup Windows gagal.";
                }
                _refreshDependentViews();
                PopulateSettingsForm();
                if (_calendarViewsLoaded)
                    RefreshCalendarViews();
                SettingsStatusLabel.Text = $"Pengaturan tersimpan. {startupStatus}";
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            {
                _logSettingsError("settings.save", ex);
                SettingsStatusLabel.Text = "Pengaturan tidak berhasil disimpan. Nilai aktif tidak diubah; periksa izin dan ruang penyimpanan.";
                return false;
            }
        }
        finally { _savingSettings = false; }
    }

    private bool TryReadSettings(out AppSettings? candidate, out string error)
    {
        candidate = null;
        error = "";
        var method = MethodBox.SelectedItem as CalculationMethod;
        if (method is null)
        {
            error = "Pilih standar perhitungan terlebih dahulu.";
            return false;
        }
        if (!TryDouble(LatitudeBox.Text, out double latitude) || latitude is < -90 or > 90)
        {
            error = "Latitude harus berupa angka antara −90 dan 90.";
            return false;
        }
        if (!TryDouble(LongitudeBox.Text, out double longitude) || longitude is < -180 or > 180)
        {
            error = "Longitude harus berupa angka antara −180 dan 180.";
            return false;
        }

        string timezone = TimeZoneBox.Text.Trim();
        try { _ = PrayerScheduleService.ResolveTimeZone(timezone); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            error = "Timezone tidak dikenal. Masukkan ID timezone yang valid, misalnya Asia/Jakarta.";
            return false;
        }
        if (!TryInt(ReminderMinutesBox.Text, 0, 120, out int reminderMinutes))
        {
            error = "Waktu pengingat harus berupa bilangan bulat 0–120 menit.";
            return false;
        }
        if (!TryInt(LockScreenDurationBox.Text, 1, 120, out int lockScreenDurationMinutes))
        {
            error = "Lama tampil gambar lock screen harus berupa bilangan bulat 1–120 menit.";
            return false;
        }
        if (!TryInt(DhuhaAfterSunriseBox.Text, 0, 180, out int dhuhaAfterSunrise))
        {
            error = "Jarak Duha setelah terbit harus berupa bilangan bulat 0–180 menit.";
            return false;
        }
        if (!CorrectionInputBehavior.TryParseDateCorrection(CalendarDateCorrectionBox.Text, out int calendarCorrection))
        {
            error = "Koreksi tanggal Masehi harus berada pada rentang −7 sampai +7 hari.";
            return false;
        }
        if (!CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out int hijriCorrection))
        {
            error = "Koreksi tanggal Hijriyah harus berada pada rentang −7 sampai +7 hari.";
            return false;
        }
        var corrections = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, TextBox box) in CorrectionBoxes())
        {
            if (!CorrectionInputBehavior.TryParseCorrection(box.Text, out int value))
            {
                error = $"Koreksi {key} harus berupa bilangan bulat −30 sampai +30 menit.";
                return false;
            }
            corrections[key] = value;
        }

        bool lockScreenOptIn = LockScreenCheckBox.IsChecked == true;
        if (lockScreenOptIn && !_inspectLockScreenSafety().CanChangeTemporarily &&
            !(_settings.LockScreenEnabled && _settings.LockScreenOptIn))
        {
            error = "Lock screen belum dapat disnapshot dan dipulihkan dengan aman. Pengaturan tidak diaktifkan.";
            return false;
        }

        candidate = CloneSettings(_settings);
        candidate.LockScreenEnabled = lockScreenOptIn;
        candidate.LockScreenOptIn = lockScreenOptIn;
        candidate.LockScreenDurationMinutes = lockScreenDurationMinutes;
        candidate.LocationName = string.IsNullOrWhiteSpace(LocationNameBox.Text) ? "Lokasi belum diatur" : LocationNameBox.Text.Trim();

        candidate.Latitude = latitude;
        candidate.Longitude = longitude;
        candidate.TimeZoneId = timezone;
        candidate.MethodId = method.Id;
        candidate.CalculationStandard = method.Name;
        candidate.Corrections = corrections;
        candidate.RemindersEnabled = ReminderEnabledCheckBox.IsChecked == true;
        candidate.AutoLockEnabled = AutoLockEnabledCheckBox.IsChecked == true;
        candidate.ReminderMinutes = reminderMinutes;
        candidate.ReminderMessage = string.IsNullOrWhiteSpace(ReminderMessageBox.Text)
            ? AppSettings.DefaultReminderMessage
            : ReminderMessageBox.Text.Trim();
        candidate.WidgetEnabled = WidgetEnabledCheckBox.IsChecked == true;
        candidate.TaskbarWidgetEnabled = TaskbarWidgetEnabledCheckBox.IsChecked == true && TaskbarWidgetSupport.Inspect().CanEnable;
        if ((TaskbarWidgetModeBox.SelectedItem as ComboBoxItem)?.Tag is not string taskbarMode ||
            taskbarMode != "popup")
        {
            error = "Pilih Popup independen.";
            return false;
        }
        candidate.TaskbarWidgetMode = taskbarMode;
        if (WidgetModeBox.SelectedItem is not ComboBoxItem widgetModeItem ||
            widgetModeItem.Tag is not string widgetMode || widgetMode is not ("essential" or "minimalist" or "detail"))
        {
            error = "Pilih mode Essential, Minimalist, atau Medium detail.";
            return false;
        }
        if (WidgetMotionBox.SelectedItem is not ComboBoxItem widgetMotionItem ||
            widgetMotionItem.Tag is not string widgetMotion || widgetMotion is not ("static" or "marquee"))
        {
            error = "Pilih gerak statis atau marquee untuk widget Essential.";
            return false;
        }
        if (WidgetPositionBox.SelectedItem is not ComboBoxItem widgetPositionItem ||
            widgetPositionItem.Tag is not string widgetPosition || widgetPosition is not ("tc" or "tl" or "tr" or "bl" or "br"))
        {
            error = "Pilih posisi tengah atas atau salah satu sudut untuk widget Medium detail.";
            return false;
        }
        if (ThemeBox.SelectedItem is not ComboBoxItem themeItem ||
            themeItem.Tag is not string themeId || !ThemeCatalog.All.Any(theme => string.Equals(theme.Id, themeId, StringComparison.Ordinal)))
        {
            error = "Pilih salah satu tema aplikasi yang tersedia.";
            return false;
        }
        candidate.WidgetMode = widgetMode;
        candidate.WidgetMotion = widgetMotion;
        candidate.WidgetPosition = widgetPosition;
        candidate.ThemeId = themeId;
        candidate.DhuhaAfterSunriseMinutes = dhuhaAfterSunrise;
        candidate.CalendarDateCorrectionDays = calendarCorrection;
        candidate.HijriDateCorrectionDays = hijriCorrection;
        candidate.StartupEnabled = StartupEnabledCheckBox.IsChecked == true;
        candidate.StartHidden = StartHiddenCheckBox.IsChecked == true;
        candidate.Normalize();
        return true;
    }

    private IEnumerable<(string Key, TextBox Box)> CorrectionBoxes()
    {
        yield return ("Fajr", CorrectionFajrBox);
        yield return ("Sunrise", CorrectionSunriseBox);
        yield return ("Dhuha", CorrectionDhuhaBox);
        yield return ("Dhuhr", CorrectionDhuhrBox);
        yield return ("Asr", CorrectionAsrBox);
        yield return ("Maghrib", CorrectionMaghribBox);
        yield return ("Isha", CorrectionIshaBox);
    }

    private void AttachCorrectionEditors()
    {
        CorrectionInputBehavior.Attach(CorrectionFajrBox, CorrectionFajrDecreaseButton, CorrectionFajrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionSunriseBox, CorrectionSunriseDecreaseButton, CorrectionSunriseIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionDhuhaBox, CorrectionDhuhaDecreaseButton, CorrectionDhuhaIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionDhuhrBox, CorrectionDhuhrDecreaseButton, CorrectionDhuhrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionAsrBox, CorrectionAsrDecreaseButton, CorrectionAsrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionMaghribBox, CorrectionMaghribDecreaseButton, CorrectionMaghribIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionIshaBox, CorrectionIshaDecreaseButton, CorrectionIshaIncreaseButton);
        CorrectionInputBehavior.AttachDateCorrection(CalendarDateCorrectionBox, CalendarDateCorrectionDecreaseButton, CalendarDateCorrectionIncreaseButton);
        CorrectionInputBehavior.AttachDateCorrection(HijriDateCorrectionBox, HijriDateCorrectionDecreaseButton, HijriDateCorrectionIncreaseButton);
    }

    private static bool TryDouble(string text, out double value) =>
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        || double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value);

    private static bool TryInt(string text, int minimum, int maximum, out int value) =>
        int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
        && value >= minimum && value <= maximum;

    private static AppSettings CloneSettings(AppSettings source)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(source);
        return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json)
            ?? throw new InvalidOperationException("Pengaturan tidak dapat disalin.");
    }

    private static void CopySettings(AppSettings source, AppSettings destination)
    {
        destination.Latitude = source.Latitude;
        destination.Longitude = source.Longitude;
        destination.TimeZoneId = source.TimeZoneId;
        destination.LocationName = source.LocationName;
        destination.OnlineGeocodingEndpoint = source.OnlineGeocodingEndpoint;
        destination.MethodId = source.MethodId;
        destination.CalculationStandard = source.CalculationStandard;
        destination.Corrections = new Dictionary<string, int>(source.Corrections, StringComparer.OrdinalIgnoreCase);
        destination.RemindersEnabled = source.RemindersEnabled;
        destination.LockScreenEnabled = source.LockScreenEnabled;
        destination.LockScreenOptIn = source.LockScreenOptIn;
        destination.LockScreenDurationMinutes = source.LockScreenDurationMinutes;
        destination.AutoLockEnabled = source.AutoLockEnabled;
        destination.StartupEnabled = source.StartupEnabled;
        destination.StartHidden = source.StartHidden;
        destination.WidgetEnabled = source.WidgetEnabled;
        destination.TaskbarWidgetEnabled = source.TaskbarWidgetEnabled;
        destination.TaskbarWidgetMode = source.TaskbarWidgetMode;
        destination.WidgetMode = source.WidgetMode;
        destination.WidgetMotion = source.WidgetMotion;
        destination.WidgetPosition = source.WidgetPosition;
        destination.ThemeId = source.ThemeId;
        destination.ReminderMinutes = source.ReminderMinutes;
        destination.ReminderMessage = source.ReminderMessage;
        destination.DhuhaAfterSunriseMinutes = source.DhuhaAfterSunriseMinutes;
        destination.CalendarDateCorrectionDays = source.CalendarDateCorrectionDays;
        destination.HijriDateCorrectionDays = source.HijriDateCorrectionDays;
        destination.OtherSettings = source.OtherSettings is null ? null : new(source.OtherSettings);
    }

    private void CancelSettings_Click(object sender, RoutedEventArgs e)
    {
        if (HasUnsavedSettingsChanges())
        {
            if (!ConfirmUnsavedSettingsChanges(discardOnly: true)) return;
        }
        else
            RestoreSavedSettingsPreview();
        SettingsStatusLabel.Text = "Perubahan form dibatalkan; pengaturan tersimpan tidak berubah.";
    }

    internal void RestoreSavedSettingsPreview()
    {
        PopulateSettingsForm();
        RestoreSavedThemePreview();
        _restoreWidgetAppearancePreview();
        _restoreTaskbarAppearancePreview();
    }

    internal void RestoreSavedThemePreview()
    {
        bool wasLoading = _loadingSettings;
        _loadingSettings = true;
        try
        {
            ThemeBox.SelectedItem = ThemeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.ThemeId, StringComparison.OrdinalIgnoreCase));
        }
        finally { _loadingSettings = wasLoading; }

        _applyThemePreview(_settings.ThemeId);
    }

    // Compare raw form values, not validated settings: invalid text is still a draft.
    // Calendar selectors, suggestions and runtime/status labels are intentionally absent.
    private object?[] CaptureSettingsFormDraft() =>
    [
        LocationNameBox.Text, LatitudeBox.Text, LongitudeBox.Text, TimeZoneBox.Text,
        MethodBox.SelectedValue,
        CorrectionFajrBox.Text, CorrectionSunriseBox.Text, CorrectionDhuhaBox.Text,
        CorrectionDhuhrBox.Text, CorrectionAsrBox.Text, CorrectionMaghribBox.Text, CorrectionIshaBox.Text,
        DhuhaAfterSunriseBox.Text, CalendarDateCorrectionBox.Text, HijriDateCorrectionBox.Text,
        ReminderEnabledCheckBox.IsChecked, ReminderMinutesBox.Text, ReminderMessageBox.Text,
        WidgetEnabledCheckBox.IsChecked, (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag,
        (WidgetMotionBox.SelectedItem as ComboBoxItem)?.Tag, (WidgetPositionBox.SelectedItem as ComboBoxItem)?.Tag,
        TaskbarWidgetEnabledCheckBox.IsChecked, (TaskbarWidgetModeBox.SelectedItem as ComboBoxItem)?.Tag,
        (ThemeBox.SelectedItem as ComboBoxItem)?.Tag,
        StartupEnabledCheckBox.IsChecked, StartHiddenCheckBox.IsChecked, AutoLockEnabledCheckBox.IsChecked,
        LockScreenCheckBox.IsChecked, LockScreenDurationBox.Text
    ];

    internal bool HasUnsavedSettingsChanges() => _savedFormBaseline is not null &&
        !_savedFormBaseline.SequenceEqual(CaptureSettingsFormDraft());

    internal bool ConfirmUnsavedSettingsChanges(bool discardOnly = false)
    {
        if (_resolvingUnsavedChanges || _savingSettings) return false;
        if (!HasUnsavedSettingsChanges()) return true;
        _resolvingUnsavedChanges = true;
        try
        {
            if (WindowState == WindowState.Minimized) WindowState = _lastNonMinimizedWindowState;
            UnsavedChangesChoice choice = _promptUnsavedChanges(discardOnly);
            if (choice == UnsavedChangesChoice.Discard)
            {
                RestoreSavedSettingsPreview();
                return true;
            }
            if (!discardOnly && choice == UnsavedChangesChoice.Save && TrySaveSettings()) return true;
            MainTabs.SelectedIndex = 2;
            return false;
        }
        finally { _resolvingUnsavedChanges = false; }
    }

    private UnsavedChangesChoice ShowUnsavedChangesDialog(bool discardOnly)
    {
        if (!IsVisible) Show();
        Activate();
        var dialog = new UnsavedChangesDialog(discardOnly) { Owner = this };
        dialog.ShowDialog();
        return dialog.Choice;
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => ((App)Application.Current).ShutdownApplication();

    private void LoadCalendar_Click(object sender, RoutedEventArgs e)
    {
        if (CalendarYearBox.SelectedItem is not int year) return;
        _selectedGregorianYear = year;
        if (_selectedGregorianDate.Year != year)
            _selectedGregorianDate = year == GetCalendarToday().Year ? GetCalendarToday() : new DateTime(year, 1, 1);
        RenderGregorianYear(year);
    }

    private void PreviousGregorianYear_Click(object sender, RoutedEventArgs e) => MoveYear(CalendarYearBox, -1, hijri: false);
    private void NextGregorianYear_Click(object sender, RoutedEventArgs e) => MoveYear(CalendarYearBox, 1, hijri: false);
    private void PreviousHijriYear_Click(object sender, RoutedEventArgs e) => MoveYear(HijriYearBox, -1, hijri: true);
    private void NextHijriYear_Click(object sender, RoutedEventArgs e) => MoveYear(HijriYearBox, 1, hijri: true);

    private void MoveYear(ComboBox selector, int delta, bool hijri)
    {
        if (selector.SelectedIndex < 0) return;
        int nextIndex = selector.SelectedIndex + delta;
        if (nextIndex < 0 || nextIndex >= selector.Items.Count) return;
        selector.SelectedIndex = nextIndex;
        if (hijri) LoadHijriCalendar_Click(this, new RoutedEventArgs());
        else LoadCalendar_Click(this, new RoutedEventArgs());
    }

    private void LoadHijriCalendar_Click(object sender, RoutedEventArgs e)
    {
        if (HijriYearBox.SelectedItem is not int year) return;
        _selectedHijriYear = year;
        RenderHijriYear(year);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _locationRequestVersion++;
        CloseLocationSuggestions();
        if (_allowClose)
        {
            bool wasLoading = _loadingSettings;
            _loadingSettings = true;
            try
            {
                LocationNameBox.Text = _settings.LocationName;
                LatitudeBox.Text = _settings.Latitude.ToString("G", CultureInfo.InvariantCulture);
                LongitudeBox.Text = _settings.Longitude.ToString("G", CultureInfo.InvariantCulture);
                TimeZoneBox.Text = _settings.TimeZoneId;
            }
            finally { _loadingSettings = wasLoading; }
            return;
        }
        e.Cancel = true;
        if (!ConfirmUnsavedSettingsChanges()) return;
        RestoreSavedSettingsPreview();
        Hide();
        FooterStatus.Text = "Aplikasi tetap aktif di area notifikasi sistem.";
    }

    private static bool IsNativeCoreFailure(Exception ex) =>
        ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException;

    private static bool IsScheduleFailure(Exception ex) =>
        IsNativeCoreFailure(ex) || ex is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException;

    private static string FormatTime(TimeSpan time) => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

    private static SolidColorBrush MakeBrush(string color) =>
        (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;

    private sealed record CalculationMethod(int Id, string Name, string TimeZoneId, double FajrAngle, double IshaAngle);
}
