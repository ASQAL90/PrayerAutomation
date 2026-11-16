using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace PrayerAutomation.UI;

public partial class App : Application
{
    static App()
    {
        // The legacy selection adorner covers glyphs at SelectionOpacity=1.
        // Use TextBox background rendering so SelectionTextBrush is respected.
        AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
    }

    private TrayService? _tray;
    private ReminderScheduler? _reminders;
    private AutoLockScheduler? _autoLock;
    private LockScreenImageScheduler? _lockScreenScheduler;
    private ScheduleWidgetWindow? _widget;
    private ScheduleWidgetWindow? _taskbarWidget;
    private TaskbarCountdownPanel? _taskbarPanel;
    private (bool Enabled, string Mode)? _taskbarAppearancePreview;
    private bool EffectiveTaskbarWidgetEnabled => _taskbarAppearancePreview?.Enabled ?? Settings.TaskbarWidgetEnabled;
    private string EffectiveTaskbarWidgetMode => _taskbarAppearancePreview?.Mode ?? Settings.TaskbarWidgetMode;
    internal string? TaskbarWidgetRuntimeStatus => _taskbarPanel?.CurrentStatus;
    private MainWindow? _mainWindow;
    private Mutex? _singleInstanceMutex;
    private bool _widgetSettingEnabled;
    private bool _widgetAppearancePreviewActive;
    private bool _widgetVisibleBeforePreview;
    private bool _widgetExistedBeforePreview;
    private bool _isShuttingDown;

    internal AppSettings Settings { get; private set; } = new();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            _singleInstanceMutex = new Mutex(false, @"Global\PrayerAutomation.Desktop.SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
                Shutdown();
                return;
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            AppLogger.Write("app.single_instance", ex);
            Shutdown();
            return;
        }
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Settings = SettingsStore.Load();
        ThemeManager.Apply(this, Settings.ThemeId);
        _widgetSettingEnabled = Settings.WidgetEnabled;
        _tray = new TrayService(this);
        _mainWindow = new MainWindow(Settings);
        MainWindow = _mainWindow;
        if (!StartupRegistration.ShouldStartHidden(e.Args, Settings.StartHidden))
            _mainWindow.Show();
        _reminders = new ReminderScheduler(Settings);
        _reminders.Start();
        _autoLock = new AutoLockScheduler(Settings);
        _autoLock.Start();
        _lockScreenScheduler = new LockScreenImageScheduler(
            () => Settings.LockScreenEnabled && Settings.LockScreenOptIn,
            date => PrayerScheduleService.ReminderEntries(PrayerScheduleService.ForDate(date, Settings))
                .Select(entry => new LockScreenPrayerTrigger(entry.Key, entry.Time)).ToArray(),
            () => PrayerScheduleService.ResolveTimeZone(Settings.TimeZoneId),
            Path.Combine(AppContext.BaseDirectory, "sedang_sholat.png"),
            new LockScreenImageTransaction(
                new WindowsLockScreenImageBackend(AppLogger.Write, trace: AppLogger.WriteInfo),
                new FileLockScreenRestoreStore()),
            AppLogger.Write,
            () => Settings.LockScreenDurationMinutes,
            new WindowsLockScreenExecutionRequest(AppLogger.WriteInfo),
            AppLogger.WriteInfo);
        _lockScreenScheduler.StatusChanged += message => _mainWindow?.SetLockScreenStatus(message);
        try
        {
            await _lockScreenScheduler.StartAsync(DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            AppLogger.Write("lock-screen.startup", ex);
            _mainWindow?.SetLockScreenStatus("Pemulihan lock screen belum terverifikasi; tidak ada perubahan baru yang dijalankan.");
        }
        CompleteWidgetStartup();
    }

    private void CompleteWidgetStartup()
    {
        if (_isShuttingDown) return;
        SynchronizeTaskbarWidget();
        if (_widgetAppearancePreviewActive)
        {
            // Startup establishes the saved-enabled baseline, not the active draft.
            if (Settings.WidgetEnabled && !_widgetExistedBeforePreview)
            {
                EnsureWidget();
                _widgetExistedBeforePreview = true;
                _widgetVisibleBeforePreview = true;
            }
            return;
        }
        if (Settings.WidgetEnabled && _widget is null) ShowWidget();
    }

