# Installer Ayo Jama'ah! 2.1.2

> **Status 2.1.2:** jalur installer utama sekarang **NSIS** (`installer/nsis/AyoJamaah.nsi`) supaya paket bisa dibangun langsung dari perangkat ini tanpa Windows. Inno Setup tetap disimpan sebagai referensi legacy dan tidak lagi menjadi jalur build utama. Payload dual x86+x64 digenerate eksplisit dari receipt portable 2.1.2 (482 file x64, 481 file x86, 484 file unik, 9 file dipin sekali). Uji install/upgrade/uninstall dan verifikasi visual UI tetap wajib di Windows.

## Jalur build NSIS

Build divalidasi dan dikompilasi oleh satu entry point:

```bash
# dari root repo
python3 installer/build_installer_nsis.py

# hanya validasi receipt + regenerate include, tanpa compile
python3 installer/build_installer_nsis.py --check-only

# lewat wrapper Termux
ayojamaah-build installer
ayojamaah-build installer --check-only
```

Alur yang dijalankan:

1. `receipt-x64.json`/`receipt-x86.json` dibaca dan arsitekturnya dicocokkan.
2. Setiap file di receipt di-hash ulang terhadap `portable-x64`/`portable-x86`. Perbedaan satu byte menghentikan proses.
3. `installer/generate_nsis_payload.py` menulis ulang `installer/nsis/AyoJamaahPayload-dual.nsh`: section payload, manifest install, dan daftar hapus uninstall. Tidak ada wildcard.
4. `makensis` dijalankan. Di Termux ini memakai proot Debian (NSIS 3.11); di Windows memakai `makensis` native atau `--makensis <path>`.
5. Output default: `<AYOJAMAAH_OUT>/installer/AyoJamaah.Setup-2.1.2.exe` (default `AYOJAMAAH_OUT` adalah `../AyoJamaah-build`).

Payload tidak dibangun ulang oleh installer. Kalau payload sengaja dibangun ulang, receipt harus dibuat ulang dengan `--force` dan include NSIS direview sebelum build berikutnya. Jangan mengganti file payload secara manual.

## Isi paket

- Shared: 9 file yang byte-identik di kedua arsitektur, dipasang sekali.
- x64-only dan x86-only: dipilih saat `.onInit` dengan `RunningX64`.
- Manifest install ditulis ke `%LOCALAPPDATA%\PrayerAutomation\install-manifest.txt`; uninstaller memakai gabungan daftar path kedua arsitektur.
- Tidak ada file produk di repo dan tidak ada artefak build yang dipindahkan ke repo.

## Alur pemasangan

1. Halaman info: pemasangan per-user, pengaturan lama dipertahankan, dan pengingat untuk memilih **Keluar** dari aplikasi/tray. Pilihan pintasan desktop tidak dicentang.
2. Halaman **Mulai dari baru** hanya muncul jika `%APPDATA%\PrayerAutomation\config.json` ditemukan. Opsi ini menghapus folder pengaturan dan value Run `PrayerAutomation`; jurnal lock screen tidak disentuh.
3. Sebelum menulis, installer memeriksa target, proses produk, mutex, dan jurnal pemulihan lock screen. Jika ada masalah, pengguna dapat memilih penutupan normal lalu **Coba Lagi**, atau menutup paksa setelah konfirmasi terpisah.
4. Payload, shortcut Start Menu, uninstaller, dan registry HKCU ditulis. Uninstaller terdaftar dengan AppId lama `{8B76B6D5-1B3C-4E13-9A39-000000000001}`.
5. Pilihan **Jalankan aplikasi** tersedia di halaman akhir dan tidak dicentang.

Tidak ada silent install/uninstall, `/ALLUSERS`, impor opsi, restart otomatis, atau pemasangan ke direktori lain. Argumen `/S`, `/LOADINF`, `/CLOSEAPPLICATIONS`, `/FORCECLOSEAPPLICATIONS`, `/RESTARTAPPLICATIONS`, `/ALLUSERS`, `/NOCANCEL`, dan `/NCRC` ditolak.

## Pengaman dan batasannya

Perilaku yang dipertahankan dari `installer/SafetyGuard.iss`:

- Lokasi wajib `%LOCALAPPDATA%\Programs\PrayerAutomation`; ancestor junction/symlink/reparse dan folder instalasi Python lama ditolak.
- Jurnal `%LOCALAPPDATA%\PrayerAutomation\LockScreen\lock-screen-restore.json` (dan variannya) memblokir install/uninstall. Jurnal tidak dihapus atau diperbaiki oleh installer.
- Proses produk (`AyoJamaah.exe`, `PrayerAutomation.UI.exe`, `PrayerAutomation.exe`, `prayer_automation.exe`) dan interpreter `python.exe`/`pythonw.exe`/`dotnet.exe` yang image-nya berada di folder produk dideteksi.
- Mutex global `Global\PrayerAutomation.Desktop.SingleInstance` diperiksa, lalu installer menahan mutex itu selama tahap tulis/hapus dan memeriksa ulang boundary.
- Setiap file target diperiksa dengan `CreateFileW` read+write tanpa share; file yang terkunci, reparse point, atau tidak terbaca memblokir operasi.
- Sebelum menulis payload NSIS baru, entri manifest lama dihapus agar file stale tidak tertinggal. Untuk instalasi Inno lama tanpa manifest NSIS, `$INSTDIR` dibersihkan setelah boundary check.
- Hanya union path payload yang dihapus saat uninstall; `%APPDATA%\PrayerAutomation` dan data pengguna tidak disentuh.

