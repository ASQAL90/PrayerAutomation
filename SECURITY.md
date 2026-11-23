# Kebijakan Keamanan

## Versi yang didukung

| Versi | Dukungan |
| --- | --- |
| 2.5.x | Perbaikan keamanan dan rilis pemeliharaan |
| < 2.5 | Tidak didukung; silakan perbarui ke rilis terbaru |

## Melaporkan kerentanan

**Jangan** membuka issue publik untuk masalah keamanan.

1. **GitHub Private Vulnerability Reporting** (disarankan):
   buka tab **Security → Report a vulnerability** pada repositori ini.
2. **Email**: `madinalani@gmail.com` dengan subjek `[SECURITY] Ayo Jama'ah!`.

Sertakan bila memungkinkan:

- versi aplikasi (tampil di About) dan arsitektur (x64/x86),
- langkah reproduksi atau proof-of-concept,
- dampak yang Anda perkirakan,
- apakah Anda ingin disebutkan namanya.

Kami berusaha mengonfirmasi laporan dalam **72 jam** dan memberikan pembaruan
status dalam **7 hari**. Mohon beri waktu perbaikan sebelum mempublikasikan
detail secara terbuka.

## Lingkup

- Rantai pembaruan otomatis (manifest, signature, delta, helper updater).
- Installer NSIS dan pipeline payload.
- Parser data lokal (boundary biner, TSV GeoNames, manifest JSON, catatan rilis).
- Penanganan file lokal, jurnal pemulihan lock screen, dan registry.
- Kebocoran data pribadi atau endpoint jaringan yang tidak terdokumentasi.

## Di luar lingkup

- Penyerang yang sudah memiliki hak Administrator atau akses fisik ke mesin.
- Malware/proses lain yang berjalan sebagai pengguna yang sama dan sudah bisa
  menulis ke folder profil pengguna.
- Kompromi akun GitHub pemilik repositori (laporkan ke GitHub terlebih dahulu).
- Layanan pihak ketiga (GitHub, ipapi.co, ipwho.is, Nominatim).
- Rekayasa sosial dan serangan denial-of-service pada mesin pengguna sendiri.

## Model ancaman (ringkas)

1. **Kanal pembaruan** — aset rilis dan akun GitHub dianggap target utama.
   Manifest pembaruan wajib bertanda tangan; rilis tanpa signature tidak boleh
   dipasang otomatis.
2. **File lokal yang dapat ditulis pengguna** — `%APPDATA%`, `%LOCALAPPDATA%`,
   dan folder portable. Aplikasi berjalan tanpa elevasi (per-user).
3. **Installer** — instalasi hanya ke `%LOCALAPPDATA%\Programs\PrayerAutomation`;
   installer tidak meminta hak Administrator.
4. **Jaringan** — hanya endpoint yang tercantum di `PRIVACY.md`; geocoding
   online memerlukan persetujuan eksplisit.
5. **Bukan target** — eskalasi hak akses lokal, persistence tingkat sistem, dan
   penyerang same-user yang sudah memiliki eksekusi kode.

## Safe harbor

Kami menghargai riset keamanan yang dilakukan dengan itikad baik. Selama Anda
tidak mengakses, mengubah, atau menyebarkan data pengguna lain, tidak melakukan
gangguan layanan, dan memberi waktu perbaikan yang wajar, kami tidak akan
menempuh tindakan hukum terkait laporan tersebut.
