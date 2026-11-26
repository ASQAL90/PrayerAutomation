# Privasi

Ayo Jama'ah! berjalan sepenuhnya offline untuk perhitungan jadwal sholat. Tidak
ada telemetri, analytics, atau iklan. Dokumen ini menjelaskan data apa yang
disimpan dan kapan jaringan digunakan.

## Data yang disimpan di perangkat

| Lokasi | Isi |
| --- | --- |
| `%APPDATA%\PrayerAutomation\config.json` | koordinat, nama lokasi, zona waktu, standar perhitungan, pengaturan pengingat, lock screen, dan startup |
| `%APPDATA%\PrayerAutomation\config.json.bak` | salinan pengaturan sebelum penyimpanan terakhir |
| `%LOCALAPPDATA%\PrayerAutomation\prayer_automation.log` | log error lokal (rotasi 2 MB, maksimal 3 arsip, retensi 30 hari) |
| `%LOCALAPPDATA%\PrayerAutomation\install-manifest.txt` | daftar berkas yang dipasang installer |
| `%LOCALAPPDATA%\PrayerAutomation\LockScreen\` | salinan gambar lock screen dan jurnal pemulihan sementara |
| `%LOCALAPPDATA%\AyoJamaah\offline-data\v2\` | paket data lokasi offline opsional (bila diunduh) |
| `%LOCALAPPDATA%\Programs\PrayerAutomation\` | berkas aplikasi hasil instalasi |

Registry HKCU yang dapat ditulis:

- `Software\Microsoft\Windows\CurrentVersion\Run` — entri autostart (dapat
  dimatikan dari pengaturan aplikasi),
- `Software\Microsoft\Windows\CurrentVersion\Uninstall\{...}_is1` — metadata
  uninstall,
- `Software\AyoJamaah` — lokasi instalasi.

## Kapan jaringan digunakan

| Tujuan | Endpoint | Kapan | Data yang terkirim |
| --- | --- | --- | --- |
| Cek pembaruan | `api.github.com` | saat cek pembaruan (manual atau terjadwal) | User-Agent, versi aplikasi |
| Unduh pembaruan/data | `github.com`, `raw.githubusercontent.com` | hanya bila pembaruan atau paket data dipilih | permintaan berkas biasa |
| Perkiraan lokasi dari jaringan | `ipapi.co`, `ipwho.is` | **hanya** saat Anda menekan tombol **Perkiraan dari jaringan** (atau deteksi awal saat lokasi Windows tidak tersedia) | alamat IP publik (otomatis oleh HTTP) |
| Geocoding online | `nominatim.openstreetmap.org` | **hanya** bila Anda mengaktifkan opsi geocoding online dan memberi persetujuan | koordinat atau nama tempat, bahasa, User-Agent |
| Tile peta pemilih lokasi | `tile.openstreetmap.org` | otomatis saat jendela peta dibuka; hapus centang **Tampilkan tile peta online** untuk menghentikannya | nomor tile/zoom yang dilihat, User-Agent, alamat IP publik |

Tidak ada data lokasi, nama, atau log yang dikirim ke pihak lain saat aplikasi
dibuka. Saat jendela peta dibuka, tile OpenStreetMap dimuat otomatis dan
mengirim nomor tile, User-Agent, serta alamat IP publik; hapus centang
**Tampilkan tile peta online** untuk mencegahnya. Pencarian dengan data lokal
dan penetapan titik tidak mengirim koordinat ke server; permintaan jaringan
lain hanya terjadi lewat tindakan eksplisit Anda di atas. Geocoding online dan
tile peta dapat dimatikan kapan saja dalam sesi tersebut.

## Menghapus data

- Nonaktifkan autostart dari pengaturan aplikasi.
- Hapus folder `%APPDATA%\PrayerAutomation` dan
  `%LOCALAPPDATA%\PrayerAutomation` setelah aplikasi ditutup.
- Uninstaller saat ini **tidak** menghapus folder pengaturan pengguna; hapus
  manual bila diinginkan.

## Perubahan

Perubahan pada dokumen ini akan dicatat di `CHANGELOG.md` dan riwayat Git.
