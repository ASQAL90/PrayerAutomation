"""Generate fixed [Files] entries from a technically reviewed receipt, not a wildcard.
No product build, app launch, or installation. Stdlib only.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import re
from pathlib import Path, PurePosixPath

UI_NAME = "AyoJamaah.dll"


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


def render(payload: dict[str, str]) -> str:
    lines = ["; Generated from the technically reviewed Ayo Jama'ah! receipt. Normal UI/native acceptance PENDING. Do not substitute/rebuild runtime.",
             "; GetSHA256OfFile is ISPP; mismatches stop compilation before packaging."]
    for rel, digest in sorted(payload.items()):
        win = rel.replace("/", "\\")
        parent = str(PurePosixPath(rel).parent)
        dest = "{app}" if parent == "." else "{app}\\" + parent.replace("/", "\\")
        lines.extend([
            f'#if GetSHA256OfFile(MyAppSourceDir + "\\{win}") != "{digest}"',
            f'  #error "Accepted payload mismatch: {win}"',
            "#endif",
            f'Source: "{{#MyAppSourceDir}}\\{win}"; DestDir: "{dest}"; Flags: ignoreversion; Check: CheckPayloadWrite',
        ])
    return "\n".join(lines) + "\n"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--receipt", type=Path, required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    payload = validated_payload(args.receipt, args.source)
    content = render(payload)
    if args.output.exists() and args.output.read_text(encoding="utf-8") != content:
        raise ValueError("Existing pin file differs; review deliberately instead of overwriting")
    args.output.write_text(content, encoding="utf-8")
    print(json.dumps({"status": "PASS", "files": len(payload), "ui_dll_sha256": payload[UI_NAME],
                      "source": str(args.source), "output": str(args.output)}, indent=2))


if __name__ == "__main__":
    main()
