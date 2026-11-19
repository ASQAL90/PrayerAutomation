"""Shared helpers for update baseline and delta packaging. Stdlib only."""
from __future__ import annotations

import hashlib
import re
from pathlib import Path

SHA256_PATTERN = re.compile(r"[0-9a-f]{64}")


def is_safe_relative(rel: str) -> bool:
    """Reject absolute paths, traversal and characters that break NTFS/NSIS paths."""
    if not rel or rel.startswith("/") or "\\" in rel or ":" in rel:
        return False
    parts = rel.split("/")
    if any(part in ("", ".", "..") for part in parts):
        return False
    return not any(char in rel for char in '";{}<>|?*')


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def relative_files(source: Path) -> dict[str, str]:
    """Maps every published file to its SHA-256 using explicit, safe POSIX paths."""
    if not source.is_dir():
        raise ValueError(f"Folder sumber tidak ditemukan: {source}")
    result: dict[str, str] = {}
    for path in sorted(source.rglob("*")):
        if not path.is_file() or path.is_symlink():
            continue
        rel = path.relative_to(source).as_posix()
        if not is_safe_relative(rel):
            raise ValueError(f"Path payload tidak aman: {rel}")
        result[rel] = sha256_file(path)
    if not result:
        raise ValueError(f"Folder sumber kosong: {source}")
    return result


def canonical_json(value: object) -> bytes:
    """Deterministic UTF-8 JSON used identically by the generator and the signer."""
    import json
    return json.dumps(value, sort_keys=True, separators=(",", ":"), ensure_ascii=False).encode("utf-8")
