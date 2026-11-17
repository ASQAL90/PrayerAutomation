# Pemasang Ayo Jama'ah! 2.1.2

Jalur utama sekarang **NSIS**. Tujuannya agar installer bisa dibangun langsung dari Termux/proot Debian tanpa menunggu Windows, sementara Inno Setup disimpan sebagai referensi legacy. Uji install/upgrade/uninstall dan verifikasi visual tetap wajib di Windows.

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

Output default: `../AyoJamaah-build/installer/AyoJamaah.Setup-2.1.2.exe`. Build root dapat diganti dengan `AYOJAMAAH_OUT`. Di Termux, `makensis` dijalankan lewat `proot-distro login debian`; di Windows, skrip memakai `makensis` di `PATH` atau `--makensis <path>`.

File yang terlibat:

| Path | Peran |
|---|---|
| `installer/build_installer_nsis.py` | Entry point: validasi receipt, regenerate include, compile |
| `installer/generate_nsis_payload.py` | Menulis section payload, manifest install, daftar hapus uninstall |
| `installer/nsis/AyoJamaah.nsi` | Script NSIS: halaman, lifecycle, pengaman, uninstaller |
| `installer/nsis/AyoJamaahPayload-dual.nsh` | Hasil generate; jangan diedit manual |

Receipt 2.1.2: 482 file x64, 481 file x86, 484 file unik, 9 file shared. Generator memvalidasi ulang hash receipt terhadap folder portable sebelum menulis include. Entry point build selalu menulis ulang include dari receipt; kalau hasilnya berubah, review diff `AyoJamaahPayload-dual.nsh` sebelum komit.

Payload tidak dibangun ulang oleh installer. Kalau portable berubah, buat receipt baru dengan `make_payload_receipt.py --force`, review, lalu build. Jangan mengganti file payload atau mengedit include secara manual.

## Perilaku installer

- Per-user, tanpa elevasi, lokasi tetap `%LOCALAPPDATA%\Programs\PrayerAutomation`.
- Halaman info, halaman opsional **Mulai dari baru**, halaman progres, dan halaman akhir.
- Pilihan Jalankan aplikasi dan pintasan desktop tidak dicentang.
- Payload x64/x86 dipilih otomatis dari arsitektur Windows.
- Manifest install di `%LOCALAPPDATA%\PrayerAutomation\install-manifest.txt` dipakai untuk membersihkan file stale dan untuk daftar hapus uninstall.
- Registry HKCU: uninstall key AppId lama dan `Software\AyoJamaah\InstallDir`.
- Uninstaller hanya menghapus union path payload, shortcut, manifest, dan key yang ditulis installer. `%APPDATA%\PrayerAutomation`, startup, dan data pengguna tidak disentuh.
- Tidak ada silent install/uninstall, `/ALLUSERS`, `/LOADINF`, restart otomatis, atau `/DIR` lain.

## Pengaman

Pengaman diport dari `installer/SafetyGuard.iss`:

- Target wajib tepat di lokasi per-user; ancestor junction/symlink/reparse ditolak.
- Folder instalasi Python lama di target diblokir, bukan ditimpa.
- Jurnal `LockScreen\lock-screen-restore.json` beserta variannya memblokir install/uninstall dan tidak dihapus installer.
- Proses produk dan interpreter yang image-nya di folder produk dideteksi lewat Toolhelp32 + session ID.
- Mutex global produk diperiksa, ditahan selama operasi, lalu boundary diperiksa ulang.
- File target diuji dengan `CreateFileW` read+write tanpa share.
- Penutupan paksa hanya lewat `TerminateProcess` setelah konfirmasi eksplisit.
- Boundary check diulang sebelum penulisan/penghapusan.

Perbedaan penting dari Inno: NSIS tidak membaca owner SID atau command line proses. Proses legacy yang dijalankan dari interpreter sistem di luar folder produk belum tentu terdeteksi. Preflight memeriksa file target sebelum penulisan, tanpa recheck per-file seperti `CheckPayloadWrite` Inno.

## Inno Setup legacy

File Inno tetap ada sebagai referensi dan riwayat:

