# Pemasang Ayo Jama'ah! 2.1.0

## Build branding terbaru

Nama tampilan `Ayo Jama'ah!`, executable `AyoJamaah.exe`, ikon `AyoJamaah.ico`, installer `AyoJamaah.Setup-2.1.0.exe`. Identitas AppId, profil, mutex, Run value serta lokasi pemasangan lama dipertahankan untuk kompatibilitas. Gunakan [panduan installer aktif](../docs/INSTALLER_ID.md), bukan command/hash 2.0.0 historis di bawah.

`generate_accepted_payload.py --receipt <receipt-final> --source <portable-final> --output <include-baru>` menerima receipt dengan `assembly_name=AyoJamaah`, `review_status=TECHNICAL_CHECKS_PASSED_UI_ACCEPTANCE_PENDING`, `file_count`, `ui_sha256` dan map `payload` yang identik dengan seluruh file nyata. `AyoJamaahPayload.iss` dipakai installer aktif; `AcceptedPayload.iss` lama hanya riwayat. Setiap file tetap di-hash-pin saat compile; perubahan payload memerlukan receipt/review baru, bukan wildcard.

Tidak menjalankan installer/produk pada profil asli, tidak force-close, tidak menghapus jurnal. Opsi Jalankan dan shortcut desktop tetap unchecked. Pengamanan recovery dan penolakan override tidak dilonggarkan oleh pergantian merek. Pemeriksaan nama executable/entrypoint lama-baru diuji dalam compiled native Inno fixture tanpa instalasi.

## Riwayat installer 2.0.0 — bukan petunjuk rilis aktif

Pemasang ini membungkus **runtime C# + Rust yang sudah diterima**, bukan membangun ulang produk. Nomor 2.0.0 adalah versi paket pemasangan; versi assembly/runtime tidak diubah. Identitas Inno tetap `{8B76B6D5-1B3C-4E13-9A39-000000000001}`.

> **Pengaman jurnal diperbaiki; penerimaan lifecycle native masih terpisah.** Source kini menolak state pemulihan persisten meskipun proses/mutex sudah hilang setelah crash/reboot. Bukti RED→GREEN terfokus tersedia di bawah. Artefak lama SHA-256 `b47583cdcc434f649c2dbc4e027b455be4320ae9143c487abeb34be19c6d5886` dan uninstaller lamanya **tidak ikut diperbaiki**: jangan gunakan sebagai bukti pengaman baru. Instalasi normal yang dilaporkan pengguna tidak menguji journal-only; profil pengguna aktif tidak diperiksa oleh fixture ini.
>
> Receipt: `<evidence>/2026-10-30-installer-release/installer-SPEC-blocker.json`. 25 PASS di bawah tetap merupakan bukti terfokus, bukan coverage blocker ini. Lihat [milestone](../docs/progress/MILESTONE_2026-10-30_ID.md).

## Build yang dapat diulang

Jalankan dari Git Bash di root repo, tanpa elevasi:

```bash
export TMPDIR="$LOCALAPPDATA/PrayerAutomationSmoke/scratch/prayerautomation-journal-guard-20261102"
export TMP="$TMPDIR" TEMP="$TMPDIR"
"$LOCALAPPDATA/Programs/Inno Setup 6/ISCC.exe" \
  '/DMyAppSourceDir=../dist/portable' \
  '/O../dist/installer-output/journal-guard' \
  installer/PrayerAutomation.iss
```

Wajib pilih `/O` baru/kosong; jangan timpa direktori installer 2.0.0 lama. Default source/output menunjuk penyimpanan rilis di luar repositori, tetapi contoh ini sengaja memakai output terpisah. `/DMyAppSourceDir` hanya boleh menunjuk salinan payload yang cocok dengan receipt diterima; tidak perlu mempertahankan `dist` lama. `AcceptedPayload.iss` memuat 485 nama berkas eksplisit dengan pemeriksaan SHA-256 ISPP untuk masing-masing berkas. Perbedaan satu berkas menghentikan compiler. Berkas ekstra tidak dikemas. DLL UI yang diterima: `52490456d29dd7903bfba2f41202b2f52e03d297638a615cef82434aca91c8ac`.

`generate_accepted_payload.py` memvalidasi seluruh receipt sebelum menghasilkan pin. Jangan gunakan receipt lain atau membangun ulang runtime untuk mengganti pin tanpa penerimaan baru.

## Batas keselamatan

