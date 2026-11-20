using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
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

        // The updater helper is a copy of this executable. It must run before the
        // single-instance mutex, otherwise it would shut itself down immediately.
        if (UpdateApplier.TryHandleCommandLine(e.Args, out int updateExitCode))
        {
            Environment.ExitCode = updateExitCode;
            Shutdown();
            return;
        }

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
        StartupRegistration.SelfHeal(Settings.StartupEnabled, Environment.ProcessPath);
        StorageMaintenance.DeleteFinishedUpdateJobs();
        ThemeManager.Apply(this, Settings.ThemeId);
        _widgetSettingEnabled = Settings.WidgetEnabled;
        _tray = new TrayService(this);
        _mainWindow = CreateMainWindow();
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
                new FileLockScreenRestoreStore(),
                () => LockScreenSafetyProbe.Inspect().ManagedByPolicy),
            AppLogger.Write,
            () => Settings.LockScreenDurationMinutes,
            new WindowsLockScreenExecutionRequest(AppLogger.WriteInfo),
            AppLogger.WriteInfo,
            imagePathProvider: LockScreenImageRenderer.RenderPrayerImage);
        _lockScreenScheduler.StatusChanged += message => _mainWindow?.SetLockScreenStatus(message);
        // Session events are only interesting while a temporary image is active;
        // outside a transaction they would flood the log on every lock/unlock.
        SystemEvents.SessionSwitch += OnSessionSwitch;
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
        _ = Task.Run(() =>
        {
            int removed = StorageMaintenance.DeleteOldInstallers();
            if (removed > 0) AppLogger.WriteInfo("storage.cleanup", $"oldInstallersRemoved={removed}");
        });
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

    /// <summary>Creates the main window and tracks its closure so a tray action can recreate it.</summary>
    /// <returns>The new main window.</returns>
    private MainWindow CreateMainWindow()
    {
        var window = new MainWindow(Settings);
        string? updateFailure = UpdateApplier.FindLastFailure();
        if (updateFailure is not null)
            window.SetStorageStatus("Pembaruan terakhir gagal dan sudah dikembalikan: " + updateFailure + ".");
        window.Closed += (_, _) =>
        {
            // WPF forbids Show() on a closed Window; drop the reference instead.
            if (ReferenceEquals(_mainWindow, window)) _mainWindow = null;
            if (ReferenceEquals(MainWindow, window)) MainWindow = null;
        };
        MainWindow = window;
        return window;
    }

    /// <summary>Shows, restores, and activates the main window, recreating it after a real close.</summary>
    internal void ShowMainWindow()
    {
        if (_isShuttingDown) return;
        MainWindow? window = _mainWindow ??= CreateMainWindow();
        if (!window.IsVisible)
        {
            window.RestoreSavedSettingsPreview();
            window.Show();
        }
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
        window.Activate();
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

        ReleaseAndShutdown();
    }

    /// <summary>Releases windows, tray, schedulers, and the single-instance mutex, then exits.</summary>
    private void ReleaseAndShutdown()
    {
        try
        {
            // Allow the real close first: Shutdown() must not be blocked by a
            // later cleanup failure leaving the window in hide-to-tray mode.
            _mainWindow?.AllowClose();
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _updateReminderTimer?.Stop();
            _lockScreenScheduler?.Dispose();
            _widget?.Close();
            _taskbarPanel?.Dispose();
            _taskbarPanel = null;
            _taskbarWidget?.ClosePermanently();
            _taskbarWidget = null;
            _tray?.Dispose();
            _reminders?.Dispose();
            _autoLock?.Dispose();
        }
        catch (Exception ex)
        {
            // A cleanup failure must not leave a half-shut app whose window can no longer be shown.
            AppLogger.Write("app.shutdown-cleanup", ex);
        }
        finally
        {
            Shutdown();
        }
    }

    /// <summary>Releases taskbar panel, tray, schedulers, lock-screen scheduler, and the single-instance mutex on exit.</summary>
    /// <param name="e">Exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _updateReminderTimer?.Stop();
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

    /// <summary>Logs session lock/unlock only while a temporary lock-screen image is active.</summary>
    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (_isShuttingDown || _lockScreenScheduler?.HasActiveImage != true) return;
        string context = e.Reason switch
        {
            SessionSwitchReason.SessionLock => "lock-screen.session.lock",
            SessionSwitchReason.SessionUnlock => "lock-screen.session.unlock",
            _ => string.Empty
        };
        if (context.Length > 0)
            AppLogger.WriteInfo(context, "transaction=active visual=unverified");
    }

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

    /// <summary>Applies the real lock-screen image for one minute, then restores automatically.</summary>
    /// <returns>The outcome of the test transaction.</returns>
    internal async Task<LockScreenTransactionResult> TestLockScreenNowAsync()
    {
        if (_lockScreenScheduler is null)
            throw new InvalidOperationException("Lock screen belum siap.");
        if (_lockScreenScheduler.HasPendingRecovery)
            throw new InvalidOperationException("Pemulihan lock screen masih tertunda; selesaikan dulu sebelum menguji.");
        return await _lockScreenScheduler.ApplyTestAsync(
            DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
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

    /// <summary>Closes the lock-screen restore journal before an update without blocking it.</summary>
    /// <remarks>
    /// One normal restore attempt runs first. An unresolved or failed verification is forced
    /// closed immediately; the update continues even when verification cannot be proven.
    /// </remarks>
    /// <param name="progress">Optional callback used by the update window.</param>
    /// <param name="cancellationToken">Cancels the preflight when the update window closes.</param>
    /// <returns>Always true unless the preflight was cancelled.</returns>
    internal async Task<bool> EnsureLockScreenReadyForUpdateAsync(
        Action<LockScreenRecoveryProgress>? progress,
        CancellationToken cancellationToken)
    {
        LockScreenImageScheduler? scheduler = _lockScreenScheduler;
        if (scheduler is null) return true;
        try
        {
            LockScreenTransactionResult result = await scheduler.StopAndRestoreAsync();
            if (UpdateRecoveryPolicy.IsReady(result))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return true;
            }
            progress?.Invoke(new LockScreenRecoveryProgress(0, scheduler.RestoreDeadlineUtc));
        }
        catch (OperationCanceledException)
        {
            // Closing the update window must not leave the scheduler in restore-only mode.
            await ResumeLockScreenAfterUpdateCancelAsync();
            throw;
        }
        catch (Exception ex)
        {
            AppLogger.Write("update.lock-screen-restore", ex);
        }

        try
        {
            await scheduler.ForceRestoreForUpdateAsync();
            _mainWindow?.SetLockScreenStatus(
                "Pemulihan lock screen diselesaikan secara paksa; pembaruan tetap dilanjutkan.");
        }
        catch (OperationCanceledException)
        {
            await ResumeLockScreenAfterUpdateCancelAsync();
            throw;
        }
        catch (Exception ex)
        {
            AppLogger.Write("update.lock-screen-force", ex);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return true;
    }

    private UpdateWindow? _updateWindow;
    private DispatcherTimer? _updateReminderTimer;
    private UpdateRelease? _snoozedUpdate;
    private DateTimeOffset _updateReminderDueUtc;

    /// <summary>Checks GitHub quietly at startup and opens the update window only when something new exists.</summary>
    internal async Task AutoCheckForUpdatesAsync()
    {
        if (_isShuttingDown) return;
        // Capture before ConfigureAwait(false): the Dispatcher getter itself
        // calls VerifyAccess and throws when read from the worker thread.
        Dispatcher ui = Dispatcher;
        try
        {
            bool installed = StartupRegistration.IsInstalledPath(
                Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty) ?? AppContext.BaseDirectory);
            UpdateRelease? release = await new UpdateService()
                .CheckAsync(AppBranding.Version, installed, CancellationToken.None).ConfigureAwait(false);
            if (release is null || release.Tag == Settings.IgnoredUpdateVersion || _isShuttingDown) return;
            _ = ui.BeginInvoke(() => ShowUpdateWindow(release));
        }
        catch (Exception ex)
        {
            // Automatic checks stay silent; the window is reserved for explicit checks.
            AppLogger.Write("update.check", ex);
        }
    }

    /// <summary>Opens (or focuses) the update window.</summary>
    /// <param name="info">Known release to display, or null to check first.</param>
    /// <param name="autoInstall">True to start the download immediately after a confirmed reminder.</param>
    internal void ShowUpdateWindow(UpdateRelease? info = null, bool autoInstall = false)
    {
        if (_isShuttingDown) return;
        StopUpdateReminder();
        if (_updateWindow is not null)
        {
            _updateWindow.Activate();
            if (autoInstall) _updateWindow.BeginInstall();
            return;
        }
        _updateWindow = new UpdateWindow(this, info);
        _updateWindow.Closed += (_, _) => _updateWindow = null;
        _updateWindow.Show();
        if (autoInstall) _updateWindow.BeginInstall();
    }

    /// <summary>Defers the update offer and counts down until lock-screen recovery is finished.</summary>
    /// <param name="release">Release to remind the user about.</param>
    internal void SnoozeUpdate(UpdateRelease release)
    {
        if (_isShuttingDown) return;
        _snoozedUpdate = release;
        _updateReminderDueUtc = ResolveUpdateReminderDueUtc();
        if (_updateReminderTimer is null)
        {
            _updateReminderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _updateReminderTimer.Tick += OnUpdateReminderTick;
        }
        _updateReminderTimer.Start();
        UpdateUpdateReminderTooltip();
    }

    /// <summary>Uses the journal deadline while recovery is active; otherwise falls back to 15 minutes.</summary>
    /// <returns>UTC time when the reminder popup may appear.</returns>
    private DateTimeOffset ResolveUpdateReminderDueUtc()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        LockScreenImageScheduler? scheduler = _lockScreenScheduler;
        return UpdateReminderPolicy.ResolveDue(
            now,
            scheduler?.HasActiveImage == true,
            scheduler?.HasPendingRecovery == true,
            scheduler?.RestoreDeadlineUtc);
    }

    /// <summary>Updates the tray countdown and opens the app-styled reminder when recovery is clear.</summary>
    /// <param name="sender">Timer source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnUpdateReminderTick(object? sender, EventArgs e)
    {
        if (_isShuttingDown || _snoozedUpdate is null)
        {
            StopUpdateReminder();
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        LockScreenImageScheduler? scheduler = _lockScreenScheduler;
        if (scheduler?.HasPendingRecovery == true)
        {
            _updateReminderDueUtc = now + UpdateReminderPolicy.RecoveryPollDelay;
            _tray?.SetTooltip($"Pembaruan {_snoozedUpdate.Tag}: menunggu pemulihan lock screen");
            return;
        }
        if (scheduler?.HasActiveImage == true)
        {
            DateTimeOffset? deadline = scheduler.RestoreDeadlineUtc;
            if (deadline is { } value && value > now)
            {
                _updateReminderDueUtc = value;
                _tray?.SetTooltip($"Pembaruan {_snoozedUpdate.Tag}: pemulihan lock screen {UpdateReminderPolicy.FormatRemaining(value - now)}");
            }
            else
            {
                _updateReminderDueUtc = now + UpdateReminderPolicy.RecoveryPollDelay;
                _tray?.SetTooltip($"Pembaruan {_snoozedUpdate.Tag}: menunggu pemulihan lock screen");
            }
            return;
        }

        if (now < _updateReminderDueUtc)
        {
            _tray?.SetTooltip($"Pembaruan {_snoozedUpdate.Tag} diingatkan dalam {UpdateReminderPolicy.FormatRemaining(_updateReminderDueUtc - now)}");
            return;
        }

        UpdateRelease release = _snoozedUpdate;
        StopUpdateReminder();
        ShowUpdateReminder(release);
    }

    /// <summary>Paints the current countdown into the tray tooltip.</summary>
    private void UpdateUpdateReminderTooltip()
    {
        if (_snoozedUpdate is null) return;
        _tray?.SetTooltip($"Pembaruan {_snoozedUpdate.Tag} diingatkan dalam {UpdateReminderPolicy.FormatRemaining(_updateReminderDueUtc - DateTimeOffset.UtcNow)}");
    }

    /// <summary>Asks again with the app-styled dialog, then installs or snoozes once more.</summary>
    /// <param name="release">Release to offer.</param>
    private void ShowUpdateReminder(UpdateRelease release)
    {
        if (_isShuttingDown || _mainWindow is null) return;
        Window? owner = _mainWindow.IsVisible ? _mainWindow : null;
        bool install = ConfirmDialog.Ask(owner, "Pembaruan tersedia",
            $"Waktu tunggu selesai. Versi {release.Tag} sudah menunggu. Mau memasang sekarang?",
            "Pasang sekarang", "Nanti lagi", confirmIsDefault: true);
        if (install) ShowUpdateWindow(release, autoInstall: true);
        else SnoozeUpdate(release);
    }

    /// <summary>Stops the tray countdown and restores the default tooltip.</summary>
    private void StopUpdateReminder()
    {
        _updateReminderTimer?.Stop();
        _snoozedUpdate = null;
        _tray?.SetTooltip(AppBranding.DisplayName);
    }


    /// <summary>Remembers a release the user chose not to be offered again.</summary>
    /// <param name="tag">Release tag, e.g. v2.2.2.</param>
    internal void IgnoreUpdate(string tag)
    {
        Settings.IgnoredUpdateVersion = tag;
        SettingsStore.Save(Settings);
    }

    /// <summary>Launches the downloaded installer after this process exits so files are not locked.</summary>
    /// <param name="installerPath">Downloaded installer executable.</param>
    /// <param name="progress">Optional lock-screen restore progress callback.</param>
    /// <param name="cancellationToken">Cancels the restore wait when the update window closes.</param>
    /// <returns>Started, Cancelled, or LockScreenPending.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the update cannot be prepared or started.</exception>
    internal async Task<UpdateLaunchResult> InstallUpdateAsync(
        string installerPath,
        Action<LockScreenRecoveryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(installerPath))
            throw new FileNotFoundException("Pemasang tidak ditemukan.", installerPath);
        if (_isShuttingDown) return UpdateLaunchResult.Cancelled;

        // Resolve prompts and pending lock-screen recovery before starting the
        // installer: it waits only briefly for this process to exit.
        if (_mainWindow?.ConfirmUnsavedSettingsChanges() == false) return UpdateLaunchResult.Cancelled;
        if (!await EnsureLockScreenReadyForUpdateAsync(progress, cancellationToken))
            return UpdateLaunchResult.LockScreenPending;
        _isShuttingDown = true;

        // The installer waits for this process to exit (see /UPDATEMODE in the
        // NSIS script), so no PowerShell helper or console window is needed.
        var launcher = new ProcessStartInfo(installerPath)
        {
            UseShellExecute = true,
            Arguments = "/S /UPDATEMODE"
        };
        try
        {
            Process.Start(launcher);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        {
            AppLogger.Write("update.launch", ex);
            await AbortUpdatePreparationAsync();
            throw new InvalidOperationException("Pemasang tidak bisa dijalankan: " + ex.Message, ex);
        }
        ReleaseAndShutdown();
        return UpdateLaunchResult.Started;
    }

    /// <summary>Stages a verified delta, writes its job, and hands it to the updater helper.</summary>
    /// <param name="plan">Verified update plan.</param>
    /// <param name="deltaZipPath">Downloaded and hash-checked delta zip.</param>
    /// <param name="progress">Optional lock-screen restore progress callback.</param>
    /// <param name="cancellationToken">Cancels the restore wait when the update window closes.</param>
    /// <returns>Started, Cancelled, or LockScreenPending.</returns>
    /// <remarks>Staging must stay on the UI thread: ReleaseAndShutdown touches windows and the tray.</remarks>
    /// <exception cref="InvalidOperationException">Thrown when the delta cannot be prepared.</exception>
    internal async Task<UpdateLaunchResult> InstallDeltaAsync(
        UpdatePlan plan,
        string deltaZipPath,
        Action<LockScreenRecoveryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(deltaZipPath))
            throw new FileNotFoundException("Berkas pembaruan tidak ditemukan.", deltaZipPath);
        if (plan.Needed.Count == 0) return UpdateLaunchResult.Cancelled;
        if (_isShuttingDown) return UpdateLaunchResult.Cancelled;
        if (_mainWindow?.ConfirmUnsavedSettingsChanges() == false) return UpdateLaunchResult.Cancelled;
        if (!await EnsureLockScreenReadyForUpdateAsync(progress, cancellationToken))
            return UpdateLaunchResult.LockScreenPending;
        _isShuttingDown = true;

        string executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("Path aplikasi tidak dapat ditentukan.");
        string root = Path.GetDirectoryName(executable) ?? AppContext.BaseDirectory;
        string jobFolder = Path.Combine(StorageMaintenance.UpdateFolder, "jobs",
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" +
            Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
        try
        {
            Directory.CreateDirectory(jobFolder);
            string stage = Path.Combine(jobFolder, "stage");
            ZipFile.ExtractToDirectory(deltaZipPath, stage);
            foreach (ManifestFile file in plan.Needed)
            {
                string source = Path.Combine(stage, file.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(source) || new FileInfo(source).Length != file.Size ||
                    !string.Equals(await UpdatePlanner.ComputeSha256Async(source, CancellationToken.None),
                        file.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Isi pembaruan tidak lolos pemeriksaan keutuhan.");
            }

            var job = new UpdateJob
            {
                TargetRoot = root,
                SourceFolder = stage,
                BackupFolder = Path.Combine(jobFolder, "backup"),
                JournalPath = Path.Combine(jobFolder, "journal.json"),
                Files = plan.Needed.Select(file => file.Path).ToList(),
                Deleted = plan.Deleted.ToList(),
                TargetVersion = plan.Manifest.Version,
                IsInstalled = StartupRegistration.IsInstalledPath(root)
            };
            string jobPath = Path.Combine(jobFolder, "job.json");
            job.Save(jobPath);

            // PublishSingleFile is false, so the helper needs the rest of the
            // application files next to it. Copy the whole folder, then give the
            // copy a different name so the locked AyoJamaah.exe can be replaced.
            string updateFolder = StorageMaintenance.UpdateFolder;
            Directory.CreateDirectory(updateFolder);
            foreach (string sourceFile in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                string destination = Path.Combine(updateFolder, Path.GetRelativePath(root, sourceFile));
                string? parent = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                File.Copy(sourceFile, destination, overwrite: true);
            }
            string helper = Path.Combine(updateFolder, "Updater.exe");
            File.Copy(executable, helper, overwrite: true);
            var start = new ProcessStartInfo(helper)
            {
                UseShellExecute = true,
                WorkingDirectory = StorageMaintenance.UpdateFolder,
                Arguments = "--apply-update \"" + jobPath + "\" " +
                    Environment.ProcessId.ToString(CultureInfo.InvariantCulture)
            };
            Process.Start(start);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException
            or System.ComponentModel.Win32Exception)
        {
            await AbortUpdatePreparationAsync().ConfigureAwait(false);
            AppLogger.Write("update.stage-delta", ex);
            throw new InvalidOperationException("Pembaruan tidak bisa disiapkan: " + ex.Message, ex);
        }
        ReleaseAndShutdown();
        return UpdateLaunchResult.Started;
    }

    /// <summary>Resumes normal lock-screen scheduling after the user cancels a blocked update.</summary>
    internal async Task ResumeLockScreenAfterUpdateCancelAsync()
    {
        if (_isShuttingDown) return;
        LockScreenImageScheduler? scheduler = _lockScreenScheduler;
        if (scheduler is null) return;
        try { await scheduler.ResumeSchedulingAsync(DateTimeOffset.UtcNow); }
        catch (Exception ex) { AppLogger.Write("update.cancel-resume", ex); }
    }

    /// <summary>Restores scheduling after a cancelled update preparation.</summary>
    private async Task AbortUpdatePreparationAsync()
    {
        _isShuttingDown = false;
        if (_lockScreenScheduler is null) return;
        // Keep the resume on the UI thread; the scheduler recreates its DispatcherTimer there.
        try { await _lockScreenScheduler.ResumeSchedulingAsync(DateTimeOffset.UtcNow); }
        catch (Exception ex) { AppLogger.Write("update.prepare-resume", ex); }
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

