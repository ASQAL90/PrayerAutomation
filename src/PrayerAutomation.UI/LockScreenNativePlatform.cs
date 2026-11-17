using System.IO;
using Microsoft.Win32;
using Windows.Storage;
using Windows.System.UserProfile;

namespace PrayerAutomation.UI;

internal interface ILockScreenNativePlatform
{
    bool IsSupported();
    (int SlideshowEnabled, int SpotlightEnabled) ReadModeFlags();
    string GetCurrentLocalImagePath();
    Task SetImageAsync(string imagePath, Func<bool> canWrite, CancellationToken cancellationToken);
    void WriteModeFlags(int slideshowEnabled, int spotlightEnabled, Func<bool>? canWrite = null);
}

internal sealed class WindowsLockScreenNativePlatform : ILockScreenNativePlatform
{
    internal const string SlideshowRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Lock Screen";
    internal const string SpotlightRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";

    public bool IsSupported() => UserProfilePersonalizationSettings.IsSupported();

    public (int SlideshowEnabled, int SpotlightEnabled) ReadModeFlags()
    {
        int slideshowEnabled = ReadRequiredDword(SlideshowRegistryPath, "SlideshowEnabled");
        int spotlightEnabled = ReadRequiredDword(SpotlightRegistryPath, "RotatingLockScreenEnabled");
        if (LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled) == LockScreenBackgroundMode.Unknown)
            throw new InvalidOperationException("The current lock-screen mode flags are missing, malformed, or conflicting.");
        return (slideshowEnabled, spotlightEnabled);
    }

    public string GetCurrentLocalImagePath()
    {
        Uri? image = LockScreen.OriginalImageFile;
        if (image is not { IsAbsoluteUri: true, IsFile: true })
            throw new InvalidOperationException("Windows did not expose a local lock-screen image file.");
        string path = image.LocalPath;
        if (!File.Exists(path))
            throw new FileNotFoundException("The current lock-screen image file is not accessible.", path);
        return Path.GetFullPath(path);
    }

    public async Task SetImageAsync(string imagePath, Func<bool> canWrite, CancellationToken cancellationToken)
    {
        StorageFile image = await StorageFile.GetFileFromPathAsync(imagePath).AsTask(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (!canWrite())
            throw new InvalidOperationException("The lock-screen source changed while Windows prepared the image; the write was refused.");
        // Once the native write starts, await its completion before rollback can inspect its effect.
        await LockScreen.SetImageFileAsync(image).AsTask().ConfigureAwait(false);
    }

    public void WriteModeFlags(int slideshowEnabled, int spotlightEnabled, Func<bool>? canWrite = null)
    {
        if (LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled) == LockScreenBackgroundMode.Unknown)
            throw new ArgumentOutOfRangeException(nameof(slideshowEnabled), "Refusing to write an invalid lock-screen mode.");
        using RegistryKey slideshowKey = Registry.CurrentUser.OpenSubKey(SlideshowRegistryPath, writable: true) ??
            throw new UnauthorizedAccessException("The current-user slideshow mode setting is not writable.");
        using RegistryKey spotlightKey = Registry.CurrentUser.OpenSubKey(SpotlightRegistryPath, writable: true) ??
            throw new UnauthorizedAccessException("The current-user Spotlight mode setting is not writable.");
        WriteModeFlagValues(slideshowEnabled, spotlightEnabled,
            value => slideshowKey.SetValue("SlideshowEnabled", value, RegistryValueKind.DWord),
            value => spotlightKey.SetValue("RotatingLockScreenEnabled", value, RegistryValueKind.DWord),
            () => ReadRequiredDword(SlideshowRegistryPath, "SlideshowEnabled"),
            () => ReadRequiredDword(SpotlightRegistryPath, "RotatingLockScreenEnabled"), canWrite);
        slideshowKey.Flush();
        spotlightKey.Flush();
        (int actualSlideshow, int actualSpotlight) = ReadModeFlags();
        if (actualSlideshow != slideshowEnabled || actualSpotlight != spotlightEnabled)
            throw new IOException("Windows did not retain the requested current-user lock-screen mode flags.");
    }

    internal static void WriteModeFlagValues(int slideshowEnabled, int spotlightEnabled,
        Action<int> writeSlideshow, Action<int> writeSpotlight,
        Func<int> readSlideshow, Func<int> readSpotlight, Func<bool>? canWrite = null)
    {
        if (LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled) == LockScreenBackgroundMode.Unknown)
            throw new ArgumentOutOfRangeException(nameof(slideshowEnabled), "Refusing to write an invalid lock-screen mode.");
        if (canWrite is not null && !canWrite())
            throw new InvalidOperationException("The lock-screen source changed before the mode write; the write was refused.");
        // Disable the incompatible feature before enabling the requested mode.
        if (slideshowEnabled == 1) writeSpotlight(spotlightEnabled);
        else writeSlideshow(slideshowEnabled);
        if (canWrite is not null && !canWrite())
            throw new InvalidOperationException("The lock-screen source changed during the mode write; further writes were refused.");
        // Read after the ownership callback: another process can re-enable the incompatible feature.
        if ((slideshowEnabled == 1 && readSpotlight() != 0) || (spotlightEnabled == 1 && readSlideshow() != 0))
            throw new InvalidOperationException("The incompatible lock-screen feature is not disabled; activation was refused.");
        if (slideshowEnabled == 1) writeSlideshow(slideshowEnabled);
        else writeSpotlight(spotlightEnabled);
    }

    private static int ReadRequiredDword(string keyPath, string valueName)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
        // A missing key or value means the feature was never configured (disabled), not an error.
        if (key is null)
            return 0;
        if (!key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
            return 0;
        // A present but malformed value is ambiguous; fail closed.
        if (key.GetValueKind(valueName) != RegistryValueKind.DWord || key.GetValue(valueName) is not int value || value is not (0 or 1))
            throw new InvalidOperationException($"Current-user lock-screen setting is not a valid DWORD: {valueName}.");
        return value;
    }
}
