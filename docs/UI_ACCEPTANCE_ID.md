# Panduan Uji UI/UX Portable — Pengguna Biasa

Panduan ini untuk acceptance manual build C# WPF + Rust. Hasil tes otomatis tersedia di [VERIFICATION_ID.md](VERIFICATION_ID.md); checklist di bawah **belum berarti sudah lulus**. Panduan program aktif ada di [dokumentasi utama](prompts/PROGRAM_DOCUMENTATION_ID.md).

## Sebelum membuka aplikasi

- Gunakan satu PC uji dahulu, bukan komputer yang sedang mengendalikan proses/equipment.
- Pakai akun Windows biasa. Pemasang juga bisa dijalankan sebagai administrator (mis. komputer satu akun administrator); pemasangan tetap untuk pengguna saat ini. Jangan memasukkan kredensial ke chat.
- Pertahankan seluruh folder portable; jangan hanya menyalin EXE.
- Build membaca profil lama di `%APPDATA%\PrayerAutomation\config.json`. Pada pemeriksaan build ini, auto-lock tersimpan aktif. Folder portable baru **tidak** membuat profil kosong.
- Buka jauh dari waktu sholat berikutnya. Jika tidak menerima risiko sesi terkunci, jangan buka dahulu; minta peninjauan profil secara terpisah. Verifikasi build tidak mengubah profil Anda.

## A. Buka dan matikan auto-lock untuk uji tampilan

1. Klik dua kali `PrayerAutomation.UI.exe` dalam folder portable.
2. Buka tab **Pengaturan**.
3. Pada **Windows dan batas keselamatan**, hilangkan centang **Kunci Windows saat lima waktu sholat**.
4. Untuk uji UI awal, biarkan checkbox **Saya setuju mengubah gambar lock screen sementara** tidak dicentang.
5. Klik **Simpan pengaturan**.
6. Buka kembali Pengaturan dan pastikan dua opsi itu tetap nonaktif. Ini perubahan profil yang Anda lakukan sendiri; bukan sesuatu yang dilakukan oleh suite otomatis.

**Hasil yang diharapkan:** tidak ada prompt UAC/admin, app bisa dinavigasi, dan uji tampilan tidak sengaja mengaktifkan penggantian gambar atau auto-lock. Jika muncul permintaan elevation, hentikan pengujian dan laporkan.

## B. Checklist tampilan dan pengaturan

