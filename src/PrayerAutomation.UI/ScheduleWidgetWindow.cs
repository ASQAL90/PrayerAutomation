using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Point = System.Windows.Point;

namespace PrayerAutomation.UI;

/// <summary>Borderless schedule widget window showing prayer times, countdown, and optional marquee; also hosts the taskbar dashboard.</summary>
internal sealed class ScheduleWidgetWindow : Window
{
    private const double MarqueePixelsPerTick = 1.25;
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    private readonly AppSettings _settings;
    private readonly bool _taskbarDashboard;
    private bool _allowClose;
    private bool _dashboardPlaced;
    private string? _taskbarIconKey;
    private System.Windows.Controls.Image _nextPrayerIcon = null!;
    private (string Mode, string Motion, string Position)? _appearancePreview;
    private readonly Dictionary<DateTime, IReadOnlyList<PrayerEntry>> _scheduleCache = new();
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _marqueeTimer;
    private readonly DispatcherTimer _marqueeStartTimer;
    private Border _frame = null!;
    private System.Windows.Controls.Panel _body = null!;
    private System.Windows.Controls.Panel _schedule = null!;
    private Grid _marqueeHost = null!;
    private TextBlock _marqueeLine = null!;
    private TranslateTransform _marqueeTranslation = null!;
    private TextBlock _clock = null!;
    private TextBlock _date = null!;
    private TextBlock _location = null!;
    private TextBlock _context = null!;
    private TextBlock _nextPrayer = null!;
    private TextBlock _countdown = null!;
    private DateTime _lastScheduleDate = DateTime.MinValue;
    private string _marqueeContent = string.Empty;
    private string _marqueeConfiguredContent = string.Empty;
    private double _marqueeOffset;
    private double _marqueeCycleWidth;
    private long _marqueeCompletedCycles;
    private double _marqueeViewportWidth = double.NaN;
    private bool _marqueeAnimationReady;
    private string? _scheduleError;

    /// <summary>Initializes the desktop schedule widget.</summary>
    /// <param name="settings">Application settings.</param>
    public ScheduleWidgetWindow(AppSettings settings)
        : this(settings, taskbarDashboard: false)
    {
    }

    /// <summary>Initializes the widget as either the desktop widget or the taskbar dashboard detail window.</summary>
    /// <param name="settings">Application settings.</param>
    /// <param name="taskbarDashboard">True to act as the taskbar dashboard (hidden instead of closed).</param>
    internal ScheduleWidgetWindow(AppSettings settings, bool taskbarDashboard)
    {
        _settings = settings;
        _settings.Normalize();
        _taskbarDashboard = taskbarDashboard;
        Title = AppBranding.DisplayName + " — Jadwal";
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = !taskbarDashboard;
        if (taskbarDashboard)
        {
            ShowActivated = false;
            WindowState = WindowState.Normal;
        }
        WindowStartupLocation = WindowStartupLocation.Manual;
        Closing += (_, e) =>
        {
            if (_taskbarDashboard && !_allowClose)
            {
                e.Cancel = true;
                Hide();
            }
        };

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => RefreshClockAndCountdown();
        _marqueeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        _marqueeTimer.Tick += (_, _) => AdvanceMarquee();
        _marqueeStartTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _marqueeStartTimer.Tick += (_, _) =>
        {
            _marqueeStartTimer.Stop();
            _marqueeAnimationReady = true;
            ConfigureMarquee(force: true);
        };
        Closed += (_, _) =>
        {
            _clockTimer.Stop();
            _marqueeTimer.Stop();
            _marqueeStartTimer.Stop();
        };

        RefreshSchedule();
        _clockTimer.Start();
    }

    /// <summary>Effective display mode, honoring an active preview and the taskbar dashboard flag.</summary>
    private string Mode => _taskbarDashboard ? "detail" : WidgetSettingsLogic.NormalizeMode(_appearancePreview?.Mode ?? _settings.WidgetMode);

