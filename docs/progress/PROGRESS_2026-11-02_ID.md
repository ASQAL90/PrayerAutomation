# Kandidat 2 November 2026 — Tentang, pengaman jurnal dan deadline lock screen

## Finalisasi terbaru — source Taskbar GDI dan installer S1 lolos; integrasi berjalan

- **Spotlight: SKIP terbatas**, bukan fixed dan bukan klaim mustahil pada seluruh Windows. Parent memverifikasi 10 sumber/14 kutipan dan 10 hash source/bukti; perubahan `App.xaml.cs` setelah riset cocok dengan baseline dan patch Taskbar yang terpisah. [Panduan batas pemulihan](../LOCKSCREEN_LIMITATIONS_ID.md) lolos strict citation/evidence check. Penerimaan R2 pada 12.37 tetap FAIL visual; tidak ada eksperimen native baru.
- **Taskbar Window opsional — bukti sebelum fix GDI:** popup lama/default dan mode desktop dipertahankan. Kandidat clean-room native GDI child HWND mencatat lima RED→GREEN, 113 PASS, compile 0 warning/error, publish exit 0. Parent memverifikasi 55 hash source, enam path perubahan, tujuh file fixture, 485 file/member ZIP, seluruh hash/CRC, dan bahwa hanya UI DLL/PDB berbeda dari R2. SPEC independen PASS, parent memverifikasi receipt dan seluruh pin source/fixture pada 14.50.56; QUALITY masih berjalan. Explorer/pixels/mixed-DPI/auto-hide/restart/penerimaan normal belum diverifikasi.
- **Taskbar QUALITY-001 diperbaiki dan re-review PASS.** Temuan awal 15.00.48 tentang hasil GDI tidak diperiksa tetap diarsipkan. RED helper compiled aktual: tiga expected failure dari 38 pemeriksaan pada DC nol; GREEN canonical dan published masing-masing 38/38 dengan fixture/input identik. Enam raster normal byte-identik. Pemeriksaan allocation/selection/drawing dan cleanup partial acquisition ditambahkan hanya pada renderer; lima produksi lain dan tujuh fixture lama tetap identik, dua fixture baru ditambahkan. Parent 15.25.40 memverifikasi 57 pin, 15 receipt/diff, 485 file/ZIP hash/CRC, serta delapan fingerprint terlindungi. SPEC delta PASS diverifikasi 15.29.08; QUALITY PASS diverifikasi 15.37.39. Negative helper bukan uji runtime owner hide/status atau exhaustion; failed-cleanup injection dan Explorer asli tetap PENDING/NOT_RUN.
- **Installer — native RED→GREEN sebelum S1, historis.** Bukti lama tetap 9 skenario/43 assertion unik/39 PASS/4 FAIL; native RED baru mereproduksi copying setelah OK/exit 0. Boolean file Check dengan sticky denial/pembatalan yang didukung menghasilkan **10 skenario/51 PASS/0 FAIL**. Jurnal terlambat, file terkunci, cancellation refusal dan latch setelah jurnal sementara hilang berhenti dengan exit 5 tanpa sukses/penulisan payload berikutnya. Clean install/upgrade/native uninstall exit 0, pending preflight exit 7, pending uninstall dan `/NOCANCEL` exit 1. Parent membaca ulang lima source/baseline, 60 hash arsip, 12 receipt host, 485 payload lama, exact include render dan RT_MANIFEST asInvoker/uiAccess=false. SPEC lalu QUALITY/integrasi payload baru masih menunggu; bukan bukti atomic rollback atau bebas race final-check→write.
- **Installer S1 kini SPEC dan QUALITY PASS.** Temuan awal `/LOADINF` 14.54.49 tetap historis. Fresh native RED tujuh check menghasilkan dua expected FAIL: payload ditolak tetapi Finish sukses ditawarkan, exit 4; bukan copying-after-exception RED sebelumnya. Tambahan tujuh baris menolak seluruh prefix `/LOADINF` case-insensitive pada batas argumen, termasuk safe INF secara sengaja. GREEN 10 skenario/50 PASS dan boundary 13/13; empat pin lain serta sembilan callback tetap identik. Parent memverifikasi 175 hash arsip, 28 mapping, 24 receipt host, exact include/payload lama 485 dan actual asInvoker. SPEC PASS diverifikasi 15.17.13; QUALITY PASS diverifikasi 15.27.39. Integrasi payload Taskbar GdiFix menghasilkan kandidat baru compile exit 0, tanpa eksekusi. Parent 15.54.54: 36 hash arsip, tiga diff strict, installer 5/runtime 57 pin, 485 payload/member ZIP hash/CRC/check, [Code] byte-identik, actual asInvoker/uiAccess=false dan installer lama hash/size/mtime selama integrasi tetap. Sebelas added line dipindai tanpa temuan. SHA kandidat `1726f5c0996df305f7b86443879cef1ba1c8cde841cb697fb3c376ab4b3b920b`; review integrasi independen masih berjalan. Kandidat lama NONRELEASE, native uninstall tetap historis; full-payload/runtime acceptance, race/atomic rollback tidak diklaim.
- **Insiden output compile diungkap:** parameter macro output yang tidak dipakai membuat compile pertama menimpa installer lama. Rekonstruksi dari checkpoint immutable menghasilkan bytes SHA historis `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886`; parent memverifikasi installer yang dipulihkan dan salinan rekonstruksi identik. Mtime lama belum diverifikasi. Kandidat berikutnya dikompilasi dengan `/O` ke folder nonrelease yang terpisah; tidak ada production EXE yang dijalankan. Jangan menyatakan berkas installer lama tidak pernah berubah.
- **Cleanup final dihentikan sebelum penghapusan.** Parent 15.43.28 memverifikasi inventaris 1.214 file pada dua root, 1.142 eligible pada root GDI, 1.189 mapping dan 599 objek CAS (223.894.157 byte) di D:. Enam hardlink serta 66 file installer dengan field `scratch` receipt yang tidak tersedia tetap ineligible. Recheck 15.46.39 menemukan hash/ukuran/mtime config dan log asli berubah dari baseline GDI; penyebab belum diketahui. Probe disposition belum tercapai, nol file target dihapus. Profil tidak dipulihkan dan baseline tidak diganti untuk melewati guard. Bukti `parent-final-cleanup/PROTECTED-DRIFT-STOP.json`; semua scratch/CAS dipertahankan.
- Tidak ada produk asli/TrafficMonitor yang dijalankan atau ditutup oleh parent untuk finalisasi ini; tidak ada elevasi, image/power setter atau manipulasi sesi/Explorer. Perubahan config/log yang teramati belum diatribusikan, sehingga tidak diklaim seluruh profil tetap identik sampai akhir. Git commit final belum dilakukan.

