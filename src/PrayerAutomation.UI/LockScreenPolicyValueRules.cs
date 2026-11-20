namespace PrayerAutomation.UI;

/// <summary>
/// Pure rules for deciding whether a registry value describes an enforced lock-screen
/// policy. Keeping this separate from the read-only registry probe makes the
/// false-positive cases testable without Windows policy keys.
/// </summary>
internal static class LockScreenPolicyValueRules
{
    /// <summary>Returns true only for an explicitly enabled prevention or pinned image value.</summary>
    /// <param name="valueName">Registry value name.</param>
    /// <param name="value">Registry value data.</param>
    /// <returns>True when the data actually enforces a lock-screen policy.</returns>
    internal static bool IsEnforced(string valueName, object? value)
    {
        if (string.IsNullOrWhiteSpace(valueName) || value is null)
            return false;

        bool explicitPrevention = valueName.Equals("NoChangingLockScreen", StringComparison.OrdinalIgnoreCase) ||
            (StartsWithAny(valueName, "No", "Prevent", "Disallow", "Block") &&
             ContainsAny(valueName, "Lock", "Personalization", "Spotlight", "Slideshow"));
        bool pinnedImage = valueName.Contains("LockScreenImage", StringComparison.OrdinalIgnoreCase);
        if (!explicitPrevention && !pinnedImage)
            return false;

        if (value is int number) return number == 1;
        if (value is long number64) return number64 == 1;
        if (value is string text)
        {
            if (pinnedImage)
                return !string.IsNullOrWhiteSpace(text);
            string trimmed = text.Trim();
            return trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    /// <summary>Returns true when text starts with one of the prefixes.</summary>
    private static bool StartsWithAny(string text, params string[] prefixes)
    {
        foreach (string prefix in prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Returns true when text contains one of the fragments.</summary>
    private static bool ContainsAny(string text, params string[] fragments)
    {
        foreach (string fragment in fragments)
        {
            if (text.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
