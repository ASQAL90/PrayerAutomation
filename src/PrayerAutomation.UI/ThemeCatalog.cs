// Built-in theme palettes and calendar theming derived from markers and palettes.
using System;

namespace PrayerAutomation.UI;

/// <summary>Full set of colors and metrics for one application theme.</summary>
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

/// <summary>Resolved colors for rendering a calendar day cell.</summary>
/// <param name="Background">Background color as a hex string.</param>
/// <param name="Foreground">Foreground color as a hex string.</param>
/// <param name="Border">Border color as a hex string.</param>
/// <param name="BorderThickness">Border thickness in pixels.</param>
/// <param name="Marker">Marker color as a hex string.</param>
internal readonly record struct CalendarThemeColors(
    string Background,
    string Foreground,
    string Border,
    double BorderThickness,
    string Marker);

/// <summary>Catalog of the built-in theme palettes and helpers to resolve and apply them.</summary>
internal static class ThemeCatalog
{
    private static readonly IReadOnlyList<ThemePalette> Themes = Array.AsReadOnly(new[]
    {
        new ThemePalette("light", "Light", "#f3f5f7", "#ffffff", "#edf1f5", "#1c2430", "#586779", "#cbd3dc",
            "#1d4ed8", "#ffffff", "#A63E00", "#007A55", "#004C8C", "#A63D82", "#e2edfc",
            "#17202b", "#f7f9fc", "#c8d3e0", "#8bbcff",
            new[] { "#6543b5", "#8b4a00", "#146b5a", "#165f99", "#813f90", "#a43b2b", "#36548a" }, 12),
        new ThemePalette("dark", "Dark", "#0e1116", "#171c24", "#202733", "#f1f5f9", "#b6c0ce", "#394453",
            "#5ea2ff", "#101826", "#F0B35E", "#7BE0B0", "#56B4E9", "#CC79A7", "#1c2c42",
            "#090c11", "#f8fafc", "#c1cad6", "#86baff",
            new[] { "#cab8ff", "#ffc857", "#7ce3c3", "#75bdff", "#e9a6ff", "#ff9b86", "#a9bcff" }, 12),
        new ThemePalette("classic", "Windows Classic", "#c0c0c0", "#efefef", "#d8d8d8", "#111111", "#303030", "#777777",
            "#000080", "#ffffff", "#9C3F00", "#005C3F", "#004C8C", "#8A2A5F", "#d8d8d8",
            "#d8d8d8", "#101010", "#333333", "#000080",
            new[] { "#4b0082", "#7a3e00", "#005a4a", "#003c8f", "#5d2a74", "#8a1d1d", "#1c286a" }, 2),
        new ThemePalette("xp", "Windows XP Luna", "#d7e7f7", "#f5f9fe", "#e4effa", "#0e2b4d", "#355675", "#8aafd2",
            "#1d5ba8", "#ffffff", "#9C3F00", "#005C3F", "#004C8C", "#8A2A5F", "#dbeafa",
            "#e9f3fd", "#10365d", "#365d83", "#174f92",
            new[] { "#553399", "#8b4d00", "#146b53", "#155a99", "#6f3b8a", "#a3382a", "#345685" }, 10),
        new ThemePalette("macos", "macOS", "#f5f5f7", "#ffffff", "#eeeeF2", "#1d1d1f", "#626268", "#d3d3d8",
            "#0069d9", "#ffffff", "#A63E00", "#007A55", "#0063A8", "#A63D82", "#eaf2fc",
            "#ffffff", "#1d1d1f", "#5e5e65", "#0055b8",
            new[] { "#6e40c9", "#9c4a00", "#116b54", "#1263a5", "#7c3aa3", "#ad3a2a", "#36548a" }, 14),
        new ThemePalette("adwaita", "Linux Adwaita", "#f6f5f4", "#ffffff", "#eeedec", "#241f31", "#5e5c64", "#c9c7c5",
            "#1c5faa", "#ffffff", "#A63E00", "#007A55", "#0063A8", "#A63D82", "#e6f0fa",
            "#ffffff", "#241f31", "#5e5c64", "#1c5faa",
            new[] { "#6135a6", "#8d4a00", "#146b52", "#145e99", "#733891", "#a43a2a", "#39588a" }, 14),
        new ThemePalette("nord", "Nord", "#2e3440", "#3b4252", "#434c5e", "#eceff4", "#c4cbd5", "#596579",
            "#88c0d0", "#1b222c", "#EBCB8B", "#9EE37D", "#A8D8E8", "#E8B4D2", "#485365",
            "#242a35", "#eceff4", "#c4cbd5", "#8fcede",
            new[] { "#c4b5fd", "#f2c57c", "#bad4a4", "#9ccddc", "#ddbad8", "#f4b49f", "#aac5df" }, 12),
        new ThemePalette("dracula", "Dracula", "#21222c", "#282a36", "#343746", "#f8f8f2", "#c2bdcf", "#53566a",
            "#bd93f9", "#1e172c", "#FFB86C", "#67E6A4", "#85E7FF", "#FF8DCE", "#383042",
            "#191a23", "#f8f8f2", "#c2bdcf", "#d0adff",
            new[] { "#ddb7ff", "#ffc857", "#67e6a4", "#85e7ff", "#ff8dce", "#ff807a", "#b7a1ff" }, 12)
    });

