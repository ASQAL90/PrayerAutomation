# Verifikasi Portable C# WPF + Rust

## Status dan arti hasil

**Regresi terkini:** 23 Oktober 2026, `12:24:17–12:25:42 +07:00`; seluruh **14 langkah** tercatat exit code `0`, termasuk **9 suite C# dengan assertion**. Portable kemudian diperiksa: metadata dan SHA256 cocok dengan hasil build. Bukti diturunkan dari JSON/log, bukan keberadaan EXE: [evidence portable terkini](evidence/portable-20261023-122417.json).

**Lifecycle dan typography:** reproduksi RED tersimpan, patch minimal, GREEN, dan rerun parent sudah direkonsiliasi. Lifecycle berubah dari 8 gagal menjadi 16/16 lulus; form diuji pada dua ukuran. **Dua review SPEC serta review kualitas/integrasi independen lulus**, pada snapshot yang cocok SHA256. Tidak ada blocking gap, temuan keamanan, atau kesalahan logika dalam lingkup review tersebut. GREEN dan review statis bukan acceptance pengguna atau persetujuan installer. [Rincian RED/GREEN](evidence/lifecycle-typography-20261023-122417.json) · [Verdict SPEC](evidence/independent-spec-20261023-122627.json) · [Verdict kualitas/integrasi](evidence/independent-quality-20261023-122627.json).

Baseline `08:50:30` tetap disimpan sebagai [bukti historis](evidence/portable-20261023-085030.json), bukan identitas payload terbaru. HEAD Git saat verifikasi: `9d364a9e6ad9e42098f2bc6a340f9c27357a5e17`; perubahan belum di-commit. SHA256 source yang benar-benar diuji tercatat di evidence terkini.

Target portable: `../dist/PrayerAutomation.WPF-lockscreen-test/PrayerAutomation.UI.exe`. Pertahankan seluruh folder; file EXE saja bukan paket lengkap. Source canonical berada di D:, C: hanya backup.

## Batas keselamatan suite

- Dijalankan sebagai **standard user**, bukan elevated.
- Tidak menjalankan aplikasi utama atau `SingleInstanceSmoke` karena profil lama memiliki auto-lock aktif.
- WPF diuji pada STA/Dispatcher terisolasi dengan pengaturan sintetis; tidak memanggil `Application.Run`/startup asli, scheduler native nyata, penguncian sesi atau setter personalisasi Windows.
- Pengujian backend/workflow memakai native-platform/store/scheduler boundaries terisolasi; file uji memakai folder scratch, bukan jurnal produksi.
- API probe hanya memeriksa dukungan dan indikator mode secara read-only. `supported=true` bukan bukti izin setter atau keberhasilan visual.
- Hash konfigurasi asli, isi dua lokasi jurnal, startup per-user dan indikator mode dibandingkan sebelum/sesudah safe suite: **tidak berubah** pada run terkini. Pengujian ini tidak mematikan opsi pengguna secara diam-diam.
- Lima flag tindakan pada fixture widget dibuat `false` dan diperiksa sebelum Window dibuat. Fixture form juga mematikan widget. Default produk tidak diubah untuk membuat tes terlihat aman.
- Tes geocoding memakai fixture/HTTP mock; tidak mengirim koordinat perangkat nyata.

Portable tetap membaca profil asli `%APPDATA%\PrayerAutomation\config.json` saat pengguna menjalankannya. Folder build baru **bukan profil kosong**. Jangan membuka EXE hanya untuk membaca versi atau memeriksa tema bila risiko auto-lock belum diterima.

**Batas riwayat isolasi:** harness terdahulu sempat memompa Dispatcher sebelum callback startup implisit dari constructor `Application` dibatalkan. Guard menemukan objek single-instance sudah terbentuk dan menghentikan replay; hash profil, jurnal, startup dan indikator mode yang dipantau tetap sama. Harness terkoreksi membatalkan callback yang tepat sebelum pump dan memeriksa objek startup tetap null. Bukti “tanpa startup prefix” hanya berlaku pada RED tervalidasi, GREEN, dan run final yang disebut, **bukan seluruh percobaan terdahulu**. Dampak sesi/native dari percobaan lama tidak dibuktikan hanya oleh hash. Insiden dan batasnya tercatat pada evidence lifecycle.

## Hasil terkini yang benar-benar dijalankan

