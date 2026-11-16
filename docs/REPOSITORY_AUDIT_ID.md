# Audit repository lokal — Ayo Jama'ah!

Audit read-only pada 11 November 2026 terhadap baseline commit `9acebfffa17616c717c7ab67c3ef6486738211a9`, sebelum perubahan dokumentasi ini. Tidak ada upload/integrasi GitHub, instalasi, perubahan runtime/profil/registry, staging, commit, penghapusan, atau rewrite history.

## Ringkasan bukti

| Pemeriksaan baseline | Hasil |
|---|---|
| Working tree awal | Bersih; remote Git tidak dikonfigurasi |
| File tracked | 168; seluruhnya diinventarisasi dan di-hash |
| Plaintext tracked | 165; tiga aset biner tidak dipindai sebagai teks |
| History seluruh branch/tag reachable | 28 commit, 233 blob unik |
| Plaintext history | 227 blob UTF-8/teks dan satu XML UTF-16 diperiksa |
| History biner tersisa | Lima blob tidak mendapat secret scan plaintext |
| Kandidat explicit prompt AI | Tidak ditemukan oleh pola scan |
| Kandidat kredensial hardcoded/private endpoint | Tidak ditemukan oleh pola scan |
| Output/cache ignored | 701 file; isi profil/IDE/runtime tidak dibuka |

Angka adalah snapshot **sebelum** dokumen baru ditambahkan. Scan mencari pola master/system prompt, role/instruction marker, token/private-key/credential assignment, dan endpoint private; hasil negatif bukan jaminan tidak ada rahasia, data pribadi, atau teks instruksi dalam seluruh format.

## Klasifikasi file tracked

| Kelompok | File baseline | Keputusan |
|---|---:|---|
| Aplikasi, backend, Shared, lisensi/data terkait | 68 | Dipertahankan |
| Rust source/manifest/lockfile | 7 | Dipertahankan |
| Test dan fixture | 60 | Dipertahankan; tingkat isolasi berbeda |
| Installer/generator/fixture | 10 | Dipertahankan; include aktif dan historis dibedakan |
| Dokumentasi/evidence | 20 | Dipertahankan; panduan aktif dipisahkan dari riwayat melalui indeks |
| Artefak root pendukung | 3 | `.gitignore`, ikon aplikasi, dan gambar lock screen dipertahankan |

### Catatan pengembangan bukan otomatis prompt AI

Scan menemukan 55 baris bertanda istilah pengembangan/review dan 79 baris dengan path lokal pada baseline. Kecocokan meliputi laporan SPEC/QUALITY, receipt, label fixture, `.spec` pada `.gitignore`, dan bahkan teks lisensi vendor. Itu **bukan bukti instruksi AI tersembunyi**. Bukti dan lisensi tidak dihapus hanya karena kata tersebut muncul.

README lama mencampurkan rilis sekarang dengan laporan kandidat 2.0.0 dan status review lama. README diperbarui menjadi pintu masuk 2.1.0; [indeks dokumentasi](README.md) menempatkan catatan lama sebagai riwayat. Salinan README sebelum perubahan disimpan dan di-hash di luar working tree; versi lama juga tetap di Git history.

## Kandidat yang tidak boleh langsung dihapus

| Path | Temuan | Keputusan pada audit ini |
|---|---|---|
| `src/PrayerAutomation.UI/NativeTaskbarRenderer.cs` | Dikeluarkan dari compile UI; backend aktif memakai WinUI | Simpan untuk perbandingan/fixture historis |
| `src/PrayerAutomation.UI/NominatimReverseGeocodingService.cs` | Dikeluarkan dari compile; field lama masih ada untuk kompatibilitas | Simpan; tidak disebut fitur online aktif |
| `installer/AcceptedPayload.iss` | Tidak di-include oleh source installer aktif; include aktif adalah `AyoJamaahPayload.iss` | Simpan sebagai pin historis; jangan dipakai mengganti include aktif |
| `src/rust-core/Cargo.lock` | Ada lockfile root workspace dan nested | Simpan; jangan mengubah struktur build hanya untuk kerapian |
| `docs/evidence/`, dokumen progress/acceptance lama | Bukti historis, sebagian mengandung path lokal | Simpan dan labeli sebagai riwayat; publikasi memerlukan review pemilik |

## Output/cache yang masih ada secara fisik

`git status` bersih tidak berarti output build hilang dari disk. Baseline mencatat:

- `dist/`: 485 file ignored, berisi runtime lama. **Tidak dihapus**, karena keterkaitan proses/startup tidak diverifikasi pada audit ini.
- `target/`: 28 file ignored. Termasuk DLL native yang diperlukan build/fixture; tidak dihapus.
- `src/PrayerAutomation.UI/obj/`, backend/test `obj/`, `.vs`, serta Python bytecode: output/cache ignored. Inventaris metadata dibuat tanpa membuka state privat.
- Direktori `bin/` dapat masih ada walaupun tidak berisi file terinventarisasi; tidak dihapus.

