namespace PrayerAutomation.UI;

internal readonly record struct WidgetPlacement(double Left, double Top, double Width, double Height);

internal static class WidgetSettingsLogic
{
    public static string NormalizeMode(string? mode) => mode?.Trim().ToLowerInvariant() switch
    {
        "minimal" or "essential" => "essential",
        "minimalist" => "minimalist",
        "normal" or "detail" => "detail",
        _ => "essential"
    };

    public static string NormalizeMotion(string? motion, bool legacyMinimalMode) => motion?.Trim().ToLowerInvariant() switch
    {
        "static" => "static",
        "marquee" => "marquee",
        _ => legacyMinimalMode ? "marquee" : "static"
    };

    public static string NormalizePosition(string? position) => position?.Trim().ToLowerInvariant() switch
    {
        "tc" => "tc",
        "tl" => "tl",
        "tr" => "tr",
        "bl" => "bl",
        "br" => "br",
        _ => "tc"
    };

    public static WidgetPlacement PlaceWidget(
        string? mode,
        string? position,
        double workAreaLeft,
        double workAreaTop,
        double workAreaWidth,
        double workAreaHeight,
        double requestedWidth,
        double requestedHeight,
        double margin)
    {
        if (!double.IsFinite(workAreaLeft) || !double.IsFinite(workAreaTop) ||
            !double.IsFinite(workAreaWidth) || !double.IsFinite(workAreaHeight) ||
            !double.IsFinite(requestedWidth) || !double.IsFinite(requestedHeight) ||
            !double.IsFinite(margin) || workAreaWidth < 0 || workAreaHeight < 0)
            throw new ArgumentOutOfRangeException(nameof(workAreaWidth), "Screen bounds and widget size must be finite and non-negative.");

        double safeMargin = Math.Max(0, margin);
        double maxWidth = Math.Max(0, workAreaWidth - 2 * safeMargin);
        double maxHeight = Math.Max(0, workAreaHeight - 2 * safeMargin);
        double width = Math.Min(Math.Max(0, requestedWidth), maxWidth);
        double height = Math.Min(Math.Max(0, requestedHeight), maxHeight);
        string normalizedPosition = NormalizePosition(position);

        double left = normalizedPosition switch
        {
            "tl" or "bl" => workAreaLeft + safeMargin,
            "tr" or "br" => workAreaLeft + workAreaWidth - width - safeMargin,
            _ => workAreaLeft + (workAreaWidth - width) / 2
        };
        double top = normalizedPosition switch
        {
            "bl" or "br" => workAreaTop + workAreaHeight - height - safeMargin,
            _ => workAreaTop
        };
        return new WidgetPlacement(left, top, width, height);
    }
}