| Langkah | Exit | Lingkup bukti |
|---|---:|---|
| Rust release tests | 0 | 8 tes, target GNU Windows x64, dependency locked/offline |
| CalendarSmoke | 0 | Round-trip Masehi/Hijriyah, panjang bulan, marker dan koreksi tampilan |
| LocationDetectionSmoke | 0 | Dua provider mock, normalisasi koordinat/timezone, disagreement dan minimisasi IP |
| OfflineGeocoderSmoke | 0 | Lookup GeoNames bundled; online consent/HTTPS/source gate dan HTTP mock |
| PrayerScheduleSmoke | 0 | Delapan metode, waktu turunan, offset manual dan filter reminder |
| PrayerUiLogicSmoke | 0 | Lima ringkasan hadis offline, determinisme, lifecycle logic dan safety gates |
| LockScreenBackendSmoke | 0 | Backend produksi dengan native boundary injected; Spotlight mode-only, Picture path/hash dan ownership |
| LockScreenWorkflowSmoke | 0 | Jurnal, fail-closed, backoff, in-flight Exit, deadline 60 menit, extension dan override sintetis |
| WidgetLayoutSmoke | 0 | Actual WPF layout, preview/Cancel/Save/tab, helper, bitmap marquee, control/text contrast |
| FormTypographySmoke | 0 | Actual WPF paragraf/copy, checkbox/accessible name, safety dinamis, error-text builder dan konten scroll |
| LockScreenApiSmoke read-only | 0 | Dukungan API dan indikator mode; tidak melakukan setter |
| WPF Release build | 0 | `0 Warning(s)`, `0 Error(s)` |
| Self-contained win-x64 publish | 0 | Portable di folder test, bukan installer |
| Git whitespace | 0 | `git diff --check`; bukan klaim working tree clean |

Jumlah suite C# dengan assertion: **9**. API probe read-only dilaporkan terpisah; jangan menghitungnya sebagai tes native write. `SingleInstanceSmoke` sengaja tidak dijalankan.

### Smoke 2.4.0 pada lingkungan build Termux

| Smoke | Exit | Lingkup bukti |
|---|---:|---|
| OfflineGeocoderSmoke | 0 | Lapisan Indonesia 258 rb titik + poligon, format nama provinsi Indonesia, perbatasan, negara terjemahan, consent/HTTPS/mock |
| LockScreenPolicySmoke | 0 | Penolakan `ManagedByPolicy` sebelum snapshot/journal, transaksi aktif tetap dipulihkan, kontrak nama GUID unik per transaksi dan kepemilikan managed tetap teruji |
| LocationDetectionSmoke | 0 | Dua provider jaringan, normalisasi koordinat/zona waktu, penanganan beda hasil, dan minimalisasi IP |
| UpdateLogicSmoke | 0 | Logika versi/manifest/planner/job, batas tunggu pemulihan 60 detik dengan retry 5 detik, timeout tidak menghapus jurnal, dan hitungan mundur pengingat tray |
| CalendarSmoke | 0 | Round-trip Masehi/Hijriyah, panjang bulan, observance/no-fast, koreksi, accessible name; dijalankan dari `/root` dengan `libprayer_core.so` Linux |
| PrayerUiLogicSmoke | 0 | Grouping Pengaturan, label "geser tanggal Hijriyah", gate lock screen, lifecycle, widget, kontras weekend turunan tema, grid empat tombol lock screen, tombol reset pabrik, dan pemulihan preview tema setelah ganti sumber kalender; dijalankan dari `/root` dengan `libprayer_core.so` Linux |
| WPF Release build x64 | 0 | `0 Warning(s)`, `0 Error(s)` setelah versi 2.4.0, pemulihan path GUID, alur updater hold/wait + Nanti saja, tombol lock screen sejajar, reset pabrik, dan perbaikan preview tema saat ganti sumber kalender; [log build](evidence/build-2.4.0-x64.txt) |
| Self-contained win-x64 publish | 0 | Portable di `/storage/emulated/0/Develop/AyoJamaah-build/portable-x64`; 485 file, 210 MB, metadata assembly `2.4.0.0`, `cmp` aset/DLL cocok dengan build output; [log publish](evidence/publish-2.4.0-x64.txt) |