    /// <summary>Allows closing (even for the taskbar dashboard) and closes the window.</summary>
    internal void ClosePermanently()
    {
        _allowClose = true;
        Close();
    }
    /// <summary>Effective motion, honoring an active preview over saved settings.</summary>
    private string Motion => _appearancePreview?.Motion ?? _settings.WidgetMotion;
    /// <summary>Effective position, honoring an active preview over saved settings.</summary>
    private string Position => _appearancePreview?.Position ?? _settings.WidgetPosition;
    /// <summary>True when the effective mode is "essential".</summary>
    private bool IsEssentialMode => Mode == "essential";
    /// <summary>True when essential mode uses the marquee motion.</summary>
    private bool IsMarqueeMode => IsEssentialMode && string.Equals(Motion, "marquee", StringComparison.OrdinalIgnoreCase);
    /// <summary>True when the effective mode is "minimalist".</summary>
    private bool IsMinimalistMode => Mode == "minimalist";
    /// <summary>True when the effective mode is "detail".</summary>
    private bool IsDetailMode => Mode == "detail";

    /// <summary>Applies a transient appearance preview without saving settings.</summary>
    /// <param name="mode">Mode to preview.</param>
    /// <param name="motion">Motion to preview.</param>
    /// <param name="position">Position to preview.</param>
    internal void ApplyAppearancePreview(string mode, string motion, string position)
    {
        _appearancePreview = (WidgetSettingsLogic.NormalizeMode(mode),
            WidgetSettingsLogic.NormalizeMotion(motion, legacyMinimalMode: false),
            WidgetSettingsLogic.NormalizePosition(position));
        RefreshSchedule();
    }

    /// <summary>Clears any active appearance preview and refreshes the schedule.</summary>
    internal void ClearAppearancePreview()
    {
        if (_appearancePreview is null) return;
        _appearancePreview = null;
        RefreshSchedule();
    }

    /// <summary>Rebuilds the layout and re-renders the schedule from settings.</summary>
    public void RefreshSchedule()
    {
        DateTime now = GetLocationNow();
        _lastScheduleDate = now.Date;
        _scheduleCache.Clear();
        if (_taskbarDashboard) _taskbarIconKey = null;
        _scheduleError = null;
        _marqueeTimer.Stop();
        _marqueeStartTimer.Stop();
        _marqueeContent = string.Empty;
        _marqueeConfiguredContent = string.Empty;
        _marqueeOffset = 0;
        _marqueeCycleWidth = 0;
        _marqueeCompletedCycles = 0;
        _marqueeViewportWidth = double.NaN;
        _marqueeAnimationReady = false;

        BuildLayout();
        RefreshClockAndDate(now);

        try
        {
            IReadOnlyList<PrayerEntry> entries = GetScheduleForDate(now.Date);
            if (IsEssentialMode)
                RenderEssentialSchedule(entries);
            else if (IsMinimalistMode)
                RenderMinimalistSchedule();
            else
                RenderDetailSchedule(entries);

            if (!IsEssentialMode)
                UpdateCountdown(now);
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            RecordScheduleFailure("widget.schedule", ex);
            ShowScheduleError();
        }
    }

