// Smoke test for version comparison, signed manifests, safe paths and delta planning.
using System.Security.Cryptography;
using System.Text.Json;
using PrayerAutomation.UI;
using UpdateLogicSmoke;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("GAGAL: " + message);
}

Check(UpdateVersion.TryParse("v2.3.1", out Version parsed), "parse tag rilis");
Check(UpdateVersion.AssetText(parsed) == "2.3.1", "format nama aset");
Check(UpdateVersion.TryParse("2.3.10", out Version tenth) && tenth > parsed, "urutan versi numerik");
Check(!UpdateVersion.TryParse("2.3.x", out _), "versi bukan angka ditolak");
Check(UpdateManifest.IsSafeRelativePath("geonames/cities1000.tsv.gz"), "path relatif diterima");
Check(!UpdateManifest.IsSafeRelativePath("../data.txt"), "traversal ditolak");
Check(!UpdateManifest.IsSafeRelativePath("C:\\data.txt"), "path absolut ditolak");
Check(!UpdateManifest.IsSafeRelativePath("a\\b.txt"), "backslash ditolak");

string root = Path.Combine(Path.GetTempPath(), "ayojamaah-update-smoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    const string relative = "AyoJamaah.dll";
    string file = Path.Combine(root, relative);
    File.WriteAllText(file, "payload uji");
    string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();

    using RSA rsa = RSA.Create(2048);
    string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
    var manifestValue = new Dictionary<string, object>
    {
        ["schema"] = 1,
        ["version"] = "2.3.1",
        ["tag"] = "v2.3.1",
        ["baseline"] = "2.3.0",
        ["arch"] = Environment.Is64BitProcess ? "x64" : "x86",
        ["delta_asset"] = "delta.zip",
        ["delta_sha256"] = new string('a', 64),
        ["delta_size"] = 100,
        ["fallback_asset"] = "full.zip",
        ["fallback_sha256"] = new string('b', 64),
        ["fallback_size"] = 1000,
        ["installer_asset"] = "setup.exe",
        ["installer_sha256"] = new string('c', 64),
        ["installer_size"] = 1000,
        ["files"] = new[] { new Dictionary<string, object> { ["p"] = relative, ["s"] = new FileInfo(file).Length, ["h"] = hash } },
        ["deleted"] = Array.Empty<string>()
    };
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(manifestValue);
    byte[] signature = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    Check(UpdateManifest.TryVerify(bytes, Convert.ToBase64String(signature), out SignedUpdateManifest? manifest,
        publicKey) && manifest is not null, "tanda tangan manifest sah");
    byte[] tampered = bytes.ToArray();
    tampered[^1] ^= 1;
    Check(!UpdateManifest.TryVerify(tampered, Convert.ToBase64String(signature), out _, publicKey),
        "manifest yang diubah ditolak");

    using CancellationTokenSource cts = new();
    UpdatePlan delta = await UpdatePlanner.CreateAsync(manifest!, "2.3.0", root, false, cts.Token);
    Check(delta.UseDelta, "versi 2.3.0 memakai delta");
    Check(delta.Needed.Count == 0, "berkas lokal yang cocok tidak diganti");
    UpdatePlan old = await UpdatePlanner.CreateAsync(manifest!, "2.2.1", root, false, cts.Token);
    Check(!old.UseDelta, "versi di bawah baseline memakai fallback");
    manifest!.DeltaSize = 900;
    UpdatePlan large = await UpdatePlanner.CreateAsync(manifest, "2.3.0", root, false, cts.Token);
    Check(!large.UseDelta, "delta besar memakai fallback");
    manifest.Files[0].Sha256 = new string('d', 64);
    UpdatePlan changed = await UpdatePlanner.CreateAsync(manifest, "2.3.0", root, false, cts.Token);
    Check(changed.Needed.Count == 1, "berkas berbeda masuk daftar ganti");
}
finally
{
    try { Directory.Delete(root, recursive: true); } catch (IOException) { }
}

await UpdateRecoveryPolicySmoke.VerifyAllAsync();
UpdateReminderPolicySmoke.Verify();

Console.WriteLine("UpdateLogicSmoke: PASS");
