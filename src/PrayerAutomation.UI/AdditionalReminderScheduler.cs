using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>Shows combined observance/dzikir reminders and persists their stop/repeat state.</summary>
internal sealed class AdditionalReminderScheduler : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private readonly AppSettings _settings;
    private readonly DispatcherTimer _timer;
    private readonly string _statePath;
    private Dictionary<string, AdditionalReminderStateEntry> _state;
    private DateOnly _currentDate;

    /// <summary>Initializes the scheduler and loads its daily state file.</summary>
    /// <param name="settings">Application settings.</param>
    public AdditionalReminderScheduler(AppSettings settings)
    {
        _settings = settings;
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = PollInterval };
        _timer.Tick += OnTick;
        _statePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrayerAutomation", "additional-reminders.json");
        _state = LoadState();
    }

    /// <summary>Starts polling and processes the current tick immediately.</summary>
    public void Start()
    {
        _timer.Start();
        OnTick(this, EventArgs.Empty);
    }

    /// <summary>Recomputes due reminders after settings change.</summary>
    public void Refresh() => OnTick(this, EventArgs.Empty);

    /// <summary>Stops polling and detaches the timer.</summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        try
        {
            TimeZoneInfo zone = PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId);
            DateTime now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
            DateOnly today = DateOnly.FromDateTime(now);
            if (today != _currentDate)
            {
                _currentDate = today;
                string todayText = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                foreach (string key in _state.Where(item => item.Value.Date != todayText).Select(item => item.Key).ToArray())
                    _state.Remove(key);
                SaveState();
            }

            IReadOnlyList<CalendarObservance> observances = NativePrayerCore.CalendarObservances(
                now, _settings.HijriDateCorrectionDays, _settings.UsesKhgtCalendar);
            IReadOnlyList<CalendarObservance> fastingObservances = NativePrayerCore.CalendarObservances(
                now.Date.AddDays(1), _settings.HijriDateCorrectionDays, _settings.UsesKhgtCalendar);
            foreach (AdditionalReminderGroup group in AdditionalReminderLogic.BuildGroups(
                         _settings, now, observances, fastingObservances))
            {
                DateTime scheduled = now.Date.Add(group.Time);
                int dueCycle = AdditionalReminderLogic.DueCycle(
                    now, scheduled, _settings.ReminderRepeatSeconds, _settings.ReminderRepeatCount);
                if (dueCycle < 0) continue;
                if (!_state.TryGetValue(group.Key, out AdditionalReminderStateEntry? entry))
                {
                    entry = new AdditionalReminderStateEntry
                    {
                        Date = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };
                    _state[group.Key] = entry;
                }
                if (entry.Stopped || entry.LastCycle >= dueCycle) continue;
                entry.LastCycle = dueCycle;
                SaveState();
                ReminderPopup.ShowCustom(group.Heading, group.Message, group.Key, OnStopped, OnAgain,
                    group.NavigationTarget is null ? null : () => OpenDzikir(group.NavigationTarget),
                    group.NavigationLabel);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException
            or ArgumentException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
            or TypeInitializationException or IOException or UnauthorizedAccessException or JsonException)
        {
            AppLogger.Write("reminder.additional", ex);
        }
    }

    /// <summary>Marks one combined schedule as permanently stopped for the day.</summary>
    /// <param name="key">Schedule key supplied by the popup.</param>
    private void OnStopped(string key)
    {
        if (!_state.TryGetValue(key, out AdditionalReminderStateEntry? entry)) return;
        entry.Stopped = true;
        SaveState();
    }

    /// <summary>Handles the remind-again button; the next natural cycle remains counted from the schedule.</summary>
    /// <param name="key">Schedule key supplied by the popup.</param>
    private static void OnAgain(string key) { _ = key; }

    /// <summary>Opens the main window on the matching dzikir sub-tab from a popup button.</summary>
    /// <param name="target"><c>morning</c>, <c>evening</c>, or <c>dzikir</c> to keep the current sub-tab.</param>
    private static void OpenDzikir(string target)
    {
        try
        {
            (Application.Current as App)?.OpenDzikirTab(target);
        }
        catch (Exception ex)
        {
            AppLogger.Write("reminder.additional.navigate", ex);
        }
    }

    private Dictionary<string, AdditionalReminderStateEntry> LoadState()
    {
        try
        {
            if (!File.Exists(_statePath)) return new Dictionary<string, AdditionalReminderStateEntry>(StringComparer.Ordinal);
            AdditionalReminderStateFile? file = JsonSerializer.Deserialize<AdditionalReminderStateFile>(File.ReadAllText(_statePath));
            return file?.Entries is null
                ? new Dictionary<string, AdditionalReminderStateEntry>(StringComparer.Ordinal)
                : new Dictionary<string, AdditionalReminderStateEntry>(file.Entries, StringComparer.Ordinal);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppLogger.Write("reminder.additional.state", ex);
            return new Dictionary<string, AdditionalReminderStateEntry>(StringComparer.Ordinal);
        }
    }

    private void SaveState()
    {
        try
        {
            string? folder = Path.GetDirectoryName(_statePath);
            if (folder is not null) Directory.CreateDirectory(folder);
            string temporary = _statePath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(
                new AdditionalReminderStateFile { Entries = _state },
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, _statePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppLogger.Write("reminder.additional.state-save", ex);
        }
    }
}
