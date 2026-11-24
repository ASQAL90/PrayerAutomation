// Chooses the native prayer_core file that the P/Invoke resolver may load.
// No WPF dependency: the path rules are exercised by the headless smoke test.
using System;
using System.IO;

namespace PrayerAutomation.UI;

/// <summary>
/// Locates the native prayer_core library inside the application folder only.
/// The default probing rules can consult locations outside the install
/// directory; pinning the path here keeps a same-named DLL planted elsewhere
/// from being loaded.
/// </summary>
internal static class NativeLibraryPath
{
    /// <summary>Name used by the <c>DllImport</c> attributes.</summary>
    internal const string LibraryName = "prayer_core";

    /// <summary>Platform-specific file name the runtime would normally probe for.</summary>
    /// <returns>Bare library file name, without any directory.</returns>
    internal static string FileName =>
        OperatingSystem.IsWindows() ? "prayer_core.dll" :
        OperatingSystem.IsMacOS() ? "libprayer_core.dylib" :
        "libprayer_core.so";

    /// <summary>Returns the validated library path inside <paramref name="baseDirectory"/>.</summary>
    /// <param name="baseDirectory">Directory the application was started from.</param>
    /// <returns>Full path to a regular, non-empty library file, or null when none is usable.</returns>
    internal static string? Locate(string? baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory)) return null;
        try
        {
            string root = Path.GetFullPath(baseDirectory);
            string rootFull = Path.TrimEndingDirectorySeparator(root);
            string candidate = Path.GetFullPath(Path.Combine(rootFull, FileName));
            // Only the exact application folder is accepted: no subdirectory,
            // no traversal, and no other drive or share.
            if (!string.Equals(Path.GetDirectoryName(candidate), rootFull, StringComparison.OrdinalIgnoreCase))
                return null;
            if (!File.Exists(candidate)) return null;
            var info = new FileInfo(candidate);
            if (info.Length == 0) return null;
            // A link or junction could point outside the verified folder even
            // though its path looks local.
            if ((info.Attributes & FileAttributes.ReparsePoint) != 0) return null;
            return candidate;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or System.Security.SecurityException)
        {
            return null;
        }
    }
}
