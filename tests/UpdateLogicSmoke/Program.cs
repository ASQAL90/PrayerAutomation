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
Check(UpdateManifest.IsSafeAssetName("AyoJamaah-delta-2.5.1-x64.zip"), "nama aset wajar diterima");
Check(!UpdateManifest.IsSafeAssetName("../evil.zip") &&
      !UpdateManifest.IsSafeAssetName("a\\b.exe") &&
      !UpdateManifest.IsSafeAssetName("C:\\evil.exe") &&
      !UpdateManifest.IsSafeAssetName("..") &&
      !UpdateManifest.IsSafeAssetName("evil:stream.exe") &&
      !UpdateManifest.IsSafeAssetName("evil\u0000.exe"),
    "nama aset traversal, absolut, ADS, dan kontrol ditolak");
Check(!UpdateManifest.RequiresSignedManifest("2.3.1") &&
      UpdateManifest.RequiresSignedManifest("2.4.0") &&
      UpdateManifest.RequiresSignedManifest("v2.5.1"),
    "manifest bertanda tangan wajib mulai rilis 2.4.0");

string root = Path.Combine(Path.GetTempPath(), "ayojamaah-update-smoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    // Hardening cs/zipslip: only contained, normalized entry names reach the plan.
    string zipRoot = Path.Combine(root, "zip");
    Directory.CreateDirectory(zipRoot);
    Check(ZipEntryPath.TryGetSafeRelative("geonames/cities1000.tsv.gz", zipRoot, out string safeZipPath) &&
          safeZipPath == "geonames/cities1000.tsv.gz", "entri ZIP relatif diterima apa adanya");
    Check(!ZipEntryPath.TryGetSafeRelative("../escaped.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("a/../../escaped.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("a\\b.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("/abs.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("a//b.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("a/./b.txt", zipRoot, out _) &&
          !ZipEntryPath.TryGetSafeRelative("C:/abs.txt", zipRoot, out _),
        "entri ZIP traversal, absolut, backslash, dan path kosong ditolak");

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

    // PA-01: even a correctly signed manifest must not name a traversal asset.
    byte[] unsafeBytes = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>(manifestValue)
    {
        ["delta_asset"] = "../evil.zip"
    });
    byte[] unsafeSignature = rsa.SignData(unsafeBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    Check(!UpdateManifest.TryVerify(unsafeBytes, Convert.ToBase64String(unsafeSignature), out _, publicKey),
        "manifest bertanda tangan dengan nama aset traversal ditolak");
    byte[] unsafeInstallerBytes = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>(manifestValue)
    {
        ["installer_asset"] = "C:\\evil.exe"
    });
    byte[] unsafeInstallerSignature = rsa.SignData(unsafeInstallerBytes, HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);
    Check(!UpdateManifest.TryVerify(unsafeInstallerBytes, Convert.ToBase64String(unsafeInstallerSignature),
            out _, publicKey),
        "manifest bertanda tangan dengan nama installer absolut ditolak");
    Check(UpdateManifest.TryVerify(bytes, Convert.ToBase64String(signature), out _, publicKey),
        "manifest dengan nama aset aman tetap diterima");

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

// Recovery progress runs after ConfigureAwait(false), so a DispatcherObject must
// never be asked for its Dispatcher from that worker thread: the getter itself
// calls VerifyAccess and throws "different thread owns it".
string updateWindowSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "UpdateWindow.xaml.cs"));
string appSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "App.xaml.cs"));
string mainWindowSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml.cs"));
string traySource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TrayService.cs"));
Check(updateWindowSource.Contains("_uiDispatcher = Dispatcher", StringComparison.Ordinal) &&
      updateWindowSource.Contains("if (!_uiDispatcher.CheckAccess())", StringComparison.Ordinal) &&
      !updateWindowSource.Contains("!Dispatcher.CheckAccess()", StringComparison.Ordinal) &&
      !updateWindowSource.Contains("if (!_waitingForRecovery && IsVisible)", StringComparison.Ordinal),
    "UpdateWindow harus memakai Dispatcher UI dan mengaktifkan kembali tombol setelah gagal.");
Check(mainWindowSource.Contains("_uiDispatcher = Dispatcher", StringComparison.Ordinal) &&
      mainWindowSource.Contains("if (!_uiDispatcher.CheckAccess())", StringComparison.Ordinal) &&
      !mainWindowSource.Contains("!Dispatcher.CheckAccess()", StringComparison.Ordinal),
    "Status setter MainWindow harus memakai Dispatcher yang ditangkap di thread UI.");
