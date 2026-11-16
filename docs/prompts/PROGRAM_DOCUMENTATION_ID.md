# Ayo Jama'ah! — Panduan Program C# WPF + Rust

## 1. Varian aktif dan batas keselamatan

**Branding 2.1.0:** nama tampilan `Ayo Jama'ah!`, executable `AyoJamaah.exe`, ikon `AyoJamaah.ico`, master logo v13. Identitas profil/mutex/Run value/AppId dan namespace lama dipertahankan; acceptance/receipt bertanggal lama di bawah adalah riwayat, bukan penerimaan versi baru. Lihat [README](../../README.md) dan [installer saat ini](../INSTALLER_ID.md).

Panduan ini untuk rilis **C# WPF + Rust** yang payload-nya disimpan di luar working tree Git (`../PrayerAutomation-releases/2.0.0/portable/`), bukan `PrayerAutomationRestoreFix.exe`/Python. **UI/UX dinyatakan LOLOS oleh pengguna pada 30 Oktober 2026**; installer per-user diizinkan. [Catatan arsip Python](../LEGACY_PYTHON_ID.md) menjelaskan lokasi backup historis.

| Fitur | Efek |
|---|---|
| Jadwal, kalender, widget | Tampilan dan perhitungan lokal |
| Pengingat | Popup teks tanpa suara/adzan |
| Auto-lock | Mengunci sesi Windows pada lima waktu wajib; opsi terpisah |
| Gambar lock screen sementara | Mengubah lock screen pengguna, bukan wallpaper desktop; tidak mengunci sesi |
| Simpan | Menulis profil dan menerapkan pilihan aktif, termasuk startup dan opsi Windows |

Konfigurasi baru menonaktifkan auto-lock dan penggantian gambar. **Profil lama tetap dibaca** dari `%APPDATA%\PrayerAutomation\config.json`; mengganti folder portable tidak menonaktifkan profil lama. Pada pemeriksaan build ini, opsi Windows pada profil lama aktif. Suite tidak menjalankan aplikasi utama dan tidak mematikan opsi tersebut secara diam-diam.

Gunakan satu PC uji dengan akun biasa. Tidak ada kebutuhan kredensial administrator; manifest portable terverifikasi `asInvoker`/`uiAccess=false`. Jika Windows menolak fitur atau muncul UAC, hentikan pengujian: jangan elevate, memasang helper, atau menulis policy sebagai workaround.

## 2. Menjalankan dan memahami tab

1. Baca [persiapan acceptance](../UI_ACCEPTANCE_ID.md) sebelum membuka app, terutama jika profil lama mengaktifkan auto-lock.
2. Pertahankan seluruh folder portable, lalu buka `AyoJamaah.exe` sebagai pengguna biasa hanya jika risiko profil sudah diterima.
3. **Ringkasan** menampilkan jadwal, konteks metode, dan status pengingat.
4. **Kalender** menyediakan **Masehi / Gregorian** dan **Hijriyah**. Hijriyah/peringatan kalender merupakan perkiraan lokal, bukan pengumuman resmi.
5. **Pengaturan** mengatur lokasi, metode/koreksi, widget, tema, startup, auto-lock, dan consent gambar.
6. Tombol X menyembunyikan jendela ke tray dan membuang preview tampilan yang belum disimpan. Gunakan **Keluar** dari tray untuk shutdown; restore aktif dapat membuat app tetap hidup untuk retry.

Tab mempertahankan bentuk default. **Simpan pengaturan tidak kembali ke Ringkasan.** Navigasi terjadi dari pilihan tab atau perintah tray yang secara eksplisit membuka tab tertentu.

## 3. Preview, Simpan, dan Batalkan

| Pilihan | Sebelum Simpan | Setelah Simpan / Batalkan |
|---|---|---|
| Tema | Terlihat langsung pada form dan widget | Simpan menetapkan baseline; Batalkan/tutup ke tray memulihkan tema tersimpan |
| Mode, gerak, posisi, visibilitas widget | Mengubah jendela widget aktual melalui state sementara | Simpan menetapkan baseline baru; Batalkan/tutup ke tray mengembalikan tampilan dan visibilitas sebelum preview |
| Lokasi, timezone, metode, koreksi | Estimasi dari nilai form; tidak mengganti konfigurasi jadwal aktif | Simpan menerapkan jadwal aktif dan me-refresh konsumen |
| Startup, reminder, auto-lock, consent gambar | Nilai form; preview bukan izin melakukan tindakan Windows | Diterapkan lewat Simpan, bukan sekadar memilih checkbox |