Arsip finalisasi berada di `<evidence>/2026-11-02-finalization/`: `parent-taskbar-gdi-readback/RECEIPT.json`, `PARENT_TASKBAR_GDI_SPEC_READBACK.json`, `PARENT_TASKBAR_GDI_QUALITY_READBACK.json`, `parent-installer-s1-readback/RECEIPT.json`, `PARENT_INSTALLER_S1_SPEC_READBACK.json`, `PARENT_INSTALLER_S1_QUALITY_READBACK.json`, dan arsip insiden `installer-payload-fix/compile-output-path-incident.json`. Bagian di bawah mempertahankan hasil historis; technical GREEN tidak menghapus FAIL visual R2.

## Berubah

- Menu systray **Tentang** membuka popup Credits terpisah, tunggal dan mengikuti tema. Popup tanpa title bar Windows tambahan; Credits sesuai permintaan dan dapat digulir. Tautan email, LinkedIn dan GitHub hanya dipanggil dari klik pengguna.
- `TrayService.cs` mendapat tiga baris tambahan; controller/window baru tidak mengubah App lifecycle, core Rust, scheduler, startup, perilaku X/minimize/Keluar atau backend lock screen.
- Guard installer sekarang menolak state pemulihan persisten walaupun proses sudah hilang. Memeriksa ancestor, reparse, akses baca, seluruh enumerasi dan prefix jurnal; memeriksa ulang pada batas install/uninstall dan sebelum penulisan payload. Tidak menghapus/memperbaiki jurnal.

## Diverifikasi

