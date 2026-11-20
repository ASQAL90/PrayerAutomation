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