Tidak ada izin cleanup massal. Penghapusan di kemudian hari memerlukan daftar target, bukti tidak dipakai, dan persetujuan; jangan menghapus source, profil, jurnal recovery, atau history.

## Hal yang masih perlu ditinjau sebelum publikasi

1. **Path lingkungan pengembang.** Sembilan file tracked baseline menyebut nama home pengguna, terutama runner/fixture dan evidence. Beberapa runner juga bergantung pada tooling di luar repository. Nilai privat tidak disalin ke laporan publik ini; lokasi temuan rinci ada pada bukti audit lokal. Normalisasi runner adalah perubahan source terpisah dan tidak dilakukan.
2. **Preset lokasi dan kontak pembuat.** `AppSettings.cs` berisi preset koordinat/nama lokasi; file About memuat kontak. Pemilik perlu memutuskan apakah data tersebut memang ditujukan untuk publik. Audit tidak mengubah default aplikasi atau menyatakan itu posisi fisik pengguna.
3. **History bukan hanya source sekarang.** History masih memuat `igcmdWin10.exe` dan XML task lama. Binary historis belum mendapat pemeriksaan lisensi/secret plaintext, dan XML memuat path lokal. Menghapus file dari HEAD tidak menghapusnya dari history. Tidak ada rewrite history atau clearance redistribusi binary lama.
4. **Lisensi aplikasi utama: MIT** (lihat [`../LICENSE`](../LICENSE)). [Inventaris third-party](THIRD_PARTY_NOTICES.md) membedakan lisensi vendor dari hak atas source aplikasi.
5. **Dokumen rinci belum semuanya sinkron.** Panduan program lama masih menjelaskan online lookup dan perilaku tutup sebelum popup konfirmasi baru. Indeks menandainya, bukan mempromosikannya sebagai acceptance 2.1.0.

## Pemeriksaan lanjutan: `igcmdWin10.exe`

Pada pemeriksaan lanjutan tanggal 11 November 2026:

- Metadata PE binary history mengidentifikasi **ImageGlass commands for Windows 10**, pembuat Duong Dieu Phap, versi `8.8.8560.41892`; bukan dependency aplikasi C# + Rust sekarang. Metadata tersebut bukan bukti lisensi atau validitas signature binary.
- File sudah ditambahkan pada commit `5b2d930a7c81c0da1bf6b767f299a2edaffa9018` dan dihapus dari HEAD pada commit `5460f9cfa83fd4af76b8bbbbe4d4d4a26f6ca467` tanggal 2 November 2026.
- Scan fisik proyek, source build/installer, final deps dan portable tidak menemukan file atau pemanggilan aktif. Referensi yang tersisa adalah dokumentasi arsip Python dan penjelasan bahwa jalur C# tidak memakainya.
- Satu salinan di arsip legacy Python ditemukan cocok byte/hash dengan Git blob `7f0a7f23521841cc5a364d1ebcd4dac2b6f43081`. Setelah pemilik secara khusus memilih **hapus salinan arsip saja**, file tersebut dihapus dan ketiadaannya dibaca kembali. SHA-256 historis: `f6e629258df9f51c91bac2c9ab81274be4018a2cf0bcd167b231fd4f2e18f75c`; ukuran 219424 byte.
- Manifest arsip asli tidak ditulis ulang. **Arsip legacy kini sengaja tidak lengkap satu binary**; receipt penghapusan tambahan disimpan di luar repository. [Panduan arsip Python](LEGACY_PYTHON_ID.md) mencatat batas ini.
- Git history dan HEAD tetap dipertahankan; byte binary masih dapat diperoleh dari blob historis. Ini bukan penghapusan binary dari seluruh history dan bukan clearance redistribusinya.

Pemeriksaan tersebut tidak membaca profil/registry privat, menjalankan helper, atau membuktikan penggunaan pada semua instalasi/PC lain. Tidak ada proses `igcmdWin10.exe` teramati saat pemeriksaan, tetapi itu sendiri bukan jaminan tidak pernah dipanggil pada waktu lain.

## Batas kesimpulan

**Yang terbukti:** inventaris baseline, hash file, cakupan scan dan temuan lokasi, serta klasifikasi aktif/historis yang dicocokkan dengan project/installer source.

**Yang tidak terbukti:** bebas semua data privat, malware, atau instruksi AI pada biner; hak redistribusi seluruh history; kemampuan build/test pada PC lain; kondisi startup/profil pengguna; acceptance runtime Windows.

Bukti rinci (`BASELINE.json`, `AUDIT-RAW.json`, `AUDIT-ENCODING.json`, dan arsip README) disimpan di luar working tree Git. Tidak perlu mengunggah bukti lokal tersebut bersama source. Repository **belum dinyatakan siap publikasi penuh** hanya karena scan pola tidak menemukan kredensial/prompt.
