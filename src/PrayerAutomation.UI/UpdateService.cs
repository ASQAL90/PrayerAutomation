// Checks GitHub releases, verifies the signed manifest, and prepares delta/fallback updates.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

/// <summary>One downloadable release asset.</summary>
/// <param name="Name">Asset file name.</param>
/// <param name="Url">Browser download URL.</param>
/// <param name="Size">Asset size in bytes.</param>
/// <param name="Sha256">Digest advertised by GitHub, when present.</param>
internal sealed record ReleaseAsset(string Name, string Url, long Size, string? Sha256);

/// <summary>A newer release found on GitHub.</summary>
/// <param name="Tag">Release tag.</param>
/// <param name="Version">Numeric version.</param>
/// <param name="Manifest">Manifest asset, or null when the release predates signed manifests.</param>
/// <param name="Signature">Manifest signature asset.</param>
/// <param name="Assets">All assets of the release by name.</param>
/// <param name="IsInstalled">True for the NSIS-installed copy.</param>
/// <param name="ReleaseNotes">Release body used for the "what's new" panel.</param>
/// <param name="ReleaseUrl">Browser URL of the release page.</param>
internal sealed record UpdateRelease(string Tag, string Version, ReleaseAsset? Manifest,
    ReleaseAsset? Signature, IReadOnlyDictionary<string, ReleaseAsset> Assets, bool IsInstalled,
    string? ReleaseNotes, string? ReleaseUrl);

/// <summary>How the prepared update will be applied.</summary>
internal enum UpdateInstallMode
{
    /// <summary>Small delta zip, applied by the helper.</summary>
    Delta,
    /// <summary>Full portable zip, applied by the helper.</summary>
    FullZip,
    /// <summary>Full NSIS installer, launched silently.</summary>
    Installer
}

/// <summary>A fully downloaded and verified update ready to install.</summary>
/// <param name="Mode">Installation path.</param>
/// <param name="LocalPath">Downloaded file.</param>
/// <param name="Plan">File plan for helper-based updates.</param>
/// <param name="Summary">Short user-facing explanation.</param>
internal sealed record UpdatePreparation(UpdateInstallMode Mode, string LocalPath,
    UpdatePlan? Plan, string Summary);