- `installer/PrayerAutomation.iss` — installer Inno 2.1.2.
- `installer/SafetyGuard.iss` — sumber perilaku pengaman yang diport ke NSIS.
- `installer/AyoJamaahPayload-dual.iss` — pin Inno legacy yang masih valid.
- `installer/generate_accepted_payload.py` — generator pin Inno.

Jalur Inno tidak lagi dipakai untuk build utama. Jangan menganggap pin Inno sebagai bukti build NSIS; keduanya membaca receipt yang sama tetapi menghasilkan artefak berbeda.

### Riwayat build Inno

```bash
python installer/make_payload_receipt.py \
  --source ../AyoJamaah-build/portable-x86 --arch x86 \
  --output ../AyoJamaah-build/receipt-x86.json
python installer/make_payload_receipt.py \
  --source ../AyoJamaah-build/portable-x64 --arch x64 \
  --output ../AyoJamaah-build/receipt-x64.json
python installer/generate_accepted_payload.py \
  --receipt-x86 ../AyoJamaah-build/receipt-x86.json \
  --source-x86 ../AyoJamaah-build/portable-x86 \
  --receipt-x64 ../AyoJamaah-build/receipt-x64.json \
  --source-x64 ../AyoJamaah-build/portable-x64 \
  --output installer/AyoJamaahPayload-dual.iss
"$LOCALAPPDATA/Programs/Inno Setup 6/ISCC.exe" \
  '/DMyAppSourceDirX86=../AyoJamaah-build/portable-x86' \
  '/DMyAppSourceDirX64=../AyoJamaah-build/portable-x64' \
  '/O../dist/installer-output/journal-guard' \
  installer/PrayerAutomation.iss
```

`make_payload_receipt.py` membuat receipt eksplisit dari folder portable: SHA-256 setiap file, `file_count`, `ui_sha256`, dan arsitektur dari header PE `AyoJamaah.dll`. `generate_accepted_payload.py` menolak receipt yang tidak cocok persis dengan isi folder. Mode dual memakai `--receipt-x86/--source-x86` dan `--receipt-x64/--source-x64`.

## Bukti dan keterbatasan

Bukti Inno tersimpan di `<evidence>/2026-10-30-installer-release/child-installer/`:

- `red-old-installer.json`: skrip lama gagal empat batas keselamatan sebelum penggantian.
- `compile-final.log`: Inno Setup **6.7.3**, compile berhasil memakai source rilis durable.
- `pinned-payload-rejection.log`: fixture rusak ditolak compiler.
- `focused-verification.json`: **25 pemeriksaan terfokus PASS**.
- `release-receipt.json`: hash artefak/manifest PE/source pendukung.

Bukti pengaman jurnal 2 November 2026 di `<evidence>/2026-11-02-installer-journal-guard/`: **29 skenario/266 assertion PASS** pada compiled Inno fixture. Fixture tidak memasang produk, tidak menyentuh profil asli, dan tidak menjalankan host uninstaller native.

**Belum diverifikasi:** install/upgrade/uninstall NSIS penuh pada VM bersih, upgrade dari Inno 2.1.0, pengguna/sesi kedua, hasil sukses penutupan aplikasi nyata, verifikasi native lock-screen/Explorer, dan pemeriksaan visual halaman kustom NSIS. Installer belum ditandatangani; kebijakan SmartScreen perlu ditangani terpisah.

Mutex global yang tidak dapat dipastikan pemiliknya sengaja menghalangi operasi. Legacy yang diganti nama/entrypoint-nya dan tidak memakai penanda produk tidak selalu dapat dikenali. Tidak ada jaminan atomik lintas-proses tanpa kerja sama runtime lama.

## Uji yang harus dijalankan di Windows

1. Compile/install bersih pada profil sintetis tanpa elevasi.
2. Upgrade dari Inno 2.1.0: pastikan pengaturan, startup, dan jurnal tidak berubah; cek tidak ada file stale.
3. Uninstall: pastikan hanya payload/shortcut/manifest/key installer yang hilang.
4. Penolakan: aplikasi berjalan, mutex aktif, jurnal recovery tertunda, file terkunci, target junction.
5. Alur Coba Lagi/Batal dan penutupan paksa setelah konfirmasi.
6. Verifikasi visual semua halaman dan hasil shortcut.
