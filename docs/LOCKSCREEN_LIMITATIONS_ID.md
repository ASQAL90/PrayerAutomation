# Batas pemulihan gambar lock screen

## Keputusan untuk kebutuhan yang dilaporkan: SKIP

Pemulihan **gambar Spotlight yang sedang terlihat ketika sesi tetap terkunci**, tanpa unlock→lock, di-skip untuk finalisasi ini. Belum ada rute lengkap yang dapat dibuktikan dalam batas tanpa admin, tanpa private API/manipulasi sesi, dan tanpa mengganti Spotlight menjadi Picture permanen. Ini keputusan kelayakan dalam batas tersebut, bukan klaim bahwa semua versi Windows mustahil melakukannya.

## Bukti pengguna, bukan hasil simulasi

Pada uji R2: trigger 11:35:55.792, deadline 12:35:55.792, hasil restore 12:35:57.088. Pada 12.37 pengguna masih melihat gambar sholat hingga unlock→lock. **Penerimaan visual tetap GAGAL.** Durasi transaksi tersebut 60 menit; konfigurasi 10 menit dari transaksi selanjutnya tidak dipakai untuk menafsirkan kasus itu.

Source memulihkan mode/flags Spotlight dan mencatat `verification=config-only visual=unverified`. Matching flags, jurnal yang telah dibersihkan, `Restored`, dan technical GREEN tidak membuktikan bahwa pixels pada LockApp sudah berubah. Tidak ada eksperimen native baru yang dijalankan dalam riset finalisasi ini.

## Mengapa tidak menambah patch timer lagi

`GetImageStream()` didokumentasikan mengambil gambar lock screen saat ini sebagai stream, dengan pembatasan capability atau kepemilikan aplikasi yang memasang gambar.[12]

Itu tetap hipotesis akuisisi: belum dibuktikan bahwa akses aplikasi ini menghasilkan latar Spotlight yang sedang terlihat, atau bahwa bytes tersebut dapat dipulihkan pada permukaan locked yang sama sambil mempertahankan Spotlight.

`SetImageStreamAsync` mempunyai kontrak mengatur gambar dari stream.[13]

Kontrak tersebut belum membuktikan kebutuhan pemulihan pixels Spotlight pada lock screen yang sedang aktif. Capture yang berhasil sekalipun tidak otomatis menyelesaikan pemulihan mode/provider dan repaint.

`TrySetLockScreenImageAsync` mencoba memasang gambar dari file dan mengembalikan hasil operasi tersebut.[15]

`SetImageFileAsync` mengatur gambar lock screen dari file.[17]

Keberhasilan setter/configuration berbeda dari bukti visual pada sesi locked yang terus sama. Tidak ada dasar untuk menyebut kegagalan ini pasti cache, menambah durasi sebagai solusi, atau memaksa LockApp restart.

## Perilaku yang dipertahankan

- Opt-in penggantian gambar, auto-lock, konfigurasi, Startup, journal recovery, dan mitigasi deadline yang sudah ada tidak dihapus atau diaktifkan ulang oleh keputusan SKIP.
- Pengaturan mode asal tetap dicoba dipulihkan oleh alur yang ada; untuk Spotlight/Slideshow jangan menganggap status mode sebagai jaminan gambar aktif pulih tepat setelah durasi.
- Tidak ada desktop-wallpaper fallback, elevasi, unlock→lock otomatis, perubahan power plan global, atau force-kill LockApp.
- Fitur Taskbar Window native merupakan opsi terpisah; keberhasilannya tidak mengubah penerimaan lock screen ini.

## Riwayat gambar di Windows (Recent pictures)

Gambar sholat dan cadangan Picture asal memakai **nama GUID baru per transaksi** di `%LOCALAPPDATA%\PrayerAutomation\LockScreen`, dengan prefix terkelola yang sama seperti sebelumnya. Pemeriksaan kepemilikan, pembersihan orphan, dan perlindungan terhadap penimpaan berkas yang sedang ditampilkan Windows tetap berlaku.

Path + SHA256 tetap diverifikasi seperti sebelumnya. Berkas yang masih ditampilkan Windows dengan isi berbeda **tidak ditimpa**; transaksi gagal dengan aman daripada melemahkan restore. `SetImageFileAsync` dipertahankan karena `SetImageStreamAsync` dapat membuat `OriginalImageFile` tidak menyediakan path yang sama untuk verifikasi.

Windows mengikat turunan gambar lock screen ke **path sumber**, bukan ke isi berkas. Memakai ulang satu path membuat Windows membuka slot cache lama yang bisa berisi turunan basi atau kosong. Bukti pengguna pada satu mesin yang sama: setelah isi `SystemData` dikosongkan, mode Picture/Spotlight/Slideshow normal, tetapi uji 2.4.0 dengan path stabil hanya menghasilkan hitam pekat; 2.3.1 dengan path GUID menghasilkan slot `LockScreen_D`/`LockScreen_E` dan gambar sholat yang benar. Path GUID memaksa Windows mengalokasikan slot baru tanpa UAC dan tanpa menyentuh cache sistem.

