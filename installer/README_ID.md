# Pemasang Ayo Jama'ah! 2.2.0

Installer dibangun dengan **NSIS** saja. Tujuannya agar installer bisa dibangun langsung dari Termux/proot Debian tanpa menunggu Windows. Uji install/upgrade/uninstall dan verifikasi visual tetap wajib di Windows.

## Build NSIS

```bash
# validasi receipt + regenerate include (tanpa compile)
python3 installer/build_installer_nsis.py --check-only

# compile installer dual-arch
python3 installer/build_installer_nsis.py

# lewat wrapper Termux
ayojamaah-build installer
ayojamaah-build installer --check-only
```

Output default: `../AyoJamaah-build/installer/AyoJamaah.Setup-2.2.0.exe`. Build root dapat diganti dengan `AYOJAMAAH_OUT`. Di Termux, `makensis` dijalankan lewat `proot-distro login debian`; di Windows, skrip memakai `makensis` di `PATH` atau `--makensis <path>`.

File yang terlibat:

| Path | Peran |
|---|---|
| `installer/build_installer_nsis.py` | Entry point: validasi receipt, regenerate include, compile |
| `installer/generate_nsis_payload.py` | Menulis section payload, manifest install, dan daftar hapus stale |
| `installer/payload_receipt.py` | Validasi receipt yang dipakai bersama generator dan wrapper |
| `installer/make_payload_receipt.py` | Membuat receipt dari portable hasil publish |
| `installer/nsis/AyoJamaah.nsi` | Script NSIS: halaman, lifecycle, pengaman, uninstaller |
| `installer/nsis/AyoJamaahPayload-dual.nsh` | Hasil generate; jangan diedit manual |
| `installer/nsis/AyoJamaahPayloadFiles-x64.txt` / `-x86.txt` | Hasil generate; daftar payload per arsitektur untuk pembersihan upgrade |
| `installer/nsis/wizard-banner.bmp` | Banner sisi kiri halaman Selamat datang/Selesai |

Receipt 2.2.0: 482 file x64, 481 file x86, 484 file unik, 9 file shared. Generator memvalidasi ulang hash receipt terhadap folder portable sebelum menulis include. Entry point build selalu menulis ulang include dari receipt; kalau hasilnya berubah, review diff `AyoJamaahPayload-dual.nsh` sebelum komit.

Payload tidak dibangun ulang oleh installer. Kalau portable berubah, buat receipt baru dengan `make_payload_receipt.py --force`, review, lalu build. Jangan mengganti file payload atau mengedit include secara manual.

## Perilaku installer

- Per-user; bisa dijalankan tanpa atau dengan elevasi (mis. komputer satu akun administrator), tetap dipasang ke `%LOCALAPPDATA%\Programs\PrayerAutomation`.
- Halaman info, halaman opsional **Mulai dari baru**, halaman progres, dan halaman akhir.
- Pilihan Jalankan aplikasi dan pintasan desktop tidak dicentang.
- Payload x64/x86 dipilih otomatis dari arsitektur Windows.
- Manifest install di `%LOCALAPPDATA%\PrayerAutomation\install-manifest.txt` dipakai untuk membersihkan file stale dan untuk daftar hapus uninstall.
- Registry HKCU: uninstall key AppId lama dan `Software\AyoJamaah\InstallDir`.
- Uninstaller hanya menghapus union path payload, shortcut, manifest, dan key yang ditulis installer. `%APPDATA%\PrayerAutomation`, startup, dan data pengguna tidak disentuh.
- Tidak ada silent install/uninstall, `/ALLUSERS`, `/LOADINF`, restart otomatis, atau `/DIR` lain.

## Pengaman

- Target wajib tepat di lokasi per-user; ancestor junction/symlink/reparse ditolak.
- Folder instalasi Python lama di target diblokir, bukan ditimpa.
- Jurnal `LockScreen\lock-screen-restore.json` beserta variannya memblokir install/uninstall dan tidak dihapus installer. Pesan galat mengarahkan pengguna ke **Pulihkan lock screen sekarang** dan **Batalkan pemulihan tertunda** di pengaturan aplikasi.
- Proses produk dan interpreter yang image-nya di folder produk dideteksi lewat Toolhelp32 + session ID.
- Mutex global produk diperiksa, ditahan selama operasi, lalu boundary diperiksa ulang.
- File target diuji dengan `CreateFileW` read+write tanpa share.
- Penutupan paksa hanya lewat `TerminateProcess` setelah konfirmasi eksplisit.
- Boundary check diulang sebelum penulisan/penghapusan.
- Pembersihan file stale memakai `DeleteFileW` dengan percobaan ulang 4× (250 ms jeda) dan melaporkan kode galat Windows. Berkas yang juga ada di payload baru tidak dihapus, melainkan ditimpa.

NSIS tidak membaca owner SID atau command line proses. Proses legacy yang dijalankan dari interpreter sistem di luar folder produk belum tentu terdeteksi. Preflight memeriksa file target sebelum penulisan, tanpa recheck per-file.

**Belum diverifikasi:** install/upgrade/uninstall penuh pada VM bersih, upgrade dari 2.1.0, pengguna/sesi kedua, hasil sukses penutupan aplikasi nyata, verifikasi native lock-screen/Explorer, dan pemeriksaan visual halaman kustom. Installer belum ditandatangani; kebijakan SmartScreen perlu ditangani terpisah.

Mutex global yang tidak dapat dipastikan pemiliknya sengaja menghalangi operasi. Legacy yang diganti nama/entrypoint-nya dan tidak memakai penanda produk tidak selalu dapat dikenali. Tidak ada jaminan atomik lintas-proses tanpa kerja sama runtime lama.

## Uji yang harus dijalankan di Windows

1. Install bersih pada profil sintetis tanpa elevasi.
2. Upgrade dari 2.1.0: pastikan pengaturan, startup, dan jurnal tidak berubah; cek tidak ada file stale.
3. Uninstall: pastikan hanya payload/shortcut/manifest/key installer yang hilang.
4. Penolakan: aplikasi berjalan, mutex aktif, jurnal recovery tertunda, file terkunci, target junction.
5. Alur Coba Lagi/Batal dan penutupan paksa setelah konfirmasi.
6. Verifikasi visual semua halaman dan hasil shortcut.
