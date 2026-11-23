// Turns ZIP entry names into safe paths relative to the application root.
using System;
using System.IO;

namespace PrayerAutomation.UI;

/// <summary>Validates archive entry names before they reach any file-system path.</summary>
internal static class ZipEntryPath
{
    /// <summary>Tries to convert a ZIP entry name into a contained relative path.</summary>
    /// <param name="fullName">Raw <see cref="System.IO.Compression.ZipArchiveEntry.FullName"/> value.</param>
    /// <param name="root">Application root used as the containment boundary.</param>
    /// <param name="relative">Sanitized POSIX-style relative path on success.</param>
    /// <returns>True when the entry stays inside <paramref name="root"/>.</returns>
    internal static bool TryGetSafeRelative(string? fullName, string root, out string relative)
    {
        relative = string.Empty;
        if (string.IsNullOrWhiteSpace(fullName) ||
            fullName.EndsWith('/') || fullName.EndsWith('\\') ||
            !UpdateManifest.IsSafeRelativePath(fullName)) return false;

        try
        {
            // Canonicalize first: GetFullPath resolves '..' before the StartsWith
            // guard, which is the pattern CodeQL recognizes for Zip Slip.
            string boundary = Path.GetFullPath(root);
            if (!boundary.EndsWith(Path.DirectorySeparatorChar)) boundary += Path.DirectorySeparatorChar;
            string candidate = Path.GetFullPath(Path.Combine(boundary, fullName));
            if (!candidate.StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) return false;

            // GetFileName is an explicit sanitizer in CodeQL and also rejects a
            // candidate that points at a device or an empty final component.
            string fileName = Path.GetFileName(candidate);
            if (!UpdateManifest.IsSafeAssetName(fileName)) return false;

            relative = Path.GetRelativePath(boundary, candidate).Replace('\\', '/');
            return UpdateManifest.IsSafeRelativePath(relative);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }
}
