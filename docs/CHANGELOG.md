# Riwayat perubahan

## v2.7.0
- Ditambahkan ikon peta inset di ujung kanan kolom Nama Lokasi, terpisah dari tombol perkiraan jaringan dan GPS.
- Klik ikon peta membuka pemilih lokasi dengan pencarian offline dan opsional Nominatim, grid koordinat, marker, serta zoom.
- Klik peta atau pilih hasil pencarian menaruh titik sementara; Tetapkan titik mengisi form dan menutup jendela, sedangkan Batal/Esc tidak mengubah form.
- Tile OpenStreetMap hanya dimuat setelah izin eksplisit dan tombol Muat peta online; atribusi ODbL tampil di UI dan notifikasi pihak ketiga.
- Tetap self-contained tanpa WebView2; penyimpanan, popup saran nama, default offline, dan logika pengingat tidak diubah.

## v2.6.0
- Ditambahkan tab Dzikir & Doa berisi dzikir pagi, dzikir petang, dan kumpulan doa dari Rumaysho.Com; tiap entri memuat Arab, transliterasi Latin, dan arti dengan kredit di bagian atas serta tautan sumber per entri. Arab tidak diubah; judul doa memakai judul deskriptif buatan dan tampil sebagai header yang lebih besar.
- Ditambahkan pengingat libur nasional & Id, puasa sunnah, larangan puasa/Tasyrik, peringatan Islam, dzikir pagi, dan dzikir petang dengan jam yang bisa diatur sendiri. Puasa sunnah dan larangan puasa mengingatkan untuk esok hari pukul `16.00`; libur nasional & Id, peringatan Islam, dan dzikir pagi pukul `08.10`; dzikir petang pukul `17.00`.
- Dikoreksi kategori pengingat: Ayyamul Bidh, puasa Senin/Kamis, Tasu'a, Asyura, Arafah, dan puasa awal Zulhijah hanya masuk pengingat puasa sunnah besok, bukan hari besar Islam hari ini; Idulfitri/Iduladha masuk kategori libur nasional & Id; Hari Tasyriq masuk larangan puasa besok.
- Jadwal pengingat tambahan yang bersamaan digabung jadi satu popup; jadwal berbeda tetap terpisah. Popup pengingat tambahan terpisah dari popup sholat. Maksimal 3 popup terlihat per layar, dan popup baru masuk antrean bila ruang vertikal tidak cukup sehingga tidak bertabrakan; popup sholat tetap auto-close 12 detik.
- Diperbaiki kalender Hijriyah agar menyorot tanggal hari ini saat pertama dibuka dan mewarnai ulang cell terpilih seperti kalender Masehi; legenda Masehi/Hijriyah disamakan menjadi `Hari libur / Id`, `Puasa sunnah`, `Larangan puasa / Tasyrik`, serta `Peringatan Islam`.
- Diringkas teks bantuan Pengaturan: cukup esensi pengingat, tanpa detail jam kantor, jam default, atau penjelasan hari besar tertentu.
- Ditambahkan tombol hentikan permanen dan ingat lagi pada popup pengingat, dengan jeda ulang dan batas maksimal yang bisa diatur; perulangan dihitung sejak jadwal terlewat.
- Diperbaiki warna hyperlink dzikir/doa agar mengikuti tema dan tetap kontras.
- Diperbaiki penyimpanan pengaturan saat jendela ditutup ke systray, diminimalkan, atau keluar; tab terakhir kini dipulihkan saat aplikasi dibuka lagi. Menu systray memuat `Pengaturan` dan shortcut `Dzikir & Doa`; popup dzikir memiliki tombol buka ke sub-tab dzikir pagi/petang sesuai konteks.
- Diperbaiki checkbox paket data lengkap agar tersimpan langsung dan status unduhan menampilkan tanggal snapshot yang terpasang.
- Diperbaiki format tanggal dan jam pada status serta isi pengingat memakai waktu lokal yang mudah dibaca; log INFO tetap memakai ISO 8601 lokal dengan offset.
- Dinaikkan nomor versi aplikasi dan installer ke 2.6.0.

