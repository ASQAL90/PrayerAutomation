// Periodically checks whether an essential prayer time has arrived and locks the workstation.
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>Background timer that triggers a workstation lock around essential prayer times.</summary>
internal sealed class AutoLockScheduler : IDisposable
{
    private static readonly TimeSpan TriggerWindow = TimeSpan.FromSeconds(5);

    private readonly AppSettings _settings;
    private readonly Func<bool> _tryLock;
    private readonly Func<DateTime, IReadOnlyList<PrayerEntry>> _scheduleForDate;
    private readonly DispatcherTimer _timer;
    private readonly HashSet<string> _attemptedPrayerKeys = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _cachedDate = DateTime.MinValue;
    private IReadOnlyList<PrayerEntry> _cachedEntries = [];
    private bool _timeZoneFailureReported;
    private bool _disposed;

    /// <summary>Initializes the scheduler using the real workstation lock and schedule services.</summary>
    /// <param name="settings">Application settings that control auto-lock behavior.</param>
    internal AutoLockScheduler(AppSettings settings)
        : this(settings, WorkstationLockService.TryLock, date => PrayerScheduleService.ForDate(date, settings))
    {
    }

    /// <summary>Initializes the scheduler with injectable dependencies, primarily for testing.</summary>
    /// <param name="settings">Application settings that control auto-lock behavior.</param>
    /// <param name="tryLock">Callback that attempts to lock the workstation; returns whether it succeeded.</param>
    /// <param name="scheduleForDate">Callback that returns the prayer entries for a given date.</param>
    internal AutoLockScheduler(
        AppSettings settings,
        Func<bool> tryLock,
        Func<DateTime, IReadOnlyList<PrayerEntry>> scheduleForDate)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(tryLock);
        ArgumentNullException.ThrowIfNull(scheduleForDate);

        _settings = settings;
        _tryLock = tryLock;
        _scheduleForDate = scheduleForDate;
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
    }

    /// <summary>Starts the background timer when auto-lock is enabled and performs an immediate check.</summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_settings.AutoLockEnabled) return;
        _timer.Start();
        OnTick(this, EventArgs.Empty);
    }

    /// <summary>Clears cached schedule state and starts or stops the timer to match current settings.</summary>
    public void Refresh()
    {
        if (_disposed) return;
        _cachedDate = DateTime.MinValue;
        _cachedEntries = [];
        _timeZoneFailureReported = false;
        if (_settings.AutoLockEnabled) _timer.Start();
        else _timer.Stop();
    }

    /// <summary>Evaluates the schedule for the given local time and locks the workstation if a prayer is due.</summary>
    /// <param name="locationLocalNow">Current local date and time at the configured location.</param>
    /// <returns><c>true</c> if the workstation was locked; otherwise <c>false</c>.</returns>
    internal bool ProcessTick(DateTime locationLocalNow)
    {
        if (!_settings.AutoLockEnabled) return false;

        if (_cachedDate != locationLocalNow.Date)
        {
            try
            {
                _cachedEntries = _scheduleForDate(locationLocalNow.Date);
                _cachedDate = locationLocalNow.Date;
            }
            catch (Exception ex) when (IsScheduleFailure(ex))
            {
                _cachedDate = locationLocalNow.Date;
                _cachedEntries = [];
                AppLogger.Write("auto-lock.schedule", ex);
                return false;
            }
        }

        foreach (PrayerEntry prayer in PrayerUiLogic.EssentialPrayerTimes(_cachedEntries))
        {
            string eventKey = $"{locationLocalNow:yyyy-MM-dd}:{prayer.Key}";
            if (_attemptedPrayerKeys.Contains(eventKey)) continue;

            double secondsUntilPrayer = (locationLocalNow.Date.Add(prayer.Time) - locationLocalNow).TotalSeconds;
            if (Math.Abs(secondsUntilPrayer) > TriggerWindow.TotalSeconds) continue;

            _attemptedPrayerKeys.Add(eventKey);
            try
            {
                return _tryLock();
            }
            catch (Exception ex) when (IsScheduleFailure(ex) || ex is Win32Exception or ExternalException)
            {
                AppLogger.Write("auto-lock.workstation", ex);
                return false;
            }
        }

        return false;
    }

    /// <summary>Handles the timer tick by resolving the configured time zone and running a check.</summary>
    private void OnTick(object? sender, EventArgs e)
    {
        if (!_settings.AutoLockEnabled)
        {
            _timer.Stop();
            return;
        }

        try
        {
            TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
            DateTime locationLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
            _timeZoneFailureReported = false;
            _ = ProcessTick(locationLocalNow);
        }
        catch (Exception ex) when (IsScheduleFailure(ex))
        {
            if (!_timeZoneFailureReported)
                AppLogger.Write("auto-lock.tick", ex);
            _timeZoneFailureReported = true;
        }
    }

    /// <summary>Returns true when the exception represents a schedule or native-interop failure worth logging.</summary>
    private static bool IsScheduleFailure(Exception exception) =>
        exception is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or
            ArgumentException or DllNotFoundException or EntryPointNotFoundException or
            BadImageFormatException or TypeInitializationException;

    /// <summary>Stops the timer and detaches the tick handler.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}

/// <summary>Native workstation-lock helper built on the Win32 <c>LockWorkStation</c> API.</summary>
internal static class WorkstationLockService
{
    /// <summary>Locks the workstation via the Win32 API.</summary>
    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();

    /// <summary>Attempts to lock the workstation and logs a warning when the call fails.</summary>
    /// <returns><c>true</c> if the workstation was locked; otherwise <c>false</c>.</returns>
    public static bool TryLock()
    {
        if (LockWorkStation()) return true;
        AppLogger.Write("auto-lock.workstation", new Win32Exception(Marshal.GetLastWin32Error()));
        return false;
    }
}
