using System.Globalization;

namespace PrayerAutomation.UI;

/// <summary>User-facing date and time formatting used by reminders and status text.</summary>
internal static class AppDateTimeFormat
{
    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

    /// <summary>Formats an ISO date (yyyy-MM-dd) as a short Indonesian date.</summary>
    /// <param name="isoDate">Date in yyyy-MM-dd form.</param>
    /// <returns>Text such as 22 Nov 2026.</returns>
    public static string Date(string isoDate)
    {
        if (!DateOnly.TryParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateOnly date))
            return isoDate;
        return date.ToString("dd MMM yyyy", Indonesian);
    }

    /// <summary>Formats a timestamp as a short, readable Indonesian date and time.</summary>
    /// <param name="value">Local timestamp.</param>
    /// <returns>Text such as 25 Nov 2026 08.10.</returns>
    public static string DateTime(System.DateTime value) => value.ToString(@"dd MMM yyyy HH\.mm", Indonesian);

    /// <summary>Formats a time-of-day value using a dot as the separator.</summary>
    /// <param name="time">Time of day.</param>
    /// <returns>Text such as 08.10.</returns>
    public static string Time(TimeSpan time) => time.ToString(@"hh\.mm", CultureInfo.InvariantCulture);

    /// <summary>Parses a user-entered time such as 08.10 or 08:10.</summary>
    /// <param name="text">Time text from a setting field.</param>
    /// <param name="time">Parsed time of day.</param>
    /// <returns>True when the text is a valid 24-hour time.</returns>
    public static bool TryParseTime(string? text, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(text)) return false;
        string normalized = text.Trim().Replace('.', ':');
        return TimeSpan.TryParseExact(normalized, @"h\:mm", CultureInfo.InvariantCulture,
                   TimeSpanStyles.None, out time)
               || TimeSpan.TryParseExact(normalized, @"hh\:mm", CultureInfo.InvariantCulture,
                   TimeSpanStyles.None, out time)
               || TimeSpan.TryParseExact(normalized, @"h\:m", CultureInfo.InvariantCulture,
                   TimeSpanStyles.None, out time)
               || TimeSpan.TryParseExact(normalized, @"hh\:m", CultureInfo.InvariantCulture,
                   TimeSpanStyles.None, out time);
    }

    /// <summary>Normalizes a time setting to HH.mm, falling back to a default.</summary>
    /// <param name="text">Stored or entered time.</param>
    /// <param name="fallback">Fallback time text.</param>
    /// <returns>Normalized HH.mm text.</returns>
    public static string NormalizeTime(string? text, string fallback) =>
        TryParseTime(text, out TimeSpan time) ? Time(time) : fallback;
}
