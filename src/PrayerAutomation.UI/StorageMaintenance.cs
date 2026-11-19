// Storage housekeeping and sanitized diagnostics export for the settings page.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrayerAutomation.UI;

/// <summary>Reports and cleans the per-user log, update, and lock-screen folders.</summary>
internal static partial class StorageMaintenance
{
    private static readonly TimeSpan OldInstallerAge = TimeSpan.FromHours(1);
    private static readonly TimeSpan LockScreenImageAge = TimeSpan.FromDays(7);

    internal static string DataRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PrayerAutomation");

    internal static string UpdateFolder => Path.Combine(DataRoot, "Update");

    internal static string UpdateJobsFolder => Path.Combine(UpdateFolder, "jobs");

    internal static string LockScreenFolder => LockScreenStatePaths.DefaultDirectory;

    internal static string LogPath => Path.Combine(DataRoot, "prayer_automation.log");

    /// <summary>Returns the combined size of the active log and its rotated copies.</summary>
    internal static long GetLogBytes()
    {
        long total = 0;
        for (int index = 0; index < 3; index++)
        {
            string path = index == 0 ? LogPath : LogPath + "." + index;
            try
            {
                var file = new FileInfo(path);
                if (file.Exists) total += file.Length;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        return total;
    }

    /// <summary>Returns the size of a folder tree without following reparse points.</summary>
    internal static long GetFolderBytes(string path)
    {
        long total = 0;
        try
        {
            var root = new DirectoryInfo(path);
            if (!root.Exists) return 0;
            foreach (FileInfo file in root.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try { total += file.Length; }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
            foreach (DirectoryInfo folder in root.EnumerateDirectories())
            {
                if ((folder.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                total += GetFolderBytes(folder.FullName);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException) { }
        return total;
    }

    /// <summary>Formats a byte count for the settings page.</summary>
    internal static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes.ToString(CultureInfo.InvariantCulture) + " B";
        if (bytes < 1024 * 1024) return (bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB";
        return (bytes / (1024d * 1024d)).ToString("0.#", CultureInfo.InvariantCulture) + " MB";
    }

    /// <summary>Deletes installers left by a finished update and partial downloads; returns the number removed.</summary>
    internal static int DeleteOldInstallers()
    {
        int removed = 0;
        try
        {
            var folder = new DirectoryInfo(UpdateFolder);
            if (!folder.Exists) return 0;
            DateTime cutoff = DateTime.UtcNow - OldInstallerAge;
            foreach (FileInfo file in folder.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    bool partial = file.Extension.Equals(".part", StringComparison.OrdinalIgnoreCase);
                    if (!partial && !file.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!partial && file.LastWriteTimeUtc >= cutoff) continue;
                    file.Delete();
                    removed++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException) { }
        return removed;
    }

    /// <summary>Removes finished update job folders, but never an in-progress one.</summary>
    /// <returns>Number of job folders removed.</returns>
    internal static int DeleteFinishedUpdateJobs()
    {
        int removed = 0;
        try
        {
            var root = new DirectoryInfo(UpdateJobsFolder);
            if (!root.Exists) return 0;
            DateTime cutoff = DateTime.UtcNow - OldInstallerAge;
            foreach (DirectoryInfo folder in root.EnumerateDirectories())
            {
                if ((folder.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                try
                {
                    string journal = Path.Combine(folder.FullName, "journal.json");
                    if (!File.Exists(journal) || folder.LastWriteTimeUtc >= cutoff) continue;
                    string status = File.ReadAllText(journal);
                    if (!status.Contains("\"applied\"", StringComparison.Ordinal) &&
                        !status.Contains("\"failed\"", StringComparison.Ordinal)) continue;
                    foreach (FileInfo file in folder.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        if ((file.Attributes & FileAttributes.ReparsePoint) == 0) file.Delete();
                    }
                    folder.Delete(recursive: true);
                    removed++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException) { }
        return removed;
    }

    /// <summary>Deletes the active log and its rotated copies; returns bytes freed.</summary>
    internal static long ClearLogs()
    {
        long freed = GetLogBytes();
        for (int index = 0; index < 3; index++)
        {
            string path = index == 0 ? LogPath : LogPath + "." + index;
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        return freed;
    }

    /// <summary>Removes downloaded update files; returns how many bytes were freed.</summary>
    internal static long ClearUpdateDownloads()
    {
        long freed = GetFolderBytes(UpdateFolder);
        try
        {
            var folder = new DirectoryInfo(UpdateFolder);
            if (!folder.Exists) return 0;
            foreach (FileInfo file in folder.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    if ((file.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    file.Delete();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException) { }
        return freed;
    }

    /// <summary>
    /// Removes stale temporary lock-screen images. Pending restore journals are
    /// left untouched so an unfinished recovery is never broken by cleanup.
    /// </summary>
    /// <returns>Number of bytes freed, or 0 when recovery is pending.</returns>
    internal static long ClearLockScreenImages()
    {
        try
        {
            if (File.Exists(Path.Combine(LockScreenFolder, LockScreenStatePaths.JournalFileName))) return 0;
            var folder = new DirectoryInfo(LockScreenFolder);
            if (!folder.Exists) return 0;
            long freed = 0;
            DateTime cutoff = DateTime.UtcNow - LockScreenImageAge;
            foreach (FileInfo file in folder.EnumerateFiles("*.png", SearchOption.TopDirectoryOnly))
            {
                if (!file.Name.StartsWith(LockScreenStatePaths.TemporaryImagePrefix, StringComparison.OrdinalIgnoreCase) &&
                    !file.Name.StartsWith(LockScreenStatePaths.RestoreImagePrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (file.LastWriteTimeUtc >= cutoff) continue;
                try
                {
                    freed += file.Length;
                    file.Delete();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
            return freed;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
            return 0;
        }
    }

    /// <summary>True when a lock-screen restore journal still needs attention.</summary>
    internal static bool HasPendingLockScreenRecovery =>
        File.Exists(Path.Combine(LockScreenFolder, LockScreenStatePaths.JournalFileName));

    /// <summary>Builds a sanitized plain-text report suitable for sharing with support.</summary>
    internal static string BuildDiagnostics(AppSettings settings)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Ayo Jama'ah! — laporan diagnostik");
        builder.AppendLine("Dibuat: " + DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture));
        builder.AppendLine("Versi: " + AppBranding.Version);
        builder.AppendLine("Sistem: " + Environment.OSVersion.VersionString + " (" + RuntimeInformation.OSArchitecture + ")");
        builder.AppendLine("Runtime: " + RuntimeInformation.FrameworkDescription);
        builder.AppendLine();
        builder.AppendLine("Pengaturan (ringkas):");
        builder.AppendLine("- Metode: " + settings.CalculationStandard);
        builder.AppendLine("- Zona waktu: " + settings.TimeZoneId);
        builder.AppendLine("- Sumber kalender: " + (settings.UsesKhgtCalendar ? "KHGT" : "sipil"));
        builder.AppendLine("- Lokasi tersimpan: " + (string.IsNullOrWhiteSpace(settings.LocationName) ? "tidak bernama" : "ya"));
        builder.AppendLine("- Pengingat: " + (settings.RemindersEnabled ? "aktif" : "nonaktif"));
        builder.AppendLine("- Kunci otomatis: " + (settings.AutoLockEnabled ? "aktif" : "nonaktif"));
        builder.AppendLine("- Gambar lock screen: " + (settings.LockScreenEnabled && settings.LockScreenOptIn ? "aktif" : "nonaktif"));
        builder.AppendLine("- Pemulihan lock screen tertunda: " + (HasPendingLockScreenRecovery ? "ya" : "tidak"));
        builder.AppendLine();
        builder.AppendLine("Penyimpanan:");
        builder.AppendLine("- Log: " + FormatBytes(GetLogBytes()));
        builder.AppendLine("- Unduhan pembaruan: " + FormatBytes(GetFolderBytes(UpdateFolder)));
        builder.AppendLine("- Gambar lock screen: " + FormatBytes(GetFolderBytes(LockScreenFolder)));
        builder.AppendLine();
        builder.AppendLine("Cuplikan log terakhir (disanitasi):");
        builder.AppendLine(Sanitize(ReadLogTail(240)));
        return builder.ToString();
    }

    /// <summary>Reads at most the last 64 KB of the active log and returns its final lines.</summary>
    private static string ReadLogTail(int maxLines)
    {
        try
        {
            var file = new FileInfo(LogPath);
            if (!file.Exists) return "(belum ada log)";
            const int maxBytes = 64 * 1024;
            using FileStream stream = file.OpenRead();
            long start = Math.Max(0, stream.Length - maxBytes);
            stream.Seek(start, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            if (start > 0)
            {
                int newline = text.IndexOf('\n');
                if (newline >= 0) text = text[(newline + 1)..];
            }
            string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, lines.TakeLast(maxLines));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return "(log tidak bisa dibaca)";
        }
    }

    /// <summary>Removes user names, profile paths, and coordinate-like numbers from text.</summary>
    private static string Sanitize(string text)
    {
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(profile))
            text = text.Replace(profile, "%USERPROFILE%", StringComparison.OrdinalIgnoreCase);
        string userName = Environment.UserName;
        if (!string.IsNullOrWhiteSpace(userName) && userName.Length >= 3)
            text = text.Replace(userName, "<pengguna>", StringComparison.OrdinalIgnoreCase);
        return CoordinatePattern().Replace(text, "<koordinat>");
    }

    [GeneratedRegex(@"-?\d{1,3}\.\d{4,}")]
    private static partial Regex CoordinatePattern();
}
