using System.IO;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

internal sealed class ReminderScheduler : IDisposable
{
    private readonly AppSettings _settings;
    private readonly DispatcherTimer _timer;
    private readonly HashSet<string> _sentForDate = new(StringComparer.Ordinal);
    private DateOnly _currentDate;
    private IReadOnlyList<PrayerEntry> _entries = [];

    public ReminderScheduler(AppSettings settings)
    {
        _settings = settings;
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        _timer.Start();
        OnTick(this, EventArgs.Empty);
    }

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

    private static string FormatMessage(string template, PrayerEntry entry, int minutesBefore, string location)
    {
        string text = string.IsNullOrWhiteSpace(template) ? AppSettings.DefaultReminderMessage : template;
        return text.Replace("{prayer}", entry.Label, StringComparison.OrdinalIgnoreCase)
            .Replace("{prayer_key}", entry.Key, StringComparison.OrdinalIgnoreCase)
            .Replace("{time}", entry.Time.ToString(@"hh\:mm"), StringComparison.OrdinalIgnoreCase)
            .Replace("{minutes_before}", minutesBefore.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{location}", location, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}

internal static class AppLogger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PrayerAutomation", "prayer_automation.log");

    public static void Write(string context, Exception exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"{DateTimeOffset.Now:O} ERROR [{context}] {exception.Message}{Environment.NewLine}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    // Only explicitly wired, privacy-safe timeline fields belong in this information channel.
    public static void WriteInfo(string context, string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"{DateTimeOffset.UtcNow:O} INFO [{context}] {message}{Environment.NewLine}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
