using System.Windows;
using System.Windows.Media;

namespace PrayerAutomation.UI;

internal static class ThemeManager
{
    internal static ThemePalette Current { get; private set; } = ThemeCatalog.Get("light");

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

    internal static System.Windows.Media.Brush ToBrush(string color)
    {
        var brush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)!);
        if (brush.CanFreeze) brush.Freeze();
        return brush;
    }
}
