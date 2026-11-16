using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrayerAutomation.UI;

internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "PrayerAutomation";

    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return IsProductRunCommand(key?.GetValue(ValueName) as string);
    }

    // Keep the existing Run value identity so upgrades never create duplicate entries.
    internal static bool IsProductRunCommand(string? value) => value is not null &&
        (value.Contains("PrayerAutomation.UI.exe", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("AyoJamaah.exe", StringComparison.OrdinalIgnoreCase));

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

    internal static string BuildRunCommand(string executablePath, bool startHidden)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        return $"\"{executablePath}\"{(startHidden ? " --hidden" : string.Empty)}";
    }

    internal static bool ShouldStartHidden(IEnumerable<string> arguments, bool preferenceEnabled)
        => preferenceEnabled;
}
