// Checks GitHub releases for a newer version and downloads the matching installer.
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

/// <summary>Release information for a newer version of Ayo Jama'ah!</summary>
internal sealed record UpdateInfo(string Tag, string Version, string AssetName, long SizeBytes,
    string? Sha256, string DownloadUrl);

/// <summary>Checks GitHub for newer releases and downloads the chosen asset with optional SHA-256 verification.</summary>
internal sealed class UpdateService
{
    private const string ReleasesUrl = "https://api.github.com/repos/ASQAL90/PrayerAutomation/releases/latest";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

    static UpdateService()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("AyoJamaah");
    }

    /// <summary>Queries GitHub and returns the newest release when it is newer than the current version.</summary>
    /// <param name="currentVersion">Current application version, e.g. "2.2.1".</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Update information, or null when already up to date or unavailable.</returns>
    internal async Task<UpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken)
    {
        using JsonDocument? document = await Http.GetFromJsonAsync<JsonDocument>(ReleasesUrl, cancellationToken)
            .ConfigureAwait(false);
        if (document is null) return null;
        JsonElement root = document.RootElement;
        string tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        string normalized = tag.TrimStart('v', 'V');
        if (!Version.TryParse(normalized, out Version? latest)) return null;

        string[] currentParts = currentVersion.Split('.');
        string currentPadded = currentVersion;
        for (int index = currentParts.Length; index < 4; index++) currentPadded += ".0";
        if (!Version.TryParse(currentPadded, out Version? current)) return null;
        if (latest <= current) return null;

        string latestText = latest.ToString(3);
        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        string[] candidates = [
            $"AyoJamaah.Setup-{latestText}-{arch}.exe",
            $"AyoJamaah.Setup-{latestText}.exe",
        ];
        foreach (string name in candidates)
        {
            foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
            {
                if (!string.Equals(asset.GetProperty("name").GetString(), name, StringComparison.OrdinalIgnoreCase))
                    continue;
                string? digest = null;
                if (asset.TryGetProperty("digest", out JsonElement digestElement))
                    digest = digestElement.GetString()?.Replace("sha256:", "", StringComparison.OrdinalIgnoreCase);
                string? url = asset.GetProperty("browser_download_url").GetString();
                if (string.IsNullOrWhiteSpace(url)) return null;
                long size = asset.TryGetProperty("size", out JsonElement sizeElement)
                    ? sizeElement.GetInt64() : 0;
                return new UpdateInfo(tag, latestText, name, size, digest, url);
            }
        }
        return null;
    }

    /// <summary>Downloads the asset and verifies its SHA-256 when a digest is available.</summary>
    /// <param name="info">The release to download.</param>
    /// <param name="destinationFolder">Folder to download into.</param>
    /// <param name="progress">Optional progress in the range 0–1.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>The downloaded file path, or null when verification failed.</returns>
    internal async Task<string?> DownloadAsync(UpdateInfo info, string destinationFolder,
        IProgress<double>? progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destinationFolder);
        string path = Path.Combine(destinationFolder, info.AssetName);
        using HttpResponseMessage response = await Http.GetAsync(info.DownloadUrl,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        long total = info.SizeBytes > 0 ? info.SizeBytes : response.Content.Headers.ContentLength ?? 0;
        await using (FileStream target = new(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            byte[] buffer = new byte[81920];
            long received = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                received += read;
                if (total > 0) progress?.Report(Math.Min(1d, (double)received / total));
            }
        }
        progress?.Report(1d);
        if (info.Sha256 is not null)
        {
            await using FileStream stream = File.OpenRead(path);
            byte[] actual = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(Convert.ToHexString(actual), info.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(path); } catch (IOException) { }
                return null;
            }
        }
        return path;
    }
}
