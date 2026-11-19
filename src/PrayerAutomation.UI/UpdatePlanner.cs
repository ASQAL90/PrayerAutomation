// Decides whether a release can use the small delta zip or must fall back to a full download.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

/// <summary>The concrete work a delta update has to perform.</summary>
/// <param name="Manifest">Verified release manifest.</param>
/// <param name="UseDelta">True when the small delta zip can be used.</param>
/// <param name="Needed">Files missing or different from the manifest.</param>
/// <param name="Deleted">Manifest files that no longer exist in this release.</param>
/// <param name="IsInstalled">True for the NSIS-installed copy.</param>
internal sealed record UpdatePlan(SignedUpdateManifest Manifest, bool UseDelta,
    IReadOnlyList<ManifestFile> Needed, IReadOnlyList<string> Deleted, bool IsInstalled);

/// <summary>Builds an update plan from local hashes and version numbers.</summary>
internal static class UpdatePlanner
{
    /// <summary>Above this ratio the delta saves too little bandwidth, so use the full payload.</summary>
    internal const double DeltaSizeRatio = 0.6;

    /// <summary>Compares local files with the manifest and chooses the download path.</summary>
    /// <param name="manifest">Verified manifest.</param>
    /// <param name="currentVersion">Installed application version.</param>
    /// <param name="root">Application root folder.</param>
    /// <param name="isInstalled">True for the NSIS-installed copy.</param>
    /// <param name="cancellationToken">Cancels local hashing.</param>
    /// <returns>The update plan.</returns>
    internal static async Task<UpdatePlan> CreateAsync(SignedUpdateManifest manifest,
        string currentVersion, string root, bool isInstalled, CancellationToken cancellationToken)
    {
        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        bool currentOk = UpdateVersion.TryParse(currentVersion, out Version current);
        bool targetOk = UpdateVersion.TryParse(manifest.Version, out Version target);
        bool baselineOk = UpdateVersion.TryParse(manifest.Baseline, out Version baseline);
        bool versionOk = currentOk && targetOk && baselineOk;
        bool canUseDelta = versionOk && arch.Equals(manifest.Arch, StringComparison.OrdinalIgnoreCase) &&
            current >= baseline && target > current &&
            manifest.DeltaSize <= manifest.FallbackSize * DeltaSizeRatio;

        var needed = new List<ManifestFile>();
        foreach (ManifestFile file in manifest.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                needed.Add(file);
                continue;
            }
            var info = new FileInfo(path);
            if ((info.Attributes & FileAttributes.ReparsePoint) != 0 || info.Length != file.Size ||
                !string.Equals(await ComputeSha256Async(path, cancellationToken).ConfigureAwait(false),
                    file.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                needed.Add(file);
            }
        }

        var deleted = new List<string>();
        foreach (string relative in manifest.Deleted)
        {
            string path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path)) deleted.Add(relative);
        }
        return new UpdatePlan(manifest, canUseDelta, needed, deleted, isInstalled);
    }

    /// <summary>Computes a lowercase SHA-256 digest of a file.</summary>
    /// <param name="path">File to hash.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>Hex digest.</returns>
    internal static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
