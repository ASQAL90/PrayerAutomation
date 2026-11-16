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

Riset lengkap, 10 sumber primer/14 kutipan literal, dan bukti transaksi disimpan dalam arsip finalisasi di luar repo. Dokumen ini tidak menyatakan uji visible lock screen baru telah dilakukan.

## Sources

[12] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.getimagestream?view=winrt-26100 — LockScreen.GetImageStream Method
[13] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagestreamasync?view=winrt-26100 — LockScreen.SetImageStreamAsync(IRandomAccessStream) Method
[15] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.userprofilepersonalizationsettings.trysetlockscreenimageasync?view=winrt-26100 — UserProfilePersonalizationSettings.TrySetLockScreenImageAsync Method (Windows.System.UserProfile) - Windows apps | Microsoft Learn
[17] https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen.setimagefileasync?view=winrt-26100 — LockScreen.SetImageFileAsync(IStorageFile) Method (Windows.System.UserProfile) - Windows apps | Microsoft Learn