/// <summary>Finds, verifies, and downloads Ayo Jama'ah! updates.</summary>
internal sealed class UpdateService
{
    private const string ReleasesUrl = "https://api.github.com/repos/ASQAL90/PrayerAutomation/releases/latest";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(10) };

    static UpdateService()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("AyoJamaah");
    }

    /// <summary>Queries GitHub for a newer release with the signed manifest assets.</summary>
    /// <param name="currentVersion">Installed version.</param>
    /// <param name="isInstalled">True for the NSIS-installed copy.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Release information, or null when up to date or unavailable.</returns>
    internal async Task<UpdateRelease?> CheckAsync(string currentVersion, bool isInstalled,
        CancellationToken cancellationToken)
    {
        using JsonDocument? document = await Http.GetFromJsonAsync<JsonDocument>(ReleasesUrl, cancellationToken)
            .ConfigureAwait(false);
        if (document is null) return null;
        JsonElement root = document.RootElement;
        string tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        if (!UpdateVersion.TryParse(tag, out Version? latest) ||
            !UpdateVersion.TryParse(currentVersion, out Version? current) || latest <= current) return null;

        string version = UpdateVersion.AssetText(latest);
        string? notes = null;
        if (root.TryGetProperty("body", out JsonElement bodyElement)) notes = bodyElement.GetString();
        if (notes is not null && notes.Length > 32000) notes = notes[..32000];
        string? releaseUrl = root.TryGetProperty("html_url", out JsonElement urlElement)
            ? urlElement.GetString()
            : null;
        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        var assets = new Dictionary<string, ReleaseAsset>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
        {
            string? name = asset.GetProperty("name").GetString();
            string? url = asset.GetProperty("browser_download_url").GetString();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url)) continue;
            string? digest = null;
            if (asset.TryGetProperty("digest", out JsonElement digestElement))
                digest = digestElement.GetString()?.Replace("sha256:", "", StringComparison.OrdinalIgnoreCase);
            long size = asset.TryGetProperty("size", out JsonElement sizeElement) ? sizeElement.GetInt64() : 0;
            assets[name] = new ReleaseAsset(name, url, size, digest);
        }

        assets.TryGetValue($"AyoJamaah-update-{version}-{arch}.json", out ReleaseAsset? manifest);
        assets.TryGetValue($"AyoJamaah-update-{version}-{arch}.sig", out ReleaseAsset? signature);
        if (manifest is null || signature is null)
        {
            // Old releases without signed manifests still support the NSIS fallback.
            ReleaseAsset? installer = FindInstaller(assets, version);
            if (installer is null) return null;
        }
        return new UpdateRelease(tag, version, manifest, signature, assets, isInstalled, notes, releaseUrl);
    }

    /// <summary>Downloads and verifies the manifest, then plans the smallest safe update.</summary>
    /// <param name="release">Release found by <see cref="CheckAsync"/>.</param>
    /// <param name="currentVersion">Installed version.</param>
    /// <param name="root">Application root.</param>
    /// <param name="folder">Download folder.</param>
    /// <param name="progress">Overall progress.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>Prepared update, or null when no usable payload was found.</returns>
    internal async Task<UpdatePreparation?> PrepareAsync(UpdateRelease release, string currentVersion,
        string root, string folder, IProgress<double> progress, CancellationToken cancellationToken)
    {
        if (release.Manifest is not null && release.Signature is not null)
        {
            byte[] manifestBytes = await DownloadBytesAsync(release.Manifest, folder, cancellationToken)
                .ConfigureAwait(false);
            byte[] signatureBytes = await DownloadBytesAsync(release.Signature, folder, cancellationToken)
                .ConfigureAwait(false);
            if (manifestBytes.Length > 0 && signatureBytes.Length > 0 &&
                DigestMatches(release.Manifest, manifestBytes) && DigestMatches(release.Signature, signatureBytes) &&
                UpdateManifest.TryVerify(manifestBytes, Encoding.UTF8.GetString(signatureBytes), out SignedUpdateManifest? manifest) &&
                manifest is not null)
            {
                UpdatePlan plan = await UpdatePlanner.CreateAsync(manifest, currentVersion, root,
                    release.IsInstalled, cancellationToken).ConfigureAwait(false);
                if (plan.UseDelta && release.Assets.TryGetValue(plan.Manifest.DeltaAsset, out ReleaseAsset? delta))
                {
                    string? deltaPath = await DownloadAssetAsync(delta, folder, progress, cancellationToken)
                        .ConfigureAwait(false);
                    if (deltaPath is not null && VerifyLocalDigest(deltaPath, plan.Manifest.DeltaSha256) &&
                        DeltaCoversPlan(deltaPath, plan))
                        return new UpdatePreparation(UpdateInstallMode.Delta, deltaPath, plan,
                            "Pembaruan kecil sudah diunduh dan diperiksa.");
                }
            }
        }

        if (release.IsInstalled)
        {
            ReleaseAsset? installer = FindInstaller(release.Assets, release.Version);
            if (installer is not null)
            {
                string? path = await DownloadAssetAsync(installer, folder, progress, cancellationToken)
                    .ConfigureAwait(false);
                if (path is not null && DigestMatches(installer, await File.ReadAllBytesAsync(path, cancellationToken)
                        .ConfigureAwait(false)))
                    return new UpdatePreparation(UpdateInstallMode.Installer, path, null,
                        "Pembaruan lengkap sudah diunduh dan diperiksa.");
            }
            return null;
        }

        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        if (!release.Assets.TryGetValue($"AyoJamaah-win-{arch}.zip", out ReleaseAsset? fullZip)) return null;
        string? zipPath = await DownloadAssetAsync(fullZip, folder, progress, cancellationToken).ConfigureAwait(false);
        if (zipPath is null || !VerifyLocalDigest(zipPath, fullZip.Sha256)) return null;
        UpdatePlan? fullPlan = await BuildFullZipPlanAsync(zipPath, release.Version, root, cancellationToken)
            .ConfigureAwait(false);
        if (fullPlan is null) return null;
        return new UpdatePreparation(UpdateInstallMode.FullZip, zipPath, fullPlan,
            "Pembaruan lengkap sudah diunduh dan diperiksa.");
    }

    /// <summary>Downloads one asset with resume support and digest verification.</summary>
    /// <param name="asset">Asset to download.</param>
    /// <param name="folder">Destination folder.</param>
    /// <param name="progress">Progress in the range 0–1.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>Verified file path, or null on failure.</returns>
    internal async Task<string?> DownloadAssetAsync(ReleaseAsset asset, string folder,
        IProgress<double>? progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, asset.Name);
        string partial = path + ".part";
        long offset = File.Exists(partial) ? new FileInfo(partial).Length : 0;
        if (asset.Size > 0 && offset >= asset.Size) offset = 0;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, asset.Url);
            if (offset > 0) request.Headers.Range = new RangeHeaderValue(offset, null);
            using HttpResponseMessage response = await Http.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            bool appending = offset > 0 && response.StatusCode == System.Net.HttpStatusCode.PartialContent;
            if (!appending) offset = 0;
            response.EnsureSuccessStatusCode();

            long total = response.Content.Headers.ContentLength is long length && appending
                ? offset + length
                : asset.Size > 0 ? asset.Size : response.Content.Headers.ContentLength ?? 0;
            if (File.Exists(partial) && !appending) File.Delete(partial);

            await using (var target = new FileStream(partial, appending ? FileMode.Append : FileMode.Create,
                             FileAccess.Write, FileShare.None))
            {
                await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);
                byte[] buffer = new byte[81920];
                long received = offset;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    received += read;
                    if (total > 0) progress?.Report(Math.Min(1d, (double)received / total));
                }
            }

            if (!VerifyLocalDigest(partial, asset.Sha256)) return null;
            File.Move(partial, path, overwrite: true);
            progress?.Report(1d);
            return path;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException
            or InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>True when the delta zip actually contains every file this device still needs.</summary>
    /// <param name="deltaPath">Downloaded delta zip.</param>
    /// <param name="plan">Planned file changes.</param>
    /// <returns>True when the delta is sufficient for this device.</returns>
    private static bool DeltaCoversPlan(string deltaPath, UpdatePlan plan)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(deltaPath);
            var names = new HashSet<string>(
                archive.Entries.Select(entry => entry.FullName), StringComparer.OrdinalIgnoreCase);
            return plan.Needed.All(file => names.Contains(file.Path));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Builds a file plan directly from a verified full zip, without needing a signed manifest.</summary>
    /// <param name="zipPath">Downloaded full portable zip.</param>
    /// <param name="version">Release version.</param>
    /// <param name="root">Application root.</param>
    /// <param name="cancellationToken">Cancels hashing.</param>
    /// <returns>A full-install plan, or null when the zip is not usable.</returns>
    private static async Task<UpdatePlan?> BuildFullZipPlanAsync(string zipPath, string version, string root,
        CancellationToken cancellationToken)
    {
        var files = new List<ManifestFile>();
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.FullName.EndsWith('/') || !UpdateManifest.IsSafeRelativePath(entry.FullName)) continue;
                await using Stream stream = entry.Open();
                byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
                files.Add(new ManifestFile
                {
                    Path = entry.FullName,
                    Size = entry.Length,
                    Sha256 = Convert.ToHexString(hash).ToLowerInvariant()
                });
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return null;
        }
        if (files.Count == 0) return null;

        // A synthetic manifest keeps the existing planner/hash logic; the delta
        // size is impossible so the plan is always a plain full replacement.
        var full = new SignedUpdateManifest
        {
            Version = version,
            Arch = Environment.Is64BitProcess ? "x64" : "x86",
            DeltaSize = long.MaxValue,
            FallbackSize = 1,
            Files = files,
            Deleted = []
        };
        return await UpdatePlanner.CreateAsync(full, version, root, false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Downloads a small asset into memory for signature verification.</summary>
    /// <param name="asset">Asset to download.</param>
    /// <param name="folder">Folder used only to keep the API shape consistent.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>Exact response bytes, or an empty array on failure.</returns>
    private static async Task<byte[]> DownloadBytesAsync(ReleaseAsset asset, string folder,
        CancellationToken cancellationToken)
    {
        _ = folder;
        try
        {
            return await Http.GetByteArrayAsync(asset.Url, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException)
        {
            return [];
        }
    }

    private static ReleaseAsset? FindInstaller(IReadOnlyDictionary<string, ReleaseAsset> assets, string version)
    {
        if (assets.TryGetValue($"AyoJamaah.Setup-{version}.exe", out ReleaseAsset? installer)) return installer;
        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        assets.TryGetValue($"AyoJamaah.Setup-{version}-{arch}.exe", out installer);
        return installer;
    }

    private static bool DigestMatches(ReleaseAsset asset, byte[] bytes) =>
        asset.Sha256 is null || string.Equals(Convert.ToHexString(SHA256.HashData(bytes)), asset.Sha256,
            StringComparison.OrdinalIgnoreCase);

    private static bool VerifyLocalDigest(string path, string? expected)
    {
        if (string.IsNullOrWhiteSpace(expected)) return true;
        try
        {
            using FileStream stream = File.OpenRead(path);
            return string.Equals(Convert.ToHexString(SHA256.HashData(stream)), expected,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
