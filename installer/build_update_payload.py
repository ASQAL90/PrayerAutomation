"""Builds the signed delta zip and manifest for one architecture.

    python3 installer/build_update_payload.py --version 2.3.1 --arch x64 \
        --source out/portable-x64 --baseline installer/update/baseline-2.3.0-x64.json \
        --portable-zip AyoJamaah-win-x64.zip --installer out/installer/AyoJamaah.Setup-2.3.1.exe \
        --output-dir out/update --signing-key key.pem

The baseline is a committed hash list, never an old zip. Only files whose hash
differs from the baseline are copied from the new build, so no previous release
artifact is needed. Stdlib only (openssl is used for RSA signing).
"""
from __future__ import annotations

import argparse
import base64
import json
import os
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from update_payload_common import canonical_json, relative_files, sha256_file  # noqa: E402


def resolve_signing_key(explicit: Path | None) -> Path | None:
    if explicit is not None:
        if not explicit.is_file():
            raise ValueError(f"Kunci penandatangan tidak ditemukan: {explicit}")
        return explicit
    encoded = os.environ.get("AYOJAMAAH_UPDATE_SIGNING_KEY_B64", "").strip()
    if not encoded:
        return None
    temp = Path(os.environ.get("RUNNER_TEMP", "."))
    der = temp / "ayojamaah-update-key.der"
    der.write_bytes(base64.b64decode(encoded, validate=True))
    key = temp / "ayojamaah-update-key.pem"
    subprocess.run(["openssl", "pkey", "-inform", "DER", "-in", str(der),
                    "-out", str(key)], check=True)
    der.unlink()
    return key


def sign_manifest(manifest_path: Path, key: Path) -> Path:
    signature = manifest_path.with_suffix(".sig.bin")
    subprocess.run([
        "openssl", "dgst", "-sha256", "-sign", str(key),
        "-out", str(signature), str(manifest_path),
    ], check=True)
    output = manifest_path.with_suffix(".sig")
    output.write_text(base64.b64encode(signature.read_bytes()).decode("ascii"), encoding="ascii")
    signature.unlink()
    return output


def main() -> None:
    parser = argparse.ArgumentParser(description="Build the signed update delta payload")
    parser.add_argument("--version", required=True)
    parser.add_argument("--tag", default=None)
    parser.add_argument("--arch", choices=("x64", "x86"), required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--portable-zip", type=Path, required=True)
    parser.add_argument("--installer", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--signing-key", type=Path, default=None)
    args = parser.parse_args()

    tag = args.tag or f"v{args.version}"
    baseline = json.loads(args.baseline.read_text(encoding="utf-8"))
    if baseline.get("arch") != args.arch:
        raise ValueError("Arsitektur baseline tidak cocok")
    base_files: dict[str, str] = baseline["files"]
    new_files = relative_files(args.source)

    changed = {rel: digest for rel, digest in new_files.items() if base_files.get(rel) != digest}
    deleted = sorted(rel for rel in base_files if rel not in new_files)
    sizes = {rel: (args.source / Path(*rel.split("/"))).stat().st_size for rel in new_files}

    output_dir = args.output_dir
    output_dir.mkdir(parents=True, exist_ok=True)
    staging = output_dir / f"delta-staging-{args.arch}"
    if staging.exists():
        shutil.rmtree(staging)
    staging.mkdir(parents=True)
    for rel in sorted(changed):
        target = staging.joinpath(*rel.split("/"))
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(args.source.joinpath(*rel.split("/")), target)

    delta_name = f"AyoJamaah-delta-{args.version}-{args.arch}.zip"
    delta_path = output_dir / delta_name
    with zipfile.ZipFile(delta_path, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for rel in sorted(changed):
            archive.write(staging.joinpath(*rel.split("/")), rel)
    shutil.rmtree(staging)

    portable_name = args.portable_zip.name
    installer_name = args.installer.name
    manifest = {
        "schema": 1,
        "version": args.version,
        "tag": tag,
        "baseline": baseline["version"],
        "arch": args.arch,
        "delta_asset": delta_name,
        "delta_sha256": sha256_file(delta_path),
        "delta_size": delta_path.stat().st_size,
        "fallback_asset": portable_name,
        "fallback_sha256": sha256_file(args.portable_zip),
        "fallback_size": args.portable_zip.stat().st_size,
        "installer_asset": installer_name,
        "installer_sha256": sha256_file(args.installer),
        "installer_size": args.installer.stat().st_size,
        "files": [{"p": rel, "s": sizes[rel], "h": digest} for rel, digest in sorted(new_files.items())],
        "deleted": deleted,
    }
    manifest_name = f"AyoJamaah-update-{args.version}-{args.arch}.json"
    manifest_path = output_dir / manifest_name
    manifest_bytes = canonical_json(manifest)
    manifest_path.write_bytes(manifest_bytes)

    key = resolve_signing_key(args.signing_key)
    signature_name = ""
    if key is not None:
        signature_name = f"AyoJamaah-update-{args.version}-{args.arch}.sig"
        sign_manifest(manifest_path, key)

    print(json.dumps({
        "status": "PASS",
        "version": args.version,
        "arch": args.arch,
        "baseline": baseline["version"],
        "files_total": len(new_files),
        "files_changed": len(changed),
        "files_deleted": len(deleted),
        "delta_bytes": delta_path.stat().st_size,
        "fallback_bytes": args.portable_zip.stat().st_size,
        "signed": bool(signature_name),
        "manifest": str(manifest_path),
        "delta": str(delta_path),
    }, indent=2))


if __name__ == "__main__":
    try:
        main()
    except (ValueError, subprocess.CalledProcessError, zipfile.BadZipFile) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
