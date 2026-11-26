using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace PrayerAutomation.UI;

// Geometry copied verbatim from the user-approved v13 master (nonzero fill).
// Display branding is separate from namespaces, executable and storage identity.
/// <summary>Display branding (name, version, logo geometry) for Ayo Jama'ah!, separate from identity and storage.</summary>
public static class AppBranding
{
    /// <summary>The user-facing application name.</summary>
    public const string DisplayName = "Ayo Jama'ah!";
    /// <summary>The application version without build metadata.</summary>
    public static string Version { get; } = ResolveVersion();
    /// <summary>The version prefixed with "v" for display.</summary>
    public static string VersionLabel => $"v{Version}";
    /// <summary>The window title combining display name and version.</summary>
    public static string WindowTitle => $"{DisplayName} {VersionLabel}";
    /// <summary>The localized "Versi {version}" label.</summary>
    public static string VersionDisplay => $"Versi {Version}";
    /// <summary>Vector path data for the mosque glyph.</summary>
    public const string MosquePath = "F1 M31 53.2 C29.1 43.7 40.5 38.95 45.25 32.3 C47.34 29.925 49.05 27.55 50 19.95 C50.95 27.55 52.66 29.925 54.75 32.3 C59.5 38.95 70.9 43.7 69 53.2 Z M25.3 53.2 L74.7 53.2 L74.7 70.3 L25.3 70.3 Z M43.73 60.99 A6.27 6.27 0 1 0 56.27 60.99 A6.27 6.27 0 1 0 43.73 60.99 Z M51.045 56.335 L51.045 60.446 L53.1674 61.9321 A1.045 1.045 0 0 1 51.9687 63.6442 L49.4006 61.846 A1.045 1.045 0 0 1 48.955 60.99 L48.955 56.335 A1.045 1.045 0 0 1 51.045 56.335 Z M22.83 47.5 L27.77 47.5 L27.77 70.3 L22.83 70.3 Z M22.07 47.5 L25.3 40.85 L28.53 47.5 Z M72.23 47.5 L77.17 47.5 L77.17 70.3 L72.23 70.3 Z M71.47 47.5 L74.7 40.85 L77.93 47.5 Z";
    /// <summary>Vector path data for the congregation glyph.</summary>
    public const string CongregationPath = "F1 M23.02 79.8 A4.18 4.18 0 1 1 31.38 79.8 A4.18 4.18 0 1 1 23.02 79.8 Z M34.42 79.8 A4.18 4.18 0 1 1 42.78 79.8 A4.18 4.18 0 1 1 34.42 79.8 Z M45.82 80.085 A4.18 4.18 0 1 1 54.18 80.085 A4.18 4.18 0 1 1 45.82 80.085 Z M57.22 80.75 A4.18 4.18 0 1 1 65.58 80.75 A4.18 4.18 0 1 1 57.22 80.75 Z M68.62 81.7 A4.18 4.18 0 1 1 76.98 81.7 A4.18 4.18 0 1 1 68.62 81.7 Z";
    /// <summary>Vector path data for the map-pin glyph in the location picker.</summary>
    public const string LocationMapPath = "F1 M12 2 C7.58 2 4 5.58 4 10 C4 16.3 12 24 12 24 C12 24 20 16.3 20 10 C20 5.58 16.42 2 12 2 Z M12 7.4 A2.6 2.6 0 1 1 12 12.6 A2.6 2.6 0 1 1 12 7.4 Z";

    /// <summary>Frozen geometry for the mosque glyph.</summary>
    public static Geometry MosqueGeometry { get; } = ParseMaster(MosquePath);
    /// <summary>Frozen geometry for the congregation glyph.</summary>
    public static Geometry CongregationGeometry { get; } = ParseMaster(CongregationPath);
    /// <summary>Frozen geometry for the location-map glyph.</summary>
    public static Geometry LocationMapGeometry { get; } = ParseMaster(LocationMapPath);

    /// <summary>Parses and freezes path data into a geometry.</summary>
    /// <param name="path">WPF geometry path markup.</param>
    /// <returns>The frozen geometry.</returns>
    private static Geometry ParseMaster(string path)
    {
        var geometry = Geometry.Parse(path);
        geometry.Freeze();
        return geometry;
    }

    /// <summary>Resolves the display version from assembly metadata, stripping any "+commit" build metadata.</summary>
    /// <returns>The version string for display.</returns>
    private static string ResolveVersion()
    {
        Assembly assembly = typeof(AppBranding).Assembly;
        string? informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        string candidate = informational ?? assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        // Build metadata ("+commit") is not part of the displayed version.
        int metadata = candidate.IndexOf('+');
        return metadata >= 0 ? candidate[..metadata] : candidate;
    }

    /// <summary>Applies brand brushes and the badge geometry to the application resource dictionary for a theme.</summary>
    /// <param name="resources">The resource dictionary to update.</param>
    /// <param name="theme">The theme whose palette drives brand colors.</param>
    internal static void Apply(ResourceDictionary resources, ThemePalette theme)
    {
        // Light is the existing default theme; its logo must match the fixed ICO.
        bool isDefault = theme.Id == "light";
        bool darkMark = theme.Id is "dark" or "nord" or "dracula";
        resources["BrandBackgroundBrush"] = ThemeManager.ToBrush(isDefault ? "#0F5C47" : theme.Accent);
        resources["BrandMosqueBrush"] = ThemeManager.ToBrush(isDefault ? "#F5EFDB" : darkMark ? "#101418" : "#ffffff");
        resources["BrandCongregationBrush"] = ThemeManager.ToBrush(isDefault ? "#E2AF4A" : darkMark ? "#101418" : "#ffd76a");
        // Watermark latar: hijau logo pada tema terang, warna teks utama pada tema
        // gelap, dengan alfa yang sudah diperhitungkan (18% / 14%).
        resources["WatermarkBrush"] = ThemeManager.ToBrush(darkMark
            ? $"#24{theme.PrimaryText.TrimStart('#')}"
            : "#2E0F5C47");
        double radius = isDefault ? 20 : theme.Id == "classic" ? 0 : theme.CornerRadius;
        var badge = new RectangleGeometry(new Rect(3, 3, 94, 94), radius, radius);
        badge.Freeze();
        resources["BrandBadgeGeometry"] = badge;
    }
}