Build/publish dijalankan lewat `proot-distro login debian` berisi .NET 10 SDK + Rust mingw; **bukan** `dotnet`/`cargo` native Termux. Wrapper `ayojamaah-build` sempat kena `MSBuild ThreadStateException: Unable to set thread priority`, sehingga log final memakai `dotnet build`/`publish` dengan `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, `MSBUILDDISABLENODEREUSE=1`, `DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER=1`, dan `-p:EnableWindowsTargeting=true` (tanpa flag itu Linux SDK menolak target Windows dengan `NETSDK1100`); klaim ini bersandar pada log perintah dan artefak yang dihasilkan, bukan pada asumsi lingkungan.

Smoke .NET dijalankan lewat `dotnet <dll>`, bukan apphost, karena bind mount repo tidak mengizinkan eksekusi apphost. `CalendarSmoke` dan `PrayerUiLogicSmoke` awalnya berhenti pada assertion narasi/struktur yang sudah basi terhadap XAML 2.4.0; ekspektasi smoke diperbarui, lalu keduanya exit `0`. Keduanya memerlukan `libprayer_core.so` Linux di folder eksekusi `/root`; menjalankannya langsung dari bind mount gagal memetakan shared object. Ini **bukan** runtime WindowsDesktop: smoke WPF nyata (`WidgetLayoutSmoke`, `FormTypographySmoke`, smoke lock screen lama, dll.) masih tidak dijalankan di Termux dan statusnya tetap seperti tabel lama, bukan bukti 2.4.0.

**Pemulihan path GUID terkini** tidak mengganti verifikasi: seluruh langkah pada tabel di atas dijalankan ulang setelah perubahan dan exit `0`. Build dan publish x64 juga dijalankan ulang; portable akhir berisi 485 file/210 MB, metadata assembly `2.4.0.0`, dan `cmp` cocok untuk `AyoJamaah.dll`, `prayer_core.dll`, `sedang_sholat.png`, serta tiga berkas GeoNames terhadap sumber/build output. Build final 20 November 2026 pukul `13:03:29 UTC` dan publish final pukul `13:04:28 UTC`, dengan flag MSBuild di atas. Nama berkas kini dibuat dengan `NewTemporaryImageName`/`NewRestoreImageName` memakai prefix managed yang sama, sehingga `IsManagedFile`, cleanup orphan, dan penolakan penimpaan berkas yang sedang aktif tetap berlaku. Path unik memaksa Windows mengalokasikan slot cache baru; entri `Recent pictures` dapat bertambah dan tetap memerlukan penghapusan manual lewat Settings Windows seperti di [batas lock screen](LOCKSCREEN_LIMITATIONS_ID.md). Tidak ada klaim `visual=verified` dari log.

**Updater hold/wait dan “Nanti saja”:** `UpdateLogicSmoke` dijalankan ulang setelah penyesuaian terakhir dan menuruti batas 60 detik, retry 5 detik, timeout yang tidak menghapus jurnal, serta hitungan mundur pengingat yang mengikuti deadline atau status pemulihan. Menutup jendela saat menunggu membatalkan pembaruan dan mengembalikan penjadwalan normal tanpa menghapus jurnal. Tray tooltip, popup `ConfirmDialog`, dan tombol `Nanti saja` tidak dijalankan di Termux; yang terbukti adalah kebijakan waktu dan gerbang jurnal, bukan tampilan tray atau hasil visual Windows.

**Tombol dan tema di Pengaturan:** empat aksi lock screen kini berbagi satu grid dua kolom dua baris yang sejajar (`Coba gambar sholat`, `Kembalikan gambar semula`, `Hentikan pemulihan otomatis`, `Matikan fitur ini di PC ini`). Tombol lama **Pulihkan pengaturan dari cadangan** diganti **Reset pengaturan awal**: `SettingsStore.Reset()` menghapus `config.json`, `config.json.bak`, dan file `.tmp`-nya, lalu memakai default instal baru (KHGT) untuk sesi berjalan; log, unduhan, dan jurnal lock screen tetap. Anomali tema saat pindah sumber kalender tanpa menyimpan terjadi karena `ApplyCalendarSource` memanggil `_refreshDependentViews()`, yang memakai `ThemeId` tersimpan, bukan pilihan yang baru dipreview. Penambahan `_applyThemePreview(SelectedThemeId)` setelah refresh mempertahankan preview tema; `PrayerUiLogicSmoke` memeriksa urutan ini. `config.json.bak` tetap dibuat saat **Simpan**, tetapi tidak lagi dipulihkan lewat tombol di Pengaturan.

**Publikasi 2.4.0:** commit `4d6ab67` dan tag beranotasi `v2.4.0` sudah di-push ke `github` dan mirror `origin`. Workflow `Release` dari tag tersebut selesai sukses (run `44747832207`, 6m32s, semua langkah hijau) dan [GitHub Release v2.4.0](https://github.com/ASQAL90/PrayerAutomation/releases/tag/v2.4.0) memuat 11 aset: installer EXE/ZIP, portable x64/x86, `SHA256SUMS.txt`, serta manifest JSON, tanda tangan `.sig`, dan delta ZIP untuk kedua arsitektur. Aset dibangun CI di Windows; ini bukan bukti instalasi, updater, atau tampilan visual di PC pengguna, dan tidak mengubah status `visual=unverified`.

### Hotfix 2.4.1 — kegagalan update dan jendela tertutup

Dua kegagalan 2.4.0 diperbaiki tanpa memindahkan tag `v2.4.0`: (1) `Dispatcher` dibaca dari thread latar setelah `ConfigureAwait(false)` pada progress pemulihan, auto-check, status `MainWindow`, dan aksi tray; (2) `ShowMainWindow()` memanggil `Show()` pada jendela yang sudah ditutup setelah `ReleaseAndShutdown()` gagal di tengah jalan. Perbaikannya: dispatcher ditangkap di thread UI, `InstallDeltaAsync`/`AbortUpdatePreparationAsync` tetap di thread UI sampai shutdown, cleanup shutdown dibungkus `try/finally` agar `Shutdown()` selalu dijalankan, jendela utama dibuat ulang lewat `CreateMainWindow()` bila tray membukanya setelah tertutup, dan tombol **Periksa lagi** aktif kembali setelah gagal. Assertion regresi ditambahkan ke `UpdateLogicSmoke`.

Pada sumber 2.4.1: WPF Release build x64 `0 Warning(s)`, `0 Error(s)`; `UpdateLogicSmoke`, `CalendarSmoke`, `PrayerUiLogicSmoke`, `LockScreenPolicySmoke`, `LocationDetectionSmoke`, dan `OfflineGeocoderSmoke` exit `0` (dua smoke terakhir dijalankan dari `/tmp` dengan `libprayer_core.so` Linux). `cargo fmt --all --check` exit `0`. Commit format `27600a7` membuat CI Rust kembali hijau (run `44806295719` sukses).

**Publikasi 2.4.1:** commit `21670ed` dan tag beranotasi `v2.4.1` sudah di-push ke `github` dan mirror `origin`; tag `v2.4.0` tidak dipindahkan. Workflow `Release` dari tag `v2.4.1` sukses (run `44806836665`, 6m22s) dan [GitHub Release v2.4.1](https://github.com/ASQAL90/PrayerAutomation/releases/tag/v2.4.1) memuat 11 aset dengan pola sama seperti 2.4.0: installer EXE/ZIP, portable x64/x86, `SHA256SUMS.txt`, serta manifest JSON, `.sig`, dan delta ZIP kedua arsitektur. Ini tetap bukti build/smoke di Termux dan CI, bukan uji update 2.4.0 → 2.4.1 di Windows; status `visual=unverified` dan `update=unverified` pada PC pengguna tetap berlaku sampai log serta hasil uji dikirim.

### Rilis 2.4.2 — catatan rilis, changelog, dan preflight

Popup pembaruan kini menampilkan ringkasan dari `body` rilis terbaru saja, dengan `ScrollViewer` vertikal dan style global `App.xaml` (tanpa style scrollbar baru). Tombol **Riwayat perubahan** di halaman Tentang membuka `docs/CHANGELOG.md` dari resource tertanam, berisi semua versi dari 2.1.2 sampai 2.4.2. Preflight update mencoba pemulihan normal sekali, lalu memaksa jurnal lock screen ditutup dan tetap melanjutkan pembaruan bila verifikasi gagal; tidak ada tunggu 60 detik lagi. Pemasang NSIS menanyakan konfirmasi Ya/Tidak dengan default Tidak bila versi terpasang sama atau lebih baru pada instalasi manual; `/UPDATEMODE` dan silent mode melewati dialog agar updater otomatis tidak menggantung. Workflow `Release` mewajibkan `docs/release-notes/<versi>.md` dan memakainya sebagai `body_path`.

Pada sumber 2.4.2: WPF Release build x64 `0 Warning(s)`, `0 Error(s)` lewat proot Debian dengan `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, `-p:EnableWindowsTargeting=true`, dan toolchain Rust `/opt/cargo` + `/opt/rustup`. `UpdateLogicSmoke`, `CalendarSmoke`, `PrayerUiLogicSmoke`, `LockScreenPolicySmoke`, `LocationDetectionSmoke`, dan `OfflineGeocoderSmoke` exit `0`; smoke dengan native dijalankan dari `/root` memakai `libprayer_core.so` Linux dan `LD_LIBRARY_PATH`. `cargo fmt --all --check` exit `0`. `git diff --check` bersih. Makro `${AyoVersionCompare}` ditinjau statis (termasuk perbaikan pemulihan register stack dan kanonisasi segmen nol); `makensis` tidak dijalankan lokal agar tidak menghasilkan installer.

