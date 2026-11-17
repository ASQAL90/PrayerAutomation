namespace PrayerAutomation.UI;

internal enum PrayerIconKind
{
    Sun,
    Moon
}

internal readonly record struct GridCellPosition(int Column, int Row);

internal enum CalendarObservanceKind
{
    PublicHoliday,
    ReligiousEvent,
    EidNoFast,
    SunnahFast,
    FlexibleFastWindow,
    NoFast
}

internal readonly record struct CalendarObservance(int Bit, string Name, CalendarObservanceKind Kind);

internal readonly record struct CalendarMarker(
    bool IsToday,
    bool IsSelected,
    bool IsHoliday,
    bool HasSunnahFast,
    bool HasFastingProhibition,
    bool HasReligiousEvent,
    string Background,
    string Border,
    double BorderThickness,
    string MarkerColor);

internal readonly record struct PrayerCountdown(string PrayerLabel, DateTime LocalTime, TimeSpan Remaining);

internal enum WidgetVisibilityAction
{
    None,
    Show,
    Hide
}

internal static class WidgetLifecycleLogic
{
    public static WidgetVisibilityAction ForSettingsChange(bool wasEnabled, bool isEnabled) =>
        wasEnabled == isEnabled
            ? WidgetVisibilityAction.None
            : isEnabled ? WidgetVisibilityAction.Show : WidgetVisibilityAction.Hide;
}

internal sealed record PrayerPresentation(string Foreground, string Background, PrayerIconKind IconKind, int SunHeight, string AccessibleDescription);

internal static class PrayerUiLogic
{
    private static readonly IReadOnlyDictionary<string, PrayerPresentation> Presentations =
        new Dictionary<string, PrayerPresentation>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fajr"] = new("#6554A4", "#F1EEFA", PrayerIconKind.Moon, 0, "Bulan sabit — waktu Fajar"),
            ["Sunrise"] = new("#C47A00", "#FFF5DE", PrayerIconKind.Sun, 10, "Matahari terbit"),
            ["Dhuha"] = new("#18856D", "#E8F7F2", PrayerIconKind.Sun, 8, "Matahari pagi — Duha"),
            ["Dhuhr"] = new("#2473B8", "#EAF3FC", PrayerIconKind.Sun, 2, "Matahari tinggi — Dzuhur"),
            ["Asr"] = new("#9B62A6", "#F6ECF7", PrayerIconKind.Sun, 8, "Matahari menurun — Ashar"),
            ["Maghrib"] = new("#C04B3A", "#FCEDEA", PrayerIconKind.Sun, 13, "Matahari terbenam — Maghrib"),
            ["Isha"] = new("#405B93", "#EDF1F9", PrayerIconKind.Moon, 0, "Bulan sabit — waktu Isya")
        };

    public static IReadOnlyList<GridCellPosition> ColumnFirstPositions(int itemCount, int columns)
    {
        if (itemCount < 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
        if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
        if (itemCount == 0) return [];
        int rowsPerColumn = (itemCount + columns - 1) / columns;
        return Enumerable.Range(0, itemCount)
            .Select(index => new GridCellPosition(index / rowsPerColumn, index % rowsPerColumn))
            .ToArray();
    }

    public static DateTime PrayerScheduleDate(DateTime locationLocalTime) => locationLocalTime.Date;

    public static PrayerPresentation PresentationFor(string key) =>
        Presentations.TryGetValue(key, out PrayerPresentation? presentation)
            ? presentation
            : new PrayerPresentation("#4D5966", "#F0F2F4", PrayerIconKind.Sun, 8, "Waktu ibadah");

    public static PrayerPresentation PresentationFor(string key, string label) =>
        key == "Dhuhr" ? PresentationFor(key) with { AccessibleDescription = $"Matahari tinggi — {label}" }
            : PresentationFor(key);

    public static IReadOnlyList<PrayerEntry> EssentialPrayerTimes(IEnumerable<PrayerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        string[] essentialKeys = ["Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"];
        return entries
            .Where(entry => essentialKeys.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Time)
            .ToArray();
    }

    public static PrayerCountdown? NextPrayerAfter(
        DateTime locationLocalNow,
        Func<DateTime, IReadOnlyList<PrayerEntry>> scheduleForDate)
    {
        ArgumentNullException.ThrowIfNull(scheduleForDate);
        string[] prayerKeys = ["Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"];
        for (int dayOffset = 0; dayOffset <= 2; dayOffset++)
        {
            DateTime scheduleDate = locationLocalNow.Date.AddDays(dayOffset);
            foreach (PrayerEntry entry in scheduleForDate(scheduleDate)
                .Where(entry => prayerKeys.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
                .OrderBy(entry => entry.Time))
            {
                DateTime candidate = scheduleDate.Add(entry.Time);
                if (candidate > locationLocalNow)
                    return new PrayerCountdown(entry.Label, candidate, candidate - locationLocalNow);
            }
        }
        return null;
    }

    public static string FormatCountdown(TimeSpan remaining)
    {
        long totalSeconds = Math.Max(0, (long)Math.Ceiling(remaining.TotalSeconds));
        long hours = totalSeconds / 3600;
        long minutes = totalSeconds / 60 % 60;
        long seconds = totalSeconds % 60;
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    public static CalendarMarker CalendarMarkerFor(DateTime date, DateTime today, bool isSelected, bool isHoliday) =>
        CalendarMarkerFor(date, today, isSelected, isHoliday
            ? [new CalendarObservance(0, "Hari libur", CalendarObservanceKind.PublicHoliday)]
            : Array.Empty<CalendarObservance>());

    public static CalendarMarker CalendarMarkerFor(
        DateTime date,
        DateTime today,
        bool isSelected,
        IReadOnlyCollection<CalendarObservance> observances)
    {
        bool isToday = date.Date == today.Date;
        bool isHoliday = observances.Any(item => item.Kind is CalendarObservanceKind.PublicHoliday or CalendarObservanceKind.EidNoFast);
        bool hasProhibition = observances.Any(item => item.Kind is CalendarObservanceKind.EidNoFast or CalendarObservanceKind.NoFast);
        bool hasSunnahFast = observances.Any(item => item.Kind is CalendarObservanceKind.SunnahFast or CalendarObservanceKind.FlexibleFastWindow);
        bool hasReligiousEvent = observances.Any(item => item.Kind is CalendarObservanceKind.ReligiousEvent or CalendarObservanceKind.EidNoFast or CalendarObservanceKind.NoFast);
        string markerColor = isHoliday ? "#C62828"
            : hasProhibition ? "#B36B00"
            : hasSunnahFast ? "#18856D"
            : hasReligiousEvent ? "#6554A4"
            : "#D8E1EA";
        string background = isSelected || isToday
            ? "#E5F1FC"
            : isHoliday ? "#FFF0F0"
            : hasProhibition ? "#FFF5E1"
            : hasSunnahFast ? "#E8F7F2"
            : hasReligiousEvent ? "#F1EEFA"
            : "#FFFFFF";
        string border = isToday
            ? "#0067B8"
            : isSelected ? "#245B86" : markerColor;
        double thickness = isToday ? 3 : isSelected || observances.Count > 0 ? 2 : 1;
        return new CalendarMarker(isToday, isSelected, isHoliday, hasSunnahFast, hasProhibition,
            hasReligiousEvent, background, border, thickness, markerColor);
    }
}