Saat widget sebelumnya disembunyikan secara manual, rollback tidak boleh memaksanya tampil. Widget yang hanya dibuat untuk preview dapat ditutup saat rollback. **Live bukan auto-save.** Pemindahan tab sendiri tidak berarti menyimpan; gunakan Simpan atau Batalkan secara eksplisit.

Penyelesaian startup gambar yang masih asinkron tidak boleh menimpa draft widget, menampilkan widget yang disembunyikan manual, atau membuat widget saat shutdown. Jalur lanjutan produksi ini direproduksi terisolasi: RED 8 gagal, kemudian GREEN dan rerun parent 16/16. Ini tidak menjalankan startup utama dengan profil asli; status review dan batasnya ada pada [laporan verifikasi](../VERIFICATION_ID.md).

Contoh uji setelah auto-lock/consent gambar dinonaktifkan oleh pengguna:

1. Di Pengaturan, aktifkan widget dan pilih **Medium detail** serta posisi sudut.
2. Amati widget aktual sebelum Simpan.
3. Pilih **Batalkan perubahan**: mode, posisi, tema, dan visibilitas kembali ke baseline.
4. Ulangi pilihan, lalu **Simpan pengaturan**: tetap di Pengaturan dan pilihan itu menjadi baseline baru.
5. Ubah lagi tanpa Simpan lalu tutup ke tray; saat dibuka ulang, yang tampil adalah baseline terakhir, bukan draft.

## 4. Lokasi, metode dan kalender

- Koordinat manual yang sudah dikonfirmasi tetap menjadi acuan sampai pengguna mengubah dan menyimpan.
- **Perkiraan dari jaringan** mengirim permintaan setelah tombol ditekan. Lokasinya dapat menunjuk gateway/proxy/VPN; bukan bukti posisi fisik.
- **Lokasi perangkat / GPS** memakai Windows Location Services. Sumber Windows dapat berupa GPS maupun sumber lokasi sistem lain.
- **Cari nama offline** memakai koordinat form dan data GeoNames yang dibundel, tanpa request jaringan. Hasil adalah permukiman perkiraan, bukan alamat persis; lisensi/atribusi ada di `src/src/PrayerAutomation.UI/Data/geonames/NOTICE.txt`.
- Lookup online memerlukan consent eksplisit yang tidak dipersistenkan. Jalur ini hanya memakai posisi Windows terbaru, bukan koordinat manual/tersimpan atau estimasi IP. Server menerima koordinat dan IP publik; jangan menguji dengan posisi nyata tanpa izin.
- Consent online dicabut saat checkbox/endpoint berubah atau jendela ditutup. Tes jaringan build ini memakai HTTP mock, bukan koordinat perangkat nyata.

Core Rust menghitung jadwal lokal; build aktif tidak memerlukan AlAdhan untuk perhitungan rutinnya. Nama metode dan koreksi menit tidak membuktikan kesesuaian jadwal resmi Kemenag/Muhammadiyah sepanjang tahun. Verifikasi resmi memerlukan tanggal, lokasi, timezone, parameter, dan pembandingan beberapa musim yang sama.

Koreksi waktu menggunakan menit bertanda `−30` sampai `+30`. Tombol koreksi tanggal Masehi/Hijriyah memiliki rentang `±7` hari dan hanya menggeser tampilan kalender, bukan tanggal perhitungan jadwal. Semua helper `−/+` memakai style Button yang sama; empat helper tanggal tidak memiliki chrome terpisah.

**Terbit dan Duha tidak memicu reminder, auto-lock, atau gambar sementara.** Ketiga scheduler memakai Subuh, Dzuhur, Ashar, Maghrib, dan Isya.

### Membaca pengaturan lanjutan

| Teks pada form | Arti singkat |
|---|---|
| Zona waktu | Acuan jam untuk lokasi yang dipilih |
| Metode hitung | Parameter perhitungan jadwal; nama metode bukan jaminan jadwal resmi |
| Koreksi waktu sholat | Menambah atau mengurangi menit pada waktu terkait |
| Koreksi kalender | Menggeser tanggal tampilan saja, bukan hari perhitungan jadwal |
| Cari nama offline | Memperkirakan nama permukiman dari koordinat form tanpa jaringan |
| Cari nama online | Mengirim posisi Windows terbaru dan IP publik ke server setelah izin dan tombol |
| Kunci Windows saat lima waktu sholat | Opsi penguncian sesi, terpisah dari pengingat dan gambar sementara |
| Saya setuju mengubah gambar lock screen sementara | Consent perubahan gambar; tidak mengaktifkan penguncian sesi |