| Bagian | Bukti terfokus | Batas |
|---|---|---|
| About | Prepatch RED feature missing, matching-harness RED→GREEN; 411 assertion actual-WPF, 8 tema × 2 ukuran | Bukan pemakaian produk/tray asli atau handler browser/email |
| Portable | Publish self-contained berhasil; 485 file lengkap dengan BACA_SAYA dan dua lisensi, ZIP CRC/set nama/setiap SHA verified | Pengguna menyatakan credit page sudah OK pada 2 November; bukan acceptance lock-screen |
| Guard installer | RED 9 callback keliru mengizinkan jurnal tunggal; GREEN 29 skenario/266 PASS/0 FAIL | Callback uninstall di Setup-host fixture, bukan uninstaller native |
| Review | SPEC dan QUALITY About/guard PASS, canonical/source snapshot 12 path tetap cocok | Bukan clearance semua lifecycle/fleet |
| Privilege/payload | Parent membaca RT_MANIFEST kedua EXE: asInvoker/uiAccess=false; 485 hash accepted payload dan hash installer lama unchanged | Tidak menjalankan kandidat pada profil pengguna |
| Deadline restore, investigasi awal | 18 assertion isolated workflow/scheduler/backend PASS | Tidak mengamati pixel Windows LockApp aktif |
| Kandidat deadline R2 | F1 dan F2 actual-C# RED sebelum patch masing-masing → GREEN; final 82 PASS, termasuk 76 assertion R1; compile/publish exit 0 | FakePower/FakeImageNative; tidak membuktikan native OS/pixels |
| Portable deadline R2 | Parent 485 hash folder/member ZIP, CRC, raw publish 482, runtime self-contained dan native accepted terverifikasi; x64 asInvoker/uiAccess=false | About/Rust/settings/startup tetap; tidak menjalankan produk |

## Portable About yang sudah diterima pengguna

Folder di luar Git: `../../PrayerAutomation-releases/2026-11-02/PrayerAutomation-Portable-About-Credits/`.

ZIP: `../../PrayerAutomation-releases/2026-11-02/PrayerAutomation-Portable-About-Credits.zip`.

SHA-256 ZIP: `ed732223733b86719e754468d32f0881627df0079569d87468ffdf272d38587a`.

1. Ekstrak seluruh ZIP ke folder baru; jangan menimpa versi berjalan.
2. Jika berganti versi, pilih **Keluar** sendiri dari systray versi lama. Jangan force-close bila pemulihan masih tertunda; X hanya menyembunyikan.
3. Jalankan EXE sebagai pengguna biasa setelah menerima bahwa konfigurasi akun yang sama tetap digunakan. Auto-lock/gambar tersimpan tidak dinonaktifkan agent.
4. Klik kanan ikon systray → **Tentang**. Periksa Credits, tema, tutup/buka ulang, serta tautan bila Anda memang ingin membukanya.

Pengguna sudah mencoba credit page melalui UI/UX normal dan menyatakan OK. Dragging, physical/per-monitor DPI dan handler tautan asli tidak diklaim diuji satu per satu. Tidak perlu mengaktifkan tindakan Windows untuk menguji About.

## Installer berbeda dari portable ini

Kandidat dengan guard baru memiliki SHA-256 `9c7758b256c2045fc82cf2f5ac94ecd24370ca920e691b1dc467a0906dfa58b7`, tetapi masih membungkus **payload accepted 30 Oktober**, belum memuat About. Installer lama SHA `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886` dan uninstaller lamanya tetap tidak diperbaiki/diblokir.

Lifecycle install/upgrade/uninstall lengkap di VM/profil sintetis, crash/reboot dan sesi kedua belum diverifikasi. Tidak ada jaminan eksklusi atomik terhadap legacy/noncooperative writers: directory bisa berubah setelah handle ditutup, enumerasi bukan snapshot, dan uninstall tidak memiliki recheck per-deletion. Jangan menyatakan cooperative mutex/rechecks sebagai operasi atomik.

## Kandidat deadline R2 — gambar lock-screen aktif belum diterima

### Uji pengguna 2 November — FAIL visual, deadline/config restoration tercatat

