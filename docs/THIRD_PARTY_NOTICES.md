# Third-party notices — Ayo Jama'ah! 2.7.0

Inventaris komponen pihak ketiga yang dipakai source 2.7.0 (dalam pengembangan) dan paket rilis 2.6.0. **Ini bukan LICENSE utama aplikasi.** Source aplikasi dilisensikan MIT (lihat [`../LICENSE`](../LICENSE), Copyright (c) 2026 Muhammad Nashiruddin Al-Asqalani). Setiap komponen di bawah tunduk pada lisensi/terms pemiliknya; lisensi MIT source aplikasi tidak menggantikan terms binary atau data pihak ketiga.

Riwayat backend taskbar WinUI dihapus dari source pada 2.2.1 beserta NOTICE/lisensinya dan tidak dicantumkan lagi di repository.

## Paket dan komponen

| Komponen | Versi terbukti | Peran | Lisensi/terms yang dibaca |
|---|---|---|---|
| GeoTimeZone | 6.1.0 | Direct; `GeoTimeZone.dll` | MIT untuk kode; ODbL untuk data boundary; atribusi Geohash-net/Geohash-js |
| TimeZoneConverter | 7.2.0 | Direct; `TimeZoneConverter.dll` | MIT untuk library |
| Data GeoNames (`cities500`, `indonesia`) | snapshot 22 November 2026 UTC | Dataset lokal permukiman dan lapisan Indonesia di `src/PrayerAutomation.UI/Data/geonames/` | CC BY 4.0 |
| Data Kemendagri lewat `cahyadsn/wilayah` | Kepmendagri No. 300.2.2-2430 Tahun 2025 | Hierarki dan nama resmi kabupaten/kota pada `indonesia.tsv.gz` | MIT untuk tooling; data tunduk pada sumber resmi |
| Sampel Copernicus DEM GLO-90 lewat Open-Meteo | GLO-90, snapshot 22 November 2026 | Kolom `sampled_elevation`, `elevation_source`, dan `elevation_quality` | CC BY 4.0; atribusi Copernicus/Open-Meteo wajib |
| Natural Earth 1:10m Admin 0 Countries | snapshot 20 November 2026 | Poligon batas Indonesia (`indonesia.boundary.bin`) | Public domain |
| Konten dzikir dan doa Rumaysho.Com | snapshot 25 November 2026 UTC | Teks Arab, transliterasi Latin, dan arti pada `Data/dzikir-doa.json` | Kredit **Rumaysho.Com — Muhammad Abduh Tuasikal**; setiap entri menautkan halaman asal; ketentuan konten mengikuti pemilik sumber |
| Runtime .NET 10 (self-contained, WPF) | 10.0.9 | `AyoJamaah.exe`, `PresentationFramework*.dll`, `System.Windows.*.dll` | MIT; terms runtime Microsoft berlaku saat redistribusi |
| Proyeksi Microsoft.Windows.SDK.NET.Ref | 10.0.26100.57 | `Microsoft.Windows.SDK.NET.dll`, `WinRT.Runtime.dll` | Windows SDK terms |
| `prayer_core.dll` | 0.1.0 | Internal; perhitungan jadwal/kalender | Source asli aplikasi |

### GeoTimeZone: kode MIT, data ODbL

- Pemilik kode: Matt Johnson-Pint dan Simon Bartlett. LICENSE juga mengakui Geohash-net oleh Sharon Lourduraj dan Geohash-js oleh David Troy.
- Boundary berasal dari OpenStreetMap melalui [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) oleh Evan Siroky.
- [LICENSE lengkap](third-party/licenses/GeoTimeZone-6.1.0-LICENSE.txt), [teks ODbL 1.0](third-party/licenses/ODbL-1.0.txt), dan [penjelasan ODbL](GEOTIMEZONE_LICENSE_ID.md).
- Exact input TBB `2025b/timezones.shapefile.zip` dan tanggal snapshot OSM belum dibuktikan; review kewajiban ODbL sebelum publikasi tetap diperlukan.

### TimeZoneConverter dan data mapping

Source pin: `f96c7a7d167d9ab93bb8c924fc7413b84f52010c`. Copyright library: Matt Johnson-Pint.

- [LICENSE lengkap](third-party/licenses/TimeZoneConverter-7.2.0-LICENSE.txt).
- Data mapping embedded tidak memerlukan unduhan ketika dipakai; resolusi `TimeZoneInfo` tetap bergantung pada data timezone Windows.
- Versi/snapshot dan lisensi sumber mapping individual belum dipetakan.

### GeoNames

