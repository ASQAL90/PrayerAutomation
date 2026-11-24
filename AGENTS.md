# AGENTS.md — Ayo Jama'ah (PrayerAutomation)

Aplikasi jadwal sholat untuk Windows. Perhitungan di library Rust, UI C# WPF, taskbar
WinUI, dan installer NSIS. MIT licensed. Nama produk di layar: **Ayo Jama'ah!**,
executable: `AyoJamaah.exe`.

## Arsitektur

| Path | Isi |
| --- | --- |
| `src/rust-core/` | Library Rust: `prayer.rs` (jadwal), `calendar.rs` (Hijriyah/Masehi), `lib.rs` |
| `src/PrayerAutomation.UI/` | WPF `net10.0-windows10.0.26100.0`, `WinExe`, nullable enabled |
| `src/PrayerAutomation.Taskbar.WinUI/` | Panel taskbar WinUI |
| `src/Shared/` | Kode yang dipakai UI dan taskbar |
| `tests/*Smoke/` | Smoke test per area (About, Calendar, FormTypography, LocationDetection, LockScreen*, OfflineGeocoder, PrayerSchedule, PrayerUiLogic, SingleInstance, UnsavedChanges, WidgetLayout) |
| `installer/` | NSIS (`nsis/AyoJamaah.nsi`), generator payload, receipt, Inno legacy |
| `docs/` | Keputusan dan batasan distribusi, publisher, lock screen, lisensi |
| `site/` | Situs GitHub Pages |

## Build dan verifikasi

Toolchain yang dibutuhkan: .NET SDK untuk `net10.0-windows10.0.26100.0`, Rust edition 2024
dengan target `x86_64-pc-windows-gnu` (dan `i686-pc-windows-gnu` untuk x86) plus linker
MinGW. Restore pertama butuh jaringan.

```bash
cargo test --workspace --release --locked --target x86_64-pc-windows-gnu
cargo fmt --check
cargo clippy -D warnings

dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release \
  -p:Arch=x64 --self-contained true -o ../AyoJamaah-build/portable-x64
dotnet publish src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release \
  -p:Arch=x86 --self-contained true -o ../AyoJamaah-build/portable-x86
```

- Output publish **di luar repo** (`../AyoJamaah-build/`). Jangan commit `dist/`, `target/`,
  `bin/`, `obj/`, atau hasil publish.
- Installer bukan sekadar hasil publish: buat receipt payload dengan
  `installer/make_payload_receipt.py`, lalu jalankan
  `python3 installer/build_installer_nsis.py` (atau `ayojamaah-build installer`).
  Script memvalidasi receipt, menghasilkan include payload, dan mengompilasi NSIS.
  `--check-only` hanya memvalidasi.
- Nomor versi installer dibaca script itu dari `<Version>` di
  `src/PrayerAutomation.UI/PrayerAutomation.UI.csproj` lalu diteruskan sebagai
  `-DAPP_VERSION`/`-DAPP_VERSION_QUAD`. `installer/nsis/AyoJamaah.nsi` sengaja tidak
  menyimpan versi sendiri; `makensis` langsung tanpa define itu akan berhenti dengan
  pesan yang jelas, bukan mengemas versi lama.
- Di Termux tidak ada `dotnet`, `cargo`, atau `makensis` native. Jangan mengklaim build
  WPF/Rust berhasil. Yang bisa dilakukan lokal: review statis, validasi receipt/payload,
  dan kompilasi NSIS bila `makensis` tersedia di mesin build.
- **Jangan jalankan semua smoke test sekaligus.** `tests/SingleInstanceSmoke` menjalankan
  produk dengan profil nyata; tinjau dulu test yang akan dijalankan.

## Konvensi

- Baca `docs/*_ID.md` sebelum mengubah installer, metadata publisher, atau distribusi.
- Perubahan UI harus mempertimbangkan Windows 10 build 19041+ dan Windows 11, x64 dan x86.
- Jadwal adalah hasil hitung aplikasi, bukan pengumuman resmi. Jangan menambah klaim
  akurasi atau kalibrasi otomatis tanpa dasar.
- Pengingat hanya teks; tanpa suara/adzan. Pertahankan batas 200 karakter.
- Tanggal Masehi mengikuti tanggal komputer; koreksi tampilan hanya untuk Hijriyah.
- Versi tampil di title bar dan halaman Tentang. Ubah dari satu sumber, jangan menduplikasi
  nomor versi di beberapa tempat.
- Installer memakai NSIS dan payload dual x86+x64 yang digenerate dari receipt. Jaga
  konsistensi antara receipt, hash pin, dan include payload.
- Cocokkan gaya C#/XAML yang ada. Event handler tipis, logika di kelas/ViewModel. Jangan
  mengedit file `.g.cs` hasil generate; ubah `.xaml` atau `.csproj`.
- Rust: ikuti `rustfmt`, hindari `unwrap` di jalur input pengguna, dan tambahkan unit test
  untuk perhitungan tanggal/jadwal.

## Verifikasi dan pelaporan

Jika build Windows tidak bisa dijalankan di environment saat ini, laporkan:

1. apa yang diverifikasi statis;
2. perintah persis yang harus dijalankan di Windows/CI;
3. risiko yang belum diuji (visual, mixed-DPI, install/upgrade/uninstall).

Jangan menyebut "lulus" untuk hal yang tidak dijalankan.
