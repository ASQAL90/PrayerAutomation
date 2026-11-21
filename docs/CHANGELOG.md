# Riwayat perubahan

## v2.4.3
- Disinkronkan nilai `DisplayVersion` pada entri uninstall setelah journal pembaruan tersimpan dan saat startup agar sesuai dengan versi biner yang sedang berjalan.
- Diselaraskan assertion harness visual dengan `AssemblyName` produk, tinggi widget 34 DIP, padding 2, dan field bantuan yang masih tersedia.
- Dinaikkan nomor versi aplikasi ke 2.4.3.
- Diperbarui contoh ID draft pada panduan kanal pembaruan internal.

## v2.4.2
- Ditambahkan panel ringkasan catatan rilis versi terbaru pada popup pembaruan.
- Ditambahkan tombol "Riwayat perubahan" pada halaman Tentang beserta jendela riwayat seluruh versi.
- Dijalankan pemulihan lock screen secara paksa pada preflight pembaruan dan dilanjutkan pembaruan meskipun verifikasi pemulihan gagal.
- Disesuaikan pemasang NSIS untuk menampilkan konfirmasi pada instalasi manual bila versi terpasang sama atau lebih baru.
- Diteruskan pembaruan otomatis /UPDATEMODE tanpa dialog konfirmasi versi.
- Dinaikkan nomor versi aplikasi ke 2.4.2.
- Diwajibkan berkas catatan rilis pada workflow Release dan dipakai sebagai body rilis.
- Status verifikasi tampilan Windows tetap visual=unverified.

## v2.4.1
- Diperbaiki kegagalan pembaruan akibat akses lintas-thread yang memicu pesan "The calling thread cannot access this object because a different thread owns it."
- Diperbaiki kesalahan "window has closed" saat jendela pembaruan ditutup lebih awal.
- Ditangkap Dispatcher UI pada thread UI sebelum callback pemulihan, pemeriksaan otomatis, status jendela, dan aksi tray dijalankan.
- Dipertahankan InstallDeltaAsync pada thread UI hingga ReleaseAndShutdown dipanggil.
- Ditempatkan Shutdown di dalam blok finally agar selalu dieksekusi.
- Dibuat ulang jendela utama saat dibuka dari tray setelah tertutup.
- Diaktifkan kembali tombol "Periksa lagi" setelah pembaruan gagal.
- Dirapikan format kode Rust agar pemeriksaan CI kembali hijau.
- Dirilis sebagai hotfix untuk perbaikan jalur pembaruan.
- Status verifikasi pembaruan dan tampilan Windows tetap update=unverified dan visual=unverified.

## v2.4.0
- Ditambahkan dataset GeoNames Indonesia offline untuk pencarian lokasi.
- Disajikan kalender 12 bulan pada aplikasi.
- Disediakan tabel KHGT dan Muhammadiyah untuk perhitungan jadwal offline.
- Diperbaiki alur gambar lock screen, termasuk penulisan berkas sementara dan pemulihan gambar semula.
- Diperbaiki pengaturan aplikasi, termasuk penjajaran tombol lock screen dan pemulihan pratinjau tema saat sumber kalender diganti.
- Istilah koreksi tanggal yang dipakai pada aplikasi adalah geser tanggal Hijriyah.
- Aset watermark silhouette dan tekstur varian C tidak diubah.
- Status verifikasi tampilan Windows tetap visual=unverified.

## v2.3.1
- Ditambahkan pembaruan delta bertanda tangan dengan manifest JSON dan berkas .sig.
- Ditambahkan pemulihan otomatis saat startup.
- Disiapkan fallback ke installer NSIS saat tanda tangan manifest tidak dapat diverifikasi.
- Status verifikasi tampilan Windows tetap visual=unverified.

## v2.3.0
- Ditanamkan tabel resmi KHGT di prayer_core.dll untuk perhitungan jadwal offline.
- Ditetapkan metode Muhammadiyah mengikuti MTT/Pedoman Hisab.
- Ditambahkan gambar lock screen dinamis berisi logo, nama sholat, dan jam.
- Disajikan kalender Hijriyah dan Masehi pada aplikasi.
- Dirilis versi 2.3.0.
- Status verifikasi tampilan Windows tetap visual=unverified.

## v2.2.1
- Diperbaiki penerbitan CI untuk arsitektur x86.
- Diterapkan penamaan berkas rilis yang dinamis.
- Dipusatkan simbol legenda kalender dengan pengaturan stretch.

## v2.2.0
- Dinaikkan nomor versi aplikasi ke 2.2.0.
- Dipusatkan simbol legenda kalender dengan bentuk vektor.
- Ditetapkan workflow rilis CI hanya dapat dipicu secara manual.

## v2.1.2
- Disinkronkan source aplikasi dengan rilis 2.1.2.
- Dirapikan baris estimasi waktu sholat pada tampilan pengaturan.
