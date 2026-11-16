# GeoTimeZone 6.1.0 — data, akurasi, dan lisensi

**Status:** snapshot lookup yang benar-benar berada di DLL telah terikat ke source pin dan hash logical. Kode MIT, database ODbL; **bukan clearance hukum seluruh aplikasi/paket**.[4][11]

## 1. Yang dipakai aplikasi

`GeoTimeZone.dll` versi paket 6.1.0 dipakai oleh tiga jalur pada `MainWindow.xaml.cs`: koordinat manual, pilihan locality, dan lokasi perangkat. Ketiganya mengambil `.Result`. `LocationDetectionService.cs` sendiri mendapat timezone dari respons provider jaringan; tidak memanggil GeoTimeZone secara langsung.

Lookup GeoTimeZone offline: koordinat diubah ke geohash base32 presisi lima, lalu dicari pada database geohash/prefix. Ini **bukan pemeriksaan point-in-polygon saat runtime** dan bukan unduhan database setiap lookup.[6][7]

Sel yang mencakup beberapa timezone dapat menghasilkan kandidat lain. Pada source pin ini, runtime mengurutkan nomor ID sebelum mengambil `.Result`; hasil pertama bukan jaminan kandidat dengan coverage terbesar walaupun builder semula mengurutkan coverage.[6][12][13]

Tanpa match, library memakai fallback `UTC`/`Etc/GMT` berdasarkan longitude.[6]

Perilaku tersebut didokumentasikan, bukan diubah. Lokasi dekat batas timezone dan fallback tetap memerlukan pemeriksaan pengguna; jangan menganggap satu hasil sebagai jaminan batas geografis tepat.

## 2. Pin dan snapshot yang terverifikasi

- Source pin: `73206b43819c89f8b6ec2926b2ca227fc783b938`.[4][5][7]
- DLL hasil publish identik byte dengan DLL net9.0 pada cache dan paket NuGet 6.1.0. SHA-256: `8f0ca2647579c95c35bb9894a484b230f01077e34608955be4015cc2057457b2`.
- NuGet package SHA-256: `b5addc4bb9f751db131a7b15f4a6e6860bb3c8ca36bd540336bda6c39ff8a66a`.
- Dua resource embedded: `GeoTimeZone.TZ.dat.gz` dan `GeoTimeZone.TZL.dat.gz`.[7]

Resource dari source upstream dan resource DLL **berbeda byte gzip**, tetapi hasil dekompresinya terbukti identik byte-per-byte. Hash berikut berlaku untuk data setelah dekompresi, bukan file gzip:

| Data | Jumlah | SHA-256 logical |
|---|---:|---|
| `TZ.dat.gz` | 1289909 record, 11609181 byte | `8bb1f15d9785f02224533724d011168ad1b2df5beaef38cf5937dd56c716de50` |
| `TZL.dat.gz` | 419 ID, 6895 byte | `b21e103b251033177a7a5fdc63cc49a4da73ac224a541d0a9370e26d05ec3f13` |

Data dibaca secara statis dari PE/CLR dan gzip. DLL tidak dimuat untuk menjalankan kode library, GPS, atau aplikasi.

## 3. Provenance boundary

Rantai yang dinyatakan upstream: **OpenStreetMap contributors → timezone-boundary-builder (Evan Siroky) → GeoTimeZone**.[4][8][10]

DataBuilder pada pin tersebut secara eksplisit meminta `2025b/timezones.shapefile.zip`, yaitu varian comprehensive tanpa ocean; bukan varian `-now` atau `-1970`.[5][8]

Tag TBB 2025b terikat ke commit `9d61564787b10c463676df553e080e73e8a73de9`. Namun ZIP input asli belum diunduh/dihash atau diregenerasi, dan run release spesifik belum dibuktikan. **2025b adalah input yang dinyatakan source**, bukan bukti lengkap bahwa exact ZIP/tanggal snapshot OSM tertentu menghasilkan binary NuGet itu. TBB sendiri menjelaskan persiapan release dapat menggunakan file yang diunduh pada hari berbeda.[5][8]

Batas tersebut tidak menghilangkan bukti snapshot lookup logical pada bagian 2.

## 4. Kode MIT tidak berarti semua isi MIT

Full LICENSE GeoTimeZone menyertakan kode GeoTimeZone (Matt Johnson-Pint & Simon Bartlett), Geohash-net (Sharon Lourduraj), dan Geohash-js (David Troy) di bawah MIT, serta menyatakan **datanya ODbL**.[4] Pertahankan notice MIT dan kredit masing-masing; jangan mengganti file vendor dengan ringkasan.

