// Signed update manifest model and verification for delta updates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrayerAutomation.UI;

/// <summary>One published file in an update manifest.</summary>
internal sealed class ManifestFile
{
    [JsonPropertyName("p")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("s")] public long Size { get; set; }
    [JsonPropertyName("h")] public string Sha256 { get; set; } = string.Empty;
}

/// <summary>Signed description of a release payload and its delta/fallback assets.</summary>
internal sealed class SignedUpdateManifest
{
    [JsonPropertyName("schema")] public int Schema { get; set; }
    [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
    [JsonPropertyName("tag")] public string Tag { get; set; } = string.Empty;
    [JsonPropertyName("baseline")] public string Baseline { get; set; } = string.Empty;
    [JsonPropertyName("arch")] public string Arch { get; set; } = string.Empty;
    [JsonPropertyName("delta_asset")] public string DeltaAsset { get; set; } = string.Empty;
    [JsonPropertyName("delta_sha256")] public string DeltaSha256 { get; set; } = string.Empty;
    [JsonPropertyName("delta_size")] public long DeltaSize { get; set; }
    [JsonPropertyName("fallback_asset")] public string FallbackAsset { get; set; } = string.Empty;
    [JsonPropertyName("fallback_sha256")] public string FallbackSha256 { get; set; } = string.Empty;
    [JsonPropertyName("fallback_size")] public long FallbackSize { get; set; }
    [JsonPropertyName("installer_asset")] public string InstallerAsset { get; set; } = string.Empty;
    [JsonPropertyName("installer_sha256")] public string InstallerSha256 { get; set; } = string.Empty;
    [JsonPropertyName("installer_size")] public long InstallerSize { get; set; }
    [JsonPropertyName("files")] public List<ManifestFile> Files { get; set; } = [];
    [JsonPropertyName("deleted")] public List<string> Deleted { get; set; } = [];
}

/// <summary>Verifies RSA-SHA256 signatures over the exact bytes of a downloaded manifest.</summary>
internal static class UpdateManifest
{
    // SubjectPublicKeyInfo (DER), base64. The private key never ships with the app.
    internal const string PublicKeyBase64 =
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5wEd5c/WaPx3xXeREQ/su7Ylb1nAGeBWjdvGnLwq+FNm9Z4xui1SSac8JaOlpRr7tg0rUbTxBnpKCBDjH09JTWfAOT5aPYL7ALJYr2fJsSvhP8STbl49BbU9TLoHIj2/2SJNIzAwGOS1RRnLuydSZgWKqIvjA9Fnt6jQdXI0XB1ilvls0Nr/k5k76FxtBS5Dce1zcxG4SFDmWedJeTwtdhL8c6/ZfUxE/XdjQsPV7X8Obt/ZdGZ+uMGuycBCcuLWzYcrgzKrsBBptl9hoSkfBQPPR6NgIK5uUvYR4RPz3L677nRIbi0k7TwXu2u2ysvi8hI24Qww5SviWNyRjv4zUQIDAQAB";

    /// <summary>Verifies the signature and parses the manifest.</summary>
    /// <param name="manifestBytes">Exact bytes of the downloaded manifest file.</param>
    /// <param name="signatureBase64">Base64-encoded RSA-SHA256 signature.</param>
    /// <param name="manifest">Parsed manifest when verification succeeds.</param>
    /// <returns>True when the signature is valid and the manifest is well formed.</returns>
    internal static bool TryVerify(byte[] manifestBytes, string signatureBase64,
        out SignedUpdateManifest? manifest, string? publicKeyBase64 = null)
    {
        manifest = null;
        if (manifestBytes.Length == 0 || string.IsNullOrWhiteSpace(signatureBase64)) return false;
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64 ?? PublicKeyBase64), out _);
            byte[] signature = Convert.FromBase64String(signatureBase64.Trim());
            if (!rsa.VerifyData(manifestBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return false;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
            SignedUpdateManifest? parsed = JsonSerializer.Deserialize<SignedUpdateManifest>(manifestBytes, options);
            if (parsed is null || parsed.Schema != 1) return false;
            if (!UpdateVersion.TryParse(parsed.Version, out _) ||
                !UpdateVersion.TryParse(parsed.Baseline, out _)) return false;
            if (parsed.Files.Count == 0) return false;
            if (parsed.Files.Any(file => !IsSafeRelativePath(file.Path) ||
                    file.Size < 0 || !IsSha256(file.Sha256))) return false;
            if (parsed.Deleted.Any(path => !IsSafeRelativePath(path))) return false;
            if (!IsSha256(parsed.DeltaSha256) || !IsSha256(parsed.FallbackSha256)) return false;
            if (parsed.DeltaSize <= 0 || parsed.FallbackSize <= 0) return false;
            manifest = parsed;
            return true;
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or JsonException
            or ArgumentException or IOException)
        {
            return false;
        }
    }

    /// <summary>True for a relative POSIX path that cannot escape the application root.</summary>
    /// <param name="path">Path to inspect.</param>
    /// <returns>True when the path is safe for update writes.</returns>
    internal static bool IsSafeRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.StartsWith('/') ||
            path.Contains('\\') || path.Contains(':')) return false;
        string[] parts = path.Split('/');
        if (parts.Any(part => part.Length == 0 || part is "." or "..")) return false;
        return !path.Any(character => character is ';' or '"' or '{' or '}' or '<' or '>' or '|' or '?' or '*');
    }

    /// <summary>True for a lowercase 64-character hexadecimal digest.</summary>
    /// <param name="value">Digest to inspect.</param>
    /// <returns>True when the digest has the expected shape.</returns>
    internal static bool IsSha256(string? value) =>
        value is not null && value.Length == 64 && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}
