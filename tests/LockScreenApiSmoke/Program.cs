using Microsoft.Win32;
using Windows.System.UserProfile;

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

static string ReadInteger(string keyPath, string valueName)
{
    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
    object? value = key?.GetValue(valueName);
    return value is int number ? number.ToString(System.Globalization.CultureInfo.InvariantCulture) : "absent";
}