    internal void ShowMainWindow()
    {
        if (_mainWindow is null) return;
        if (!_mainWindow.IsVisible)
        {
            _mainWindow.RestoreSavedSettingsPreview();
            _mainWindow.Show();
        }
        if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    internal void OpenCalendar()
    {
        ShowMainWindow();
        _mainWindow?.SelectCalendarTab();
    }

    internal void OpenSettings()
    {
        ShowMainWindow();
        _mainWindow?.SelectSettingsTab();
    }

    internal void ToggleWidget()
    {
        if (_widget is not null && _widget.IsVisible)
        {
            _widget.Hide();
            return;
        }

        ShowWidget();
    }

    private ScheduleWidgetWindow EnsureWidget()
    {
        if (_widget is null)
        {
            _widget = new ScheduleWidgetWindow(Settings);
            _widget.Closed += (_, _) => _widget = null;
        }
        return _widget;
    }

    private void ShowWidget()
    {
        ScheduleWidgetWindow widget = EnsureWidget();
        if (!widget.IsVisible) widget.Show();
        widget.Activate();
    }

    internal void ApplyWidgetAppearancePreview(bool enabled, string mode, string motion, string position)
    {
        if (!_widgetAppearancePreviewActive)
        {
            _widgetAppearancePreviewActive = true;
            _widgetVisibleBeforePreview = _widget?.IsVisible == true;
            _widgetExistedBeforePreview = _widget is not null;
        }

        if (!enabled)
        {
            _widget?.Hide();
            return;
        }

        ScheduleWidgetWindow widget = EnsureWidget();
        widget.ApplyAppearancePreview(mode, motion, position);
        if (!widget.IsVisible)
        {
            bool showActivated = widget.ShowActivated;
            widget.ShowActivated = false;
            try { widget.Show(); }
            finally { widget.ShowActivated = showActivated; }
        }
    }

    internal void RestoreWidgetAppearancePreview()
    {
        if (!_widgetAppearancePreviewActive) return;
        _widgetAppearancePreviewActive = false;
        if (!_widgetExistedBeforePreview)
        {
            _widget?.Close();
            return;
        }
        _widget?.ClearAppearancePreview();
        if (_widgetVisibleBeforePreview)
            _widget?.Show();
        else
            _widget?.Hide();
    }

    internal void ApplyThemePreview(string themeId)
    {
        ThemeManager.Apply(this, themeId);
        _widget?.RefreshSchedule();
        _taskbarWidget?.RefreshSchedule();
        _mainWindow?.RefreshThemeDependentViews();
        _taskbarPanel?.RefreshSchedule();
    }

    internal void RefreshDependentViews()
    {
        _taskbarAppearancePreview = null;
        _widgetAppearancePreviewActive = false;
        _widget?.ClearAppearancePreview();
        ThemeManager.Apply(this, Settings.ThemeId);
        _reminders?.Refresh();
        _autoLock?.Refresh();
        _ = RefreshLockScreenSchedulerAsync();
        WidgetVisibilityAction action = WidgetLifecycleLogic.ForSettingsChange(_widgetSettingEnabled, Settings.WidgetEnabled);
        if (action == WidgetVisibilityAction.Show)
            ShowWidget();
        else if (action == WidgetVisibilityAction.Hide)
            _widget?.Hide();
        _widgetSettingEnabled = Settings.WidgetEnabled;
        _widget?.RefreshSchedule();
        SynchronizeTaskbarWidget();
        _mainWindow?.RefreshOverview();
    }

    internal void ApplyTaskbarAppearancePreview(bool enabled, string mode)
    {
        if (_isShuttingDown || mode is not ("popup" or "native")) return;
        _taskbarAppearancePreview = (enabled, mode);
        SynchronizeTaskbarWidget();
    }

    internal void RestoreTaskbarAppearancePreview()
    {
        if (_taskbarAppearancePreview is null) return;
        _taskbarAppearancePreview = null;
        SynchronizeTaskbarWidget();
    }

    private void SynchronizeTaskbarWidget()
    {
        string mode = EffectiveTaskbarWidgetMode;
        if (!_isShuttingDown && EffectiveTaskbarWidgetEnabled && TaskbarWidgetSupport.Inspect().CanEnable)
        {
            if (_taskbarPanel is not null && _taskbarPanel.RendererMode != mode)
            {
                _taskbarPanel.Dispose();
                _taskbarPanel = null;
            }
            if (_taskbarPanel is null)
                // Only renderer choice is transient. Clock and schedules always use
                // the shared saved settings, including later successful Save updates.
                _taskbarPanel = new TaskbarCountdownPanel(new AppSettings { TaskbarWidgetMode = mode },
                    ShowTaskbarDetail, text => _mainWindow?.SetTaskbarWidgetRuntimeStatus(text),
                    null, () => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
                        PrayerScheduleService.ResolveTimeZone(Settings.TimeZoneId)).DateTime,
                    date => PrayerScheduleService.ForDate(date, Settings), true);
            else
                _taskbarPanel.RefreshSchedule();
            _taskbarWidget?.RefreshSchedule();
        }
        else
        {
            _taskbarPanel?.Dispose();
            _taskbarPanel = null;
            _taskbarWidget?.ClosePermanently();
            _taskbarWidget = null;
            _mainWindow?.SetTaskbarWidgetRuntimeStatus("Panel taskbar nonaktif. Widget desktop tetap independen.");
        }
    }