Konsekuensinya, entri `Recent pictures` dapat bertambah. Aplikasi tidak menghapus entri lama. Bila perlu, bersihkan manual lewat **Settings → Personalization → Lock screen → Browse/riwayat gambar**. Jangan mengubah Registry, menaikkan privilege, restart LockApp, atau menghapus paksa cache sistem untuk tujuan ini.

Bagian ini tidak mengubah batas verifikasi: status tetap `verification=config-only visual=unverified` sampai penerimaan visual dilakukan pada Windows nyata.

## Updater menunggu pemulihan

Updater 2.4.0 memanggil pemulihan paksa sebelum memasang. Ia menahan hingga **60 detik**, mencoba ulang tiap **5 detik**, dan membatalkan pembaruan bila jurnal masih ada; jurnal tidak dihapus. Opsi **Nanti saja** menutup jendela, menghitung mundur di tray sampai pemulihan lock screen selesai (fallback 15 menit bila tidak ada pemulihan aktif), lalu popup aplikasi menanyakan lagi apakah pembaruan mau dipasang. Menutup jendela saat menunggu juga membatalkan pembaruan dan mengembalikan penjadwalan normal tanpa menghapus jurnal. Ini hanya gerbang keselamatan berbasis jurnal dan verifikasi konfigurasi, bukan bukti visual; status tetap `visual=unverified`.

## Korelasi dengan pemulihan cache manual (kasus pengguna)

Pengguna melaporkan: setelah gambar diganti ke Picture mana pun atau slideshow folder mana pun, layar yang dikunci tetap menampilkan gambar sholat. Pemulihan yang berhasil dijalankan bukan mengubah sumber, melainkan **merename cache turunan Windows** di `%ProgramData%\Microsoft\Windows\SystemData\<SID>\ReadOnly\LockScreen_A…G\LockScreen___<lebar>_<tinggi>_notdimmed.jpg` supaya Windows membuatnya ulang. Sumber `LockScreen.jpg` saat itu sudah benar. Ini bukti pengguna, bukan hasil uji aplikasi.

Kasus ini masuk batas `visual=unverified`: path + SHA256 dan mode flag yang benar tidak membuktikan pixels LockApp berubah. Gejala serupa juga bisa muncul bila mode Picture masih mengarah ke berkas aplikasi setelah restore visual gagal; pada kasus ini bukti yang ada menunjukkan jalur cache turunan yang basi.

Korelasinya dengan regresi awal 2.4.0: versi itu sempat mengganti path GUID dengan satu nama stabil per peran. Panduan paket pemulihan manual mencatat bahwa reset satu slot hanya sementara karena pemilihan gambar lain dapat memakai slot riwayat lain yang masih berisi cache lama. Uji A/B setelah cache dikosongkan mengonfirmasi bahwa path stabil menghasilkan hitam, sedangkan path GUID menghasilkan slot baru dan gambar yang benar. Kini perilaku GUID dikembalikan, dengan seluruh jaminan safety 2.4.0 dipertahankan.

Batasnya tetap tegas: paket pemulihan itu tindakan manual satu kali untuk cache yang sudah basi, bukan bagian aplikasi. Ia tidak menghapus entri `Recent pictures`, tidak mengubah config/journal aplikasi, dan tidak mengubah perilaku aplikasi. Aplikasi tidak mengadopsi UAC, penghentian LockApp, rename cache sistem, atau penghapusan isi `SystemData`; penghapusan manual yang pernah dilakukan pengguna adalah pemulihan satu kali dan sebaiknya tidak diulang tanpa kebutuhan nyata. Entri lama tetap dibersihkan manual lewat Settings Windows.

Riset lengkap, 10 sumber primer/14 kutipan literal, dan bukti transaksi disimpan dalam arsip finalisasi di luar repo. Dokumen ini tidak menyatakan uji visible lock screen baru telah dilakukan.

## Sources

[12] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.getimagestream?view=winrt-26100 — LockScreen.GetImageStream Method
[13] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagestreamasync?view=winrt-26100 — LockScreen.SetImageStreamAsync(IRandomAccessStream) Method
[15] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.userprofilepersonalizationsettings.trysetlockscreenimageasync?view=winrt-26100 — UserProfilePersonalizationSettings.TrySetLockScreenImageAsync Method (Windows.System.UserProfile) - Windows apps | Microsoft Learn
[17] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagefileasync?view=winrt-26100 — LockScreen.SetImageFileAsync(IStorageFile) Method (Windows.System.UserProfile) - Windows apps | Microsoft Learn
