# Arsip Python — bukan panduan build C# + Rust

> **Status 30 Oktober 2026:** source Python, helper, task XML/batch, spec dan mockup lama telah diarsipkan di luar repo setelah verifikasi SHA256. Backup lokal: `../PrayerAutomation-archives/2026-10-30-cleanup/legacy-python/` relatif terhadap root repository; `manifest.json` mencatat seluruh file. Riwayat Git dipertahankan. Isi historis berikut bukan source yang masih dipaketkan atau prosedur installer aktif.

> **Pembaruan 11 November 2026:** salinan arsip `igcmdWin10.exe` telah dihapus atas pilihan eksplisit pemilik setelah dikonfirmasi tidak dipakai pada proyek/paket C# + Rust aktif. Manifest arsip lama tetap menjadi bukti snapshot awal; arsip sekarang tidak lengkap satu binary. Git history tidak dihapus. Lihat [audit lanjutan](REPOSITORY_AUDIT_ID.md#pemeriksaan-lanjutan-igcmdwin10exe).

> **ARSIP HISTORIS.** Isi di bawah dipertahankan untuk perbandingan versi Python lama. Nama executable, UAC, HKLM/CSP, helper pihak ketiga, BMP, dan mekanisme state lama **tidak berlaku untuk portable C# + Rust saat ini**. Jangan mengikuti langkah administrator di arsip ini untuk menguji build baru. Gunakan [dokumentasi aktif](prompts/PROGRAM_DOCUMENTATION_ID.md) dan [checklist pengguna biasa](UI_ACCEPTANCE_ID.md). Detail lama yang belum diverifikasi ulang bukan jaminan perilaku versi Python.

---

# Prayer Automation — Dokumentasi Program

## 1. Tujuan

Prayer Automation adalah aplikasi Windows portable untuk memantau jadwal sholat, menampilkan pengingat teks, mengunci komputer pada waktu sholat, mengganti **lock-screen picture saja**, lalu mencoba memulihkan pengaturan lock screen sebelumnya setelah 60 menit.

Aplikasi tidak mengeluarkan suara, tidak memainkan audio adzan, dan tidak menggunakan API desktop wallpaper.

## 2. Cara menjalankan

1. Jalankan `PrayerAutomationRestoreFix.exe`.
2. Setujui prompt UAC dengan memilih **Yes/Ya**.
3. Ikon aplikasi muncul di system tray.
4. Buka menu tray untuk melihat jadwal, widget, atau Pengaturan.
5. Untuk keluar, gunakan menu tray **Keluar**.

UAC diperlukan karena implementasi lock screen menggunakan device-scoped registry HKLM. Tanpa hak administrator, pengaturan tersebut dapat ditolak Windows.

## 3. Alur keseluruhan

Flowchart visual lengkap tersedia di:

`program_full_flowchart.html`

Urutan utamanya:

```text
START
  ↓
UAC / administrator
  ↓
main()
  ↓
load_config()
  ↓
inisialisasi icon, tray, widget, dan worker
  ↓
pilih sumber lokasi
  ↓
ambil lokasi dan elevasi bila diperlukan
  ↓
build parameter metode
  ↓
ambil jadwal dari cache/API
  ↓
terapkan koreksi manual
  ↓
worker memantau jadwal setiap 20 detik
  ├─ H−3 menit → popup teks
  ├─ waktu sholat → simpan state → ganti lock screen → lock Windows
  └─ deadline +60 menit → restore lock screen
  ↓
kembali memantau jadwal
  ↓
exit_app()
  ↓
END
```

## 4. Konfigurasi

File konfigurasi aktif:

```text
%APPDATA%\\PrayerAutomation\\config.json
```

Konfigurasi mencakup:

- koordinat manual;
- timezone;
- sumber lokasi: manual, GPS, atau network;
- metode perhitungan;
- koreksi menit per waktu;
- pengaturan popup;
- lock komputer;
- penggantian lock-screen picture;
- startup Windows.

