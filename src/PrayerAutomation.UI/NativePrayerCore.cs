// P/Invoke wrapper around the native prayer_core library for Hijri calendar and prayer-time calculations.
using System.Reflection;
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
    /// <summary>Converts a Gregorian date through the official KHGT month table.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_gregorian_to_hijri_khgt", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int ConvertGregorianToHijriKhgt(int year, int month, int day, out NativeCalendarDate result);
    /// <summary>Loads information about a Hijri month.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_hijri_month", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int LoadHijriMonth(int year, int month, out NativeHijriMonth result);
    /// <summary>Loads a KHGT Hijri month from the official month table.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_hijri_month_khgt", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int LoadHijriMonthKhgt(int year, int month, out NativeHijriMonth result);
    /// <summary>Returns a bitmask of observances applicable to the given date.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_holiday_mask", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern uint GetHolidayMask(int year, int month, int day, int hijriCorrectionDays);
    /// <summary>Returns an observance bitmask using the official KHGT date.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calendar_holiday_mask_khgt", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int GetHolidayMaskKhgt(int year, int month, int day, int hijriCorrectionDays, out uint mask);
    /// <summary>Calculates prayer times for the given date and location.</summary>
    [DllImport(Library, EntryPoint = "prayer_core_calculate_prayer_times", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int CalculatePrayerTimes(int year, int month, int day, double latitude, double longitude,
        int timezoneOffsetMinutes, int method, double customFajrAngle, double customIshaAngle,
        double elevationMeters, out NativePrayerTimes result);

    private static readonly string[] HijriMonthNames =
    ["Muharram", "Safar", "Rabiul Awal", "Rabiul Akhir", "Jumadil Awal", "Jumadil Akhir",
     "Rajab", "Syaban", "Ramadan", "Syawal", "Zulkaidah", "Zulhijah"];

    /// <summary>Verifies the native library ABI version at first use.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the ABI version does not match.</exception>
    static NativePrayerCore()
    {
        // PA-05: resolve prayer_core ourselves so the load is limited to
        // NativeLibraryPath (the application folder). This runs before the
        // first native call below, so no default probe can happen first.
        NativeLibrary.SetDllImportResolver(typeof(NativePrayerCore).Assembly, ResolveNativeLibrary);
        uint version = GetAbiVersion();
        if (version != 2)
            throw new InvalidOperationException($"ABI prayer-core tidak cocok (ditemukan {version}, dibutuhkan 2).");
    }

    /// <summary>Loads prayer_core from the application folder, and nothing else.</summary>
    /// <param name="name">Library name from the <c>DllImport</c> attribute.</param>
    /// <param name="assembly">Assembly requesting the symbol; unused.</param>
    /// <param name="searchPath">Default search path; deliberately ignored.</param>
    /// <returns>Handle for prayer_core; zero for every other native library.</returns>
    /// <exception cref="DllNotFoundException">Thrown when the validated file is missing.</exception>
    private static IntPtr ResolveNativeLibrary(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        _ = assembly;
        _ = searchPath;
        if (!string.Equals(name, NativeLibraryPath.LibraryName, StringComparison.Ordinal)) return IntPtr.Zero;
        string? path = NativeLibraryPath.Locate(AppContext.BaseDirectory);
        // Failing loudly here is intentional: returning zero would hand the
        // request back to the default probe, which is what PA-05 removes.
        if (path is null)
            throw new DllNotFoundException(
                $"prayer_core tidak ditemukan di folder aplikasi ({AppContext.BaseDirectory}). Instalasi tidak lengkap.");
        return NativeLibrary.Load(path);
    }

    /// <summary>First Hijri year in the embedded official KHGT table.</summary>
    public const int KhgtFirstYear = 1440;
    /// <summary>Last Hijri year in the embedded official KHGT table.</summary>
    public const int KhgtLastYear = 1499;

    /// <summary>Converts a Gregorian date to its Hijri equivalent.</summary>
    /// <param name="gregorianDate">Gregorian date to convert.</param>
    /// <param name="useKhgt">Whether to use the official KHGT table instead of the civil calendar.</param>
    /// <returns>The Hijri calendar date.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails or the date is outside KHGT coverage.</exception>
    public static NativeCalendarDate HijriDate(DateTime gregorianDate, bool useKhgt = false)
    {
        int status = useKhgt
            ? ConvertGregorianToHijriKhgt(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, out var result)
            : ConvertGregorianToHijri(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, out result);
        if (status != 0)
            throw new InvalidOperationException(useKhgt && status == 1
                ? $"Tanggal di luar cakupan tabel KHGT ({KhgtFirstYear}–{KhgtLastYear} H)."
                : $"Konversi Hijriyah gagal (kode {status}).");
        return result;
    }

    /// <summary>Tries to convert a Gregorian date without throwing.</summary>
    /// <param name="gregorianDate">Gregorian date to convert.</param>
    /// <param name="useKhgt">Whether to use the official KHGT table.</param>
    /// <param name="result">The converted date when successful.</param>
    /// <returns>True when the date has a Hijri equivalent for the selected source.</returns>
    public static bool TryHijriDate(DateTime gregorianDate, bool useKhgt, out NativeCalendarDate result)
    {
        int status = useKhgt
            ? ConvertGregorianToHijriKhgt(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, out result)
            : ConvertGregorianToHijri(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, out result);
        return status == 0;
    }

    /// <summary>Loads information about a Hijri month.</summary>
    /// <param name="year">Hijri year.</param>
    /// <param name="month">Hijri month (1–12).</param>
    /// <param name="useKhgt">Whether to use the official KHGT table instead of the civil calendar.</param>
    /// <returns>The Hijri month details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails or KHGT does not cover the month.</exception>
    public static NativeHijriMonth HijriMonth(int year, int month, bool useKhgt = false)
    {
        int status = useKhgt ? LoadHijriMonthKhgt(year, month, out var result) : LoadHijriMonth(year, month, out result);
        if (status != 0)
            throw new InvalidOperationException(useKhgt
                ? $"Bulan {year}/{month} H tidak tersedia di tabel KHGT resmi ({KhgtFirstYear}–{KhgtLastYear} H)."
                : $"Pemuatan bulan Hijriyah {year}/{month} gagal (kode {status}).");
        return result;
    }

    private static readonly CalendarObservance[] ObservanceDefinitions =
    [
        new(0, "Tahun Baru Masehi", "Tahun Baru Masehi", CalendarObservanceKind.PublicHoliday),
        new(1, "Hari Buruh Internasional", "Hari Buruh", CalendarObservanceKind.PublicHoliday),
        new(2, "Hari Lahir Pancasila", "Pancasila", CalendarObservanceKind.PublicHoliday),
        new(3, "Hari Kemerdekaan Republik Indonesia", "Kemerdekaan RI", CalendarObservanceKind.PublicHoliday),
        new(4, "Hari Natal", "Natal", CalendarObservanceKind.PublicHoliday),
        new(5, "Tahun Baru Islam — perkiraan", "Tahun Baru Islam", CalendarObservanceKind.ReligiousEvent),
        new(6, "Maulid Nabi Muhammad SAW — perkiraan; observansi setempat", "Maulid Nabi", CalendarObservanceKind.ReligiousEvent),
        new(7, "Isra Mikraj — perkiraan; observansi setempat", "Isra Mikraj", CalendarObservanceKind.ReligiousEvent),
        new(8, "Idulfitri — perkiraan; tidak berpuasa", "Idulfitri", CalendarObservanceKind.EidNoFast),
        new(9, "Iduladha — perkiraan; tidak berpuasa", "Iduladha", CalendarObservanceKind.EidNoFast),
        new(10, "Awal Ramadan — perkiraan", "Awal Ramadan", CalendarObservanceKind.ReligiousEvent),
        new(11, "Nuzulul Qur'an — 17 Ramadan; observansi setempat", "Nuzulul Qur'an", CalendarObservanceKind.ReligiousEvent),
        new(12, "Tasu'a — puasa sunnah (9 Muharram)", "Tasu'a", CalendarObservanceKind.SunnahFast),
        new(13, "Asyura — puasa sunnah (10 Muharram)", "Asyura", CalendarObservanceKind.SunnahFast),
        new(14, "Arafah — puasa sunnah bagi yang tidak berhaji (9 Zulhijah)", "Arafah", CalendarObservanceKind.SunnahFast),
        new(15, "Hari Tasyriq — tidak berpuasa (11–13 Zulhijah)", "Tasyrik", CalendarObservanceKind.NoFast),
        new(16, "Ayyamul Bidh — puasa sunnah (13–15 Hijriyah)", "Ayyamul Bidh", CalendarObservanceKind.SunnahFast),
        new(17, "Jendela Puasa Syawal — pilih 6 hari (2–30 Syawal)", "Puasa Syawal (6 hari)", CalendarObservanceKind.FlexibleFastWindow),
        new(18, "Puasa sunnah Senin", "Puasa Senin", CalendarObservanceKind.SunnahFast),
        new(19, "Puasa sunnah Kamis", "Puasa Kamis", CalendarObservanceKind.SunnahFast),
        new(20, "Nisfu Sya'ban — observansi dapat berbeda", "Nisfu Sya'ban", CalendarObservanceKind.ReligiousEvent),
        new(21, "Puasa sunnah awal Zulhijah (1–8)", "Puasa awal Zulhijah", CalendarObservanceKind.SunnahFast)
    ];

    /// <summary>Returns the names of observances applicable to the given date.</summary>
    /// <param name="date">Gregorian date to inspect.</param>
    /// <param name="hijriCorrectionDays">Correction applied to Hijri-based observances, in days.</param>
    /// <param name="useKhgt">Whether to use the official KHGT table.</param>
    /// <returns>Names of the observances for the date.</returns>
    public static string[] HolidayNames(DateTime date, int hijriCorrectionDays, bool useKhgt = false) =>
        CalendarObservances(date, hijriCorrectionDays, useKhgt).Select(item => item.Name).ToArray();

    /// <summary>Returns the observances applicable to the given date.</summary>
    /// <param name="date">Gregorian date to inspect.</param>
    /// <param name="hijriCorrectionDays">Correction applied to Hijri-based observances, in days.</param>
    /// <param name="useKhgt">Whether to use the official KHGT table, with a civil fallback outside its range.</param>
    /// <returns>Observances for the date.</returns>
    public static IReadOnlyList<CalendarObservance> CalendarObservances(DateTime date, int hijriCorrectionDays,
        bool useKhgt = false)
    {
        if (useKhgt && GetHolidayMaskKhgt(date.Year, date.Month, date.Day, hijriCorrectionDays, out uint khgtMask) == 0)
            return CalendarObservancesFromMask(khgtMask);
        return CalendarObservancesFromMask(GetHolidayMask(date.Year, date.Month, date.Day, hijriCorrectionDays));
    }

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
    /// <param name="elevationMeters">Observer height above sea level in metres (0–9000).</param>
    /// <returns>Calculated prayer times in minutes since midnight.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the native call fails.</exception>
    public static NativePrayerTimes PrayerTimes(DateTime date, double latitude, double longitude,
        int timezoneOffsetMinutes, int method, double customFajrAngle = 18, double customIshaAngle = 18,
        double elevationMeters = 0)
    {
        int status = CalculatePrayerTimes(date.Year, date.Month, date.Day, latitude, longitude,
            timezoneOffsetMinutes, method, customFajrAngle, customIshaAngle, elevationMeters, out var result);
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
