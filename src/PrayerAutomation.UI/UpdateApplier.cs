// Helper-side application of a downloaded delta. Runs before WPF/single-instance startup.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PrayerAutomation.UI;

/// <summary>Applies a prepared update job from a copied AyoJamaah.exe helper.</summary>
internal static class UpdateApplier
{
    private const int WaitSeconds = 60;

    /// <summary>Handles the helper command line before the WPF application starts.</summary>
    /// <param name="args">Process arguments.</param>
    /// <param name="exitCode">Process exit code to use.</param>
    /// <returns>True when this process is the update helper.</returns>
    internal static bool TryHandleCommandLine(IReadOnlyList<string> args, out int exitCode)
    {
        exitCode = 0;
        for (int index = 0; index < args.Count; index++)
        {
            if (!string.Equals(args[index], "--apply-update", StringComparison.OrdinalIgnoreCase)) continue;
            if (index + 2 >= args.Count) return false;
            string jobPath = args[index + 1];
            if (!int.TryParse(args[index + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int oldPid))
                return false;
            exitCode = Apply(jobPath, oldPid);
            return true;
        }
        return false;
    }

    /// <summary>Waits for the old process, replaces files, then restarts the application.</summary>
    /// <param name="jobPath">Prepared job file.</param>
    /// <param name="oldPid">Process id of the application that started the helper.</param>
    /// <returns>Zero on success; non-zero when the update was rolled back.</returns>
    internal static int Apply(string jobPath, int oldPid)
    {
        if (!UpdateJob.TryLoad(jobPath, out UpdateJob? job) || job is null) return 2;
        var journal = new UpdateJournal
        {
            Status = "preparing",
            TargetVersion = job.TargetVersion,
            UpdatedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        };
        try
        {
            journal.Save(job.JournalPath);
            WaitForExit(oldPid);
            if (File.Exists(Path.Combine(LockScreenStatePaths.DefaultDirectory,
                    LockScreenStatePaths.JournalFileName)))
                throw new InvalidOperationException("Pemulihan lock screen masih tertunda.");

            var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var created = new List<string>();
            try
            {
                foreach (string relative in job.Files.Concat(job.Deleted))
                {
                    string source = Path.Combine(job.SourceFolder, ToLocal(relative));
                    string target = Path.Combine(job.TargetRoot, ToLocal(relative));
                    EnsureParent(target);
                    bool isDelete = job.Deleted.Any(item =>
                        string.Equals(item, relative, StringComparison.OrdinalIgnoreCase));
                    if (!isDelete && !File.Exists(source))
                        throw new FileNotFoundException("Berkas pembaruan tidak lengkap.", source);
                    if (File.Exists(target))
                    {
                        string backup = Path.Combine(job.BackupFolder, ToLocal(relative));
                        EnsureParent(backup);
                        File.Copy(target, backup, overwrite: true);
                        backups[target] = backup;
                    }
                    else if (!isDelete)
                    {
                        created.Add(target);
                    }
                }

                foreach (string relative in job.Files)
                {
                    string source = Path.Combine(job.SourceFolder, ToLocal(relative));
                    string target = Path.Combine(job.TargetRoot, ToLocal(relative));
                    File.Copy(source, target, overwrite: true);
                }
                foreach (string relative in job.Deleted)
                {
                    string target = Path.Combine(job.TargetRoot, ToLocal(relative));
                    if (File.Exists(target)) File.Delete(target);
                }

                if (job.IsInstalled) RefreshInstallManifest(job);
                journal.Status = "applied";
                journal.Message = string.Empty;
                journal.UpdatedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
                journal.Save(job.JournalPath);
            }
            catch (Exception ex)
            {
                Rollback(job, backups, created);
                journal.Status = "failed";
                journal.Message = ex.Message;
                journal.UpdatedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
                journal.Save(job.JournalPath);
                AppLogger.Write("update.apply", ex);
                Relaunch(job.TargetRoot, job.AppExe);
                return 1;
            }

            Relaunch(job.TargetRoot, job.AppExe);
            return 0;
        }
        catch (Exception ex)
        {
            AppLogger.Write("update.helper", ex);
            return 2;
        }
    }

    /// <summary>Returns the most recent failed update message, if one is still on disk.</summary>
    /// <returns>Failure message, or null when no failed update is recorded.</returns>
    internal static string? FindLastFailure()
    {
        try
        {
            var root = new DirectoryInfo(StorageMaintenance.UpdateJobsFolder);
            if (!root.Exists) return null;
            DirectoryInfo? newest = root.EnumerateDirectories()
                .Where(folder => File.Exists(Path.Combine(folder.FullName, "journal.json")))
                .OrderByDescending(folder => folder.LastWriteTimeUtc)
                .FirstOrDefault();
            if (newest is null) return null;
            using JsonDocument document = JsonDocument.Parse(
                File.ReadAllBytes(Path.Combine(newest.FullName, "journal.json")));
            JsonElement rootElement = document.RootElement;
            if (!rootElement.TryGetProperty("status", out JsonElement status) ||
                status.GetString() != "failed") return null;
            string message = rootElement.TryGetProperty("message", out JsonElement detail)
                ? detail.GetString() ?? string.Empty : string.Empty;
            return string.IsNullOrWhiteSpace(message) ? "pembaruan dibatalkan" : message;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    /// <summary>Waits until the spawning process has exited, with a hard deadline.</summary>
    /// <param name="oldPid">Process id to watch.</param>
    private static void WaitForExit(int oldPid)
    {
        if (oldPid <= 0) return;
        for (int attempt = 0; attempt < WaitSeconds * 4; attempt++)
        {
            try
            {
                using Process? process = Process.GetProcessById(oldPid);
                if (process.HasExited) return;
            }
            catch (ArgumentException) { return; }
            catch (InvalidOperationException) { return; }
            Thread.Sleep(250);
        }
    }

    /// <summary>Restores every file that existed before the failed update.</summary>
    /// <param name="job">Failed job.</param>
    /// <param name="backups">Target-to-backup map.</param>
    /// <param name="created">Files that did not exist before.</param>
    private static void Rollback(UpdateJob job, IReadOnlyDictionary<string, string> backups,
        IReadOnlyList<string> created)
    {
        foreach (string target in created)
        {
            try { if (File.Exists(target)) File.Delete(target); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        foreach ((string target, string backup) in backups)
        {
            try
            {
                EnsureParent(target);
                File.Copy(backup, target, overwrite: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        foreach (string relative in job.Deleted)
        {
            string backup = Path.Combine(job.BackupFolder, ToLocal(relative));
            string target = Path.Combine(job.TargetRoot, ToLocal(relative));
            if (File.Exists(backup) && !File.Exists(target))
            {
                try { File.Copy(backup, target, overwrite: true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    /// <summary>Keeps the NSIS install manifest in sync with files added or removed by the delta.</summary>
    /// <param name="job">Applied job.</param>
    private static void RefreshInstallManifest(UpdateJob job)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrayerAutomation", "install-manifest.txt");
        var entries = new List<string>();
        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                string value = line.Trim();
                if (UpdateManifest.IsSafeRelativePath(value)) entries.Add(value);
            }
        }
        entries.RemoveAll(relative => job.Deleted.Any(item =>
            string.Equals(item, relative, StringComparison.OrdinalIgnoreCase)));
        foreach (string relative in job.Files)
            if (!entries.Any(item => string.Equals(item, relative, StringComparison.OrdinalIgnoreCase)))
                entries.Add(relative);
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        File.WriteAllText(path, string.Join("\r\n", entries.OrderBy(value => value, StringComparer.Ordinal)) + "\r\n");
    }

    /// <summary>Starts the application again from its real root.</summary>
    /// <param name="root">Application root.</param>
    /// <param name="exe">Executable file name.</param>
    private static void Relaunch(string root, string exe)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Path.Combine(root, exe))
            {
                UseShellExecute = true,
                WorkingDirectory = root
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or System.ComponentModel.Win32Exception)
        {
            AppLogger.Write("update.relaunch", ex);
        }
    }

    private static string ToLocal(string relative) => relative.Replace('/', Path.DirectorySeparatorChar);

    private static void EnsureParent(string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
    }
}