Check(traySource.Contains("_uiDispatcher = Application.Current.Dispatcher", StringComparison.Ordinal) &&
      !traySource.Contains("Application.Current.Dispatcher.CheckAccess()", StringComparison.Ordinal),
    "TrayService harus menangkap Dispatcher sekali di konstruktor UI, bukan dari thread latar.");
Check(appSource.Contains("Dispatcher ui = Dispatcher;", StringComparison.Ordinal) &&
      appSource.Contains("ui.BeginInvoke(() => ShowUpdateWindow(release))", StringComparison.Ordinal) &&
      !appSource.Contains("_ = Dispatcher.BeginInvoke(() => ShowUpdateWindow(release))", StringComparison.Ordinal),
    "Auto-check harus memakai Dispatcher yang ditangkap sebelum ConfigureAwait(false).");
int deltaStart = appSource.IndexOf("internal async Task<UpdateLaunchResult> InstallDeltaAsync", StringComparison.Ordinal);
int deltaEnd = appSource.IndexOf("/// <summary>Resumes normal lock-screen scheduling", StringComparison.Ordinal);
Check(deltaStart >= 0 && deltaEnd > deltaStart &&
      !System.Text.RegularExpressions.Regex.IsMatch(
          appSource[deltaStart..deltaEnd],
          @"ComputeSha256Async\(source[^\n]*\n\s*\.ConfigureAwait\(false\)"),
    "InstallDeltaAsync harus tetap di thread UI sampai ReleaseAndShutdown memakai jendela dan tray.");
int abortStart = appSource.IndexOf("private async Task AbortUpdatePreparationAsync", StringComparison.Ordinal);
Check(abortStart >= 0 &&
      !appSource[abortStart..].Contains("ResumeSchedulingAsync(DateTimeOffset.UtcNow).ConfigureAwait(false)", StringComparison.Ordinal),
    "Pemulihan jadwal setelah update gagal harus di thread UI agar DispatcherTimer tidak terikat thread keliru.");
int showStart = appSource.IndexOf("internal void ShowMainWindow()", StringComparison.Ordinal);
int showEnd = appSource.IndexOf("/// <summary>Shows the main window and selects the calendar tab", StringComparison.Ordinal);
Check(showStart >= 0 && showEnd > showStart &&
      appSource[showStart..showEnd].Contains("_mainWindow ??= CreateMainWindow()", StringComparison.Ordinal) &&
      appSource.Contains("private MainWindow CreateMainWindow()", StringComparison.Ordinal),
    "ShowMainWindow harus membuat ulang jendela utama yang sudah ditutup, bukan Show() pada window closed.");
int releaseStart = appSource.IndexOf("private void ReleaseAndShutdown()", StringComparison.Ordinal);
int releaseEnd = appSource.IndexOf("/// <summary>Releases taskbar panel", StringComparison.Ordinal);
string releaseSource = releaseStart >= 0 && releaseEnd > releaseStart
    ? appSource[releaseStart..releaseEnd]
    : string.Empty;
Check(System.Text.RegularExpressions.Regex.IsMatch(releaseSource, @"finally\s*\{\s*Shutdown\(\);"),
    "ReleaseAndShutdown harus tetap memanggil Shutdown walau satu langkah cleanup gagal.");
Check(traySource.Contains("catch (Exception ex) { AppLogger.Write(\"tray.action\", ex); }", StringComparison.Ordinal),
    "Aksi tray tidak boleh melempar dialog unhandled ke pengguna.");

// Ringkasan rilis: parser murni, panel popup vertikal, changelog About, rilis, dan installer.
var parsedNotes = ReleaseNotesParser.Parse(
    "## Fitur baru\n- Panel **ringkasan** [perubahan](https://example.invalid)\n" +
    "## Perbaikan\n- Perbaikan lintas-thread\n" +
    "## Lainnya\n- Bagian yang tidak dikenali\n" +
    "**Full Changelog**: https://example.invalid");
Check(parsedNotes.Count == 2 && parsedNotes[0].Heading == "Fitur baru" &&
      parsedNotes[0].Items.Count == 1 && parsedNotes[0].Items[0] == "Panel ringkasan perubahan" &&
      parsedNotes[1].Heading == "Perbaikan",
    "Parser harus membersihkan Markdown, membuang noise Full Changelog, dan hanya membaca bagian dikenali.");
