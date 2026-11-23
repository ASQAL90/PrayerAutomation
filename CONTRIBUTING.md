# Berkontribusi

Terima kasih telah tertarik membantu Ayo Jama'ah!.

## Sebelum mulai

- Untuk **masalah keamanan**, jangan buka issue publik. Ikuti `SECURITY.md`.
- Untuk perubahan besar, buka issue diskusi terlebih dahulu agar arahnya jelas.
- Jangan pernah menambahkan kunci privat, token, atau data pribadi ke repositori.

## Prasyarat

| Alat | Versi | Dipakai untuk |
| --- | --- | --- |
| .NET SDK | 10.0 | aplikasi WPF dan smoke test |
| Rust (stable) | terbaru | `src/rust-core` |
| NSIS | 3.x | installer Windows |
| Python | 3.9+ (stdlib saja) | skrip installer dan receipt |

Build aplikasi WPF memerlukan Windows. Core Rust dan skrip installer dapat
dikerjakan di Linux/Termux.

## Build dan test

```bash
# Core Rust
cargo test --workspace --locked
cargo fmt --all --check
cargo clippy --all-targets -- -D warnings

# Smoke test parser dan data offline (Windows, .NET 10)
dotnet run --project tests/OfflineGeocoderSmoke

# Validasi payload + generate include NSIS tanpa kompilasi
python3 installer/build_installer_nsis.py --check-only
```

Selalu jalankan `--check-only` sebelum mengubah daftar payload. Jangan pernah
mengedit `installer/nsis/AyoJamaahPayload-*.nsh` secara manual; berkas itu
dihasilkan oleh `generate_nsis_payload.py`.

## Aturan commit

- Satu commit = satu perubahan logis.
- Tulis pesan commit dalam bentuk imperatif, misalnya
  `Perbaiki validasi nama aset pembaruan`.
- Jangan commit artefak build (`bin/`, `obj/`, `out/`, `dist/`, `target/`),
  log, kunci, atau salinan data pribadi.
- Jangan menulis ulang history branch yang sudah dipublikasikan.

## Checklist pull request

- [ ] Perubahan diuji (sebutkan perintah/lingkungannya).
- [ ] Tidak ada kredensial, token, atau path mesin pribadi.
- [ ] Dokumentasi diperbarui bila perilaku pengguna berubah.
- [ ] `SECURITY.md`/`PRIVACY.md` diperbarui bila ada endpoint atau data baru.
- [ ] Tidak mengubah perilaku installer tanpa menjalankan uji install/uninstall.
- [ ] Workflow GitHub Actions memakai commit SHA, bukan tag/branch.

## Lisensi kontribusi

Dengan mengirim pull request, Anda menyatakan bahwa kontribusi tersebut adalah
karya Anda sendiri dan boleh didistribusikan dengan lisensi yang sama seperti
repositori ini.
