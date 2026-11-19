"""Records the reviewed 2.3.0 portable payload as the delta baseline.

    python3 installer/make_update_baseline.py --source <portable-2.3.0> \
        --arch x64 --output installer/update/baseline-2.3.0-x64.json

The output is a small committed JSON file. It replaces the need to keep an old
zip: CI compares the new build against these hashes and copies only changed
files from the new build. Stdlib only.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from update_payload_common import canonical_json, relative_files  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Create an update delta baseline")
    parser.add_argument("--source", type=Path, required=True, help="Published portable folder")
    parser.add_argument("--version", default="2.3.0", help="Version the source represents")
    parser.add_argument("--arch", choices=("x64", "x86"), required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    files = relative_files(args.source)
    payload = {
        "schema": 1,
        "version": args.version,
        "arch": args.arch,
        "files": files,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes(canonical_json(payload))
    print(json.dumps({
        "status": "PASS",
        "version": args.version,
        "arch": args.arch,
        "files": len(files),
        "output": str(args.output),
    }, indent=2))


if __name__ == "__main__":
    try:
        main()
    except ValueError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
