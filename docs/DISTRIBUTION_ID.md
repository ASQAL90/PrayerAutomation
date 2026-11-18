# Distribusi lokal — Ayo Jama'ah!

> **Status 2.2.0:** portable 2.2.0 sudah dipublish di luar repo (x64 482 file, x86 481 file). Installer 2.2.0 sekarang dibangun dengan **NSIS** (`installer/build_installer_nsis.py`) sehingga compile dan validasi payload dapat dilakukan di perangkat ini; output default `../AyoJamaah-build/installer/AyoJamaah.Setup-2.2.0.exe`. Uji install/upgrade/uninstall dan verifikasi visual tetap harus di Windows. Artefak 2.1.0 di bawah adalah paket lokal terakhir yang sudah tersedia sebelumnya.

Paket di bawah disiapkan lokal untuk pemeriksaan pengguna. **Unsigned: signing belum selesai.** CompanyName/AppPublisher `Ayo Jama'ah!` hanya metadata, bukan publisher Authenticode terverifikasi. SmartScreen/Smart App Control masih dapat memperingatkan atau memblokir. Tidak ada pemasangan sertifikat self-signed, pengecualian Defender, atau perubahan trust Windows.

## Lokasi di root repository

- Installer: `dist/installer/AyoJamaah.Setup-2.1.0-unsigned.exe`.
- Portable ZIP: `dist/portable/AyoJamaah.Portable-win-x64-2.1.0-unsigned.zip`.
- Portable hasil ekstrak: `dist/portable/AyoJamaah.Portable-win-x64-2.1.0-unsigned/`.
- Checksum SHA256 masing-masing tersedia sebagai file `.sha256` di samping EXE/ZIP.

Folder `/dist/` diabaikan Git agar runtime dan installer tidak ikut tercampur ke source. Tidak ada commit, push, integrasi GitHub atau tautan unduhan publik dari pekerjaan ini. Paket terdahulu dipertahankan sebagai baseline dan tidak ditimpa.

## Isi dan batas

Runtime menggunakan build/publish metadata-publisher yang source dan checksum-nya diverifikasi ulang. Tidak ada perubahan kode jadwal, UI, recovery, native core atau identitas upgrade. File pendamping memuat full license/notice vendor, salinan lookup timezone ODbL yang sudah ada, serta penghargaan source/inspirasi lintas bahasa.

Installer dikompilasi dengan guard dan entrypoint lama yang byte-identical, memakai include payload baru yang mem-pin setiap file SHA256. Installer per-user (`PrivilegesRequired=lowest`), target legacy `%LOCALAPPDATA%\Programs\PrayerAutomation`, profil/mutex/AppId lama dipertahankan. Pilihan menjalankan aplikasi dan shortcut desktop tidak dicentang otomatis.

Pemeriksaan paket lulus: compile installer exit 0 tanpa warning; **762 file payload, 739 file runtime**; ZIP CRC/per-entry SHA256, manifest PE `asInvoker`/`uiAccess=false`, metadata/ikon dan **14 notice lama byte-identical** diverifikasi. Runtime tidak dibangun ulang karena 70 source/artwork/license build terkait masih cocok dengan build metadata-publisher yang dipakai. **Build bukan acceptance install/upgrade/uninstall, UI/Explorer/DPI, atau pemulihan lock screen nyata.** Aplikasi dan installer tidak dijalankan oleh agen untuk pemeriksaan paket.

## Sebelum digunakan

1. Ekstrak seluruh folder portable; jangan mengambil EXE saja.
2. Gunakan akun Windows biasa, bukan Run as administrator.
3. Profil lama `%APPDATA%\PrayerAutomation\config.json` tetap dibaca. Startup, auto-lock atau gambar lock screen pada profil lama dapat aktif. Portable tidak berarti profil terisolasi.
4. Untuk installer, keluar dari aplikasi lama secara normal dan tunggu pemulihan selesai. Jangan force-close, menghapus jurnal, atau memakai admin untuk melewati guard.
5. Uji satu PC non-operasional terlebih dahulu. Lisensi source utama/hak aset dan clearance distribusi keseluruhan masih terpisah dari pemeriksaan teknis paket.

Lihat [penghargaan source](SOURCE_ORIGINS_ID.md), [third-party notices](THIRD_PARTY_NOTICES.md), [metadata publisher](PUBLISHER_METADATA_ID.md) dan [batas pemulihan lock screen](LOCKSCREEN_LIMITATIONS_ID.md).
