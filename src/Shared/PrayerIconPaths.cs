namespace PrayerAutomation.Shared;

// User-selected set C from prayer-icon-options.html: 24-unit solid silhouettes.
// SVG paths are preserved; circles and rounded rectangles use equivalent arcs.
// Link this file into both renderers so geometry cannot drift between WPF and WinUI.
internal static class PrayerIconPaths
{
    internal const double Size = 24;

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Paths =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fajr"] = Array.AsReadOnly(new[]
            {
                "F1 M14 2A9 9 0 1 0 21 15A8 8 0 0 1 14 2Z",
                "F1 M4 20H20a1 1 0 0 1 1 1V21a1 1 0 0 1 -1 1H4a1 1 0 0 1 -1 -1V21a1 1 0 0 1 1 -1Z"
            }),
            ["Sunrise"] = Array.AsReadOnly(new[]
            {
                "F1 M3 16a6 6 0 0 1 12 0Z",
                "F1 M3 17H15a1 1 0 0 1 1 1V18a1 1 0 0 1 -1 1H3a1 1 0 0 1 -1 -1V18a1 1 0 0 1 1 -1Z",
                "F1 m20 3-4 4h2.5v7h3V7H24Z"
            }),
            ["Dhuha"] = Array.AsReadOnly(new[]
            {
                "F1 M3 11a4 4 0 1 0 8 0a4 4 0 1 0 -8 0Z",
                "F1 M3 20H21a1 1 0 0 1 1 1V21a1 1 0 0 1 -1 1H3a1 1 0 0 1 -1 -1V21a1 1 0 0 1 1 -1Z"
            }),
            ["Dhuhr"] = Array.AsReadOnly(new[]
            {
                "F1 M8 6a4 4 0 1 0 8 0a4 4 0 1 0 -8 0Z",
                "F1 M3 20H21a1 1 0 0 1 1 1V21a1 1 0 0 1 -1 1H3a1 1 0 0 1 -1 -1V21a1 1 0 0 1 1 -1Z"
            }),
            ["Asr"] = Array.AsReadOnly(new[]
            {
                "F1 M13 12a4 4 0 1 0 8 0a4 4 0 1 0 -8 0Z",
                "F1 M3 20H21a1 1 0 0 1 1 1V21a1 1 0 0 1 -1 1H3a1 1 0 0 1 -1 -1V21a1 1 0 0 1 1 -1Z"
            }),
            ["Maghrib"] = Array.AsReadOnly(new[]
            {
                "F1 M3 16a6 6 0 0 1 12 0Z",
                "F1 M3 17H15a1 1 0 0 1 1 1V18a1 1 0 0 1 -1 1H3a1 1 0 0 1 -1 -1V18a1 1 0 0 1 1 -1Z",
                "F1 M18.5 3v7H16l4 4 4-4h-2.5V3Z"
            }),
            ["Isha"] = Array.AsReadOnly(new[]
            {
                "F1 M13 2A10 10 0 1 0 22 15A9 9 0 0 1 13 2Z",
                "F1 m19 2 1.1 2.9L23 6l-2.9 1.1L19 10l-1.1-2.9L15 6l2.9-1.1Z"
            })
        };

    internal static IReadOnlyList<string> ForKey(string? key)
    {
        if (string.Equals(key, "Imsak", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "moon", StringComparison.OrdinalIgnoreCase)) return Paths["Fajr"];
        if (string.Equals(key, "night", StringComparison.OrdinalIgnoreCase)) return Paths["Isha"];
        return key is not null && Paths.TryGetValue(key, out var paths) ? paths : Paths["Dhuha"];
    }
}