Perubahan lokasi atau metode tidak langsung disimpan. Program mengambil preview jadwal terlebih dahulu. Konfigurasi baru disimpan setelah pengguna menyetujuinya.

## 5. Sumber lokasi

### Manual

Koordinat yang tersimpan digunakan langsung. Ini adalah sumber yang paling stabil untuk lokasi yang sudah dikonfirmasi pengguna.

### Windows Location Services

`detect_windows_gps()` menggunakan Windows Location Services melalui API asynchronous. Hasilnya digunakan jika tersedia dan valid.

### Network/IP fallback

`detect_network_location()` memakai provider geolokasi jaringan sebagai perkiraan. IP proxy, gateway, VPN, atau jaringan perusahaan tidak otomatis dianggap sebagai lokasi fisik yang pasti. Tidak ada aturan hardcode untuk negara, kota, operator, atau organisasi tertentu.

## 6. Pengambilan jadwal

### `get_elevation(latitude, longitude)`

Mengambil elevasi berdasarkan koordinat dan menyimpan cache elevasi per koordinat.

### `build_timing_params(method, ...)`

Membangun parameter request API, termasuk metode, koordinat, timezone, method settings, dan tune.

### `fetch_timings(date, location, method)`

Mengambil jadwal dari provider API.

### `schedule_cache_key(...)`

Membuat kunci cache berdasarkan tanggal, koordinat, timezone, metode, dan koreksi. Perubahan metode atau koreksi tidak menggunakan cache yang salah.

### `apply_manual_corrections(timings, corrections)`

Menerapkan koreksi manual pada Fajr, Sunrise, Dhuhr, Asr, Maghrib, dan Isha. Rentang valid adalah `-30` sampai `+30` menit.

### `get_timings(date)`

Menggabungkan cache, API, elevasi, parameter metode, dan koreksi manual untuk menghasilkan jadwal aktif.

## 7. Event jadwal

Waktu yang ditampilkan:

```text
Subuh / Fajr
Terbit / Sunrise
Dzuhur / Dhuhr
Ashar / Asr
Maghrib
Isya / Isha
```

Terbit hanya untuk tampilan dan tidak memicu:

- popup sholat;
- penguncian komputer;
- penggantian lock-screen picture.

Event sholat aktif:

```text
Fajr, Dhuhr, Asr, Maghrib, Isha
```

## 8. Popup teks

### `show_popup(title, message)`

Menampilkan popup Tkinter dengan teks pengingat. Tidak ada suara, adzan audio, atau pemanggilan audio Windows.

Worker menjaga daftar event agar popup tidak berulang untuk event yang sama.

## 9. Lock-screen picture

### `set_lock_screen_wallpaper(path)`

Urutannya:

1. Membaca value lock-screen yang tersedia.
2. Menyimpan snapshot ke memori dan `previous_lock_screen.json`.
3. Menulis policy/CSP device-scoped ke HKLM.
4. Menjalankan helper:

   ```text
   igcmdWin10.exe setlockimage <path-baru>
   ```

5. Menyimpan deadline restore ke `lock_screen_deadline.json`.
6. Mengunci komputer.

Aplikasi tidak memakai:

```text
SystemParametersInfoW(20, ...)
```

karena API tersebut mengubah desktop wallpaper. Desktop wallpaper tidak boleh disentuh.

## 10. Penyimpanan state sebelumnya

File:

```text
%LOCALAPPDATA%\\PrayerAutomation\\previous_lock_screen.json
```

File ini menyimpan value registry yang ditemukan sebelum perubahan, termasuk informasi value yang sebelumnya tidak ada.

Deadline disimpan terpisah:

```text
%LOCALAPPDATA%\\PrayerAutomation\\lock_screen_deadline.json
```

Deadline berupa waktu absolut, sehingga recovery dapat dilakukan setelah restart aplikasi.

## 11. Restore

