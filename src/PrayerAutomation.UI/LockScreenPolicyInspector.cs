using System.IO;
using System.Security;
using Microsoft.Win32;

namespace PrayerAutomation.UI;

/// <summary>The result of inspecting device lock-screen policies.</summary>
internal sealed record LockScreenPolicyInfo(bool ManagedByPolicy, string? PolicySource);

/// <summary>
/// Read-only detection of organisation policies that own the lock screen.
/// It never writes the registry, never elevates, and never tries to bypass a policy.
/// </summary>
internal static class LockScreenPolicyInspector
{
    private const string PersonalizationPolicyPath =
        @"SOFTWARE\Policies\Microsoft\Windows\Personalization";
    private const string PersonalizationCspPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";
    private const string PolicyManagerDeviceRoot =
        @"SOFTWARE\Microsoft\PolicyManager\current\device";
    private const string SpotlightOverridePath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string LockScreenOverridePath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen";

    private static readonly string[] PolicyValueHints =
    [
        "lock", "personalization", "spotlight", "slideshow"
    ];

    /// <summary>Inspects the known policy locations on the local machine.</summary>
    /// <returns>Whether a policy owns the lock screen and the first matching source.</returns>
    internal static LockScreenPolicyInfo Inspect()
    {
        LockScreenPolicyInfo info = InspectCore();
        LogPolicyState(info);
        return info;
    }

    private static bool? _lastLoggedManaged;
    private static string? _lastLoggedSource;
    private static readonly object LogSync = new();

    /// <summary>Writes a log line only when the detected policy state changes.</summary>
    /// <param name="info">The current policy state.</param>
    private static void LogPolicyState(LockScreenPolicyInfo info)
    {
        lock (LogSync)
        {
            if (_lastLoggedManaged == info.ManagedByPolicy &&
                string.Equals(_lastLoggedSource, info.PolicySource, StringComparison.Ordinal))
            {
                return;
            }
            _lastLoggedManaged = info.ManagedByPolicy;
            _lastLoggedSource = info.PolicySource;
        }
        AppLogger.WriteInfo("lock-screen.policy",
            $"managed={(info.ManagedByPolicy ? "yes" : "no")} source={info.PolicySource ?? "none"}");
    }

    /// <summary>Reads the known policy locations without logging.</summary>
    /// <returns>The detected policy state.</returns>
    private static LockScreenPolicyInfo InspectCore()
    {
        try
        {
            // Group Policy: explicitly forbids changing the lock screen.
            string? personalization = ReadPolicyValue(
                PersonalizationPolicyPath,
                valueName => valueName.Equals("NoChangingLockScreen", StringComparison.OrdinalIgnoreCase) ||
                    valueName.Contains("LockScreen", StringComparison.OrdinalIgnoreCase));
            if (personalization is not null)
                return new LockScreenPolicyInfo(true, personalization);

            // Personalization CSP: an administrator pinned a lock-screen image.
            string? csp = ReadPolicyValue(
                PersonalizationCspPath,
                valueName => valueName.StartsWith("LockScreenImage", StringComparison.OrdinalIgnoreCase));
            if (csp is not null)
                return new LockScreenPolicyInfo(true, csp);

            // MDM/Intune device policy (the registry mirror of the dmmap WMI classes).
            foreach (string subKey in new[] { "Personalization", "Experience" })
            {
                string? mdm = ReadPolicyValue(
                    PolicyManagerDeviceRoot + @"\" + subKey,
                    valueName => PolicyValueHints.Any(hint =>
                        valueName.Contains(hint, StringComparison.OrdinalIgnoreCase)));
                if (mdm is not null)
                    return new LockScreenPolicyInfo(true, mdm);
            }

            // Machine-wide lock-screen policy values; ordinary 0/1 feature flags are ignored.
            string? spotlight = ReadAnyValue(SpotlightOverridePath);
            if (spotlight is not null)
                return new LockScreenPolicyInfo(true, spotlight);
            string? slideshow = ReadAnyValue(LockScreenOverridePath);
            if (slideshow is not null)
                return new LockScreenPolicyInfo(true, slideshow);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or ArgumentException)
        {
            // Unreadable policy data must not crash the settings page; the probe stays conservative.
            return new LockScreenPolicyInfo(false, null);
        }

        return new LockScreenPolicyInfo(false, null);
    }

    /// <summary>Returns a value name when a readable value is an enforced policy.</summary>
    /// <param name="path">HKLM key path.</param>
    /// <param name="predicate">Value-name filter.</param>
    /// <returns>Key path plus value name, or null.</returns>
    private static string? ReadPolicyValue(string path, Func<string, bool> predicate)
    {
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, writable: false);
        if (key is null)
            return null;
        foreach (string valueName in key.GetValueNames())
        {
            if (predicate(valueName) && LockScreenPolicyValueRules.IsEnforced(valueName, key.GetValue(valueName)))
                return path + @"\" + valueName;
        }
        return null;
    }

    /// <summary>Returns the key path when the machine-wide key contains an enforced lock-screen policy.</summary>
    /// <param name="path">HKLM key path.</param>
    /// <returns>Key path plus a value name, or null.</returns>
    private static string? ReadAnyValue(string path)
    {
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, writable: false);
        if (key is null)
            return null;
        foreach (string valueName in key.GetValueNames())
        {
            if (LockScreenPolicyValueRules.IsEnforced(valueName, key.GetValue(valueName)))
                return path + @"\" + valueName;
        }
        return null;
    }
}
