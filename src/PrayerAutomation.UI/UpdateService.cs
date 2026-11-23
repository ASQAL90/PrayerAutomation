// Checks GitHub releases, verifies the signed manifest, and prepares delta/fallback updates.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
internal sealed record ReleaseAsset(string Name, string Url, long Size, string? Sha256, string? ApiUrl = null);

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
    private const string InternalUrlVariable = "AYOJAMAAH_UPDATE_URL";
    private const string InternalTokenVariable = "AYOJAMAAH_UPDATE_TOKEN";
    private const string GitHubApiHost = "api.github.com";
    private static readonly HttpClient ChannelHttp = CreateChannelClient();
    private static readonly string? InternalUpdateUrl = ReadInternalSetting(InternalUrlVariable);
    private static readonly string? InternalToken = ReadInternalSetting(InternalTokenVariable);

    static UpdateService()
    {
        ChannelHttp.DefaultRequestHeaders.UserAgent.ParseAdd("AyoJamaah");
    }

    private static string UpdateUrl => InternalUpdateUrl ?? ReleasesUrl;

    /// <summary>Queries GitHub for a newer release with the signed manifest assets.</summary>
    /// <param name="currentVersion">Installed version.</param>
    /// <param name="isInstalled">True for the NSIS-installed copy.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Release information, or null when up to date or unavailable.</returns>
    internal async Task<UpdateRelease?> CheckAsync(string currentVersion, bool isInstalled,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendWithInternalAuthAsync(UpdateUrl, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        await using Stream jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using JsonDocument document = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
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
        bool unsafeAssetNameSeen = false;
        foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
        {
            string? name = asset.GetProperty("name").GetString();
            string? url = asset.GetProperty("browser_download_url").GetString();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url)) continue;
            if (!UpdateManifest.IsSafeAssetName(name)) { unsafeAssetNameSeen = true; continue; }
            string? apiUrl = asset.TryGetProperty("url", out JsonElement apiElement) ? apiElement.GetString() : null;
            string? digest = null;
            if (asset.TryGetProperty("digest", out JsonElement digestElement))
                digest = digestElement.GetString()?.Replace("sha256:", "", StringComparison.OrdinalIgnoreCase);
            long size = asset.TryGetProperty("size", out JsonElement sizeElement) ? sizeElement.GetInt64() : 0;
            assets[name] = new ReleaseAsset(name, url, size, digest, apiUrl);
        }

        assets.TryGetValue($"AyoJamaah-update-{version}-{arch}.json", out ReleaseAsset? manifest);
        assets.TryGetValue($"AyoJamaah-update-{version}-{arch}.sig", out ReleaseAsset? signature);
        // Fail closed: since 2.4.0 a release without a complete, safely named
        // signed-manifest pair is not offered as an update at all.
        if (UpdateManifest.RequiresSignedManifest(tag) &&
            (unsafeAssetNameSeen || manifest is null || signature is null)) return null;
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
        bool signedRequired = UpdateManifest.RequiresSignedManifest(release.Version);
        if (signedRequired && (release.Manifest is null || release.Signature is null)) return null;

        SignedUpdateManifest? manifest = null;
        if (release.Manifest is not null && release.Signature is not null)
        {
            byte[] manifestBytes = await DownloadBytesAsync(release.Manifest, folder, cancellationToken)
                .ConfigureAwait(false);
            byte[] signatureBytes = await DownloadBytesAsync(release.Signature, folder, cancellationToken)
                .ConfigureAwait(false);
            bool verified = manifestBytes.Length > 0 && signatureBytes.Length > 0 &&
                DigestMatches(release.Manifest, manifestBytes) && DigestMatches(release.Signature, signatureBytes) &&
                UpdateManifest.TryVerify(manifestBytes, Encoding.UTF8.GetString(signatureBytes), out manifest) &&
                manifest is not null;
            // Fail closed: a release that promises a signed manifest must never
            // silently continue with an unsigned installer or portable zip.
            if (!verified && signedRequired) return null;
        }

        if (manifest is not null)
        {
            UpdatePlan plan = await UpdatePlanner.CreateAsync(manifest, currentVersion, root,
                release.IsInstalled, cancellationToken).ConfigureAwait(false);
            if (plan.UseDelta &&
                release.Assets.TryGetValue(manifest.DeltaAsset, out ReleaseAsset? delta) &&
                UpdateManifest.IsSafeAssetName(delta.Name))
            {
                string? deltaPath = await DownloadAssetAsync(delta, folder, progress, cancellationToken)
                    .ConfigureAwait(false);
                if (deltaPath is not null && VerifyLocalDigest(deltaPath, manifest.DeltaSha256) &&
                    DeltaCoversPlan(deltaPath, plan))
                    return new UpdatePreparation(UpdateInstallMode.Delta, deltaPath, plan,
                        "Pembaruan kecil sudah diunduh dan diperiksa.");
            }

            // Fallback assets are taken from the signed manifest only. Their
            // digests are signed too, so a tampered release cannot substitute
            // an arbitrary installer or portable zip here.
            if (release.IsInstalled)
            {
                if (release.Assets.TryGetValue(manifest.InstallerAsset, out ReleaseAsset? installer) &&
                    UpdateManifest.IsSafeAssetName(installer.Name))
                {
                    string? path = await DownloadAssetAsync(installer, folder, progress, cancellationToken)
                        .ConfigureAwait(false);
                    if (path is not null && VerifyLocalDigest(path, manifest.InstallerSha256))
                        return new UpdatePreparation(UpdateInstallMode.Installer, path, null,
                            "Pembaruan lengkap sudah diunduh dan diperiksa.");
                }
            }
            else if (release.Assets.TryGetValue(manifest.FallbackAsset, out ReleaseAsset? fallback) &&
                     UpdateManifest.IsSafeAssetName(fallback.Name))
            {
                string? path = await DownloadAssetAsync(fallback, folder, progress, cancellationToken)
                    .ConfigureAwait(false);
                if (path is not null && VerifyLocalDigest(path, manifest.FallbackSha256))
                {
                    UpdatePlan? signedFullPlan = await BuildFullZipPlanAsync(path, release.Version, root,
                        cancellationToken).ConfigureAwait(false);
                    if (signedFullPlan is not null)
                        return new UpdatePreparation(UpdateInstallMode.FullZip, path, signedFullPlan,
                            "Pembaruan lengkap sudah diunduh dan diperiksa.");
                }
            }
            // A verified manifest with no usable signed payload is still a
            // failed update; do not fall through to unsigned legacy assets.
            return null;
        }

        // Legacy releases (before signed manifests) only.
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
        // asset.Name becomes a local file name below, so reject traversal here
        // even if it came from a signed manifest or a trusted-looking API.
        if (!UpdateManifest.IsSafeAssetName(asset.Name)) return null;
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, asset.Name);
        string partial = path + ".part";
        long offset = File.Exists(partial) ? new FileInfo(partial).Length : 0;
        if (asset.Size > 0 && offset >= asset.Size) offset = 0;

        try
        {
            using HttpResponseMessage response = await SendAssetAsync(AssetDownloadUrl(asset), cancellationToken,
                offset > 0 ? offset : null).ConfigureAwait(false);
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
    private async Task<byte[]> DownloadBytesAsync(ReleaseAsset asset, string folder,
        CancellationToken cancellationToken)
    {
        _ = folder;
        try
        {
            using HttpResponseMessage response = await SendAssetAsync(AssetDownloadUrl(asset), cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException)
        {
            return [];
        }
    }

    private static string AssetDownloadUrl(ReleaseAsset asset) =>
        InternalToken is not null && asset.ApiUrl is not null ? asset.ApiUrl : asset.Url;

    private static bool IsGitHubApi(Uri uri) =>
        string.Equals(uri.Host, GitHubApiHost, StringComparison.OrdinalIgnoreCase);

    private static Task<HttpResponseMessage> SendAssetAsync(string url, CancellationToken cancellationToken,
        long? offsetFrom = null) =>
        SendWithInternalAuthAsync(url, cancellationToken, offsetFrom, octetStream: true);

    private static async Task<HttpResponseMessage> SendWithInternalAuthAsync(string url,
        CancellationToken cancellationToken, long? offsetFrom = null, bool octetStream = false)
    {
        Uri uri = new(url, UriKind.Absolute);
        for (int hop = 0; hop < 5; hop++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (IsGitHubApi(uri) && InternalToken is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", InternalToken);
                if (octetStream) request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            }
            if (offsetFrom is not null) request.Headers.Range = new RangeHeaderValue(offsetFrom, null);
            HttpResponseMessage response = await ChannelHttp.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is not null)
            {
                Uri next = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(uri, response.Headers.Location);
                response.Dispose();
                uri = next;
                continue;
            }
            return response;
        }
        throw new HttpRequestException("Terlalu banyak redirect saat mengambil data.");
    }

    private static HttpClient CreateChannelClient()
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        return new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) };
    }

    private static string? ReadInternalSetting(string name)
    {
#if AYOJAMAAH_INTERNAL_UPDATE
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
#else
        _ = name;
        return null;
#endif
    }

    private static ReleaseAsset? FindInstaller(IReadOnlyDictionary<string, ReleaseAsset> assets, string version)
    {
        // Prefer the smaller architecture-specific package; the universal
        // installer ships both payloads and is only a fallback for older tags.
        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        if (assets.TryGetValue($"AyoJamaah.Setup-{version}-{arch}.exe", out ReleaseAsset? installer)) return installer;
        assets.TryGetValue($"AyoJamaah.Setup-{version}.exe", out installer);
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