## v2.5.4
- Teks riwayat perubahan tidak lagi dipotong pada batas 220 karakter di jendela About maupun panel pratinjau pembaruan; Markdown decoration dibersihkan agar kalimat mudah dibaca.
- Catatan rilis 2.5.2 dan 2.5.3 dipecah menjadi satu gagasan per baris sehingga tidak ada kalimat terpotong.
- Dinaikkan nomor versi aplikasi dan installer ke 2.5.4 agar perbaikan tampilan ini masuk paket rilis resmi.

## v2.5.3
- Pemuatan `prayer_core` kini memakai resolver P/Invoke yang dipasang sebelum panggilan native pertama.
- Resolver hanya menerima satu berkas di `AppContext.BaseDirectory` dengan nama platform yang benar; berkas kosong dan reparse point ditolak.
- Bila pustaka tidak valid, resolver melempar `DllNotFoundException` tanpa fallback ke pencarian bawaan.
- Respons pembaruan dibatasi sebelum diverifikasi: JSON rilis 1 MiB, manifest 512 KiB, dan tanda tangan 16 KiB.
- Batas diperiksa dari `Content-Length` dan ditegakkan lagi saat streaming, sehingga endpoint yang rusak tidak bisa menghabiskan memori.
- Parser catatan rilis memakai `RegexOptions.NonBacktracking` dan body dibatasi 64 KiB sebelum diurai.
- Nama aset divalidasi sebagai nama berkas polos; manifest dengan nama aset berbahaya ditolak.
- Rilis 2.4.0 ke atas bersifat fail-closed bila manifest atau tanda tangan gagal diverifikasi, tanpa fallback ke aset unsigned.
- Dinaikkan nomor versi aplikasi dan installer ke 2.5.3 agar seluruh perbaikan masuk paket rilis; isi v2.5.2 tetap menjadi riwayat.

## v2.5.2
- Nomor versi hanya punya satu sumber: `<Version>` di `src/PrayerAutomation.UI/PrayerAutomation.UI.csproj`.
- Skrip installer membaca nilai itu untuk nama berkas keluaran dan meneruskannya ke NSIS sebagai `-DAPP_VERSION`/`-DAPP_VERSION_QUAD`.
- `AyoJamaah.nsi` tidak lagi menyimpan versi sendiri dan menolak kompilasi langsung tanpa define tersebut.
- `/UPDATEMODE` hanya dikenali sebagai satu argumen utuh, sehingga potongan teks di path tidak mengaktifkan mode pembaruan senyap.
- `/D=` ditolak sebelum halaman pertama karena NSIS menerapkannya setelah `.onInit`.
- Uninstaller menanyakan simpan pengaturan (default) atau hapus semua data pengguna.
- Penghapusan data hanya berjalan bila pilihan hapus dipilih; jurnal pemulihan lock screen tetap tidak disentuh.
- Dinaikkan nomor versi aplikasi dan installer ke 2.5.2.

## v2.5.1
- Diperkecil ukuran paket self-contained lewat optimasi Tier 1: payload publish memangkas berkas runtime/framework yang tidak diperlukan aplikasi, memverifikasi berkas wajib, dan membersihkan/remap `deps.json`.
- Dihapus dependensi WinForms lewat optimasi Tier 2: tray icon memakai native `Shell_NotifyIconW` dengan menu konteks WPF, sehingga `UseWindowsForms` tidak lagi diperlukan.
- Diperbaiki tampilan menu systray agar mengikuti tema aplikasi, termasuk latar, hover, kontras tema gelap, dan separator.
- Ditambahkan tiga varian installer NSIS: universal (`x64+x86`), `x64`, dan `x86`; updater memprioritaskan installer sesuai arsitektur dengan fallback universal.
- Dinaikkan nomor versi aplikasi dan installer ke 2.5.1.

