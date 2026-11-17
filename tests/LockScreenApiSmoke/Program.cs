// Smoke probe reporting lock-screen personalization API and registry mode flag availability on the host, without changing any setting.
using Microsoft.Win32;
using Windows.System.UserProfile;

/// <summary>Invokes a probe and prints its value, or its exception type and HResult when it fails.</summary>
/// <typeparam name="T">Type of the probe result.</typeparam>
/// <param name="name">Label printed before the result.</param>
/// <param name="probe">Probe to invoke.</param>
static void Report<T>(string name, Func<T> probe)
{
    try
    {
        T value = probe();
        Console.WriteLine($"{name}={value}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{name}=error:{ex.GetType().Name}:0x{ex.HResult:X8}");
    }
}

Report("profile_api_supported", UserProfilePersonalizationSettings.IsSupported);
Report("lock_screen_image_uri_available", () =>
    LockScreen.OriginalImageFile is { IsAbsoluteUri: true });
Report("lock_screen_image_uri_is_file", () =>
    LockScreen.OriginalImageFile is { IsFile: true });
Report("slideshow_mode_flag", () => ReadInteger(
    @"Software\Microsoft\Windows\CurrentVersion\Lock Screen", "SlideshowEnabled"));
Report("spotlight_mode_flag", () => ReadInteger(
    @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled"));

/// <summary>Reads an integer registry value, returning "absent" when missing or not an integer.</summary>
/// <param name="keyPath">Subkey path below HKCU.</param>
/// <param name="valueName">Name of the value to read.</param>
/// <returns>The value as an invariant-culture string, or "absent".</returns>
static string ReadInteger(string keyPath, string valueName)
{
    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
    object? value = key?.GetValue(valueName);
    return value is int number ? number.ToString(System.Globalization.CultureInfo.InvariantCulture) : "absent";
}
