// Version handling for the delta updater. Keeps release tags and assembly versions comparable.
using System;
using System.Globalization;
using System.Linq;

namespace PrayerAutomation.UI;

/// <summary>Normalizes and compares dotted version strings used by releases and manifests.</summary>
internal static class UpdateVersion
{
    /// <summary>Parses a version or a v-prefixed release tag into a four-part version.</summary>
    /// <param name="text">Version text, e.g. "2.3.1" or "v2.3.1".</param>
    /// <param name="version">Parsed version.</param>
    /// <returns>True when the text is a valid numeric dotted version.</returns>
    internal static bool TryParse(string? text, out Version version)
    {
        version = new Version(0, 0, 0, 0);
        if (string.IsNullOrWhiteSpace(text)) return false;
        string candidate = text.Trim().TrimStart('v', 'V');
        string[] parts = candidate.Split('.');
        if (parts.Length is < 1 or > 4) return false;
        if (parts.Any(part => part.Length == 0 || part.Any(character => !char.IsAsciiDigit(character))))
            return false;
        string padded = string.Join(".", parts.Concat(Enumerable.Repeat("0", 4 - parts.Length)));
        if (!Version.TryParse(padded, out Version? parsed)) return false;
        version = parsed;
        return true;
    }

    /// <summary>Formats a version as a three-part string used in asset names.</summary>
    /// <param name="version">Version to format.</param>
    /// <returns>Asset-safe version text.</returns>
    internal static string AssetText(Version version) =>
        $"{version.Major.ToString(CultureInfo.InvariantCulture)}." +
        $"{version.Minor.ToString(CultureInfo.InvariantCulture)}." +
        $"{version.Build.ToString(CultureInfo.InvariantCulture)}";
}
