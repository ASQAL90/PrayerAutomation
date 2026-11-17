// P/Invoke wrapper around the native prayer_core library for Hijri calendar and prayer-time calculations.
using System.Runtime.InteropServices;

namespace PrayerAutomation.UI;

/// <summary>Native struct mirroring a converted Gregorian-to-Hijri calendar date.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeCalendarDate
{
    public int Status;
    public int Year;
    public int Month;
    public int Day;
    public int WeekdayMonday0;
}

/// <summary>Native struct mirroring one Hijri month plus its starting Gregorian date.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeHijriMonth
{
    public int Status;
    public int HijriYear;
    public int HijriMonth;
    public int DaysInMonth;
    public int GregorianYear;
    public int GregorianMonth;
    public int GregorianDay;
    public int WeekdayMonday0;
}

/// <summary>Native struct mirroring calculated prayer times in minutes since midnight.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativePrayerTimes
{
    public int Status;
    public int FajrMinutes;
    public int SunriseMinutes;
    public int DhuhrMinutes;
    public int AsrMinutes;
    public int MaghribMinutes;
    public int IshaMinutes;
}

/// <summary>Managed entry points and P/Invoke declarations for the native prayer_core library.</summary>
internal static class NativePrayerCore
{
    private const string Library = "prayer_core";

