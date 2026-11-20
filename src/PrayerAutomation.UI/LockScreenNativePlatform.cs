using System.IO;
using Microsoft.Win32;
using Windows.Storage;
using Windows.System.UserProfile;

namespace PrayerAutomation.UI;

/// <summary>Native interop abstraction for reading and writing lock-screen state.</summary>
internal interface ILockScreenNativePlatform
{
    /// <summary>Returns true when the Windows user-profile personalization API is supported.</summary>
    /// <returns>True when supported.</returns>
    bool IsSupported();
    /// <summary>Reads the raw SlideshowEnabled and RotatingLockScreenEnabled registry flags.</summary>
    /// <returns>The raw flag pair.</returns>
    (int SlideshowEnabled, int SpotlightEnabled) ReadModeFlags();
    /// <summary>Returns the path of the current local lock-screen image file.</summary>
    /// <returns>The local file path.</returns>
    string GetCurrentLocalImagePath();
    /// <summary>Sets the lock-screen image, refusing the write if <paramref name="canWrite"/> turns false.</summary>
    /// <param name="imagePath">Path of the image file to set.</param>
    /// <param name="canWrite">Ownership callback consulted before and during the write.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetImageAsync(string imagePath, Func<bool> canWrite, CancellationToken cancellationToken);
    /// <summary>Writes the slideshow/spotlight mode flags, disabling the incompatible feature first.</summary>
    /// <param name="slideshowEnabled">Slideshow flag to write (0/1).</param>
    /// <param name="spotlightEnabled">Spotlight flag to write (0/1).</param>
    /// <param name="canWrite">Optional ownership callback consulted before and during the write.</param>
    void WriteModeFlags(int slideshowEnabled, int spotlightEnabled, Func<bool>? canWrite = null);
}

/// <summary>Windows implementation of <see cref="ILockScreenNativePlatform"/> using the WinRT LockScreen API and current-user registry.</summary>
internal sealed class WindowsLockScreenNativePlatform : ILockScreenNativePlatform
{
    /// <summary>Registry path of the slideshow mode setting.</summary>
    internal const string SlideshowRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Lock Screen";
    /// <summary>Registry path of the spotlight (rotating) lock-screen setting.</summary>
    internal const string SpotlightRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";

    /// <inheritdoc/>
    public bool IsSupported() => UserProfilePersonalizationSettings.IsSupported();

    /// <inheritdoc/>
    public (int SlideshowEnabled, int SpotlightEnabled) ReadModeFlags()
    {
        int slideshowEnabled = ReadRequiredDword(SlideshowRegistryPath, "SlideshowEnabled");
        int spotlightEnabled = ReadRequiredDword(SpotlightRegistryPath, "RotatingLockScreenEnabled");
        if (LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled) == LockScreenBackgroundMode.Unknown)
            throw new InvalidOperationException("The current lock-screen mode flags are missing, malformed, or conflicting.");
        return (slideshowEnabled, spotlightEnabled);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task SetImageAsync(string imagePath, Func<bool> canWrite, CancellationToken cancellationToken)
    {
        StorageFile image = await StorageFile.GetFileFromPathAsync(imagePath).AsTask(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (!canWrite())
            throw new InvalidOperationException("The lock-screen source changed while Windows prepared the image; the write was refused.");
        // SetImageFileAsync returns an IAsyncAction (no success flag). Await completion
        // here; the backend verifies the exposed image path and SHA256 before it commits.
        await LockScreen.SetImageFileAsync(image).AsTask().ConfigureAwait(false);
    }

    /// <inheritdoc/>
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

    /// <summary>Writes mode flags with ownership checks; disables the incompatible feature before enabling the requested one.</summary>
    /// <param name="slideshowEnabled">Slideshow flag to write (0/1).</param>
    /// <param name="spotlightEnabled">Spotlight flag to write (0/1).</param>
    /// <param name="writeSlideshow">Writes the slideshow flag.</param>
    /// <param name="writeSpotlight">Writes the spotlight flag.</param>
    /// <param name="readSlideshow">Reads the slideshow flag back.</param>
    /// <param name="readSpotlight">Reads the spotlight flag back.</param>
    /// <param name="canWrite">Optional ownership callback consulted before, during and after the write.</param>
    /// <exception cref="ArgumentOutOfRangeException">A flag is not 0/1 or the combination is conflicting.</exception>
    /// <exception cref="InvalidOperationException">The write was refused or a disabled-state check failed.</exception>
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

    /// <summary>Reads a required DWord flag; missing keys/values mean disabled, malformed values fail closed.</summary>
    /// <param name="keyPath">Registry key path under HKCU.</param>
    /// <param name="valueName">Value name to read.</param>
    /// <returns>0 or 1.</returns>
    /// <exception cref="InvalidOperationException">The value exists but is not a valid 0/1 DWord.</exception>
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
