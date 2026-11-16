# Milestone UI/UX C# + Rust — 30 Oktober 2026

Milestone ini dicatat dalam Git pada 2 November 2026 sebagai **checkpoint pekerjaan yang sudah ada**, bukan pernyataan bahwa seluruh pekerjaan atau keselamatan installer telah selesai.

## Yang dipertahankan

- Source aplikasi aktif C# WPF + Rust, data GeoNames offline, pengujian, dan dokumentasi.
- UI/UX diterima pengguna pada 30 Oktober 2026: preview tema/jadwal sebelum Simpan, startup/minimize ke tray, dan Taskbar Window.
- Installer per-user/no-admin 2.0.0 membungkus portable yang diterima, tanpa mengganti metadata assembly atau membangun ulang accepted payload.
- Pengguna melaporkan telah memasang program melalui installer tanpa masalah pada alur normal. Laporan ini tidak menggantikan uji lifecycle pada VM bersih atau kasus recovery setelah crash/reboot.
- Penghapusan source/helper Python lama yang sudah diarsipkan: checkpoint mencatat 19 item dihapus dan 24 berkas backup SHA-256 terverifikasi. Tidak ada penghapusan baru pada commit milestone.

## Bukti dan batas verifikasi

Sebelum pencatatan Git, 106 berkas sumber/dokumentasi yang belum di-commit cocok SHA-256 dengan snapshot 30 Oktober. Pada langkah milestone hanya dokumentasi status installer diperjelas dan catatan ini ditambahkan; tidak ada perubahan runtime, build baru, peluncuran aplikasi, perubahan profil/Registry, atau pengujian native lock screen.

Receipt sumber, hasil compile/publish terdahulu, accepted portable, hasil tes terfokus, arsip cleanup, dan review disimpan di luar repo:

- `<evidence>/2026-10-30-installer-release/`
- `../../PrayerAutomation-releases/2.0.0/`
- `../../PrayerAutomation-archives/2026-10-30-cleanup/`

Commit tidak menyertakan output build/runtime, konfigurasi pengguna, kredensial, atau isi arsip tersebut. Tidak ada push atau history rewrite.

## Blocker installer yang masih terbuka

`installer/SafetyGuard.iss` memeriksa proses/mutex/berkas, tetapi belum membaca jurnal pemulihan lock screen persisten saat proses tidak lagi berjalan. Setelah crash/reboot, pemulihan bisa masih tertunda meskipun proses/mutex tidak ditemukan. Installer/uninstaller dapat tetap lanjut.

**Jangan menggunakan build installer tersebut atau uninstaller terkait sampai guard diperbaiki.** SHA-256 installer: `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886`. 25 pemeriksaan PASS sebelumnya tidak mencakup kondisi journal-only. Ini temuan source review, bukan bukti bahwa profil pengguna saat ini mengalami masalah.

Receipt: `<evidence>/2026-10-30-installer-release/installer-SPEC-blocker.json`.

## Hanya pekerjaan tersisa

1. **Pengaman installer:** tambahkan pemeriksaan jurnal persisten sebelum install/uninstall mutation; pending, malformed, atau unreadable harus fail-closed. Uji terarah terisolasi, compile installer, verifikasi payload/hash, serta review keselamatan. Jangan mengganti accepted runtime untuk memenuhi guard. Uji lifecycle penuh pada VM/profil sintetis masih belum dilakukan.
2. **Popup Tentang/Credits:** klik kanan systray → Tentang; identitas, kontak, slogan, dan ucapan terima kasih kepada mbak Luna/Sol sesuai permintaan pengguna. Brief lengkap tersimpan pada `<evidence>/2026-10-30-installer-release/ABOUT_REQUEST_ID.md`. Pertahankan tema dan perilaku aplikasi; compile/publish portable baru dan uji WPF terarah tanpa menjalankan profil nyata. Payload baru untuk About tidak boleh disebut identik dengan accepted payload lama.
3. **Cleanup output repo:** checkpoint mencatat 71 target tersisa. Verifikasi proses/Windows Startup terlebih dahulu; arsip dan verifikasi sebelum menghapus hanya output yang diizinkan. Jangan menyentuh portable/instalasi aktif, source, profil, atau bukti. Working tree Git yang clean tidak berarti seluruh output ignored sudah dibersihkan.

[README](../README.md) · [Status installer](../INSTALLER_ID.md) · [Arsip Python](../LEGACY_PYTHON_ID.md)
