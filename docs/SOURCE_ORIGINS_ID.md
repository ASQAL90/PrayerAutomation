# Penghargaan dan asal source — Ayo Jama'ah!

Aplikasi memakai beberapa referensi teknis dan dependency yang perlu disebut. Daftar ini sengaja ringkas: hanya yang relevan dengan source sekarang. Atribusi bersifat penghargaan dan tidak mengubah lisensi source aplikasi (MIT) maupun hak pihak lain atas datanya.

## Howard Hinnant — algoritma civil-date

- [Artikel algoritma kalender](https://howardhinnant.github.io/date_algorithms.html), pin halaman `c4852d230081ba5504e60db12f5f55c57990ed0b`; [library date v3.0.1](https://github.com/HowardHinnant/date/tree/6e921e1b1d21e84a5c82416ba7ecd98e33a436d0).
- Helper Rust `days_from_civil` di `prayer.rs`/`calendar.rs` dan inverse `civil_from_days` mengikuti keluarga algoritma tersebut, walaupun ditulis ulang dalam bahasa yang berbeda.
- Artikel menyatakan algoritmanya **"Consider these donated to the public domain."** Asal port/intermediate belum dikonfirmasi satu-satu; atribusi ini penghargaan, bukan klaim rantai donor yang pasti.

## GeoTimeZone — Geohash-net / Geohash-js

GeoTimeZone memuat Geohash-net (Sharon Lourduraj) dan port Geohash-js (David Troy) di dalamnya. Kredit ada pada [LICENSE GeoTimeZone](third-party/licenses/GeoTimeZone-6.1.0-LICENSE.txt); keduanya source di dalam dependency, bukan bukti tiga salinan langsung ke source aplikasi. Data timezone ODbL tetap terpisah; lihat [third-party notices](THIRD_PARTY_NOTICES.md).

## Algoritma internal

- Perhitungan jadwal sholat (`prayer-core`) memakai algoritma standar yang umum dipakai (posisi matahari/kalender hijriah); implementasinya ditulis sendiri di Rust.
- Konversi Hijriah diport dari source Python internal lama proyek ini.