- Hanya pengguna saat ini, tanpa UAC/admin: `%LOCALAPPDATA%\Programs\PrayerAutomation`. Lokasi `/DIR` lain ditolak, termasuk direktori konfigurasi; ancestor junction/symlink juga ditolak.
- HKCU/HKLM uninstall metadata 32/64-bit hanya dibaca untuk memberitahu instalasi sebelumnya. Pemasang tidak menghapus instalasi Python, startup, atau pengaturan lama. Metadata uninstall HKCU dengan AppId sama dapat diperbarui setelah persetujuan; jangan jalankan uninstaller Python lama pada folder versi baru.
- Proses bernama produk dan Python/dotnet dengan entrypoint/lokasi produk diperiksa melalui WMI. Identitas pemilik dibandingkan dengan SID pengguna pemasang (`GetOwnerSid`); lokasi image dan sesi dibaca lewat Win32. Interpreter umum tanpa penanda aplikasi tidak diblokir secara membabi buta. Identitas kandidat yang tidak dapat diverifikasi menjadi penghalang.
- Tidak ada IPC/argumen shutdown yang didukung runtime. Karena X/minimize menyembunyikan jendela, pemasang **tidak mengirim WM_CLOSE**, tidak memakai Restart Manager shutdown, dan tidak menjalankan taskkill/TerminateProcess. Pilihan Ya hanya mengizinkan pengguna memilih **Keluar** sendiri. Coba Lagi memeriksa proses/mutex/berkas lagi; Tidak, Batal, atau proses yang belum selesai menghalangi perubahan aplikasi.
- Penolakan Keluar karena pemulihan lock screen belum terverifikasi tetap dihormati. Pemasang tidak mencoba memperbaiki, menghapus jurnal, atau melewati pengaman tersebut.
- `%LOCALAPPDATA%\PrayerAutomation\LockScreen` diperiksa secara read-only, bukan direktori konfigurasi `%APPDATA%`. Semua nama berawalan `lock-screen-restore.json` menolak operasi: jurnal biasa, `.invalid-*`, `.writing-*`, suffix tidak dikenal, atau direktori dengan nama jurnal. Keberadaan jurnal adalah transaksi tertunda; nilai `RestoreRequested`/`IsApplied`, JSON null/rusak, dan umur berkas tidak membolehkan bypass. Tidak ada parser atau cleanup jurnal baru.
- State hanya dianggap tidak ada setelah chain direktori/induk biasa dapat diverifikasi. Non-directory, unreadable, junction/symlink/reparse, kegagalan identitas handle, atau enumerasi tidak selesai menolak operasi. Handle hanya `LIST_DIRECTORY`, `OPEN_EXISTING`, tanpa hak tulis atau elevasi. Enumerasi Win32 eksplisit membedakan `ERROR_NO_MORE_FILES` dari kegagalan; `DLLGetLastError` tidak mewakili hasil built-in FindNext Inno.
- Setelah pemeriksaan bersih, pemasang menahan mutex produk melalui tahap pemasangan/uninstall, lalu memeriksa lagi tepat sebelum penulisan/penghapusan. Pemeriksaan eksklusif berkas memakai `OPEN_EXISTING` tanpa menulis isi. Setiap berkas payload diperiksa lagi sebelum disalin.
- Pemasangan/uninstall silent tidak didukung. Override `/CLOSEAPPLICATIONS`, `/FORCECLOSEAPPLICATIONS`, `/RESTARTAPPLICATIONS`, dan `/ALLUSERS` ditolak.
- Desktop shortcut dan pilihan menjalankan aplikasi setelah pemasangan **tidak dicentang**. Tidak ada pendaftaran startup baru. Setelah pengguna sengaja menjalankan aplikasi, pengaturan tersimpan dapat kembali mengaktifkan startup/lock sesuai perilaku produk.
- Tidak ada `[Registry]`, `[InstallDelete]`, `[UninstallDelete]`, atau `[UninstallRun]`. Uninstall hanya mengikuti berkas/pintasan yang dicatat Inno; `%APPDATA%\PrayerAutomation` dan data pengguna tidak menjadi target penghapusan. Skema konfigurasi lama tidak dimigrasikan atau dijamin kompatibel.

## Bukti dan keterbatasan

Bukti rilis tersimpan di `<evidence>/2026-10-30-installer-release/child-installer/`:

- `red-old-installer.json`: skrip lama gagal empat batas keselamatan sebelum penggantian.
- `compile-final.log`: Inno Setup **6.7.3**, compiler berhasil memakai source rilis durable.
- `pinned-payload-rejection.log`: berkas fixture rusak benar-benar ditolak compiler.
- `focused-verification.json`: **25 pemeriksaan terfokus PASS**. Meliputi SID/image/sesi, Python terkait vs interpreter netral, penolakan izin, Ya lalu Batal, retry dengan proses tetap hidup, berkas terkunci, namespace mutex sintetis, penolakan silent/override oleh installer produksi terkompilasi, serta uninstaller Inno nyata yang mempertahankan sentinel saat ditolak/dibatalkan.
- `release-receipt.json`: hash artefak/manifest PE/source pendukung, bukti 485 pin compiler berhasil, serta hash UI/native yang diperiksa kembali setelah pengujian.

