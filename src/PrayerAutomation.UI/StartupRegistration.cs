using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrayerAutomation.UI;

/// <summary>Manages the HKCU Run registry value that launches the app at Windows sign-in.</summary>
internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "PrayerAutomation";

    /// <summary>Returns true when the Run value points at this product's executable.</summary>
    /// <returns>True when startup registration is present for this product.</returns>
    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return IsProductRunCommand(key?.GetValue(ValueName) as string);
    }

    // Keep the existing Run value identity so upgrades never create duplicate entries.
    /// <summary>Returns true when a Run value references this product's executable name.</summary>
    /// <param name="value">The registry value to inspect.</param>
    /// <returns>True when the value targets this product.</returns>
    internal static bool IsProductRunCommand(string? value) => value is not null &&
        (value.Contains("PrayerAutomation.UI.exe", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("AyoJamaah.exe", StringComparison.OrdinalIgnoreCase));

    /// <summary>Registers or unregisters the app to run at sign-in.</summary>
    /// <param name="enabled">True to register; false to remove the Run value.</param>
    /// <param name="startHidden">Pass --hidden when the app runs at sign-in.</param>
    /// <exception cref="InvalidOperationException">The executable path cannot be determined.</exception>
    public static void SetEnabled(bool enabled, bool startHidden = false)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        if (!enabled)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            return;
        }
        string executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("Path executable tidak dapat ditentukan.");
        key.SetValue(ValueName, BuildRunCommand(executable, startHidden), RegistryValueKind.String);
    }

    /// <summary>Builds the quoted Run-registry command line, optionally with --hidden.</summary>
    /// <param name="executablePath">Full path to the executable.</param>
    /// <param name="startHidden">Append --hidden.</param>
    /// <returns>The command line to store in the registry.</returns>
    /// <exception cref="ArgumentException"><paramref name="executablePath"/> is null or whitespace.</exception>
    internal static string BuildRunCommand(string executablePath, bool startHidden)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        return $"\"{executablePath}\"{(startHidden ? " --hidden" : string.Empty)}";
    }

    /// <summary>Returns whether the app should start hidden; mirrors the saved preference.</summary>
    /// <param name="arguments">Command-line arguments (unused).</param>
    /// <param name="preferenceEnabled">The saved start-hidden preference.</param>
    /// <returns>The preference value.</returns>
    internal static bool ShouldStartHidden(IEnumerable<string> arguments, bool preferenceEnabled)
        => preferenceEnabled;
}
