// Pure logic for normalizing widget settings and computing on-screen widget placement.
namespace PrayerAutomation.UI;

/// <summary>Resolved screen placement and size of the widget.</summary>
/// <param name="Left">Left coordinate in pixels.</param>
/// <param name="Top">Top coordinate in pixels.</param>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
internal readonly record struct WidgetPlacement(double Left, double Top, double Width, double Height);

/// <summary>Normalization and placement logic for widget settings.</summary>
internal static class WidgetSettingsLogic
{
    /// <summary>Normalizes a widget mode value; unknown values fall back to "essential".</summary>
    /// <param name="mode">Raw mode value from settings.</param>
    /// <returns>The normalized mode.</returns>
    public static string NormalizeMode(string? mode) => mode?.Trim().ToLowerInvariant() switch
    {
        "minimal" or "essential" => "essential",
        "minimalist" => "minimalist",
        "normal" or "detail" => "detail",
        _ => "essential"
    };

    /// <summary>Normalizes a motion value; when unset, legacy minimal mode implies marquee, otherwise static.</summary>
    /// <param name="motion">Raw motion value from settings.</param>
    /// <param name="legacyMinimalMode">True when the configured mode is the legacy minimal mode.</param>
    /// <returns>The normalized motion.</returns>
    public static string NormalizeMotion(string? motion, bool legacyMinimalMode) => motion?.Trim().ToLowerInvariant() switch
    {
        "static" => "static",
        "marquee" => "marquee",
        _ => legacyMinimalMode ? "marquee" : "static"
    };

    /// <summary>Normalizes a position value; unknown values fall back to "tc" (top center).</summary>
    /// <param name="position">Raw position value from settings.</param>
    /// <returns>The normalized position code.</returns>
    public static string NormalizePosition(string? position) => position?.Trim().ToLowerInvariant() switch
    {
        "tc" => "tc",
        "tl" => "tl",
        "tr" => "tr",
        "bl" => "bl",
        "br" => "br",
        _ => "tc"
    };

    /// <summary>Computes the clamped widget placement within the work area for the given mode and position.</summary>
    /// <param name="mode">Widget mode value.</param>
    /// <param name="position">Requested position code (e.g. tl, tc, tr, bl, br).</param>
    /// <param name="workAreaLeft">Left edge of the work area.</param>
    /// <param name="workAreaTop">Top edge of the work area.</param>
    /// <param name="workAreaWidth">Width of the work area.</param>
    /// <param name="workAreaHeight">Height of the work area.</param>
    /// <param name="requestedWidth">Requested widget width.</param>
    /// <param name="requestedHeight">Requested widget height.</param>
    /// <param name="margin">Margin from the work area edges.</param>
    /// <returns>The resolved placement.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bounds or sizes are not finite or negative.</exception>
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
