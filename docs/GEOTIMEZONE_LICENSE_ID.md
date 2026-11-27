# GeoTimeZone 7.0.0 — data, akurasi, dan lisensi

**Status:** snapshot data yang benar-benar berada di DLL telah terikat ke source pin dan hash. Kode MIT, database ODbL; **bukan clearance hukum seluruh aplikasi/paket**.[4][11]

## 1. Yang dipakai aplikasi

`GeoTimeZone.dll` versi paket 7.0.0 dipakai lewat `LocationDefaults.ResolveTimeZoneForCoordinates` untuk koordinat manual, pilihan locality, lokasi perangkat, dan deteksi lokasi awal aplikasi. Helper tersebut mengambil `.Result`. `LocationDetectionService.cs` sendiri mendapat timezone dari respons provider jaringan; tidak memanggil GeoTimeZone secara langsung.

Lookup GeoTimeZone 7.0.0 berjalan offline. Grid yang lebih halus menyelesaikan sel interior secara langsung; sel di dekat batas memakai geometri polygon sumber yang disimpan di DLL untuk pemeriksaan point-in-polygon. Alternatif hanya dikembalikan bila polygon benar-benar bertumpuk atau berbagi batas, bukan karena satu sel grid memuat beberapa zona. Bila tidak ada polygon yang cocok, library memakai fallback `UTC`/`Etc/GMT` berdasarkan longitude.[6]

Data embedded dimuat otomatis pada lookup pertama. Pemanggil juga dapat memberi `GeoTimeZoneData.zip` sendiri sebelum lookup/`Initialize`; komponen polygon di-cache dengan LRU 8 MiB. Aplikasi tidak memakai jalur custom data dan tidak melakukan unduhan saat berjalan.

Perilaku tersebut didokumentasikan, bukan diubah. Hasil mengikuti geometri TBB 2026f yang embedded, tetapi tetap bukan jaminan batas hukum atau navigasi; lokasi di perbatasan sebaiknya diperiksa pengguna.

## 2. Pin dan snapshot yang terverifikasi

- Source pin: `4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3` (tag `7.0.0`).[4][5][7]
- DLL hasil publish identik byte dengan DLL `lib/net10.0/GeoTimeZone.dll` pada paket NuGet 7.0.0. SHA-256: `ce9f15a3a8f38ba781864ed9c6e6b097fd54374b6671011c727eb5e47655d83a`.
- NuGet package SHA-256: `51e3f30be431a692207359b442863c509d894b55d9251896733f36fa9b98f3a1` (158.132.575 byte; 150,81 MB).
- Satu resource embedded: `GeoTimeZone.GeoTimeZoneData.zip`, 26.348.089 byte. SHA-256: `ff2d8a18ecc872656ce853c1499e3cdcdeb671c02ca4f0b5997f3778bf77d88b`.

Isi resource tersebut:

| Data | Jumlah | SHA-256 logical |
|---|---:|---|
| `manifest.txt` | 13 byte (`GeoTimeZone/1`) | — |
| `TZIndex.dat` | 1.383.864 byte | `0fb9cb7641cb52d26efd04b8179b24a07b88a3a2661ceb52ec4fff4fc235bfab` |
| `TZGeometry.dat` | 26.216.039 byte | `1fc0eed5481b7561907c2bdd8b2410f17e41718577f20c27f05ca52fdaf18905` |

Archive `GeoTimeZoneData.zip` di repo upstream pada tag yang sama memiliki `TZIndex.dat` identik, tetapi byte `TZGeometry.dat` berbeda. Karena itu hash di tabel diambil dari resource DLL yang benar-benar dikirim, bukan dari archive repo. Data dibaca secara statis dari PE/CLR dan ZIP; DLL tidak dijalankan untuk memeriksa isinya.

## 3. Provenance boundary

Rantai yang dinyatakan upstream: **OpenStreetMap contributors → timezone-boundary-builder (Evan Siroky) → GeoTimeZone**.[4][8][10]

DataBuilder pada pin tersebut meminta `2026f/timezones.shapefile.zip`, yaitu varian comprehensive tanpa ocean; bukan varian `-now` atau `-1970`.[5][8]

Tag TBB `2026f` terikat ke commit `37c69a133132646fda2345ac097508230491b61d`. Namun ZIP input asli belum diunduh/dihash atau diregenerasi, dan run release spesifik belum dibuktikan. **2026f adalah input yang dinyatakan source**, bukan bukti lengkap bahwa exact ZIP/tanggal snapshot OSM tertentu menghasilkan binary NuGet itu. TBB sendiri menjelaskan persiapan release dapat menggunakan file yang diunduh pada hari berbeda.[5][8]

Batas tersebut tidak menghilangkan bukti snapshot data pada bagian 2.

## 4. Kode MIT tidak berarti semua isi MIT

Full LICENSE GeoTimeZone 7.0.0 menyertakan kode GeoTimeZone (Matt Johnson-Pint & Simon Bartlett), Geohash-net (Sharon Lourduraj), dan Geohash-js (David Troy) di bawah MIT, serta menyatakan **datanya ODbL**.[4] Teks lisensi ini sama dengan 6.1.0. Pertahankan notice MIT dan kredit masing-masing; jangan mengganti file vendor dengan ringkasan.

