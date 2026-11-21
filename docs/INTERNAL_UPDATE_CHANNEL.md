# Kanal pembaruan internal untuk QC

Rilis `v2.4.2` sengaja disimpan sebagai **draft**. Draft hanya terlihat oleh akun dengan akses tulis; `GET /repos/ASQAL90/PrayerAutomation/releases/latest` tetap mengembalikan `v2.4.1`. Ini bukan rilis stable dan belum lulus QC.

## Build uji

Override hanya dibaca bila assembly dibangun dengan symbol `AYOJAMAAH_INTERNAL_UPDATE`. Build stable tanpa symbol tidak memanggil `Environment.GetEnvironmentVariable` untuk kanal ini dan tetap memakai `.../releases/latest`.

```bash
proot-distro login debian -- bash -lc 'export RUSTUP_HOME=/opt/rustup CARGO_HOME=/opt/cargo PATH=/opt/cargo/bin:/opt/dotnet:$PATH; cd /storage/emulated/0/Develop/PrayerAutomation && MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER=1 dotnet build src/PrayerAutomation.UI/PrayerAutomation.UI.csproj -c Release -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:EnableWindowsTargeting=true -p:DefineConstants=AYOJAMAAH_INTERNAL_UPDATE'
```

Sebelum menjalankan build uji, set dua variabel lingkungan:

```bash
export AYOJAMAAH_UPDATE_URL='https://api.github.com/repos/ASQAL90/PrayerAutomation/releases/tags/v2.4.2'
export AYOJAMAAH_UPDATE_TOKEN='token-dengan-akses-tulis-ke-repo'
```

Token diperlukan agar draft terbaca. Jangan menaruh token di source, installer, manifest, atau build stable.

## Perilaku updater

- `AYOJAMAAH_UPDATE_URL` adalah endpoint API JSON rilis, bukan halaman web.
- Request metadata dan aset memakai `Authorization` hanya bila hostnya `api.github.com`.
- Saat token ada, aset diambil dari field `url` API; tanpa token tetap memakai `browser_download_url`.
- Redirect ke CDN diikuti manual dengan `AllowAutoRedirect=false` dan tanpa header `Authorization`.
- Tanpa token, rilis draft tidak terbaca dan aplikasi menganggap tidak ada pembaruan.

## Promosi

Promosi ke stable/latest dilakukan hanya setelah QC Windows lulus dan dengan perintah eksplisit:

```bash
GH_TOKEN="$($HOME/bin/gh-guard token)" gh api repos/ASQAL90/PrayerAutomation/releases/453384124 -X PATCH -F draft=false -F prerelease=false -f make_latest=true
```

Tag dan aset tidak dihapus atau diganti saat status diubah.