Check(ReleaseNotesParser.Parse(null).Count == 0 && ReleaseNotesParser.Parse("- tanpa heading").Count == 0,
    "Catatan kosong atau tanpa heading tidak boleh menampilkan panel.");

string updateXaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "UpdateWindow.xaml"));
string notesWindowXaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReleaseNotesWindow.xaml"));
string notesWindowSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReleaseNotesWindow.xaml.cs"));
string aboutXaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AboutWindow.xaml"));
string releaseWorkflow = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "release.yml"));
string nsisSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AyoJamaah.nsi"));
string uiProject = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "PrayerAutomation.UI.csproj"));
string installerScript = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "build_installer_nsis.py"));
string changelog = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "CHANGELOG.md"));
string newestNotes = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "release-notes-2.5.1.md"));
string currentNotes = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "release-notes-2.5.2.md"));

Check(updateXaml.Contains("Name=\"ReleaseNotesPanel\"", StringComparison.Ordinal) &&
      updateXaml.Contains("Name=\"ReleaseNotesScroll\"", StringComparison.Ordinal) &&
      updateXaml.Contains("VerticalScrollBarVisibility=\"Auto\"", StringComparison.Ordinal) &&
      updateXaml.Contains("HorizontalScrollBarVisibility=\"Disabled\"", StringComparison.Ordinal) &&
      !updateXaml.Contains("TargetType=\"{x:Type ScrollBar}\"", StringComparison.Ordinal) &&
      !updateXaml.Contains("TargetType=\"ScrollBar\"", StringComparison.Ordinal) &&
      updateWindowSource.Contains("ReleaseNotesParser.Parse", StringComparison.Ordinal),
    "Panel catatan rilis memakai ScrollViewer vertikal dan style global, tanpa style scrollbar baru.");
Check(notesWindowXaml.Contains("VerticalScrollBarVisibility=\"Auto\"", StringComparison.Ordinal) &&
      notesWindowXaml.Contains("HorizontalScrollBarVisibility=\"Disabled\"", StringComparison.Ordinal) &&
      notesWindowSource.Contains("GetManifestResourceStream", StringComparison.Ordinal) &&
      aboutXaml.Contains("AboutReleaseNotesButton", StringComparison.Ordinal),
    "Riwayat perubahan dibuka dari About, bersumber resource lokal, dan bergulir vertikal saja.");
Check(releaseWorkflow.Contains("Validasi catatan rilis", StringComparison.Ordinal) &&
      releaseWorkflow.Contains("body_path: ${{ env.NOTES_PATH }}", StringComparison.Ordinal) &&
      !releaseWorkflow.Contains("generate_release_notes", StringComparison.Ordinal),
    "Workflow rilis harus memvalidasi dan memakai catatan rilis sebagai body.");
Check(uiProject.Contains("<Version>2.5.2</Version>", StringComparison.Ordinal) &&
      !System.Text.RegularExpressions.Regex.IsMatch(nsisSource, "(?m)^!define APP_VERSION \"") &&
      nsisSource.Contains("!ifndef APP_VERSION", StringComparison.Ordinal) &&
      nsisSource.Contains("!error", StringComparison.Ordinal) &&
      installerScript.Contains("PrayerAutomation.UI.csproj", StringComparison.Ordinal) &&
      installerScript.Contains("-DAPP_VERSION={version}", StringComparison.Ordinal) &&
      nsisSource.Contains("AyoVersionCompare", StringComparison.Ordinal) &&
      nsisSource.Contains("MB_YESNO", StringComparison.Ordinal) &&
      nsisSource.Contains("IfSilent version_check_done", StringComparison.Ordinal) &&
      nsisSource.Contains("/UPDATEMODE", StringComparison.Ordinal),
    "Versi hanya boleh berasal dari csproj; .nsi menolak kompilasi tanpa -DAPP_VERSION, dan installer mengonfirmasi versi sama/lebih baru hanya pada jalur manual.");