Batasan yang berbeda dari Inno:

- Deteksi proses memakai nama image, image path, dan session ID. Tidak ada pembacaan owner SID atau command line seperti di Inno/WMI. Proses legacy yang dijalankan dari interpreter sistem di luar folder produk belum tentu terdeteksi.
- Mutex global yang tidak dapat dipastikan pemiliknya tetap memblokir operasi, termasuk kemungkinan dari sesi lain.
- Preflight memeriksa file target sebelum penulisan; tidak ada recheck per-file seperti `CheckPayloadWrite` Inno. Proteksi sharing Windows tetap berlaku saat file ditulis.
- Tidak ada IPC shutdown; installer tidak mengirim `WM_CLOSE`. Penutupan paksa hanya lewat `TerminateProcess` setelah konfirmasi eksplisit.

## Pengaturan dan upgrade

- `%APPDATA%\PrayerAutomation\config.json` dan data pemulihan tidak dihapus, di-reset, atau dimigrasikan. Skema Python lama yang tidak dikenal tidak dijamin kompatibel.
- Instalasi Python, folder, dan startup lama tidak dibersihkan otomatis. Jika folder tujuan masih berisi Python lama, pemasangan diblokir.
- Identitas installer tetap AppId lama. Metadata uninstall HKCU dengan ID sama dapat diperbarui setelah persetujuan; jangan menjalankan uninstaller Python lama pada folder versi baru.
- Startup tidak dibuat/dipindahkan oleh installer. Setelah pengguna menjalankan aplikasi, perilaku startup tersimpan ditangani aplikasi.
- Uninstall memakai boundary check yang sama dan tidak menghapus profil/startup lama.

## Yang masih harus diuji di Windows

- Compile final dan pemeriksaan ukuran/header `MZ` sudah bisa dilakukan di perangkat ini; eksekusi installer tidak.
- Install bersih, upgrade dari Inno 2.1.0, dan uninstall pada VM/profil sintetis.
- Penolakan saat aplikasi berjalan, saat jurnal recovery tertunda, saat file terkunci, dan saat target berupa junction.
- Alur Coba Lagi/Batal serta penutupan paksa setelah konfirmasi.
- Verifikasi visual halaman kustom, shortcut, entri registry, dan hasil uninstall.
- Pemeriksaan tanda tangan/SmartScreen: installer belum ditandatangani.

## Inno Setup legacy

`installer/PrayerAutomation.iss`, `installer/SafetyGuard.iss`, `installer/AyoJamaahPayload-dual.iss`, dan `installer/generate_accepted_payload.py` disimpan sebagai referensi perilaku dan riwayat. `AyoJamaahPayload-dual.iss` masih valid sebagai pin legacy, tetapi build utama tidak lagi memakainya. Dokumentasi Inno di bawah dipertahankan sebagai riwayat, bukan instruksi rilis aktif.

## Riwayat Inno 2.0.0

UI/UX rilis 2.0.0 dinyatakan **LOLOS oleh pengguna pada 30 Oktober 2026**. Installer Inno membungkus portable yang sama; nomor assembly aplikasi tidak diubah.

> **Installer Inno lama masih WIP.** Pengguna melaporkan instalasi normal berhasil. Source review menemukan installer/uninstaller lama belum memeriksa jurnal pemulihan lock screen persisten ketika proses/mutex sudah hilang setelah crash/reboot. Artefak SHA-256 `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886` dan uninstaller lamanya **tidak ikut diperbaiki**. Jangan gunakan sebagai bukti pengaman baru.

Kandidat pengaman Inno 2 November 2026: **29 skenario/266 PASS**, termasuk jurnal biasa, quarantine invalid, interrupted writing, suffix tidak dikenal, direktori tidak terbaca, dan reparse. Payload saat itu masih 485 file accepted lama. Kandidat terpisah: `<evidence>/2026-11-02-installer-journal-guard/artifact-final/PrayerAutomationSetup-2.0.0.exe` (SHA-256 `9c7758b256c2045fc82cf2f5ac94ecd24370ca920e691b1dc467a0906dfa58b7`). Callback uninstall diuji di Setup-host fixture, bukan host uninstaller native.

## Lokasi artefak lama

```text
../PrayerAutomation-releases/2.0.0/installer/PrayerAutomationSetup-2.0.0.exe
../PrayerAutomation-releases/2.0.0/portable/PrayerAutomation.UI.exe
```

Portable Inno yang dipaketkan berisi **485 berkas**. Setiap nama dan SHA-256 dipin saat compile; runtime tidak dibuild ulang. Bukti build, PE manifest, dan hash artefak tersimpan di `<evidence>/2026-10-30-installer-release/child-installer/`.

Belum ada klaim pemasangan/upgrade/uninstall penuh pada VM bersih, pengguna/sesi kedua, atau penutupan aplikasi nyata. Installer belum ditandatangani.

[Detail build dan payload](../installer/README_ID.md) · [Panduan aplikasi](prompts/PROGRAM_DOCUMENTATION_ID.md) · [Catatan arsip Python](LEGACY_PYTHON_ID.md)
