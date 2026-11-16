# Third-party notices — Ayo Jama'ah! 2.1.0

Inventaris lokal per 11 November 2026. **Ini bukan LICENSE utama aplikasi atau clearance hukum menyeluruh.** Source asli repository ini dilisensikan MIT (lihat [`../LICENSE`](../LICENSE), Copyright (c) 2026 Muhammad Nashiruddin Al-Asqalani). Masing-masing komponen di bawah tetap tunduk pada lisensi/terms pemiliknya; lisensi MIT pada source aplikasi tidak menggantikan terms binary atau data pihak ketiga.

## Dasar pemeriksaan

- `PackageReference` dua project C#, manifest/lockfile Cargo, header source, dan [provenance WinUI](../src/PrayerAutomation.Taskbar.WinUI/PROVENANCE.md).
- Versi paket terkirim dipetakan menggunakan **`AyoJamaah.deps.json` dari portable final 2.1.0** dan manifest receipt final; versi dalam cache NuGet tidak otomatis dianggap versi terkirim.
- Metadata cached `.nuspec` untuk dependency/owner/source pin serta lisensi/NOTICE yang sudah ada. `project.assets.json` kerja tidak cukup untuk memulihkan seluruh dependency build final.
- Source dan payload memuat **14 file lisensi/NOTICE yang telah ada**: dua lisensi source adaptasi, 11 file vendor, dan satu NOTICE GeoNames. File tersebut dipertahankan, bukan disunting agar cocok dengan format repository.
- Tiga salinan lisensi tambahan di `docs/third-party/licenses/` hanya melengkapi dokumentasi repository. **Portable/installer final tidak direpack atau diubah** pada pekerjaan ini.

## Paket direct dan runtime

| Komponen | Versi terbukti | Peran | Lisensi/terms yang dibaca |
|---|---|---|---|
| GeoTimeZone | 6.1.0 | Direct; `GeoTimeZone.dll` | MIT untuk kode; ODbL untuk data boundary; atribusi Geohash-net/Geohash-js |
| TimeZoneConverter | 7.2.0 | Direct; `TimeZoneConverter.dll` | MIT untuk library; sumber mapping embedded dibahas terpisah di bawah |
| Microsoft.WindowsAppSDK.WinUI | 2.6.4 | Direct; backend WinUI dan app-local assets | Microsoft Windows App SDK Software License Terms + NOTICE |
| Microsoft.WindowsAppSDK.Foundation | 2.6.4 | Transitive; projection/bootstrap runtime | Microsoft Windows App SDK Software License Terms |
| Microsoft.WindowsAppSDK.InteractiveExperiences | 2.6.4 | Transitive; projection/native assets | Microsoft Windows App SDK Software License Terms |
| Microsoft.Web.WebView2 | 1.0.3719.77 | Transitive; loader/core/projection | LICENSE gaya BSD-3-Clause + NOTICE |
| Microsoft.NETCore.App.Runtime.win-x64 | 10.0.9 | Self-contained .NET runtime | MIT + aggregate THIRD-PARTY-NOTICES |
| Microsoft.WindowsDesktop.App.Runtime.win-x64 | 10.0.9 | Self-contained WPF/WinForms runtime | MIT; aggregate .NET notices ikut dipertahankan |
| Microsoft.Windows.SDK.NET.Ref | 10.0.26100.57 | Runtime pack; `Microsoft.Windows.SDK.NET.dll`, `WinRT.Runtime.dll` | Package metadata menunjuk Windows SDK terms; exact REDIST/runtime attribution belum ditutup |

Entry `AyoJamaah`, `AyoJamaah.Taskbar.WinUI`, dan `.Reference` pada deps adalah assembly milik aplikasi, bukan third-party terpisah. Metadata library/reference dapat berbeda dari versi produk; inventaris tidak mengubahnya.

**Batas pemetaan:** Microsoft.WindowsAppSDK.Base 2.6.4 terbukti dari dependency WinUI/cached metadata, tetapi tidak mempunyai entry library sendiri pada final deps. Seluruh app-local assets belum dipetakan ke setiap subkomponen internal. Aggregate NOTICE dipertahankan utuh; banyaknya nama komponen di NOTICE bukan bukti semuanya dikirim sebagai dependency aplikasi.

