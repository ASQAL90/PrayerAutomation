# Riwayat perubahan

## v2.7.0
- Pemilih lokasi lewat peta: klik ikon peta di kanan kolom Nama Lokasi untuk membuka peta dan menaruh titik.
- Pin merah menandai titik yang dipilih. Tombol Tetapkan titik mengisi nama lokasi, koordinat, ketinggian, zona waktu, dan metode hitung.
- Peta bisa digeser dan di-zoom dengan scroll di posisi kursor. Ada tombol untuk menuju lokasi saat ini dengan GPS atau perkiraan jaringan.
- Saat peta dibuka, pin langsung berada di lokasi yang sedang dipakai di form pada zoom 15. Nama lokasi yang sudah ada ikut tampil di kotak pencarian.
- Tile OpenStreetMap langsung dimuat dan bisa dimatikan lewat centang. Tanpa tile, peta tetap memakai grid dan data lokal.
- Daftar hasil pencarian langsung menutup setelah lokasi dipilih.
- Badge zoom, tombol zoom, kotak pencarian, dan checkbox tile dirapikan agar sesuai tema.
- Atribusi OpenStreetMap dan tautan lisensinya tampil di jendela peta.
- Permintaan tile memakai HTTPS, User-Agent aplikasi, batas ukuran respons, dan cache sementara. Alamat IP tetap terlihat oleh server tile selama tile online aktif.
- Aplikasi tetap tanpa WebView2, dan installer maupun versi portable tetap berjalan sendiri.

## v2.6.0
- Tab Dzikir & Doa berisi dzikir pagi, dzikir petang, dan kumpulan doa dari Rumaysho.Com, lengkap dengan teks Arab, transliterasi, arti, dan tautan sumber.
- Pengingat baru untuk libur nasional dan Id, puasa sunnah, larangan puasa, peringatan hari besar Islam, dzikir pagi, dan dzikir petang. Jamnya bisa diatur sendiri.
- Pengingat yang jatuh pada waktu sama digabung dalam satu popup. Popup tambahan dipisah dari popup sholat dan ditata agar tidak bertabrakan.
- Tombol untuk menghentikan pengingat hari itu dan Ingatkan lagi. Jeda pengulangan dan jumlah maksimalnya bisa diatur.
- Kalender Hijriyah menyorot tanggal hari ini saat pertama dibuka, sama seperti kalender Masehi.
- Pengaturan dan tab terakhir tidak hilang lagi saat jendela ditutup ke tray, diminimalkan, atau keluar.
- Warna tautan dzikir dan doa mengikuti tema, termasuk saat kursor diarahkan.
- Checkbox paket data lengkap langsung tersimpan dan menampilkan tanggal paket yang terpasang.
- Tanggal dan jam di status serta isi pengingat memakai format lokal yang mudah dibaca.
- Kategori pengingat dirapikan supaya hari puasa tidak muncul sebagai hari besar, dan sebaliknya.
- Teks bantuan di Pengaturan diringkas agar tidak terlalu penuh.
- Dua doa hujan belum punya transliterasi di sumbernya, jadi kolom Latinnya sengaja dikosongkan.

## v2.5.4
- Jendela Riwayat perubahan tidak lagi memotong kalimat panjang.
- Tanda Markdown seperti bintang dan backtick tidak ikut tampil di teks riwayat.
- Pratinjau catatan rilis di popup pembaruan sekarang muat teks yang lebih panjang.

## v2.5.3
- Teks riwayat perubahan tidak lagi terpotong pada batas 220 karakter.
- Aplikasi hanya memuat prayer_core dari folder aplikasi. Berkas kosong, symlink, dan berkas di luar folder aplikasi ditolak.
- Ukuran respons pembaruan dibatasi sebelum diperiksa, baik JSON rilis, manifest, maupun tanda tangan.
- Parser catatan rilis dibuat agar tidak bisa dipaksa bekerja berat oleh teks yang aneh.
- Nama berkas di manifest pembaruan diperiksa, dan nama yang berbahaya ditolak.
- Untuk versi 2.4.0 ke atas, pembaruan berhenti kalau manifest atau tanda tangan gagal diverifikasi. Tidak ada lagi fallback ke aset tanpa tanda tangan.

## v2.5.2
- Uninstaller menanyakan apakah pengaturan dan data pengguna disimpan atau dihapus. Pilihan awal adalah menyimpan data.
- Nomor versi aplikasi dan installer sekarang diambil dari satu tempat, jadi keduanya tidak bisa berbeda.
- Mode pembaruan senyap hanya aktif kalau /UPDATEMODE dikirim sebagai argumen tersendiri.
- Argumen /D= ditolak agar folder instalasi tidak bisa diarahkan ke luar folder yang diizinkan.
- Data pengguna hanya dihapus kalau pengguna memilihnya saat uninstall. Jurnal pemulihan lock screen tidak pernah dihapus.