Petunjuk/penjelasan memakai font `14 DIP`, line step `20 DIP`, wrapping, rata kiri, dan jarak kiri/kanan yang seragam. Label checkbox dapat membungkus; default control/tab dipertahankan. Pesan error dan alasan keselamatan dinamis tidak dihapus demi teks ringkas. Pada jendela kecil, konten tetap perlu discroll; dua ukuran harness tidak membuktikan semua monitor/DPI bebas clipping.

## 5. Widget dan marquee

| Mode | Tampilan |
|---|---|
| Essential Statis | Lima waktu wajib mengisi satu baris |
| Essential Marquee | Satu baris berulang yang bergerak perlahan |
| Minimalist | Sholat berikutnya dan countdown |
| Medium detail | Jam, tanggal, jadwal dan countdown; posisi dapat dipilih |

Essential dan Minimalist berada di tengah atas. Dropdown gerak berlaku untuk Essential; dropdown posisi berlaku untuk Medium detail.

Marquee menyatukan tanggal Masehi, Hijriyah berlabel perkiraan sipil, jam/timezone, lima waktu, countdown, penanda kalender/libur/puasa yang berlaku, serta pesan agama offline. Teks diulang melewati viewport; complete line diukur sebelum clipping agar tidak muncul interval seluruh viewport kosong ketika wrap.

**Bukti terkini 23 Oktober 2026, run `12:24:17`:** actual-widget bitmap mencakup 64 frame, teks pendek/produksi, viewport full/narrow, raster DPI 96/144: `empty=0`, entry sisi kanan diperiksa. Timer, jam/countdown, dan lima transisi putaran yang dipicu dekat batas juga lulus pada harness. Sampling offset/transform dan forced wrap belum membuktikan seluruh tick/content replacement; raster DPI bukan perubahan DPI layout window pada monitor. Hidden-text/reference ROI dan per-monitor DPI belum diuji. Lihat [bukti tepat dan batasnya](../VERIFICATION_ID.md); ini bukan acceptance seluruh monitor/font.

### Pesan agama offline

Pesan berlabel **“Hadis (ringkasan makna Indonesia)”**, bukan terjemahan harfiah bersertifikat. Koleksi build ini memuat:

- Niat — ringkasan makna Bukhari 1.[5]
- Amalan rutin walau sedikit — ringkasan makna Bukhari 6464.[6]
- Kelembutan — ringkasan makna Muslim 2593.[7]
- Memudahkan dan kabar baik — ringkasan makna Bukhari 69.[8]
- Kebaikan sebagai sedekah — ringkasan makna Muslim 1005.[9]

Nomor/URL sumber ikut tersimpan dalam teks. Pesan berotasi secara deterministik setelah putaran penuh, tanpa internet. Tidak ada nasihat yang diatribusikan kepada Salaf tanpa sumber dalam koleksi ini.

## 6. Gambar lock screen tanpa administrator

### Jalur yang dipakai dan yang tidak dipakai

Native boundary memakai `LockScreen.SetImageFileAsync` untuk gambar lock screen; Microsoft mendeskripsikan API tersebut sebagai pengaturan gambar dari `StorageFile`.[1] `OriginalImageFile` hanya mengambil file, dan dapat gagal jika gambar sebelumnya disetel melalui stream.[2]

Untuk **Picture**, backend mensyaratkan gambar asal lokal yang bisa dibaca, disalin, dan diverifikasi SHA256 sebelum apply baru. **Spotlight** tidak mensyaratkan URI/gambar awal: target restore adalah mode, bukan foto yang sama. **Slideshow** juga dipulihkan sebagai mode; build tidak membackup folder/options slideshow.

Windows menyediakan pilihan **Windows Spotlight**, **Picture**, dan **Slideshow** melalui **Settings > Personalization > Lock screen**.[3]

**Batas implementasi:** mode dikenali dan dipulihkan melalui indikator HKCU `SlideshowEnabled` dan `RotatingLockScreenEnabled`. Sumber Microsoft yang diperiksa tidak menetapkan keduanya sebagai public API pemulihan mode. DWORD hilang, malformed, atau konflik membuat fitur menolak apply; flags readback bukan bukti gambar/mode terlihat di Windows.

