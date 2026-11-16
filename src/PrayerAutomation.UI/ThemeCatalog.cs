namespace PrayerAutomation.UI;

internal sealed record ThemePalette(
    string Id,
    string Name,
    string Canvas,
    string Surface,
    string SurfaceAlt,
    string PrimaryText,
    string MutedText,
    string Border,
    string Accent,
    string AccentText,
    string ErrorText,
    string SuccessText,
    string WarningText,
    string NoteText,
    string TodayBackground,
    string WidgetBackground,
    string WidgetText,
    string WidgetMuted,
    string WidgetAccent,
    IReadOnlyList<string> PrayerColors,
    double CornerRadius);

internal readonly record struct CalendarThemeColors(
    string Background,
    string Foreground,
    string Border,
    double BorderThickness,
    string Marker);

internal static class ThemeCatalog
{
    private static readonly IReadOnlyList<ThemePalette> Themes = Array.AsReadOnly(new[]
    {
        new ThemePalette("light", "Light", "#f3f5f7", "#ffffff", "#edf1f5", "#1c2430", "#586779", "#cbd3dc",
            "#1d4ed8", "#ffffff", "#9e2525", "#0e705a", "#8a4b00", "#5e4a9a", "#e2edfc",
            "#17202b", "#f7f9fc", "#c8d3e0", "#8bbcff",
            new[] { "#6543b5", "#8b4a00", "#146b5a", "#165f99", "#813f90", "#a43b2b", "#36548a" }, 12),
        new ThemePalette("dark", "Dark", "#0e1116", "#171c24", "#202733", "#f1f5f9", "#b6c0ce", "#394453",
            "#5ea2ff", "#101826", "#ff8a80", "#71e6bb", "#ffca73", "#c7b3ff", "#1c2c42",
            "#090c11", "#f8fafc", "#c1cad6", "#86baff",
            new[] { "#cab8ff", "#ffc857", "#7ce3c3", "#75bdff", "#e9a6ff", "#ff9b86", "#a9bcff" }, 12),
        new ThemePalette("classic", "Windows Classic", "#c0c0c0", "#efefef", "#d8d8d8", "#111111", "#303030", "#777777",
            "#000080", "#ffffff", "#800000", "#005a00", "#754000", "#000080", "#d8d8d8",
            "#d8d8d8", "#101010", "#333333", "#000080",
            new[] { "#4b0082", "#7a3e00", "#005a4a", "#003c8f", "#5d2a74", "#8a1d1d", "#1c286a" }, 2),
        new ThemePalette("xp", "Windows XP Luna", "#d7e7f7", "#f5f9fe", "#e4effa", "#0e2b4d", "#355675", "#8aafd2",
            "#1d5ba8", "#ffffff", "#a12626", "#0a704f", "#8a5300", "#205b9a", "#dbeafa",
            "#e9f3fd", "#10365d", "#365d83", "#174f92",
            new[] { "#553399", "#8b4d00", "#146b53", "#155a99", "#6f3b8a", "#a3382a", "#345685" }, 10),
        new ThemePalette("macos", "macOS", "#f5f5f7", "#ffffff", "#eeeeF2", "#1d1d1f", "#626268", "#d3d3d8",
            "#0069d9", "#ffffff", "#b42318", "#18794e", "#8a5a00", "#5856d6", "#eaf2fc",
            "#ffffff", "#1d1d1f", "#5e5e65", "#0055b8",
            new[] { "#6e40c9", "#9c4a00", "#116b54", "#1263a5", "#7c3aa3", "#ad3a2a", "#36548a" }, 14),
        new ThemePalette("adwaita", "Linux Adwaita", "#f6f5f4", "#ffffff", "#eeedec", "#241f31", "#5e5c64", "#c9c7c5",
            "#1c5faa", "#ffffff", "#c01c28", "#136f3e", "#8c5a00", "#5e5c64", "#e6f0fa",
            "#ffffff", "#241f31", "#5e5c64", "#1c5faa",
            new[] { "#6135a6", "#8d4a00", "#146b52", "#145e99", "#733891", "#a43a2a", "#39588a" }, 14),
        new ThemePalette("nord", "Nord", "#2e3440", "#3b4252", "#434c5e", "#eceff4", "#c4cbd5", "#596579",
            "#88c0d0", "#1b222c", "#ffaaaa", "#c0dca8", "#ebcb8b", "#e0bfd9", "#485365",
            "#242a35", "#eceff4", "#c4cbd5", "#8fcede",
            new[] { "#c4b5fd", "#f2c57c", "#bad4a4", "#9ccddc", "#ddbad8", "#f4b49f", "#aac5df" }, 12),
        new ThemePalette("dracula", "Dracula", "#21222c", "#282a36", "#343746", "#f8f8f2", "#c2bdcf", "#53566a",
            "#bd93f9", "#1e172c", "#ff9494", "#50fa7b", "#ffb86c", "#bd93f9", "#383042",
            "#191a23", "#f8f8f2", "#c2bdcf", "#d0adff",
            new[] { "#ddb7ff", "#ffc857", "#67e6a4", "#85e7ff", "#ff8dce", "#ff807a", "#b7a1ff" }, 12)
    });

    private static readonly string[] PrayerKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];

    public static IReadOnlyList<ThemePalette> All => Themes;

    public static string NormalizeId(string? id) => Themes.FirstOrDefault(theme =>
        string.Equals(theme.Id, id?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id ?? "light";

    public static ThemePalette Get(string? id) => Themes.First(theme =>
        string.Equals(theme.Id, NormalizeId(id), StringComparison.Ordinal));

    public static string PrayerColor(ThemePalette palette, string key)
    {
        int index = Array.FindIndex(PrayerKeys, prayerKey => string.Equals(prayerKey, key, StringComparison.OrdinalIgnoreCase));
        return palette.PrayerColors[index >= 0 && index < palette.PrayerColors.Count ? index : 0];
    }

    public static CalendarThemeColors CalendarColorsFor(ThemePalette palette, CalendarMarker marker)
    {
        bool hasObservance = marker.IsHoliday || marker.HasFastingProhibition || marker.HasSunnahFast || marker.HasReligiousEvent;
        string categoryColor = marker.IsHoliday ? palette.ErrorText
            : marker.HasFastingProhibition ? palette.WarningText
            : marker.HasSunnahFast ? palette.SuccessText
            : marker.HasReligiousEvent ? palette.NoteText
            : palette.Border;
        string background = marker.IsSelected ? palette.Accent
            : marker.IsToday ? palette.TodayBackground
            : hasObservance ? palette.SurfaceAlt
            : palette.Surface;
        string foreground = marker.IsSelected ? palette.AccentText
            : marker.IsHoliday ? palette.ErrorText
            : marker.HasFastingProhibition ? palette.WarningText
            : marker.HasSunnahFast ? palette.SuccessText
            : marker.HasReligiousEvent ? palette.NoteText
            : marker.IsToday ? palette.Accent
            : palette.PrimaryText;
        string border = marker.IsToday || marker.IsSelected ? palette.Accent
            : hasObservance ? categoryColor
            : palette.Border;
        string markerColor = marker.IsSelected ? palette.AccentText : categoryColor;
        return new CalendarThemeColors(background, foreground, border, marker.BorderThickness, markerColor);
    }
}
