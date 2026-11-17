# Installer Ayo Jama'ah! 2.1.2

> **Status 2.1.2:** source installer di repo memakai versi 2.1.2, halaman opsional "Mulai dari baru", penutupan paksa setelah konfirmasi, dan payload dual x86+x64. `AyoJamaahPayload-dual.iss` masih placeholder sampai digenerate dari receipt x86/x64 yang cocok; compile akan berhenti dengan pesan jelas. `AyoJamaahPayload.iss` hanya receipt historis 2.1.0. Catatan 2.1.0 di bawah tetap menjadi riwayat artefak yang pernah dibuat.

## Kandidat finalisasi branding 11 November 2026

- Produk aktif: `AyoJamaah.exe` dengan logo v13; paket `AyoJamaah.Portable-win-x64-2.1.0-20261111.zip`; installer `AyoJamaah.Setup-2.1.0.exe`. Artefak final berada di `<evidence>/2026-11-11-finalization/final/` setelah build terverifikasi.
- Instalasi memakai akun biasa, lokasi tetap `%LOCALAPPDATA%\Programs\PrayerAutomation`. Path tersebut identitas kompatibilitas, bukan nama tampilan. Profil, mutex, Run value dan AppId lama tetap; pengaturan tersimpan tidak di-reset.
- Gunakan **Keluar** dari aplikasi/tray lama dan tunggu pemulihan selesai sebelum memasang. Jika proses, jurnal recovery, izin, atau target belum dapat diverifikasi: **Batal**, bukan End task/admin/cleanup jurnal.
- Pilihan **Jalankan Ayo Jama'ah!** dan pintasan desktop tidak dicentang otomatis. Produk/installer tidak dijalankan oleh agent pada profil asli yang auto-lock-nya aktif.
- Startup mengenali `AyoJamaah.exe` serta `PrayerAutomation.UI.exe` lama pada Run value yang sama. Installer tidak menulis/menghapus pendaftaran startup. Saat pengguna menyimpan startup dari aplikasi baru, path mengikuti executable yang sedang digunakan.
- Seluruh file paket di-hash-pin eksplisit; installer tidak membuild ulang produk. Native `prayer_core.dll` dipertahankan byte-identik pada perubahan managed-only.
- Batas yang tetap: belum acceptance instalasi/upgrade/uninstall penuh, Windows lain/VM bersih, UI/UX/DPI/Explorer, atau pemulihan gambar lock-screen nyata. Installer belum ditandatangani; kebijakan SmartScreen dapat memberi peringatan.

## Riwayat 2.0.0 — hash dan acceptance bukan milik 2.1.0

UI/UX rilis ini dinyatakan **LOLOS oleh pengguna pada 30 Oktober 2026**. Installer membungkus portable yang sama; nomor assembly aplikasi tidak diubah.

> **Installer masih WIP dengan blocker keselamatan.** Pengguna melaporkan instalasi normal berhasil. Namun source review mengonfirmasi bahwa installer/uninstaller belum memeriksa jurnal pemulihan lock screen persisten ketika proses/mutex sudah hilang setelah crash/reboot. Pending restoration dapat luput dari guard. **Jangan menjalankan build ini atau uninstaller terkait sampai diperbaiki.** Ini bukan bukti bahwa instalasi atau lock screen pengguna saat ini bermasalah.
>
> Artefak yang dimaksud: SHA-256 `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886`. Bukti terfokus 25 PASS tidak mencakup kasus journal-only tersebut. Receipt: `<evidence>/2026-10-30-installer-release/installer-SPEC-blocker.json`. [Milestone dan pekerjaan tersisa](progress/MILESTONE_2026-10-30_ID.md).

Petunjuk berikut mendokumentasikan alur installer yang sudah dibuat; **bukan izin menjalankan build yang masih terblokir**.

## Kandidat pengaman baru — 2 November 2026

Source guard kini memeriksa state pemulihan walaupun proses/mutex hilang: jurnal biasa, quarantine invalid, interrupted writing, suffix tidak dikenal, direktori tidak terbaca atau reparse menolak perubahan. Hanya state yang terbukti tidak ada boleh lewat. Tidak ada parser, penghapusan jurnal, pemulihan gambar, atau bypass Keluar baru.

