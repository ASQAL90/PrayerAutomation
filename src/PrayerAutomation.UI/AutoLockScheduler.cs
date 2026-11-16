using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

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

    internal AutoLockScheduler(AppSettings settings)
        : this(settings, WorkstationLockService.TryLock, date => PrayerScheduleService.ForDate(date, settings))
    {
    }

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

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_settings.AutoLockEnabled) return;
        _timer.Start();
        OnTick(this, EventArgs.Empty);
    }

    public void Refresh()
    {
        if (_disposed) return;
        _cachedDate = DateTime.MinValue;
        _cachedEntries = [];
        _timeZoneFailureReported = false;
        if (_settings.AutoLockEnabled) _timer.Start();
        else _timer.Stop();
    }

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

    private static bool IsScheduleFailure(Exception exception) =>
        exception is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or
            ArgumentException or DllNotFoundException or EntryPointNotFoundException or
            BadImageFormatException or TypeInitializationException;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}

internal static class WorkstationLockService
{
    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();

    public static bool TryLock()
    {
        if (LockWorkStation()) return true;
        AppLogger.Write("auto-lock.workstation", new Win32Exception(Marshal.GetLastWin32Error()));
        return false;
    }
}
