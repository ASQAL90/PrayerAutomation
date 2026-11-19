// Pure presentation and layout logic for the prayer UI, calendar markers, and countdowns.
namespace PrayerAutomation.UI;

/// <summary>Kinds of icons used to represent prayer times in the UI.</summary>
internal enum PrayerIconKind
{
    Sun,
    Moon
}

/// <summary>Position of an item within a fixed-column grid.</summary>
/// <param name="Column">Zero-based column index.</param>
/// <param name="Row">Zero-based row index.</param>
internal readonly record struct GridCellPosition(int Column, int Row);

/// <summary>Classification of a calendar observance for marker styling and fasting rules.</summary>
internal enum CalendarObservanceKind
{
    PublicHoliday,
    ReligiousEvent,
    EidNoFast,
    SunnahFast,
    FlexibleFastWindow,
    NoFast
}

/// <summary>A single calendar observance entry, keyed by bit position in the native mask.</summary>
/// <param name="Bit">Bit index of the observance in the native mask.</param>
/// <param name="Name">Full display name of the observance.</param>
/// <param name="ShortName">Compact label used in the fixed-height date detail panel.</param>
/// <param name="Kind">Classification used for styling and fasting rules.</param>
internal readonly record struct CalendarObservance(int Bit, string Name, string ShortName, CalendarObservanceKind Kind);

/// <summary>Visual state for a calendar day cell, including colors and border thickness.</summary>
/// <param name="IsToday">True if the cell represents today.</param>
/// <param name="IsSelected">True if the cell is selected.</param>
/// <param name="IsHoliday">True if a public-holiday or Eid observance applies.</param>
/// <param name="HasSunnahFast">True if a sunnah-fast observance applies.</param>
/// <param name="HasFastingProhibition">True if fasting is prohibited on the day.</param>
/// <param name="HasReligiousEvent">True if a religious event observance applies.</param>
/// <param name="Background">Background color as a hex string.</param>
/// <param name="Border">Border color as a hex string.</param>
/// <param name="BorderThickness">Border thickness in pixels.</param>
/// <param name="MarkerColor">Marker color as a hex string.</param>
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

/// <summary>Countdown to the next prayer, including the target local time.</summary>
/// <param name="PrayerLabel">Display label of the upcoming prayer.</param>
/// <param name="LocalTime">Local time at which the prayer occurs.</param>
/// <param name="Remaining">Time remaining until the prayer.</param>
internal readonly record struct PrayerCountdown(string PrayerLabel, DateTime LocalTime, TimeSpan Remaining);

/// <summary>Action to take on the widget when its enabled setting changes.</summary>
internal enum WidgetVisibilityAction
{
    None,
    Show,
    Hide
}

/// <summary>Decides whether the widget should be shown, hidden, or left alone after a settings change.</summary>
internal static class WidgetLifecycleLogic
{
    /// <summary>Maps an enabled-state transition to the widget visibility action to perform.</summary>
    /// <param name="wasEnabled">Previous enabled state.</param>
    /// <param name="isEnabled">New enabled state.</param>
    /// <returns>The action to take.</returns>
    public static WidgetVisibilityAction ForSettingsChange(bool wasEnabled, bool isEnabled) =>
        wasEnabled == isEnabled
            ? WidgetVisibilityAction.None
            : isEnabled ? WidgetVisibilityAction.Show : WidgetVisibilityAction.Hide;
}

/// <summary>Presentation details for rendering a prayer entry (colors, icon, and accessible description).</summary>
/// <param name="Foreground">Foreground color as a hex string.</param>
/// <param name="Background">Background color as a hex string.</param>
/// <param name="IconKind">Icon to display for the entry.</param>
/// <param name="SunHeight">Sun position indicator used by sunrise/sunset visuals.</param>
/// <param name="AccessibleDescription">Screen-reader description of the entry.</param>
internal sealed record PrayerPresentation(string Foreground, string Background, PrayerIconKind IconKind, int SunHeight, string AccessibleDescription);

