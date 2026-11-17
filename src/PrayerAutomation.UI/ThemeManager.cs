// Applies theme palettes to the WPF application resource dictionary.
using System.Windows;
using System.Windows.Media;

namespace PrayerAutomation.UI;

/// <summary>Applies the selected theme by updating application-level brush and radius resources.</summary>
internal static class ThemeManager
{
    /// <summary>The palette currently applied to the application resources.</summary>
    internal static ThemePalette Current { get; private set; } = ThemeCatalog.Get("light");

    /// <summary>Applies the theme identified by themeId to the application's resource dictionary.</summary>
    /// <param name="application">The WPF application whose resources are updated.</param>
    /// <param name="themeId">Theme id to apply; unknown values fall back to the default theme.</param>
    public static void Apply(System.Windows.Application application, string? themeId)
    {
        ArgumentNullException.ThrowIfNull(application);
        Current = ThemeCatalog.Get(themeId);
        ResourceDictionary resources = application.Resources;
        resources["AppBackgroundBrush"] = ToBrush(Current.Canvas);
        resources["SurfaceBrush"] = ToBrush(Current.Surface);
        resources["SurfaceAltBrush"] = ToBrush(Current.SurfaceAlt);
        resources["PrimaryTextBrush"] = ToBrush(Current.PrimaryText);
        resources["MutedTextBrush"] = ToBrush(Current.MutedText);
        resources["AppBorderBrush"] = ToBrush(Current.Border);
        resources["AccentBrush"] = ToBrush(Current.Accent);
        resources["AccentTextBrush"] = ToBrush(Current.AccentText);
        resources["ErrorTextBrush"] = ToBrush(Current.ErrorText);
        resources["SuccessTextBrush"] = ToBrush(Current.SuccessText);
        resources["WarningTextBrush"] = ToBrush(Current.WarningText);
        resources["NoteTextBrush"] = ToBrush(Current.NoteText);
        resources["CardCornerRadius"] = new CornerRadius(Current.CornerRadius);
        AppBranding.Apply(resources, Current);
    }

    /// <summary>Converts a hex color string to a frozen solid-color brush.</summary>
    /// <param name="color">Color string in a format understood by ColorConverter.</param>
    /// <returns>A frozen brush for the color.</returns>
    internal static System.Windows.Media.Brush ToBrush(string color)
    {
        var brush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)!);
        if (brush.CanFreeze) brush.Freeze();
        return brush;
    }
}
