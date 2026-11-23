## Ringkasan

Jelaskan perubahan dan alasannya.

## Jenis perubahan

- [ ] Perbaikan bug
- [ ] Fitur baru
- [ ] Dokumentasi
- [ ] Perubahan installer/build
- [ ] Perbaikan keamanan

## Cara diuji

Sebutkan perintah dan lingkungan (Windows/Termux, versi .NET/Rust/NSIS).

## Checklist

- [ ] Tidak ada kredensial, token, atau path mesin pribadi.
- [ ] `cargo test --workspace --locked` lulus (bila menyentuh Rust).
- [ ] `python3 installer/build_installer_nsis.py --check-only` lulus (bila menyentuh installer).
- [ ] Uji install/uninstall di Windows (bila menyentuh NSIS).
- [ ] Dokumentasi dan `PRIVACY.md` diperbarui bila perlu.
- [ ] Workflow Actions tetap memakai commit SHA.