Pengguna mengoreksi waktu kembali menjadi **12.32**, unlock→Win+L **12.34**, dan pengamatan sesudah deadline **12.37**. Gambar pada 12.37 tetap "sedang sholat" sampai unlock→lock berikutnya; **R2 tidak lolos acceptance pemulihan tampilan aktif**.

Log transaksi mencatat trigger 11:35:55.792 (jadwal 11.36), deadline **12:35:55.792**, restore mulai 12:35:57.050, flags Spotlight 0→1 pada 12:35:57.083 dan result `Restored` pada 12:35:57.088. Selisih trigger/deadline **60 menit**; result sekitar **1,30 detik sesudah deadline**. Pengamatan 12.32/12.34 berada sebelum deadline ini, tetapi 12.37 sesudahnya tetap gagal. Konfigurasi saat snapshot 13.00 sudah menunjukkan durasi 10 menit; bukan bukti nilai konfigurasi saat trigger. Agent tidak mengubahnya.

`verification=config-only visual=unverified` pada log penting: `WindowsLockScreenImageBackend.cs:179–217` memulihkan Spotlight melalui flags dan `IsRestored()` menerima kesamaan flags; tidak memverifikasi pixels aktif. Jadi jalur deadline/flag telah berjalan pada transaksi tersebut sebelum unlock 12.37, bukan bukti tampilan telah pulih. Mekanisme native refresh Spotlight standard-user saat locked masih belum terverifikasi; tidak ada klaim cache rusak, Windows selalu tidak bisa refresh, atau bahwa memperpanjang lease akan menyelesaikannya.

Snapshot 13.00:23 menemukan proses dari folder R2 dengan adjacent UI DLL hash cocok; ini identitas path/disk saat snapshot, bukan identitas proses historis/loaded bytes. Jurnal aktif saat snapshot milik deadline 13:01:55 dengan trigger baru sesudah transaksi pertama; tidak dianggap jurnal tertinggal transaksi 11.36. Evidence tersaring dan timeline terhitung: `<evidence>/2026-11-02-lockscreen-deadline-fix/r2/user-failure/{USER_FAILURE_ID.md,diagnostics-130023.json,FIRST_TEST_TIMELINE.json}`. Tidak ada source patch, build/test baru, app launch/close, native power/image setter, perubahan profil/Registry/sesi/power plan, installer atau cleanup oleh agent. Technical PASS historis tetap valid tetapi bukan acceptance visual.

### Riwayat kandidat sebelum uji pengguna

Laporan pengguna: gambar "sedang sholat" tetap tampil sesudah durasi, baru berganti setelah unlock→lock. Pemeriksaan read-only menunjukkan mode Spotlight; URI API masih temporary ketika jurnal tidak ada. Source memulihkan Spotlight/Slideshow lewat flag mode, sedangkan verifikasi tidak mengukur pixel yang sedang dirender LockApp. Bukti mengarah pada perbedaan pemulihan konfigurasi dan penyegaran tampilan, **belum menjadi sebab native yang terkonfirmasi**.

Source kandidat kini memakai optional scoped `ExecutionRequired` sebelum transaksi gambar sementara, tanpa perubahan power plan, `DisplayRequired`, force-kill LockApp, auto-unlock/relock, admin/CSP atau desktop-wallpaper fallback. Ini mitigasi eksekusi deadline, bukan bukti bahwa gambar aktif sudah kembali. Bukti lanjut harus membedakan deadline aktual, perubahan mode/URI, dan gambar yang tampak. Jangan menyatakan Windows selalu tidak bisa refresh atau bahwa timer native sudah terbukti berjalan tepat pada kejadian pengguna.

Kandidat pertama **R1 diblokir**: review menemukan lease yang diperpanjang sebelum extension jurnal terverifikasi (F1), serta reacquire setelah watchdog expired pada UTC mundur/deadline lebih tinggi (F2). GREEN 76 R1 tidak mencakup dua kasus ini. Arsip R1 tidak ditimpa atau dihapus.

