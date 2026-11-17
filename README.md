# Ayo Jama'ah!

[![Rilis terbaru](https://img.shields.io/github/v/release/asqal90/PrayerAutomation?label=rilis%20terbaru)](https://github.com/asqal90/PrayerAutomation/releases/latest)
[![Unduh](https://img.shields.io/badge/unduh-Windows%20x64-0078D6?logo=windows)](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah-win-x64.zip)
[![Situs](https://img.shields.io/badge/situs-asqal90.github.io%2FPrayerAutomation-0e9f6e)](https://asqal90.github.io/PrayerAutomation/)

Aplikasi jadwal sholat untuk Windows. Hitung jadwal pakai library Rust, tampilan dan integrasi Windows pakai WPF. Ada kalender Hijriyah, widget, dan pengingat teks. Tanpa suara atau adzan.

**Unduh:** [AyoJamaah-win-x64.zip](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah-win-x64.zip) (Windows 10 build 19041 ke atas dan Windows 11) · [x86](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah-win-x86.zip) · [Installer](https://github.com/asqal90/PrayerAutomation/releases/latest/download/AyoJamaah.Setup-2.1.2.exe).

## Status

Versi sekarang 2.1.2. Nama di layar "Ayo Jama'ah!", file-nya `AyoJamaah.exe`. Portable (x64/x86) dan installer sudah diunggah di [Releases](https://github.com/asqal90/PrayerAutomation/releases/latest). Belum ditandatangani, jadi SmartScreen akan memberi peringatan pertama kali jalan.

Yang berubah di 2.1.2: panel taskbar jadi popup saja, ada pratinjau gambar lock screen di Pengaturan, estimasi jadwal harian dipindah ke atas form lokasi, publish win-x86 ikut disediakan, plus perapihan kecil lain (koreksi Hijriyah diberi label, koreksi Masehi dihapus karena ikut tanggal komputer, koreksi Duha pakai tombol −/+). Installer pakai NSIS — bisa dibangun dari perangkat mana pun, payload dual x86+x64.

Baru diuji di Windows 11. Visual UI 2.1.2 belum diverifikasi di Windows.

## Fitur

- Jadwal sholat harian, beberapa metode, timezone, koreksi menit.
- Kalender Masehi dan Hijriyah. Tanggal Masehi ikut komputer; koreksi cuma untuk penanggalan Hijriyah.
- Tiga widget: Essential, Minimalist, Detail. Tema dan posisi bebas diatur.
- Panel taskbar berbentuk popup.
- Pengingat teks saja, tanpa suara atau adzan.
- Lokasi bisa manual, perkiraan dari jaringan, atau dari Windows Location Services.
- Cari nama tempat offline pakai data GeoNames.
- Startup Windows, auto-lock, dan gambar lock screen sementara.

Jadwal di aplikasi bukan pengumuman resmi. Satu metode belum tentu pas untuk semua daerah, dan kalibrasi satu tempat belum tentu berlaku untuk tempat lain.

## Sebelum dipakai

1. Jalankan sebagai user biasa, bukan Run as administrator.
2. Versi portable: jangan pisah EXE dari DLL dan folder pendampingnya.
3. Profil disimpan di `%APPDATA%\PrayerAutomation\config.json`. Portable dan installer pakai profil yang sama.
4. Di profil baru, startup Windows nyala secara default. Auto-lock dan gambar lock screen mati. Kalau gak mau jalan waktu login, matikan dari Pengaturan.
5. Coba dulu di satu PC, jangan langsung di PC kerja.

Installer memasang per-user ke `%LOCALAPPDATA%\Programs\PrayerAutomation`. Nama profil, namespace, dan mutex dibiarkan turunan supaya upgrade tidak menghapus pengaturan. Keluar dulu dari versi lama lewat menu Keluar.

## Cara pakai

1. Buka `AyoJamaah.exe`.
2. Masuk Pengaturan, cek lokasi, koordinat, timezone, dan metode. Preset lokasi bukan posisi Anda.
3. Atur pengingat, widget, dan startup. Untuk percobaan tampilan, biarkan auto-lock dan gambar lock screen mati.
4. Klik Simpan. Tema dan widget langsung berubah, tapi belum tersimpan sebelum Anda klik Simpan.
5. Kalau tutup jendela dengan perubahan belum disimpan, muncul pilihan Simpan, Buang perubahan, atau Kembali mengedit.
6. Tombol X dan minimize menyembunyikan jendela ke tray. Untuk benar-benar keluar, pakai menu Keluar.

## Lokasi dan jaringan

- Jadwal, kalender, timezone, dan pencarian nama tempat dihitung lokal.
- Tombol jaringan memanggil `ipapi.co` dan `ipwho.is`. IP publik Anda terlihat oleh penyedia itu; hasil bisa menunjuk gateway, proxy, atau VPN.
- Lokasi perangkat pakai Windows Location Services, tidak selalu GPS.
- Nominatim sudah tidak dipakai di build yang jalan; kodenya masih ada untuk perbandingan.

## Struktur repo

| Path | Isi |
|---|---|
| `src/PrayerAutomation.UI/` | Form WPF, tray, widget, pengaturan, adapter Windows |
| `src/Shared/` | Geometri ikon yang dipakai C# |
| `src/rust-core/` | Perhitungan jadwal dan kalender, diekspor sebagai `prayer_core.dll` |
| `installer/` | Installer NSIS per-user |
| `tests/` | Fixture dan smoke test |
| `docs/` | Panduan, audit, `prompts/`, dan `progress/` |
| `dist/` | Installer dan portable lokal, tidak ikut Git |

## Build

Butuh: .NET SDK untuk `net10.0-windows10.0.26100.0`, Rust edition 2024 dengan target `x86_64-pc-windows-gnu` (tambah `i686-pc-windows-gnu` untuk x86), dan linker MinGW. Restore pertama butuh jaringan.

```bash
cargo test --workspace --release --locked --target x86_64-pc-windows-gnu
dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -p:Arch=x64 --self-contained true -o ../AyoJamaah-build/portable-x64
dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -p:Arch=x86 --self-contained true -o ../AyoJamaah-build/portable-x86
```

Installer NSIS dibangun terpisah: buat receipt payload dengan `installer/make_payload_receipt.py`, lalu jalankan `python3 installer/build_installer_nsis.py` (lihat [panduan installer](installer/README_ID.md)).

## Pengujian

2.1.2 baru divalidasi statis: `cargo test`, `cargo fmt --check`, `cargo clippy -D warnings` lulus; source C#/XAML direview tanpa build WPF di mesin ini. Compile NSIS dan validasi payload jalan di perangkat ini. Install/upgrade/uninstall dan verifikasi visual tetap harus di Windows.

Belum tercakup: UI/UX normal 2.1.2, fokus dan modal sungguhan, Explorer, mixed-DPI, auto-hide, serta instalasi/upgrade/uninstall penuh. Pemulihan gambar lock screen, terutama untuk Spotlight, belum selesai sepenuhnya.

Jangan jalankan semua test sekaligus. `tests/SingleInstanceSmoke` menjalankan produk dengan profil nyata, dan test lain perlu ditinjau dulu.

## Lisensi

Source aplikasi pakai MIT, lihat [`LICENSE`](LICENSE). Copyright (c) 2026 Muhammad Nashiruddin Al-Asqalani.

Komponen pihak ketiga ikut lisensinya masing-masing: paket NuGet (GeoTimeZone, TimeZoneConverter), data GeoTimeZone/ODbL, dan GeoNames. Rincian ada di [inventaris third-party](docs/THIRD_PARTY_NOTICES.md).

## Dokumentasi

- [Indeks dokumentasi](docs/README.md)
- [Status distribusi](docs/DISTRIBUTION_ID.md)
- [Batas pemulihan lock screen](docs/LOCKSCREEN_LIMITATIONS_ID.md)
- [Asal source dan penghargaan](docs/SOURCE_ORIGINS_ID.md)
