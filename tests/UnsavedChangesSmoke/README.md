# UnsavedChangesSmoke — inert actual-C#

Menggunakan assembly portable asli, bukan implementasi ulang Python. Runner hanya menjalankan `dotnet UnsavedChangesSmoke.dll`; seluruh EXE produk dikecualikan dari fixture.

## Menjalankan portable final milik parent

```bash
python tests/UnsavedChangesSmoke/run_ui.py --product "<folder-portable-final>" --suite all --label parent-final
```

Direktori portable juga dapat diberikan sebagai argumen positional. `AyoJamaah.dll` dideteksi terlebih dahulu; nama legacy `PrayerAutomation.UI.dll` tetap didukung. `--help` sudah dijalankan dan kedua bentuk argumen sudah digunakan pada eksekusi nyata.

Suite terfokus: `close-cancel`, `minimize-cancel`, `discard-cancel`, `exit-cancel`, `dirty`, `decisions`, `failures`, `save`, `dialog`. `all` menjalankan sembilan batch terikat tersebut, bukan full regression aplikasi. Eksekusi dibatasi sebelum pukul 16:00. `--expect-red` mensyaratkan assertion failure (exit 1); setup error tidak diterima sebagai RED.

## Isolasi

- Callback pending Dispatcher dari ctor `Application` dicocokkan tepat (`Application.<.ctor>b__1_0`, priority Send), diabort tepat sekali sebelum pump pertama. Perubahan shape framework harus dianggap setup blocker.
- Delapan objek startup (`mutex`, tray, reminders, auto-lock, lock-screen scheduler, widget, taskbar widget, taskbar panel) harus tetap null pada setiap batch.
- Pengaturan sintetis baru menggunakan delapan consequential flags false; persistence mock juga menolak candidate dengan satu pun flag true. Flag UI dapat dibuat true secara inert untuk dirty detection/invalid validation, tetapi tidak disimpan.
- Ctor lima-argumen lama tetap tersedia. Ctor enam-argumen baru menerima logger counter/no-op sebelum inisialisasi. Default produksi tetap `AppLogger.Write`; tidak ada patch DLL/path logger. Fallback reflection pada fixture dipakai hanya untuk RED seam ctor terhadap candidate sebelumnya yang sudah mempunyai field logger inert.
- MainWindow tidak `Show`; clock dan location timer dihentikan, GPS/network service fields dinul, persistence/startup/safety/preview/dependent-view callbacks inert. Fixture memasang DispatcherSynchronizationContext.
- Native `prayer_core.dll` yang dipin hanya dipakai untuk perhitungan kalender/jadwal lokal. SHA256 wajib `7f18fd13ed58430475904d38914575ca421f408381d62c43c6041d1e2e525858`. Native core tidak dibangun ulang.
- Profil asli, registry startup asli, GPS/network, personalization, auto-lock, NotifyIcon, SingleInstance smoke, dan produk EXE tidak dibaca/dijalankan/ditulis. Tidak ada profile-hash probe.
- Dialog choices/handlers dan layout content diuji offscreen; tidak ada modal automation atau Window.Show.
- Build harness memakai source copy baru ke scratch, offline NuGet config parent, dan obj terpisah. Tidak menulis bin/obj canonical. Candidate UI dibangun oleh `build_frozen.py` dari immutable branding-03 plus empat source popup, dengan props/targets isolation milik parent.

## Bukti final slice

Evidence root: `<evidence>/2026-11-11-finalization/popup-resume`.

| Slice | RED nyata | GREEN nyata |
|---|---|---|
| Batal → confirmation | `run-red-Batal-resume-e3e2db9e-RESULT.json`: 4 checks, 3 gagal, DLL branding-03 | `run-green-Batal-resume-e1e4ac32-RESULT.json`: 4/4 PASS, portable-Batal-01 |
| Central exit → confirm before guard | `run-red-exit-resume-45f99d29-RESULT.json`: 4 checks, 2 gagal, portable-Batal-01 | `run-green-exit-resume-8205337c-RESULT.json`: 4/4 PASS, portable-exit-01 |
| Ctor-injected inert logger | `run-red-constructor-logger-1302d477-RESULT.json`: 14 checks, 1 gagal, portable-final-01 | `run-green-constructor-logger-bacbd406-RESULT.json`: 14/14 PASS, portable-final-02 |

Final selected run: `run-final-bounded-86b3a529-RESULT.json`: **99/99 PASS**, setup_error null, callback startup aborted 1, synthetic writes 4 (all child-owned), save attempts 5 (one synthetic IOException), startup mock calls 4, inert logger calls 1, unhandled exceptions 0.

Portable slice: `<evidence>/2026-11-11-finalization/popup-resume/portable-final-02`.
UI SHA256: `f383e3d89e8cb4acff07aa3d5a0abcd87ce22a46bfa09a8177538b07eab45d65`.
Final harness SHA256: `bc12975c261316fd0e7d9809939697d38269bd03e360de132bc6c7cccfc2d082`.
`BUILD-final-02.json` binds immutable source hashes, portable payload hashes, build/publish exit 0 and zero warnings. The final execution receipt binds actual portable DLL, fixture DLL, harness/source SHA, runner SHA, all 738 copied runtime files, and the omitted AyoJamaah.exe. Intermediate reports remain retained, including a harness warning setup failure and a partial 73-check setup failure from using incorrect JSON property names; those are not counted as final PASS or feature RED.

Coverage: clean/edit/revert, calendar navigation exclusion, all 30 raw values (all 7 prayer corrections, both date corrections, disabled check flags, raw theme/widget tags), Save/direct button/X/minimize success, Discard, Cancel, Batal discard-only, failed validation/save retaining full draft, startup/save callback counts and reentrancy, X/minimize cancellation, central exit cancellation/nested guard ordering, default Cancel + real dialog handlers and offscreen content measure.

## Batas bukti

Normal UI/UX acceptance tetap milik user. Main-window actual visibility/focus/modal pixels, actual tray integration, registry registration, and native lock-screen restoration are not exercised. Successful central exit decision gating is tested through Confirm directly; the full successful App.Shutdown dispatcher path is intentionally not executed. PendingVerification/exception recovery refusal and disposal order are preserved in source; no fake scheduler/native restoration seam was introduced to claim runtime verification.

Source changes left uncommitted: MainWindow.xaml.cs and App.xaml.cs. Existing UnsavedChangesDialog files retained unchanged. No branding/project/namespace/backend/mutex/profile/installer ID changes in this slice.
