// Optional, consent-gated download of the complete offline location dataset.
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerAutomation.UI;

/// <summary>One file in the optional offline data pack manifest.</summary>
internal sealed class OfflineDataFile
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha256")] public string Sha256 { get; set; } = string.Empty;
}

/// <summary>Manifest describing the complete optional offline data pack.</summary>
internal sealed class OfflineDataManifest
{
    [JsonPropertyName("schema")] public int Schema { get; set; }
    [JsonPropertyName("dataset_version")] public int DatasetVersion { get; set; }
    [JsonPropertyName("snapshot")] public string Snapshot { get; set; } = string.Empty;
    [JsonPropertyName("files")] public List<OfflineDataFile> Files { get; set; } = [];
}

/// <summary>Progress event for an offline data download.</summary>
/// <param name="FileName">File currently being processed.</param>
/// <param name="BytesDownloaded">Bytes downloaded so far.</param>
/// <param name="TotalBytes">Total bytes in the pack, or 0 while reading the manifest.</param>
/// <param name="Phase">Human-readable phase in Indonesian.</param>
internal sealed record OfflineDownloadProgress(
    string FileName,
    long BytesDownloaded,
    long TotalBytes,
    string Phase);

/// <summary>Result of installing the optional offline data pack.</summary>
/// <param name="Installed">True when a verified pack is available.</param>
/// <param name="Directory">Cache directory containing the verified files.</param>
/// <param name="Message">Short Indonesian status message.</param>
internal sealed record OfflineDataInstallResult(bool Installed, string Directory, string Message);

/// <summary>Downloads and verifies the optional complete offline dataset.</summary>
internal sealed class OfflineDataDownloadService
{
    private const int ExpectedSchema = 1;
    private const int ExpectedDatasetVersion = 2;
    private readonly HttpClient _httpClient;

    internal OfflineDataDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Public raw URL of the data-pack manifest in this repository.</summary>
    internal const string ManifestUrl =
        "https://raw.githubusercontent.com/ASQAL90/PrayerAutomation/master/data/offline-data-manifest.json";

    /// <summary>Permanent cache directory for the verified data pack.</summary>
    internal static string CacheDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AyoJamaah", "offline-data", "v2");

    /// <summary>Loads an already verified cache without network access.</summary>
    /// <returns>True when the cache exists and is usable.</returns>
    internal bool TryLoadCached()
    {
        if (!IsCacheComplete(CacheDirectory)) return false;
        try
        {
            OfflineReverseGeocoder.UseDownloaded(OfflineReverseGeocoder.LoadFromDirectory(CacheDirectory));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            OfflineReverseGeocoder.ClearDownloaded();
            return false;
        }
    }

    /// <summary>Downloads, verifies, and activates the optional data pack.</summary>
    /// <param name="progress">Progress reporter for the UI.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The install result.</returns>
    internal async Task<OfflineDataInstallResult> DownloadAsync(
        IProgress<OfflineDownloadProgress> progress,
        CancellationToken cancellationToken)
    {
        progress.Report(new OfflineDownloadProgress(string.Empty, 0, 0, "Membaca manifest data…"));
        string staging = CacheDirectory + ".stage";
        string partial = CacheDirectory + ".partial";
        try
        {
            byte[] manifestBytes = await _httpClient.GetByteArrayAsync(ManifestUrl, cancellationToken).ConfigureAwait(false);
            OfflineDataManifest? manifest = ParseManifest(manifestBytes);
            if (manifest is null)
                return new OfflineDataInstallResult(false, string.Empty, "Manifest data tidak dapat dibaca atau tidak valid.");

            long totalBytes = manifest.Files.Sum(file => Math.Max(0, file.Size));
            if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
            if (Directory.Exists(partial)) Directory.Delete(partial, recursive: true);
            Directory.CreateDirectory(staging);
            Directory.CreateDirectory(partial);

            long downloaded = 0;
            foreach (OfflineDataFile file in manifest.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? destination = SafeCombine(staging, file.Name);
                if (destination is null)
                    return new OfflineDataInstallResult(false, string.Empty, "Manifest memuat nama berkas yang tidak aman.");

                progress.Report(new OfflineDownloadProgress(file.Name, downloaded, totalBytes,
                    $"Mengunduh {file.Name}…"));
                string temporary = Path.Combine(partial, Path.GetFileName(file.Name));
                string hash = await DownloadFileAsync(file, temporary, progress, downloaded, totalBytes, cancellationToken)
                    .ConfigureAwait(false);
                if (!string.Equals(hash, file.Sha256, StringComparison.OrdinalIgnoreCase))
                    return new OfflineDataInstallResult(false, string.Empty,
                        $"Integritas {file.Name} gagal diverifikasi. Data lama tetap dipakai.");

                File.Move(temporary, destination, overwrite: true);
                downloaded += Math.Max(0, file.Size);
            }

            ActivateStaging(staging);
            CleanupDirectory(partial);
            OfflineReverseGeocoder.UseDownloaded(OfflineReverseGeocoder.LoadFromDirectory(CacheDirectory));
            return new OfflineDataInstallResult(true, CacheDirectory,
                $"Paket data lengkap ({manifest.Snapshot}) terpasang dan siap dipakai tanpa internet.");
        }
        catch (OperationCanceledException)
        {
            CleanupDirectory(staging);
            CleanupDirectory(partial);
            if (cancellationToken.IsCancellationRequested) throw;
            return new OfflineDataInstallResult(false, string.Empty,
                "Unduhan melebihi batas waktu. Aplikasi tetap memakai data bawaan.");
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException
            or InvalidDataException or NotSupportedException or JsonException)
        {
            CleanupDirectory(staging);
            CleanupDirectory(partial);
            return new OfflineDataInstallResult(false, string.Empty,
                $"Unduhan gagal: {ex.Message}. Aplikasi tetap memakai data bawaan.");
        }
        catch (Exception ex)
        {
            CleanupDirectory(staging);
            CleanupDirectory(partial);
            return new OfflineDataInstallResult(false, string.Empty,
                $"Unduhan gagal: {ex.Message}. Aplikasi tetap memakai data bawaan.");
        }
    }