Check(nsisSource.Contains("UninstPage custom un.UninstallDataPageCreate un.UninstallDataPageLeave", StringComparison.Ordinal) &&
      nsisSource.Contains("Simpan pengaturan dan data saya", StringComparison.Ordinal) &&
      nsisSource.Contains("${NSD_Check} $UninstallKeepDataRadio", StringComparison.Ordinal) &&
      nsisSource.Contains("StrCpy $UninstallDeleteData 0", StringComparison.Ordinal) &&
      nsisSource.Contains("RMDir /r \"$APPDATA\\PrayerAutomation\"", StringComparison.Ordinal) &&
      nsisSource.Contains("$UninstallDeleteData == 1", StringComparison.Ordinal),
    "Uninstall harus menanyakan simpan/hapus data dengan default simpan, dan menghapus profil hanya bila dipilih.");
Check(nsisSource.Contains("w \" /UPDATEMODE \"", StringComparison.Ordinal) &&
      !nsisSource.Contains("StrStrIW(w r0, w \"/UPDATEMODE\")", StringComparison.Ordinal) &&
      nsisSource.Contains("w \"/D=\"", StringComparison.Ordinal) &&
      nsisSource.Contains("/D= tidak diizinkan", StringComparison.Ordinal),
    "Pencocokan /UPDATEMODE harus satu argumen utuh, dan /D= harus ditolak sebelum halaman pertama.");
Check(!nsisSource.Contains("\\r\\n", StringComparison.Ordinal) &&
      !nsisSource.Contains("\u2192", StringComparison.Ordinal) &&
      nsisSource.Contains("$\\r$\\n$\\r$\\n", StringComparison.Ordinal),
    "Teks installer memakai escape NSIS dan ASCII, tanpa escape literal atau panah Unicode.");
Check(appSource.Contains("ForceRestoreForUpdateAsync()", StringComparison.Ordinal) &&
      !appSource.Contains("WaitForRecoveryAsync(", StringComparison.Ordinal) &&
      appSource.Contains("pembaruan tetap dilanjutkan", StringComparison.Ordinal),
    "Preflight update tidak boleh menunggu 60 detik dan harus memaksa pemulihan lalu melanjutkan.");
Check(currentNotes.Contains("## Perbaikan", StringComparison.Ordinal) &&
      currentNotes.Contains("## Fitur baru", StringComparison.Ordinal) &&
      currentNotes.Contains("self-contained", StringComparison.OrdinalIgnoreCase) &&
      !currentNotes.Contains("user meminta", StringComparison.OrdinalIgnoreCase),
    "Catatan rilis versi berjalan harus memakai bagian yang dikenali dan bahasa pihak ketiga.");
Check(changelog.Contains("## v2.5.2", StringComparison.Ordinal) &&
      changelog.Contains("## v2.5.1", StringComparison.Ordinal) &&
      changelog.Contains("## v2.5.0", StringComparison.Ordinal) &&
      changelog.Contains("## v2.4.3", StringComparison.Ordinal) &&
      changelog.Contains("## v2.1.2", StringComparison.Ordinal) &&
      newestNotes.Contains("## Perbaikan", StringComparison.Ordinal) &&
      newestNotes.Contains("self-contained", StringComparison.OrdinalIgnoreCase) &&
      !newestNotes.Contains("user meminta", StringComparison.OrdinalIgnoreCase) &&
      !newestNotes.Contains("diminta", StringComparison.OrdinalIgnoreCase) &&
      !newestNotes.Contains("dibantu", StringComparison.OrdinalIgnoreCase),
    "Changelog memuat semua versi dan catatan rilis 2.5.1 memakai bahasa pihak ketiga.");

string updateServiceSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "UpdateService.cs"));
Check(updateServiceSource.Contains("UpdateManifest.IsSafeAssetName(asset.Name)") &&
      updateServiceSource.Contains("UpdateManifest.RequiresSignedManifest(release.Version)") &&
      updateServiceSource.Contains("if (!verified && signedRequired) return null;") &&
      updateServiceSource.Contains("manifest.FallbackAsset") &&
      updateServiceSource.Contains("manifest.InstallerAsset"),
    "Updater harus memvalidasi nama aset dan tidak memakai fallback unsigned untuk rilis bertanda tangan.");
Check(updateServiceSource.Contains("ZipEntryPath.TryGetSafeRelative(entry.FullName, root, out string relative)") &&
      updateServiceSource.Contains("DeltaCoversPlan(deltaPath, plan, root)") &&
      !updateServiceSource.Contains("Path = entry.FullName") &&
      !updateServiceSource.Contains("archive.Entries.Select(entry => entry.FullName)"),
    "Updater harus menyaring entri ZIP lewat ZipEntryPath sebelum memakai nama berkas.");