## v2.5.1
- Tersedia tiga installer: universal (x64 dan x86), x64, dan x86. Updater memilih installer sesuai arsitektur Windows, lalu memakai versi universal kalau yang sesuai tidak tersedia.
- Ukuran paket lebih kecil karena komponen runtime yang tidak dipakai dipangkas. Aplikasi tetap berjalan sendiri tanpa perlu memasang .NET Runtime.
- Menu tray memakai tema aplikasi, termasuk warna latar, hover, dan garis pemisah.
- Tray icon tidak lagi bergantung pada WinForms dan tampil konsisten di tema terang maupun gelap.

## v2.5.0
- Data lokasi offline diperluas dengan daftar kota dunia dan data Indonesia dari Kemendagri yang dipadukan dengan GeoNames. Ketinggian permukiman memakai data GeoNames dan sampel Copernicus bila tersedia.
- Paket data global lengkap bisa diunduh dari Pengaturan, dengan progres, pembatalan, dan cache permanen. Tanpa paket ini aplikasi tetap memakai data bawaan.
- Nominatim online tersedia sebagai pilihan dan default-nya mati. Kalau diaktifkan, koordinat bisa dilengkapi lewat reverse geocoding dan nama tempat bisa dicari sebagai saran.
- Instalasi baru mencoba mendeteksi lokasi otomatis: lokasi perangkat atau GPS lebih dulu, lalu perkiraan jaringan, lalu lokasi bawaan.
- Preset instalasi baru mengaktifkan widget desktop, panel taskbar, mulai tersembunyi, jalankan saat masuk Windows, pemeriksaan pembaruan, dan kunci layar saat waktu sholat.
- Pencarian lokasi sekarang membedakan nama yang mirip, seperti Cibarusa dan Cibarusah.
- Widget dan panel taskbar langsung aktif saat dicentang atau disimpan, tanpa perlu keluar-masuk aplikasi.
- Panel taskbar tidak lagi hilang setelah deteksi lokasi otomatis saat instalasi baru.
- Versi yang tampil di Control Panel dan Programs & Features ikut diperbarui setelah pembaruan.
- Paket data global diperiksa dengan SHA-256 sebelum dipakai. Kalau unduhan atau manifest gagal, aplikasi tetap memakai data bawaan.

## v2.4.3
- Versi yang tampil di Control Panel dan Programs & Features sekarang ikut diperbarui setelah pembaruan.
- Semua cara mengisi lokasi, baik manual, data lokal, perkiraan jaringan, maupun GPS, sekarang mengisi nama, ketinggian, zona waktu, koordinat, dan metode hitung dengan lengkap.
- Karakter panah pada halaman selamat datang installer diganti agar tidak tampil rusak.
- Escape pada pengaman lock screen diperbaiki.

## v2.4.2
- Popup pembaruan menampilkan ringkasan catatan rilis versi terbaru.
- Halaman Tentang punya tombol Riwayat perubahan untuk melihat catatan semua versi.
- Pemulihan lock screen tetap dicoba sebelum pembaruan. Kalau verifikasi pemulihan gagal, pembaruan tetap dilanjutkan.
- Installer manual memberi konfirmasi kalau versi yang terpasang sama atau lebih baru.
- Pembaruan otomatis tidak lagi berhenti di dialog konfirmasi versi.

## v2.4.1
- Pembaruan tidak lagi gagal karena akses lintas-thread saat berpindah dari jendela pembaruan ke proses update.
- Menutup jendela pembaruan lebih awal tidak lagi memunculkan error "window has closed".
- Jendela utama bisa dibuka lagi dari tray setelah ditutup.
- Tombol Periksa lagi aktif kembali setelah pembaruan gagal.

## v2.4.0
- Data lokasi Indonesia dari GeoNames bisa dipakai untuk mencari tempat tanpa internet.
- Kalender 12 bulan tersedia di aplikasi.
- Perhitungan jadwal offline memakai tabel KHGT dan Muhammadiyah.
- Alur gambar lock screen diperbaiki, termasuk penulisan berkas sementara dan pemulihan gambar semula.
- Tombol lock screen di Pengaturan dirapikan, dan pratinjau tema kembali normal saat sumber kalender diganti.

## v2.3.1
- Pembaruan delta bertanda tangan dengan manifest JSON dan berkas tanda tangan.
- Pemulihan otomatis saat aplikasi dibuka lagi setelah pembaruan terputus.
- Setiap manifest pembaruan punya tanda tangan yang diperiksa sebelum dipakai. Kalau verifikasi gagal, aplikasi memakai installer NSIS biasa.

## v2.3.0
- Tabel resmi KHGT ditanam di prayer_core untuk perhitungan jadwal tanpa internet.
- Metode Muhammadiyah mengikuti MTT/Pedoman Hisab.
- Gambar lock screen dinamis menampilkan logo, nama sholat, dan jam.
- Kalender Hijriyah dan Masehi tersedia di aplikasi.

## v2.2.1
- Build 32-bit (x86) kembali bisa diterbitkan.
- Nama berkas rilis mengikuti versi secara otomatis.
- Simbol legenda kalender dirapikan.

## v2.2.0
- Simbol legenda kalender sekarang berada di tengah dan tetap tajam di berbagai ukuran layar.
- Rilis hanya bisa dijalankan manual agar tidak terbit tanpa sengaja.

## v2.1.2
- Baris estimasi waktu sholat di halaman Pengaturan dirapikan agar lebih mudah dibaca.
