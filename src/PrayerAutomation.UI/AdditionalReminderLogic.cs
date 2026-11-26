using System.Globalization;
using System.Text.Json.Serialization;

namespace PrayerAutomation.UI;

/// <summary>One combined reminder schedule for a single time of day.</summary>
/// <param name="Key">Stable key including the local date and time.</param>
/// <param name="Time">Scheduled time of day.</param>
/// <param name="Heading">Popup heading.</param>
/// <param name="Message">Combined human-readable reminder text.</param>
internal sealed record AdditionalReminderGroup(string Key, TimeSpan Time, string Heading, string Message,
    string? NavigationTarget, string? NavigationLabel);

/// <summary>Per-day state for a combined reminder schedule.</summary>
internal sealed class AdditionalReminderStateEntry
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("stopped")] public bool Stopped { get; set; }
    [JsonPropertyName("last_cycle")] public int LastCycle { get; set; } = -1;
}

/// <summary>Persisted stop/repeat state for additional reminders.</summary>
internal sealed class AdditionalReminderStateFile
{
    [JsonPropertyName("entries")] public Dictionary<string, AdditionalReminderStateEntry> Entries { get; set; } =
        new(StringComparer.Ordinal);
}

/// <summary>Pure grouping and repeat calculations for observance and dzikir reminders.</summary>
internal static class AdditionalReminderLogic
{
    private sealed class ReminderBucket
    {
        internal List<string> Names { get; } = [];
        internal Dictionary<string, List<string>> CategoriesByName { get; } = new(StringComparer.Ordinal);
        internal List<string> PlainSegments { get; } = [];

        internal void AddObservance(string name, string category)
        {
            if (!CategoriesByName.TryGetValue(name, out List<string>? categories))
            {
                categories = [];
                CategoriesByName[name] = categories;
                Names.Add(name);
            }
            if (!categories.Contains(category, StringComparer.Ordinal)) categories.Add(category);
        }

        internal IEnumerable<string> Segments()
        {
            foreach (string name in Names)
                yield return $"{name} ({string.Join(" & ", CategoriesByName[name])})";
            foreach (string segment in PlainSegments)
                yield return segment;
        }
    }