Tidak ada write HKLM, Personalization CSP, helper `igcmdWin10.exe`, API desktop wallpaper, atau elevation fallback pada jalur C# ini. **Personalization CSP `LockScreenImageUrl` adalah Device, bukan User**, sehingga dokumentasi Python lama tidak boleh dipakai sebagai panduan jalur pengguna biasa.[4]

### Transaksi dan pemulihan

1. Consent gambar tersimpan dan preflight harus mengizinkan fitur.
2. Backend membaca mode; Picture memerlukan salinan gambar asal valid, sedangkan Spotlight/Slideshow mode-only.
3. Salinan PNG sementara dan hash disiapkan. Jurnal persisten disimpan **sebelum** native setter.
4. Source/ownership diperiksa kembali menjelang setter. Pemeriksaan ulang ini tidak membuat operasi atomik terhadap perubahan pengguna/Windows yang bersamaan.
5. Gambar sementara diterapkan. Verifikasi internal tetap terpisah dari pengamatan gambar yang terlihat.
6. Setelah **60 menit**, transaksi mencoba restore. Ini durasi tetap dari event, **bukan deteksi waktu selesai sholat**. Event baru saat transaksi masih dimiliki dapat memperpanjang deadline.
7. Spotlight → mode Spotlight; Slideshow → mode Slideshow; Picture → gambar asal terverifikasi dan mode Picture.
8. Jika hasil belum dapat diverifikasi, jurnal dipertahankan untuk retry. Perubahan mode/gambar oleh pengguna harus dihormati, bukan ditimpa.

Startup melakukan recovery sebelum apply baru. Scheduler memakai polling recovery 30 detik/backoff saat diperlukan. Mematikan consent dan Simpan meminta restore-only; Keluar menunggu operasi in-flight dan meminta pemulihan. Jika gagal, app dapat tetap hidup untuk retry. Force-close berarti app tidak bisa memulihkan sampai dijalankan kembali.

**Belum diterima di Windows nyata:** gambar sementara terlihat, pemulihan mode Spotlight/Picture/Slideshow, Picture asal, folder/options slideshow tetap berfungsi, restart/force-close, serta override serentak. Build/synthetic tidak membuktikan hasil-hasil itu.

## 7. Profil dan data runtime

