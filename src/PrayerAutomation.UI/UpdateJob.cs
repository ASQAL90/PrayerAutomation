// Hand-off file between the running app and the small updater helper.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerAutomation.UI;

/// <summary>Describes one pending delta application.</summary>
internal sealed class UpdateJob
{
    [JsonPropertyName("schema")] public int Schema { get; set; } = 1;
    [JsonPropertyName("target_root")] public string TargetRoot { get; set; } = string.Empty;
    [JsonPropertyName("source_folder")] public string SourceFolder { get; set; } = string.Empty;
    [JsonPropertyName("backup_folder")] public string BackupFolder { get; set; } = string.Empty;
    [JsonPropertyName("journal_path")] public string JournalPath { get; set; } = string.Empty;
    [JsonPropertyName("files")] public List<string> Files { get; set; } = [];
    [JsonPropertyName("deleted")] public List<string> Deleted { get; set; } = [];
    [JsonPropertyName("target_version")] public string TargetVersion { get; set; } = string.Empty;
    [JsonPropertyName("app_exe")] public string AppExe { get; set; } = "AyoJamaah.exe";
    [JsonPropertyName("is_installed")] public bool IsInstalled { get; set; }

    /// <summary>Writes the job to disk with a fixed layout.</summary>
    /// <param name="path">Destination job file.</param>
    internal void Save(string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(this, JobOptions));
    }

    /// <summary>Reads and validates a job file.</summary>
    /// <param name="path">Job file path.</param>
    /// <param name="job">Parsed job when valid.</param>
    /// <returns>True when the job is usable.</returns>
    internal static bool TryLoad(string path, out UpdateJob? job)
    {
        job = null;
        try
        {
            UpdateJob? parsed = JsonSerializer.Deserialize<UpdateJob>(File.ReadAllBytes(path), JobOptions);
            if (parsed is null || parsed.Schema != 1) return false;
            if (string.IsNullOrWhiteSpace(parsed.TargetRoot) || !Directory.Exists(parsed.TargetRoot)) return false;
            if (string.IsNullOrWhiteSpace(parsed.SourceFolder) || !Directory.Exists(parsed.SourceFolder)) return false;
            if (string.IsNullOrWhiteSpace(parsed.BackupFolder) || string.IsNullOrWhiteSpace(parsed.JournalPath)) return false;
            if (parsed.AppExe is not ("AyoJamaah.exe" or "PrayerAutomation.exe" or "PrayerAutomation.UI.exe")) return false;
            if (parsed.Files.Count == 0) return false;
            if (parsed.Files.Any(path => !UpdateManifest.IsSafeRelativePath(path))) return false;
            if (parsed.Deleted.Any(path => !UpdateManifest.IsSafeRelativePath(path))) return false;
            job = parsed;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static readonly JsonSerializerOptions JobOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = false
    };
}

/// <summary>Runtime journal used to roll a failed delta application back.</summary>
internal sealed class UpdateJournal
{
    [JsonPropertyName("schema")] public int Schema { get; set; } = 1;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("target_version")] public string TargetVersion { get; set; } = string.Empty;
    [JsonPropertyName("updated_utc")] public string UpdatedUtc { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

    /// <summary>Writes the journal atomically enough for a single-helper update.</summary>
    /// <param name="path">Journal path.</param>
    internal void Save(string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        string temporary = path + ".tmp";
        File.WriteAllBytes(temporary, JsonSerializer.SerializeToUtf8Bytes(this));
        File.Move(temporary, path, overwrite: true);
    }
}
