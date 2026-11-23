"""Shared validation for reviewed payload receipts.

Used by the NSIS payload generator and the installer build wrapper.
Stdlib only.
"""
from __future__ import annotations
import hashlib
import json
import re
import sys
from pathlib import Path, PurePosixPath

sys.path.insert(0, str(Path(__file__).resolve().parent))
from update_payload_common import iter_payload_files  # noqa: E402

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
    actual = {p.relative_to(source).as_posix() for p in iter_payload_files(source)}
    if actual != set(payload):
        raise ValueError(f"Payload names differ: missing={set(payload)-actual}, extra={actual-set(payload)}")
    for rel, expected in payload.items():
        if not isinstance(expected, str) or not re.fullmatch(r"[0-9a-f]{64}", expected):
            raise ValueError(f"Invalid SHA256 pin: {rel}")
        parts = PurePosixPath(rel).parts
        if not parts or ".." in parts or PurePosixPath(rel).is_absolute() or any(c in rel for c in '";{}:\\'):
            raise ValueError(f"Unsafe manifest path: {rel}")
        path = source.joinpath(*parts)
        if path.is_symlink() or hashlib.sha256(path.read_bytes()).hexdigest() != expected:
            raise ValueError(f"Payload hash mismatch: {rel}")
    return payload
