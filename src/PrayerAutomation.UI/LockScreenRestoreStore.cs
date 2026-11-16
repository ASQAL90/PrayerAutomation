using System;
using System.IO;
using System.Text.Json;

namespace PrayerAutomation.UI;

internal static class LockScreenStatePaths
{
    internal const string TemporaryImagePrefix = "PrayerAutomation-LockScreen-Temporary-";
    internal const string RestoreImagePrefix = "PrayerAutomation-Restore-Original-";
    internal const string JournalFileName = "lock-screen-restore.json";

    internal static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PrayerAutomation",
        "LockScreen");

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

internal sealed class FileLockScreenRestoreStore : ILockScreenRestoreStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };
    private readonly string _directory;
    private readonly string _journalPath;
    private readonly object _sync = new();

    internal FileLockScreenRestoreStore(string? directory = null)
    {
        _directory = Path.GetFullPath(directory ?? LockScreenStatePaths.DefaultDirectory);
        _journalPath = Path.Combine(_directory, LockScreenStatePaths.JournalFileName);
    }

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

    public void Delete()
    {
        lock (_sync)
        {
            if (File.Exists(_journalPath)) File.Delete(_journalPath);
        }
    }

    private void ThrowIfRecoveryBlocked()
    {
        if (Directory.Exists(_directory) && Directory.EnumerateFiles(_directory,
                LockScreenStatePaths.JournalFileName + ".invalid-*", SearchOption.TopDirectoryOnly).Any())
            throw new InvalidDataException("A quarantined lock-screen journal requires explicit review; new image changes are blocked.");
    }

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

    private static bool IsSha256(string? value)
    {
        if (value is null || value.Length != 64) return false;
        foreach (char c in value)
            if (!Uri.IsHexDigit(c)) return false;
        return true;
    }
}
