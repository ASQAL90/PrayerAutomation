using System.Globalization;

namespace PrayerAutomation.UI;

/// <summary>Builds the marquee line text from date, Hijri date, prayer times, countdown, and observances.</summary>
internal static class WidgetMarqueeContent
{
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");
    private static readonly (string Meaning, string Reference, string SourceUrl)[] ReligiousMessages =
    [
        ("Ganjaran amal bergantung pada niat; setiap orang memperoleh sesuai niatnya.",
            "Sahih al-Bukhari 1", "https://sunnah.com/bukhari:1"),
        ("Amal yang paling dicintai Allah adalah yang dilakukan terus-menerus, walaupun sedikit.",
            "Sahih al-Bukhari 6464", "https://sunnah.com/bukhari:6464"),
        ("Mudahkanlah, jangan mempersulit; berilah kabar gembira, jangan membuat orang menjauh.",
            "Sahih al-Bukhari 69", "https://sunnah.com/bukhari:69"),
        ("Allah Maha Lembut dan mencintai kelembutan.",
            "Sahih Muslim 2593", "https://sunnah.com/muslim:2593"),
        ("Setiap perbuatan baik adalah sedekah.",
            "Sahih Muslim 1005", "https://sunnah.com/muslim:1005")
    ];

    /// <summary>Returns the rotating religious message for the given completed marquee cycle count.</summary>
    /// <param name="completedCycles">Number of completed marquee cycles.</param>
    /// <returns>The message text including reference and source URL.</returns>
    internal static string ReligiousMessageAt(long completedCycles)
    {
        int index = (int)(completedCycles % ReligiousMessages.Length);
        if (index < 0) index += ReligiousMessages.Length;
        var message = ReligiousMessages[index];
        return $"Hadis (ringkasan makna Indonesia): {message.Meaning} — {message.Reference} | {message.SourceUrl}";
    }

    /// <summary>Builds the full marquee content string.</summary>
    /// <param name="now">Location-local current time.</param>
    /// <param name="settings">Application settings.</param>
    /// <param name="hijri">Hijri date to display.</param>
    /// <param name="schedule">Prayer times for today.</param>
    /// <param name="countdown">The next-prayer countdown, if available.</param>
    /// <param name="observances">Calendar observances for today.</param>
    /// <param name="message">Optional religious message override.</param>
    /// <returns>The combined marquee text.</returns>
    internal static string Build(DateTime now, AppSettings settings, NativeCalendarDate hijri,
        IReadOnlyList<PrayerEntry> schedule, PrayerCountdown? countdown,
        IReadOnlyList<CalendarObservance> observances, string? message = null)
    {
        DateTime calendarDate = now.Date;
        string calendarLabel = settings.UsesKhgtCalendar ? "KHGT, perkiraan" : "sipil, perkiraan";
        var parts = new List<string>
        {
            $"Masehi: {calendarDate.ToString("dddd, d MMMM yyyy", IndonesianCulture)}",
            $"Hijriyah: {hijri.Day} {NativePrayerCore.HijriMonthName(hijri.Month)} {hijri.Year} H ({calendarLabel})",
            $"Jam: {now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)} ({settings.TimeZoneId})",
            string.Join("   •   ", PrayerUiLogic.EssentialPrayerTimes(schedule).Select(entry =>
                $"{entry.Label} {entry.Time.ToString(@"hh\:mm", CultureInfo.InvariantCulture)}")),
            countdown is PrayerCountdown next
                ? $"Berikutnya: {next.PrayerLabel} {next.LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture)} — {PrayerUiLogic.FormatCountdown(next.Remaining)} lagi"
                : "Berikutnya: jadwal belum tersedia",
            $"Lokasi: {settings.LocationName}"
        };
        CalendarObservance[] fixedHolidays = observances.Where(item => item.Kind == CalendarObservanceKind.PublicHoliday).ToArray();
        if (fixedHolidays.Length > 0)
            parts.Add("Libur nasional Indonesia (tanggal tetap): " + string.Join("; ", fixedHolidays.Select(item => item.Name)));
        CalendarObservance[] religious = observances.Where(item => item.Kind != CalendarObservanceKind.PublicHoliday).ToArray();
        if (hijri.Month == 9)
            parts.Add("Ramadan — puasa wajib (tanggal perkiraan; ikuti penetapan setempat)");
        if (religious.Length > 0)
            parts.Add($"Penanda ibadah ({calendarLabel}, bukan penetapan resmi): " + string.Join("; ", religious.Select(item => item.Name)));
        message ??= ReligiousMessageAt(0);
        if (!string.IsNullOrWhiteSpace(message)) parts.Add(message);
        return string.Join("   •   ", parts);
    }
}
