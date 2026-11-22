using System.IO;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>Polls the prayer schedule and shows reminder popups ahead of each prayer time.</summary>
internal sealed class ReminderScheduler : IDisposable
{
    private readonly AppSettings _settings;
    private readonly DispatcherTimer _timer;
    private readonly HashSet<string> _sentForDate = new(StringComparer.Ordinal);
    private DateOnly _currentDate;
    private IReadOnlyList<PrayerEntry> _entries = [];

    /// <summary>Initializes the scheduler and its 15-second timer.</summary>
    /// <param name="settings">Application settings.</param>
    public ReminderScheduler(AppSettings settings)
    {
        _settings = settings;
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += OnTick;
    }

    /// <summary>Starts the timer and processes the current tick immediately.</summary>
    public void Start()
    {
        _timer.Start();
        OnTick(this, EventArgs.Empty);
    }

    /// <summary>Recomputes the schedule from settings; clears entries when reminders are disabled.</summary>
    public void Refresh()
    {
        if (!_settings.RemindersEnabled || _settings.ReminderMinutes <= 0)
        {
            _entries = [];
            return;
        }
        try
        {
            TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
            DateTime now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
            DateOnly today = DateOnly.FromDateTime(now);
            if (today != _currentDate)
            {
                _currentDate = today;
                _sentForDate.Clear();
            }
            _entries = PrayerScheduleService.ReminderEntries(PrayerScheduleService.ForDate(now, _settings));
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException)
        {
            AppLogger.Write("reminder.refresh", ex);
            _entries = [];
        }
    }

    /// <summary>Timer handler; shows reminders due within the configured lead time.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnTick(object? sender, EventArgs e)
    {
        if (!_settings.RemindersEnabled || _settings.ReminderMinutes <= 0) return;
        try
        {
            TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
            DateTime now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
            var today = DateOnly.FromDateTime(now);
            if (today != _currentDate)
            {
                _currentDate = today;
                _sentForDate.Clear();
                _entries = PrayerScheduleService.ReminderEntries(PrayerScheduleService.ForDate(now, _settings));
            }
            foreach (PrayerEntry entry in _entries)
            {
                if (_sentForDate.Contains(entry.Key)) continue;
                DateTime prayerTime = now.Date.Add(entry.Time);
                double seconds = (prayerTime - now).TotalSeconds;
                if (seconds <= 0 || seconds > _settings.ReminderMinutes * 60) continue;
                _sentForDate.Add(entry.Key);
                string message = FormatMessage(_settings.ReminderMessage, entry, _settings.ReminderMinutes, _settings.LocationName);
                ReminderPopup.Show(entry.Label, message);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException)
        {
            AppLogger.Write("reminder.schedule", ex);
        }
    }

    /// <summary>Expands a reminder template, replacing {prayer}, {prayer_key}, {time}, {minutes_before} and {location}.</summary>
    /// <param name="template">Message template; falls back to the default when blank.</param>
    /// <param name="entry">The prayer entry.</param>
    /// <param name="minutesBefore">Configured lead time in minutes.</param>
    /// <param name="location">Location display name.</param>
    /// <returns>The expanded message.</returns>
    private static string FormatMessage(string template, PrayerEntry entry, int minutesBefore, string location)
    {
        string text = string.IsNullOrWhiteSpace(template) ? AppSettings.DefaultReminderMessage : template;
        return text.Replace("{prayer}", entry.Label, StringComparison.OrdinalIgnoreCase)
            .Replace("{prayer_key}", entry.Key, StringComparison.OrdinalIgnoreCase)
            .Replace("{time}", entry.Time.ToString(@"hh\:mm"), StringComparison.OrdinalIgnoreCase)
            .Replace("{minutes_before}", minutesBefore.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{location}", location, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Stops and detaches the timer.</summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}

/// <summary>Appends error and privacy-safe information entries to the application log file.</summary>
internal static class AppLogger
{
    private const long MaxLogBytes = 2 * 1024 * 1024;
    private const int MaxLogFiles = 3;
    private static readonly TimeSpan ArchiveRetention = TimeSpan.FromDays(30);
    private static DateTime _lastRetentionCheckUtc = DateTime.MinValue;

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PrayerAutomation", "prayer_automation.log");

    /// <summary>Appends an ERROR entry to the log; I/O failures are swallowed.</summary>
    /// <param name="context">Context tag identifying the subsystem.</param>
    /// <param name="exception">Exception to log.</param>
    public static void Write(string context, Exception exception)
    {
        Append($"{DateTimeOffset.Now:O} ERROR [{context}] {exception}{Environment.NewLine}");
    }

    // Only explicitly wired, privacy-safe timeline fields belong in this information channel.
    /// <summary>Appends a privacy-safe INFO entry to the log; I/O failures are swallowed.</summary>
    /// <param name="context">Context tag identifying the subsystem.</param>
    /// <param name="message">Pre-approved timeline message.</param>
    public static void WriteInfo(string context, string message)
    {
        Append($"{DateTimeOffset.UtcNow:O} INFO [{context}] {message}{Environment.NewLine}");
    }

    /// <summary>Appends one line, rotating at 2 MB and keeping three files at most.</summary>
    /// <param name="line">Complete log line including a newline.</param>
    private static void Append(string line)
    {
        try
        {
            string? folder = Path.GetDirectoryName(LogPath);
            if (folder is not null) Directory.CreateDirectory(folder);
            RotateIfNeeded();
            DeleteExpiredArchives();
            File.AppendAllText(LogPath, line);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    /// <summary>Shifts the active log to .1 and .1 to .2 once the size limit is reached.</summary>
    private static void RotateIfNeeded()
    {
        var active = new FileInfo(LogPath);
        if (!active.Exists || active.Length < MaxLogBytes) return;
        for (int index = MaxLogFiles - 1; index >= 1; index--)
        {
            string source = index == 1 ? LogPath : LogPath + "." + (index - 1);
            string target = LogPath + "." + index;
            if (!File.Exists(source)) continue;
            if (File.Exists(target)) File.Delete(target);
            File.Move(source, target);
        }
    }

    /// <summary>Deletes rotated logs older than the retention window, at most once a day.</summary>
    private static void DeleteExpiredArchives()
    {
        DateTime nowUtc = DateTime.UtcNow;
        if (nowUtc - _lastRetentionCheckUtc < TimeSpan.FromDays(1)) return;
        _lastRetentionCheckUtc = nowUtc;
        for (int index = 1; index < MaxLogFiles; index++)
        {
            string path = LogPath + "." + index;
            try
            {
                var archive = new FileInfo(path);
                if (archive.Exists && nowUtc - archive.LastWriteTimeUtc > ArchiveRetention)
                    archive.Delete();
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
