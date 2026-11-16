# Penghargaan dan asal source — Ayo Jama'ah!

Kami menghargai pengembang, penulis algoritma, dan komunitas yang source atau idenya membantu aplikasi ini. Penghargaan tetap relevan ketika implementasi diadaptasi ke bahasa lain. **Adaptasi source, inspirasi teknis, dan penggunaan dependency adalah kategori berbeda**; pencantuman berikut tidak mengklaim seluruh aplikasi berasal dari satu proyek maupun seluruh source original.

## Source yang terbukti diadaptasi

### Awqat Salaat — Khiro

- [Repository Awqat Salaat](https://github.com/Khiro95/Awqat-Salaat), pin `c77a733efad9dc986901f07236e886b0bd6642ff`.
- Subset hosting XAML Island/taskbar `AwqatSalaat.WinUI/TaskBarWidget.cs` dan interop `AwqatSalaat.Common/Interop/Native.cs` diadaptasi pada `AwqatTaskbarRenderer.cs`.
- Perubahan lokal mencakup UI/countdown, validasi/error handling, lifetime management dan penempatan. Tidak mengklaim kalkulasi sholat/lokasi atau seluruh helper Awqat diambil.
- MIT, `Copyright (c) 2023 Khiro`. [License lengkap](../src/PrayerAutomation.Taskbar.WinUI/LICENSE-Awqat-Salaat.txt) dipertahankan; lihat [provenance backend](../src/PrayerAutomation.Taskbar.WinUI/PROVENANCE.md).
- Source renderer masuk DLL portable dan dapat dipanggil pada mode native; ini bukti build statis, bukan pengamatan mode aktif pada PC pengguna.

### Microsoft Windows App SDK

- [Repository WindowsAppSDK](https://github.com/microsoft/WindowsAppSDK), initializer Foundation 2.6.4 / pin `5e215b73121e0fefad2da5b3d534321bd63bcce0`.
- Subset `UndockedRegFreeWinRT-AutoInitializer.cs` diadaptasi untuk bootstrap registration-free XAML Island: base-directory/PID environment dan `WindowsAppRuntime_EnsureIsLoaded`.
- [MIT source Microsoft](../src/PrayerAutomation.Taskbar.WinUI/LICENSE-WindowsAppSDK-Source.txt) dipertahankan. License subset source ini tidak menggantikan terms binary SDK.

## Referensi teknis dan keluarga algoritma

### TrafficMonitor — zhongyang219

- [Repository TrafficMonitor](https://github.com/zhongyang219/TrafficMonitor), pin `930f17533d6098989ebad62f210aa97d75ef174b`.
- Dihargai sebagai referensi teknis taskbar/GDI. Audit perbandingan klasik/Win11/GDI belum menghasilkan bukti positif copying/adaptation ekspresi kode: aplikasi memakai occupancy/gap allocator, bukan mekanisme upstream yang mengecilkan atau menggeser tasklist.
- Kesamaan API Windows atau header clean-room bukan bukti asal-usul sendiri. Absence copying universal tidak disertifikasi; authoring sebelum checkpoint Git tidak pulih dari history.
- [License upstream](https://github.com/zhongyang219/TrafficMonitor/blob/930f17533d6098989ebad62f210aa97d75ef174b/LICENSE) adalah **Anti 996 Version 1.0 (Draft), bukan MIT**. Kredit referensi bukan relicense atau pernyataan aplikasi otomatis turunan Anti996. Jika bukti copied source ditemukan, terms donor perlu diperiksa kembali.

### Howard Hinnant — algoritma civil-date

- [Artikel algoritma kalender](https://howardhinnant.github.io/date_algorithms.html), pin halaman `c4852d230081ba5504e60db12f5f55c57990ed0b`; [library date v3.0.1](https://github.com/HowardHinnant/date/tree/6e921e1b1d21e84a5c82416ba7ecd98e33a436d0).
- Helper Rust `days_from_civil` di `prayer.rs`/`calendar.rs` dan inverse `civil_from_days` memiliki kemiripan implementasi sangat kuat dengan keluarga algoritma tersebut, walaupun bahasa berbeda. Kami memberi penghargaan tanpa mengklaim jalur donor langsung sudah diketahui.
- Summary artikel resmi menyatakan **“Consider these donated to the public domain.”** Library/port lain dapat mempunyai MIT per-file atau terms tersendiri. Asal artikel, header library, atau intermediate port belum terkonfirmasi; jangan otomatis menetapkan MIT sebagai fakta donor atau menyatakan pelanggaran pasti.

## Dependency dan history internal

- GeoTimeZone memuat Geohash-net (Sharon Lourduraj), port Geohash-js (David Troy). Kredit/permission MIT ada pada [LICENSE GeoTimeZone lengkap](third-party/licenses/GeoTimeZone-6.1.0-LICENSE.txt); ini source di dalam dependency, bukan bukti tiga salinan langsung ke source aplikasi. Data timezone ODbL tetap terpisah.
- Python historis menggunakan dependency adhanpy. Source-copy Adhan/PrayTimes ke Rust belum terkonfirmasi; faktor Asr/rumus/default yang serupa tidak cukup sebagai bukti donor.
- Konversi Hijri Rust diport dari Python internal lama. Asal eksternal Python sebelum checkpoint pertama masih belum terungkap.

## Batas dan pencantuman

Audit November 2026 membandingkan upstream pinned, source/history lokal, membership PDB dan call-site CIL paket metadata-publisher. Hasil tidak mengeliminasi seluruh donor/versi/fork di internet dan tidak memberi clearance semua aset/data/binary.

Penghargaan ini **bukan pengganti teks lisensi**. [Inventaris third-party](THIRD_PARTY_NOTICES.md) tetap berlaku. Lisensi source utama dan pemegang copyright aplikasi belum dipilih; tidak menetapkan seluruh repository MIT. Dokumen disiapkan lokal untuk publikasi mendatang, **bukan bukti sudah diunggah ke GitHub**.
