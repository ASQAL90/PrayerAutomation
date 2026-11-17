// File-backed journal store for restoring the original lock-screen state after a temporary change.
using System;
using System.IO;
using System.Text.Json;

namespace PrayerAutomation.UI;

/// <summary>Shared paths and managed-file checks for lock-screen temporary and restore images and the journal.</summary>
internal static class LockScreenStatePaths
{
    internal const string TemporaryImagePrefix = "PrayerAutomation-LockScreen-Temporary-";
    internal const string RestoreImagePrefix = "PrayerAutomation-Restore-Original-";
    internal const string JournalFileName = "lock-screen-restore.json";

    /// <summary>Default per-user directory used to store lock-screen restore state.</summary>
    internal static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PrayerAutomation",
        "LockScreen");

    /// <summary>Returns true when the path lives in the managed directory and starts with the expected prefix.</summary>
    /// <param name="path">Candidate file path.</param>
    /// <param name="expectedPrefix">Required file-name prefix.</param>
    /// <param name="directory">Managed directory that must contain the file.</param>
    /// <returns><c>true</c> if the path is a managed file.</returns>
    internal static bool IsManagedFile(string path, string expectedPrefix, string directory)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        try
        {
            string fullPath = Path.GetFullPath(path);
            string fullDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
            string directoryPrefix = fullDirectory + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase) &&
                   Path.GetFileName(fullPath).StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            return false;
        }
    }
}

/// <summary>JSON-file implementation of <see cref="ILockScreenRestoreStore"/> with atomic writes and quarantine handling.</summary>
internal sealed class FileLockScreenRestoreStore : ILockScreenRestoreStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };
    private readonly string _directory;
    private readonly string _journalPath;
    private readonly object _sync = new();

    /// <summary>Initializes the store rooted at the given directory, or the default per-user directory.</summary>
    /// <param name="directory">Override directory; defaults to <see cref="LockScreenStatePaths.DefaultDirectory"/>.</param>
    internal FileLockScreenRestoreStore(string? directory = null)
    {
        _directory = Path.GetFullPath(directory ?? LockScreenStatePaths.DefaultDirectory);
        _journalPath = Path.Combine(_directory, LockScreenStatePaths.JournalFileName);
    }

    /// <summary>Loads and validates the restore snapshot, or null when no journal exists.</summary>
    /// <returns>The validated snapshot.</returns>
    /// <exception cref="InvalidDataException">Thrown when the journal is invalid or a quarantine is pending review.</exception>
    public LockScreenRestoreSnapshot? Load()
    {
        lock (_sync)
        {
            ThrowIfRecoveryBlocked();
            if (!File.Exists(_journalPath)) return null;
            try
            {
                using FileStream stream = new(_journalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                LockScreenRestoreSnapshot? snapshot = JsonSerializer.Deserialize<LockScreenRestoreSnapshot>(stream, SerializerOptions);
                if (snapshot is null)
                    throw new InvalidDataException("Lock-screen restoration journal is empty.");
                Validate(snapshot);
                return snapshot;
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException)
            {
                QuarantineInvalidJournal();
                throw new InvalidDataException("Lock-screen restoration journal is invalid; the current lock-screen state was left unchanged.", ex);
            }
        }
    }

    /// <summary>Validates and atomically writes the restore snapshot to the journal.</summary>
    /// <param name="snapshot">Snapshot to persist.</param>
    /// <exception cref="InvalidDataException">Thrown when the snapshot fails validation or a quarantine is pending review.</exception>
    public void Save(LockScreenRestoreSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Validate(snapshot);
        lock (_sync)
        {
            ThrowIfRecoveryBlocked();
            Directory.CreateDirectory(_directory);
            string temporaryPath = _journalPath + ".writing-" + Guid.NewGuid().ToString("N");
            try
            {
                using (FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, snapshot, SerializerOptions);
                    stream.Flush(flushToDisk: true);
                }
                File.Move(temporaryPath, _journalPath, overwrite: true);
            }
            finally
            {
                try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        }
    }

    /// <summary>Deletes the restore journal if it exists.</summary>
    public void Delete()
    {
        lock (_sync)
        {
            if (File.Exists(_journalPath)) File.Delete(_journalPath);
        }
    }

    /// <summary>Blocks new operations when a quarantined journal is awaiting explicit review.</summary>
    private void ThrowIfRecoveryBlocked()
    {
        if (Directory.Exists(_directory) && Directory.EnumerateFiles(_directory,
                LockScreenStatePaths.JournalFileName + ".invalid-*", SearchOption.TopDirectoryOnly).Any())
            throw new InvalidDataException("A quarantined lock-screen journal requires explicit review; new image changes are blocked.");
    }

    /// <summary>Renames an unreadable journal aside so it can be reviewed manually.</summary>
    private void QuarantineInvalidJournal()
    {
        try
        {
            if (!File.Exists(_journalPath)) return;
            string invalidPath = _journalPath + ".invalid-" + DateTime.UtcNow.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);
            File.Move(_journalPath, invalidPath, overwrite: false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    /// <summary>Validates version, mode consistency, and managed image paths/hashes of a snapshot.</summary>
    /// <param name="snapshot">Snapshot to validate.</param>
    /// <exception cref="InvalidDataException">Thrown when the snapshot is inconsistent or unsafe.</exception>
    private void Validate(LockScreenRestoreSnapshot snapshot)
    {
        if (snapshot.Version != 1 || snapshot.RestoreAtUtc == default)
            throw new InvalidDataException("Unsupported or incomplete lock-screen journal version.");
        if (snapshot.PreviousMode is not (LockScreenBackgroundMode.Picture or LockScreenBackgroundMode.Slideshow or LockScreenBackgroundMode.Spotlight))
            throw new InvalidDataException("Unknown previous lock-screen mode.");
        if (LockScreenModeLogic.Resolve(snapshot.PreviousSlideshowEnabled, snapshot.PreviousSpotlightEnabled) != snapshot.PreviousMode)
            throw new InvalidDataException("Lock-screen mode flags do not match the saved mode.");
        if (!LockScreenStatePaths.IsManagedFile(snapshot.TemporaryImagePath, LockScreenStatePaths.TemporaryImagePrefix, _directory) ||
            !IsSha256(snapshot.TemporaryImageSha256))
            throw new InvalidDataException("Temporary lock-screen image path or hash is invalid.");

        if (snapshot.PreviousMode == LockScreenBackgroundMode.Picture)
        {
            if (!LockScreenStatePaths.IsManagedFile(snapshot.PreviousImagePath ?? "", LockScreenStatePaths.RestoreImagePrefix, _directory) ||
                !IsSha256(snapshot.PreviousImageSha256))
                throw new InvalidDataException("Previous Picture-mode image backup is invalid.");
        }
        else if (snapshot.PreviousImagePath is not null || snapshot.PreviousImageSha256 is not null)
        {
            throw new InvalidDataException("A Spotlight/slideshow journal must not contain a Picture-mode image backup.");
        }
    }

    /// <summary>Returns true when the value is a 64-character hex string, i.e. a SHA-256 hash.</summary>
    private static bool IsSha256(string? value)
    {
        if (value is null || value.Length != 64) return false;
        foreach (char c in value)
            if (!Uri.IsHexDigit(c)) return false;
        return true;
    }
}