Derived `cities500` dan `indonesia`, snapshot 22 November 2026 UTC, **CC BY 4.0**. Source: `cities500.zip`, `ID.zip`, `admin1CodesASCII.txt`, `admin2Codes.txt`, `countryInfo.txt`, dan `alternateNamesV2.zip`. Rekaman feature-class `P` disaring; kolom yang dipilih mencakup koordinat WGS84, `dem` asli, kode fitur, populasi, dan nama alternatif. Nama alternatif hanya menjadi kunci pencarian, bukan pengganti nama resmi tempat. Normalisasi whitespace dan kompresi gzip diterapkan oleh pipeline proyek. [NOTICE lengkap](../src/PrayerAutomation.UI/Data/geonames/NOTICE.txt) disertakan di source dan paket portable; attribution, snapshot, dan perubahan tercantum di sana.

### Kemendagri dan `cahyadsn/wilayah`

Hierarki administrasi Indonesia berasal dari [cahyadsn/wilayah](https://github.com/cahyadsn/wilayah), yang mengikuti Kepmendagri No. 300.2.2-2430 Tahun 2025. Skrip tooling tersebut berlisensi MIT. `wilayah.sql` dan `wilayah_level_1_2.sql` dipakai pada saat build untuk memetakan nama kabupaten/kota resmi dan kode wilayah. Sekitar 95,5% baris Indonesia memperoleh kecocokan resmi; baris tanpa kecocokan mempertahankan nama dari GeoNames. Data resmi tetap tunduk pada ketentuan instansi asal, bukan pada lisensi MIT tooling-nya.

### Copernicus DEM GLO-90 lewat Open-Meteo

Sampel ketinggian diambil saat penyusunan data melalui [Open-Meteo Elevation API](https://open-meteo.com/en/docs/elevation-api) dari Copernicus DEM GLO-90. Median jendela `3×3` dipakai untuk menekan puncak DSM dari atap atau kanopi. Sumber dan kualitas ketinggian ditulis pada kolom `elevation_source` dan `elevation_quality`; baris tanpa sampel memakai `dem` GeoNames sebagai fallback. Aplikasi tidak memanggil API ini saat berjalan dan tetap bekerja offline. Atribusi: Copernicus DEM GLO-90 via Open-Meteo; data disediakan "as is" tanpa jaminan akurasi atau kelengkapan.

### Tile peta OpenStreetMap (opsional, 2.7.0)

Pemilih lokasi memuat tile raster dari `https://tile.openstreetmap.org/{z}/{x}/{y}.png` secara otomatis saat jendela peta dibuka; kotak **Tampilkan tile peta online** aktif secara default, dan menghapus centang menghentikannya. Tidak ada tile yang dibundel atau dimuat otomatis, dan tidak ada WebView2/package peta baru. Permintaan memakai HTTPS, User-Agent aplikasi, pembatasan laju, cache memori 24 jam, serta batas ukuran respons.

Atribusi wajib di jendela peta: **© OpenStreetMap contributors**. Data peta tersebut tersedia di bawah [Open Database License (ODbL) 1.0](https://www.openstreetmap.org/copyright); pengguna juga harus mematuhi [Tile Usage Policy](https://operations.osmfoundation.org/policies/tiles/) OpenStreetMap Foundation. Koordinat yang diklik dipakai lokal untuk perhitungan jadwal, bukan untuk mengirim nama lokasi ke server tile.

### Natural Earth

`indonesia.boundary.bin` diturunkan dari Natural Earth 1:10m Admin 0 Countries (Indonesia), **public domain**. Hanya ring luar yang disimpan; dipakai untuk memilih lapisan nama Indonesia, bukan batas hukum atau navigasi. Sumber: <https://www.naturalearthdata.com/>.

### Runtime .NET dan proyeksi Windows SDK

Aplikasi diterbitkan self-contained (`net10.0-windows10.0.26100.0`); runtime dibundel, bukan dibagikan dari NuGet pada mesin pengguna. Lisensi MIT .NET dan terms Windows SDK tetap berlaku pada distribusi; teks lisensinya tidak diduplikasi di repository ini. Notice/REDIST untuk proyeksi exact belum ditutup secara formal.

### Rust

`Cargo.lock` hanya memuat `prayer-core 0.1.0`; `[dependencies]` kosong dan tidak ada crate third-party. Runtime statis GCC/MinGW dan attribution-nya belum diaudit.

## Syarat yang belum ditutup sebelum publikasi

1. Review kewajiban ODbL (data boundary GeoTimeZone) dan mapping data TimeZoneConverter.
2. Tutup notice/REDIST proyeksi Windows SDK exact.
3. Audit toolchain native/statis dan final license source utama.
4. Terbitkan tag data `data-v2` beserta manifest SHA-256 sebelum paket opsional didistribusikan; berkas data rilis bukan dibuat oleh workflow `Release` aplikasi.

## Penghalusan status

- Baris di atas adalah inventaris berbasis paket yang benar-benar terpasang; bukan clearance hukum menyeluruh.
- Keputusan atribusi untuk konten referensi aplikasi, mis. teks ringkas hadis di `WidgetMarqueeContent.cs`, masih menunggu konfirmasi pemilik. Teks dzikir dan doa sudah memakai kredit serta tautan sumber per entri.