Fix cycle 1 hanya mengubah dua file produksi `LockScreenImageScheduler.cs`/`LockScreenExecutionRequest.cs` dan tiga fixture. Lease aktif sekarang tidak mengikuti deadline prospective saat `PendingVerification`; expiry dilatch sampai scenario berakhir lewat `Clear`. Initial request sebelum asynchronous apply, memoized failure, satu Set/live handle, timer relative/generation/disposal tetap dipertahankan. Grace 30 detik tidak mengubah durasi gambar dalam jurnal.

- Parent readback 11:23:59 waktu komputer lokal: 126 canonical hashes, 5 owned R2 dan 121 nonowned unchanged, snapshot/diff/raw-log/harness/input pins, build/publish, protected native/arsip dan 76 evidence R1 cocok **sebelum pembaruan dokumentasi parent ini**.
- Re-review SPEC dan QUALITY/security/logic F1/F2 **PASS**; parent membaca receipt asli dan mencocokkan lima hash source serta diff. Ini review sempit pada patch, bukan clearance OS/UI. Tidak ada suite About/installer penuh atau uji OS yang dijalankan ulang.
- Folder: `../../PrayerAutomation-releases/2026-11-02/PrayerAutomation-Portable-LockScreen-Deadline-R2/`.
- ZIP: `../../PrayerAutomation-releases/2026-11-02/PrayerAutomation-Portable-LockScreen-Deadline-R2.zip`.
- SHA-256 ZIP: `12c981077e86e813da15de5b5da14889b41a68cb9d9b2ffb62bfb71f423bd55c`.
- Bukti: `<evidence>/2026-11-02-lockscreen-deadline-fix/r2/`, terutama `parent-readback-final/PARENT_R2_DELIVERY_READBACK.json`.

Setelah handoff review selesai, pengguna dapat mencoba seluruh ZIP di folder baru sebagai pengguna biasa. Pilih **Keluar** sendiri di systray versi lama sebelum berganti; bila pemulihan tertunda/ditolak, jangan force-close atau menghapus jurnal. Config akun yang sama dan auto-lock tersimpan tetap berlaku. Untuk acceptance, catat saat gambar muncul, batas durasi, dan gambar yang terlihat setelah deadline ketika sesi **masih terkunci**; unlock→lock saja bukan PASS pemulihan tampilan aktif. Agent tidak menjalankan app/native power/image ataupun mengubah profil/sesi.

## Pekerjaan tersisa dan catatan review

- Installer: native/lifecycle dan penerimaan operator; installer About baru menunggu acceptance payload baru, bukan mengganti pin lama diam-diam.
- Lock-screen aktif: uji manual R2 **FAIL visual**; deadline dan flags pada transaksi 11.36 tercatat dipulihkan sekitar 12.35.57, tetapi gambar pada 12.37 baru berubah setelah unlock→lock. Tersisa investigasi pemulihan Spotlight aktif serta dukungan OS/DC/manual-sleep; tidak ada kandidat baru atau gate regression berulang otomatis.
- Cleanup: scope 71 root sudah selesai diperiksa; 68 root terhapus, 3 dipertahankan karena hardlink/salinan source dengan provenance tidak cukup. Parent memverifikasi 22.015 file/9.597.393.785 logical bytes, 68 manifest dan seluruh 1.027 objek CAS; 58 file retained stat-unchanged. Source/runtime/profil terlindungi bukan target. Bukti: `<evidence>/2026-11-02-repo-residual/PARENT_READBACK_VERIFIED.json` dan `PARENT_PROTECTED_READBACK.json`. Tidak ada cleanup lanjutan otomatis.
- Saran review non-blocking: tambah cakupan minimize/Close-idempotent/status-link small window; perbaiki loss karakter non-ASCII pada diagnostik nama jurnal Inno (prefix ASCII tetap diblokir); tambah fixture cabang error handle/enumerasi. Saran ini tidak menambah gate regression berulang atau mengubah runtime saat ini.

Bukti durable: `<evidence>/2026-11-02-todo-continuation/`, `<evidence>/2026-11-02-about/`, `<evidence>/2026-11-02-installer-journal-guard/`, dan `<evidence>/2026-11-02-lockscreen-visible/`. Source perubahan belum staged/committed; milestone tetap `5460f9cfa83fd4af76b8bbbbe4d4d4a26f6ca467`. Tidak ada push.
