# Metadata publisher — Ayo Jama'ah! 2.1.0

> **Catatan 2.2.0:** backend taskbar WinUI tidak lagi diproduksi. Tabel di bawah mendokumentasikan build 2.1.0 untuk referensi historis; metadata `AyoJamaah.exe`/`AyoJamaah.dll` tetap berlaku.

## Identitas yang dirapikan

Field `Company` pada kedua project C# ditetapkan menjadi **`Ayo Jama'ah!`**. Ini label publisher/brand untuk metadata file, bukan penetapan badan hukum atau pemegang hak cipta.

| Artefak | CompanyName sebelum | CompanyName sekarang | ProductName |
|---|---|---|---|
| `AyoJamaah.exe` | `AyoJamaah` | `Ayo Jama'ah!` | `Ayo Jama'ah!` |
| `AyoJamaah.dll` | `AyoJamaah` | `Ayo Jama'ah!` | `Ayo Jama'ah!` |

Nilai dibaca dari version-info PE hasil build/publish, bukan hanya dari XML project.

- Produk utama tetap `2.1.0`, FileVersion `2.1.0.0`.
- Versi backend library tetap `1.0.0`/`1.0.0.0`; tidak diubah hanya untuk metadata publisher.
- AssemblyName, namespace, profil/storage, mutex, identitas startup, AppId dan direktori upgrade installer tetap dipertahankan.
- Source installer sudah memakai `AppPublisher=Ayo Jama'ah!`; tidak ada perubahan pada guard atau payload include installer lama.
- `LegalCopyright` tidak diisi dengan nama/tahun dugaan. Pemegang hak cipta legal masih perlu diputuskan pemilik proyek.

## Batas penting

**Metadata CompanyName tidak memberikan status Verified publisher, signature Authenticode, atau reputasi SmartScreen.** SmartScreen menilai certificate/publisher dan hash file; binary yang ditandatangani pun masih dapat mendapat peringatan sampai reputasinya terbentuk.[2] Hasil pemeriksaan build ini menunjukkan certificate table embedded kosong pada tiga PE aplikasi. Itu tidak merupakan pemeriksaan catalog signing atau reputasi Windows.

Build dan publish self-contained win-x64 dijalankan dengan sumber NuGet offline; keduanya exit 0 tanpa warning. Native Rust DLL dipakai kembali dengan hash identik. Runtime berisi 739 file; hanya apphost, dua managed DLL dan dua PDB yang berubah. Sembilan frame ikon v13 dan manifest `asInvoker` diverifikasi tanpa menjalankan produk.

Portable hasil perubahan ini adalah kandidat lokal; bukan bukti acceptance UI/native Windows. Aplikasi tetap memakai lokasi profil legacy yang sama, sehingga portable tidak berarti profil terisolasi. **Jangan menjalankannya otomatis untuk pengujian**, terutama jika profil tersimpan mengaktifkan auto-lock atau gambar lock screen.

Installer final sebelumnya tidak dibangun ulang pada pekerjaan metadata ini. Jangan mengganti runtime installer dengan file portable baru tanpa regenerasi/verifikasi payload yang tepat. Tidak ada signing, instalasi, upload, staging, commit, atau perubahan profil/registry oleh tahap ini.

Bukti rinci dan build/publish disimpan di luar working tree agar path mesin dan receipt lokal tidak perlu dipublikasikan bersama source. Lihat juga [third-party notices](THIRD_PARTY_NOTICES.md) untuk batas lisensi paket dan [audit repository](REPOSITORY_AUDIT_ID.md) untuk history.

## Distribusi lokal lanjutan — 12 November 2026

Installer baru sudah dikompilasi dari runtime metadata-publisher dan notices yang sama dengan portable terbaru di `dist/`. Compile exit 0 tanpa warning; guard/entrypoint installer dan runtime tetap identik dengan sumber terverifikasi. Keduanya **unsigned**: ini bukan Authenticode/Verified publisher atau jaminan lolos SmartScreen. Paket finalisasi lama tetap tidak berubah. Lihat [lokasi dan batas distribusi terbaru](DISTRIBUTION_ID.md).

## Sources

[2] https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation — Microsoft SmartScreen reputation