    /// <summary>Builds the visual tree for the current mode, including marquee, clock, and schedule areas.</summary>
    private void BuildLayout()
    {
        ThemePalette palette = ThemeManager.Current;
        string mode = Mode;
        double requestedWidth = mode switch
        {
            "essential" => 700,
            "minimalist" => 470,
            _ => 440
        };
        double requestedHeight = mode switch
        {
            "essential" => 34,
            "minimalist" => 34,
            _ => 300
        };

        if (_taskbarDashboard)
        {
            requestedWidth = _dashboardPlaced ? Width : 440;
            requestedHeight = _dashboardPlaced ? Height : 300;
        }
        // Clear Detail constraints before assigning the compact window size.
        MinWidth = 0;
        MinHeight = 0;
        if (!IsDetailMode && WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        ResizeMode = IsDetailMode ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
        Width = requestedWidth;
        Height = requestedHeight;
        Background = System.Windows.Media.Brushes.Transparent;

        _frame = new Border
        {
            Background = ThemeManager.ToBrush(palette.WidgetBackground),
            BorderBrush = ThemeManager.ToBrush(palette.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(Math.Max(0, palette.CornerRadius)),
            Padding = IsEssentialMode || IsMinimalistMode ? new Thickness(9, 2, 9, 2) : new Thickness(8, 8, 8, 2),
            SnapsToDevicePixels = true
        };

        var host = new Grid();
        host.ColumnDefinitions.Add(new ColumnDefinition());
        host.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _body = IsEssentialMode || IsMinimalistMode
            ? new Grid { VerticalAlignment = VerticalAlignment.Center }
            : new StackPanel
            {
                Orientation = IsDetailMode ? System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
        AutomationProperties.SetAutomationId(_body, $"ScheduleWidgetContent_{mode}");
        host.Children.Add(_body);

        var hideButton = new Button
        {
            Content = "×",
            ToolTip = _taskbarDashboard ? "Sembunyikan dashboard; panel taskbar tetap berjalan" : "Sembunyikan widget",
            MinWidth = 28,
            MinHeight = 28,
            Padding = new Thickness(0),
            Margin = new Thickness(IsDetailMode ? 2 : 6, 0, 0, 0),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = ThemeManager.ToBrush(palette.WidgetMuted),
            BorderThickness = new Thickness(0),
            VerticalAlignment = IsDetailMode ? VerticalAlignment.Top : VerticalAlignment.Center
        };
        AutomationProperties.SetName(hideButton, "Sembunyikan widget jadwal");
        hideButton.Click += (_, _) => Hide();
        Grid.SetColumn(hideButton, 1);
        host.Children.Add(hideButton);

        _schedule = IsEssentialMode
            ? new Grid { VerticalAlignment = VerticalAlignment.Center }
            : new StackPanel
            {
                Orientation = IsDetailMode ? System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
        AutomationProperties.SetAutomationId(_schedule, "ScheduleWidgetSchedule");
        _marqueeHost = new Grid { ClipToBounds = true, Height = 26, VerticalAlignment = VerticalAlignment.Center };
        AutomationProperties.SetAutomationId(_marqueeHost, "EssentialMarqueeViewport");
        _marqueeLine = new TextBlock
        {
            FontSize = Width < 520 ? 11 : 12,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.None,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Foreground = ThemeManager.ToBrush(palette.WidgetText)
        };
        _marqueeTranslation = new TranslateTransform();
        _marqueeLine.RenderTransform = _marqueeTranslation;
        AutomationProperties.SetAutomationId(_marqueeLine, "EssentialMarqueeText");
        // Measure the complete line before applying the viewport clip.
        // A direct Grid child receives a layout clip that moves with the text.
        var marqueeTrack = new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        marqueeTrack.Children.Add(_marqueeLine);
        _marqueeHost.Children.Add(marqueeTrack);
        _marqueeHost.SizeChanged += (_, _) => ConfigureMarquee(force: true);

        _clock = new TextBlock
        {
            FontSize = 25,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeManager.ToBrush(palette.WidgetText),
            VerticalAlignment = VerticalAlignment.Center
        };
        _date = new TextBlock
        {
            FontSize = 11,
            Foreground = ThemeManager.ToBrush(palette.WidgetMuted),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        _location = new TextBlock
        {
            FontSize = 11,
            Foreground = ThemeManager.ToBrush(palette.WidgetMuted),
            Margin = new Thickness(0, 3, 0, 0)
        };
        _context = new TextBlock
        {
            FontSize = 10,
            Foreground = ThemeManager.ToBrush(palette.WidgetMuted),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 1, 0, 5)
        };
        _nextPrayer = new TextBlock
        {
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeManager.ToBrush(palette.WidgetText),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap
        };
        _countdown = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeManager.ToBrush(palette.WidgetAccent),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = IsDetailMode ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Margin = new Thickness(IsDetailMode ? 0 : 14, IsDetailMode ? 4 : 0, 0, 0)
        };

        if (IsEssentialMode)
        {
            _body.Children.Add(_schedule);
        }
        else if (IsMinimalistMode)
        {
            _nextPrayerIcon = new System.Windows.Controls.Image
            {
                Width = 18, Height = 18, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center
            };
            var minimalistGrid = (Grid)_body;
            minimalistGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            minimalistGrid.ColumnDefinitions.Add(new ColumnDefinition());
            minimalistGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(_nextPrayerIcon, 0);
            Grid.SetColumn(_nextPrayer, 1);
            Grid.SetColumn(_countdown, 2);
            AutomationProperties.SetAutomationId(_nextPrayerIcon, "MinimalistNextPrayerIcon");
            _body.Children.Add(_nextPrayerIcon);
            _countdown.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            AutomationProperties.SetAutomationId(_nextPrayer, "MinimalistNextPrayer");
            AutomationProperties.SetAutomationId(_countdown, "MinimalistCountdown");
            _body.Children.Add(_nextPrayer);
            _body.Children.Add(_countdown);
        }
        else
        {
            var clockAndDate = new Grid { Margin = new Thickness(0, 0, 0, 2) };
            clockAndDate.ColumnDefinitions.Add(new ColumnDefinition());
            clockAndDate.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            clockAndDate.Children.Add(_clock);
            Grid.SetColumn(_date, 1);
            clockAndDate.Children.Add(_date);
            _body.Children.Add(clockAndDate);
            _location.Text = _settings.LocationName;
            _body.Children.Add(_location);
            _context.Text = $"{_settings.TimeZoneId} · {_settings.CalculationStandard}";
            _body.Children.Add(_context);
            _body.Children.Add(_countdown);
            _body.Children.Add(new TextBlock
            {
                Text = "JADWAL HARI INI",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ThemeManager.ToBrush(palette.WidgetMuted),
                Margin = new Thickness(0, 9, 0, 4)
            });
            _body.Children.Add(_schedule);
        }

        _frame.Child = host;
        Content = _taskbarDashboard
            ? new ScrollViewer { Content = _frame, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }
            : _frame;
        _frame.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed && e.OriginalSource is not Button)
            {
                DragMove();
                e.Handled = true;
            }
        };
        ApplyPlacement(requestedWidth, requestedHeight);
    }

    /// <summary>Positions the window on the work area according to mode, dashboard flag, and configured position.</summary>
    /// <param name="requestedWidth">Requested width before placement adjustment.</param>
    /// <param name="requestedHeight">Requested height before placement adjustment.</param>
    private void ApplyPlacement(double requestedWidth, double requestedHeight)
    {
        Rect workArea = SystemParameters.WorkArea;
        if (_taskbarDashboard)
        {
            Width = Math.Min(requestedWidth, workArea.Width);
            Height = Math.Min(requestedHeight, workArea.Height);
            if (!_dashboardPlaced)
            {
                Left = workArea.Left + (workArea.Width - Width) / 2;
                Top = workArea.Top + (workArea.Height - Height) / 2;
                _dashboardPlaced = true;
            }
            MinWidth = Math.Min(360, Width);
            MinHeight = Math.Min(300, Height);
            return;
        }
        WidgetPlacement placement = WidgetSettingsLogic.PlaceWidget(
            Mode,
            Position,
            workArea.Left,
            workArea.Top,
            workArea.Width,
            workArea.Height,
            requestedWidth,
            requestedHeight,
            margin: 4);
        Width = placement.Width;
        Height = placement.Height;
        Left = placement.Left;
        Top = placement.Top;
        if (IsDetailMode)
        {
            MinWidth = Math.Min(360, placement.Width);
            MinHeight = Math.Min(300, placement.Height);
        }
    }

    /// <summary>Renders the essential (five obligatory prayers) mode, or configures the marquee.</summary>
    /// <param name="entries">Prayer times for today.</param>
    /// <exception cref="InvalidOperationException">The five obligatory prayers are not available.</exception>
    private void RenderEssentialSchedule(IReadOnlyList<PrayerEntry> entries)
    {
        IReadOnlyList<PrayerEntry> essential = PrayerUiLogic.EssentialPrayerTimes(entries);
        if (essential.Count == 0)
            throw new InvalidOperationException("Jadwal lima waktu wajib tidak tersedia.");

        _schedule.Children.Clear();
        if (IsMarqueeMode)
        {
            UpdateMarqueeContent(GetLocationNow());
            _schedule.Children.Add(_marqueeHost);
            ConfigureMarquee(force: true);
            _marqueeStartTimer.Start();
            return;
        }

        if (_schedule is Grid essentialGrid)
        {
            essentialGrid.ColumnDefinitions.Clear();
            for (int index = 0; index < essential.Count; index++)
            {
                essentialGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                if (index < essential.Count - 1)
                    essentialGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
        }

        for (int index = 0; index < essential.Count; index++)
        {
            PrayerEntry entry = essential[index];
            string time = entry.Time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            var prayerText = new TextBlock
            {
                Text = $"{entry.Label}  {time}",
                FontSize = Width < 540 ? 10 : 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.ToBrush(ThemeManager.Current.WidgetText),
                TextWrapping = TextWrapping.NoWrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 3, 0)
            };
            AutomationProperties.SetAutomationId(prayerText, $"EssentialPrayer_{entry.Key}");
            AutomationProperties.SetName(prayerText, $"{entry.Label} {time}");
            if (_schedule is Grid)
                Grid.SetColumn(prayerText, index * 2);
            _schedule.Children.Add(prayerText);
            if (index < essential.Count - 1)
            {
                var separator = new Border
                {
                    Width = 1,
                    Height = 22,
                    Background = ThemeManager.ToBrush(ThemeManager.Current.Border),
                    Margin = new Thickness(5, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                if (_schedule is Grid)
                    Grid.SetColumn(separator, index * 2 + 1);
                _schedule.Children.Add(separator);
            }
        }
    }

    /// <summary>Renders the minimalist placeholder texts; the countdown is updated separately.</summary>
    private void RenderMinimalistSchedule()
    {
        _nextPrayer.Text = "Sholat berikutnya";
        _countdown.Text = "Menghitung…";
    }

    /// <summary>Renders the detail list of prayer times with icons.</summary>
    /// <param name="entries">Prayer times to render.</param>
    private void RenderDetailSchedule(IReadOnlyList<PrayerEntry> entries)
    {
        _schedule.Children.Clear();
        foreach (PrayerEntry entry in entries.OrderBy(item => item.Time))
        {
            string prayerColor = ThemeCatalog.PrayerColor(ThemeManager.Current, entry.Key);
            var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new Border
            {
                Background = ThemeManager.ToBrush(prayerColor),
                CornerRadius = new CornerRadius(1),
                Margin = new Thickness(0, 2, 7, 2)
            });
            var label = new TextBlock
            {
                Text = entry.Label,
                FontSize = 12,
                Foreground = ThemeManager.ToBrush(ThemeManager.Current.WidgetText),
                VerticalAlignment = VerticalAlignment.Center
            };
            var icon = new System.Windows.Controls.Image
            {
                Source = PrayerWidgetIcons.Create(entry.Key, ThemeManager.ToBrush(ThemeManager.Current.WidgetText)),
                Width = 18, Height = 18, Margin = new Thickness(6, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center
            };
            AutomationProperties.SetAutomationId(icon, $"DetailPrayerIcon_{entry.Key}");
            AutomationProperties.SetName(icon, PrayerUiLogic.PresentationFor(entry.Key, entry.Label).AccessibleDescription);
            Grid.SetColumn(icon, 1);
            row.Children.Add(icon);
            Grid.SetColumn(label, 2);
            row.Children.Add(label);
            var time = new TextBlock
            {
                Text = entry.Time.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.ToBrush(ThemeManager.Current.WidgetAccent),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(time, 3);
            row.Children.Add(time);
            _schedule.Children.Add(row);
        }
    }

    /// <summary>Builds and applies the marquee text for the current time.</summary>
    /// <param name="now">Location-local current time.</param>
    private void UpdateMarqueeContent(DateTime now)
    {
        if (_scheduleError is not null) return;
        DateTime calendarDate = now.Date;
        NativeCalendarDate hijri = NativePrayerCore.HijriDate(calendarDate.AddDays(_settings.HijriDateCorrectionDays));
        _marqueeContent = WidgetMarqueeContent.Build(now, _settings, hijri, GetScheduleForDate(now.Date),
            PrayerUiLogic.NextPrayerAfter(now, GetScheduleForDate),
            NativePrayerCore.CalendarObservances(calendarDate, _settings.HijriDateCorrectionDays),
            WidgetMarqueeContent.ReligiousMessageAt(_marqueeCompletedCycles));
        AutomationProperties.SetName(_marqueeLine, _marqueeContent);
        ConfigureMarquee();
    }

    /// <summary>Advances the marquee offset; wraps and rebuilds content after a full cycle.</summary>
    private void AdvanceMarquee()
    {
        if (!ConfigureMarquee()) return;
        if (_marqueeCycleWidth <= 0) return;

        _marqueeOffset += MarqueePixelsPerTick;
        if (_marqueeOffset >= _marqueeCycleWidth)
        {
            _marqueeOffset -= _marqueeCycleWidth;
            _marqueeCompletedCycles++;
            UpdateMarqueeContent(GetLocationNow());
        }
        _marqueeTranslation.X = -_marqueeOffset;
    }

    /// <summary>Measures and configures the marquee text repeat/clip based on the viewport.</summary>
    /// <param name="force">Reconfigure even when content and viewport are unchanged.</param>
    /// <returns>True when the marquee is ready to animate.</returns>
    private bool ConfigureMarquee(bool force = false)
    {
        if (!IsMarqueeMode ||
            string.IsNullOrEmpty(_marqueeContent))
            return false;

        double viewportWidth = _marqueeHost.ActualWidth;
        if (viewportWidth <= 0)
            return false;

        if (!force &&
            string.Equals(_marqueeConfiguredContent, _marqueeContent, StringComparison.Ordinal) &&
            Math.Abs(_marqueeViewportWidth - viewportWidth) < 0.5)
            return true;

        _marqueeConfiguredContent = _marqueeContent;
        _marqueeViewportWidth = viewportWidth;
        string cycle = _marqueeContent + "   •   ";
        _marqueeCycleWidth = MeasureMarqueeWidth(cycle);

        int repeats = Math.Max(2, (int)Math.Ceiling(viewportWidth / _marqueeCycleWidth) + 1);
        string repeated = string.Concat(Enumerable.Repeat(cycle, repeats));
        _marqueeLine.Text = repeated;
        _marqueeLine.Width = MeasureMarqueeWidth(repeated);
        _marqueeLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        _marqueeOffset %= _marqueeCycleWidth;
        _marqueeTranslation.X = -_marqueeOffset;
        if (_marqueeAnimationReady)
            _marqueeTimer.Start();
        return true;
    }

    /// <summary>Measures the rendered width of a marquee line.</summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The width in device-independent pixels.</returns>
    private double MeasureMarqueeWidth(string text)
    {
        var typeface = new Typeface(_marqueeLine.FontFamily, _marqueeLine.FontStyle, _marqueeLine.FontWeight, _marqueeLine.FontStretch);
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            _marqueeLine.FlowDirection,
            typeface,
            _marqueeLine.FontSize,
            _marqueeLine.Foreground,
            VisualTreeHelper.GetDpi(_marqueeLine).PixelsPerDip);
        return Math.Ceiling(formatted.WidthIncludingTrailingWhitespace);
    }

    /// <summary>Per-second tick: refreshes clock, date, countdown, and marquee as applicable.</summary>
    private void RefreshClockAndCountdown()
    {
        DateTime now = GetLocationNow();
        if (now.Date != _lastScheduleDate)
        {
            RefreshSchedule();
            return;
        }

        RefreshClockAndDate(now);
        if (IsMarqueeMode)
            UpdateMarqueeContent(now);
        else if (!IsEssentialMode)
            UpdateCountdown(now);
    }

    /// <summary>Updates the clock and date texts in detail mode.</summary>
    /// <param name="now">Location-local current time.</param>
    private void RefreshClockAndDate(DateTime now)
    {
        if (IsDetailMode)
        {
            _clock.Text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            _date.Text = now.ToString("dddd, d MMM yyyy", IndonesianCulture);
        }
    }

    /// <summary>Updates next-prayer and countdown texts (and icon) for detail/minimalist modes.</summary>
    /// <param name="now">Location-local current time.</param>
    private void UpdateCountdown(DateTime now)
    {
        if (_scheduleError is not null)
        {
            SetCountdownError();
            return;
        }

        try
        {
            PrayerCountdown? next = PrayerUiLogic.NextPrayerAfter(now, GetScheduleForDate);
            if (next is not PrayerCountdown countdown)
            {
                _nextPrayer.Text = "Sholat berikutnya belum tersedia.";
                _countdown.Text = "--:--:--";
                return;
            }

            string time = countdown.LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            string remaining = PrayerUiLogic.FormatCountdown(countdown.Remaining);
            string key = GetScheduleForDate(countdown.LocalTime.Date)
                .Where(entry => entry.Label == countdown.PrayerLabel).Select(entry => entry.Key).FirstOrDefault() ?? "Fajr";
            if (_taskbarDashboard)
            {
                if (_taskbarIconKey != key)
                {
                    PrayerPresentation presentation = PrayerUiLogic.PresentationFor(key, countdown.PrayerLabel);
                    Icon = PrayerWidgetIcons.Create(key, ThemeManager.ToBrush(presentation.Foreground), taskbar: true);
                    _taskbarIconKey = key;
                }
            }
            else if (IsMinimalistMode)
            {
                _nextPrayerIcon.Source = PrayerWidgetIcons.Create(key, ThemeManager.ToBrush(ThemeManager.Current.WidgetText));
                AutomationProperties.SetName(_nextPrayerIcon, PrayerUiLogic.PresentationFor(key, countdown.PrayerLabel).AccessibleDescription);
            }
            if (IsMinimalistMode)
            {
                _nextPrayer.Text = $"Berikutnya: {countdown.PrayerLabel} · {time}";
                _countdown.Text = remaining;
            }
            else
            {
                _countdown.Text = $"Berikutnya: {countdown.PrayerLabel} pukul {time} · {remaining}";
            }
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            RecordScheduleFailure("widget.countdown", ex);
            SetCountdownError();
        }
    }

    /// <summary>Shows the countdown error message.</summary>
    private void SetCountdownError()
    {
        if (IsMinimalistMode)
            _nextPrayer.Text = "Jadwal sholat belum tersedia";
        _countdown.Text = "Countdown belum tersedia. Periksa lokasi dan native core.";
        _countdown.Foreground = ThemeManager.ToBrush(ThemeManager.Current.ErrorText);
    }

    /// <summary>Shows the schedule error message in the current mode.</summary>
    private void ShowScheduleError()
    {
        var error = new TextBlock
        {
            Text = "Jadwal sholat belum dapat dihitung. Periksa lokasi, timezone, dan native core.",
            FontSize = 12,
            Foreground = ThemeManager.ToBrush(ThemeManager.Current.ErrorText),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 4)
        };
        if (IsEssentialMode)
        {
            _schedule.Children.Clear();
            _schedule.Children.Add(error);
        }
        else if (IsMinimalistMode)
        {
            _nextPrayer.Text = "Jadwal sholat belum tersedia";
            SetCountdownError();
        }
        else
        {
            _schedule.Children.Clear();
            _schedule.Children.Add(error);
            SetCountdownError();
        }
    }

    /// <summary>Returns the prayer schedule for a date, cached per day.</summary>
    /// <param name="date">The date to get the schedule for.</param>
    /// <returns>The prayer entries for the date.</returns>
    private IReadOnlyList<PrayerEntry> GetScheduleForDate(DateTime date)
    {
        DateTime key = date.Date;
        if (!_scheduleCache.TryGetValue(key, out IReadOnlyList<PrayerEntry>? entries) || entries is null)
        {
            entries = PrayerScheduleService.ForDate(key, _settings);
            _scheduleCache[key] = entries;
        }
        return entries;
    }

    /// <summary>Returns the current time in the configured time zone; falls back to local time on failure.</summary>
    /// <returns>The location-local current time.</returns>
    private DateTime GetLocationNow()
    {
        try
        {
            TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            return DateTime.Now;
        }
    }

    /// <summary>Records the first schedule failure of this render and logs it.</summary>
    /// <param name="category">Log category tag.</param>
    /// <param name="exception">The failure.</param>
    private void RecordScheduleFailure(string category, Exception exception)
    {
        if (_scheduleError is not null) return;
        _scheduleError = exception.GetType().Name;
        AppLogger.Write(category, exception);
    }

    /// <summary>Returns true for exceptions that indicate a schedule failure.</summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>True when the exception is a schedule failure.</returns>
    private static bool IsScheduleFailure(Exception exception) =>
        exception is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or
            ArgumentException or DllNotFoundException or EntryPointNotFoundException or
            BadImageFormatException or TypeInitializationException;
}

/// <summary>Creates frozen drawing images for prayer icons used by the widget.</summary>
internal static class PrayerWidgetIcons
{
    /// <summary>Creates a frozen drawing image for a prayer key.</summary>
    /// <param name="key">Prayer key used to resolve icon paths and colors.</param>
    /// <param name="foreground">Brush used to draw the glyph.</param>
    /// <param name="taskbar">Draw a background ellipse for the taskbar renderer.</param>
    /// <returns>The frozen drawing image.</returns>
    internal static DrawingImage Create(string key, System.Windows.Media.Brush foreground, bool taskbar = false)
    {
        PrayerPresentation presentation = PrayerUiLogic.PresentationFor(key);
        var drawing = new DrawingGroup();
        using (DrawingContext context = drawing.Open())
        {
            context.DrawRectangle(System.Windows.Media.Brushes.Transparent, null, new Rect(0, 0, 24, 24));
            if (taskbar)
                context.DrawEllipse(ThemeManager.ToBrush(presentation.Background), null, new Point(12, 12), 11.5, 11.5);
            foreach (string data in PrayerAutomation.Shared.PrayerIconPaths.ForKey(key))
                context.DrawGeometry(foreground, null, Geometry.Parse(data));
        }
        drawing.Freeze();
        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }
}

