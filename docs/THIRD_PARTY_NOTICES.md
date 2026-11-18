# Third-party notices — Ayo Jama'ah! 2.2.0

Inventaris komponen pihak ketiga yang benar-benar dipakai source dan paket 2.2.0. **Ini bukan LICENSE utama aplikasi.** Source aplikasi dilisensikan MIT (lihat [`../LICENSE`](../LICENSE), Copyright (c) 2026 Muhammad Nashiruddin Al-Asqalani). Setiap komponen di bawah tunduk pada lisensi/terms pemiliknya; lisensi MIT source aplikasi tidak menggantikan terms binary atau data pihak ketiga.

Riwayat backend taskbar WinUI dihapus dari source pada 2.2.0 beserta NOTICE/lisensinya dan tidak dicantumkan lagi di repository.

## Paket dan komponen

| Komponen | Versi terbukti | Peran | Lisensi/terms yang dibaca |
|---|---|---|---|
| GeoTimeZone | 6.1.0 | Direct; `GeoTimeZone.dll` | MIT untuk kode; ODbL untuk data boundary; atribusi Geohash-net/Geohash-js |
| TimeZoneConverter | 7.2.0 | Direct; `TimeZoneConverter.dll` | MIT untuk library |
| Data GeoNames (`cities1000`) | snapshot 22 Oktober 2026 UTC | Dataset lokal permukiman di `src/PrayerAutomation.UI/Data/` | CC BY 4.0 |
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

Derived `cities1000`, snapshot 22 Oktober 2026 UTC, **CC BY 4.0** (filter feature-class `P`, pilihan kolom, normalisasi whitespace, kompresi TSV). [NOTICE lengkap](../src/PrayerAutomation.UI/Data/geonames/NOTICE.txt) disertakan di source dan paket portable; attribution, snapshot, dan perubahan tercantum di sana.

### Runtime .NET dan proyeksi Windows SDK

Aplikasi diterbitkan self-contained (`net10.0-windows10.0.26100.0`); runtime dibundel, bukan dibagikan dari NuGet pada mesin pengguna. Lisensi MIT .NET dan terms Windows SDK tetap berlaku pada distribusi; teks lisensinya tidak diduplikasi di repository ini. Notice/REDIST untuk proyeksi exact belum ditutup secara formal.

### Rust

`Cargo.lock` hanya memuat `prayer-core 0.1.0`; `[dependencies]` kosong dan tidak ada crate third-party. Runtime statis GCC/MinGW dan attribution-nya belum diaudit.

## Syarat yang belum ditutup sebelum publikasi

1. Review kewajiban ODbL (data boundary GeoTimeZone) dan mapping data TimeZoneConverter.
2. Tutup notice/REDIST proyeksi Windows SDK exact.
3. Audit toolchain native/statis dan final license source utama.

## Penghalusan status

- Baris di atas adalah inventaris berbasis paket yang benar-benar terpasang; bukan clearance hukum menyeluruh.
- Keputusan atribusi untuk konten referensi aplikasi, mis. teks ringkas hadis di `WidgetMarqueeContent.cs`, masih menunggu konfirmasi pemilik.
