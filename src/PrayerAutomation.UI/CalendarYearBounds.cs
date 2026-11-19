// Selectable year ranges for the Gregorian and Hijri calendar tabs.
using System;

namespace PrayerAutomation.UI;

/// <summary>Inclusive range of years offered by a calendar year selector.</summary>
/// <param name="First">First selectable year.</param>
/// <param name="Last">Last selectable year.</param>
internal readonly record struct CalendarYearRange(int First, int Last)
{
    /// <summary>Returns true when <paramref name="year"/> is inside the range.</summary>
    /// <param name="year">Year to test.</param>
    /// <returns>True when selectable.</returns>
    internal bool Contains(int year) => year >= First && year <= Last;

    /// <summary>Clamps a year into the range.</summary>
    /// <param name="year">Year to clamp.</param>
    /// <returns>The year itself or the nearest bound.</returns>
    internal int Clamp(int year) => Math.Clamp(year, First, Last);
}

/// <summary>
/// Keeps the year selectors inside the range each calendar source can actually render.
/// The civil calendar covers a wide span, while the embedded KHGT table only covers
/// 1440 H onwards, so KHGT is the stricter source whenever it is selected.
/// </summary>
internal static class CalendarYearBounds
{
    /// <summary>Years selectable before and after the current year for the civil calendar.</summary>
    private const int CivilSpanYears = 100;
    /// <summary>Last Hijri year the civil converter accepts.</summary>
    private const int CivilLastHijriYear = 9_666;
    /// <summary>Used when the native core cannot be probed; matches the current KHGT table.</summary>
    private static readonly CalendarYearRange KhgtFallbackHijri = new(NativePrayerCore.KhgtFirstYear, 1_498);
    /// <summary>Used when the native core cannot be probed; matches the current KHGT table.</summary>
    private static readonly CalendarYearRange KhgtFallbackGregorian = new(2_019, 2_075);

    /// <summary>Cached KHGT coverage; the table is fixed for the life of the process.</summary>
    private static (CalendarYearRange Hijri, CalendarYearRange Gregorian)? _khgtRange;

    /// <summary>Returns the selectable Hijri years for the active source.</summary>
    /// <param name="useKhgt">True when the official KHGT table is the active source.</param>
    /// <param name="currentHijriYear">Today's Hijri year, used to centre the civil range.</param>
    /// <returns>The selectable range.</returns>
    internal static CalendarYearRange Hijri(bool useKhgt, int currentHijriYear) => useKhgt
        ? KhgtRange().Hijri
        : new(Math.Max(1, currentHijriYear - CivilSpanYears),
            Math.Min(CivilLastHijriYear, currentHijriYear + CivilSpanYears));

    /// <summary>Returns the selectable Gregorian years for the active source.</summary>
    /// <param name="useKhgt">True when the official KHGT table is the active source.</param>
    /// <param name="currentGregorianYear">Today's Gregorian year, used to centre the civil range.</param>
    /// <returns>The selectable range.</returns>
    internal static CalendarYearRange Gregorian(bool useKhgt, int currentGregorianYear) => useKhgt
        ? KhgtRange().Gregorian
        : new(Math.Max(1, currentGregorianYear - CivilSpanYears),
            Math.Min(9_999, currentGregorianYear + CivilSpanYears));

    /// <summary>Returns the KHGT coverage, probing the table once and caching the result.</summary>
    /// <returns>The Hijri and Gregorian ranges fully covered by the table.</returns>
    private static (CalendarYearRange Hijri, CalendarYearRange Gregorian) KhgtRange()
    {
        _khgtRange ??= ProbeKhgtRange();
        return _khgtRange.Value;
    }

    /// <summary>
    /// Derives the KHGT bounds from the table itself so a refreshed table widens the
    /// selectors automatically. Falls back to the current table's bounds on failure.
    /// </summary>
    /// <returns>The Hijri and Gregorian ranges fully covered by the table.</returns>
    private static (CalendarYearRange Hijri, CalendarYearRange Gregorian) ProbeKhgtRange()
    {
        try
        {
            NativeHijriMonth firstMonth = NativePrayerCore.HijriMonth(NativePrayerCore.KhgtFirstYear, 1, useKhgt: true);
            DateTime coverageStart = new(firstMonth.GregorianYear, firstMonth.GregorianMonth, firstMonth.GregorianDay);

            // A year is only offered when all twelve months exist; the final table year
            // is usually incomplete and would fail the whole-year render.
            int lastCompleteYear = NativePrayerCore.KhgtLastYear;
            while (lastCompleteYear > NativePrayerCore.KhgtFirstYear && !HasAllMonths(lastCompleteYear))
                lastCompleteYear--;

            DateTime coverageEnd = FindCoverageEnd();
            int firstGregorianYear = coverageStart.Month == 1 && coverageStart.Day == 1
                ? coverageStart.Year
                : coverageStart.Year + 1;
            int lastGregorianYear = coverageEnd.Month == 12 && coverageEnd.Day == 31
                ? coverageEnd.Year
                : coverageEnd.Year - 1;

            if (lastCompleteYear < NativePrayerCore.KhgtFirstYear)
                throw new InvalidOperationException("Tabel KHGT tidak memuat satu pun tahun lengkap.");
            if (lastGregorianYear < firstGregorianYear)
                throw new InvalidOperationException("Tabel KHGT tidak memuat satu pun tahun Masehi lengkap.");

            return (new CalendarYearRange(NativePrayerCore.KhgtFirstYear, lastCompleteYear),
                new CalendarYearRange(firstGregorianYear, lastGregorianYear));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException
            or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
            or TypeInitializationException)
        {
            AppLogger.Write("calendar.year-bounds", ex);
            return (KhgtFallbackHijri, KhgtFallbackGregorian);
        }
    }

    /// <summary>Returns true when all twelve months of the given Hijri year are in the table.</summary>
    /// <param name="hijriYear">Hijri year to test.</param>
    /// <returns>True when every month loads.</returns>
    private static bool HasAllMonths(int hijriYear)
    {
        for (int month = 1; month <= 12; month++)
        {
            try
            {
                _ = NativePrayerCore.HijriMonth(hijriYear, month, useKhgt: true);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException
                or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
                or TypeInitializationException)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Finds the last day covered by the table, including the final partial month.</summary>
    /// <returns>The last covered Gregorian date.</returns>
    private static DateTime FindCoverageEnd()
    {
        for (int year = NativePrayerCore.KhgtLastYear; year >= NativePrayerCore.KhgtFirstYear; year--)
        {
            for (int month = 12; month >= 1; month--)
            {
                try
                {
                    NativeHijriMonth info = NativePrayerCore.HijriMonth(year, month, useKhgt: true);
                    return new DateTime(info.GregorianYear, info.GregorianMonth, info.GregorianDay)
                        .AddDays(info.DaysInMonth - 1);
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException
                    or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
                    or TypeInitializationException)
                {
                    // Missing month; continue scanning backwards.
                }
            }
        }
        throw new InvalidOperationException("Tabel KHGT tidak memuat satu pun bulan.");
    }
}
