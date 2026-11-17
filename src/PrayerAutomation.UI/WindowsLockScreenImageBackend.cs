using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;


namespace PrayerAutomation.UI;

/// <summary>Windows implementation of the lock-screen image backend: snapshot, apply, verify, restore, cleanup.</summary>
internal sealed class WindowsLockScreenImageBackend : ILockScreenImageBackend
{
    private readonly ILockScreenNativePlatform _native;
    private readonly string _directory;
    private readonly Action<string, Exception>? _log;
    private readonly Action<string, string>? _trace;

    /// <summary>Initializes the backend.</summary>
    /// <param name="log">Optional error log sink.</param>
    /// <param name="directory">Override for the managed files directory.</param>
    /// <param name="native">Optional native interop override (for testing).</param>
    /// <param name="trace">Optional trace sink for timeline events.</param>
    internal WindowsLockScreenImageBackend(Action<string, Exception>? log = null, string? directory = null,
        ILockScreenNativePlatform? native = null, Action<string, string>? trace = null)
    {
        _native = native ?? new WindowsLockScreenNativePlatform();
        _log = log;
        _trace = trace;
        _directory = Path.GetFullPath(directory ?? LockScreenStatePaths.DefaultDirectory);
    }

    /// <inheritdoc/>
    public LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset restoreAtUtc)
    {
        if (!_native.IsSupported())
            throw new InvalidOperationException("The Windows user-profile lock-screen API is not supported.");
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            throw new FileNotFoundException("The bundled prayer lock-screen image is missing.", imagePath);

        Directory.CreateDirectory(_directory);
        string? temporaryImagePath = null;
        string? previousImagePath = null;
        try
        {
            string? currentLocalImagePath = null;
            (int slideshowEnabled, int spotlightEnabled) = _native.ReadModeFlags();
            LockScreenBackgroundMode mode = LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled);
            if (mode == LockScreenBackgroundMode.Unknown)
                throw new InvalidOperationException("The previous lock-screen mode cannot be identified safely.");

            if (mode is LockScreenBackgroundMode.Spotlight or LockScreenBackgroundMode.Slideshow)
            {
                // A dynamic mode can retain the last Picture URI; it is not the displayed image.
                // Keep its file protected during cleanup, but restore these modes without a Picture backup.
                currentLocalImagePath = TryGetCurrentLocalImagePath();
            }

            string imageExtension = RequireSupportedImageExtension(imagePath);
            temporaryImagePath = Path.Combine(_directory,
                LockScreenStatePaths.TemporaryImagePrefix + Guid.NewGuid().ToString("N") + imageExtension);
            CopyAndVerify(imagePath, temporaryImagePath);
            string temporaryImageHash = ComputeSha256(temporaryImagePath);

            string? previousImageHash = null;
            if (mode == LockScreenBackgroundMode.Picture)
            {
                string originalImagePath = GetCurrentLocalImagePath();
                currentLocalImagePath = originalImagePath;
                if (Path.GetFileName(originalImagePath).StartsWith(LockScreenStatePaths.TemporaryImagePrefix, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("An unowned temporary lock-screen image is active; it will not be overwritten.");

                string originalExtension = RequireSupportedImageExtension(originalImagePath);
                previousImagePath = Path.Combine(_directory,
                    LockScreenStatePaths.RestoreImagePrefix + Guid.NewGuid().ToString("N") + originalExtension);
                CopyAndVerify(originalImagePath, previousImagePath);
                previousImageHash = ComputeSha256(previousImagePath);
            }

            var snapshot = new LockScreenRestoreSnapshot(
                Version: 1,
                PreviousMode: mode,
                PreviousSlideshowEnabled: slideshowEnabled,
                PreviousSpotlightEnabled: spotlightEnabled,
                PreviousImagePath: previousImagePath,
                PreviousImageSha256: previousImageHash,
                TemporaryImagePath: temporaryImagePath,
                TemporaryImageSha256: temporaryImageHash,
                RestoreAtUtc: restoreAtUtc,
                IsApplied: false);
            CleanupOrphanedManagedFiles(currentLocalImagePath, temporaryImagePath, previousImagePath);
            Trace("snapshot", $"previousMode={mode} slideshowFlag={slideshowEnabled} spotlightFlag={spotlightEnabled} visual=unverified");
            return snapshot;
        }
        catch
        {
            DeleteManagedFile(temporaryImagePath, LockScreenStatePaths.TemporaryImagePrefix);
            DeleteManagedFile(previousImagePath, LockScreenStatePaths.RestoreImagePrefix);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ValidateSnapshotPaths(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_native.IsSupported())
            throw new InvalidOperationException("The Windows user-profile lock-screen API is not supported.");
        if (!IsRestored(snapshot))
            throw new InvalidOperationException("The lock-screen state changed after its recovery snapshot was captured.");
        if (!string.Equals(ComputeSha256(snapshot.TemporaryImagePath), snapshot.TemporaryImageSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The temporary lock-screen image does not match its saved hash.");

        await _native.SetImageAsync(snapshot.TemporaryImagePath, () => IsRestored(snapshot), cancellationToken).ConfigureAwait(false);
        if (CheckTemporaryImage(snapshot) != LockScreenImageOwnership.TemporaryImage)
            throw new InvalidOperationException("Windows did not expose the newly selected lock-screen image for verification.");

        _native.WriteModeFlags(slideshowEnabled: 0, spotlightEnabled: 0,
            canWrite: () => CheckTemporaryImage(snapshot) == LockScreenImageOwnership.TemporaryImage);
        (int actualSlideshow, int actualSpotlight) = _native.ReadModeFlags();
        if (LockScreenModeLogic.Resolve(actualSlideshow, actualSpotlight) != LockScreenBackgroundMode.Picture)
            throw new InvalidOperationException("Windows did not accept the temporary Picture mode for the lock screen.");
        if (CheckTemporaryImage(snapshot) != LockScreenImageOwnership.TemporaryImage)
            throw new InvalidOperationException("The temporary lock-screen image changed before verification completed.");
    }

    /// <inheritdoc/>
    public LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot)
    {
        try
        {
            ValidateSnapshotPaths(snapshot);
            (int slideshowEnabled, int spotlightEnabled) = _native.ReadModeFlags();
            return ClassifyImageOwnership(snapshot, TryGetCurrentLocalImagePath(),
                LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled));
        }
        catch (Exception ex) when (IsVerificationFailure(ex))
        {
            return LockScreenImageOwnership.Unknown;
        }
    }

    /// <summary>Classifies how the current image relates to a snapshot without touching global state.</summary>
    /// <param name="snapshot">The snapshot to classify against.</param>
    /// <param name="currentImagePath">Path of the currently exposed image, or null when unavailable.</param>
    /// <param name="currentMode">The currently resolved background mode.</param>
    /// <returns>The ownership classification.</returns>
    internal static LockScreenImageOwnership ClassifyImageOwnership(LockScreenRestoreSnapshot snapshot,
        string? currentImagePath, LockScreenBackgroundMode currentMode)
    {
        try
        {
            if (currentMode == LockScreenBackgroundMode.Unknown || string.IsNullOrWhiteSpace(currentImagePath) ||
                !File.Exists(currentImagePath) || !File.Exists(snapshot.TemporaryImagePath) ||
                !string.Equals(ComputeSha256(snapshot.TemporaryImagePath), snapshot.TemporaryImageSha256, StringComparison.OrdinalIgnoreCase))
                return LockScreenImageOwnership.Unknown;
            string currentHash = ComputeSha256(currentImagePath);
            if (snapshot.RestoreRequested && snapshot.PreviousImagePath is not null &&
                PathsEqual(currentImagePath, snapshot.PreviousImagePath) &&
                string.Equals(currentHash, snapshot.PreviousImageSha256, StringComparison.OrdinalIgnoreCase))
                return LockScreenImageOwnership.RestorationImage;
            if (PathsEqual(currentImagePath, snapshot.TemporaryImagePath) &&
                string.Equals(currentHash, snapshot.TemporaryImageSha256, StringComparison.OrdinalIgnoreCase))
                return snapshot.IsApplied && !snapshot.RestoreRequested && currentMode != LockScreenBackgroundMode.Picture
                    ? LockScreenImageOwnership.TemporaryImageModeDrift
                    : LockScreenImageOwnership.TemporaryImage;
            return LockScreenImageOwnership.DifferentImage;
        }
        catch (Exception ex) when (IsVerificationFailure(ex))
        {
            return LockScreenImageOwnership.Unknown;
        }
    }

    /// <inheritdoc/>
    public async Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ValidateSnapshotPaths(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        if (!snapshot.IsApplied || snapshot.RestoreRequested ||
            CheckTemporaryImage(snapshot) != LockScreenImageOwnership.TemporaryImageModeDrift)
            throw new InvalidOperationException("Only confirmed mode drift on the active application-owned image may be repaired.");

        bool StillOwned() => CheckTemporaryImage(snapshot) is
            LockScreenImageOwnership.TemporaryImage or LockScreenImageOwnership.TemporaryImageModeDrift;
        // Reapply only after verified mode drift, never on healthy ticks or a different source.
        await _native.SetImageAsync(snapshot.TemporaryImagePath, StillOwned, cancellationToken).ConfigureAwait(false);
        _native.WriteModeFlags(slideshowEnabled: 0, spotlightEnabled: 0, canWrite: StillOwned);
        if (CheckTemporaryImage(snapshot) != LockScreenImageOwnership.TemporaryImage)
            throw new InvalidOperationException("Windows did not confirm the maintained temporary Picture image.");
    }

    /// <inheritdoc/>
    public async Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ValidateSnapshotPaths(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        TraceNativeState("restore-before", snapshot);

        if (snapshot.PreviousMode == LockScreenBackgroundMode.Picture)
        {
            string previousImagePath = snapshot.PreviousImagePath ??
                throw new InvalidDataException("The previous Picture-mode image backup is missing.");
            if (!File.Exists(previousImagePath) ||
                !string.Equals(ComputeSha256(previousImagePath), snapshot.PreviousImageSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The previous Picture-mode image backup failed validation.");

            await _native.SetImageAsync(previousImagePath,
                () => CheckTemporaryImage(snapshot) is LockScreenImageOwnership.TemporaryImage or LockScreenImageOwnership.RestorationImage,
                cancellationToken).ConfigureAwait(false);
            if (!CurrentImageMatches(snapshot.PreviousImageSha256))
                throw new InvalidOperationException("Windows did not confirm restoration of the previous Picture-mode image.");
        }

        _native.WriteModeFlags(snapshot.PreviousSlideshowEnabled, snapshot.PreviousSpotlightEnabled,
            canWrite: () => CheckTemporaryImage(snapshot) is
                LockScreenImageOwnership.TemporaryImage or LockScreenImageOwnership.RestorationImage);
        if (!IsRestored(snapshot))
            throw new InvalidOperationException("The previous lock-screen mode or image could not be verified after restoration.");
        TraceNativeState("restore-after", snapshot);
    }

    /// <inheritdoc/>
    public bool IsRestored(LockScreenRestoreSnapshot snapshot)
    {
        try
        {
            (int slideshowEnabled, int spotlightEnabled) = _native.ReadModeFlags();
            if (slideshowEnabled != snapshot.PreviousSlideshowEnabled || spotlightEnabled != snapshot.PreviousSpotlightEnabled ||
                LockScreenModeLogic.Resolve(slideshowEnabled, spotlightEnabled) != snapshot.PreviousMode)
                return false;

            return snapshot.PreviousMode != LockScreenBackgroundMode.Picture || CurrentImageMatches(snapshot.PreviousImageSha256);
        }
        catch (Exception ex) when (IsVerificationFailure(ex))
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void Cleanup(LockScreenRestoreSnapshot snapshot)
    {
        DeleteManagedFile(snapshot.TemporaryImagePath, LockScreenStatePaths.TemporaryImagePrefix);
        DeleteManagedFile(snapshot.PreviousImagePath, LockScreenStatePaths.RestoreImagePrefix);
    }

    /// <summary>Returns the current local image path via the native platform.</summary>
    /// <returns>The local file path.</returns>
    private string GetCurrentLocalImagePath() => _native.GetCurrentLocalImagePath();

    /// <summary>Checks whether the current image matches an expected SHA-256 hash.</summary>
    /// <param name="expectedSha256">Expected hash, or null/empty.</param>
    /// <returns>True when the current image hash matches.</returns>
    private bool CurrentImageMatches(string? expectedSha256)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256)) return false;
        return string.Equals(ComputeSha256(GetCurrentLocalImagePath()), expectedSha256, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Validates the image extension (jpg/jpeg/png/bmp) and returns it.</summary>
    /// <param name="path">Image path to check.</param>
    /// <returns>The extension as found.</returns>
    /// <exception cref="InvalidDataException">The extension is not supported.</exception>
    private static string RequireSupportedImageExtension(string path)
    {
        string extension = Path.GetExtension(path);
        if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
            return extension;
        throw new InvalidDataException("Only JPG, PNG, or BMP lock-screen image files can be snapshotted safely.");
    }

    /// <summary>Copies a file and verifies SHA-256 hashes match; deletes the copy on mismatch.</summary>
    /// <param name="sourcePath">Source file.</param>
    /// <param name="destinationPath">Destination file (must not exist).</param>
    /// <exception cref="IOException">Hash mismatch after copy.</exception>
    private static void CopyAndVerify(string sourcePath, string destinationPath)
    {
        File.Copy(sourcePath, destinationPath, overwrite: false);
        string sourceHash = ComputeSha256(sourcePath);
        string destinationHash = ComputeSha256(destinationPath);
        if (!string.Equals(sourceHash, destinationHash, StringComparison.OrdinalIgnoreCase))
        {
            try { File.Delete(destinationPath); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            throw new IOException("A lock-screen image snapshot did not match its source file.");
        }
    }

    /// <summary>Computes the SHA-256 hex hash of a file.</summary>
    /// <param name="path">File to hash.</param>
    /// <returns>Uppercase hex hash string.</returns>
    private static string ComputeSha256(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    /// <summary>Deletes an application-managed file unless it is the current lock-screen image; logs failures.</summary>
    /// <param name="path">Candidate path, or null.</param>
    /// <param name="prefix">Required managed-file prefix.</param>
    private void DeleteManagedFile(string? path, string prefix)
    {
        if (!LockScreenStatePaths.IsManagedFile(path ?? "", prefix, _directory)) return;
        try
        {
            string? currentImagePath = TryGetCurrentLocalImagePath();
            if (currentImagePath is not null && PathsEqual(currentImagePath, path!)) return;
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            _log?.Invoke("lock-screen.cleanup", ex);
        }
    }

    /// <summary>Deletes managed files not in <paramref name="preservePaths"/>; logs failures.</summary>
    /// <param name="preservePaths">Paths to keep.</param>
    private void CleanupOrphanedManagedFiles(params string?[] preservePaths)
    {
        try
        {
            foreach (string prefix in new[] { LockScreenStatePaths.TemporaryImagePrefix, LockScreenStatePaths.RestoreImagePrefix })
            {
                foreach (string file in Directory.EnumerateFiles(_directory, prefix + "*", SearchOption.TopDirectoryOnly))
                {
                    if (preservePaths.Any(path => path is not null && PathsEqual(path, file))) continue;
                    DeleteManagedFile(file, prefix);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            _log?.Invoke("lock-screen.orphan-cleanup", ex);
        }
    }

    /// <summary>Returns the current local image path, or null when verification fails.</summary>
    /// <returns>The path, or null.</returns>
    private string? TryGetCurrentLocalImagePath()
    {
        try { return GetCurrentLocalImagePath(); }
        catch (Exception ex) when (IsVerificationFailure(ex)) { return null; }
    }

    /// <summary>Compares two paths by full path, case-insensitively.</summary>
    /// <param name="first">First path.</param>
    /// <param name="second">Second path.</param>
    /// <returns>True when the full paths are equal; false on any error.</returns>
    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or SecurityException)
        {
            return false;
        }
    }

    /// <summary>Validates snapshot version, managed paths and hashes; throws on anything else.</summary>
    /// <param name="snapshot">The snapshot to validate.</param>
    /// <exception cref="InvalidDataException">The snapshot is not a valid application-owned transaction.</exception>
    private void ValidateSnapshotPaths(LockScreenRestoreSnapshot snapshot)
    {
        if (snapshot.Version != 1 ||
            !LockScreenStatePaths.IsManagedFile(snapshot.TemporaryImagePath, LockScreenStatePaths.TemporaryImagePrefix, _directory) ||
            string.IsNullOrWhiteSpace(snapshot.TemporaryImageSha256) || snapshot.TemporaryImageSha256.Length != 64)
            throw new InvalidDataException("The lock-screen snapshot is not a valid application-owned transaction.");
        if (snapshot.PreviousMode == LockScreenBackgroundMode.Picture)
        {
            if (!LockScreenStatePaths.IsManagedFile(snapshot.PreviousImagePath ?? "", LockScreenStatePaths.RestoreImagePrefix, _directory) ||
                string.IsNullOrWhiteSpace(snapshot.PreviousImageSha256) || snapshot.PreviousImageSha256.Length != 64)
                throw new InvalidDataException("The previous Picture-mode image backup is invalid.");
        }
        else if (snapshot.PreviousMode is not (LockScreenBackgroundMode.Spotlight or LockScreenBackgroundMode.Slideshow) ||
                 snapshot.PreviousImagePath is not null || snapshot.PreviousImageSha256 is not null)
        {
            throw new InvalidDataException("The saved previous lock-screen mode or image backup is invalid.");
        }
    }

    /// <summary>Returns true for exceptions that indicate a verification/IO failure rather than a bug.</summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>True when the exception is a verification failure.</returns>
    private static bool IsVerificationFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or
            InvalidOperationException or NotSupportedException or System.Runtime.InteropServices.COMException or
            System.Runtime.InteropServices.ExternalException or System.Threading.Tasks.TaskCanceledException;

    /// <summary>Traces mode flags and image role; failures are swallowed.</summary>
    /// <param name="action">Trace action name.</param>
    /// <param name="snapshot">The snapshot to describe.</param>
    private void TraceNativeState(string action, LockScreenRestoreSnapshot snapshot)
    {
        if (_trace is null) return;
        try
        {
            (int slideshow, int spotlight) = _native.ReadModeFlags();
            string role = "unknown";
            try
            {
                string current = _native.GetCurrentLocalImagePath();
                role = PathsEqual(current, snapshot.TemporaryImagePath) ? "temporary" : "other";
            }
            catch (Exception) { }
            Trace(action, $"mode={LockScreenModeLogic.Resolve(slideshow, spotlight)} slideshowFlag={slideshow} spotlightFlag={spotlight} apiUriRole={role} verification=config-only visual=unverified");
        }
        catch (Exception) { Trace(action, "category=unavailable visual=unverified"); }
    }

    /// <summary>Emits a privacy-safe backend trace entry; failures are swallowed.</summary>
    /// <param name="action">Trace action name.</param>
    /// <param name="detail">Trace detail payload.</param>
    private void Trace(string action, string detail)
    {
        try { _trace?.Invoke("lock-screen.backend." + action, $"utc={DateTimeOffset.UtcNow:O} {detail}"); }
        catch (Exception) { }
    }
}