### GeoTimeZone: kode MIT, data ODbL

Source pin dari `.nuspec`: `73206b43819c89f8b6ec2926b2ca227fc783b938`.

- Pemilik kode: Matt Johnson-Pint dan Simon Bartlett.
- LICENSE juga mengakui Geohash-net oleh Sharon Lourduraj dan Geohash-js oleh David Troy; tidak ada DLL dependency terpisah untuk dua sumber itu.
- Boundary berasal dari OpenStreetMap, dikorelasikan melalui [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) oleh Evan Siroky.
- [LICENSE lengkap yang disalin](third-party/licenses/GeoTimeZone-6.1.0-LICENSE.txt) dan [sumber pinned](https://raw.githubusercontent.com/mattjohnsonpint/GeoTimeZone/73206b43819c89f8b6ec2926b2ca227fc783b938/LICENSE).
- [Teks ODbL 1.0](third-party/licenses/ODbL-1.0.txt) dan [sumber Open Data Commons](https://opendatacommons.org/licenses/odbl/odbl-10.txt).

**Lanjutan terverifikasi:** kedua resource lookup logical embedded sudah dicocokkan dengan source pin yang sama. Builder menyatakan input TBB `2025b/timezones.shapefile.zip`, tetapi exact ZIP input/tanggal snapshot OSM belum dibuktikan. [Penjelasan GeoTimeZone](GEOTIMEZONE_LICENSE_ID.md) merinci geohash, hash, kandidat `.Result`, serta ODbL §2.3 dan §4.2–4.8.

Kandidat portable metadata-publisher kini menyertakan full LICENSE GeoTimeZone/ODbL dan salinan machine-readable seluruh lookup yang logical-identik. Notice/output UI publik dan clearance kewajiban lain masih perlu ditinjau. **Portable/installer final sebelumnya tidak berubah** dan tidak memperoleh notice secara retroaktif. Jangan menyebut seluruh paket MIT-only atau otomatis menerapkan ODbL pada seluruh source aplikasi.

### TimeZoneConverter dan data mapping

Source pin: `f96c7a7d167d9ab93bb8c924fc7413b84f52010c`. Copyright library: Matt Johnson-Pint.

- [LICENSE lengkap](third-party/licenses/TimeZoneConverter-7.2.0-LICENSE.txt); [sumber pinned](https://raw.githubusercontent.com/mattjohnsonpint/TimeZoneConverter/f96c7a7d167d9ab93bb8c924fc7413b84f52010c/LICENSE.txt).
- [README upstream pada pin yang sama](https://raw.githubusercontent.com/mattjohnsonpint/TimeZoneConverter/f96c7a7d167d9ab93bb8c924fc7413b84f52010c/README.md) menyebut Unicode CLDR, IANA timezone data, Windows timezone updates, Rails ActiveSupport mapping, serta pengetahuan penulis.
- Data mapping embedded tidak memerlukan unduhan ketika dipakai. Resolusi `TimeZoneInfo` tertentu tetap bergantung pada data timezone Windows.

**Gap:** versi/snapshot dan lisensi sumber mapping individual belum dipetakan. Metadata MIT library tidak dipakai sebagai bukti clearance seluruh data asal. Full LICENSE library ditambahkan pada kandidat portable metadata-publisher, tetapi portable/installer final sebelumnya tetap tidak berubah. RuntimeInformation/ValueTuple untuk target framework lain tidak muncul pada final deps net10 dan tidak dihitung sebagai paket terkirim.

### Windows App SDK binary: pertahankan terms, bukan MIT-only

File yang sudah disertakan dan dipertahankan:

- [WinUI terms](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.winui-2.6.4-license.txt) dan [NOTICE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.winui-2.6.4-NOTICE.txt).
- [Foundation terms](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.foundation-2.6.4-license.txt).
- [InteractiveExperiences terms](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.interactiveexperiences-2.6.4-license.txt).
- [Base terms](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.base-2.6.4-license.txt) dan [aggregate NOTICE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsappsdk.base-2.6.4-NOTICE.txt).

Review persyaratan distribusi, termasuk bagian 3 dan terms pelindung yang diminta Microsoft, masih diperlukan. Menyalin notice saja bukan bukti semua syarat redistribusi telah dipenuhi. NOTICE internal yang menyebut Newtonsoft.Json tidak membuktikan paket Newtonsoft.Json terpisah ikut pada final deps.

### WebView2 dan .NET

- [WebView2 LICENSE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.web.webview2-1.0.3719.77-LICENSE.txt) dan [NOTICE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.web.webview2-1.0.3719.77-NOTICE.txt). NOTICE menyebut Antlr3.Runtime dan StringTemplate4; itu atribusi internal, bukan otomatis dua paket dependency aplikasi terpisah.
- `WebView2Loader.dll`/projection terkirim **bukan bukti browser Edge WebView2 runtime atau runtime installer ikut dikirim**.
- [.NET runtime LICENSE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.netcore.app.runtime.win-x64-10.0.9-LICENSE.TXT), [THIRD-PARTY-NOTICES](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.netcore.app.runtime.win-x64-10.0.9-THIRD-PARTY-NOTICES.TXT), dan [WindowsDesktop LICENSE](../src/PrayerAutomation.Taskbar.WinUI/THIRD_PARTY_LICENSES/microsoft.windowsdesktop.app.runtime.win-x64-10.0.9-LICENSE).
- NuGet runtime source pin tercatat `901ca941248413c79832d2fdbd709da0c4386353`; lisensi .NET Foundation and Contributors dan notice tambahan dipertahankan.

### Windows SDK projection dan WinRT.Runtime

Final deps memetakan keduanya ke runtime pack Microsoft.Windows.SDK.NET.Ref 10.0.26100.57, bukan paket Microsoft.Windows.CsWinRT terpisah. Metadata SDK menunjuk [Windows SDK terms](https://download.microsoft.com/download/0/F/F/0FF2B061-47DD-4F55-89B6-FD1D8C44F14D/sdk_license.rtf).

Repository [CsWinRT saat diperiksa](https://raw.githubusercontent.com/microsoft/CsWinRT/master/LICENSE) berlisensi MIT, tetapi LICENSE branch tersebut **belum diikat ke source pin exact DLL final**. Ini referensi investigasi, bukan pin rilis. Notice/REDIST untuk projection dan runtime exact belum ditutup; tidak menerapkan source MIT WindowsAppSDK secara otomatis pada keduanya.

## Source yang diadaptasi

| Komponen | Pin/versi provenance | Lingkup | Lisensi |
|---|---|---|---|
| Awqat Salaat | `c77a733efad9dc986901f07236e886b0bd6642ff` | Subset hosting/native taskbar di `AwqatTaskbarRenderer.cs`; bukan seluruh aplikasi upstream | [MIT, copyright Khiro](../src/PrayerAutomation.Taskbar.WinUI/LICENSE-Awqat-Salaat.txt) |
| WindowsAppSDK registration-free bootstrap | 2.6.4 menurut provenance lokal | Subset bootstrap untuk XAML Island | [MIT, Microsoft](../src/PrayerAutomation.Taskbar.WinUI/LICENSE-WindowsAppSDK-Source.txt) |

[Provenance](../src/PrayerAutomation.Taskbar.WinUI/PROVENANCE.md) merinci perubahan lokal dan asal hosting. Lisensi source bootstrap tidak menggantikan terms paket binary SDK di atas.

## Dataset dan konten

**GeoNames:** derived `cities1000` locality dataset, snapshot 22 Oktober 2026 UTC, **CC BY 4.0**. Source `cities1000`, admin1, dan countryInfo; perubahan berupa filter feature-class P, pemilihan kolom, normalisasi whitespace, serta kompresi TSV. [NOTICE lengkap](../src/PrayerAutomation.UI/Data/geonames/NOTICE.txt) sudah ikut source dan portable; attribution, snapshot, perubahan, dan [license link](https://creativecommons.org/licenses/by/4.0/) tersedia. Label permukiman terdekat bukan alamat jalan atau bukti posisi tepat.

**Ringkasan makna hadis:** `WidgetMarqueeContent.cs` menyimpan teks ringkas Indonesia dan tautan referensi sunnah.com; bukan dependency jaringan dan bukan salinan seluruh situs. Atribusi referensi tidak otomatis merupakan izin lisensi terjemahan/konten. Kepemilikan/asal-usul ringkasan perlu dikonfirmasi sebelum menetapkan lisensi aplikasi utama.

**Logo, geometri ikon, dan PNG:** keputusan desain dan master lokal tersedia, tetapi audit ini tidak menetapkan pemegang hak atau clearance seluruh aset visual. Jangan menyimpulkan semua aset milik aplikasi atau semua gambar berlisensi MIT dari lisensi library UI.

## Toolchain dan referensi historis

- **Microsoft.Windows.SDK.BuildTools 10.0.26100.4654 / BuildTools.MSIX 1.7.251221100:** dependency/cache metadata WinUI/Base, Windows SDK terms. Tidak muncul sebagai package runtime pada deps final. Versi tool yang benar-benar dieksekusi perlu diikat ke bukti build, bukan disimpulkan hanya dari cache.
- **Inno Setup 6.7.3:** versi engine tercantum pada log compile installer final dan dipin dengan hash compiler dalam receipt. Compiler tidak disertakan pada portable, tetapi engine installer merupakan bagian yang perlu ditinjau untuk distribusi installer. [Lisensi versi 6.7.3](https://raw.githubusercontent.com/jrsoftware/issrc/is-6_7_3/license.txt) mengizinkan penggunaan/redistribusi dengan empat syarat. Acknowledgment dokumentasi dihargai, bukan diwajibkan oleh teks tersebut. Audit seluruh subkomponen engine installer belum lengkap.
- **Rust/Cargo/GNU Windows toolchain:** kedua Cargo.lock hanya memuat `prayer-core 0.1.0`; `[dependencies]` kosong. Tidak ditemukan third-party crates, **bukan berarti native DLL bebas komponen third-party**. Versi/pin, Rust std, serta kemungkinan runtime statis GCC/MinGW dan attribution-nya belum diaudit. Tidak menambahkan lisensi toolchain dari asumsi.
- **TrafficMonitor:** referensi teknis pada pin `930f17533d6098989ebad62f210aa97d75ef174b`; [LICENSE upstream](https://raw.githubusercontent.com/zhongyang219/TrafficMonitor/930f17533d6098989ebad62f210aa97d75ef174b/LICENSE) adalah **Anti 996 License Version 1.0 (Draft)**, bukan MIT/BSD. Audit lanjutan discovery/occupancy klasik, Win11, GDI dan history belum menemukan bukti positif source-copy/adaptation. Renderer GDI lama tidak masuk kedua DLL publisher yang diperiksa; header clean-room bukan sertifikasi independensi. [Penghargaan dan asal source](SOURCE_ORIGINS_ID.md) tetap mengakui referensi teknis, termasuk lintas bahasa. Jika bukti kode turunan muncul, terms/notice upstream perlu dipertahankan; jangan mengubahnya menjadi MIT.
- **`igcmdWin10.exe` dalam Git history:** tidak ada pada HEAD/source sekarang; asal dan izin redistribusi belum diperiksa. Tidak dijalankan dan tidak dimasukkan ke paket/dokumen sebagai dependency aktif. [Audit history](REPOSITORY_AUDIT_ID.md) mencatat batasnya.

## Syarat yang belum ditutup sebelum distribusi/publikasi penuh

1. Pilih LICENSE source utama setelah hak atas kode/aset/konten dikonfirmasi pemilik.
2. Identifikasi snapshot GeoTimeZone/TBB/OSM dan review kewajiban ODbL serta mapping data TimeZoneConverter.
3. Tutup notice/REDIST Windows SDK projection/WinRT exact dan syarat distribusi binary Windows App SDK.
4. Audit native toolchain/static-link dan engine installer; jangan menganggap Cargo.lock kosong menyelesaikan semuanya.
5. Pada **rilis baru yang diotorisasi**, masukkan notice yang masih kurang ke distribusi, regenerasi payload/hash, build/test, lalu verifikasi ulang. Jangan menyunting diam-diam folder paket final yang sudah diterima.
6. Review hak redistribusi binary lama dan informasi privat dalam history sebelum publikasi history.

Saat ini yang tersedia adalah inventaris berbukti dengan gap terbuka. Tidak ada pengajuan SignPath, signing, root LICENSE yang dipilih otomatis, atau jaminan seluruh komponen siap direlisensi/diterbitkan.
