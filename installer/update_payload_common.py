"""Shared helpers for update baseline and delta packaging. Stdlib only."""
from __future__ import annotations

import hashlib
import os
import re
from pathlib import Path

SHA256_PATTERN = re.compile(r"[0-9a-f]{64}")


def iter_payload_files(source: Path) -> list[Path]:
    """Lists payload files without following symlinks.

    Nothing outside the publish folder may be hashed, zipped or installed, so a
    symlink (to a file or a directory) is rejected instead of being copied.
    """
    if source.is_symlink():
        raise ValueError(f"Folder sumber adalah symlink: {source}")
    if not source.is_dir():
        raise ValueError(f"Folder sumber tidak ditemukan: {source}")

    files: list[Path] = []
    for root, directories, names in os.walk(source, followlinks=False):
        root_path = Path(root)
        for name in sorted(directories):
            candidate = root_path / name
            if candidate.is_symlink():
                raise ValueError(
                    f"Symlink di payload: {candidate.relative_to(source).as_posix()}"
                )
        for name in sorted(names):
            candidate = root_path / name
            if candidate.is_symlink():
                raise ValueError(
                    f"Symlink di payload: {candidate.relative_to(source).as_posix()}"
                )
            if candidate.is_file():
                files.append(candidate)
    return files


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
    result: dict[str, str] = {}
    for path in iter_payload_files(source):
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