ODbL §2.3(a) mengecualikan program untuk membuat/mengoperasikan database. Karena itu, database ODbL **tidak otomatis membuat seluruh source Ayo Jama'ah! wajib ODbL**. Sebaliknya, mengemas data sebagai resource dalam DLL tidak menghilangkan kewajiban database.[11]

| Kondisi | Kewajiban yang perlu diperhatikan |
|---|---|
| Publicly Convey database atau turunannya | §4.2: notice hak tetap utuh; sertakan lisensi/URI bersama database dan dokumentasi relevan. Bila format tidak memungkinkan notice di dalamnya, letakkan di lokasi relevan yang mudah ditemukan.[11] |
| Publicly Use Produced Work dari database | §4.3: notice terkait output menjelaskan asal database dan ODbL, dengan link database/lisensi. Jangan menganggap setiap satu jawaban timezone otomatis merupakan pengambilan substantial.[11] |
| Publicly Use Derivative Database | §4.4: share-alike database di bawah ODbL/opsi yang diizinkan lisensi; §4.5 membatasi penerapan untuk collective database, Produced Work itu sendiri, dan penggunaan internal.[11] |
| Penggunaan publik derivative atau Produced Work darinya | §4.6: tawarkan seluruh derivative database dalam bentuk machine-readable, atau seluruh perubahan/metode beserta tambahan data; gratis melalui internet, biaya produksi wajar untuk media fisik. Ini bukan kewajiban membuka seluruh aplikasi.[11] |
| Tambahan pembatasan/teknologi | §4.7–4.8 membatasi pembatasan tambahan atas hak database dan mensyaratkan parallel unrestricted distribution untuk kondisi teknologi yang disebutkan.[11] |

Konversi polygon menjadi geohash merupakan alasan kuat meninjau data lookup sebagai Derivative Database, bukan alasan melisensikan seluruh software dengan ODbL. Kredit **OpenStreetMap contributors** dan pemberitahuan ODbL juga didukung panduan atribusi OSM.[4][10][11]

## 5. Salinan data dan notice pada kandidat portable baru

Kandidat portable metadata-publisher menyertakan full LICENSE GeoTimeZone, full LICENSE TimeZoneConverter, ODbL 1.0, dan notice/data-source berikut **tanpa mengganti notice vendor lama**:

- `THIRD-PARTY/GeoTimeZone-NOTICE.md` — kredit kode/data, pin, format, hash logical, dan batas offer.
- `THIRD-PARTY/geotimezone-data/TZ.dat.gz` — seluruh lookup geohash dalam bentuk machine-readable gzip.
- `THIRD-PARTY/geotimezone-data/TZL.dat.gz` — seluruh pemetaan ID lookup yang berpasangan dengannya.

Kedua gzip adalah salinan source upstream pinned yang sudah terbukti menghasilkan data logical identik dengan resource DLL. File tersebut disediakan sebagai salinan data, **bukan input baru untuk runtime**; source lookup, dependency, dan database DLL tidak diubah. Seluruh data lookup disediakan kepada penerima, bukan hanya potongan query. Distribusi offline data ini tidak diberi pembatasan tambahan oleh aplikasi; hak database tetap mengikuti ODbL.[11]

Source data juga dapat diperoleh gratis dari URL pinned berikut:

- [TZ.dat.gz](https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone/TZ.dat.gz)
- [TZL.dat.gz](https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone/TZL.dat.gz)

**Batas:** ini pemenuhan teknis salinan/notice, bukan opini bahwa seluruh kewajiban ODbL selesai. Notice yang memadai pada output/UI publik belum diverifikasi; hak lain pada data/binary pihak ketiga tetap perlu ditutup; source aplikasi sendiri memakai MIT (lihat [`../LICENSE`](../LICENSE)). Installer/final portable sebelumnya tidak berubah dan tidak memperoleh notice baru secara retroaktif. Lihat [inventaris third-party](THIRD_PARTY_NOTICES.md) dan [metadata publisher](PUBLISHER_METADATA_ID.md).

## Sources

[4] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/LICENSE
[5] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone.DataBuilder/Program.cs
[6] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone/TimeZoneLookup.cs
[7] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone/GeoTimeZone.csproj
[8] https://raw.githubusercontent.com/evansiroky/timezone-boundary-builder/9d61564787b10c463676df553e080e73e8a73de9/README.md
[10] https://www.openstreetmap.org/copyright
[11] https://opendatacommons.org/licenses/odbl/odbl-10.txt
[12] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone/TimeZoneResult.cs
[13] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/src/GeoTimeZone.DataBuilder/TimeZoneDataBuilder.cs
