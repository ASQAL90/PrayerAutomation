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
    private const string UninstallKey =
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{8B76B6D5-1B3C-4E13-9A39-000000000001}_is1";
    private const string DefaultInstallDirectory = "Programs\\PrayerAutomation";

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

    /// <summary>Repairs a stale portable startup path and follows the last portable folder used.</summary>
    /// <param name="enabled">The saved startup preference; false leaves the registry alone.</param>
    /// <param name="currentExecutable">Full path of the running executable.</param>
    public static void SelfHeal(bool enabled, string? currentExecutable)
    {
        if (!enabled || string.IsNullOrWhiteSpace(currentExecutable) || !File.Exists(currentExecutable)) return;
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            string? value = key?.GetValue(ValueName) as string;
            if (key is null || !IsProductRunCommand(value)) return;
            if (!TryGetExecutablePath(value, out string? target) || string.IsNullOrWhiteSpace(target)) return;
            if (string.Equals(Path.GetFullPath(target), Path.GetFullPath(currentExecutable),
                    StringComparison.OrdinalIgnoreCase)) return;

            bool targetMissing = !File.Exists(target);
            bool installedTarget = IsInstalledPath(target);
            bool installedCurrent = IsInstalledPath(currentExecutable);
            if (!targetMissing && (installedTarget || installedCurrent)) return;

            key.SetValue(ValueName, BuildRunCommand(currentExecutable, HasHiddenFlag(value)), RegistryValueKind.String);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or ArgumentException)
        {
            // Startup is a convenience; never block the application because of registry access.
        }
    }

    /// <summary>Extracts the executable path from a quoted Run command.</summary>
    /// <param name="command">Registry command line.</param>
    /// <param name="path">Executable path when present.</param>
    /// <returns>True when a path could be extracted.</returns>
    internal static bool TryGetExecutablePath(string? command, out string? path)
    {
        path = null;
        if (string.IsNullOrWhiteSpace(command)) return false;
        string text = command.Trim();
        if (text.StartsWith('"'))
        {
            int closing = text.IndexOf('"', 1);
            if (closing <= 1) return false;
            path = text[1..closing];
        }
        else
        {
            int space = text.IndexOf(' ');
            path = space < 0 ? text : text[..space];
        }
        return !string.IsNullOrWhiteSpace(path);
    }

    /// <summary>True when a Run command asks for a hidden start.</summary>
    /// <param name="command">Registry command line.</param>
    /// <returns>True when --hidden is present.</returns>
    internal static bool HasHiddenFlag(string? command) =>
        command is not null && command.Contains("--hidden", StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the path is the NSIS per-user install directory.</summary>
    /// <param name="path">Path to inspect.</param>
    /// <returns>True for the installed copy.</returns>
    internal static bool IsInstalledPath(string path)
    {
        string? full;
        try { full = Path.GetFullPath(path); }
        catch (ArgumentException) { return false; }
        string root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DefaultInstallDirectory);
        string? registered = null;
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(UninstallKey, writable: false);
            registered = key?.GetValue("InstallLocation") as string;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { }
        bool matches(string? directory) => !string.IsNullOrWhiteSpace(directory) &&
            full.StartsWith(Path.GetFullPath(directory) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
        return matches(root) || matches(registered);
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