| ID | Tindakan | Hasil yang diharapkan | Hasil Anda |
|---|---|---|---|
| U01 | Buka Ringkasan, kalender Masehi/Gregorian, Hijriyah, lalu Pengaturan. | Tab mempertahankan bentuk default; isi terlihat dan dapat discroll. | Belum diuji |
| U02 | Coba delapan tema satu per satu; buka dropdown, sorot/pilih item, fokus textbox, klik button, dan centang checkbox. | Teks, background, glyph, item selected/disabled tetap terbaca. Tidak ada tab rounded baru. | Belum diuji |
| U03 | Ubah tema tanpa Simpan, lalu Batalkan perubahan; hide ke tray dan buka kembali. | Tema/form kembali ke nilai tersimpan, bukan preview yang ditinggalkan. | Belum diuji |
| U04 | Bandingkan tombol −/+ koreksi penanggalan Hijriyah dengan helper koreksi waktu; tekan masing-masing. | Label "Koreksi penanggalan hijriyah" tampil di kiri input; ukuran, bentuk, font dan warna helper seragam; perubahan satu hari, rentang maksimum ±7; jadwal harian tidak digeser oleh koreksi tampilan. | Belum diuji |
| U05 | Ubah satu koreksi waktu sholat dengan −/+; amati estimasi sebelum Save. | Hanya waktu terkait berubah sesuai menit. Save menerapkan; Cancel tidak menyimpan preview. | Belum diuji |
| U06 | Aktifkan widget Essential lalu pilih Marquee sebelum Simpan; amati widget aktual, kemudian Simpan. | Perubahan langsung pada widget aktual, bukan panel tiruan. Satu baris bergerak; tidak membeku/melompat setiap refresh jam. Simpan tetap di tab Pengaturan. | Belum diuji |
| U07 | Tunggu isi marquee lewat satu putaran penuh, termasuk sebelum/saat/sesudah wrap. | Tidak ada interval seluruh viewport kosong; teks berikutnya masuk dari sisi kanan. Tanggal Masehi/Hijriyah, jam/timezone, jadwal/countdown, kalender, dan hadis bersumber terbaca; hadis berganti sesudah putaran selesai. | Belum diuji |
| U08 | Coba Essential Statis, Minimalist, dan Medium detail. | Essential mengisi lebar; Minimalist ringkas; Medium detail tidak terpotong. | Belum diuji |
| U09 | Buka/tutup widget dari tray; buka Pengaturan berulang kali. | Tidak ada duplikasi jendela atau hang; menutup widget tidak menghentikan reminder. | Belum diuji |
| U10 | Tunggu pengingat pada jadwal yang memang Anda pilih untuk uji. | Popup hanya teks, tanpa suara/adzan. Terbit dan Duha tidak memicu reminder lima waktu. | Belum diuji |
| U11 | Gunakan tombol nama lokasi offline dengan koordinat form yang telah disetujui. | Label permukiman perkiraan muncul tanpa request jaringan; tidak dianggap alamat persis. | Belum diuji |
| U12 | Buka form reverse geocoding online tanpa consent. | Tidak ada koordinat dikirim; consent diperlukan dan harus diberikan lagi setelah form dibuka ulang. Tidak perlu mengirim lokasi nyata untuk menguji UI ini. | Belum diuji |
| U13 | Ubah Medium detail/posisi/tema/visibilitas tanpa Simpan; Batalkan, lalu ulangi dan tutup ke tray. | Actual widget/form/tema kembali ke baseline; widget yang tadinya disembunyikan manual tetap tersembunyi. Tidak ada draft disimpan. | Belum diuji |
| U14 | Simpan pilihan widget baru dari Pengaturan, ubah lagi tanpa Simpan, lalu Batalkan/reopen tray. | Tetap di tab aktif setelah Simpan; Cancel/reopen kembali ke baseline baru. Hanya pilihan tab atau perintah tray bertarget yang menavigasi. | Belum diuji |
| U15 | Baca penjelasan lokasi, izin online, widget, tema dan opsi Windows; ubah ukuran jendela lalu scroll sampai bawah. | Paragraf rata kiri, jarak baris/indentasi konsisten, checkbox dapat dibaca, peringatan/error tidak hilang. Tidak ada kontrol wajib yang tak terjangkau. | Belum diuji |

Delapan tema: Light, Dark, Windows Classic, Windows XP Luna, macOS, Linux Adwaita, Nord, Dracula. Periksa pada DPI/resolusi yang Anda gunakan. Ukuran atau kontras yang lulus di harness belum menggantikan hasil mata pengguna.

Harness terbaru lulus 14/14 langkah, dua ukuran form dan 16 kasus lifecycle widget tanpa membuka aplikasi utama. Ini tidak mengisi kolom **Hasil Anda**. Nama metode pada ComboBox tertutup masih dapat terpotong di lebar 940 DIP; tepi helper `+` dan DPI monitor perlu pengamatan pengguna.

## C. Uji gambar lock screen — tiga mode, tanpa auto-lock

Ini **mengubah pengaturan lock screen pengguna**. Lakukan hanya ketika Anda menerima efek tersebut dan siap memulihkannya. **Auto-lock tetap nonaktif.** Jangan mengubah jam Windows, membuat waktu sholat buatan, atau mengubah Registry untuk mempercepat tes.

### Persiapan tiap mode