/// <summary>Pure presentation logic for prayer rows, countdowns, and calendar markers in the UI.</summary>
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

    /// <summary>Computes column-first grid cell positions for the given item count and column count.</summary>
    /// <param name="itemCount">Number of items to lay out.</param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <returns>Positions in item order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when arguments are out of range.</exception>
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

    /// <summary>Returns the date to use for the prayer schedule relative to a local time.</summary>
    /// <param name="locationLocalTime">Local time at the configured location.</param>
    /// <returns>The date component of the local time.</returns>
    public static DateTime PrayerScheduleDate(DateTime locationLocalTime) => locationLocalTime.Date;

    /// <summary>Returns the presentation for a prayer key, falling back to a generic style for unknown keys.</summary>
    /// <param name="key">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <returns>The matching presentation.</returns>
    public static PrayerPresentation PresentationFor(string key) =>
        Presentations.TryGetValue(key, out PrayerPresentation? presentation)
            ? presentation
            : new PrayerPresentation("#4D5966", "#F0F2F4", PrayerIconKind.Sun, 8, "Waktu ibadah");

    /// <summary>Returns the presentation for a prayer key, using the label in the accessible description for Dhuhr.</summary>
    /// <param name="key">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <param name="label">Display label to use in the accessible description.</param>
    /// <returns>The matching presentation.</returns>
    public static PrayerPresentation PresentationFor(string key, string label) =>
        key == "Dhuhr" ? PresentationFor(key) with { AccessibleDescription = $"Matahari tinggi — {label}" }
            : PresentationFor(key);

    /// <summary>Filters and orders the essential prayers (Fajr, Dhuhr, Asr, Maghrib, Isha) by time.</summary>
    /// <param name="entries">Entries to filter.</param>
    /// <returns>Essential prayer entries ordered by time.</returns>
    public static IReadOnlyList<PrayerEntry> EssentialPrayerTimes(IEnumerable<PrayerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        string[] essentialKeys = ["Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"];
        return entries
            .Where(entry => essentialKeys.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Time)
            .ToArray();
    }

    /// <summary>Finds the next essential prayer after the given local time, looking up to two days ahead.</summary>
    /// <param name="locationLocalNow">Current local time at the configured location.</param>
    /// <param name="scheduleForDate">Callback that returns the prayer entries for a given date.</param>
    /// <returns>The countdown to the next prayer, or null if none is found within two days.</returns>
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

    /// <summary>Formats a remaining time span as HH:MM:SS (clamped at zero, rounded up to whole seconds).</summary>
    /// <param name="remaining">Time remaining until the event.</param>
    /// <returns>The formatted countdown string.</returns>
    public static string FormatCountdown(TimeSpan remaining)
    {
        long totalSeconds = Math.Max(0, (long)Math.Ceiling(remaining.TotalSeconds));
        long hours = totalSeconds / 3600;
        long minutes = totalSeconds / 60 % 60;
        long seconds = totalSeconds % 60;
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    /// <summary>Builds a calendar day marker from a simple holiday flag.</summary>
    /// <param name="date">The date of the cell.</param>
    /// <param name="today">The current date, used to detect today.</param>
    /// <param name="isSelected">True if the cell is selected.</param>
    /// <param name="isHoliday">True if a public holiday applies to the date.</param>
    /// <returns>The marker visual state for the cell.</returns>
    public static CalendarMarker CalendarMarkerFor(DateTime date, DateTime today, bool isSelected, bool isHoliday) =>
        CalendarMarkerFor(date, today, isSelected, isHoliday
            ? [new CalendarObservance(0, "Hari libur", "Libur", CalendarObservanceKind.PublicHoliday)]
            : Array.Empty<CalendarObservance>());

    /// <summary>Builds a calendar day marker from the set of observances applicable to the date.</summary>
    /// <param name="date">The date of the cell.</param>
    /// <param name="today">The current date, used to detect today.</param>
    /// <param name="isSelected">True if the cell is selected.</param>
    /// <param name="observances">Observances applicable to the date.</param>
    /// <returns>The marker visual state for the cell.</returns>
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