### `restore_lock_screen_wallpaper()`

Setelah deadline tercapai:

1. Memuat snapshot lama jika diperlukan.
2. Memulihkan value registry satu per satu.
3. Menghapus value yang sebelumnya memang tidak ada.
4. Memanggil helper dengan path lama jika path valid.
5. Memastikan error registry/helper tidak disamarkan.
6. Menghapus state hanya jika restore selesai tanpa error.

Jika restore gagal, file state dipertahankan dan popup error ditampilkan. Dengan begitu, informasi recovery tidak hilang.

### Batasan restore

Aplikasi tidak mengambil screenshot lock screen. Jika lock screen lama berasal dari Windows Spotlight, MDM, atau policy yang tidak memberikan path file yang dapat dibaca, aplikasi mungkin dapat memulihkan registry tetapi tidak dapat menjamin gambar visual persis sebelumnya kembali.

## 12. Worker

### `worker(stop, icon)`

Loop utama berjalan selama aplikasi aktif:

1. membaca waktu sekarang;
2. mengambil jadwal hari ini;
3. memeriksa popup H−3 menit;
4. memeriksa event sholat;
5. melakukan snapshot dan replacement;
6. mengunci komputer;
7. memeriksa deadline restore secara global;
8. memulihkan lock screen jika waktunya tercapai;
9. menunggu polling berikutnya.

Deadline restore tidak bergantung pada iterasi event sholat.

## 13. UI dan tray

### `open_schedule_widget()`

Membuka widget ringkas berisi jadwal hari ini dan tombol muat ulang.

### `open_settings()`

Membuka pengaturan sumber lokasi, metode, koreksi, reminder, startup, lock komputer, dan lock screen. Preview dilakukan sebelum konfigurasi disimpan.

### `make_tray_image()`

Membuat gambar/icon untuk system tray.

### `apply_window_icon(window)`

Menerapkan file ICO yang sama pada window Tkinter dan native Windows taskbar.

### `write_user_startup(enabled)`

Mengatur startup per-user melalui registry HKCU. Tidak memakai Scheduled Task administrator.

## 14. Fungsi pendukung

| Fungsi | Tanggung jawab |
|---|---|
| `load_config()` | Membaca dan menormalisasi konfigurasi |
| `save_config()` | Menyimpan konfigurasi aktif |
| `update_flag()` | Memperbarui status konfigurasi |
| `schedule_text()` | Membuat teks jadwal ringkas |
| `timings_text()` | Membuat teks lengkap termasuk Terbit |
| `lock_workstation()` | Memanggil penguncian Windows |
| `ensure_wallpaper()` | Memastikan asset BMP tersedia |
| `exit_app()` | Menghentikan worker dan tray |
| `main()` | Entry point dan inisialisasi program |

## 15. File penting

```text
prayer_automation.py             source utama
popup_text.py                    popup teks
create_wallpaper.py              generator wallpaper
sedang_sholat.bmp                wallpaper lock screen
igcmdWin10.exe                   helper lock screen
prayer_automation.ico            icon aplikasi
program_full_flowchart.html      flowchart lengkap
previous_lock_screen.json        snapshot lock screen lama
lock_screen_deadline.json        deadline restore
config.json                      konfigurasi aktif
```

## 16. Verifikasi dan batasan

Pengujian source-level mencakup parameter jadwal, metode, cache, koreksi, Sunrise, icon, helper lock screen, wallpaper, dan recovery deadline.

Namun runtime Windows tetap dapat dipengaruhi oleh:

- UAC dan izin administrator;
- Group Policy;
- MDM organisasi;
- Windows Spotlight;
- antivirus;
- path wallpaper lama yang sudah dihapus;
- kompatibilitas helper pihak ketiga.

Karena itu keberhasilan restore pada perangkat target harus divalidasi dengan event nyata dan pemeriksaan registry sebelum replacement, sesudah replacement, serta sesudah deadline restore.