    /// <summary>Builds one group per distinct reminder time, combining every active segment.</summary>
    /// <param name="settings">Current application settings.</param>
    /// <param name="date">Local execution date; the key and stamp always use this date.</param>
    /// <param name="observances">Observances on the execution date (holidays and Islamic events).</param>
    /// <param name="fastingObservances">Observances on the next date, used for sunnah-fast and no-fast reminders.</param>
    /// <returns>Groups ordered by time of day.</returns>
    internal static IReadOnlyList<AdditionalReminderGroup> BuildGroups(
        AppSettings settings, DateTime date, IReadOnlyList<CalendarObservance> observances,
        IReadOnlyList<CalendarObservance>? fastingObservances = null)
    {
        Dictionary<string, ReminderBucket> buckets = new(StringComparer.Ordinal);

        ReminderBucket Bucket(string timeText)
        {
            string time = AppDateTimeFormat.NormalizeTime(timeText, AppSettings.DefaultMorningReminderTime);
            if (!buckets.TryGetValue(time, out ReminderBucket? bucket))
            {
                bucket = new ReminderBucket();
                buckets[time] = bucket;
            }
            return bucket;
        }

        static bool IsSunnahFast(CalendarObservanceKind kind) =>
            kind is CalendarObservanceKind.SunnahFast or CalendarObservanceKind.FlexibleFastWindow;

        // Each observance belongs to exactly one reminder bucket so the same date never
        // appears both as "hari besar" today and "puasa sunnah" tomorrow.
        static bool IsHolidayOrEid(CalendarObservanceKind kind) =>
            kind is CalendarObservanceKind.PublicHoliday or CalendarObservanceKind.EidNoFast;

        foreach (CalendarObservance observance in observances)
        {
            if (settings.PublicHolidayReminderEnabled && IsHolidayOrEid(observance.Kind))
            {
                Bucket(settings.PublicHolidayReminderTime).AddObservance(observance.ShortName,
                    observance.Kind == CalendarObservanceKind.EidNoFast ? "hari raya Id" : "hari libur nasional");
            }
            if (settings.IslamicHolidayReminderEnabled && observance.Kind == CalendarObservanceKind.ReligiousEvent)
                Bucket(settings.IslamicHolidayReminderTime).AddObservance(observance.ShortName, "peringatan Islam");
        }

        // Puasa dan larangan puasa berlaku Subuh–Maghrib, jadi pengingatnya ditujukan ke besok.
        string nextDate = AppDateTimeFormat.Date(
            date.Date.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        foreach (CalendarObservance observance in fastingObservances ?? observances)
        {
            if (settings.SunnahFastingReminderEnabled && IsSunnahFast(observance.Kind))
                Bucket(settings.SunnahFastingReminderTime).PlainSegments.Add(
                    $"Besok, {nextDate}: {observance.ShortName} (puasa sunnah)");
            if (settings.NoFastReminderEnabled && observance.Kind == CalendarObservanceKind.NoFast)
                Bucket(settings.NoFastReminderTime).PlainSegments.Add(
                    $"Besok, {nextDate}: {observance.ShortName} (dilarang berpuasa)");
        }

        if (settings.MorningDzikirReminderEnabled)
            Bucket(settings.MorningDzikirReminderTime).PlainSegments.Add("Dzikir pagi");
        if (settings.EveningDzikirReminderEnabled)
            Bucket(settings.EveningDzikirReminderTime).PlainSegments.Add("Dzikir petang");

        return buckets
            .OrderBy(pair => AppDateTimeFormat.NormalizeTime(pair.Key, AppSettings.DefaultMorningReminderTime),
                StringComparer.Ordinal)
            .Select(pair =>
            {
                AppDateTimeFormat.TryParseTime(pair.Key, out TimeSpan time);
                string[] segments = pair.Value.Segments().ToArray();
                // The execution date may arrive with a time component; the label always
                // shows the scheduled clock time, never the time the tick happened to run.
                string stamp = AppDateTimeFormat.DateTime(date.Date.Add(time));
                string message = $"{stamp} — {string.Join("; ", segments)}.";
                string heading = segments.Length == 1 ? segments[0] : "Pengingat gabungan";
                string key = date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "|" +
                             AppDateTimeFormat.Time(time);
                string[] dzikirSegments = pair.Value.PlainSegments
                    .Where(segment => segment is "Dzikir pagi" or "Dzikir petang")
                    .ToArray();
                string? navigationTarget = dzikirSegments.Length switch
                {
                    0 => null,
                    1 => dzikirSegments[0] == "Dzikir pagi" ? "morning" : "evening",
                    // Both dzikir reminders share one popup at this time. Open the tab
                    // without forcing either sub-tab, since both are relevant.
                    _ => "dzikir"
                };
                string? navigationLabel = navigationTarget switch
                {
                    "morning" => "Buka dzikir pagi",
                    "evening" => "Buka dzikir petang",
                    "dzikir" => "Buka Dzikir & Doa",
                    _ => null
                };
                return new AdditionalReminderGroup(key, time, heading, message, navigationTarget, navigationLabel);
            })
            .ToArray();
    }

    /// <summary>Calculates which repeat cycle is due, counting from the scheduled time.</summary>
    /// <param name="now">Current local time.</param>
    /// <param name="scheduled">Scheduled time.</param>
    /// <param name="intervalSeconds">Seconds between repeats.</param>
    /// <param name="maxCount">Maximum repeat count after the initial schedule.</param>
    /// <returns>The due cycle, capped at the maximum; -1 when the schedule is in the future.</returns>
    internal static int DueCycle(DateTime now, DateTime scheduled, int intervalSeconds, int maxCount)
    {
        double elapsed = (now - scheduled).TotalSeconds;
        if (elapsed < 0) return -1;
        int interval = Math.Clamp(intervalSeconds, 30, 3600);
        int maximum = Math.Clamp(maxCount, 0, 10);
        int cycle = (int)Math.Floor(elapsed / interval);
        return Math.Min(cycle, maximum);
    }
}