ODbL §2.3(a) mengecualikan program untuk membuat/mengoperasikan database. Karena itu, database ODbL **tidak otomatis membuat seluruh source Ayo Jama'ah! wajib ODbL**. Sebaliknya, mengemas data sebagai resource dalam DLL tidak menghilangkan kewajiban database.[11]

| Kondisi | Kewajiban yang perlu diperhatikan |
|---|---|
| Publicly Convey database atau turunannya | §4.2: notice hak tetap utuh; sertakan lisensi/URI bersama database dan dokumentasi relevan. Bila format tidak memungkinkan notice di dalamnya, letakkan di lokasi relevan yang mudah ditemukan.[11] |
| Publicly Use Produced Work dari database | §4.3: notice terkait output menjelaskan asal database dan ODbL, dengan link database/lisensi. Jangan menganggap setiap satu jawaban timezone otomatis merupakan pengambilan substantial.[11] |
| Publicly Use Derivative Database | §4.4: share-alike database di bawah ODbL/opsi yang diizinkan lisensi; §4.5 membatasi penerapan untuk collective database, Produced Work itu sendiri, dan penggunaan internal.[11] |
| Penggunaan publik derivative atau Produced Work darinya | §4.6: tawarkan seluruh derivative database dalam bentuk machine-readable, atau seluruh perubahan/metode beserta tambahan data; gratis melalui internet, biaya produksi wajar untuk media fisik. Ini bukan kewajiban membuka seluruh aplikasi.[11] |
| Tambahan pembatasan/teknologi | §4.7–4.8 membatasi pembatasan tambahan atas hak database dan mensyaratkan parallel unrestricted distribution untuk kondisi teknologi yang disebutkan.[11] |

Penyimpanan geometri polygon TBB sebagai resource merupakan alasan kuat meninjau data lookup sebagai Derivative Database, bukan alasan melisensikan seluruh software dengan ODbL. Kredit **OpenStreetMap contributors** dan pemberitahuan ODbL juga didukung panduan atribusi OSM.[4][10][11]

## 5. Salinan data dan notice pada kandidat portable baru

Kandidat portable metadata-publisher menyertakan full LICENSE GeoTimeZone, full LICENSE TimeZoneConverter, ODbL 1.0, dan notice/data-source berikut **tanpa mengganti notice vendor lama**:

- `THIRD-PARTY/GeoTimeZone-NOTICE.md` — kredit kode/data, pin, format, hash logical, dan batas offer.
- `THIRD-PARTY/geotimezone-data/GeoTimeZoneData.zip` — seluruh data lookup embedded dalam bentuk machine-readable.

ZIP tersebut adalah salinan resource data dari DLL 7.0.0 (SHA-256 `ff2d8a18…`), berisi `TZIndex.dat` dan `TZGeometry.dat`. File ini disediakan sebagai salinan data, **bukan input baru untuk runtime**; source lookup, dependency, dan database DLL tidak diubah. Seluruh data lookup disediakan kepada penerima, bukan hanya potongan query. Distribusi offline data ini tidak diberi pembatasan tambahan oleh aplikasi; hak database tetap mengikuti ODbL.[11]

Archive sumber juga dapat diperoleh gratis dari URL pinned berikut:

- [GeoTimeZoneData.zip](https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone/GeoTimeZoneData.zip) — 26.348.089 byte, SHA-256 `0a243525473e8b436b06e12e72c5d9ea009e012ce5a09be0373106bf920a5759`. Archive repo ini bukan salinan byte-identik dengan resource DLL; gunakan salinan embedded di atas untuk data yang benar-benar dikirim.

**Batas:** ini pemenuhan teknis salinan/notice, bukan opini bahwa seluruh kewajiban ODbL selesai. Notice yang memadai pada output/UI publik belum diverifikasi; hak lain pada data/binary pihak ketiga tetap perlu ditutup; source aplikasi sendiri memakai MIT (lihat [`../LICENSE`](../LICENSE)). Installer/final portable sebelumnya tidak berubah dan tidak memperoleh notice baru secara retroaktif. Lihat [inventaris third-party](THIRD_PARTY_NOTICES.md) dan [metadata publisher](PUBLISHER_METADATA_ID.md).

## Sources

[4] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/LICENSE
[5] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone.DataBuilder/Program.cs
[6] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone/TimeZoneLookup.cs
[7] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone/GeoTimeZone.csproj
[8] https://raw.githubusercontent.com/evansiroky/timezone-boundary-builder/37c69a133132646fda2345ac097508230491b61d/README.md
[10] https://www.openstreetmap.org/copyright
[11] https://opendatacommons.org/licenses/odbl/odbl-10.txt
[12] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone/TimeZoneResult.cs
[13] https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/4d6c52d7bc9e4e3bcf3ee1cb698ce1de372981c3/src/GeoTimeZone.DataBuilder/TimeZoneDataBuilder.cs
