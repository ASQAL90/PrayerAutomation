namespace PrayerAutomation.UI;

internal sealed record PrayerEntry(string Key, string Label, TimeSpan Time);

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

    public static int GetUtcOffsetMinutes(DateTime date, string timezoneId)
    {
        TimeZoneInfo zone = ResolveTimeZone(timezoneId);
        DateTime localDate = DateTime.SpecifyKind(date.Date.AddHours(12), DateTimeKind.Unspecified);
        return checked((int)zone.GetUtcOffset(localDate).TotalMinutes);
    }

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

    public static IReadOnlyList<PrayerEntry> ReminderEntries(IReadOnlyList<PrayerEntry> entries) =>
        entries.Where(entry => entry.Key is "Fajr" or "Dhuhr" or "Asr" or "Maghrib" or "Isha").ToArray();
}