Uji workflow `Release` dari branch `master`: run `44834408908` gagal di `Build NSIS installer` karena penjaga `IfErrors` ditulis sebagai makro LogicLib (`${IfErrors}`); diperbaiki di commit `0f7c4f8` memakai perintah inti `IfErrors`. Uji ulang run `44834722704` sukses `5m53s`: validasi catatan rilis, publish portable x64/x86, receipt, installer NSIS, ZIP, delta bertanda tangan, dan upload artifact semuanya hijau. Langkah **Publish GitHub release** sengaja terlewat karena ref bukan `refs/tags/v*`, jadi rilis dan aset yang sudah ada tidak disentuh.

**Publikasi 2.4.2:** commit `cdc0bc6` dan tag beranotasi `v2.4.2` sudah di-push ke `github` dan mirror `origin`; tag `v2.4.0` dan `v2.4.1` tidak dipindahkan. Workflow `Release` dari tag `v2.4.2` sukses (run `44835744434`, `5m37s`) dan [GitHub Release v2.4.2](https://github.com/ASQAL90/PrayerAutomation/releases/tag/v2.4.2) memuat 11 aset: installer EXE/ZIP, portable x64/x86, `SHA256SUMS.txt`, serta manifest JSON, `.sig`, dan delta ZIP kedua arsitektur. Body rilis berasal dari `docs/release-notes/2.4.2.md`, bukan `generate_release_notes`. Ini bukti build/smoke dan CI, bukan uji pembaruan 2.4.1 → 2.4.2 di Windows; status tersebut ditutup oleh acceptance PC pada 21 November 2026 di bawah.

**Status draft dan kanal internal:** rilis `v2.4.2` sudah diubah menjadi draft tanpa menghapus tag atau 11 aset; `GET /releases/latest` kini mengembalikan `v2.4.1`. Build uji dapat mengarahkan updater ke draft lewat `AYOJAMAAH_UPDATE_URL` dan `AYOJAMAAH_UPDATE_TOKEN`, tetapi keduanya hanya dibaca bila assembly dibangun dengan symbol `AYOJAMAAH_INTERNAL_UPDATE`. Token dikirim hanya ke `api.github.com`; redirect ke CDN diikuti tanpa `Authorization`, dan aset draft memakai URL API, bukan `browser_download_url`. Build stable tidak membaca ENV kanal ini dan tetap memakai `releases/latest`. Panduan lengkap ada di [kanal pembaruan internal](INTERNAL_UPDATE_CHANNEL.md). `UpdateLogicSmoke` exit `0` dengan assertion override, batas host, redirect, dan URL API aset. WPF Release build x64 tanpa symbol `0 Warning(s)`, `0 Error(s)`; build uji ber-symbol `AYOJAMAAH_INTERNAL_UPDATE` juga `0 Warning(s)`, `0 Error(s)`. Pemeriksaan biner: `GetEnvironmentVariable` tidak ada di DLL stable dan ada di DLL uji; nama variabel memang tetap tercantum sebagai konstanta, tetapi tidak dipanggil pada stable. Kanal ini sudah dibuktikan di Windows pada 21 November 2026; lihat acceptance di bawah.

### Acceptance Windows 2.4.2 — 21 November 2026

**Status akhir: `visual=verified`, `update=verified`** untuk kanal internal dan draft `v2.4.2`. Bukti berasal dari PC pengguna, bukan dari build Termux atau CI.

- **Draft dipulihkan:** release draft sempat terdiscard, lalu dibuat ulang dari tag `cdc0bc6` memakai 11 aset artifact workflow run `44835744434` (release ID `453384124`). Digest aset identik dengan aset lama; `GET /releases/latest` tetap `v2.4.1`.
- **Visual:** konfirmasi manual 5/5 lolos — changelog 2.1.2–2.4.2 terbuka, scrollbar ringkasan vertikal saja, kotak detail tanpa scrollbar, label “geser tanggal Hijriyah”, dan grid 2×2 empat tombol lock screen sejajar. Screenshot dan UIA ada di `visual-output\screenshots\` dan `visual-result.json`. Tiga smoke lolos: `UpdateLogicSmoke`, `PrayerUiLogicSmoke`, `LockScreenPolicySmoke`.
- **Tiga smoke visual gagal karena harness lama, bukan regresi aplikasi:** `AboutWindowSmoke` menuliskan `assembly=PrayerAutomation.UI` padahal `AssemblyName` produk adalah `AyoJamaah`; `WidgetLayoutSmoke` masih menuntut tinggi 40 DIP sementara `RequestedHeight` memakai 34; `FormTypographySmoke` masih mencari elemen geocoding online yang sudah dihapus dan margin lama. Kode produksi tidak diubah untuk mengatasi ini.
- **Patch kit 2,4 MB** diterapkan di PC ke `C:\qc-kit-2.4.2\smoke\`: DLL runtime WPF yang benar, `PrayerUiLogicSmoke.dll`, dan `UpdateLogicSmoke.dll` beserta sumber pendukung. Kit 420 MB tidak disalin ulang.
- **Update:** `update-check.ps1` melaporkan `accepted=true`, `before=2.4.1+dcc7f01…`, `after=2.4.2+cdc0bc6…`, `filesChecked=485`, `mismatches=0`, `timedOut=false`, `tokenPersisted=false`. Popup muncul dan tombol “Unduh dan pasang” diklik; aset diambil lewat URL API draft dengan token, redirect CDN tanpa `Authorization`. Skrip lokal perlu membuang metadata SemVer sebelum cast `[version]` di PowerShell 5.1; skrip di kit tidak diubah.
- **Installer manual:** pengguna mengonfirmasi `/UPDATEMODE` lanjut tanpa dialog dan konfirmasi versi sama/lebih baru pada instalasi manual berjalan OK di PC. Bersama bukti visual dan update di atas, cakupan uji 2.4.2 lengkap.
- **Batas:** ini membuktikan jalur draft/kanal internal, helper, dan installer. Sinkronisasi `DisplayVersion` (commit lokal `552804d`, disiapkan pada `v2.4.3`) belum diuji di PC. Tidak ada push, tag, workflow, atau publish baru dari sesi QC ini.
- **Persiapan 2.4.3:** nomor versi dinaikkan pada csproj dan NSIS, catatan rilis serta changelog ditambah, dan assertion harness visual diselaraskan dengan produk saat ini. Build WPF Release stable/ber-symbol serta smoke lokal lolos; aset, tag, rilis, dan uji PC untuk 2.4.3 belum dibuat.
- **Tindak lanjut harness 2.4.3:** putaran pertama `HANDOFF-PC.md` melaporkan 3 smoke gagal karena paket biner, bukan regresi: DLL produk di `smoke\` masih 2.4.2 dan `*.deps.json` masih mengunci 2.4.2 sementara harness sudah menaut ke 2.4.3, sedangkan `AboutWindowSmoke` kehilangan deklarasi `xmlns:sys`. Paket `handoff-fix/` berisi DLL produk 2.4.3 dan deps.json berhasil membetulkan `appVersion` serta `FileNotFoundException`. Putaran kedua menyisakan 3 FAIL baru: prefix `local` tanpa `;assembly=AyoJamaah`, batas tepi kiri teks widget yang mengabaikan ikon, dan assertion tipografi/form yang belum mengikuti margin produksi. Paket `handoff-fix2/` memulihkan `AboutWindowSmoke` dan assertion ikon widget, sehingga putaran ketiga menjadi 4 PASS / 2 FAIL. Dua FAIL terakhir murni assertion lama: `WidgetLayoutSmoke` perlu Pump setelah Cancel dan memeriksa tag tema tersimpan, sedangkan `FormTypographySmoke` memakai teks bantuan/margin produksi saat ini, menghitung grup koreksi `>= 7` karena ada editor Duha tambahan, serta melewati tangkapan layer tak terlihat yang memicu `RenderTargetBitmap(pixelWidth=0)`. Patch `handoff-fix3/` menyesuaikan harness saja; kode produksi tidak diubah. Hasil ulang PC masih ditunggu.

### Instrumentasi first-open kalender (2.4.0)

- Titik ukur ada di `MainWindow.RefreshCalendarViews()` saat pemanggilan pertama dari `EnsureCalendarViewsLoaded()`. Stopwatch terpisah untuk `RenderGregorianYear` dan `RenderHijriYear`, lalu satu baris `AppLogger.WriteInfo("calendar.first-open", "gregorianMs=… hijriMs=… totalMs=… source=civil|khgt")` per proses. Tidak ada perubahan layout atau area kalender 12 bulan.
- Cara ukur yang disyaratkan: proses aplikasi baru, profil/config sama, buka tab Kalender lewat tray (`App.OpenCalendar()`), baca satu baris tersebut; ulang untuk sumber `civil` dan `khgt` pada data yang sama. `CalendarSmoke` hanya menguji logika domain dan inspeksi sumber, jangan memakai angkanya sebagai first-open UI.
- **Status:** instrumen terpasang dan build Release lolos, tetapi angka belum diukur karena WPF tidak bisa dijalankan di lingkungan build ini. Ambang performa belum ditetapkan; akan diajukan setelah angka dari PC target tersedia.

### Bukti tampilan, bukan acceptance seluruh monitor

- Delapan tema: pemeriksaan control/state `368`, rasio minimum `5.095:1`, kegagalan `0`.
- Teks ter-render: pemeriksaan `10.920`, rasio minimum `4.673:1`, kegagalan `0`.
- Helper koreksi: `64` pemeriksaan style/ukuran/font/warna normal dan disabled.
- Marquee bitmap: `64` frame, string pendek/produksi, viewport full/narrow, raster DPI `96/144`; frame kosong `0`, entry sisi kanan diperiksa.
- Timer, perubahan jam/countdown, offset preservation dan rotasi putaran diperiksa dalam harness. Lima transisi putaran runtime dipicu dekat batas dan dicatat; ini bukan rekaman seluruh tick.

**Batas marquee:** render DPI bitmap bukan perubahan DPI layout window pada monitor nyata. Sampling offset/transform bukan bukti seluruh tick `AdvanceMarquee` saat wrap dan penggantian konten. Hidden-text baseline/reference ROI serta actual per-monitor DPI belum diperiksa. Jangan mengubah bukti `64/64` menjadi klaim semua kombinasi font/monitor/wrap sudah diterima.

### Notifikasi proses lama

Log lama `prayerautomation-all-text-contrast-green.log` berakhir exit `1`: control brushes lulus tetapi teks ter-render memiliki `120` kegagalan, termasuk putih di atas putih. Nama file “green” tidak berarti hasil lulus. Log run terkini mencatat nol kegagalan. Ini perbandingan hasil tersimpan, bukan tes ulang proses lama.

### Perapian form: RED lalu GREEN

- Paragraf memakai font `14 DIP`, line step `20 DIP`, `BlockLineHeight`, wrapping, dan rata kiri. Petunjuk memakai margin kiri/kanan `3`, atas `4`, bawah `8 DIP`; label field mempertahankan tata letak field.
- Keterangan lokasi, consent online, widget, tema, startup dan opsi Windows diringkas. Tujuh label checkbox memakai paragraf wrap dengan jarak dari glyph; nama aksesibilitas diperiksa pada kontrol aktual.
- Alasan keselamatan Windows tetap berasal dari `safety.Message`. Status scheduler non-null tetap tampil terpisah; pesan default tidak ditampilkan dua kali. Tes tidak menghapus alasan keselamatan demi teks pendek.
- Petunjuk koreksi dan dua jalur error jadwal kini memakai style yang sama; warna error tema dipertahankan. Builder error diuji memakai pesan sintetis, tanpa memaksa native failure atau menulis log produksi.
- RED awal mencatat `87` assertion gagal. RED khusus dua gap reviewer mencatat petunjuk koreksi/error belum seragam; GREEN terkini memiliki nol kegagalan.
- Layout aktual `940×650` dan `1180×820 DIP`, DPI layout `96`: masing-masing `33` paragraf dan `12` label field diperiksa; bagian bawah dapat dicapai dengan scroll.

**Batas visual form:** screenshot konten panjang tidak berarti semua kontrol muat sekaligus dalam viewport. Pada lebar 940 DIP, nama metode di ComboBox tertutup tampak terpotong; bentuk/lebar kontrol lama dipertahankan. Tepi sebagian tombol `+` perlu acceptance visual pengguna. Screenshot 1180 DIP adalah composite dengan keterbatasan capture, bukan bukti seluruh form bebas clipping atau semua DPI diterima.

## Metadata dan isi payload terkini

- Apphost EXE dan native DLL: AMD64, PE32+.
- Manifest: `requestedExecutionLevel=asInvoker`, `uiAccess=false`.
- Runtime bundled: `Microsoft.NETCore.App 10.0.9` dan `Microsoft.WindowsDesktop.App 10.0.9`.
- `480` berkas; verifier tidak menemukan helper/admin/script yang dilarang dalam payload.
- EXE/DLL/PDB managed cocok SHA256 dengan build output pada pemeriksaan tersebut.
- Rust DLL, ikon, PNG dan GeoNames cocok dengan sumber aset. PNG `1920×1080`; data GeoNames `171.572` baris.
- Tujuh fungsi native C ABI **murni** benar-benar dimuat/dipanggil dengan fixture sintetis. Tidak memanggil Windows lock-screen/session API.

Hash penting ada pada [JSON evidence terkini](evidence/portable-20261023-122417.json). Hash itu mengidentifikasi payload run `12:24:17`; publish berikutnya perlu verifier baru. Load DLL dan manifest tidak membuktikan PC bersih atau kebijakan organisasi mengizinkan tindakan.

## Rekonsiliasi temuan reviewer

| Temuan | Bukti saat dicatat | Hasil verifikasi terkini |
|---|---|---|
| Startup completion menimpa draft/visibility; baseline awal null hilang | RED tervalidasi: 8/16 kasus gagal pada continuation produksi | `CompleteWidgetStartup` menjaga draft, baseline saved-enabled, manual-hide dan shutdown. GREEN tersimpan dan parent rerun masing-masing 16/16; startup prefix tidak dijalankan |
| JSON fixture memakai `reminders_enabled`, bukan mapping `enabled` | Default reminder true; unknown key masuk extension data | Key sekarang `enabled=false`; guard lima flag tindakan sebelum constructor. Fixture utama juga set lima flag eksplisit; guard RED exit 1 kemudian GREEN |
| Motion aktual dan initial-null rollback belum dicakup | Coverage interaksi sebelumnya | Checkbox/motion/Cancel/Save/tray, initial-null, saved-disabled, manual-hidden dan shutdown dicakup 16 kasus aktual |
| Petunjuk koreksi dan dua error paragraphs tidak seragam | Dua assertion RED typography reviewer | Shared help style/error builder, warna error tetap; current FormTypographySmoke exit 0 |
| Actual wrap/per-monitor DPI/reference bitmap belum lengkap | Coverage marquee | Sample bitmap dan lima forced runtime wraps lulus; hidden-text/reference ROI, seluruh tick dan layout DPI monitor nyata tetap belum diterima |

Source produk lifecycle cocok dengan hash GREEN tersimpan; harness bertambah guard/diagnostik dan sudah dijalankan ulang oleh parent. Snapshot post-fix berisi 12 file dengan SHA256 yang cocok. Scan statis terarah tidak menemukan pola secret/injection pada file/diff yang diperiksa; itu bukan audit keamanan penuh. Dua reviewer SPEC lulus terpisah: form dan lifecycle/guard. Reviewer kualitas/integrasi juga lulus tanpa temuan keamanan atau kesalahan logika. Saran nonblocking: timer/offset dapat diperiksa langsung di fixture startup yang sama; replay saat ini menguji penyelesaian sukses, bukan cabang exception. Batas ComboBox 940 DIP tetap catatan visual yang belum diterima pengguna, bukan bukti acceptance.

## Reproduksi pengembang: hanya suite terisolasi

Dari root source dengan .NET SDK/Rust/linker yang cocok:

```bash
cargo test --workspace --release --locked --offline --target x86_64-pc-windows-gnu
dotnet run --project tests/WidgetLayoutSmoke/WidgetLayoutSmoke.csproj --configuration Release
dotnet run --project tests/FormTypographySmoke/FormTypographySmoke.csproj --configuration Release
dotnet run --project tests/PrayerUiLogicSmoke/PrayerUiLogicSmoke.csproj --configuration Release
dotnet build src/PrayerAutomation.UI/PrayerAutomation.UI.csproj --configuration Release
dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -r win-x64 --self-contained true -o dist/PrayerAutomation.WPF-lockscreen-test
git diff --check
```

Periksa keselamatan harness sebelum menjalankannya bila source berubah. Jangan mengganti command dengan `dotnet run` project UI atau `SingleInstanceSmoke`. Build/publish harus setelah perubahan source; installer hanya setelah instruksi eksplisit pengguna.

## Verifikasi dokumentasi

Panduan dan diagram aktif diperiksa terpisah dari aplikasi. Review screenshot panjang menemukan URL Sources yang melebar akibat CSS terpotong. Check yang diperketat berhasil RED (exit `1`), CSS dipulihkan dari template lokal lengkap, lalu struktur dan browser lulus ulang (exit `0`).

- `9` dokumen, `60` tautan lokal, dan struktur `4` HTML diperiksa: tidak ada link/fragment/ID rusak pada versi guide ini.
- Sitasi panduan: strict + verbatim evidence lulus; ledger dokumentasi memakai ID yang sama, tanpa kandidat riset yang tidak dikutip.
- Browser Edge headless dengan profil scratch terpisah: `4` layout untuk dua diagram pada lebar `1280/520` px; tidak ada overlap kolom/teks atau URL yang melewati content box, navigasi fragment bekerja, tanpa dependency web.
- Screenshot first-fold terbaru diperiksa pada flow aplikasi 520 px dan flow lock-screen 1280 px: tidak tampak overlap atau clipping horizontal; peringatan tetap terbaca. Screenshot penuh lock-screen 520 px dari koreksi CSS terdahulu tetap bukti historis. Section lain diperiksa secara DOM, bukan review screenshot menyeluruh.

Ringkasan mesin: [document-verification.json](evidence/document-verification.json). Struktur/sitasi dan empat layout browser diperiksa ulang setelah pembaruan guide; evidence mengikat hasil ke hash dokumen dan membedakannya dari pemeriksaan lama. Ini bukti dokumentasi, bukan pengujian aplikasi utama atau native setter.

## Masih belum terverifikasi pada Windows nyata

1. Startup utama, single-instance dan profil asli — sengaja tidak dijalankan.
2. Gambar sementara benar-benar terlihat; restore Spotlight, Slideshow dan Picture+gambar asal.
3. Folder/options Slideshow tetap berfungsi, force-close/restart recovery dan override Windows yang serentak.
4. Kebijakan organisasi, PC bersih, monitor/font/per-monitor DPI pengguna. Deteksi 2.4.0 membaca cermin registry `PolicyManager\current\device`, GPO/CSP, dan kunci lock screen machine-wide, tetapi hanya menganggap terkelola bila **data value** benar-benar aktif (`NoChangingLockScreen=1`, path gambar non-kosong, dll.); flag `0` dan nilai default tidak memicu penolakan. Query WMI `root\cimv2\mdm\dmmap` belum ditambahkan karena memerlukan dependency baru, jangan menganggap semua sumber MDM/Intune tercakup.
5. Kesesuaian jadwal resmi sepanjang tahun dan bahwa durasi 60 menit sama dengan selesai sholat — **tidak diklaim**.
6. Acceptance pengguna dan build installer — **belum dilakukan**.

Gunakan [checklist UI/UX dan tiga mode](UI_ACCEPTANCE_ID.md) setelah menerima risiko profil. Diagram aktif [alur aplikasi](program_full_flowchart.html) dan [alur lock screen](lockscreen_restore_flow.html) merupakan ilustrasi source, bukan screenshot atau hasil setter.