    private static readonly string[] PrayerKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];

    /// <summary>All built-in theme palettes.</summary>
    public static IReadOnlyList<ThemePalette> All => Themes;

    /// <summary>Normalizes a theme id to a known built-in id, defaulting to "light" for unknown values.</summary>
    /// <param name="id">Theme id to normalize.</param>
    /// <returns>The normalized theme id.</returns>
    public static string NormalizeId(string? id) => Themes.FirstOrDefault(theme =>
        string.Equals(theme.Id, id?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id ?? "light";

    /// <summary>Returns the palette for the given theme id, normalizing unknown values to "light".</summary>
    /// <param name="id">Theme id to look up.</param>
    /// <returns>The matching palette.</returns>
    public static ThemePalette Get(string? id) => Themes.First(theme =>
        string.Equals(theme.Id, NormalizeId(id), StringComparison.Ordinal));

    /// <summary>Returns the prayer color for a prayer key within the palette, defaulting to the first color for unknown keys.</summary>
    /// <param name="palette">Palette to read the color from.</param>
    /// <param name="key">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <returns>The color for the key.</returns>
    public static string PrayerColor(ThemePalette palette, string key)
    {
        int index = Array.FindIndex(PrayerKeys, prayerKey => string.Equals(prayerKey, key, StringComparison.OrdinalIgnoreCase));
        return palette.PrayerColors[index >= 0 && index < palette.PrayerColors.Count ? index : 0];
    }

    /// <summary>Blends two #RRGGBB colors; the second color contributes <paramref name="secondWeight"/>.</summary>
    private static string BlendHex(string first, string second, double secondWeight)
    {
        static int Channel(string hex, int offset) => Convert.ToInt32(hex.Substring(offset, 2), 16);
        int r = (int)Math.Round(Channel(first, 1) * (1 - secondWeight) + Channel(second, 1) * secondWeight);
        int g = (int)Math.Round(Channel(first, 3) * (1 - secondWeight) + Channel(second, 3) * secondWeight);
        int b = (int)Math.Round(Channel(first, 5) * (1 - secondWeight) + Channel(second, 5) * secondWeight);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>Header colors for Saturday and Sunday, derived from the active palette.</summary>
    /// <param name="palette">Palette to derive colors from.</param>
    /// <returns>Foreground and background hex colors for weekend day headers.</returns>
    public static (string Foreground, string Background) WeekendHeaderColors(ThemePalette palette) =>
        (palette.PrimaryText, BlendHex(palette.SurfaceAlt, palette.MutedText, 0.18));

    /// <summary>Resolves the calendar cell colors for a marker against the given palette.</summary>
    /// <param name="palette">Palette to derive colors from.</param>
    /// <param name="marker">Marker visual state of the calendar cell.</param>
    /// <returns>The resolved calendar cell colors.</returns>
    public static CalendarThemeColors CalendarColorsFor(ThemePalette palette, CalendarMarker marker)
    {
        bool hasObservance = marker.IsHoliday || marker.HasFastingProhibition || marker.HasSunnahFast || marker.HasReligiousEvent;
        string categoryColor = marker.IsHoliday ? palette.ErrorText
            : marker.HasFastingProhibition ? palette.WarningText
            : marker.HasSunnahFast ? palette.SuccessText
            : marker.HasReligiousEvent ? palette.NoteText
            : palette.Border;
        string weekendBackground = BlendHex(palette.Surface, palette.MutedText, 0.12);
        string weekendBorder = BlendHex(palette.Border, palette.Accent, 0.35);
        string background = marker.IsSelected ? palette.Accent
            : marker.IsToday ? palette.TodayBackground
            : hasObservance ? palette.SurfaceAlt
            : marker.IsWeekend ? weekendBackground
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
            : marker.IsWeekend ? weekendBorder
            : palette.Border;
        string markerColor = marker.IsSelected ? palette.AccentText : categoryColor;
        return new CalendarThemeColors(background, foreground, border, marker.BorderThickness, markerColor);
    }
}
