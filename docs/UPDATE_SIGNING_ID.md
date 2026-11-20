# Penandatanganan pembaruan 2.4.0

Pembaruan delta memakai manifest JSON kecil yang ditandatangani dengan RSA-SHA256. Aplikasi menyimpan **public key** di `src/PrayerAutomation.UI/UpdateManifest.cs`; private key tidak pernah ikut dikirim.

## Rahasia CI

Repo GitHub perlu satu repository secret:

- **Nama:** `AYOJAMAAH_UPDATE_SIGNING_KEY_B64`
- **Isi:** private key PKCS#8 dalam format DER yang di-base64 satu baris.

Pembuatan pasangan kunci (dari komputer pengembang):

```sh
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out update-key.pem
openssl pkey -in update-key.pem -pubout -out update-public.pem

openssl pkey -in update-key.pem -outform DER | base64 -w0       # isi secret CI
openssl pkey -pubin -in update-public.pem -outform DER | base64 -w0
```

Nilai public key DER base64 di atas harus sama dengan konstanta `PublicKeyBase64` di `UpdateManifest.cs`. Bila kunci diganti, aplikasi lama akan menolak manifest baru dan otomatis memakai installer lengkap; kunci baru baru berlaku setelah versi aplikasi yang membawanya terpasang.

## Aset rilis

Tiap rilis dengan dukungan delta mengunggah, per arsitektur:

- `AyoJamaah-update-<versi>-<arch>.json` — manifest berisi hash semua berkas, daftar `deleted`, dan nama aset delta/fallback.
- `AyoJamaah-update-<versi>-<arch>.sig` — tanda tangan base64 atas **byte persis** file manifest.
- `AyoJamaah-delta-<versi>-<arch>.zip` — hanya berkas yang berubah sejak baseline.

Baseline 2.3.0 disimpan sebagai `installer/update/baseline-2.3.0-<arch>.json`, sehingga CI tidak perlu mengunduh zip lama. Jika secret tidak diisi atau manifest gagal diverifikasi, aplikasi jatuh ke zip penuh (portable) atau installer NSIS (instalasi). Rilis v2.4.0 sudah mengunggah manifest, `.sig`, dan delta untuk kedua arsitektur memakai baseline 2.3.0 serta kunci yang sama dengan `PublicKeyBase64` di `UpdateManifest.cs`.

Workflow `Release` mempublikasikan rilis bila dijalankan dari tag `v*`; v2.4.0 dipublish dari tag `v2.4.0`, bukan dari cabang `master`.
