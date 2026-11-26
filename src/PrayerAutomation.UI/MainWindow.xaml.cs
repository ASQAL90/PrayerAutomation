using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using Rectangle = System.Windows.Shapes.Rectangle;
using TextBox = System.Windows.Controls.TextBox;
using UniformGrid = System.Windows.Controls.Primitives.UniformGrid;
using Key = System.Windows.Input.Key;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyboardFocusChangedEventArgs = System.Windows.Input.KeyboardFocusChangedEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace PrayerAutomation.UI;

/// <summary>Main application window: overview, calendar, settings, and schedule preview UI.</summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");
    private static readonly string[] WeekdayShortNames = ["Sen", "Sel", "Rab", "Kam", "Jum", "Sab", "Min"];
    private readonly AppSettings _settings;
    // Captured on the UI thread; status setters are also called from restore retries.
    private readonly Dispatcher _uiDispatcher;
    private readonly Action<AppSettings> _saveSettings;
    private readonly Action<string, Exception> _logSettingsError;
    private readonly Action<bool, bool> _setStartupEnabled;
    private readonly Func<bool> _isStartupEnabled;
    private readonly Func<LockScreenSafetyInfo> _inspectLockScreenSafety;
    private readonly Action<string> _applyThemePreview;
    private readonly Action _refreshDependentViews;
    private readonly Action _restoreWidgetAppearancePreview;
    private readonly Action<bool, string, string, string, bool, bool, bool> _applyWidgetAppearancePreview;
    private readonly Action<bool, string> _applyTaskbarAppearancePreview;
    private readonly Action _restoreTaskbarAppearancePreview;
    private readonly LocationDetectionService _locationDetectionService = new();
    private readonly DeviceLocationService _deviceLocationService = new();
    private int _locationRequestVersion;
    private int _locationSearchVersion;
    /// <summary>True when the location name still holds an automatically generated value.</summary>
    private bool _locationNameAutoFilled;
    /// <summary>True while the location name box is being filled by the application.</summary>
    private bool _settingLocationName;
    private readonly DispatcherTimer _locationSearchTimer = new(DispatcherPriority.Background)
    {
        Interval = TimeSpan.FromMilliseconds(180)
    };


    private readonly DispatcherTimer _clockTimer;
    private readonly Dictionary<DateTime, Button> _gregorianDateButtons = [];
    private readonly Dictionary<DateTime, CalendarObservance[]> _gregorianObservances = [];
    private readonly Dictionary<DateTime, Button> _hijriDateButtons = [];
    private readonly Dictionary<DateTime, CalendarObservance[]> _hijriObservances = [];
    private bool _loadingSettings;
    private bool _allowClose;
    private object?[]? _savedFormBaseline;
    // Position of DownloadFullDataCheckBox.IsChecked in CaptureSettingsFormDraft().
    private const int FullOfflineDataDraftIndex = 16;
    /// <summary>True once the user edits the height field by hand.</summary>
    private bool _elevationManuallyEdited;
    /// <summary>True while the height field is being filled from the offline dataset.</summary>
    private bool _settingAutoElevation;
    private bool _resolvingUnsavedChanges;
    private bool _savingSettings;
    private WindowState _lastNonMinimizedWindowState = WindowState.Normal;
    private Func<bool, UnsavedChangesChoice> _promptUnsavedChanges;
    private int _selectedGregorianYear;
    private int _selectedHijriYear;
    private DateTime _selectedGregorianDate;
    private DateTime _selectedHijriDate;
    private bool _hasHijriSelection;
    private DateTime _lastCalendarToday;
    // A full year in each calendar creates hundreds of WPF controls.  Keep that
    // work out of the startup path so the initially visible controls accept input
    // as soon as the window is shown.
    private bool _calendarViewsLoaded;
    // First-open timing is measured once per process with the same data on both
    // calendars; it never changes layout and never enters the startup path.
    private bool _calendarTimingPending = true;
    // The year selectors are filled and moved by code as well; this keeps the
    // SelectionChanged handlers from rendering the same year twice.
    private bool _syncingYearSelectors;
    private readonly OfflineDataDownloadService _offlineDataDownloadService = new(new HttpClient());
    private CancellationTokenSource? _offlineDataDownloadCancellation;

    /// <summary>Initializes the window with the default settings store and startup registration.</summary>
    /// <param name="settings">Application settings.</param>
    internal MainWindow(AppSettings settings)
        : this(settings, SettingsStore.Save, StartupRegistration.SetEnabled, StartupRegistration.IsEnabled)
    {
    }

    /// <summary>Initializes the window with injectable save/startup delegates.</summary>
    /// <param name="settings">Application settings.</param>
    /// <param name="saveSettings">Persists settings.</param>
    /// <param name="setStartupEnabled">Sets the startup registration.</param>
    /// <param name="isStartupEnabled">Reads the startup registration.</param>
    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled)
        : this(settings, saveSettings, setStartupEnabled, isStartupEnabled, null) { }

    /// <summary>Initializes the window, optionally overriding the lock-screen safety probe.</summary>
    /// <param name="settings">Application settings.</param>
    /// <param name="saveSettings">Persists settings.</param>
    /// <param name="setStartupEnabled">Sets the startup registration.</param>
    /// <param name="isStartupEnabled">Reads the startup registration.</param>
    /// <param name="inspectLockScreenSafety">Optional lock-screen safety probe override.</param>
    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled,
        Func<LockScreenSafetyInfo>? inspectLockScreenSafety)
        : this(settings, saveSettings, setStartupEnabled, isStartupEnabled, inspectLockScreenSafety, null) { }

    /// <summary>Initializes the window, optionally overriding the lock-screen probe and settings-error logger.</summary>
    /// <param name="settings">Application settings.</param>
    /// <param name="saveSettings">Persists settings.</param>
    /// <param name="setStartupEnabled">Sets the startup registration.</param>
    /// <param name="isStartupEnabled">Reads the startup registration.</param>
    /// <param name="inspectLockScreenSafety">Optional lock-screen safety probe override.</param>
    /// <param name="logSettingsError">Optional logger for settings errors.</param>
    internal MainWindow(AppSettings settings, Action<AppSettings> saveSettings,
        Action<bool, bool> setStartupEnabled, Func<bool> isStartupEnabled,
        Func<LockScreenSafetyInfo>? inspectLockScreenSafety, Action<string, Exception>? logSettingsError)
    {
        _settings = settings;
        _uiDispatcher = Dispatcher;
        _logSettingsError = logSettingsError ?? AppLogger.Write;
        _promptUnsavedChanges = ShowUnsavedChangesDialog;
        _saveSettings = saveSettings ?? throw new ArgumentNullException(nameof(saveSettings));
        _setStartupEnabled = setStartupEnabled ?? throw new ArgumentNullException(nameof(setStartupEnabled));
        _isStartupEnabled = isStartupEnabled ?? throw new ArgumentNullException(nameof(isStartupEnabled));
        _inspectLockScreenSafety = inspectLockScreenSafety ?? LockScreenSafetyProbe.Inspect;
        _applyThemePreview = id => ((App)Application.Current).ApplyThemePreview(id);
        _refreshDependentViews = () => ((App)Application.Current).RefreshDependentViews();
        _restoreWidgetAppearancePreview = () => ((App)Application.Current).RestoreWidgetAppearancePreview();
        _applyWidgetAppearancePreview = (enabled, mode, motion, position, alwaysOnTop, clickThrough, autoHideOnHover) =>
            ((App)Application.Current).ApplyWidgetAppearancePreview(enabled, mode, motion, position, alwaysOnTop,
                clickThrough, autoHideOnHover);
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
        RefreshStorageSummary();
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
            if (!TrySaveSettings() && !ConfirmUnsavedSettingsChanges()) return;
            RestoreSavedSettingsPreview();
            Hide();
        };

        InitializeReminderTimePickers();
        InitializeYearSelectors();
        PopulateSettingsForm();
        TryRestoreOfflineDataCache();
        LoadDzikirDoaContent();
        RestoreLastTabs();
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

    /// <summary>Allows the window to close instead of hiding to the tray.</summary>
    internal void AllowClose() => _allowClose = true;

    /// <summary>Selects the calendar tab and loads the calendar views.</summary>
    internal void SelectCalendarTab()
    {
        MainTabs.SelectedIndex = 1;
        EnsureCalendarViewsLoaded();
        Activate();
    }

    /// <summary>Selects the settings tab, which is the last tab.</summary>
    internal void SelectSettingsTab()
    {
        MainTabs.SelectedIndex = 3;
        Activate();
    }

    /// <summary>Selects the Dzikir &amp; Doa tab without forcing a sub-tab.</summary>
    internal void SelectDzikirTab()
    {
        MainTabs.SelectedIndex = 2;
        Activate();
    }

    /// <summary>Selects the Dzikir &amp; Doa tab and the requested sub-tab.</summary>
    /// <param name="target"><c>morning</c>, <c>evening</c>, or <c>dzikir</c> to keep the current sub-tab.</param>
    internal void SelectDzikirTab(string target)
    {
        SelectDzikirTab();
        if (string.Equals(target, "morning", StringComparison.OrdinalIgnoreCase))
            DzikirDoaTabs.SelectedIndex = 0;
        else if (string.Equals(target, "evening", StringComparison.OrdinalIgnoreCase))
            DzikirDoaTabs.SelectedIndex = 1;
    }

    /// <summary>Restores the tab selected before the app was minimized or closed.</summary>
    private void RestoreLastTabs()
    {
        MainTabs.SelectedIndex = Math.Clamp(_settings.LastMainTabIndex, 0, MainTabs.Items.Count - 1);
        DzikirDoaTabs.SelectedIndex = Math.Clamp(_settings.LastDzikirTabIndex, 0, DzikirDoaTabs.Items.Count - 1);
        if (MainTabs.SelectedIndex == 1) EnsureCalendarViewsLoaded();
    }

    /// <summary>Refreshes overview, calendar, and schedule preview after a theme change.</summary>
    internal void RefreshThemeDependentViews()
    {
        if (_calendarViewsLoaded)
            RefreshCalendarViews();
        RefreshOverview();
        RefreshSettingsSchedulePreview();
    }

    /// <summary>Loads the calendar views the first time the calendar tab is selected.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != MainTabs) return;
        _settings.LastMainTabIndex = MainTabs.SelectedIndex;
        if (MainTabs.SelectedIndex == 1) EnsureCalendarViewsLoaded();
    }

    /// <summary>Remembers the selected dzikir sub-tab for the next launch.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void DzikirDoaTabs_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        _settings.LastDzikirTabIndex = DzikirDoaTabs.SelectedIndex;

    /// <summary>Refreshes the overview tab: date, location summary, reminder status, and today's schedule.</summary>
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
        OverviewCalculationNote.Text = _settings.MethodId == 99
            ? $"Jadwal {scheduleDate.ToString("d MMMM yyyy", IndonesianCulture)} dihitung tanpa internet (metode Muhammadiyah 18°). Ihtiyat 1–2 menit dan ketinggian {_settings.ElevationMeters} mdpl sudah diterapkan; koreksi menit tetap dapat diatur."
            : $"Jadwal {scheduleDate.ToString("d MMMM yyyy", IndonesianCulture)} dihitung tanpa internet (metode {_settings.MethodId}). Sudut dan sumber mengikuti metode yang dipilih. Koreksi menit dapat diatur.";
        OverviewReminderStatus.Text = _settings.RemindersEnabled
            ? $"Pengingat teks: aktif — {_settings.ReminderMinutes} menit sebelumnya (tanpa suara)."
            : "Pengingat teks: nonaktif.";
        OverviewCalendarStatus.Text = "Kalender Masehi dan Hijriyah dapat dikoreksi tanpa mengubah jadwal sholat. " + CurrentCalendarSourceNote();

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
                "Jadwal belum bisa dihitung. Periksa lokasi, zona waktu, dan metode perhitungan.");
            Grid.SetColumnSpan(message, 1);
            ScheduleOverviewGrid.Children.Add(message);
            FooterStatus.Text = "Jadwal belum tersedia; rincian teknis disimpan di log aplikasi.";
        }
    }

    /// <summary>Creates the error text shown when the schedule cannot be calculated.</summary>
    /// <param name="text">The message to display.</param>
    /// <returns>The configured TextBlock.</returns>
    private TextBlock CreateScheduleErrorText(string text) => new()
    {
        Text = text,
        Style = (Style)FindResource("FormHelpText"),
        Foreground = MakeBrush(ThemeManager.Current.ErrorText)
    };

    /// <summary>Clears a schedule grid and recreates its column definitions.</summary>
    /// <param name="target">The grid to reset.</param>
    /// <param name="columns">Number of star-width columns to create.</param>
    private static void ResetScheduleGrid(Grid target, int columns)
    {
        target.Children.Clear();
        target.RowDefinitions.Clear();
        target.ColumnDefinitions.Clear();
        for (int column = 0; column < columns; column++)
            target.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    }

    /// <summary>Renders prayer schedule cards into a grid.</summary>
    /// <param name="target">The grid to fill.</param>
    /// <param name="entries">Prayer entries to render.</param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <param name="footerFactory">Optional factory for per-entry footer content.</param>
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

    /// <summary>Creates a card UI element for one prayer entry.</summary>
    /// <param name="entry">The prayer entry.</param>
    /// <param name="footer">Optional footer element.</param>
    /// <returns>The card element.</returns>
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

    /// <summary>Moves the Hijri correction editor next to its date cell.</summary>
    private void MoveCalendarCorrectionEditorsBesideDates()
    {
        MoveToHost(HijriDateCorrectionBox.Parent as FrameworkElement, HijriDateCorrectionHost);
    }

    /// <summary>Reparents an editor element into another panel.</summary>
    /// <param name="editor">The element to move; no-op when null or parentless.</param>
    /// <param name="host">The destination panel.</param>
    private static void MoveToHost(FrameworkElement? editor, System.Windows.Controls.Panel host)
    {
        if (editor is null || editor.Parent is not System.Windows.Controls.Panel parent) return;
        parent.Children.Remove(editor);
        host.Children.Add(editor);
    }

    /// <summary>Returns the repositioned correction editor for a prayer entry, or null for unsupported keys.</summary>
    /// <param name="entry">The prayer entry.</param>
    /// <returns>The editor element.</returns>
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

    /// <summary>Creates the prayer icon image for a key/presentation pair.</summary>
    /// <param name="key">Prayer key.</param>
    /// <param name="presentation">Icon presentation (brush, kind, description).</param>
    /// <returns>The icon image.</returns>
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

    /// <summary>Initializes the Gregorian and Hijri year selector ranges around today.</summary>
    private void InitializeYearSelectors()
    {
        DateTime today = GetCalendarToday();
        _selectedGregorianYear = today.Year;
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
        _selectedHijriYear = Math.Clamp(currentHijriYear, 1, 9666);
        RebuildYearSelectors();
    }

    /// <summary>
    /// Rebuilds both year selectors from the ranges the active source can render,
    /// clamps the current selections into those ranges, and updates the arrows.
    /// </summary>
    private void RebuildYearSelectors()
    {
        DateTime today = GetCalendarToday();
        int todayHijriYear = _selectedHijriYear;
        try { todayHijriYear = HijriForCalendarDate(today).Year; }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.selector-hijri-year", ex);
        }

        CalendarYearRange gregorian = CalendarYearBounds.Gregorian(UseKhgtCalendar, today.Year);
        CalendarYearRange hijri = CalendarYearBounds.Hijri(UseKhgtCalendar, todayHijriYear);

        _syncingYearSelectors = true;
        try
        {
            _selectedGregorianYear = gregorian.Clamp(_selectedGregorianYear);
            if (_selectedGregorianDate.Year != _selectedGregorianYear)
                _selectedGregorianDate = _selectedGregorianYear == today.Year ? today : new DateTime(_selectedGregorianYear, 1, 1);
            CalendarYearBox.ItemsSource = Enumerable
                .Range(gregorian.First, gregorian.Last - gregorian.First + 1).ToArray();
            CalendarYearBox.SelectedItem = _selectedGregorianYear;

            _selectedHijriYear = hijri.Clamp(_selectedHijriYear);
            HijriYearBox.ItemsSource = Enumerable
                .Range(hijri.First, hijri.Last - hijri.First + 1).ToArray();
            HijriYearBox.SelectedItem = _selectedHijriYear;
        }
        finally { _syncingYearSelectors = false; }
        UpdateYearNavigationButtons();
    }

    /// <summary>Disables the year arrows that would move past the selectable range.</summary>
    private void UpdateYearNavigationButtons()
    {
        if (PreviousCalendarYearButton is null || NextCalendarYearButton is null ||
            PreviousHijriYearButton is null || NextHijriYearButton is null)
            return;
        PreviousCalendarYearButton.IsEnabled = CalendarYearBox.SelectedIndex > 0;
        NextCalendarYearButton.IsEnabled = CalendarYearBox.SelectedIndex >= 0 &&
            CalendarYearBox.SelectedIndex < CalendarYearBox.Items.Count - 1;
        PreviousHijriYearButton.IsEnabled = HijriYearBox.SelectedIndex > 0;
        NextHijriYearButton.IsEnabled = HijriYearBox.SelectedIndex >= 0 &&
            HijriYearBox.SelectedIndex < HijriYearBox.Items.Count - 1;
    }

    /// <summary>Resets the calendar view state to today and re-renders both calendars.</summary>
    private void RefreshCalendarViews()
    {
        bool measureFirstOpen = _calendarTimingPending;
        _calendarTimingPending = false;
        Stopwatch? total = measureFirstOpen ? Stopwatch.StartNew() : null;
        Stopwatch? phase = measureFirstOpen ? new Stopwatch() : null;
        DateTime correctedToday = GetCalendarToday();
        _lastCalendarToday = correctedToday;
        _selectedGregorianYear = correctedToday.Year;
        _selectedGregorianDate = correctedToday;
        try
        {
            _selectedHijriYear = Math.Clamp(HijriForCalendarDate(correctedToday).Year, 1, 9666);
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.refresh-hijri-year", ex);
        }
        RebuildYearSelectors();
        string sourceNote = CurrentCalendarSourceNote();
        GregorianCalendarNote.Text = sourceNote;
        HijriCalendarNote.Text = sourceNote;
        phase?.Restart();
        RenderGregorianYear(_selectedGregorianYear);
        long gregorianMs = phase?.ElapsedMilliseconds ?? 0;
        phase?.Restart();
        RenderHijriYear(_selectedHijriYear);
        long hijriMs = phase?.ElapsedMilliseconds ?? 0;
        if (measureFirstOpen)
        {
            AppLogger.WriteInfo("calendar.first-open",
                $"gregorianMs={gregorianMs} hijriMs={hijriMs} totalMs={total!.ElapsedMilliseconds} " +
                $"source={_settings.CalendarSource}");
        }
    }

    /// <summary>Renders the calendar views on first demand.</summary>
    private void EnsureCalendarViewsLoaded()
    {
        if (_calendarViewsLoaded) return;
        _calendarViewsLoaded = true;
        RefreshCalendarViews();
    }

    /// <summary>Renders a full Gregorian year grid with date buttons and observance markers.</summary>
    /// <param name="year">The year to render.</param>
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
                    (string headerForeground, string headerBackground) = weekday is 5 or 6
                        ? ThemeCatalog.WeekendHeaderColors(ThemeManager.Current)
                        : (ThemeManager.Current.MutedText, ThemeManager.Current.SurfaceAlt);
                    var header = new TextBlock
                    {
                        Text = WeekdayShortNames[weekday],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = MakeBrush(headerForeground),
                        Background = MakeBrush(headerBackground),
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
                        daysGrid.Children.Add(new Border { Margin = new Thickness(1), MinHeight = 32 });
                        continue;
                    }
                    DateTime date = new(year, month, day);
                    CalendarObservance[] observances = NativePrayerCore.CalendarObservances(date, GetHijriDateCorrectionDays(), UseKhgtCalendar).ToArray();
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
            FooterStatus.Text = $"Kalender Masehi {year}: 12 bulan dimuat. Penanda membedakan hari besar, puasa sunnah, dan hari tidak berpuasa. " + CurrentCalendarSourceNote();
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            AppLogger.Write("calendar.gregorian.render", ex);
            CalendarGrid.Children.Clear();
            _gregorianDateButtons.Clear();
            _gregorianObservances.Clear();
            CalendarGrid.Children.Add(new TextBlock
            {
                Text = "Kalender belum bisa dimuat karena mesin perhitungan tidak tersedia atau tanggal tidak valid.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = MakeBrush(ThemeManager.Current.ErrorText),
                Margin = new Thickness(10)
            });
            SelectedDateDetail.Text = "Mesin perhitungan rusak. Instal ulang Ayo Jama’ah! atau periksa log aplikasi.";
            FooterStatus.Text = "Kalender Masehi gagal dimuat; rincian teknis disimpan di log aplikasi.";
        }
    }

    /// <summary>Creates a button for one Gregorian date cell.</summary>
    /// <param name="date">The cell date.</param>
    /// <param name="today">Today's date for marker styling.</param>
    /// <param name="observances">Calendar observances for the date.</param>
    /// <param name="isSelected">Whether the date is the current selection.</param>
    /// <returns>The date button.</returns>
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
            Foreground = MakeBrush(themeColors.Foreground),
            Margin = new Thickness(0, 0, 0, 6)
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
            var marker = new Rectangle
            {
                Width = 11,
                Height = 3,
                RadiusX = 1.5,
                RadiusY = 1.5,
                Fill = MakeBrush(themeColors.Marker),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 4)
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
            MinHeight = 32,
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

    /// <summary>Renders a full Hijri year grid with date buttons and observance markers.</summary>
    /// <param name="year">The Hijri year to render.</param>
    private void RenderHijriYear(int year)
    {
        HijriCalendarGrid.Children.Clear();
        _hijriDateButtons.Clear();
        _hijriObservances.Clear();
        try
        {
            DateTime calendarToday = GetCalendarToday();
            int totalDays = 0;
            for (int month = 1; month <= 12; month++)
            {
                NativeHijriMonth nativeMonth = NativePrayerCore.HijriMonth(year, month, UseKhgtCalendar);
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
                    (string headerForeground, string headerBackground) = weekday is 5 or 6
                        ? ThemeCatalog.WeekendHeaderColors(ThemeManager.Current)
                        : (ThemeManager.Current.MutedText, ThemeManager.Current.SurfaceAlt);
                    var header = new TextBlock
                    {
                        Text = WeekdayShortNames[weekday],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = MakeBrush(headerForeground),
                        Background = MakeBrush(headerBackground),
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
                        datesGrid.Children.Add(new Border { Margin = new Thickness(1), MinHeight = 32 });
                        continue;
                    }
                    DateTime gregorian = displayStart.AddDays(day - 1);
                    bool isSelected = _hasHijriSelection && gregorian.Date == _selectedHijriDate.Date;
                    CalendarObservance[] observances = NativePrayerCore.CalendarObservances(
                        gregorian, GetHijriDateCorrectionDays(), UseKhgtCalendar).ToArray();
                    _hijriObservances[gregorian.Date] = observances;
                    Button cell = CreateHijriDateButton(year, month, day, gregorian, calendarToday, isSelected, observances);
                    _hijriDateButtons[gregorian.Date] = cell;
                    datesGrid.Children.Add(cell);
                }
                monthPanel.Children.Add(datesGrid);
                HijriCalendarGrid.Children.Add(monthCard);
            }
            RestoreHijriSelection(year, calendarToday);
            HijriYearLabel.Text = $"Tahun {year} H — 12 bulan dimuat ({totalDays} hari). " + CurrentCalendarSourceNote();
            FooterStatus.Text = $"Kalender Hijriyah {year} H: 12 bulan dan {totalDays} tanggal dimuat.";
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            AppLogger.Write("calendar.hijri.render", ex);
            HijriCalendarGrid.Children.Clear();
            _hijriDateButtons.Clear();
            _hijriObservances.Clear();
            if (UseKhgtCalendar && year is < NativePrayerCore.KhgtFirstYear or > NativePrayerCore.KhgtLastYear)
            {
                HijriYearLabel.Text = $"Tahun {year} H belum tersedia di tabel KHGT resmi ({NativePrayerCore.KhgtFirstYear}–{NativePrayerCore.KhgtLastYear} H).";
                HijriSelectedDateDetail.Text = "Pilih tahun lain, atau ganti sumber kalender ke Sipil/tabular di Pengaturan.";
                FooterStatus.Text = "Tabel KHGT tidak mencakup tahun ini; tidak ada tanggal yang ditampilkan.";
            }
            else
            {
                HijriYearLabel.Text = $"Tahun {year} H dipilih, tetapi data bulan tidak dapat dimuat.";
                HijriSelectedDateDetail.Text = "Mesin perhitungan rusak. Instal ulang Ayo Jama’ah! atau periksa log aplikasi.";
                FooterStatus.Text = "Kalender Hijriyah gagal dimuat; rincian teknis disimpan di log aplikasi.";
            }
        }
    }

    /// <summary>Creates a button for one Hijri date cell.</summary>
    /// <param name="year">Hijri year.</param>
    /// <param name="month">Hijri month (1-12).</param>
    /// <param name="day">Hijri day.</param>
    /// <param name="gregorianDate">Gregorian date backing the cell.</param>
    /// <param name="calendarToday">Today's date for marker styling.</param>
    /// <param name="isSelected">Whether the cell is the current selection.</param>
    /// <param name="observances">Precomputed observances for the backing Gregorian date.</param>
    /// <returns>The date button.</returns>
    private Button CreateHijriDateButton(int year, int month, int day, DateTime gregorianDate, DateTime calendarToday,
        bool isSelected, IReadOnlyList<CalendarObservance> observances)
    {
        CalendarMarker markerStyle = PrayerUiLogic.CalendarMarkerFor(gregorianDate, calendarToday, isSelected, observances);
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
            Foreground = MakeBrush(themeColors.Foreground),
            Margin = new Thickness(0, 0, 0, 6)
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
            var marker = new Rectangle
            {
                Width = 11,
                Height = 3,
                RadiusX = 1.5,
                RadiusY = 1.5,
                Fill = MakeBrush(themeColors.Marker),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 4)
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
            MinHeight = 32,
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

    /// <summary>Selects a Gregorian date and updates button states and the detail caption.</summary>
    /// <param name="date">The date to select.</param>
    private void SelectGregorianDate(DateTime date)
    {
        _selectedGregorianDate = date.Date;
        DateTime today = GetCalendarToday();
        foreach ((DateTime cellDate, Button button) in _gregorianDateButtons)
            UpdateGregorianDateButton(cellDate, button, today);
        UpdateGregorianSelection(_selectedGregorianDate);
    }

    /// <summary>Applies marker styling and selection state to a date button.</summary>
    /// <param name="date">The cell date.</param>
    /// <param name="button">The button to update.</param>
    /// <param name="today">Today's date for marker styling.</param>
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
            foreach (Shape shape in content.Children.OfType<Shape>())
            {
                string automationId = AutomationProperties.GetAutomationId(shape);
                if (automationId.StartsWith("TodayRing-", StringComparison.Ordinal))
                {
                    shape.Stroke = MakeBrush(themeColors.Border);
                    shape.StrokeThickness = themeColors.BorderThickness;
                    shape.Visibility = markerStyle.IsToday ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (automationId.StartsWith("CalendarMarker-", StringComparison.Ordinal))
                {
                    shape.Fill = MakeBrush(themeColors.Marker);
                }
            }
        }
    }

    /// <summary>Updates the selected-date caption with observances and the corrected Hijri date.</summary>
    /// <param name="date">The selected date.</param>
    private void UpdateGregorianSelection(DateTime date)
    {
        try
        {
            string weekday = date.ToString("dddd", IndonesianCulture);
            string dateCaption = $"{weekday}, {date.ToString("d MMMM yyyy", IndonesianCulture)}";
            CalendarObservance[] observances = NativePrayerCore.CalendarObservances(date, GetHijriDateCorrectionDays(), UseKhgtCalendar).ToArray();
            string fullObservanceLine = observances.Length == 0 ? "" : "\nPenanda: " + string.Join("; ", observances.Select(item => item.Name));
            string shortObservanceLine = observances.Length == 0 ? "" : "\nPenanda: " + string.Join("; ", observances.Select(item => item.ShortName));
            NativeCalendarDate hijri = HijriForCalendarDate(date);
            string hijriLine = $"\nHijriyah: {hijri.Day} {NativePrayerCore.HijriMonthName(hijri.Month)} {hijri.Year} H";
            SelectedDateDetail.Text = dateCaption + shortObservanceLine + hijriLine;
            AutomationProperties.SetName(SelectedDateDetail, dateCaption + fullObservanceLine + hijriLine + "\n" + CurrentCalendarSourceNote());
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException)
        {
            AppLogger.Write("calendar.gregorian.selection", ex);
            SelectedDateDetail.Text = UseKhgtCalendar
                ? $"{date.ToString("dddd, d MMMM yyyy", IndonesianCulture)}\nTanggal ini di luar cakupan tabel KHGT ({NativePrayerCore.KhgtFirstYear}–{NativePrayerCore.KhgtLastYear} H); detail Hijriyah tidak ditampilkan."
                : $"{date.ToString("dddd, d MMMM yyyy", IndonesianCulture)}\nDetail Hijriyah tidak tersedia.";
        }
    }

    /// <summary>Selects a Hijri date and updates the detail caption.</summary>
    /// <param name="year">Hijri year.</param>
    /// <param name="month">Hijri month (1-12).</param>
    /// <param name="day">Hijri day.</param>
    /// <param name="gregorianDate">Gregorian date backing the cell.</param>
    private void SelectHijriDate(int year, int month, int day, DateTime gregorianDate)
    {
        _selectedHijriDate = gregorianDate.Date;
        _hasHijriSelection = true;
        string name = NativePrayerCore.HijriMonthName(month);
        IReadOnlyList<CalendarObservance> observances = NativePrayerCore.CalendarObservances(gregorianDate, GetHijriDateCorrectionDays(), UseKhgtCalendar);
        string fullObservanceLine = observances.Count == 0 ? "" : "\nPenanda: " + string.Join("; ", observances.Select(item => item.Name));
        string shortObservanceLine = observances.Count == 0 ? "" : "\nPenanda: " + string.Join("; ", observances.Select(item => item.ShortName));
        string dateCaption = $"{day} {name} {year} H — {gregorianDate.ToString("dddd, d MMMM yyyy", IndonesianCulture)} M";
        HijriSelectedDateDetail.Text = dateCaption + shortObservanceLine;
        AutomationProperties.SetName(HijriSelectedDateDetail, dateCaption + "\n" + CurrentCalendarSourceNote() + fullObservanceLine);
        UpdateHijriSelection();
    }

    /// <summary>Keeps a valid selection or defaults it to today's Hijri date, as in the Gregorian view.</summary>
    /// <param name="year">The Hijri year currently rendered.</param>
    /// <param name="calendarToday">Today in the location time zone.</param>
    private void RestoreHijriSelection(int year, DateTime calendarToday)
    {
        try
        {
            if (_hasHijriSelection && _hijriDateButtons.ContainsKey(_selectedHijriDate.Date))
            {
                NativeCalendarDate selectedHijri = HijriForCalendarDate(_selectedHijriDate);
                if (selectedHijri.Year == year)
                {
                    SelectHijriDate(selectedHijri.Year, selectedHijri.Month, selectedHijri.Day, _selectedHijriDate);
                    return;
                }
            }

            NativeCalendarDate todayHijri = HijriForCalendarDate(calendarToday);
            if (todayHijri.Year == year && _hijriDateButtons.ContainsKey(calendarToday.Date))
            {
                SelectHijriDate(todayHijri.Year, todayHijri.Month, todayHijri.Day, calendarToday.Date);
                return;
            }

            NativeHijriMonth firstMonth = NativePrayerCore.HijriMonth(year, 1, UseKhgtCalendar);
            DateTime firstDay = new DateTime(firstMonth.GregorianYear, firstMonth.GregorianMonth, firstMonth.GregorianDay)
                .AddDays(-GetHijriDateCorrectionDays());
            if (_hijriDateButtons.ContainsKey(firstDay.Date))
                SelectHijriDate(year, 1, 1, firstDay);
            else
                ClearHijriSelection();
        }
        catch (Exception ex) when (IsNativeCoreFailure(ex) || ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            AppLogger.Write("calendar.hijri.selection", ex);
            ClearHijriSelection();
        }
    }

    /// <summary>Clears the Hijri caption when the rendered year has no selectable cell.</summary>
    private void ClearHijriSelection()
    {
        _hasHijriSelection = false;
        const string prompt = "Pilih tanggal Hijriyah untuk melihat padanan Masehi.";
        HijriSelectedDateDetail.Text = prompt;
        AutomationProperties.SetName(HijriSelectedDateDetail, prompt);
    }

    /// <summary>Repaints every Hijri cell so only the selected date keeps the selection style.</summary>
    private void UpdateHijriSelection()
    {
        DateTime today = GetCalendarToday();
        foreach ((DateTime cellDate, Button button) in _hijriDateButtons)
            UpdateHijriDateButton(cellDate, button, today);
    }

    /// <summary>Applies marker styling and selection state to a Hijri date button.</summary>
    /// <param name="date">The backing Gregorian date of the cell.</param>
    /// <param name="button">The button to update.</param>
    /// <param name="today">Today's date for marker styling.</param>
    private void UpdateHijriDateButton(DateTime date, Button button, DateTime today)
    {
        bool isSelected = _hasHijriSelection && date.Date == _selectedHijriDate.Date;
        _hijriObservances.TryGetValue(date.Date, out CalendarObservance[]? observances);
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
            foreach (Shape shape in content.Children.OfType<Shape>())
            {
                string automationId = AutomationProperties.GetAutomationId(shape);
                if (automationId.StartsWith("HijriTodayRing-", StringComparison.Ordinal))
                {
                    shape.Stroke = MakeBrush(themeColors.Border);
                    shape.StrokeThickness = themeColors.BorderThickness;
                    shape.Visibility = markerStyle.IsToday ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (automationId.StartsWith("HijriCalendarMarker-", StringComparison.Ordinal))
                {
                    shape.Fill = MakeBrush(themeColors.Marker);
                }
            }
        }
    }

    /// <summary>Computes the corrected Hijri date for a Gregorian date.</summary>
    /// <param name="date">The Gregorian date.</param>
    /// <returns>The corrected Hijri date.</returns>
    private NativeCalendarDate HijriForCalendarDate(DateTime date) =>
        NativePrayerCore.HijriDate(date.Date.AddDays(GetHijriDateCorrectionDays()), UseKhgtCalendar);

    /// <summary>Returns the current time in the configured time zone.</summary>
    /// <returns>The location-local current time.</returns>
    private DateTime GetLocationNow()
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
    }

    /// <summary>Returns today in the location time zone; falls back to local date on failure.</summary>
    /// <returns>Today's date.</returns>
    private DateTime GetCalendarToday()
    {
        DateTime now;
        try { now = GetLocationNow(); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            now = DateTime.Now;
        }
        return now.Date;
    }

    /// <summary>Returns the Hijri date correction entered in the form (0 while loading settings).</summary>
    /// <returns>The correction in days.</returns>
    private int GetHijriDateCorrectionDays() =>
        !_loadingSettings && CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox?.Text, out int days)
            ? days
            : _settings.HijriDateCorrectionDays;

    /// <summary>True when the saved settings use the official KHGT calendar table.</summary>
    private bool UseKhgtCalendar => _settings.UsesKhgtCalendar;

    /// <summary>Builds the honest source note shown below both calendars.</summary>
    /// <param name="useKhgt">Whether the KHGT table is active.</param>
    /// <param name="correctionDays">Manual correction applied after the source date.</param>
    /// <returns>The source note.</returns>
    private static string CalendarSourceNote(bool useKhgt, int correctionDays)
    {
        string note = useKhgt
            ? $"Kalender Hijriyah KHGT (perkiraan) dari tabel resmi Muhammadiyah {NativePrayerCore.KhgtFirstYear}–{NativePrayerCore.KhgtLastYear} H; penetapan resmi tetap milik otoritas."
            : "Perkiraan kalender sipil/tabular, bukan penetapan resmi. Ikuti hasil rukyat atau otoritas setempat.";
        if (correctionDays != 0)
            note += $" Digeser {correctionDays:+0;-0} hari dari sumber kalender.";
        return note;
    }

    /// <summary>Returns the source note for the saved settings.</summary>
    /// <returns>The source note.</returns>
    private string CurrentCalendarSourceNote() => CalendarSourceNote(UseKhgtCalendar, GetHijriDateCorrectionDays());

    /// <summary>Returns the date for which prayer times are scheduled; falls back to tomorrow after Isha.</summary>
    /// <returns>The schedule date.</returns>
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

    /// <summary>Returns the schedule date for settings; tomorrow after Isha time, otherwise today.</summary>
    /// <param name="settings">Application settings.</param>
    /// <returns>The schedule date.</returns>
    private static DateTime GetPrayerScheduleDate(AppSettings settings)
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(settings.TimeZoneId);
        DateTime locationLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        return PrayerUiLogic.PrayerScheduleDate(locationLocalNow);
    }

    /// <summary>Returns today in the settings' time zone; falls back to local date on failure.</summary>
    /// <param name="settings">Application settings.</param>
    /// <returns>Today's date.</returns>
    private static DateTime GetCalendarToday(AppSettings settings)
    {
        TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(settings.TimeZoneId);
        DateTime locationLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        return locationLocalNow.Date;
    }

    /// <summary>Returns the Monday-based weekday index (Monday=0).</summary>
    /// <param name="date">The date to convert.</param>
    /// <returns>Weekday index, Monday=0.</returns>
    private static int MondayBasedWeekday(DateTime date) => ((int)date.DayOfWeek + 6) % 7;

    /// <summary>Updates the clock and date displays in the overview and schedule tabs.</summary>
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

    /// <summary>Reloads settings form controls from the current app settings.</summary>
    internal void ReloadSettingsFromApp()
    {
        if (!_uiDispatcher.CheckAccess()) { _ = _uiDispatcher.InvokeAsync(ReloadSettingsFromApp); return; }
        PopulateSettingsForm();
        RefreshSettingsSchedulePreview();
        RefreshOverview();
    }

    /// <summary>Loads current settings into the settings form controls.</summary>
    private void PopulateSettingsForm()
    {
        _locationRequestVersion++;
        CloseLocationSuggestions();
        OfflineReverseGeocodeStatus.Text = string.Empty;
        _loadingSettings = true;
        try
        {
            LocationNameBox.Text = _settings.LocationName;
            _locationNameAutoFilled = false;
            LatitudeBox.Text = _settings.Latitude.ToString("G", CultureInfo.InvariantCulture);
            LongitudeBox.Text = _settings.Longitude.ToString("G", CultureInfo.InvariantCulture);
            ElevationBox.Text = _settings.ElevationMeters.ToString(CultureInfo.InvariantCulture);
            _elevationManuallyEdited = false;
            TimeZoneBox.ItemsSource = TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames
                .Append(_settings.TimeZoneId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray();
            TimeZoneBox.Text = _settings.TimeZoneId;
            var options = LocationDefaults.Methods.ToList();
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
            DownloadFullDataCheckBox.IsChecked = _settings.FullOfflineDataEnabled;
            DownloadFullDataButton.IsEnabled = _settings.FullOfflineDataEnabled;
            SunnahFastingReminderCheckBox.IsChecked = _settings.SunnahFastingReminderEnabled;
            PublicHolidayReminderCheckBox.IsChecked = _settings.PublicHolidayReminderEnabled;
            IslamicHolidayReminderCheckBox.IsChecked = _settings.IslamicHolidayReminderEnabled;
            LaranganPuasaReminderCheckBox.IsChecked = _settings.NoFastReminderEnabled;
            MorningDzikirReminderCheckBox.IsChecked = _settings.MorningDzikirReminderEnabled;
            EveningDzikirReminderCheckBox.IsChecked = _settings.EveningDzikirReminderEnabled;
            SetTimePicker(SunnahFastingTimeBox, SunnahFastingMinuteBox, _settings.SunnahFastingReminderTime);
            SetTimePicker(PublicHolidayTimeBox, PublicHolidayMinuteBox, _settings.PublicHolidayReminderTime);
            SetTimePicker(IslamicHolidayTimeBox, IslamicHolidayMinuteBox, _settings.IslamicHolidayReminderTime);
            SetTimePicker(LaranganPuasaTimeBox, LaranganPuasaMinuteBox, _settings.NoFastReminderTime);
            SetTimePicker(MorningDzikirTimeBox, MorningDzikirMinuteBox, _settings.MorningDzikirReminderTime);
            SetTimePicker(EveningDzikirTimeBox, EveningDzikirMinuteBox, _settings.EveningDzikirReminderTime);
            ReminderRepeatSecondsBox.Text = _settings.ReminderRepeatSeconds.ToString(CultureInfo.InvariantCulture);
            ReminderRepeatCountBox.Text = _settings.ReminderRepeatCount.ToString(CultureInfo.InvariantCulture);
            WidgetEnabledCheckBox.IsChecked = _settings.WidgetEnabled;
            WidgetModeBox.SelectedItem = WidgetModeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetMode, StringComparison.OrdinalIgnoreCase));
            WidgetMotionBox.SelectedItem = WidgetMotionBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetMotion, StringComparison.OrdinalIgnoreCase));
            WidgetPositionBox.SelectedItem = WidgetPositionBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.WidgetPosition, StringComparison.OrdinalIgnoreCase));
            WidgetAlwaysOnTopCheckBox.IsChecked = _settings.WidgetAlwaysOnTop;
            WidgetClickThroughCheckBox.IsChecked = _settings.WidgetClickThrough;
            WidgetAutoHideCheckBox.IsChecked = _settings.WidgetAutoHideOnHover;
            ThemeBox.SelectedItem = ThemeBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, _settings.ThemeId, StringComparison.OrdinalIgnoreCase));
            DhuhaAfterSunriseBox.Text = _settings.DhuhaAfterSunriseMinutes.ToString(CultureInfo.InvariantCulture);
            HijriDateCorrectionBox.Text = _settings.HijriDateCorrectionDays.ToString(CultureInfo.InvariantCulture);
            CalendarSourceBox.SelectedItem = CalendarSourceBox.Items.OfType<ComboBoxItem>().FirstOrDefault(
                item => string.Equals(item.Tag as string,
                    _settings.UsesKhgtCalendar ? AppSettings.CalendarSourceKhgt : AppSettings.CalendarSourceCivil,
                    StringComparison.OrdinalIgnoreCase));
            HijriSourceBox.SelectedItem = HijriSourceBox.Items.OfType<ComboBoxItem>().FirstOrDefault(
                item => string.Equals(item.Tag as string,
                    _settings.UsesKhgtCalendar ? AppSettings.CalendarSourceKhgt : AppSettings.CalendarSourceCivil,
                    StringComparison.OrdinalIgnoreCase));
            StartupEnabledCheckBox.IsChecked = _settings.StartupEnabled;
            StartHiddenCheckBox.IsChecked = _settings.StartHidden;

            AutoLockEnabledCheckBox.IsChecked = _settings.AutoLockEnabled;
            LockScreenCheckBox.IsChecked = _settings.LockScreenEnabled && _settings.LockScreenOptIn;
            LockScreenDurationBox.Text = _settings.LockScreenDurationMinutes.ToString(CultureInfo.InvariantCulture);
            UpdateAutoCheckCheckBox.IsChecked = _settings.UpdateAutoCheck;
            InstalledVersionText.Text = AppBranding.VersionLabel;
            RefreshLockScreenSafety();
            UpdateTaskbarWidgetAvailability();
            OnlineGeocodingEnabledCheckBox.IsChecked = _settings.OnlineGeocodingEnabled;
            UpdateOnlineGeocodingStatus();
        }
        finally { _loadingSettings = false; }
        UpdateWidgetOptionStates();
        UpdateStartupOptionStates();
        CalendarSourceHelp.Text = CalendarSourceNote(_settings.UsesKhgtCalendar, _settings.HijriDateCorrectionDays);
        RefreshSettingsSchedulePreview();
        _savedFormBaseline = CaptureSettingsFormDraft();
        UpdateKhgtOfferPanel();
    }

    /// <summary>Returns the stored correction for a prayer as text, defaulting to "0".</summary>
    /// <param name="key">Prayer key.</param>
    /// <returns>The correction in minutes as text.</returns>
    private string Correction(string key) => _settings.Corrections.TryGetValue(key, out int value)
        ? value.ToString(CultureInfo.InvariantCulture)
        : "0";

    /// <summary>Refreshes the schedule preview when the calculation method changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || MethodBox.SelectedItem is not CalculationMethod) return;
        RefreshSettingsSchedulePreview();
    }

    /// <summary>Returns the calendar source selected in the form.</summary>
    private string SelectedCalendarSource =>
        string.Equals((CalendarSourceBox.SelectedItem as ComboBoxItem)?.Tag as string,
            AppSettings.CalendarSourceKhgt, StringComparison.OrdinalIgnoreCase)
            ? AppSettings.CalendarSourceKhgt
            : AppSettings.CalendarSourceCivil;

    /// <summary>Returns the theme currently previewed in the form, even when it is not saved yet.</summary>
    private string SelectedThemeId => ThemeCatalog.NormalizeId((ThemeBox.SelectedItem as ComboBoxItem)?.Tag as string);

    /// <summary>Updates the source help text and schedule preview when the calendar source changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void CalendarSourceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings) return;
        int correction = CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox?.Text, out int days)
            ? days
            : 0;
        CalendarSourceHelp.Text = CalendarSourceNote(SelectedCalendarSource == AppSettings.CalendarSourceKhgt, correction);
        RefreshSettingsSchedulePreview();
    }

    /// <summary>Applies the calendar source chosen directly in the Hijriyah tab.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void HijriSourceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings) return;
        string source = string.Equals((HijriSourceBox.SelectedItem as ComboBoxItem)?.Tag as string,
            AppSettings.CalendarSourceKhgt, StringComparison.OrdinalIgnoreCase)
            ? AppSettings.CalendarSourceKhgt
            : AppSettings.CalendarSourceCivil;
        ApplyCalendarSource(source);
    }

    /// <summary>Accepts the one-time KHGT offer shown on the overview tab.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void KhgtOfferAccept_Click(object sender, RoutedEventArgs e) =>
        ApplyCalendarSource(AppSettings.CalendarSourceKhgt);

    /// <summary>Declines the one-time KHGT offer and keeps the civil calendar.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void KhgtOfferDecline_Click(object sender, RoutedEventArgs e)
    {
        _settings.KhgtOfferSeen = true;
        SaveCalendarSourceChange();
        UpdateKhgtOfferPanel();
        FooterStatus.Text = "Kalender sipil/tabular tetap dipakai; pilihan bisa diubah di Pengaturan atau tab Hijriyah.";
    }

    /// <summary>Shows the KHGT offer only until an existing installation answers it once.</summary>
    private void UpdateKhgtOfferPanel()
    {
        if (KhgtOfferPanel is null) return;
        KhgtOfferPanel.Visibility = !_settings.UsesKhgtCalendar && !_settings.KhgtOfferSeen
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    /// <summary>Persists the current settings object after a calendar-source change.</summary>
    private void SaveCalendarSourceChange()
    {
        try
        {
            _saveSettings(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            _logSettingsError("settings.calendar-source", ex);
            SettingsStatusLabel.Text = "Sumber kalender berubah untuk sesi ini, tetapi gagal disimpan. Periksa izin dan ruang penyimpanan.";
        }
    }

    /// <summary>Applies a calendar source immediately and refreshes every dependent view.</summary>
    /// <param name="source">Either the civil or the KHGT source constant.</param>
    private void ApplyCalendarSource(string source)
    {
        bool hadUnsavedChanges = HasUnsavedSettingsChanges();
        _settings.CalendarSource = string.Equals(source, AppSettings.CalendarSourceKhgt, StringComparison.OrdinalIgnoreCase)
            ? AppSettings.CalendarSourceKhgt
            : AppSettings.CalendarSourceCivil;
        _settings.KhgtOfferSeen = true;
        SaveCalendarSourceChange();

        bool wasLoading = _loadingSettings;
        _loadingSettings = true;
        try
        {
            CalendarSourceBox.SelectedItem = CalendarSourceBox.Items.OfType<ComboBoxItem>().FirstOrDefault(
                item => string.Equals(item.Tag as string, _settings.CalendarSource, StringComparison.OrdinalIgnoreCase));
            HijriSourceBox.SelectedItem = HijriSourceBox.Items.OfType<ComboBoxItem>().FirstOrDefault(
                item => string.Equals(item.Tag as string, _settings.CalendarSource, StringComparison.OrdinalIgnoreCase));
            CalendarSourceHelp.Text = CurrentCalendarSourceNote();
            HijriCalendarNote.Text = CurrentCalendarSourceNote();
        }
        finally { _loadingSettings = wasLoading; }

        // The source is already persisted, so it must not look like an unsaved form edit.
        if (!hadUnsavedChanges)
            _savedFormBaseline = CaptureSettingsFormDraft();
        UpdateKhgtOfferPanel();
        _refreshDependentViews();
        // RefreshDependentViews applies the saved theme. Keep an unsaved theme
        // choice visible so changing only the calendar source cannot silently
        // discard the preview and make the next Simpan change it a second time.
        if (hadUnsavedChanges)
            _applyThemePreview(SelectedThemeId);
        if (_calendarViewsLoaded)
            RefreshCalendarViews();
        RefreshSettingsSchedulePreview();
        FooterStatus.Text = _settings.UsesKhgtCalendar
            ? "Kalender Hijriyah memakai tabel KHGT resmi 1440–1499 H (perkiraan)."
            : "Kalender Hijriyah memakai kalender sipil/tabular (perkiraan).";
    }

    /// <summary>Applies a live theme preview when the theme selection changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || ThemeBox.SelectedItem is not ComboBoxItem themeItem ||
            themeItem.Tag is not string themeId)
            return;

        _applyThemePreview(themeId);
    }

    /// <summary>Updates widget option states and applies an appearance preview when the mode changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void WidgetModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings) return;
        UpdateWidgetOptionStates();
        ApplyWidgetAppearancePreview();
    }

    /// <summary>Applies an appearance preview when widget preview options change.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void WidgetPreviewOptions_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings)
            ApplyWidgetAppearancePreview();
    }

    /// <summary>Computes the widget appearance from the form and forwards it to the app for live preview.</summary>
    private void ApplyWidgetAppearancePreview()
    {
        string mode = (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetMode;
        string motion = (WidgetMotionBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetMotion;
        string position = (WidgetPositionBox.SelectedItem as ComboBoxItem)?.Tag as string ?? _settings.WidgetPosition;
        _applyWidgetAppearancePreview(
            WidgetEnabledCheckBox.IsChecked == true, mode, motion, position,
            WidgetAlwaysOnTopCheckBox.IsChecked == true, WidgetClickThroughCheckBox.IsChecked == true,
            WidgetAutoHideCheckBox.IsChecked == true);
    }

    /// <summary>Applies the startup registration when the startup checkbox changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void StartupEnabled_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings)
            UpdateStartupOptionStates();
    }

    /// <summary>Enables/disables the start-hidden checkbox based on the startup checkbox state.</summary>
    private void UpdateStartupOptionStates()
    {
        if (StartHiddenCheckBox is not null && StartupEnabledCheckBox is not null)
            StartHiddenCheckBox.IsEnabled = true;
    }

    /// <summary>Loads the lock-screen preview image and safety text into the settings tab.</summary>
    private void LoadLockScreenPreview()
    {
        if (LockScreenPreviewImage is null) return;
        try
        {
            (string key, TimeSpan time) = PreviewPrayer();
            LockScreenPreviewImage.Source = LockScreenImageRenderer.RenderPreview(key, time);
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ArgumentException or FileFormatException or InvalidOperationException)
        {
            AppLogger.Write("lock-screen.preview", ex);
        }
    }

    /// <summary>Chooses the next essential prayer for the preview image.</summary>
    /// <returns>The prayer key and its scheduled time.</returns>
    private (string Key, TimeSpan Time) PreviewPrayer()
    {
        try
        {
            DateTime now = GetLocationNow();
            IReadOnlyList<PrayerEntry> essential = PrayerUiLogic.EssentialPrayerTimes(
                PrayerScheduleService.ForDate(now, _settings));
            PrayerEntry? next = essential.Where(entry => entry.Time >= now.TimeOfDay)
                .OrderBy(entry => entry.Time)
                .FirstOrDefault();
            if (next is not null) return (next.Key, next.Time);
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            AppLogger.Write("lock-screen.preview-schedule", ex);
        }
        return ("Asr", TimeSpan.FromMinutes(15 * 60 + 1));
    }

    /// <summary>Installs the real lock-screen image for one minute as a live test.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private async void LockScreenTestNow_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app)
        {
            LockScreenStatus.Text = "Uji hanya tersedia saat aplikasi berjalan penuh.";
            return;
        }
        if (!ConfirmDialog.Ask(this, "Uji lock screen",
            "Gambar uji akan dipasang selama satu menit, lalu layar dikunci.\n\n" +
            "Setelah Anda masuk kembali, gambar lock screen dipulihkan otomatis.",
            "Pasang & kunci layar"))
            return;

        LockScreenTestNowButton.IsEnabled = false;
        LockScreenStatus.Text = "Memasang gambar uji selama satu menit…";
        bool imageApplied = false;
        bool screenLocked = false;
        string resultText = "none";
        try
        {
            LockScreenTransactionResult result = await app.TestLockScreenNowAsync();
            resultText = result.ToString();
            if (result is LockScreenTransactionResult.Applied or LockScreenTransactionResult.Extended)
            {
                imageApplied = true;
                LockScreenStatus.Text = "Gambar uji terpasang. Mengunci layar dalam dua detik…";
                await Task.Delay(TimeSpan.FromSeconds(2));
                screenLocked = WorkstationLockService.TryLock();
                LockScreenStatus.Text = screenLocked
                    ? "Layar dikunci. Setelah masuk kembali, gambar dipulihkan otomatis dalam satu menit."
                    : "Gambar uji terpasang, tetapi layar tidak dapat dikunci. Kunci Windows secara manual untuk melihatnya; gambar dipulihkan otomatis setelah satu menit.";
            }
            else if (result == LockScreenTransactionResult.ExternalChangePreserved)
            {
                LockScreenStatus.Text = "Perubahan lock screen dari pengguna dipertahankan; gambar uji tidak dipasang.";
            }
            else if (result == LockScreenTransactionResult.ManagedByPolicy)
            {
                LockScreenStatus.Text = "Perangkat ini dikelola kebijakan organisasi; gambar lock screen tidak diubah. Hubungi admin IT.";
            }
            else
            {
                LockScreenStatus.Text = "Status lock screen belum dapat diverifikasi; gambar uji tidak dipasang.";
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            AppLogger.Write("lock-screen.test", ex);
            LockScreenStatus.Text = "Uji gagal: " + ex.Message;
        }
        finally
        {
            AppLogger.WriteInfo("lock-screen.test.lock",
                $"result={(imageApplied && screenLocked ? "ok" : "failed")} reason={resultText} " +
                $"image={(imageApplied ? "applied" : "not-applied")} " +
                $"lock={(screenLocked ? "ok" : "not-confirmed")} visual=unverified");
            LockScreenTestNowButton.IsEnabled = true;
        }
    }

    /// <summary>Reloads the lock-screen preview image when it is clicked.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event arguments.</param>
    private void LockScreenPreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (LockScreenPreviewImage.Source is not ImageSource source) return;
        e.Handled = true;
        const double chromeHeight = 48;
        double availableWidth = SystemParameters.WorkArea.Width * 0.9;
        double availableHeight = SystemParameters.WorkArea.Height * 0.9 - chromeHeight;
        double aspect = source.Width > 0 && source.Height > 0 ? source.Width / source.Height : 16.0 / 9.0;
        double width = Math.Min(availableWidth, availableHeight * aspect);
        var window = new Window
        {
            Title = "Pratinjau gambar lock screen",
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = width,
            Height = width / aspect + chromeHeight,
            MinWidth = 480,
            MinHeight = 300,
            Background = MakeBrush(ThemeManager.Current.Surface),
            Content = new Image { Source = source, Stretch = Stretch.Uniform, Margin = new Thickness(12) }
        };
        AutomationProperties.SetName(window, "Pratinjau gambar lock screen diperbesar");
        window.PreviewKeyDown += (_, args) =>
        {
            if (args.Key != Key.Escape) return;
            args.Handled = true;
            window.Close();
        };
        window.ShowDialog();
    }

    /// <summary>Re-evaluates lock-screen safety and updates the checkbox, safety text, and status.</summary>
    /// <param name="status">Optional status override to display.</param>
    private void RefreshLockScreenSafety(string? status = null)
    {
        if (LockScreenCheckBox is null || LockScreenSafetyText is null || LockScreenStatus is null)
            return;

        LockScreenSafetyInfo safety = _inspectLockScreenSafety();
        bool alreadyEnabled = _settings.LockScreenOptIn && _settings.LockScreenEnabled;
        LockScreenCheckBox.IsEnabled = !safety.ManagedByPolicy && (safety.CanChangeTemporarily || alreadyEnabled);
        if (LockScreenTestNowButton is not null)
            LockScreenTestNowButton.IsEnabled = !safety.ManagedByPolicy && (safety.CanChangeTemporarily || alreadyEnabled);
        LockScreenSafetyText.Text = safety.Message;
        LockScreenStatus.Text = status ?? string.Empty;
        LockScreenStatus.Visibility = Visibility.Visible;
        if (LockScreenOptOutButton is not null)
            LockScreenOptOutButton.Visibility = safety.ManagedByPolicy || alreadyEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
        if (LockScreenCancelRecoveryButton is not null)
        {
            LockScreenCancelRecoveryButton.Visibility = Visibility.Visible;
            LockScreenCancelRecoveryButton.IsEnabled =
                Application.Current is App app && app.HasPendingLockScreenRecovery;
        }
    }

    /// <summary>Restores the lock-screen image immediately, without waiting for the scheduled restore.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private async void LockScreenRestoreNow_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        LockScreenRestoreNowButton.IsEnabled = false;
        try
        {
            await app.RestoreLockScreenNowAsync();
        }
        finally
        {
            LockScreenRestoreNowButton.IsEnabled = true;
        }
        RefreshLockScreenSafety();
    }

    /// <summary>Clears a stuck lock-screen restore journal after an explicit, informed confirmation.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void LockScreenCancelRecovery_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        if (!ConfirmDialog.Ask(this, "Batalkan pemulihan lock screen",
            "Lock screen masih menampilkan gambar sholat dan menghalangi pemasangan/penghapusan.\n\n" +
            "Batalkan pemulihan? Catatan pemulihan dihapus, fitur gambar lock screen dimatikan, dan gambar lock screen " +
            "dibiarkan seperti saat ini. Lanjutkan hanya bila gambar lock screen Anda sudah sesuai tampilan semula dan " +
            "tidak bisa dipastikan kembali.",
            "Batalkan pemulihan"))
            return;
        app.CancelLockScreenRecovery();
        LockScreenCheckBox.IsChecked = false;
        RefreshLockScreenSafety("Pemulihan lock screen dibatalkan; catatan pemulihan dihapus dan fitur gambar dimatikan.");
    }

    /// <summary>Stops installing and trying the lock-screen image without touching any registry policy.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void LockScreenOptOut_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDialog.Ask(this, "Matikan gambar lock screen",
            "Hentikan pemasangan gambar lock screen di perangkat ini?\n\n" +
            "Aplikasi berhenti mencoba pada setiap waktu sholat dan tidak mengubah kebijakan perangkat. " +
            "Gambar yang sedang aktif akan dikembalikan oleh pemulihan otomatis bila catatannya masih sah.",
            "Matikan gambar"))
            return;

        _settings.LockScreenEnabled = false;
        _settings.LockScreenOptIn = false;
        try
        {
            _saveSettings(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            _logSettingsError("settings.lock-screen-opt-out", ex);
            RefreshLockScreenSafety("Fitur dimatikan untuk sesi ini, tetapi pengaturan gagal disimpan. Periksa izin dan ruang penyimpanan.");
            return;
        }
        LockScreenCheckBox.IsChecked = false;
        RefreshLockScreenSafety("Gambar lock screen dimatikan. Aplikasi tidak akan mencoba memasang gambar sampai fitur dinyalakan lagi.");
    }

    /// <summary>Opens the update window for an explicit check.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void CheckUpdatesNow_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current is App app) app.ShowUpdateWindow();
    }

    /// <summary>Refreshes the reported sizes of log, update, and lock-screen folders.</summary>
    private void RefreshStorageSummary()
    {
        if (StorageSummaryText is null) return;
        long log = StorageMaintenance.GetLogBytes();
        long update = StorageMaintenance.GetFolderBytes(StorageMaintenance.UpdateFolder);
        long images = StorageMaintenance.GetFolderBytes(StorageMaintenance.LockScreenFolder);
        string backupStatus = File.Exists(SettingsStore.BackupPath) ? "ada" : "belum ada";
        StorageSummaryText.Text =
            "Total " + StorageMaintenance.FormatBytes(log + update + images) +
            " · Log " + StorageMaintenance.FormatBytes(log) +
            " · Unduhan pembaruan " + StorageMaintenance.FormatBytes(update) +
            " · Gambar lock screen " + StorageMaintenance.FormatBytes(images) +
            " · Cadangan pengaturan " + backupStatus + "." +
            (string.IsNullOrWhiteSpace(SettingsStore.BackupWarning) ? "" : " " + SettingsStore.BackupWarning);
    }

    /// <summary>Opens the per-user data folder in Explorer.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(StorageMaintenance.DataRoot);
            Process.Start(new ProcessStartInfo(StorageMaintenance.DataRoot) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            AppLogger.Write("storage.open-folder", ex);
            StorageStatusText.Text = "Folder data tidak bisa dibuka: " + ex.Message;
        }
    }

    /// <summary>Opens the current log file, or the data folder when no log exists yet.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OpenLog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (File.Exists(StorageMaintenance.LogPath))
            {
                Process.Start(new ProcessStartInfo(StorageMaintenance.LogPath) { UseShellExecute = true });
                return;
            }
            Directory.CreateDirectory(StorageMaintenance.DataRoot);
            StorageStatusText.Text = "Belum ada log. Folder data dibuka.";
            Process.Start(new ProcessStartInfo(StorageMaintenance.DataRoot) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            AppLogger.Write("storage.open-log", ex);
            StorageStatusText.Text = "Log tidak bisa dibuka: " + ex.Message;
        }
    }

    /// <summary>Clears logs, downloaded updates, and stale lock-screen images after confirmation.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDialog.Ask(this, "Bersihkan cache & log",
            "Hapus log, unduhan pembaruan, dan gambar lock screen lama?\n\n" +
            "Pengaturan, jadwal, dan catatan pemulihan lock screen tidak dihapus. " +
            "Gambar lock screen yang masih dibutuhkan pemulihan akan dilewati.",
            "Bersihkan"))
            return;

        long freed = StorageMaintenance.ClearLogs()
            + StorageMaintenance.ClearUpdateDownloads()
            + StorageMaintenance.ClearLockScreenImages();
        RefreshStorageSummary();
        StorageStatusText.Text = StorageMaintenance.HasPendingLockScreenRecovery
            ? "Cache dibersihkan (" + StorageMaintenance.FormatBytes(freed) + "). Gambar lock screen dilewati karena pemulihan masih tertunda."
            : "Cache dibersihkan; sekitar " + StorageMaintenance.FormatBytes(freed) + " ruang dibebaskan.";
    }

    /// <summary>Resets all settings to factory defaults after confirmation.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void ResetConfig_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDialog.Ask(this, "Reset pengaturan awal",
            "Hapus semua pengaturan aplikasi dan mulai lagi seperti instal baru?\n\n" +
            "Pengaturan dan cadangannya dihapus; perubahan form yang belum disimpan dibatalkan. " +
            "Log, unduhan, dan catatan pemulihan lock screen tidak diubah.",
            "Reset pengaturan"))
            return;
        try
        {
            AppSettings fresh = SettingsStore.Reset();
            CopySettings(fresh, _settings);
            _refreshDependentViews();
            PopulateSettingsForm();
            if (_calendarViewsLoaded)
                RefreshCalendarViews();
            RefreshStorageSummary();
            SettingsStatusLabel.Text = "Pengaturan kembali ke nilai awal pabrik.";
            StorageStatusText.Text = "Pengaturan dan cadangannya dihapus. Nilai awal berlaku untuk sesi ini.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _logSettingsError("settings.reset-factory", ex);
            StorageStatusText.Text = "Reset gagal: " + ex.Message;
        }
    }

    /// <summary>Saves a sanitized diagnostics report chosen by the user.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void ExportDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Simpan laporan diagnostik",
            Filter = "Berkas teks (*.txt)|*.txt|Semua berkas (*.*)|*.*",
            DefaultExt = ".txt",
            FileName = "AyoJamaah-diagnostik-" + DateTime.Now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture) + ".txt"
        };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            File.WriteAllText(dialog.FileName, StorageMaintenance.BuildDiagnostics(_settings),
                new System.Text.UTF8Encoding(false));
            StorageStatusText.Text = "Laporan diagnostik disimpan. Periksa isinya sebelum dibagikan.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            AppLogger.Write("storage.export-diagnostics", ex);
            StorageStatusText.Text = "Laporan tidak bisa disimpan: " + ex.Message;
        }
    }

    /// <summary>Sets the storage/update status text; marshals to the UI thread when needed.</summary>
    /// <param name="message">Status message to display.</param>
    internal void SetStorageStatus(string message)
    {
        if (!_uiDispatcher.CheckAccess())
        {
            _uiDispatcher.BeginInvoke(() => SetStorageStatus(message));
            return;
        }
        StorageStatusText.Text = message;
    }

    /// <summary>Sets the lock-screen status text; marshals to the UI thread when needed.</summary>
    /// <param name="message">Status message to display.</param>
    internal void SetLockScreenStatus(string message)
    {
        if (!_uiDispatcher.CheckAccess())
        {
            _uiDispatcher.BeginInvoke(() => SetLockScreenStatus(message));
            return;
        }
        RefreshLockScreenSafety(message);
    }

    /// <summary>Applies a taskbar appearance preview when taskbar options change.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void TaskbarPreviewOptions_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingSettings) ApplyTaskbarAppearancePreview();
    }

    /// <summary>Computes the taskbar appearance from the form and forwards it to the app for live preview.</summary>
    private void ApplyTaskbarAppearancePreview() =>
        _applyTaskbarAppearancePreview(TaskbarWidgetEnabledCheckBox.IsChecked == true, "popup");

    /// <summary>Enables/disables widget option controls based on the current mode selection.</summary>
    private void UpdateWidgetOptionStates()
    {
        if (WidgetMotionBox is null || WidgetPositionBox is null ||
            WidgetMotionLabel is null || WidgetPositionLabel is null ||
            WidgetAlwaysOnTopCheckBox is null || WidgetAutoHideCheckBox is null)
            return;

        string selectedMode = (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "essential";
        string mode = WidgetSettingsLogic.NormalizeMode(selectedMode);
        bool isEssential = mode == "essential";
        bool isDetailMode = mode == "detail";

        WidgetMotionBox.IsEnabled = isEssential;
        WidgetMotionLabel.IsEnabled = isEssential;
        WidgetPositionBox.IsEnabled = true;
        WidgetPositionLabel.IsEnabled = true;
        // Medium Detail never stays above other windows, so the option only
        // applies to the thin Essential/Minimalist bars.
        WidgetAlwaysOnTopCheckBox.IsEnabled = !isDetailMode;
        // The large Detail panel is interacted with directly, so hover-hiding is
        // limited to the thin Essential/Minimalist bars.
        WidgetAutoHideCheckBox.IsEnabled = !isDetailMode;
    }

    /// <summary>Schedules a save-free schedule preview refresh when a settings input changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Text changed event arguments.</param>
    private async void SettingsPreviewInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (_loadingSettings) return;

        if (sender == ElevationBox && !_settingAutoElevation)
            _elevationManuallyEdited = true;
        if (sender == LocationNameBox && !_settingLocationName)
            _locationNameAutoFilled = false;
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
            int requestVersion = _locationRequestVersion;
            string timeZoneId = _settings.TimeZoneId;
            try { timeZoneId = LocationDefaults.ResolveTimeZoneForCoordinates(latitude, longitude); }
            catch (Exception) { /* Keep the configured zone for ocean or unresolvable coordinates. */ }
            _loadingSettings = true;
            try
            {
                TimeZoneBox.Text = timeZoneId;
                ApplyAutoCalculationMethod(null, timeZoneId);
            }
            finally { _loadingSettings = false; }
            await UpdateLocationNameFromCoordinatesAsync(latitude, longitude, timeZoneId, requestVersion);
        }
        RefreshSettingsSchedulePreview();
    }

    /// <summary>Fills the location name, elevation, and method from offline datasets, optionally supplemented by Nominatim.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="timeZoneId">IANA time zone already resolved for the coordinates.</param>
    /// <param name="requestVersion">Request generation captured before the lookup.</param>
    private async Task UpdateLocationNameFromCoordinatesAsync(double latitude, double longitude, string timeZoneId, int requestVersion)
    {
        bool onlineEnabled = OnlineGeocodingEnabledCheckBox.IsChecked == true;
        string onlineEndpoint = _settings.OnlineGeocodingEndpoint;
        try
        {
            OfflineLocationResult? place = await Task.Run(
                () => OfflineReverseGeocoder.Current.FindNearest(latitude, longitude));
            if (requestVersion != _locationRequestVersion) return;

            OnlineLocationResult? online = null;
            string? onlineError = null;
            if (onlineEnabled)
            {
                try
                {
                    var nominatim = new NominatimReverseGeocodingService();
                    online = await nominatim.ReverseGeocodeCoordinatesAsync(
                        latitude, longitude, onlineEndpoint, userConsentGranted: true);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException
                    or IOException or InvalidDataException or ArgumentException or InvalidOperationException or System.Text.Json.JsonException)
                {
                    onlineError = ex.Message;
                }
            }
            if (requestVersion != _locationRequestVersion) return;

            _elevationManuallyEdited = false;
            ApplyAutoElevation(place?.ElevationMeters);
            string displayName = place?.DisplayName ?? online?.DisplayName ?? "Lokasi manual";
            SetAutoLocationName(displayName);
            _locationNameAutoFilled = place is not null || online is not null;
            ApplyAutoCalculationMethod(place?.CountryCode, timeZoneId);

            if (online is not null && place is not null)
                OfflineReverseGeocodeStatus.Text = $"{AutoLocationSourceLabel(place)}: {place.DisplayName}. Pelengkap Nominatim: {online.DisplayName}. Belum disimpan.";
            else if (online is not null)
                OfflineReverseGeocodeStatus.Text = $"Nama dari Nominatim online: {online.DisplayName}. {online.Attribution}. Belum disimpan.";
            else if (place is not null && onlineError is not null)
                OfflineReverseGeocodeStatus.Text = $"{AutoLocationSourceLabel(place)}: {place.DisplayName}, sekitar {place.DistanceKm:0.0} km. Nominatim gagal, data offline dipakai.";
            else if (place is not null)
                OfflineReverseGeocodeStatus.Text = onlineEnabled
                    ? $"{AutoLocationSourceLabel(place)}: {place.DisplayName}, sekitar {place.DistanceKm:0.0} km. Belum disimpan."
                    : $"{AutoLocationSourceLabel(place)}: {place.DisplayName}, sekitar {place.DistanceKm:0.0} km. Perkiraan, bukan alamat; belum disimpan dan tidak dikirim ke internet.";
            else if (onlineError is not null)
                OfflineReverseGeocodeStatus.Text = $"Tidak ada permukiman lokal dalam radius 75 km dan Nominatim gagal. Nama diisi 'Lokasi manual'; tidak ada data dikirim selain percobaan Nominatim yang gagal.";
            else
                OfflineReverseGeocodeStatus.Text = onlineEnabled
                    ? "Tidak ada permukiman lokal/Nominatim yang cocok. Nama diisi 'Lokasi manual'; belum disimpan."
                    : "Tidak ada permukiman dalam radius 75 km. Nama diisi 'Lokasi manual'; koordinat dan metode tetap lokal, belum disimpan.";
        }
        catch (Exception)
        {
            if (requestVersion != _locationRequestVersion) return;
            SetAutoLocationName("Lokasi manual");
            _locationNameAutoFilled = false;
            ApplyAutoCalculationMethod(null, timeZoneId);
            OfflineReverseGeocodeStatus.Text = "Dataset lokal tidak dapat dibaca. Nama diisi 'Lokasi manual'; koordinat, zona waktu, dan metode tetap di draf.";
        }
    }

    /// <summary>Sets the location name from an automatic source without treating it as user input.</summary>
    /// <param name="name">Generated display name.</param>
    private void SetAutoLocationName(string name)
    {
        _settingLocationName = true;
        try { LocationNameBox.Text = name; }
        finally { _settingLocationName = false; }
    }

    /// <summary>Describes which offline dataset produced the automatic name.</summary>
    /// <param name="place">The lookup result.</param>
    /// <returns>A short Indonesian description.</returns>
    private static string AutoLocationSourceLabel(OfflineLocationResult place) =>
        place.IsIndonesiaLayer
            ? "Nama otomatis dari data desa/kecamatan Indonesia"
            : "Nama otomatis dari daftar kota lokal";

    /// <summary>Applies the automatic calculation method without retriggering the form handlers.</summary>
    /// <param name="countryCode">ISO country code from the location lookup, when known.</param>
    /// <param name="timeZoneId">IANA time zone already resolved for the location.</param>
    private void ApplyAutoCalculationMethod(string? countryCode, string timeZoneId)
    {
        int currentId = MethodBox.SelectedItem is CalculationMethod current ? current.Id : _settings.MethodId;
        CalculationMethod method = LocationDefaults.ResolveAutoCalculationMethod(countryCode, timeZoneId, currentId);
        if (currentId == method.Id) return;
        bool wasLoading = _loadingSettings;
        _loadingSettings = true;
        try { MethodBox.SelectedValue = method.Id; }
        finally { _loadingSettings = wasLoading; }
    }

    /// <summary>
    /// Fills the elevation field from the nearest settlement, unless the user has
    /// typed their own value. Values in metres are clamped by <see cref="AppSettings"/>.
    /// </summary>
    /// <param name="elevationMeters">DEM height from the offline dataset, or null.</param>
    private void ApplyAutoElevation(int? elevationMeters)
    {
        if (elevationMeters is not int value || _elevationManuallyEdited) return;
        string text = Math.Clamp(value, 0, 9000).ToString(CultureInfo.InvariantCulture);
        _settingAutoElevation = true;
        try
        {
            ElevationBox.Text = text;
        }
        finally { _settingAutoElevation = false; }
    }

    /// <summary>Refreshes the Hijri calendar after the correction input changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Text changed event arguments.</param>
    private void HijriCorrectionInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (_loadingSettings ||
            !CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out _))
            return;

        RefreshOverview();
        if (_calendarViewsLoaded)
            RefreshCalendarViews();
        RefreshSettingsSchedulePreview();
    }

    /// <summary>Debounces the location search input before running it.</summary>
    private void QueueLocationSearch()
    {
        CloseLocationSuggestions();
        if (LocationNameBox.Text.Trim().Length >= 2)
            _locationSearchTimer.Start();
    }

    /// <summary>Timer handler; runs the offline location search for the pending query.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void LocationSearchTimer_Tick(object? sender, EventArgs e)
    {
        _locationSearchTimer.Stop();
        int version = _locationSearchVersion;
        string query = LocationNameBox.Text.Trim();
        if (_loadingSettings || query.Length < 2) return;
        bool onlineEnabled = OnlineGeocodingEnabledCheckBox.IsChecked == true;
        string onlineEndpoint = _settings.OnlineGeocodingEndpoint;
        try
        {
            Task<IReadOnlyList<OfflineLocationSuggestion>> offlineTask = Task.Run(() =>
                OfflineReverseGeocoder.Current.Search(query, maxResults: 8));
            Task<IReadOnlyList<OfflineLocationSuggestion>> onlineTask = onlineEnabled
                ? new NominatimReverseGeocodingService().SearchAsync(query, onlineEndpoint, userConsentGranted: true, limit: 5)
                : Task.FromResult<IReadOnlyList<OfflineLocationSuggestion>>([]);

            IReadOnlyList<OfflineLocationSuggestion> offline = [];
            IReadOnlyList<OfflineLocationSuggestion> online = [];
            bool onlineFailed = false;
            try { offline = await offlineTask; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException) { offline = []; }
            if (onlineEnabled)
            {
                try { online = await onlineTask; }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException
                    or IOException or InvalidDataException or ArgumentException or InvalidOperationException
                    or System.Text.Json.JsonException)
                { onlineFailed = true; }
            }
            if (version != _locationSearchVersion) return;

            var merged = MergeLocationSuggestions(offline, online, 10);
            CompleteLocationSearch(version, query, merged, onlineEnabled, online.Count > 0, onlineFailed);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            if (version != _locationSearchVersion) return;
            OfflineReverseGeocodeStatus.Text = "Data lokasi lokal tidak dapat dibaca. Nama boleh diisi sendiri; koordinat tidak diubah dan tidak dikirim ke internet.";
        }
    }

    /// <summary>Applies search results to the suggestion list when the version is current.</summary>
    private void CompleteLocationSearch(int version, string query, IReadOnlyList<OfflineLocationSuggestion> results,
        bool onlineEnabled = false, bool hasOnlineResults = false, bool onlineFailed = false)
    {
        if (_loadingSettings || _allowClose || version != _locationSearchVersion ||
            !string.Equals(query, LocationNameBox.Text.Trim(), StringComparison.Ordinal)) return;
        LocationSuggestionsList.ItemsSource = results;
        LocationSuggestionsList.SelectedIndex = -1;
        LocationSuggestionsPopup.IsOpen = results.Count > 0 && LocationNameBox.IsKeyboardFocusWithin;
        if (results.Count > 0)
        {
            string source = hasOnlineResults ? "daftar lokal dan Nominatim online"
                : onlineEnabled && onlineFailed ? "daftar lokal (Nominatim gagal, fallback offline)"
                : "daftar kota lokal";
            OfflineReverseGeocodeStatus.Text = $"Pilihan nama dari {source}. Pilih untuk mengisi koordinat dan zona waktu; perubahan masih draf sampai Simpan.";
        }
        else
        {
            OfflineReverseGeocodeStatus.Text = onlineEnabled
                ? "Tidak ada nama yang cocok dari data lokal maupun Nominatim. Coba nama lain atau isi manual."
                : "Tidak ada nama yang cocok dalam data kota/permukiman lokal. Coba nama lain atau isi nama dan koordinat sendiri; tidak ada pencarian internet.";
        }
    }

    private static IReadOnlyList<OfflineLocationSuggestion> MergeLocationSuggestions(
        IReadOnlyList<OfflineLocationSuggestion> offline,
        IReadOnlyList<OfflineLocationSuggestion> online,
        int maxResults)
    {
        var merged = new List<OfflineLocationSuggestion>(maxResults);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void AddRange(IEnumerable<OfflineLocationSuggestion> items)
        {
            foreach (OfflineLocationSuggestion item in items)
            {
                if (merged.Count >= maxResults) break;
                string key = $"{Math.Round(item.Latitude, 3)}|{Math.Round(item.Longitude, 3)}|{item.DisplayName}";
                if (seen.Add(key)) merged.Add(item);
            }
        }
        AddRange(offline);
        AddRange(online);
        return merged;
    }

    /// <summary>Fills the location form fields from a chosen suggestion.</summary>
    /// <param name="suggestion">The chosen suggestion.</param>
    private void ApplyLocationSuggestion(OfflineLocationSuggestion suggestion)
    {
        if (!LocationSuggestionsList.Items.Contains(suggestion)) return;
        string timezone;
        try
        {
            timezone = LocationDefaults.ResolveTimeZoneForCoordinates(suggestion.Latitude, suggestion.Longitude);
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
            _locationNameAutoFilled = true;
            LatitudeBox.Text = suggestion.Latitude.ToString("G", CultureInfo.InvariantCulture);
            LongitudeBox.Text = suggestion.Longitude.ToString("G", CultureInfo.InvariantCulture);
            _elevationManuallyEdited = false;
            ApplyAutoElevation(suggestion.ElevationMeters);
            TimeZoneBox.Text = timezone;
            ApplyAutoCalculationMethod(suggestion.CountryCode, timezone);
        }
        finally { _loadingSettings = false; }
        OfflineReverseGeocodeStatus.Text = $"Dipilih dari daftar lokasi: {suggestion.DisplayName}. Koordinat, mdpl, zona waktu, dan metode hitung mewakili permukiman, bukan alamat persis. Belum disimpan.";
        RefreshSettingsSchedulePreview();
        if (IsVisible && IsActive) LocationNameBox.Focus();
    }

    /// <summary>Opens the map picker beside the location name field and applies the chosen point as a draft.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private async void OpenLocationMap_Click(object sender, RoutedEventArgs e)
    {
        CloseLocationSuggestions();
        double latitude = TryDouble(LatitudeBox.Text, out double formLatitude) &&
                          double.IsFinite(formLatitude) && formLatitude is >= -90 and <= 90
            ? formLatitude
            : _settings.Latitude;
        double longitude = TryDouble(LongitudeBox.Text, out double formLongitude) &&
                           double.IsFinite(formLongitude) && formLongitude is >= -180 and <= 180
            ? formLongitude
            : _settings.Longitude;

        var dialog = new LocationMapDialog(this, latitude, longitude,
            OnlineGeocodingEnabledCheckBox.IsChecked == true, _settings.OnlineGeocodingEndpoint);
        if (dialog.ShowDialog() != true || !dialog.HasSelection) return;
        await ApplyMapLocationAsync(dialog.Latitude, dialog.Longitude, dialog.DisplayName);
    }

    /// <summary>Applies a confirmed map point to the settings draft, then resolves its name and details.</summary>
    /// <param name="latitude">Confirmed latitude.</param>
    /// <param name="longitude">Confirmed longitude.</param>
    /// <param name="previewName">Display name previewed in the picker, when known.</param>
    private async Task ApplyMapLocationAsync(double latitude, double longitude, string previewName)
    {
        int requestVersion = ++_locationRequestVersion;
        _locationNameAutoFilled = false;
        _loadingSettings = true;
        try
        {
            LatitudeBox.Text = latitude.ToString("G", CultureInfo.InvariantCulture);
            LongitudeBox.Text = longitude.ToString("G", CultureInfo.InvariantCulture);
        }
        finally { _loadingSettings = false; }

        string timezone;
        try
        {
            timezone = LocationDefaults.ResolveTimeZoneForCoordinates(latitude, longitude);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            SetAutoLocationName("Lokasi manual");
            _locationNameAutoFilled = false;
            OfflineReverseGeocodeStatus.Text = "Zona waktu titik peta ini belum dapat dicocokkan. Koordinat ada di draf; isi zona dan nama secara manual bila perlu.";
            RefreshSettingsSchedulePreview();
            return;
        }

        _loadingSettings = true;
        try { TimeZoneBox.Text = timezone; }
        finally { _loadingSettings = false; }

        OfflineReverseGeocodeStatus.Text = string.IsNullOrEmpty(previewName)
            ? "Titik dari peta dipilih. Mencari permukiman terdekat…"
            : $"Titik dari peta: {previewName}. Mencari permukiman terdekat…";
        await UpdateLocationNameFromCoordinatesAsync(latitude, longitude, timezone, requestVersion);
        if (requestVersion == _locationRequestVersion && !string.IsNullOrEmpty(previewName) &&
            string.Equals(LocationNameBox.Text, "Lokasi manual", StringComparison.Ordinal))
        {
            SetAutoLocationName(previewName);
            _locationNameAutoFilled = true;
            OfflineReverseGeocodeStatus.Text = $"Titik dari peta: {previewName}. Nama menjadi perkiraan; koordinat dan zona waktu berasal dari titik. Belum disimpan.";
        }
        RefreshSettingsSchedulePreview();
        if (IsVisible && IsActive) LocationNameBox.Focus();
    }

    /// <summary>Closes and clears the location suggestion popup.</summary>
    private void CloseLocationSuggestions()
    {
        _locationSearchVersion++;
        _locationSearchTimer.Stop();
        LocationSuggestionsPopup.IsOpen = false;
        LocationSuggestionsList.ItemsSource = null;
    }

    /// <summary>Closes the suggestions when the popup closes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void LocationSuggestionsPopup_Closed(object? sender, EventArgs e) => CloseLocationSuggestions();

    /// <summary>Closes suggestions when focus leaves the location box.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Focus event arguments.</param>
    private void LocationNameBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (!LocationNameBox.IsKeyboardFocusWithin && !LocationSuggestionsList.IsKeyboardFocusWithin)
                CloseLocationSuggestions();
        }));
    }

    /// <summary>Handles arrow/enter/escape keys in the location box to drive the suggestion list.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key event arguments.</param>
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

    /// <summary>Handles enter/escape keys in the suggestion list.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key event arguments.</param>
    private void LocationSuggestionsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { CloseLocationSuggestions(); e.Handled = true; }
        else if (e.Key == Key.Enter && LocationSuggestionsList.SelectedItem is OfflineLocationSuggestion suggestion)
        {
            ApplyLocationSuggestion(suggestion);
            e.Handled = true;
        }
    }

    /// <summary>Applies the suggestion under the mouse when clicked.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event arguments.</param>
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

    /// <summary>Recomputes and renders the schedule preview from the settings form.</summary>
    private void RefreshSettingsSchedulePreview()
    {
        if (_loadingSettings) return;
        if (!TryReadSchedulePreviewSettings(out AppSettings? preview, out string error))
        {
            ResetScheduleGrid(SettingsSchedulePreviewGrid, 7);
            SettingsScheduleDateLabel.Text = "Perkiraan belum tersedia.";
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
            NativeCalendarDate hijri = NativePrayerCore.HijriDate(calendarDate.AddDays(previewSettings.HijriDateCorrectionDays), previewSettings.UsesKhgtCalendar);
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
            SettingsScheduleDateLabel.Text = "Perkiraan belum tersedia.";
            SettingsScheduleHijriDateLabel.Text = string.Empty;
            SettingsSchedulePreviewStatus.Text = "Rincian teknis disimpan di log aplikasi.";
        }
    }

    /// <summary>Reads the form into a candidate settings object for preview validation.</summary>
    /// <param name="candidate">The parsed candidate on success.</param>
    /// <param name="error">Validation error message on failure.</param>
    /// <returns>True when the form is valid for preview.</returns>
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
        if (!TryInt(ElevationBox.Text, 0, 9000, out int elevationMeters))
        {
            error = "Ketinggian harus berupa bilangan bulat 0–9000 mdpl.";
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

        if (!CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out int hijriCorrection))
        {
            error = "Geser tanggal Hijriyah harus berada pada rentang ±7 hari.";
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
        candidate.ElevationMeters = elevationMeters;
        candidate.TimeZoneId = timezone;
        candidate.MethodId = method.Id;
        candidate.CalculationStandard = method.Name;
        candidate.DhuhaAfterSunriseMinutes = dhuhaAfterSunrise;
        candidate.Corrections = corrections;
        candidate.HijriDateCorrectionDays = hijriCorrection;
        candidate.CalendarSource = SelectedCalendarSource;
        candidate.LocationName = LocationNameBox.Text.Trim();
        candidate.Normalize();
        return true;
    }

    /// <summary>Restores a previously downloaded offline data pack without network access.</summary>
    private void TryRestoreOfflineDataCache()
    {
        try
        {
            if (_offlineDataDownloadService.TryLoadCached(out string snapshot))
                DownloadFullDataStatus.Text = OfflineDataDownloadService.InstalledStatus(snapshot);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            DownloadFullDataStatus.Text = "Cache data lengkap tidak dapat dibaca; memakai data bawaan.";
            _logSettingsError("Membaca cache data offline", ex);
        }
    }

    /// <summary>Loads the embedded dzikir and doa catalog into the three sub-tabs.</summary>
    private void LoadDzikirDoaContent()
    {
        try
        {
            DzikirDoaCatalog catalog = DzikirDoaCatalog.Load();
            DzikirDoaSection? morning = catalog.Find("dzikir-pagi");
            DzikirDoaSection? evening = catalog.Find("dzikir-petang");
            DzikirDoaSection? doa = catalog.Find("doa");
            DzikirPagiPane.DataContext = morning;
            DzikirPetangPane.DataContext = evening;
            DoaPane.DataContext = doa;
            if (morning is null || evening is null || doa is null)
                throw new InvalidDataException("Salah satu bagian dzikir atau doa tidak ditemukan.");
            DzikirDoaStatus.Text =
                $"{morning.Entries.Count} dzikir pagi · {evening.Entries.Count} dzikir petang · {doa.Entries.Count} doa. " +
                "Teks Arab, transliterasi, dan arti dari Rumaysho.Com; tautan kredit tersedia tiap entri.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException)
        {
            DzikirPagiPane.DataContext = null;
            DzikirPetangPane.DataContext = null;
            DoaPane.DataContext = null;
            DzikirDoaStatus.Text = "Dzikir dan doa belum dapat dimuat: " + ex.Message;
            _logSettingsError("dzikir-doa.load", ex);
        }
    }

    /// <summary>Opens a Rumaysho source link in the default browser after validating its host.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Navigation request containing the source URL.</param>
    private void DzikirDoaLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (e.Uri is null || !e.Uri.IsAbsoluteUri || e.Uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(e.Uri.Host, "rumaysho.com", StringComparison.OrdinalIgnoreCase))
            return;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or NotSupportedException)
        {
            _logSettingsError("dzikir-doa.link", ex);
            DzikirDoaStatus.Text = "Tautan tidak dapat dibuka. Periksa aplikasi browser bawaan Windows.";
        }
    }

    /// <summary>Enables the download button only after the user grants consent.</summary>
    private void OnlineGeocodingEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_loadingSettings) return;
        UpdateOnlineGeocodingStatus();
    }

    private void UpdateOnlineGeocodingStatus()
    {
        bool enabled = OnlineGeocodingEnabledCheckBox.IsChecked == true;
        OfflineReverseGeocodeStatus.Text = enabled
            ? "Pencarian offline dipakai dulu. Nominatim online opsional aktif; koordinat/nama pencarian dapat dikirim saat diperlukan."
            : "Pencarian offline memakai koordinat form. Tidak ada data dikirim.";
    }

    private void DownloadFullDataCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool enabled = DownloadFullDataCheckBox.IsChecked == true;
        DownloadFullDataButton.IsEnabled = enabled;
        if (_loadingSettings || _offlineDataDownloadCancellation is not null) return;
        PersistFullOfflineDataChoice(enabled);
    }

    /// <summary>Persists the offline-pack consent immediately, independent of the Simpan button.</summary>
    /// <param name="enabled">Whether the optional pack is accepted.</param>
    private void PersistFullOfflineDataChoice(bool enabled)
    {
        _settings.FullOfflineDataEnabled = enabled;
        try
        {
            _saveSettings(_settings);
            // Only this consent was persisted. Recapturing the whole form here
            // would silently mark unrelated edits as saved.
            if (_savedFormBaseline is not null && !_savingSettings && !_resolvingUnsavedChanges)
                _savedFormBaseline[FullOfflineDataDraftIndex] = enabled;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            _logSettingsError("settings.full-offline-data", ex);
            SettingsStatusLabel.Text = "Pilihan paket data lengkap belum dapat disimpan; periksa izin penyimpanan.";
        }
    }

    /// <summary>Downloads, verifies, and activates the optional complete data pack.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private async void DownloadFullData_Click(object sender, RoutedEventArgs e)
    {
        if (DownloadFullDataCheckBox.IsChecked != true) return;
        if (_offlineDataDownloadCancellation is not null)
        {
            _offlineDataDownloadCancellation.Cancel();
            DownloadFullDataButton.Content = "Membatalkan…";
            return;
        }

        var cancellation = new CancellationTokenSource();
        _offlineDataDownloadCancellation = cancellation;
        DownloadFullDataButton.Content = "Batalkan unduhan";
        DownloadFullDataProgress.Visibility = Visibility.Visible;
        DownloadFullDataProgress.Value = 0;
        var progress = new Progress<OfflineDownloadProgress>(update =>
        {
            try
            {
                DownloadFullDataStatus.Text = update.TotalBytes > 0
                    ? $"{update.Phase} {update.BytesDownloaded * 100 / update.TotalBytes}%"
                    : update.Phase;
                if (update.TotalBytes > 0)
                    DownloadFullDataProgress.Value = Math.Clamp(
                        update.BytesDownloaded * 100.0 / update.TotalBytes, 0, 100);
            }
            catch (Exception ex) { AppLogger.Write("settings.download-full-data-progress", ex); }
        });
        try
        {
            OfflineDataInstallResult result = await _offlineDataDownloadService
                .DownloadAsync(progress, cancellation.Token).ConfigureAwait(true);
            DownloadFullDataStatus.Text = result.Message;
            if (result.Installed)
            {
                DownloadFullDataCheckBox.IsChecked = true;
                PersistFullOfflineDataChoice(true);
            }
        }
        catch (OperationCanceledException)
        {
            DownloadFullDataStatus.Text = "Unduhan dibatalkan. Data bawaan tetap dipakai.";
        }
        catch (Exception ex)
        {
            AppLogger.Write("settings.download-full-data", ex);
            DownloadFullDataStatus.Text = "Unduhan gagal. Data bawaan tetap dipakai.";
        }
        finally
        {
            _offlineDataDownloadCancellation.Dispose();
            _offlineDataDownloadCancellation = null;
            DownloadFullDataButton.Content = "Unduh paket data lengkap";
            DownloadFullDataButton.IsEnabled = DownloadFullDataCheckBox.IsChecked == true;
            DownloadFullDataProgress.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>Detects approximate location via IP geolocation providers and fills the form.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
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

            string providerName = CombineLocationName(result.IpApi, result.IpWho);
            (double latitude, double longitude) = LocationDetectionService.Midpoint(result.IpApi, result.IpWho);
            string coordinateTimeZone = result.IpApi.TimeZoneId ?? _settings.TimeZoneId;
            try { coordinateTimeZone = LocationDefaults.ResolveTimeZoneForCoordinates(latitude, longitude); }
            catch (Exception) { /* Keep the provider zone when coordinates cannot be resolved. */ }
            bool wasLoading = _loadingSettings;
            _loadingSettings = true;
            try
            {
                SetAutoLocationName(providerName);
                _locationNameAutoFilled = true;
                LatitudeBox.Text = latitude.ToString("G", CultureInfo.InvariantCulture);
                LongitudeBox.Text = longitude.ToString("G", CultureInfo.InvariantCulture);
                TimeZoneBox.Text = coordinateTimeZone;
                ApplyAutoCalculationMethod(result.IpApi.CountryCode, coordinateTimeZone);
            }
            finally { _loadingSettings = wasLoading; }

            OfflineLocationResult? offlinePlace = null;
            bool offlineLookupFailed = false;
            try
            {
                offlinePlace = await Task.Run(
                    () => OfflineReverseGeocoder.Current.FindNearest(latitude, longitude));
            }
            catch (Exception)
            {
                offlineLookupFailed = true;
            }

            if (requestVersion != _locationRequestVersion) return;
            if (offlinePlace is not null)
            {
                SetAutoLocationName(offlinePlace.DisplayName);
                _locationNameAutoFilled = true;
                _elevationManuallyEdited = false;
                ApplyAutoElevation(offlinePlace.ElevationMeters);
                ApplyAutoCalculationMethod(offlinePlace.CountryCode, TimeZoneBox.Text);
                OfflineReverseGeocodeStatus.Text =
                    $"{AutoLocationSourceLabel(offlinePlace)}: {offlinePlace.DisplayName}, sekitar {offlinePlace.DistanceKm:0.0} km. Nama, mdpl, dan metode dari data lokal; belum disimpan.";
                NetworkLocationStatus.Text = $"Lokasi perkiraan diisi ({result.IpApi.CountryCode}); nama dicocokkan lokal; belum disimpan.";
            }
            else if (offlineLookupFailed)
            {
                NetworkLocationStatus.Text = $"Lokasi perkiraan diisi ({result.IpApi.CountryCode}); data lokal tidak terbaca, nama jaringan dipertahankan; belum disimpan.";
            }
            else
            {
                NetworkLocationStatus.Text = $"Lokasi perkiraan diisi ({result.IpApi.CountryCode}); tidak ada nama lokal dalam radius, nama jaringan dipertahankan; belum disimpan.";
            }
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

    /// <summary>Detects device location via GPS/Wi-Fi/cellular and fills the form.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
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
                TimeZoneBox.Text = LocationDefaults.ResolveTimeZoneForCoordinates(
                    result.Latitude!.Value, result.Longitude!.Value);
                SetAutoLocationName(DeviceLocationLogic.FormatLocalLocationName(
                    result.Source, result.Latitude!.Value, result.Longitude!.Value));
                _locationNameAutoFilled = true;
                LatitudeBox.Text = result.Latitude!.Value.ToString("G", CultureInfo.InvariantCulture);
                LongitudeBox.Text = result.Longitude!.Value.ToString("G", CultureInfo.InvariantCulture);
                ApplyAutoCalculationMethod(null, TimeZoneBox.Text);
            }
            finally { _loadingSettings = wasLoading; }
            RefreshSettingsSchedulePreview();

            GpsLocationStatus.Text = $"Lokasi perangkat diisi ({DeviceLocationLogic.SourceLabel(result.Source)}); belum disimpan.";
            OfflineLocationResult? offlinePlace = null;
            bool offlineLookupFailed = false;
            try
            {
                offlinePlace = await Task.Run(() => OfflineReverseGeocoder.Current.FindNearest(
                    result.Latitude!.Value, result.Longitude!.Value));
            }
            catch (Exception)
            {
                offlineLookupFailed = true;
            }

            if (requestVersion != _locationRequestVersion) return;
            if (offlinePlace is not null)
            {
                SetAutoLocationName(offlinePlace.DisplayName);
                _locationNameAutoFilled = true;
                _elevationManuallyEdited = false;
                ApplyAutoElevation(offlinePlace.ElevationMeters);
                ApplyAutoCalculationMethod(offlinePlace.CountryCode, TimeZoneBox.Text);
                GpsLocationStatus.Text += $" Nama terdekat: {offlinePlace.DisplayName}.";
                OfflineReverseGeocodeStatus.Text = $"{AutoLocationSourceLabel(offlinePlace)}: {offlinePlace.DisplayName}, sekitar {offlinePlace.DistanceKm:0.0} km. Tidak ada permintaan internet.";
            }
            else if (offlineLookupFailed)
            {
                OfflineReverseGeocodeStatus.Text = "Data lokasi lokal tidak bisa dibaca; koordinat tetap di form dan tidak dikirim ke internet.";
            }
            else
            {
                GpsLocationStatus.Text += " Nama lokal dipertahankan.";
                OfflineReverseGeocodeStatus.Text = "Tidak ada permukiman dalam radius 75 km; label lokal awal dipertahankan. Tidak ada permintaan internet.";
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

    /// <summary>Enables/disables the taskbar widget controls based on platform support.</summary>
    private void UpdateTaskbarWidgetAvailability()
    {
        TaskbarWidgetAvailability availability = TaskbarWidgetSupport.Inspect();
        TaskbarWidgetEnabledCheckBox.IsEnabled = availability.CanEnable;
        TaskbarWidgetEnabledCheckBox.IsChecked = availability.CanEnable && _settings.TaskbarWidgetEnabled;
        TaskbarWidgetEnabledCheckBox.ToolTip = availability.Message;
    }

    /// <summary>Formats a provider name, city/region/country for display.</summary>
    /// <param name="provider">The provider location.</param>
    /// <returns>The description text.</returns>
    private static string DescribeLocationProvider(ProviderLocation provider)
    {
        string place = string.Join(", ", new[] { provider.City, provider.Region, provider.CountryCode }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return $"{provider.ProviderName}: {(string.IsNullOrWhiteSpace(place) ? "lokasi tidak tersedia" : place)}";
    }

    /// <summary>Combines two provider locations into a display location name.</summary>
    /// <param name="first">First provider location.</param>
    /// <param name="second">Second provider location.</param>
    /// <returns>The combined location name.</returns>
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

    /// <summary>Saves the settings form when the save button is clicked.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void SaveSettings_Click(object sender, RoutedEventArgs e) => TrySaveSettings();

    /// <summary>Validates, normalizes, and persists the settings form; refreshes dependent views.</summary>
    /// <returns>True when settings were saved.</returns>
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
                        ? "Dijalankan saat masuk Windows aktif."
                        : "Status dijalankan saat masuk Windows belum bisa dipastikan.";
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
                {
                    _logSettingsError("settings.startup", ex);
                    startupStatus = "Pengaturan tersimpan, tetapi memasang aplikasi saat masuk Windows gagal.";
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

    /// <summary>Reads and validates the full settings form.</summary>
    /// <param name="candidate">The parsed settings on success.</param>
    /// <param name="error">Validation error message on failure.</param>
    /// <returns>True when the form is valid.</returns>
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
        if (!TryInt(ElevationBox.Text, 0, 9000, out int elevationMeters))
        {
            error = "Ketinggian harus berupa bilangan bulat 0–9000 mdpl.";
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
        if (!TryReminderTime(SunnahFastingTimeBox, SunnahFastingMinuteBox, "puasa sunnah", ref error, out string sunnahFastingTime) ||
            !TryReminderTime(PublicHolidayTimeBox, PublicHolidayMinuteBox, "libur nasional", ref error, out string publicHolidayTime) ||
            !TryReminderTime(IslamicHolidayTimeBox, IslamicHolidayMinuteBox, "peringatan Islam", ref error, out string islamicHolidayTime) ||
            !TryReminderTime(LaranganPuasaTimeBox, LaranganPuasaMinuteBox, "larangan puasa", ref error, out string laranganPuasaTime) ||
            !TryReminderTime(MorningDzikirTimeBox, MorningDzikirMinuteBox, "dzikir pagi", ref error, out string morningDzikirTime) ||
            !TryReminderTime(EveningDzikirTimeBox, EveningDzikirMinuteBox, "dzikir petang", ref error, out string eveningDzikirTime))
        {
            error += " Gunakan format HH.mm, misalnya 08.10.";
            return false;
        }
        if (!TryInt(ReminderRepeatSecondsBox.Text, 30, 3600, out int reminderRepeatSeconds))
        {
            error = "Jeda pengulangan harus berupa bilangan bulat 30–3600 detik.";
            return false;
        }
        if (!TryInt(ReminderRepeatCountBox.Text, 0, 10, out int reminderRepeatCount))
        {
            error = "Maksimal pengulangan harus berupa bilangan bulat 0–10 kali.";
            return false;
        }
        string reminderMessage = ReminderMessageBox.Text.Trim();
        if (reminderMessage.Length > AppSettings.MaxReminderMessageLength)
        {
            error = $"Pesan pengingat maksimal {AppSettings.MaxReminderMessageLength} karakter.";
            return false;
        }
        if (!TryInt(LockScreenDurationBox.Text, 1, 60, out int lockScreenDurationMinutes))
        {
            error = "Lama tampil gambar lock screen harus berupa bilangan bulat 1–60 menit.";
            return false;
        }
        if (!TryInt(DhuhaAfterSunriseBox.Text, 0, 180, out int dhuhaAfterSunrise))
        {
            error = "Jarak Duha setelah terbit harus berupa bilangan bulat 0–180 menit.";
            return false;
        }
        if (!CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text, out int hijriCorrection))
        {
            error = "Geser tanggal Hijriyah harus berada pada rentang −7 sampai +7 hari.";
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
            error = "Lock screen tidak bisa dicadangkan dan dikembalikan dengan aman. Pengaturan tidak diaktifkan.";
            return false;
        }

        candidate = CloneSettings(_settings);
        candidate.LockScreenEnabled = lockScreenOptIn;
        candidate.LockScreenOptIn = lockScreenOptIn;
        candidate.LockScreenDurationMinutes = lockScreenDurationMinutes;
        candidate.UpdateAutoCheck = UpdateAutoCheckCheckBox.IsChecked == true;
        candidate.IgnoredUpdateVersion = _settings.IgnoredUpdateVersion;
        candidate.LocationName = string.IsNullOrWhiteSpace(LocationNameBox.Text) ? "Lokasi belum diatur" : LocationNameBox.Text.Trim();

        candidate.Latitude = latitude;
        candidate.Longitude = longitude;
        candidate.ElevationMeters = elevationMeters;
        candidate.TimeZoneId = timezone;
        candidate.MethodId = method.Id;
        candidate.CalculationStandard = method.Name;
        candidate.Corrections = corrections;
        candidate.RemindersEnabled = ReminderEnabledCheckBox.IsChecked == true;
        candidate.AutoLockEnabled = AutoLockEnabledCheckBox.IsChecked == true;
        candidate.ReminderMinutes = reminderMinutes;
        candidate.ReminderMessage = string.IsNullOrEmpty(reminderMessage)
            ? AppSettings.DefaultReminderMessage
            : reminderMessage;
        candidate.FullOfflineDataEnabled = DownloadFullDataCheckBox.IsChecked == true;
        candidate.SunnahFastingReminderEnabled = SunnahFastingReminderCheckBox.IsChecked == true;
        candidate.PublicHolidayReminderEnabled = PublicHolidayReminderCheckBox.IsChecked == true;
        candidate.IslamicHolidayReminderEnabled = IslamicHolidayReminderCheckBox.IsChecked == true;
        candidate.NoFastReminderEnabled = LaranganPuasaReminderCheckBox.IsChecked == true;
        candidate.MorningDzikirReminderEnabled = MorningDzikirReminderCheckBox.IsChecked == true;
        candidate.EveningDzikirReminderEnabled = EveningDzikirReminderCheckBox.IsChecked == true;
        candidate.SunnahFastingReminderTime = sunnahFastingTime;
        candidate.PublicHolidayReminderTime = publicHolidayTime;
        candidate.IslamicHolidayReminderTime = islamicHolidayTime;
        candidate.NoFastReminderTime = laranganPuasaTime;
        candidate.MorningDzikirReminderTime = morningDzikirTime;
        candidate.EveningDzikirReminderTime = eveningDzikirTime;
        candidate.ReminderRepeatSeconds = reminderRepeatSeconds;
        candidate.ReminderRepeatCount = reminderRepeatCount;
        candidate.WidgetEnabled = WidgetEnabledCheckBox.IsChecked == true;
        candidate.WidgetAlwaysOnTop = WidgetAlwaysOnTopCheckBox.IsChecked == true;
        candidate.WidgetClickThrough = WidgetClickThroughCheckBox.IsChecked == true;
        candidate.WidgetAutoHideOnHover = WidgetAutoHideCheckBox.IsChecked == true;
        candidate.TaskbarWidgetEnabled = TaskbarWidgetEnabledCheckBox.IsChecked == true && TaskbarWidgetSupport.Inspect().CanEnable;
        candidate.TaskbarWidgetMode = "popup";
        candidate.OnlineGeocodingEnabled = OnlineGeocodingEnabledCheckBox.IsChecked == true;
        if (WidgetModeBox.SelectedItem is not ComboBoxItem widgetModeItem ||
            widgetModeItem.Tag is not string widgetMode || widgetMode is not ("essential" or "minimalist" or "detail"))
        {
            error = "Pilih tampilan Lengkap, Ringkas, atau Rinci.";
            return false;
        }
        if (WidgetMotionBox.SelectedItem is not ComboBoxItem widgetMotionItem ||
            widgetMotionItem.Tag is not string widgetMotion || widgetMotion is not ("static" or "marquee"))
        {
            error = "Pilih gerakan Diam atau Berjalan untuk tampilan Lengkap.";
            return false;
        }
        if (WidgetPositionBox.SelectedItem is not ComboBoxItem widgetPositionItem ||
            widgetPositionItem.Tag is not string widgetPosition || widgetPosition is not ("tc" or "tl" or "tr" or "bl" or "br"))
        {
            error = "Pilih posisi tengah atas atau salah satu sudut untuk tampilan Rinci.";
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
        candidate.HijriDateCorrectionDays = hijriCorrection;
        candidate.CalendarSource = SelectedCalendarSource;
        candidate.StartupEnabled = StartupEnabledCheckBox.IsChecked == true;
        candidate.StartHidden = StartHiddenCheckBox.IsChecked == true;
        candidate.Normalize();
        return true;
    }

    /// <summary>Enumerates the per-prayer correction input boxes with their keys.</summary>
    /// <returns>Key/box pairs for the correction editors.</returns>
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

    /// <summary>Attaches numeric input behavior to the correction boxes.</summary>
    private void AttachCorrectionEditors()
    {
        CorrectionInputBehavior.Attach(CorrectionFajrBox, CorrectionFajrDecreaseButton, CorrectionFajrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionSunriseBox, CorrectionSunriseDecreaseButton, CorrectionSunriseIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionDhuhaBox, CorrectionDhuhaDecreaseButton, CorrectionDhuhaIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionDhuhrBox, CorrectionDhuhrDecreaseButton, CorrectionDhuhrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionAsrBox, CorrectionAsrDecreaseButton, CorrectionAsrIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionMaghribBox, CorrectionMaghribDecreaseButton, CorrectionMaghribIncreaseButton);
        CorrectionInputBehavior.Attach(CorrectionIshaBox, CorrectionIshaDecreaseButton, CorrectionIshaIncreaseButton);
        CorrectionInputBehavior.AttachDateCorrection(HijriDateCorrectionBox, HijriDateCorrectionDecreaseButton, HijriDateCorrectionIncreaseButton);
        CorrectionInputBehavior.AttachMinutesAfterSunrise(DhuhaAfterSunriseBox, DhuhaAfterSunriseDecreaseButton, DhuhaAfterSunriseIncreaseButton);
    }

    /// <summary>Parses an invariant-culture double.</summary>
    /// <param name="text">The input text.</param>
    /// <param name="value">The parsed value on success.</param>
    /// <returns>True when parsing succeeded.</returns>
    private static bool TryDouble(string text, out double value) =>
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        || double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value);

    /// <summary>Parses an invariant-culture integer within a range.</summary>
    /// <param name="text">The input text.</param>
    /// <param name="minimum">Inclusive minimum.</param>
    /// <param name="maximum">Inclusive maximum.</param>
    /// <param name="value">The parsed value on success.</param>
    /// <returns>True when parsing succeeded and the value is in range.</returns>
    private static bool TryInt(string text, int minimum, int maximum, out int value) =>
        int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
        && value >= minimum && value <= maximum;

    /// <summary>Populates the hour and minute drop-downs used by every reminder time.</summary>
    private void InitializeReminderTimePickers()
    {
        string[] hours = Enumerable.Range(0, 24).Select(hour => hour.ToString("00", CultureInfo.InvariantCulture)).ToArray();
        string[] minutes = Enumerable.Range(0, 60).Select(minute => minute.ToString("00", CultureInfo.InvariantCulture)).ToArray();
        ComboBox[] hourBoxes =
        [
            SunnahFastingTimeBox, PublicHolidayTimeBox, IslamicHolidayTimeBox, LaranganPuasaTimeBox,
            MorningDzikirTimeBox, EveningDzikirTimeBox
        ];
        ComboBox[] minuteBoxes =
        [
            SunnahFastingMinuteBox, PublicHolidayMinuteBox, IslamicHolidayMinuteBox, LaranganPuasaMinuteBox,
            MorningDzikirMinuteBox, EveningDzikirMinuteBox
        ];
        foreach (ComboBox box in hourBoxes) box.ItemsSource = hours;
        foreach (ComboBox box in minuteBoxes) box.ItemsSource = minutes;
    }

    /// <summary>Selects the two drop-down values represented by an HH.mm setting.</summary>
    /// <param name="hour">Hour drop-down.</param>
    /// <param name="minute">Minute drop-down.</param>
    /// <param name="text">Stored HH.mm value.</param>
    private static void SetTimePicker(ComboBox hour, ComboBox minute, string text)
    {
        if (!AppDateTimeFormat.TryParseTime(text, out TimeSpan time)) time = TimeSpan.Zero;
        hour.SelectedItem = time.Hours.ToString("00", CultureInfo.InvariantCulture);
        minute.SelectedItem = time.Minutes.ToString("00", CultureInfo.InvariantCulture);
    }

    /// <summary>Reads the two drop-down values as HH.mm for draft comparison.</summary>
    /// <param name="hour">Hour drop-down.</param>
    /// <param name="minute">Minute drop-down.</param>
    /// <returns>The combined HH.mm text.</returns>
    private static string TimePickerText(ComboBox hour, ComboBox minute) =>
        $"{(hour.SelectedItem as string) ?? "00"}.{(minute.SelectedItem as string) ?? "00"}";

    /// <summary>Validates one reminder time picker and normalizes it to HH.mm.</summary>
    /// <param name="hour">Hour drop-down.</param>
    /// <param name="minute">Minute drop-down.</param>
    /// <param name="label">Human-readable reminder label for the error message.</param>
    /// <param name="error">Validation error, set when parsing fails.</param>
    /// <param name="value">Normalized HH.mm text.</param>
    /// <returns>True when the picker contains a valid 24-hour time.</returns>
    private static bool TryReminderTime(ComboBox hour, ComboBox minute, string label, ref string error, out string value)
    {
        if (AppDateTimeFormat.TryParseTime(TimePickerText(hour, minute), out TimeSpan time))
        {
            value = AppDateTimeFormat.Time(time);
            return true;
        }
        error = $"Jam pengingat {label} tidak valid.";
        value = string.Empty;
        return false;
    }

    /// <summary>Creates a deep copy of settings via serialization-equivalent field copy.</summary>
    /// <param name="source">Settings to copy.</param>
    /// <returns>The cloned settings.</returns>
    private static AppSettings CloneSettings(AppSettings source)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(source);
        return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json)
            ?? throw new InvalidOperationException("Pengaturan tidak dapat disalin.");
    }

    /// <summary>Copies settings field-by-field from source to destination.</summary>
    /// <param name="source">The settings to copy from.</param>
    /// <param name="destination">The settings to copy into.</param>
    private static void CopySettings(AppSettings source, AppSettings destination)
    {
        destination.Latitude = source.Latitude;
        destination.Longitude = source.Longitude;
        destination.ElevationMeters = source.ElevationMeters;
        destination.TimeZoneId = source.TimeZoneId;
        destination.LocationName = source.LocationName;
        destination.OnlineGeocodingEndpoint = source.OnlineGeocodingEndpoint;
        destination.OnlineGeocodingEnabled = source.OnlineGeocodingEnabled;
        destination.MethodId = source.MethodId;
        destination.CalculationStandard = source.CalculationStandard;
        destination.Corrections = new Dictionary<string, int>(source.Corrections, StringComparer.OrdinalIgnoreCase);
        destination.FullOfflineDataEnabled = source.FullOfflineDataEnabled;
        destination.LastMainTabIndex = source.LastMainTabIndex;
        destination.LastDzikirTabIndex = source.LastDzikirTabIndex;
        destination.SunnahFastingReminderEnabled = source.SunnahFastingReminderEnabled;
        destination.PublicHolidayReminderEnabled = source.PublicHolidayReminderEnabled;
        destination.IslamicHolidayReminderEnabled = source.IslamicHolidayReminderEnabled;
        destination.NoFastReminderEnabled = source.NoFastReminderEnabled;
        destination.MorningDzikirReminderEnabled = source.MorningDzikirReminderEnabled;
        destination.EveningDzikirReminderEnabled = source.EveningDzikirReminderEnabled;
        destination.SunnahFastingReminderTime = source.SunnahFastingReminderTime;
        destination.PublicHolidayReminderTime = source.PublicHolidayReminderTime;
        destination.IslamicHolidayReminderTime = source.IslamicHolidayReminderTime;
        destination.NoFastReminderTime = source.NoFastReminderTime;
        destination.MorningDzikirReminderTime = source.MorningDzikirReminderTime;
        destination.EveningDzikirReminderTime = source.EveningDzikirReminderTime;
        destination.ReminderRepeatSeconds = source.ReminderRepeatSeconds;
        destination.ReminderRepeatCount = source.ReminderRepeatCount;
        destination.RemindersEnabled = source.RemindersEnabled;
        destination.LockScreenEnabled = source.LockScreenEnabled;
        destination.LockScreenOptIn = source.LockScreenOptIn;
        destination.LockScreenDurationMinutes = source.LockScreenDurationMinutes;
        destination.UpdateAutoCheck = source.UpdateAutoCheck;
        destination.IgnoredUpdateVersion = source.IgnoredUpdateVersion;
        destination.AutoLockEnabled = source.AutoLockEnabled;
        destination.StartupEnabled = source.StartupEnabled;
        destination.StartHidden = source.StartHidden;
        destination.WidgetEnabled = source.WidgetEnabled;
        destination.TaskbarWidgetEnabled = source.TaskbarWidgetEnabled;
        destination.TaskbarWidgetMode = source.TaskbarWidgetMode;
        destination.WidgetMode = source.WidgetMode;
        destination.WidgetMotion = source.WidgetMotion;
        destination.WidgetPosition = source.WidgetPosition;
        destination.WidgetAlwaysOnTop = source.WidgetAlwaysOnTop;
        destination.WidgetClickThrough = source.WidgetClickThrough;
        destination.WidgetAutoHideOnHover = source.WidgetAutoHideOnHover;
        destination.ThemeId = source.ThemeId;
        destination.ReminderMinutes = source.ReminderMinutes;
        destination.ReminderMessage = source.ReminderMessage;
        destination.DhuhaAfterSunriseMinutes = source.DhuhaAfterSunriseMinutes;
        destination.HijriDateCorrectionDays = source.HijriDateCorrectionDays;
        destination.CalendarSource = source.CalendarSource;
        destination.KhgtOfferSeen = source.KhgtOfferSeen;
        destination.OtherSettings = source.OtherSettings is null ? null : new(source.OtherSettings);
    }

    /// <summary>Discards the form edits and restores saved settings.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
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

    /// <summary>Restores the form, theme, and widget/taskbar appearance to saved settings.</summary>
    internal void RestoreSavedSettingsPreview()
    {
        PopulateSettingsForm();
        RestoreSavedThemePreview();
        _restoreWidgetAppearancePreview();
        _restoreTaskbarAppearancePreview();
    }

    /// <summary>Restores the theme to the saved theme id.</summary>
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
    /// <summary>Captures the current form control values as a comparison baseline.</summary>
    /// <returns>The captured field values.</returns>
    private object?[] CaptureSettingsFormDraft() =>
    [
        LocationNameBox.Text, LatitudeBox.Text, LongitudeBox.Text, ElevationBox.Text, TimeZoneBox.Text,
        MethodBox.SelectedValue,
        CorrectionFajrBox.Text, CorrectionSunriseBox.Text, CorrectionDhuhaBox.Text,
        CorrectionDhuhrBox.Text, CorrectionAsrBox.Text, CorrectionMaghribBox.Text, CorrectionIshaBox.Text,
        DhuhaAfterSunriseBox.Text, HijriDateCorrectionBox.Text,
        (CalendarSourceBox.SelectedItem as ComboBoxItem)?.Tag,
        DownloadFullDataCheckBox.IsChecked,
        ReminderEnabledCheckBox.IsChecked, ReminderMinutesBox.Text, ReminderMessageBox.Text,
        SunnahFastingReminderCheckBox.IsChecked, TimePickerText(SunnahFastingTimeBox, SunnahFastingMinuteBox),
        PublicHolidayReminderCheckBox.IsChecked, TimePickerText(PublicHolidayTimeBox, PublicHolidayMinuteBox),
        IslamicHolidayReminderCheckBox.IsChecked, TimePickerText(IslamicHolidayTimeBox, IslamicHolidayMinuteBox),
        LaranganPuasaReminderCheckBox.IsChecked, TimePickerText(LaranganPuasaTimeBox, LaranganPuasaMinuteBox),
        MorningDzikirReminderCheckBox.IsChecked, TimePickerText(MorningDzikirTimeBox, MorningDzikirMinuteBox),
        EveningDzikirReminderCheckBox.IsChecked, TimePickerText(EveningDzikirTimeBox, EveningDzikirMinuteBox),
        ReminderRepeatSecondsBox.Text, ReminderRepeatCountBox.Text,
        WidgetEnabledCheckBox.IsChecked, (WidgetModeBox.SelectedItem as ComboBoxItem)?.Tag,
        (WidgetMotionBox.SelectedItem as ComboBoxItem)?.Tag, (WidgetPositionBox.SelectedItem as ComboBoxItem)?.Tag,
        WidgetAlwaysOnTopCheckBox.IsChecked, WidgetClickThroughCheckBox.IsChecked,
        WidgetAutoHideCheckBox.IsChecked, TaskbarWidgetEnabledCheckBox.IsChecked,
        (ThemeBox.SelectedItem as ComboBoxItem)?.Tag,
        StartupEnabledCheckBox.IsChecked, StartHiddenCheckBox.IsChecked, AutoLockEnabledCheckBox.IsChecked,
        LockScreenCheckBox.IsChecked, LockScreenDurationBox.Text
    ];

    /// <summary>Returns true when the form differs from the saved baseline.</summary>
    /// <returns>True when there are unsaved changes.</returns>
    internal bool HasUnsavedSettingsChanges() => _savedFormBaseline is not null &&
        !_savedFormBaseline.SequenceEqual(CaptureSettingsFormDraft());

    /// <summary>Prompts to save/discard/cancel when the form has unsaved changes.</summary>
    /// <param name="discardOnly">Skip the save option and only allow discard/cancel.</param>
    /// <returns>True when it is safe to proceed.</returns>
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
            MainTabs.SelectedIndex = 3;
            return false;
        }
        finally { _resolvingUnsavedChanges = false; }
    }

    /// <summary>Shows the unsaved-changes dialog and maps the result to a choice.</summary>
    /// <param name="discardOnly">Restrict the dialog to discard/cancel.</param>
    /// <returns>The user's choice.</returns>
    private UnsavedChangesChoice ShowUnsavedChangesDialog(bool discardOnly)
    {
        if (!IsVisible) Show();
        Activate();
        var dialog = new UnsavedChangesDialog(discardOnly) { Owner = this };
        dialog.ShowDialog();
        return dialog.Choice;
    }

    /// <summary>Requests application shutdown from the exit button.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void Exit_Click(object sender, RoutedEventArgs e) => ((App)Application.Current).ShutdownApplication();

    /// <summary>Renders the selected Gregorian year as soon as the selector changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void CalendarYearBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingYearSelectors || !_calendarViewsLoaded) return;
        if (CalendarYearBox.SelectedItem is not int year) return;
        _selectedGregorianYear = year;
        if (_selectedGregorianDate.Year != year)
            _selectedGregorianDate = year == GetCalendarToday().Year ? GetCalendarToday() : new DateTime(year, 1, 1);
        RenderGregorianYear(year);
        UpdateYearNavigationButtons();
    }

    /// <summary>Renders the selected Hijri year as soon as the selector changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void HijriYearBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingYearSelectors || !_calendarViewsLoaded) return;
        if (HijriYearBox.SelectedItem is not int year) return;
        _selectedHijriYear = year;
        RenderHijriYear(year);
        UpdateYearNavigationButtons();
    }

    /// <summary>Moves the Gregorian year selector back one year.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void PreviousGregorianYear_Click(object sender, RoutedEventArgs e) => MoveYear(CalendarYearBox, -1);
    /// <summary>Moves the Gregorian year selector forward one year.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void NextGregorianYear_Click(object sender, RoutedEventArgs e) => MoveYear(CalendarYearBox, 1);
    /// <summary>Moves the Hijri year selector back one year.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void PreviousHijriYear_Click(object sender, RoutedEventArgs e) => MoveYear(HijriYearBox, -1);
    /// <summary>Moves the Hijri year selector forward one year.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void NextHijriYear_Click(object sender, RoutedEventArgs e) => MoveYear(HijriYearBox, 1);

    /// <summary>Shifts a year selector by one step; its SelectionChanged handler re-renders the calendar.</summary>
    /// <param name="selector">The year ComboBox to shift.</param>
    /// <param name="delta">Year delta (+1/-1).</param>
    private static void MoveYear(ComboBox selector, int delta)
    {
        if (selector.SelectedIndex < 0) return;
        int nextIndex = selector.SelectedIndex + delta;
        if (nextIndex < 0 || nextIndex >= selector.Items.Count) return;
        selector.SelectedIndex = nextIndex;
    }

    /// <summary>Cancels close to hide to the tray unless allowed; prompts for unsaved changes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Cancel event arguments.</param>
    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _locationRequestVersion++;
        CloseLocationSuggestions();
        if (_allowClose)
        {
            TrySaveSettings();
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
        if (!TrySaveSettings() && !ConfirmUnsavedSettingsChanges()) return;
        RestoreSavedSettingsPreview();
        Hide();
        FooterStatus.Text = "Aplikasi tetap aktif di area notifikasi sistem.";
    }

    /// <summary>Returns true for exceptions that indicate the native core failed to load.</summary>
    /// <param name="ex">The exception to classify.</param>
    /// <returns>True when the exception is a native-core failure.</returns>
    private static bool IsNativeCoreFailure(Exception ex) =>
        ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException;

    /// <summary>Returns true for exceptions that indicate a schedule calculation failure.</summary>
    /// <param name="ex">The exception to classify.</param>
    /// <returns>True when the exception is a schedule failure.</returns>
    private static bool IsScheduleFailure(Exception ex) =>
        IsNativeCoreFailure(ex) || ex is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException;

    /// <summary>Formats a time of day as HH:mm.</summary>
    /// <param name="time">The time to format.</param>
    /// <returns>The formatted time.</returns>
    private static string FormatTime(TimeSpan time) => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

    /// <summary>Creates a SolidColorBrush from a color string.</summary>
    /// <param name="color">The color string, e.g. "#RRGGBB".</param>
    /// <returns>The brush.</returns>
    private static SolidColorBrush MakeBrush(string color) =>
        (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;

}
