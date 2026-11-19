using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace PrayerAutomation.UI;

/// <summary>WPF application entry point; owns tray, schedulers, widget, and single-instance lifetime.</summary>
public partial class App : Application
{
    /// <summary>Applies process-wide WPF switches before any window is created.</summary>
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
    /// <summary>Gets the taskbar widget enabled state, honoring an active preview over saved settings.</summary>
    private bool EffectiveTaskbarWidgetEnabled => _taskbarAppearancePreview?.Enabled ?? Settings.TaskbarWidgetEnabled;
    /// <summary>Gets the taskbar widget mode, honoring an active preview over saved settings.</summary>
    private string EffectiveTaskbarWidgetMode => _taskbarAppearancePreview?.Mode ?? Settings.TaskbarWidgetMode;
    private MainWindow? _mainWindow;
    private Mutex? _singleInstanceMutex;
    private bool _widgetSettingEnabled;
    private bool _widgetAppearancePreviewActive;
    private bool _widgetVisibleBeforePreview;
    private bool _widgetExistedBeforePreview;
    private bool _isShuttingDown;

    /// <summary>Gets the current application settings.</summary>
    internal AppSettings Settings { get; private set; } = new();

    /// <summary>Initializes single-instance enforcement, settings, theme, tray, main window, and schedulers at startup.</summary>
    /// <param name="e">Startup event arguments containing command-line args.</param>
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
            AppLogger.WriteInfo,
            imagePathProvider: LockScreenImageRenderer.RenderPrayerImage);
        _lockScreenScheduler.StatusChanged += message => _mainWindow?.SetLockScreenStatus(message);
        try
        {
            await _lockScreenScheduler.StartAsync(DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            AppLogger.Write("lock-screen.startup", ex);
            _mainWindow?.SetLockScreenStatus("Lock screen belum bisa dipastikan; tidak ada perubahan baru yang dijalankan.");
        }
        CompleteWidgetStartup();
        if (Settings.UpdateAutoCheck)
            _ = AutoCheckForUpdatesAsync();
    }

    /// <summary>Completes widget startup after the main window is shown, respecting any active appearance preview.</summary>
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

    /// <summary>Shows, restores, and activates the main window.</summary>
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

    /// <summary>Shows the main window and selects the calendar tab.</summary>
    internal void OpenCalendar()
    {
        ShowMainWindow();
        _mainWindow?.SelectCalendarTab();
    }

    /// <summary>Shows the main window and selects the settings tab.</summary>
    internal void OpenSettings()
    {
        ShowMainWindow();
        _mainWindow?.SelectSettingsTab();
    }

    /// <summary>Toggles the schedule widget visibility.</summary>
    internal void ToggleWidget()
    {
        if (_widget is not null && _widget.IsVisible)
        {
            _widget.Hide();
            return;
        }

        ShowWidget();
    }

    /// <summary>True when the widget is enabled in settings, so the tray entry makes sense.</summary>
    internal bool IsWidgetMenuAvailable => Settings.WidgetEnabled;

    /// <summary>True when the schedule widget currently exists and is visible.</summary>
    internal bool IsScheduleWidgetVisible => _widget is not null && _widget.IsVisible;

    /// <summary>Returns the schedule widget, creating it if it does not exist.</summary>
    /// <returns>The schedule widget instance.</returns>
    private ScheduleWidgetWindow EnsureWidget()
    {
        if (_widget is null)
        {
            _widget = new ScheduleWidgetWindow(Settings);
            _widget.Closed += (_, _) => _widget = null;
        }
        return _widget;
    }

    /// <summary>Shows and activates the schedule widget, creating it if needed.</summary>
    private void ShowWidget()
    {
        ScheduleWidgetWindow widget = EnsureWidget();
        widget.RestoreFixedPlacement();
        if (!widget.IsVisible) widget.Show();
        widget.Activate();
    }

    /// <summary>Applies a transient widget appearance preview without saving settings.</summary>
    /// <param name="enabled">Whether the preview shows the widget.</param>
    /// <param name="mode">Widget mode to preview.</param>
    /// <param name="motion">Widget motion to preview.</param>
    /// <param name="position">Widget position to preview.</param>
    /// <param name="alwaysOnTop">Whether the previewed widget stays above other windows.</param>
    /// <param name="clickThrough">Whether clicks pass through the previewed widget.</param>
    /// <param name="autoHideOnHover">Whether the previewed widget hides while the cursor is near it.</param>
    internal void ApplyWidgetAppearancePreview(bool enabled, string mode, string motion, string position, bool alwaysOnTop,
        bool clickThrough, bool autoHideOnHover)
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
        widget.ApplyAppearancePreview(mode, motion, position, alwaysOnTop, clickThrough, autoHideOnHover);
        if (!widget.IsVisible)
        {
            bool showActivated = widget.ShowActivated;
            widget.ShowActivated = false;
            try { widget.Show(); }
            finally { widget.ShowActivated = showActivated; }
        }
    }

    /// <summary>Restores the widget state captured before the appearance preview began.</summary>
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

    /// <summary>Applies a theme and refreshes theme-dependent views.</summary>
    /// <param name="themeId">Theme identifier to apply.</param>
    internal void ApplyThemePreview(string themeId)
    {
        ThemeManager.Apply(this, themeId);
        _widget?.RefreshSchedule();
        _taskbarWidget?.RefreshSchedule();
        _mainWindow?.RefreshThemeDependentViews();
        _taskbarPanel?.RefreshSchedule();
    }

    /// <summary>Reapplies saved settings across theme, schedulers, widget, and dependent views after a save.</summary>
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

    /// <summary>Applies a transient taskbar widget appearance preview without saving settings.</summary>
    /// <param name="enabled">Whether the preview shows the taskbar widget.</param>
    /// <param name="mode">Taskbar widget mode ("popup" or "native").</param>
    internal void ApplyTaskbarAppearancePreview(bool enabled, string mode)
    {
        if (_isShuttingDown || mode is not ("popup" or "native")) return;
        _taskbarAppearancePreview = (enabled, mode);
        SynchronizeTaskbarWidget();
    }

    /// <summary>Restores the taskbar widget state captured before the appearance preview began.</summary>
    internal void RestoreTaskbarAppearancePreview()
    {
        if (_taskbarAppearancePreview is null) return;
        _taskbarAppearancePreview = null;
        SynchronizeTaskbarWidget();
    }

    /// <summary>Creates, refreshes, or disposes the taskbar panel to match effective settings and support state.</summary>
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
                    ShowTaskbarDetail, null,
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
        }
    }

    /// <summary>Shows the taskbar dashboard detail window, creating it if needed.</summary>
    private void ShowTaskbarDetail()
    {
        if (_isShuttingDown || !EffectiveTaskbarWidgetEnabled) return;
        _taskbarWidget ??= new ScheduleWidgetWindow(Settings, taskbarDashboard: true);
        _taskbarWidget.WindowState = WindowState.Normal;
        if (!_taskbarWidget.IsVisible) _taskbarWidget.Show();
        _taskbarWidget.Activate();
    }

    /// <summary>Shuts down the application, restoring the lock screen first and aborting if restoration is unverified.</summary>
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
                    _mainWindow?.SetLockScreenStatus("Lock screen belum bisa dipastikan; aplikasi tetap berjalan agar pemulihan bisa dicoba lagi.");
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

    /// <summary>Releases taskbar panel, tray, schedulers, lock-screen scheduler, and the single-instance mutex on exit.</summary>
    /// <param name="e">Exit event arguments.</param>
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

    /// <summary>Refreshes the lock-screen scheduler, surfacing failures as a status message.</summary>
    /// <returns>A task representing the asynchronous refresh.</returns>
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
            _mainWindow?.SetLockScreenStatus("Lock screen belum bisa dipastikan; perubahan ditahan demi keamanan.");
        }
    }

    /// <summary>True when a pending lock-screen restore journal is blocking install/uninstall.</summary>
    internal bool HasPendingLockScreenRecovery => _lockScreenScheduler?.HasPendingRecovery == true;

    /// <summary>Restores the previous lock-screen state immediately, without waiting for the scheduled restore.</summary>
    internal async Task RestoreLockScreenNowAsync()
    {
        if (_lockScreenScheduler is null) return;
        try
        {
            await _lockScreenScheduler.RequestRestoreNowAsync();
        }
        catch (Exception ex)
        {
            AppLogger.Write("lock-screen.restore-now", ex);
            _mainWindow?.SetLockScreenStatus(
                "Pemulihan lock screen belum berhasil; aplikasi tetap berjalan untuk percobaan berikutnya.");
        }
    }

    /// <summary>Cancels a pending lock-screen recovery after explicit user confirmation.</summary>
    internal void CancelLockScreenRecovery()
    {
        _lockScreenScheduler?.CancelPendingRecovery();
        Settings.LockScreenEnabled = false;
        Settings.LockScreenOptIn = false;
        SettingsStore.Save(Settings);
        _mainWindow?.SetLockScreenStatus(
            "Pemulihan lock screen dibatalkan. Gambar lock screen dibiarkan seperti saat ini; catatan pemulihan dihapus.");
    }

    private UpdateWindow? _updateWindow;

    /// <summary>Checks GitHub quietly at startup and opens the update window only when something new exists.</summary>
    internal async Task AutoCheckForUpdatesAsync()
    {
        if (_isShuttingDown) return;
        try
        {
            UpdateInfo? info = await new UpdateService().CheckAsync(AppBranding.Version, CancellationToken.None);
            if (info is null || info.Tag == Settings.IgnoredUpdateVersion || _isShuttingDown) return;
            _ = Dispatcher.BeginInvoke(() => ShowUpdateWindow(info));
        }
        catch (Exception ex)
        {
            // Automatic checks stay silent; the window is reserved for explicit checks.
            AppLogger.Write("update.check", ex);
        }
    }

    /// <summary>Opens (or focuses) the update window.</summary>
    /// <param name="info">Known release to display, or null to check first.</param>
    internal void ShowUpdateWindow(UpdateInfo? info = null)
    {
        if (_isShuttingDown) return;
        if (_updateWindow is not null)
        {
            _updateWindow.Activate();
            return;
        }
        _updateWindow = new UpdateWindow(this, info);
        _updateWindow.Closed += (_, _) => _updateWindow = null;
        _updateWindow.Show();
    }

    /// <summary>Remembers a release the user chose not to be offered again.</summary>
    /// <param name="tag">Release tag, e.g. v2.2.1.</param>
    internal void IgnoreUpdate(string tag)
    {
        Settings.IgnoredUpdateVersion = tag;
        SettingsStore.Save(Settings);
    }

    /// <summary>Launches the downloaded installer after this process exits so files are not locked.</summary>
    /// <param name="installerPath">Downloaded installer executable.</param>
    internal void LaunchInstallerAndExit(string installerPath)
    {
        var launcher = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true };
        launcher.ArgumentList.Add("-NoProfile");
        launcher.ArgumentList.Add("-WindowStyle");
        launcher.ArgumentList.Add("Hidden");
        launcher.ArgumentList.Add("-Command");
        launcher.ArgumentList.Add("Wait-Process -Id " + Environment.ProcessId +
            " -ErrorAction SilentlyContinue; Start-Process -FilePath '" + installerPath.Replace("'", "''") + "'");
        Process.Start(launcher);
        ShutdownApplication();
    }

    /// <summary>Triggers an update check requested by the user.</summary>
    internal void CheckForUpdatesNow() => ShowUpdateWindow();

    /// <summary>Loads the application icon from the base directory, or null if it is missing.</summary>
    /// <returns>The decoded icon frame, or null when the icon file does not exist.</returns>
    internal static BitmapFrame? LoadApplicationIcon()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "AyoJamaah.ico");
        if (!File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        var decoder = IconBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames.FirstOrDefault();
    }
}