1. Buka Windows **Settings > Personalization > Lock screen**.
2. Pilih satu mode awal untuk diuji: Windows Spotlight, Picture, atau Slideshow.
3. Untuk Picture, pilih file yang masih ada dan mudah dikenali. Untuk Slideshow, catat folder/options agar dapat dibandingkan setelah restore.
4. Periksa wallpaper desktop secara terpisah dan ingat tampilannya; fitur ini tidak boleh mengubahnya.
5. Buka Pengaturan aplikasi dan baca status lock screen. Jika checkbox disabled atau ada status pending/error, jangan memaksa enable/elevate.
6. Jika setuju, centang **Saya setuju mengubah gambar lock screen sementara**, lalu **Simpan pengaturan**.
7. Biarkan aplikasi berjalan sampai salah satu dari lima waktu wajib berikutnya. Build ini tidak menyediakan tombol trigger segera dalam panduan ini.

### Pengamatan dan restore

- Setelah event, periksa status aplikasi. **Status accepted/readback bukan bukti gambar terlihat.**
- Jika Anda setuju mengunci sesi secara manual, tekan **Win+L** dan periksa gambar sementara. Win+L adalah tindakan Anda, bukan bukti scheduler auto-lock.
- Unlock sendiri menggunakan metode login biasa; jangan mengirim PIN/password/kode ke chat.
- Tunggu **60 menit** dari event, dengan app tetap berjalan. Timer ini bukan deteksi selesai sholat.
- Buka Settings Windows untuk memeriksa mode, lalu periksa lock screen lagi secara manual bila Anda setuju. Bandingkan wallpaper desktop secara terpisah.

| ID | Mode awal | Hasil restore yang harus diterima | Hasil Anda |
|---|---|---|---|
| L01 | Spotlight | Kembali ke **mode Windows Spotlight**. Tidak harus kembali ke gambar Spotlight yang sama; URI/gambar awal bukan syarat. | Belum diuji |
| L02 | Picture | Kembali ke **mode Picture dan gambar asal** yang dipilih sebelum tes. Jika backup tidak valid, app harus menahan apply baru. | Belum diuji |
| L03 | Slideshow | Kembali ke **mode Slideshow**; bandingkan folder/options dan slideshow berjalan. Build hanya menyimpan flags mode, bukan backup konfigurasi lengkap. | Belum diuji |
| L04 | Semua mode | Wallpaper desktop tetap sama. Hasil ini dicatat terpisah dari hasil lock screen. | Belum diuji |

Ulangi satu mode per siklus; jangan mengganti mode di tengah restore normal. Jika Anda memang mengubah mode/gambar sendiri saat transaksi aktif, aplikasi seharusnya menghormati override; itu kasus uji terpisah, bukan restore normal yang gagal.

## D. Recovery dan Exit — uji lanjutan, bukan syarat membuka UI

| ID | Kasus | Yang perlu diamati pada Windows nyata | Hasil Anda |
|---|---|---|---|
| R01 | Matikan consent gambar saat transaksi aktif, lalu Save. | App meminta pemulihan tanpa apply baru; bila gagal/pending, status jelas dan recovery tetap tersedia. | Belum diuji |
| R02 | Pilih Keluar dari tray saat gambar sementara aktif. | App menunggu operasi berjalan dan meminta restore. Bila belum terverifikasi, app boleh tetap hidup untuk retry; jangan anggap hide sebagai exit. | Belum diuji |
| R03 | Anda memilih mode/gambar lain selama transaksi aktif. | App tidak menimpa pilihan baru pengguna. Catat hasil terpisah dari restore normal. | Belum diuji |
| R04 | Recovery sesudah force-close/restart. | Hanya lakukan pada PC uji setelah menerima risiko gambar tertinggal. App yang mati tidak bisa restore sampai dijalankan kembali. Jangan hapus jurnal/backup. | Belum diuji |