| Lokasi | Isi dan perlakuan |
|---|---|
| `%APPDATA%\PrayerAutomation\config.json` | Profil per pengguna; tidak menjadi bagian portable/Git |
| `%LOCALAPPDATA%\PrayerAutomation\LockScreen\lock-screen-restore.json` | Jurnal produksi untuk restore; jangan dihapus ketika transaksi pending |
| `%LOCALAPPDATA%\PrayerAutomation\LockScreen\` | Gambar sementara/backup terkelola dan blocker recovery bila ada |
| `%LOCALAPPDATA%\PrayerAutomation\lock-screen-state\` | Lokasi state versi/tes lama yang ikut dipantau oleh suite; bukan alasan memindahkan atau menghapus state sendiri |
| `../PrayerAutomation-releases/2.0.0/portable/` | Payload diterima: EXE, runtime self-contained, native DLL, ikon, PNG dan GeoNames; berada di luar working tree Git |

App tidak menyimpan kata sandi/PIN untuk membuka sesi. Unlock dilakukan pengguna melalui metode login Windows biasa; jangan mengirim kredensial/kode lewat chat.

## 8. Build, hasil verifikasi, dan acceptance

Target proyek adalah `net10.0-windows10.0.26100.0`, minimum platform yang dinyatakan `10.0.19041.0`. Minimum tersebut adalah konfigurasi proyek, **bukan bukti sudah diuji di semua Windows 10/11**. Portable memuat runtime .NET/WindowsDesktop x64.

```bash
cargo test --workspace --release --locked --offline --target x86_64-pc-windows-gnu
dotnet build src/src/PrayerAutomation.UI/PrayerAutomation.UI.csproj --configuration Release
dotnet publish src/src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -r win-x64 --self-contained true -o ../PrayerAutomation-releases/developer-build/portable
```

SDK/Rust/linker diperlukan untuk build, bukan untuk pemakaian portable. Target MSBuild membangun `prayer-core` sebelum UI. `SingleInstanceSmoke` tidak masuk safe suite karena memulai aplikasi utama.

[Hasil verifikasi](../VERIFICATION_ID.md) memuat exit code, frame/kontras, metadata, native-load, hash, dan batas pemeriksaan. [Checklist acceptance](../UI_ACCEPTANCE_ID.md) memandu pengamatan pengguna biasa. Flowchart adalah ilustrasi dari source, bukan screenshot atau bukti native write: [alur aplikasi](../program_full_flowchart.html), [alur lock screen](../lockscreen_restore_flow.html). Source/diagram Python lama telah diarsipkan di luar working tree Git; lokasi dan hash tercatat pada [catatan arsip Python](../LEGACY_PYTHON_ID.md). Prosedur UAC/HKLM historis tidak berlaku untuk rilis aktif. [Installer dan upgrade aman](../INSTALLER_ID.md) menjelaskan penutupan normal, pembatalan, dan preservasi profil.

## 9. Jika fitur ditolak atau restore pending

1. Baca status pada **Windows dan batas keselamatan**; jangan memaksa checkbox yang disabled.
2. Jangan elevate, menghapus jurnal/backup, menulis Registry sendiri, atau menjalankan helper Python sebagai workaround.
3. Jika pengguna perlu memulihkan tampilan secara manual, pilih mode semula melalui Settings Windows dan laporkan sebagai override manual, bukan bukti restore otomatis.
4. Catat folder build, mode awal, event/tindakan, status, dan ID checklist. Redaksi nama/notifikasi/lokasi sebelum membagikan log atau screenshot.
5. Installer dan rollout baru dilakukan setelah acceptance pengguna dan instruksi eksplisit.

## Sources

[1] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagefileasync?view=winrt-26100 — LockScreen.SetImageFileAsync — Microsoft Learn
    > "Sets the lock screen image from a StorageFile object."
[2] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.originalimagefile?view=winrt-26100 — LockScreen.OriginalImageFile — Microsoft Learn
    > "This property retrieves only files. If the image was set through a stream, this call will return E_FILE_NOT_FOUND."
[3] https://support.microsoft.com/en-us/windows/experience/personalization/customize-the-lock-screen-in-windows — Customize the Lock Screen in Windows — Microsoft Support
    > "Select Start > Settings > Personalization > Lock screen."
    > "Windows spotlight: displays a rotating set of images from Microsoft, along with tips, tricks, and notifications."
    > "Picture: select a single image as your lock screen background."
    > "Slideshow: use a slideshow of images from a folder on your device."
[4] https://learn.microsoft.com/en-us/windows/client-management/mdm/personalization-csp — Personalization CSP — Microsoft Learn
    > "## LockScreenImageUrl

| Scope | Editions | Applicable OS |
|---|---|---|
| ✅ Device ❌ User | ✅ Pro ✅ Enterprise ✅ Education ✅ IoT Enterprise / IoT Enterprise LTSC | ✅ Windows 10, version 1709 [10.0.16299] and later |

```Device
./Vendor/MSFT/Personalization/LockScreenImageUrl
```

An http or https Url to a jpg, jpeg or png image that needs to be downloaded and used as the Lock Screen Image or a file Url to a local image on the file system that needs to be used as the Lock Screen Image."
[5] https://sunnah.com/bukhari:1 — Sahih al-Bukhari 1 - Revelation - كتاب بدء الوحى - Sunnah.com - Sayings and Teachings of Prophet Muhammad (صلى الله عليه و سلم)
    > "The reward of deeds depends upon the intentions and every person will get the reward according to what he has intended."
[6] https://sunnah.com/bukhari:6464 — Sahih al-Bukhari 6464 - To make the Heart Tender (Ar-Riqaq) - كتاب الرقاق - Sunnah.com - Sayings and Teachings of Prophet Muhammad (صلى الله عليه و سلم)
    > "the most beloved deed to Allah is the most regular and constant even if it were little."
[7] https://sunnah.com/muslim:2593 — Sahih Muslim 2593 - The Book of Virtue, Enjoining Good Manners, and Joining of the Ties of Kinship - كتاب البر والصلة والآداب - Sunnah.com - Sayings and Teachings of Prophet Muhammad (صلى الله عليه و سلم)
    > "verily Allah is kind and He loves kindness"
[8] https://sunnah.com/bukhari:69 — Sahih al-Bukhari 69 - Knowledge - كتاب العلم - Sunnah.com - Sayings and Teachings of Prophet Muhammad (صلى الله عليه و سلم)
    > "Facilitate things to people (concerning religious matters), and do not make it hard for them and give them good tidings and do not make them run away (from Islam)."
[9] https://sunnah.com/muslim:1005 — Sahih Muslim 1005 - The Book of Zakat - كتاب الزكاة - Sunnah.com - Sayings and Teachings of Prophet Muhammad (صلى الله عليه و سلم)
    > "Every act of goodness is sadaqa."
