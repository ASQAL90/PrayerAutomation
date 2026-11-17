"""Generate fixed [Files] entries from technically reviewed receipts, not a wildcard.
Supports the historical single-architecture payload and the 2.1.2 dual x86+x64 payload.
No product build, app launch, or installation. Stdlib only.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import re
from pathlib import Path, PurePosixPath

UI_NAME = "AyoJamaah.dll"
PLACEHOLDER_MARKER = "; PLACEHOLDER"
HEADER = [
    "; Generated from the technically reviewed Ayo Jama'ah! receipt. Normal UI/native acceptance PENDING. Do not substitute/rebuild runtime.",
    "; GetSHA256OfFile is ISPP; mismatches stop compilation before packaging.",
]


def validated_payload(receipt: Path, source: Path) -> dict[str, str]:
    data = json.loads(receipt.read_text(encoding="utf-8"))
    payload = data["payload"]
    if not isinstance(payload, dict) or not payload or data["file_count"] != len(payload):
        raise ValueError("Receipt file count does not match the explicit payload")
    if data.get("assembly_name") != "AyoJamaah" or data.get("review_status") != "TECHNICAL_CHECKS_PASSED_UI_ACCEPTANCE_PENDING":
        raise ValueError("Not the technically reviewed Ayo Jama'ah! payload receipt")
    if payload.get(UI_NAME) != data["ui_sha256"] or not re.fullmatch(r"[0-9a-f]{64}", data["ui_sha256"]):
        raise ValueError("Reviewed UI pin does not match the payload")
    actual = {p.relative_to(source).as_posix() for p in source.rglob("*") if p.is_file()}
    if actual != set(payload):
        raise ValueError(f"Payload names differ: missing={set(payload)-actual}, extra={actual-set(payload)}")
    for rel, expected in payload.items():
        if not isinstance(expected, str) or not re.fullmatch(r"[0-9a-f]{64}", expected):
            raise ValueError(f"Invalid SHA256 pin: {rel}")
        parts = PurePosixPath(rel).parts
        if not parts or ".." in parts or PurePosixPath(rel).is_absolute() or any(c in rel for c in '\";{}:\\'):
            raise ValueError(f"Unsafe manifest path: {rel}")
        path = source.joinpath(*parts)
        if path.is_symlink() or hashlib.sha256(path.read_bytes()).hexdigest() != expected:
            raise ValueError(f"Payload hash mismatch: {rel}")
    return payload


def destination(rel: str) -> str:
    parent = str(PurePosixPath(rel).parent)
    return "{app}" if parent == "." else "{app}\\" + parent.replace("/", "\\")


def hash_guard(source_macro: str, rel: str, digest: str, architecture: str | None = None) -> list[str]:
    win = rel.replace("/", "\\")
    label = f" ({architecture})" if architecture else ""
    return [
        f'#if GetSHA256OfFile({source_macro} + "\\{win}") != "{digest}"',
        f'  #error "Accepted payload mismatch{label}: {win}"',
        "#endif",
    ]


def source_entry(source_macro: str, rel: str, check: str) -> str:
    win = rel.replace("/", "\\")
    return f'Source: "{{#{source_macro}}}\\{win}"; DestDir: "{destination(rel)}"; Flags: ignoreversion; Check: {check}'


def render(payload: dict[str, str]) -> str:
    lines = list(HEADER)
    for rel, digest in sorted(payload.items()):
        lines.extend(hash_guard("MyAppSourceDir", rel, digest))
        lines.append(source_entry("MyAppSourceDir", rel, "CheckPayloadWrite"))
    return "\n".join(lines) + "\n"


def render_dual(payload_x86: dict[str, str], payload_x64: dict[str, str]) -> str:
    lines = list(HEADER)
    lines.append("; Shared files are hash-checked against both source trees and installed once.")
    for rel in sorted(set(payload_x86) | set(payload_x64)):
        digest_x86 = payload_x86.get(rel)
        digest_x64 = payload_x64.get(rel)
        if digest_x86 is not None and digest_x86 == digest_x64:
            lines.extend(hash_guard("MyAppSourceDirX86", rel, digest_x86, "x86"))
            lines.extend(hash_guard("MyAppSourceDirX64", rel, digest_x64, "x64"))
            lines.append(source_entry("MyAppSourceDirX64", rel, "CheckPayloadWrite"))
            continue
        if digest_x64 is not None:
            lines.extend(hash_guard("MyAppSourceDirX64", rel, digest_x64, "x64"))
            lines.append(source_entry("MyAppSourceDirX64", rel, "CheckPayloadWrite and IsX64Arch"))
        if digest_x86 is not None:
            lines.extend(hash_guard("MyAppSourceDirX86", rel, digest_x86, "x86"))
            lines.append(source_entry("MyAppSourceDirX86", rel, "CheckPayloadWrite and IsX86Arch"))
    return "\n".join(lines) + "\n"


def write_output(output: Path, content: str, force: bool = False) -> None:
    if output.exists():
        existing = output.read_text(encoding="utf-8")
        if existing != content and PLACEHOLDER_MARKER not in existing and not force:
            raise ValueError("Existing pin file differs; review deliberately instead of overwriting (use --force after reviewing the new payload)")
    output.write_text(content, encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--receipt", type=Path)
    parser.add_argument("--source", type=Path)
    parser.add_argument("--receipt-x86", type=Path)
    parser.add_argument("--source-x86", type=Path)
    parser.add_argument("--receipt-x64", type=Path)
    parser.add_argument("--source-x64", type=Path)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--force", action="store_true", help="Replace an existing pin file after reviewing the new payload")
    args = parser.parse_args()

    single = args.receipt is not None or args.source is not None
    dual = any(value is not None for value in (args.receipt_x86, args.source_x86, args.receipt_x64, args.source_x64))
    if single and dual:
        raise ValueError("Choose either --receipt/--source or the x86/x64 pair, not both")
    if dual:
        missing = [name for name, value in {
            "--receipt-x86": args.receipt_x86,
            "--source-x86": args.source_x86,
            "--receipt-x64": args.receipt_x64,
            "--source-x64": args.source_x64,
        }.items() if value is None]
        if missing:
            raise ValueError("Dual payload needs " + ", ".join(missing))
        payload_x86 = validated_payload(args.receipt_x86, args.source_x86)
        payload_x64 = validated_payload(args.receipt_x64, args.source_x64)
        content = render_dual(payload_x86, payload_x64)
        file_count = len(set(payload_x86) | set(payload_x64))
        shared = sum(1 for name in payload_x86 if payload_x86.get(name) == payload_x64.get(name))
        result = {"status": "PASS", "mode": "dual", "files": file_count, "shared_files": shared,
                  "ui_dll_sha256_x86": payload_x86[UI_NAME], "ui_dll_sha256_x64": payload_x64[UI_NAME],
                  "output": str(args.output)}
    elif single:
        if args.receipt is None or args.source is None:
            raise ValueError("Single payload needs both --receipt and --source")
        payload = validated_payload(args.receipt, args.source)
        content = render(payload)
        result = {"status": "PASS", "mode": "single", "files": len(payload),
                  "ui_dll_sha256": payload[UI_NAME], "source": str(args.source), "output": str(args.output)}
    else:
        raise ValueError("Nothing to generate; pass a receipt/source pair")

    write_output(args.output, content, args.force)
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