Failure scheduler dan interruption sudah diuji memakai backend/native platform terisolasi. Itu tidak berarti fault injection/force-close pada lock screen pengguna sudah diuji. Jangan menyimulasikan failure dengan memblokir file, merusak Registry, atau menghapus gambar asal pada PC harian.

## E. Uji data lokasi 2.5.0 — belum dijalankan

Bagian ini untuk build `2.5.0`. Kolom **Hasil Anda** belum diisi; menjalankan checklist ini tidak berarti rilis atau uji pembaruan `2.4.3` sudah diperintahkan.

| ID | Tindakan | Hasil yang diharapkan | Hasil Anda |
|---|---|---|---|
| F01 | Buka Pengaturan tanpa menyentuh opsi paket data. | Checkbox paket data lokasi lengkap tidak dicentang dan tombol unduh nonaktif. Tidak ada unduhan otomatis; data bawaan tetap dipakai. | Belum diuji |
| F02 | Centang izin, klik unduh, lalu amati progres; klik **Batalkan unduhan** sebelum selesai. | Progres/status bergerak tanpa UI membeku; tombol berubah menjadi batal. Pembatalan menghentikan unduhan, membereskan berkas sementara, dan data bawaan tetap dipakai. | Belum diuji |
| F03 | Centang lagi, unduh sampai selesai, tutup aplikasi, putuskan internet, lalu buka kembali. | Status menyebut paket tersedia dari cache; pencarian tetap memakai data terverifikasi tanpa internet. Bila hash tidak cocok, aplikasi kembali ke data bawaan dan status menjelaskannya. | Belum diuji |
| F04 | Cari `Cibarusa`, `Cibarusah`, `Cileungsi`, `Cileungsir`, dan `Cicendo` satu per satu; pilih hasil yang sesuai. | Kelima nama tetap merupakan tempat berbeda, kabupaten/kota sesuai, dan tidak ada alias yang menimpa nama resmi. | Belum diuji |
| F05 | Isi lokasi lewat koordinat manual, pilihan nama lokal, perkiraan jaringan, dan GPS. | Enam kolom — nama, mdpl, zona waktu, lintang, bujur, dan metode — terisi di semua jalur. Pilihan Muhammadiyah 99 di Indonesia tidak ditimpa. | Belum diuji |
| F06 | Gunakan titik `-6.4106170,107.0961768` dan `-6.23489335,106.98961635`. | GPS menghasilkan permukiman terdekat yang benar-benar dekat, bukan dipaksa menjadi `Cibarusa`; jaringan menghasilkan `Bekasi, Kota Bekasi`. Nilai mdpl tidak kosong dan sumber ketinggian tercatat pada data. | Belum diuji |

Uji F02–F03 memakai jaringan yang tersedia di PC uji; jangan mengirim kredensial atau data pribadi. F05–F06 memakai koordinat contoh di atas, bukan lokasi nyata bila tidak diperlukan.

## F. Jika hasil tidak sesuai

1. Catat ID checklist, versi/folder portable, mode awal, waktu event, tindakan, dan teks status.
2. Jangan mengaktifkan administrator, menghapus jurnal, atau menambahkan helper EXE sebagai workaround.
3. Jika gambar/mode tertinggal, pilih background melalui Settings Windows secara manual sesuai mode semula dan laporkan. Jangan mengedit Registry sendiri.
4. Jika meminta peninjauan log, gunakan `%LOCALAPPDATA%\PrayerAutomation\prayer_automation.log` dan redaksi data pribadi. Screenshot lock screen dapat memuat nama/notifikasi; potong/redaksi sebelum berbagi.
5. Tandai hasil **Lulus / Gagal / Belum diuji**. Persetujuan build installer hanya setelah acceptance Anda dan instruksi eksplisit.

**Batas klaim:** build, metadata, DLL load, dan harness WPF sudah diperiksa. Uji visual lock screen, recovery real Windows, PC bersih, dan kebijakan organisasi masih menunggu acceptance; tidak boleh dinyatakan lulus hanya dari compile.
