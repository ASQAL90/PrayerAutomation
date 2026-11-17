"""Build a payload receipt from a published portable folder.
The receipt is an explicit file map with SHA-256 pins; no wildcard, no build,
no launch, no installation. Consumed by generate_nsis_payload.py and build_installer_nsis.py (via payload_receipt.py). Stdlib only.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import sys
from pathlib import Path, PurePosixPath

UI_NAME = "AyoJamaah.dll"
EXE_NAME = "AyoJamaah.exe"
DEFAULT_REVIEW_STATUS = "TECHNICAL_CHECKS_PASSED_UI_ACCEPTANCE_PENDING"
PE_MACHINE = {0x8664: "x64", 0x14C: "x86"}


def pe_machine(path: Path) -> str:
    data = path.read_bytes()[:4096]
    if len(data) < 0x40 or data[:2] != b"MZ":
        raise ValueError(f"Not a PE file: {path.name}")
    offset = int.from_bytes(data[0x3C:0x40], "little")
    if offset + 6 > len(data) or data[offset:offset + 4] != b"PE\0\0":
        raise ValueError(f"Missing PE header: {path.name}")
    machine = int.from_bytes(data[offset + 4:offset + 6], "little")
    if machine not in PE_MACHINE:
        raise ValueError(f"Unexpected PE machine 0x{machine:04x}: {path.name}")
    return PE_MACHINE[machine]


def collect_payload(source: Path) -> dict[str, str]:
    if not source.is_dir():
        raise ValueError(f"Payload source is not a directory: {source}")
    payload: dict[str, str] = {}
    for path in sorted(source.rglob("*")):
        if path.is_symlink():
            raise ValueError(f"Symlink in payload: {path.relative_to(source).as_posix()}")
        if not path.is_file():
            continue
        rel = path.relative_to(source).as_posix()
        parts = PurePosixPath(rel).parts
        if not parts or ".." in parts or PurePosixPath(rel).is_absolute() or any(c in rel for c in '";{}:\\'):
            raise ValueError(f"Unsafe manifest path: {rel}")
        payload[rel] = hashlib.sha256(path.read_bytes()).hexdigest()
    if not payload:
        raise ValueError("Payload source is empty")
    for required in (UI_NAME, EXE_NAME):
        if required not in payload:
            raise ValueError(f"Required file missing from payload: {required}")
    return payload


def write_receipt(output: Path, receipt: dict, force: bool) -> None:
    content = json.dumps(receipt, indent=2, ensure_ascii=False) + "\n"
    if output.exists() and not force:
        if output.read_text(encoding="utf-8") != content:
            raise ValueError(f"{output} already exists with different hashes; rerun with --force after reviewing")
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(content, encoding="utf-8", newline="\n")


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate a payload receipt from a portable folder")
    parser.add_argument("--source", type=Path, required=True, help="Published portable folder")
    parser.add_argument("--output", type=Path, required=True, help="Receipt JSON to write")
    parser.add_argument("--arch", choices=sorted(PE_MACHINE.values()), help="Expected architecture of AyoJamaah.dll")
    parser.add_argument("--review-status", default=DEFAULT_REVIEW_STATUS)
    parser.add_argument("--force", action="store_true", help="Overwrite a receipt with different hashes")
    args = parser.parse_args()

    payload = collect_payload(args.source)
    detected = pe_machine(args.source / UI_NAME)
    if args.arch is not None and args.arch != detected:
        raise ValueError(f"--arch {args.arch} does not match {UI_NAME} ({detected})")

    receipt = {
        "assembly_name": "AyoJamaah",
        "review_status": args.review_status,
        "architecture": detected,
        "file_count": len(payload),
        "ui_sha256": payload[UI_NAME],
        "payload": payload,
    }
    write_receipt(args.output, receipt, args.force)
    print(json.dumps({
        "status": "PASS",
        "architecture": detected,
        "files": len(payload),
        "ui_dll_sha256": payload[UI_NAME],
        "output": str(args.output),
    }, indent=2))


if __name__ == "__main__":
    try:
        main()
    except ValueError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