    private void ShowTaskbarDetail()
    {
        if (_isShuttingDown || !EffectiveTaskbarWidgetEnabled) return;
        _taskbarWidget ??= new ScheduleWidgetWindow(Settings, taskbarDashboard: true);
        _taskbarWidget.WindowState = WindowState.Normal;
        if (!_taskbarWidget.IsVisible) _taskbarWidget.Show();
        _taskbarWidget.Activate();
    }

    internal async void ShutdownApplication()
    {
        if (_isShuttingDown) return;
        if (_mainWindow?.ConfirmUnsavedSettingsChanges() == false) return;
        _isShuttingDown = true;
        try
        {
            if (_lockScreenScheduler is not null)
            {
                LockScreenTransactionResult restoreResult = await _lockScreenScheduler.StopAndRestoreAsync();
                if (restoreResult == LockScreenTransactionResult.PendingVerification)
                {
                    _mainWindow?.SetLockScreenStatus("Lock screen belum dapat diverifikasi; aplikasi tetap berjalan agar pemulihan dapat dicoba lagi.");
                    _isShuttingDown = false;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.Write("lock-screen.shutdown", ex);
            _mainWindow?.SetLockScreenStatus("Lock screen tidak dapat dipulihkan sekarang; aplikasi tetap berjalan untuk percobaan berikutnya.");
            _isShuttingDown = false;
            return;
        }

        _lockScreenScheduler?.Dispose();
        _mainWindow?.AllowClose();
        _widget?.Close();
        _taskbarPanel?.Dispose();
        _taskbarPanel = null;
        _taskbarWidget?.ClosePermanently();
        _taskbarWidget = null;
        _tray?.Dispose();
        _reminders?.Dispose();
        _autoLock?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _taskbarPanel?.Dispose();
        _taskbarPanel = null;
        _taskbarWidget?.ClosePermanently();
        _taskbarWidget = null;
        _tray?.Dispose();
        _reminders?.Dispose();
        _autoLock?.Dispose();
        _lockScreenScheduler?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private async Task RefreshLockScreenSchedulerAsync()
    {
        try
        {
            if (_lockScreenScheduler is not null)
                await _lockScreenScheduler.RefreshAsync(DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            AppLogger.Write("lock-screen.refresh", ex);
            _mainWindow?.SetLockScreenStatus("Status lock screen belum terverifikasi; perubahan ditahan demi keamanan.");
        }
    }

    internal static BitmapFrame? LoadApplicationIcon()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "AyoJamaah.ico");
        if (!File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        var decoder = IconBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames.FirstOrDefault();
    }
}

