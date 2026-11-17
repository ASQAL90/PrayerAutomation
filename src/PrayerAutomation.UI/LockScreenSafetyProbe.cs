using System.IO;
using System.Security;
using Microsoft.Win32;
using Windows.System.UserProfile;

namespace PrayerAutomation.UI;

internal sealed record LockScreenSafetyInfo(
    bool ProfileApiSupported,
    LockScreenBackgroundMode BackgroundMode,
    bool HasLocalImageFile,
    bool IsAppTemporaryImage,
    bool CanChangeTemporarily,
    string Message);

internal static class LockScreenSafetyProbe
{
    private const string SlideshowRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Lock Screen";
    private const string SpotlightRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string TemporaryImagePrefix = "PrayerAutomation-LockScreen-";

    public static LockScreenSafetyInfo Inspect()
    {
        bool profileApiSupported;
        try { profileApiSupported = UserProfilePersonalizationSettings.IsSupported(); }
        catch (Exception) { profileApiSupported = false; }

        int? slideshowEnabled = ReadInteger(SlideshowRegistryPath, "SlideshowEnabled");
        int? spotlightEnabled = ReadInteger(SpotlightRegistryPath, "RotatingLockScreenEnabled");
        LockScreenBackgroundMode mode = LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled);

        Uri? originalImage = null;
        try { originalImage = LockScreen.OriginalImageFile; }
        catch (Exception) { }

        bool hasLocalImage = false;
        bool isAppTemporaryImage = false;
        if (originalImage is { IsAbsoluteUri: true, IsFile: true })
        {
            try
            {
                string path = originalImage.LocalPath;
                hasLocalImage = File.Exists(path);
                isAppTemporaryImage = hasLocalImage && Path.GetFileName(path)
                    .StartsWith(TemporaryImagePrefix, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or SecurityException)
            {
                hasLocalImage = false;
                isAppTemporaryImage = false;
            }
        }

        bool canChangeTemporarily = profileApiSupported && (mode switch
        {
            LockScreenBackgroundMode.Spotlight or LockScreenBackgroundMode.Slideshow => true,
            LockScreenBackgroundMode.Picture => hasLocalImage && !isAppTemporaryImage,
            _ => false
        });

        string message = !profileApiSupported
            ? "Windows belum mengonfirmasi dukungan perubahan gambar lock screen. Fitur tetap nonaktif."
            : mode switch
            {
                LockScreenBackgroundMode.Spotlight when canChangeTemporarily => "Mode Spotlight: gambar saja bisa diganti sementara. Setelah durasi pilihan habis, aplikasi mencoba kembali ke Spotlight. Fitur gambar tidak mengunci komputer.",
                LockScreenBackgroundMode.Slideshow when canChangeTemporarily => "Mode slideshow: gambar saja bisa diganti sementara. Setelah durasi pilihan habis, aplikasi mencoba kembali ke slideshow. Fitur gambar tidak mengunci komputer.",
                LockScreenBackgroundMode.Picture when isAppTemporaryImage => "Gambar sementara milik aplikasi terdeteksi. Tanpa jurnal pemulihan yang valid, gambar tidak akan ditimpa.",
                LockScreenBackgroundMode.Picture when hasLocalImage => "Mode Picture terdeteksi dan file gambar lokal tersedia untuk salinan pemulihan setelah durasi pilihan habis.",
                LockScreenBackgroundMode.Picture => "File gambar lock screen lokal untuk pemulihan tidak tersedia. Fitur tetap nonaktif.",
                LockScreenBackgroundMode.Unknown => "Mode lock screen tidak dapat dipastikan secara aman dari status Windows; fitur tetap nonaktif.",
                _ => "Fitur lock screen tidak tersedia pada kondisi saat ini."
            };

        return new LockScreenSafetyInfo(profileApiSupported, mode, hasLocalImage, isAppTemporaryImage, canChangeTemporarily, message);
    }

    private static int? ReadInteger(string keyPath, string valueName)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
            // A missing key or value means the feature was never configured (disabled), not an error.
            if (key is null)
                return 0;
            if (!key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
                return 0;
            // A present but malformed value is ambiguous; fail closed.
            if (key.GetValueKind(valueName) != RegistryValueKind.DWord)
                return null;
            return key.GetValue(valueName) is int value && value is (0 or 1) ? value : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            return null;
        }
    }
}