Fixture `BoundaryProbe.iss` tidak memasang produk. `LifecycleProbe.iss` hanya memasang sentinel + uninstaller di scratch dengan `CreateUninstallRegKey=no`, tanpa pintasan, registry, atau payload produk. Namespace mutex fixture berbeda dari produk. Script verifikasi hanya mengeklik dialog milik proses fixture sendiri; worker Python sintetis selesai melalui stdin, bukan force kill.

**Belum diverifikasi:** pemasangan/upgrade/uninstall lengkap runtime pada VM bersih; notifikasi positif registry instalasi Python nyata (key mesin saat pengujian memang tidak ada); pengguna/sesi kedua; hasil sukses penutupan aplikasi nyata; verifikasi native lock-screen/Explorer. Aplikasi pengguna yang sudah berjalan dibiarkan berjalan. Tidak ada claim bahwa source test menggantikan instalasi bersih atau UI acceptance.

Mutex global yang tidak dapat dipastikan pemiliknya sengaja menghalangi operasi, termasuk kemungkinan berasal dari pengguna/sesi lain. Legacy yang diganti nama/entrypoint-nya dan tidak memakai penanda produk tidak selalu dapat dikenali. Proses legacy tanpa protokol mutex bisa mulai setelah preflight; pengecekan per-berkas dan proteksi sharing Windows tetap berlaku, tetapi tidak ada jaminan atomik lintas-proses tanpa kerja sama runtime lama. Tidak ada code signing; kebijakan SmartScreen/distribusi perlu ditangani terpisah.

## Verifikasi berikutnya oleh operator

### Bukti pengaman jurnal (2026-11-02)

`<evidence>/2026-11-02-installer-journal-guard/` menyimpan `red-receipt.json`, `green-receipt.json`, log compiler/probe, `compile-receipt.json`, `payload-receipt.json`, `source-pin-receipt.json`, dan `artifact-receipt.json`. RED native memanggil sembilan batas/callback dengan jurnal tunggal tanpa proses dan semuanya keliru mengizinkan. GREEN: **29 skenario / 266 assertion PASS**, termasuk kedua Boolean install/uninstall, preflight, held-mutex recheck, `ssInstall`/`usUninstall`, dan callback tiap payload. Jurnal yang muncul setelah preflight bersih menolak ketiga jalur recheck.

`installer/tests/verify_persistent_recovery.py` mengompilasi salinan scratch dari guard dan callback produksi. Hanya seam proses-absen, path compile-time milik fixture, namespace mutex Local khusus, serta dialog/native host-context uninstall yang disubstitusi. Setup host tidak mengizinkan API `UninstallSilent`; callback uninstall diuji dalam konteks non-silent sintetis. **Ini bukan eksekusi host uninstaller native.** `InitializeSetup` fixture selalu mengembalikan False, `CreateAppDir=no`, `Uninstallable=no`; tidak ada pemasangan/penghapusan, payload runtime, registry, atau profil asli yang diakses. ACL hanya berubah pada direktori scratch khusus, kemudian DACL asli dipulihkan. Tidak ada runtime/About rebuild, product/LockApp launch/close, admin, atau PowerShell.

Jalankan ulang dengan lokasi scratch baru (runner menolak memakai folder fase yang sudah ada):

```bash
python installer/tests/verify_persistent_recovery.py \
  --repo . \
  --scratch "$LOCALAPPDATA/PrayerAutomationSmoke/scratch/prayerautomation-journal-guard-rerun" \
  --report ../dist/evidence/journal-guard-rerun \
  --iscc "$LOCALAPPDATA/Programs/Inno Setup 6/ISCC.exe" \
  --phase green
```

Suite lama 25 PASS tidak dijalankan ulang karena membaca registry/profil/proses produk dan memasang lifecycle fixture; itu melampaui izin tugas ini. Compile installer baru memakai **payload lama 485 berkas**, bukan portable About baru. Artefak compiler tidak dijalankan. Coverage crash/reboot/recovery tampilan native, lifecycle penuh clean-VM termasuk host uninstall sebenarnya, user/sesi kedua, dan upgrade tetap belum diverifikasi. Mutex menutup runtime kooperatif; tidak ada jaminan atomik terhadap actor legacy/non-kooperatif yang membuat state sesudah recheck terakhir.

Gunakan VM/profil Windows sintetis, bukan profil pengguna aktif. Pasang tanpa elevasi dengan opsi Jalankan tetap kosong; bandingkan seluruh 485 berkas terhadap receipt dan periksa shortcut/uninstaller HKCU. Uji Python lama dengan Keluar manual (bukan X), Tidak/Batal, serta Keluar yang tertunda; pastikan tidak ada salinan/penghapusan sebelum proses selesai. Uji uninstall sambil proses aktif dan sesudah keluar normal, lalu pastikan konfigurasi/startup lama tetap ada. Jangan memakai force-close command line atau menghapus state pemulihan untuk menyelesaikan pengujian.