    /// <summary>Parses and validates a downloaded manifest.</summary>
    private static OfflineDataManifest? ParseManifest(byte[] bytes)
    {
        try
        {
            OfflineDataManifest? manifest = JsonSerializer.Deserialize<OfflineDataManifest>(bytes);
            if (manifest is null || manifest.Schema != ExpectedSchema ||
                manifest.DatasetVersion != ExpectedDatasetVersion || manifest.Files.Count == 0)
                return null;
            if (manifest.Files.Any(file => SafeRelativeName(file.Name) is null ||
                    file.Size <= 0 || !UpdateManifest.IsSha256(file.Sha256)))
                return null;
            return manifest;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Streams one file to disk while computing its SHA-256.</summary>
    private async Task<string> DownloadFileAsync(
        OfflineDataFile file,
        string temporary,
        IProgress<OfflineDownloadProgress> progress,
        long completedBytes,
        long totalBytes,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            OfflineDataUrl(file.Name), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destination = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None,
            81920, useAsync: true);

        byte[] buffer = new byte[81920];
        long fileBytes = 0;
        using var sha = SHA256.Create();
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            sha.TransformBlock(buffer, 0, read, null, 0);
            fileBytes += read;
            progress.Report(new OfflineDownloadProgress(file.Name, completedBytes + fileBytes, totalBytes,
                $"Mengunduh {file.Name}…"));
        }
        sha.TransformFinalBlock([], 0, 0);
        if (sha.Hash is null || fileBytes != file.Size)
            throw new InvalidDataException($"Ukuran {file.Name} tidak sesuai manifest.");
        return Convert.ToHexString(sha.Hash).ToLowerInvariant();
    }

    /// <summary>Builds the release URL for one data file.</summary>
    private static string OfflineDataUrl(string name) =>
        $"https://github.com/ASQAL90/PrayerAutomation/releases/download/data-v2/{name}";

    /// <summary>Replaces the installed cache atomically when possible.</summary>
    private static void ActivateStaging(string staging)
    {
        string backup = CacheDirectory + ".old";
        CleanupDirectory(backup);
        if (Directory.Exists(CacheDirectory))
        {
            try
            {
                Directory.Move(CacheDirectory, backup);
            }
            catch (IOException)
            {
                // The previous cache may be locked by another process; overwrite in place.
                CleanupDirectory(CacheDirectory);
            }
        }
        Directory.Move(staging, CacheDirectory);
        CleanupDirectory(backup);
    }

    private static bool IsCacheComplete(string directory)
    {
        // Both the current dataset and the downloaded global layer must be present.
        return File.Exists(Path.Combine(directory, "global.tsv.gz")) &&
               File.Exists(Path.Combine(directory, "indonesia.tsv.gz")) &&
               File.Exists(Path.Combine(directory, "indonesia.boundary.bin"));
    }

    private static string? SafeRelativeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || Path.IsPathRooted(name) ||
            name.Contains('\\') || name.Contains(':') || name.Contains("..", StringComparison.Ordinal))
            return null;
        return Path.GetFileName(name) == name ? name : null;
    }

    private static string? SafeCombine(string root, string name)
    {
        string? safe = SafeRelativeName(name);
        return safe is null ? null : Path.Combine(root, safe);
    }

    private static void CleanupDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
