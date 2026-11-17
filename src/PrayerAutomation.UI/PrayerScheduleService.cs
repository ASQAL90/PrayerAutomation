// Builds daily prayer-time entries from native calculations and user settings.
namespace PrayerAutomation.UI;

/// <summary>A single scheduled prayer (or related) time for a day.</summary>
/// <param name="Key">Stable identifier of the entry (e.g. Fajr, Dhuhr).</param>
/// <param name="Label">Display label for the entry.</param>
/// <param name="Time">Time of day for the entry.</param>
internal sealed record PrayerEntry(string Key, string Label, TimeSpan Time);

/// <summary>Computes and filters daily prayer schedules from user settings and native calculations.</summary>
internal static class PrayerScheduleService
{
    private static readonly (string Key, string Label, Func<NativePrayerTimes, int> Minutes)[] PrayerOrder =
    [
        ("Fajr", "Subuh", time => time.FajrMinutes),
        ("Sunrise", "Terbit", time => time.SunriseMinutes),
        ("Dhuha", "Duha", time => time.SunriseMinutes),
        ("Dhuhr", "Dzuhur", time => time.DhuhrMinutes),
        ("Asr", "Ashar", time => time.AsrMinutes),
        ("Maghrib", "Maghrib", time => time.MaghribMinutes),
        ("Isha", "Isya", time => time.IshaMinutes),
    ];

    /// <summary>Resolves a time zone by trying the given id, then IANA↔Windows conversions.</summary>
    /// <param name="id">Time zone identifier (IANA or Windows).</param>
    /// <returns>The resolved time zone.</returns>
    /// <exception cref="TimeZoneNotFoundException">Thrown when the id cannot be resolved.</exception>
    public static TimeZoneInfo ResolveTimeZone(string id)
    {
        string candidate = id.Trim();
        try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(candidate, out string? windowsId))
                return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            if (TimeZoneInfo.TryConvertWindowsIdToIanaId(candidate, out string? ianaId))
                return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
            throw;
        }
    }

    /// <summary>Returns the UTC offset in minutes for the given date in the given time zone.</summary>
    /// <param name="date">Date to evaluate (noon is used to avoid daylight-saving edge cases).</param>
    /// <param name="timezoneId">IANA or Windows time zone identifier.</param>
    /// <returns>UTC offset in minutes.</returns>
    public static int GetUtcOffsetMinutes(DateTime date, string timezoneId)
    {
        TimeZoneInfo zone = ResolveTimeZone(timezoneId);
        DateTime localDate = DateTime.SpecifyKind(date.Date.AddHours(12), DateTimeKind.Unspecified);
        return checked((int)zone.GetUtcOffset(localDate).TotalMinutes);
    }

    /// <summary>Builds the full list of prayer entries for a date, applying corrections and settings.</summary>
    /// <param name="date">Date to calculate.</param>
    /// <param name="settings">Application settings (location, method, corrections).</param>
    /// <returns>The prayer entries for the date.</returns>
    public static IReadOnlyList<PrayerEntry> ForDate(DateTime date, AppSettings settings)
    {
        settings.Normalize();
        int offset = GetUtcOffsetMinutes(date, settings.TimeZoneId);
        NativePrayerTimes native = NativePrayerCore.PrayerTimes(date, settings.Latitude, settings.Longitude,
            offset, settings.MethodId);
        var entries = new List<PrayerEntry>(PrayerOrder.Length);
        foreach (var item in PrayerOrder)
        {
            int minutes = item.Minutes(native);
            if (item.Key == "Dhuha") minutes += settings.DhuhaAfterSunriseMinutes;
            if (settings.Corrections.TryGetValue(item.Key, out int correction)) minutes += correction;
            string label = item.Key == "Dhuhr" && date.DayOfWeek == DayOfWeek.Friday ? "Jum'at" : item.Label;
            entries.Add(new PrayerEntry(item.Key, label, TimeSpan.FromMinutes(minutes)));
        }
        return entries;
    }

    /// <summary>Filters entries down to the essential prayers used for reminders (Fajr, Dhuhr, Asr, Maghrib, Isha).</summary>
    /// <param name="entries">Entries to filter.</param>
    /// <returns>Only the reminder-relevant entries.</returns>
    public static IReadOnlyList<PrayerEntry> ReminderEntries(IReadOnlyList<PrayerEntry> entries) =>
        entries.Where(entry => entry.Key is "Fajr" or "Dhuhr" or "Asr" or "Maghrib" or "Isha").ToArray();
}