    /// <summary>Returns the ABI version of the loaded native library.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_abi_version", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern uint GetAbiVersion();
    /// <summary>Converts a Gregorian date to its Hijri calendar equivalent.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_gregorian_to_hijri", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int ConvertGregorianToHijri(int year, int month, int day, out NativeCalendarDate result);
    /// <summary>Loads information about a Hijri month.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_hijri_month", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int LoadHijriMonth(int year, int month, out NativeHijriMonth result);
    /// <summary>Returns a bitmask of observances applicable to the given date.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_holiday_mask", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern uint GetHolidayMask(int year, int month, int day, int hijriCorrectionDays);
    /// <summary>Calculates prayer times for the given date and location.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calculate_prayer_times", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int CalculatePrayerTimes(int year, int month, int day, double latitude, double longitude,
        int timezoneOffsetMinutes, int method, double customFajrAngle, double customIshaAngle, out NativePrayerTimes result);

    private static readonly string[] HijriMonthNames =
    ["Muharram", "Safar", "Rabiul Awal", "Rabiul Akhir", "Jumadil Awal", "Jumadil Akhir",
     "Rajab", "Syaban", "Ramadan", "Syawal", "Zulkaidah", "Zulhijah"];

    /// <summary>Verifies the native library ABI version at first use.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the ABI version does not match.</exception>
    static NativePrayerCore()
    {
        uint version = GetAbiVersion();
        if (version != 1)
            throw new InvalidOperationException($"ABI prayer-core tidak cocok (ditemukan {version}, dibutuhkan 1).");
    }

    /// <summary>Converts a Gregorian date to its Hijri equivalent.</summary>
    /// <param name="gregorianDate">Gregorian date to convert.</param>
    /// <returns>The Hijri calendar date.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails.</exception>
    public static NativeCalendarDate HijriDate(DateTime gregorianDate)
    {
        int status = ConvertGregorianToHijri(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, out var result);
        if (status != 0) throw new InvalidOperationException($"Konversi Hijriyah gagal (kode {status}).");
        return result;
    }

    /// <summary>Loads information about a Hijri month.</summary>
    /// <param name="year">Hijri year.</param>
    /// <param name="month">Hijri month (1–12).</param>
    /// <returns>The Hijri month details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails.</exception>
    public static NativeHijriMonth HijriMonth(int year, int month)
    {
        int status = LoadHijriMonth(year, month, out var result);
        if (status != 0) throw new InvalidOperationException($"Pemuatan bulan Hijriyah {year}/{month} gagal (kode {status}).");
        return result;
    }

    private static readonly CalendarObservance[] ObservanceDefinitions =
    [
        new(0, "Tahun Baru Masehi", CalendarObservanceKind.PublicHoliday),
        new(1, "Hari Buruh Internasional", CalendarObservanceKind.PublicHoliday),
        new(2, "Hari Lahir Pancasila", CalendarObservanceKind.PublicHoliday),
        new(3, "Hari Kemerdekaan Republik Indonesia", CalendarObservanceKind.PublicHoliday),
        new(4, "Hari Natal", CalendarObservanceKind.PublicHoliday),
        new(5, "Tahun Baru Islam — perkiraan kalender sipil", CalendarObservanceKind.ReligiousEvent),
        new(6, "Maulid Nabi Muhammad SAW — perkiraan; observansi setempat", CalendarObservanceKind.ReligiousEvent),
        new(7, "Isra Mikraj — perkiraan; observansi setempat", CalendarObservanceKind.ReligiousEvent),
        new(8, "Idulfitri — perkiraan; tidak berpuasa", CalendarObservanceKind.EidNoFast),
        new(9, "Iduladha — perkiraan; tidak berpuasa", CalendarObservanceKind.EidNoFast),
        new(10, "Awal Ramadan — perkiraan kalender sipil", CalendarObservanceKind.ReligiousEvent),
        new(11, "Nuzulul Qur'an — 17 Ramadan; observansi setempat", CalendarObservanceKind.ReligiousEvent),
        new(12, "Tasu'a — puasa sunnah (9 Muharram)", CalendarObservanceKind.SunnahFast),
        new(13, "Asyura — puasa sunnah (10 Muharram)", CalendarObservanceKind.SunnahFast),
        new(14, "Arafah — puasa sunnah bagi yang tidak berhaji (9 Zulhijah)", CalendarObservanceKind.SunnahFast),
        new(15, "Hari Tasyriq — tidak berpuasa (11–13 Zulhijah)", CalendarObservanceKind.NoFast),
        new(16, "Ayyamul Bidh — puasa sunnah (13–15 Hijriyah)", CalendarObservanceKind.SunnahFast),
        new(17, "Jendela Puasa Syawal — pilih 6 hari (2–30 Syawal)", CalendarObservanceKind.FlexibleFastWindow),
        new(18, "Puasa sunnah Senin", CalendarObservanceKind.SunnahFast),
        new(19, "Puasa sunnah Kamis", CalendarObservanceKind.SunnahFast),
        new(20, "Nisfu Sya'ban — observansi dapat berbeda", CalendarObservanceKind.ReligiousEvent),
        new(21, "Puasa sunnah awal Zulhijah (1–8)", CalendarObservanceKind.SunnahFast)
    ];

    /// <summary>Returns the names of observances applicable to the given date.</summary>
    /// <param name="date">Gregorian date to inspect.</param>
    /// <param name="hijriCorrectionDays">Correction applied to Hijri-based observances, in days.</param>
    /// <returns>Names of the observances for the date.</returns>
    public static string[] HolidayNames(DateTime date, int hijriCorrectionDays) =>
        CalendarObservances(date, hijriCorrectionDays).Select(item => item.Name).ToArray();

    /// <summary>Returns the observances applicable to the given date.</summary>
    /// <param name="date">Gregorian date to inspect.</param>
    /// <param name="hijriCorrectionDays">Correction applied to Hijri-based observances, in days.</param>
    /// <returns>Observances for the date.</returns>
    public static IReadOnlyList<CalendarObservance> CalendarObservances(DateTime date, int hijriCorrectionDays) =>
        CalendarObservancesFromMask(GetHolidayMask(date.Year, date.Month, date.Day, hijriCorrectionDays));

    /// <summary>Maps a native observance bitmask to the matching observance definitions.</summary>
    /// <param name="mask">Bitmask returned by the native library.</param>
    /// <returns>Observances whose bits are set in the mask.</returns>
    internal static IReadOnlyList<CalendarObservance> CalendarObservancesFromMask(uint mask) =>
        ObservanceDefinitions.Where(item => (mask & (1u << item.Bit)) != 0).ToArray();

    /// <summary>Calculates prayer times for the given date and location.</summary>
    /// <param name="date">Gregorian date to calculate.</param>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="timezoneOffsetMinutes">UTC offset of the location in minutes.</param>
    /// <param name="method">Calculation method identifier.</param>
    /// <param name="customFajrAngle">Custom Fajr angle when the method supports it.</param>
    /// <param name="customIshaAngle">Custom Isha angle when the method supports it.</param>
    /// <returns>Calculated prayer times in minutes since midnight.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails.</exception>
    public static NativePrayerTimes PrayerTimes(DateTime date, double latitude, double longitude,
        int timezoneOffsetMinutes, int method, double customFajrAngle = 18, double customIshaAngle = 18)
    {
        int status = CalculatePrayerTimes(date.Year, date.Month, date.Day, latitude, longitude,
            timezoneOffsetMinutes, method, customFajrAngle, customIshaAngle, out var result);
        if (status != 0) throw new InvalidOperationException($"Perhitungan waktu sholat gagal (kode {status}).");
        return result;
    }

    /// <summary>Returns the display name of a Hijri month.</summary>
    /// <param name="month">Hijri month (1–12).</param>
    /// <returns>The month name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the month is not within 1–12.</exception>
    public static string HijriMonthName(int month) =>
        month is >= 1 and <= 12 ? HijriMonthNames[month - 1] : throw new ArgumentOutOfRangeException(nameof(month));
}
