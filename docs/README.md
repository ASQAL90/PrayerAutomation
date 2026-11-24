# Indeks dokumentasi Ayo Jama'ah!

## Mulai di sini — ringkasan 2.5.4

> **Catatan 2.5.4:** stable terbaru adalah `v2.5.4`. Rilis ini merapikan tampilan riwayat perubahan yang sebelumnya terpotong dan tetap membawa seluruh pengerasan dari 2.5.2/2.5.3. Catatan versi lama di bawah tetap berlaku sebagai riwayat.
>
> **Catatan 2.5.1:** stable 2.5.1. Rilis ini memfokuskan optimasi ukuran paket self-contained (Tier 1 prune payload, Tier 2 tray native `Shell_NotifyIconW` tanpa WinForms) dan menyediakan tiga varian installer (universal/x64/x86). Angka versi sebelumnya di dokumen lain adalah riwayat.

- [README aplikasi](../README.md): fitur, penggunaan, build, dan batas pengujian saat ini.
- [Inventaris third-party](THIRD_PARTY_NOTICES.md): komponen yang dipakai source/paket sekarang dan kewajiban atribusinya.
- [Penghargaan dan asal source](SOURCE_ORIGINS_ID.md): referensi teknis dan algoritma lintas bahasa yang dipakai source sekarang.
- [Paket distribusi lokal](DISTRIBUTION_ID.md): riwayat paket lokal dan status unsigned; unduhan draft terbaru memakai aset rilis v2.4.2, stable terbaru tetap v2.4.1.
- [GeoTimeZone](GEOTIMEZONE_LICENSE_ID.md): snapshot logical terikat, input TBB yang dinyatakan, batas lookup dan kewajiban ODbL.
- [Metadata publisher](PUBLISHER_METADATA_ID.md): hasil PE, identitas upgrade yang dipertahankan, dan perbedaan metadata/signing.
- [Penandatanganan pembaruan](UPDATE_SIGNING_ID.md): kunci manifest delta, secret CI, dan aset pembaruan rilis 2.4.2.
- [Audit repository lokal](REPOSITORY_AUDIT_ID.md): source/history, output build, catatan internal, dan batas pemeriksaan.
- [Batas pemulihan lock screen](LOCKSCREEN_LIMITATIONS_ID.md): keterbatasan yang belum diselesaikan; bukan panduan bypass keamanan Windows.

## Panduan rinci — sebagian masih perlu sinkronisasi

Dokumen berikut dipertahankan untuk referensi, **bukan seluruhnya instruksi rilis 2.2.1**. Beberapa mencampurkan status lama dengan tambahan branding baru. Gunakan README saat ini untuk nama paket dan alur aktif.

- [Panduan program](prompts/PROGRAM_DOCUMENTATION_ID.md): masih memuat lookup nama online lama, alur tutup/preview sebelum konfirmasi baru, dan referensi paket 2.0.0.
- [Installer](INSTALLER_ID.md): jalur NSIS; snapshot lokal 2.2.1, rilis draft 2.4.2 dibangun CI, stable terbaru tetap v2.4.1.
- [Persiapan UI/UX](UI_ACCEPTANCE_ID.md): bahan perencanaan uji, bukan hasil acceptance baru.
- [Panduan build/fixture installer](../installer/README_ID.md): banyak path/receipt lama bersifat lokal; jangan langsung menjalankan semua perintahnya.
- [UnsavedChangesSmoke](../tests/UnsavedChangesSmoke/README.md): rincian fixture dan bukti slice; path lokal serta status commit di akhir adalah snapshot historis.

## Riwayat pengembangan — bukan rilis atau acceptance baru

Dokumen riwayat dikelompokkan: spesifikasi/prompt di [`prompts/`](prompts/), catatan progres di [`progress/`](progress/). Isi dan tanggalnya tidak diubah.

- [PRD awal](prompts/PRD.md)
- [Progress 23 Oktober](progress/PROGRESS_2026-10-23_ID.md)
- [Verifikasi 23 Oktober](VERIFICATION_ID.md)
- [Milestone 30 Oktober](progress/MILESTONE_2026-10-30_ID.md)
- [Progress 2 November](progress/PROGRESS_2026-11-02_ID.md)
- [Arsip Python](LEGACY_PYTHON_ID.md)
- [Flowchart program lama](program_full_flowchart.html)
- [Flowchart lock screen lama](lockscreen_restore_flow.html)

Tanggal, hitungan test, hash, dan status di dokumen historis hanya berlaku pada artefak/versi yang disebut. Hasil lama tidak membuktikan bahwa rilis sekarang lolos UI/UX, instalasi, atau pemulihan visual Windows.