## v2.5.0
- Diperluas data lokasi offline dengan daftar global `cities500` dan data Indonesia hybrid Kemendagri–GeoNames yang memuat koordinat, `dem`, nama alternatif, kode fitur, dan populasi.
- Ditingkatkan ketinggian permukiman dengan sampel Copernicus GLO-90 di samping `dem` GeoNames; sumber dan kualitas ketinggian disimpan pada data.
- Diperbaiki peringkat pencarian nama lokasi agar nama resmi dan nama alternatif tidak menggabungkan tempat yang berbeda, khususnya `Cibarusa`/`Cibarusah` dan `Cileungsi`/`Cileungsir`.
- Disatukan katalog metode hitung dan aturan negara/zona waktu untuk semua jalur input lokasi; Muhammadiyah 99 tetap dipertahankan di Indonesia.
- Ditambahkan opsi paket data global lengkap dengan checkbox izin, tombol unduh, progres, pembatalan, cache permanen, dan verifikasi SHA-256. Kegagalan manifest/jaringan ditangani tanpa menghentikan aplikasi; data bawaan tetap dipakai; direktori staging `v2.partial` dibersihkan setelah instalasi sukses.
- Ditambahkan opsi Nominatim online opsional (default mati, izin eksplisit lewat checkbox). Saat aktif, koordinat dapat dilengkapi lewat reverse geocoding dan nama lokasi dapat dicari sebagai acuan saran; bila jaringan/layanan gagal, data offline tetap dipakai tanpa menghentikan aplikasi.
- Ditambahkan deteksi lokasi otomatis saat instalasi baru: prioritas lokasi perangkat/GPS, fallback ke perkiraan jaringan, lalu fallback ke lokasi bawaan; nama, mdpl, zona waktu, lintang, bujur, dan metode diisi otomatis.
- Disesuaikan preset instalasi baru: metode Muhammadiyah, widget desktop aktif (lima waktu statis, tengah atas, selalu di atas, klik tembus, sembunyi saat kursor mendekat), panel taskbar aktif, mulai tersembunyi, jalankan saat masuk Windows, periksa pembaruan otomatis, kunci Windows lima waktu sholat, dan persetujuan lock screen aktif.
- Diperbaiki aktivasi widget desktop dan panel taskbar agar langsung diterapkan saat dicentang atau disimpan, tanpa memerlukan keluar-masuk aplikasi.
- Diperbaiki panel taskbar yang bisa hilang setelah deteksi lokasi otomatis instalasi baru; refresh latar tidak lagi membongkar panel yang sehat dan kegagalan sinkronisasi dicatat tanpa mematikan aplikasi.
- Diperbaiki sinkronisasi `DisplayVersion` Control Panel/Programs & Features saat root instalasi terdeteksi, termasuk setelah pembaruan diterapkan.
- Diperbarui atribusi sumber data pada `NOTICE.txt` dan `THIRD_PARTY_NOTICES.md`.
- Dinaikkan nomor versi aplikasi dan installer ke 2.5.0.

## v2.4.3
- Disinkronkan nilai `DisplayVersion` pada entri uninstall setelah journal pembaruan tersimpan dan saat startup agar sesuai dengan versi biner yang sedang berjalan.
- Diselaraskan assertion harness visual dengan `AssemblyName` produk, tinggi widget 34 DIP, padding 2, teks bantuan, margin, jarak layout, direktori scratch milik harness, dan jalur Cancel tanpa dialog.
- Diselaraskan semua jalur penentuan lokasi—koordinat manual, pilihan nama lokal, perkiraan jaringan, dan GPS—agar mengisi nama lokasi, mdpl, zona waktu, lintang, bujur, dan metode hitung secara otomatis; Muhammadiyah 99 dipertahankan bila sedang aktif di Indonesia.
- Dihilangkan karakter panah non-ASCII pada halaman selamat datang installer dan diganti escape baris literal pada pengaman lock screen dengan escape NSIS yang benar.
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
