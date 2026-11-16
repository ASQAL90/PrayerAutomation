# Ayo Jama'ah!

[![Rilis terbaru](https://img.shields.io/github/v/release/asqal90/PrayerAutomation?label=rilis%20terbaru)](https://github.com/asqal90/PrayerAutomation/releases/latest)
[![Unduh](https://img.shields.io/badge/unduh-Windows%20x64-0078D6?logo=windows)](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah-win-x64.zip)
[![Situs](https://img.shields.io/badge/situs-asqal90.github.io%2FPrayerAutomation-0e9f6e)](https://asqal90.github.io/PrayerAutomation/)

Aplikasi jadwal sholat untuk Windows. Perhitungan jadwal ada di library Rust, tampilan dan integrasi Windows pakai C# WPF. Ada kalender Hijriyah, widget di desktop, dan pengingat teks tanpa suara.

**Unduh:** [AyoJamaah-win-x64.zip](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah-win-x64.zip) untuk Windows 10 (build 19041 ke atas) dan Windows 11, x64.

## Status 2.1.0

Nama di layar adalah Ayo Jama'ah!, file executable-nya `AyoJamaah.exe`. Portable dan installer yang saya simpan di `dist/` belum diunggah ke halaman Releases. Keduanya belum ditandatangani, jadi SmartScreen akan memberi peringatan saat pertama dijalankan. Nama publisher di metadata file juga belum terverifikasi.

Sejauh ini baru diuji di Windows 11. Versi Windows lain dan konfigurasi taskbar yang berbeda belum dicoba semua.

## Fitur

- Jadwal sholat harian, beberapa metode perhitungan, pilihan timezone, dan koreksi menit.
- Kalender Masehi dan Hijriyah, termasuk koreksi tampilan tanggal.
- Tiga widget: Essential, Minimalist, dan Detail. Tema dan posisinya bisa diatur.
- Panel taskbar dengan dua mode: popup terpisah, atau backend WinUI yang masih eksperimental.
- Pengingat berupa teks saja, tanpa suara atau adzan.
- Lokasi bisa diisi manual, diperkirakan dari jaringan, atau diambil dari Windows Location Services.
- Pencarian nama tempat offline memakai data GeoNames, tanpa request jaringan.
- Pilihan startup Windows, auto-lock sesi, dan gambar lock screen sementara.

Jadwal yang muncul adalah hasil hitung aplikasi, bukan pengumuman resmi. Satu metode belum tentu cocok untuk semua daerah, dan kalibrasi lokal tidak otomatis berlaku di tempat lain.

## Sebelum dipakai

1. Jalankan dengan akun Windows biasa, bukan Run as administrator.
2. Untuk versi portable, jangan pisahkan EXE dari DLL dan folder pendampingnya. Ini bukan aplikasi single-file.
3. Profil disimpan di `%APPDATA%\PrayerAutomation\config.json` dan dipakai bersama oleh portable maupun installer. Memindahkan folder aplikasi tidak membuat profil baru.
4. Di profil baru, startup Windows aktif secara default, sedangkan auto-lock dan gambar lock screen mati. Kalau tidak mau aplikasi jalan saat login, matikan lewat Pengaturan.
5. Coba dulu di satu PC, jangan langsung di PC kerja.

Installer memasang per-user ke `%LOCALAPPDATA%\Programs\PrayerAutomation`. Nama profil, namespace, mutex, dan AppId lama sengaja tidak diubah supaya upgrade tidak menghapus pengaturan. Installer juga tidak mereset profil atau membersihkan instalasi lama. Keluar dulu dari versi lama lewat menu Keluar, lalu tunggu proses pemulihan selesai.

## Cara pakai

1. Buka `AyoJamaah.exe`.
2. Masuk ke Pengaturan dan cek lokasi, koordinat, timezone, serta metode. Preset lokasi bukan posisi Anda.
3. Atur pengingat, widget, dan startup. Untuk percobaan tampilan, biarkan auto-lock dan gambar lock screen mati.
4. Klik Simpan pengaturan. Perubahan tema dan widget langsung terlihat, tapi belum tersimpan sebelum Anda klik Simpan.
5. Kalau menutup jendela dengan perubahan yang belum disimpan, muncul pilihan Simpan, Buang perubahan, atau Kembali mengedit.
6. Tombol X dan minimize menyembunyikan jendela ke tray. Untuk benar-benar keluar, pakai menu Keluar.

Mode taskbar WinUI belum tentu dapat ruang di semua konfigurasi Explorer dan DPI. Kalau bermasalah, ganti ke mode popup.

## Lokasi dan jaringan

- Jadwal, kalender, konversi timezone, dan pencarian nama tempat dihitung lokal.
- Tombol perkiraan jaringan memanggil `ipapi.co` dan `ipwho.is`. IP publik Anda terlihat oleh penyedia itu, dan hasilnya bisa menunjuk gateway, proxy, atau VPN.
- Lokasi perangkat memakai Windows Location Services, bukan selalu GPS.
- Nominatim sudah tidak dipakai di build aktif. Kodenya masih ada untuk perbandingan.

## Struktur repo

| Path | Isi |
|---|---|
| `src/PrayerAutomation.UI/` | Form WPF, tray, widget, pengaturan, adapter Windows |
| `src/PrayerAutomation.Taskbar.WinUI/` | Backend XAML Island untuk panel taskbar |
| `src/Shared/` | Geometri ikon yang dipakai dua project C# |
| `src/rust-core/` | Perhitungan jadwal dan kalender, diekspor sebagai `prayer_core.dll` |
| `installer/` | Skrip Inno Setup per-user |
| `tests/` | Fixture dan smoke test, cakupannya berbeda-beda |
| `docs/` | Panduan, audit, atribusi, `prompts/`, dan `progress/` |
| `dist/` | Installer dan portable lokal, tidak ikut Git |

## Build

Yang dibutuhkan: .NET SDK yang mendukung `net10.0-windows10.0.26100.0`, Rust dengan edition 2024 dan target `x86_64-pc-windows-gnu` beserta linker MinGW x64, dan paket NuGet sesuai `PackageReference`. Restore pertama butuh jaringan.

```bash
cargo test --workspace --release --locked --target x86_64-pc-windows-gnu
dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -r win-x64 --self-contained true -o ../AyoJamaah-build/portable
```

MSBuild membangun Rust sebelum UI. Output publish sengaja ditaruh di luar repo. Installer butuh receipt payload dan hash pin yang cocok, jadi bukan sekadar hasil publish. Beberapa path default di installer dan runner test masih khusus komputer saya.

## Pengujian

Pada 11 November 2026 saya cek build dan publish. Fixture branding lulus 57/57 dan fixture perubahan belum disimpan lulus 99/99, keduanya terhadap DLL finalisasi yang sama. Setelah metadata publisher diubah, build dan publish lulus lagi, tapi fixture UI tadi tidak diulang ke DLL baru.

Yang belum tercakup: UI/UX normal 2.1.0, fokus dan modal sungguhan, Explorer, mixed-DPI, auto-hide, serta instalasi, upgrade, dan uninstall penuh.

Pemulihan gambar lock screen, terutama untuk Spotlight, belum selesai sepenuhnya. Lulus build dan test tidak berarti gambar kembali normal secara visual.

Jangan jalankan semua test sekaligus. `tests/SingleInstanceSmoke` menjalankan produk dengan profil nyata, dan test lain perlu ditinjau dulu.

## Lisensi

Source aplikasi memakai MIT, lihat [`LICENSE`](LICENSE). Copyright (c) 2026 Muhammad Nashiruddin Al-Asqalani.

Komponen pihak ketiga tetap ikut lisensinya masing-masing: NuGet, Windows App SDK, data GeoTimeZone/ODbL, dan GeoNames. Rinciannya ada di [inventaris third-party](docs/THIRD_PARTY_NOTICES.md). Dokumen itu bukan clearance hukum untuk seluruh paket rilis.

## Dokumentasi

- [Indeks dokumentasi](docs/README.md)
- [Status distribusi](docs/DISTRIBUTION_ID.md)
- [Batas pemulihan lock screen](docs/LOCKSCREEN_LIMITATIONS_ID.md)
- [Asal source dan adaptasi](docs/SOURCE_ORIGINS_ID.md)