- Uji compiled Inno terisolasi: **29 skenario/266 PASS/0 FAIL**, baseline RED sembilan callback keliru mengizinkan jurnal tunggal tanpa proses.
- SPEC dan QUALITY review PASS; seluruh source pin cocok, serta parent membaca langsung manifest PE `asInvoker`/`uiAccess=false`.
- Kandidat terpisah: `<evidence>/2026-11-02-installer-journal-guard/artifact-final/PrayerAutomationSetup-2.0.0.exe`; SHA-256 `9c7758b256c2045fc82cf2f5ac94ecd24370ca920e691b1dc467a0906dfa58b7`.
- Payload tetap **485 berkas accepted lama**, belum memuat About/Credits baru. Installer/uninstaller lama tidak ikut berubah.
- Kandidat tidak dijalankan pada profil pengguna. Callback uninstall diuji dalam Setup-host fixture, **bukan host uninstaller native**. Lifecycle penuh clean-VM, upgrade, crash/reboot, sesi kedua dan aktor legacy non-kooperatif tetap belum diverifikasi. Mutex/pemeriksaan ulang bukan jaminan eksklusi filesystem atomik; tidak ada recheck per-deletion uninstall.

[Portable About, bukti dan pekerjaan tersisa](progress/PROGRESS_2026-11-02_ID.md). Jangan memakai hasil terfokus ini sebagai clearance fleet atau bukti refresh gambar lock-screen aktif.

## Lokasi dan penggunaan

Rilis disimpan di luar folder Git:

```text
../PrayerAutomation-releases/2.0.0/installer/PrayerAutomationSetup-2.0.0.exe
../PrayerAutomation-releases/2.0.0/portable/PrayerAutomation.UI.exe
```

1. Jalankan installer **sebagai pengguna biasa**, bukan `Run as administrator`.
2. Baca pemberitahuan instalasi/pengaturan lama. Lokasi pemasangan tetap `%LOCALAPPDATA%\Programs\PrayerAutomation`.
3. Jika aplikasi lama masih berjalan, installer meminta persetujuan. Pilih **Ya** hanya jika ingin keluar secara normal; kemudian pilih **Keluar** dari menu tray/aplikasi. **X/minimize hanya menyembunyikan jendela.**
4. Jika pemulihan lock screen belum selesai atau Keluar ditolak, tunggu atau **Batal**. Installer tidak memaksa penghentian dan tidak melewati pengaman pemulihan.
5. Setelah proses benar-benar selesai, pilih **Coba Lagi**. Tidak/Batal atau proses/berkas yang masih aktif menghalangi pemasangan.
6. Pilihan pintasan desktop dan **Jalankan aplikasi** awalnya kosong. Jangan memilih Jalankan bila belum siap menjalankan pengaturan startup/auto-lock tersimpan.

Installer ini tidak dijalankan oleh agent pada profil pengguna. Build dan uji keselamatan terisolasi terpisah dari penerimaan pemasangan nyata.

## Pengaturan dan upgrade

- `%APPDATA%\PrayerAutomation\config.json` dan data pemulihan tidak dihapus/reset/dimigrasikan. Kompatibilitas skema Python lama yang tidak dikenal tidak dijamin.
- Instalasi Python, foldernya, dan startup lama tidak dibersihkan otomatis. Jika lokasi tujuan masih berisi Python lama, pemasangan diblokir, bukan ditimpa.
- Identitas installer tetap `{8B76B6D5-1B3C-4E13-9A39-000000000001}`. Metadata uninstall per-user dengan ID sama bisa diperbarui setelah persetujuan. Jangan memakai uninstaller Python lama pada folder rilis baru.
- Startup tidak dibuat/dipindahkan oleh installer. Setelah pengguna sengaja menjalankan aplikasi baru, perilaku startup tersimpan tetap ditangani aplikasi. Folder `dist` yang masih dirujuk proses/Windows Startup dipertahankan sampai pengguna berpindah secara normal.
- Uninstall memerlukan batas keselamatan yang sama dan tidak menghapus profil/startup lama. Silent install/uninstall, force-close, restart otomatis dan pemasangan semua pengguna tidak didukung.

## Bukti dan batas

Portable yang dipaketkan berisi **485 berkas**. Setiap nama dan SHA-256 dipin saat compile; runtime tidak dibuild ulang untuk installer. Source build, PE manifest, hasil pemeriksaan terfokus, dan hash artefak tersimpan pada `<evidence>/2026-10-30-installer-release/child-installer/`.

Belum diuji sebagai pemasangan/upgrade/uninstall penuh pada VM bersih, pengguna/sesi kedua, atau penutupan aplikasi nyata. Installer **belum ditandatangani**; tidak ada jaminan lolos kebijakan SmartScreen. Tidak ada janji atomik terhadap legacy yang tidak bekerja sama dengan mutex; pemeriksaan ulang dan sharing Windows tetap fail-safe, bukan pengganti uji deployment.

[Detail build dan fixture keselamatan](../installer/README_ID.md) · [Panduan aplikasi](prompts/PROGRAM_DOCUMENTATION_ID.md) · [Catatan arsip Python](LEGACY_PYTHON_ID.md)