Check(updateServiceSource.Contains("AYOJAMAAH_UPDATE_URL") &&
      updateServiceSource.Contains("AYOJAMAAH_UPDATE_TOKEN") &&
      updateServiceSource.Contains("#if AYOJAMAAH_INTERNAL_UPDATE") &&
      updateServiceSource.Contains("Environment.GetEnvironmentVariable(name)") &&
      updateServiceSource.Contains("InternalUpdateUrl ?? ReleasesUrl") &&
      updateServiceSource.Contains("api.github.com"),
    "Kanal internal hanya dibaca dari ENV pada build ber-symbol; stable tetap memakai rilis latest.");
int internalStart = updateServiceSource.IndexOf("#if AYOJAMAAH_INTERNAL_UPDATE", StringComparison.Ordinal);
int internalEnd = updateServiceSource.IndexOf("#else", internalStart, StringComparison.Ordinal);
Check(internalStart >= 0 && internalEnd > internalStart &&
      updateServiceSource[internalStart..internalEnd].Contains("Environment.GetEnvironmentVariable(name)") &&
      !updateServiceSource[..internalStart].Contains("Environment.GetEnvironmentVariable(") &&
      !updateServiceSource[internalEnd..].Contains("Environment.GetEnvironmentVariable("),
    "Pembacaan ENV hanya boleh berada di dalam blok symbol internal.");
Check(updateServiceSource.Contains("GitHubApiHost") &&
      updateServiceSource.Contains("IsGitHubApi(uri)") &&
      updateServiceSource.Contains("AllowAutoRedirect = false") &&
      updateServiceSource.Contains("request.Headers.Authorization") &&
      updateServiceSource.Contains("response.Headers.Location") &&
      updateServiceSource.Contains("new Uri(uri, response.Headers.Location)"),
    "Token hanya dikirim ke api.github.com; redirect ke CDN diikuti tanpa Authorization.");
Check(updateServiceSource.Contains("ApiUrl = null") &&
      updateServiceSource.Contains("apiElement") &&
      updateServiceSource.Contains("asset.ApiUrl : asset.Url"),
    "Aset draft/privat memakai URL API, bukan browser_download_url, saat token tersedia.");

// Auto-update lewat delta/helper tidak menjalankan NSIS, jadi registry Harus
// disinkronisasi oleh helper dan oleh startup agar Programs & Features tidak stale.
string installSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "StartupRegistration.cs"));
string applierSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "UpdateApplier.cs"));
Check(installSource.Contains("SyncInstalledVersion(string root, string version)", StringComparison.Ordinal) &&
      installSource.Contains("IsInstalledPath(root)", StringComparison.Ordinal) &&
      installSource.Contains("key.SetValue(\"DisplayVersion\", version, RegistryValueKind.String)", StringComparison.Ordinal) &&
      installSource.Contains("key.GetValue(\"DisplayVersion\")", StringComparison.Ordinal),
    "DisplayVersion hanya ditulis untuk copy terpasang dan hanya saat nilainya berbeda.");
int syncStart = installSource.IndexOf("internal static void SyncInstalledVersion", StringComparison.Ordinal);
int syncEnd = installSource.IndexOf("/// <summary>Builds the quoted Run-registry", StringComparison.Ordinal);
Check(syncStart >= 0 && syncEnd > syncStart &&
      installSource[syncStart..syncEnd].Contains("catch (Exception ex) when", StringComparison.Ordinal) &&
      installSource[syncStart..syncEnd].Contains("AppLogger.Write(\"install.version\", ex)", StringComparison.Ordinal),
    "Gagal menulis registry tidak boleh menggagalkan pembaruan yang sudah diterapkan.");
Check(applierSource.Contains("journal.Status = \"applied\"", StringComparison.Ordinal) &&
      applierSource.Contains("journal.Save(job.JournalPath);\n                if (job.IsInstalled) StartupRegistration.SyncInstalledVersion(job.TargetRoot, job.TargetVersion);",
          StringComparison.Ordinal),
    "Helper harus menulis DisplayVersion setelah journal pembaruan berhasil disimpan.");
Check(appSource.Contains("StartupRegistration.SyncInstalledVersion(\n            Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory,\n            AppBranding.Version)",
          StringComparison.Ordinal),
    "Startup harus merekonsiliasi DisplayVersion dengan versi biner yang sedang berjalan.");

Console.WriteLine("UpdateLogicSmoke: PASS");
