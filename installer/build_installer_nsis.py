"""Build the NSIS installer from the published portable folders.

This is the supported entry point on Termux and on Windows:

    python3 installer/build_installer_nsis.py

It re-validates both receipts against the portable folders, regenerates the
explicit payload include, then runs makensis (through proot Debian on Termux).
No product rebuild, app launch or installation happens here. Stdlib only.
"""
from __future__ import annotations

import argparse
import json
import os
import shlex
import shutil
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from payload_receipt import validated_payload  # noqa: E402
from generate_nsis_payload import render, render_files_list, render_single, write_output  # noqa: E402

def app_version(nsis_dir: Path) -> str:
    """Reads APP_VERSION from the NSIS script so the output name never drifts."""
    for line in (nsis_dir / "AyoJamaah.nsi").read_text(encoding="utf-8").splitlines():
        stripped = line.strip()
        if stripped.startswith("!define APP_VERSION "):
            return stripped.split('"')[1]
    raise ValueError("APP_VERSION tidak ditemukan di AyoJamaah.nsi")


def receipt_architecture(receipt: Path, expected: str) -> None:
    data = json.loads(receipt.read_text(encoding="utf-8"))
    actual = data.get("architecture")
    if actual != expected:
        raise ValueError(f"{receipt}: architecture {actual!r}, expected {expected!r}")


def nsis_command(args: argparse.Namespace, script: Path, output: Path) -> list[str]:
    defines = [
        f"-DPAYLOAD_X64={args.portable_x64.resolve()}",
        f"-DPAYLOAD_X86={args.portable_x86.resolve()}",
        f"-DOUTFILE={output.resolve()}",
    ]
    if args.variant == "x64":
        defines.append("-DSINGLE_ARCH_X64")
    elif args.variant == "x86":
        defines.append("-DSINGLE_ARCH_X86")
    if args.makensis:
        return [args.makensis, "-V3", *defines, str(script.resolve())]

    proot = shutil.which("proot-distro")
    if proot:
        nsis_dir = script.parent.resolve()
        out_dir = output.parent.resolve()
        out_dir.mkdir(parents=True, exist_ok=True)
        binds = [
            f"{nsis_dir}:/mnt/nsis",
            f"{args.portable_x64.resolve()}:/mnt/payload-x64",
            f"{args.portable_x86.resolve()}:/mnt/payload-x86",
            f"{out_dir}:/mnt/out",
        ]
        command = (
            "cd /mnt/nsis && makensis -V3 "
            f"-DPAYLOAD_X64=/mnt/payload-x64 "
            f"-DPAYLOAD_X86=/mnt/payload-x86 "
            f"-DOUTFILE=/mnt/out/{shlex.quote(output.name)} "
            + ("-DSINGLE_ARCH_X64 " if args.variant == "x64" else
               "-DSINGLE_ARCH_X86 " if args.variant == "x86" else "")
            + f"{shlex.quote(script.name)}"
        )
        result = [proot, "login", "debian"]
        for bind in binds:
            result.extend(["--bind", bind])
        result.extend(["--", "bash", "-lc", command])
        return result

    makensis = shutil.which("makensis")
    if makensis:
        return [makensis, "-V3", *defines, str(script.resolve())]
    raise ValueError("makensis tidak ditemukan; pasang nsis di proot Debian atau "
                     "berikan --makensis <path>")


def main() -> None:
    repo = Path(__file__).resolve().parent.parent
    build_root = Path(os.environ.get("AYOJAMAAH_OUT", repo.parent / "AyoJamaah-build"))
    nsis_dir = repo / "installer" / "nsis"

    parser = argparse.ArgumentParser(description="Build the Ayo Jama'ah! NSIS installer")
    parser.add_argument("--portable-x64", type=Path, default=build_root / "portable-x64")
    parser.add_argument("--portable-x86", type=Path, default=build_root / "portable-x86")
    parser.add_argument("--receipt-x64", type=Path, default=build_root / "receipt-x64.json")
    parser.add_argument("--receipt-x86", type=Path, default=build_root / "receipt-x86.json")
    parser.add_argument("--variant", choices=("universal", "x64", "x86"), default="universal",
                        help="universal ships both payloads; x64/x86 ship one architecture")
    parser.add_argument("--output", type=Path,
                        help="Output path (default depends on --variant)")
    parser.add_argument("--makensis", help="Path to makensis (skips the Termux proot wrapper)")
    parser.add_argument("--check-only", action="store_true", help="Validate and regenerate the include, but do not compile")
    args = parser.parse_args()

    version = app_version(nsis_dir)
    variant_suffix = {"universal": "", "x64": "-x64", "x86": "-x86"}[args.variant]
    if args.output is None:
        args.output = build_root / "installer" / f"AyoJamaah.Setup-{version}{variant_suffix}.exe"

    receipt_architecture(args.receipt_x64, "x64")
    receipt_architecture(args.receipt_x86, "x86")
    payload_x64 = validated_payload(args.receipt_x64, args.portable_x64)
    payload_x86 = validated_payload(args.receipt_x86, args.portable_x86)

    if args.variant == "universal":
        include = nsis_dir / "AyoJamaahPayload-dual.nsh"
        write_output(include, render(payload_x64, payload_x86), force=True)
    else:
        payload = payload_x64 if args.variant == "x64" else payload_x86
        include = nsis_dir / f"AyoJamaahPayload-{args.variant}.nsh"
        write_output(include, render_single(payload, args.variant), force=True)
    for arch, payload in (("x64", payload_x64), ("x86", payload_x86)):
        write_output(nsis_dir / f"AyoJamaahPayloadFiles-{arch}.txt",
                     render_files_list(list(payload)), force=True)

    result = {
        "status": "PASS",
        "variant": args.variant,
        "files_x64": len(payload_x64),
        "files_x86": len(payload_x86),
        "files_unique": len(set(payload_x64) | set(payload_x86)),
        "shared_files": sum(1 for rel in payload_x64 if payload_x86.get(rel) == payload_x64[rel]),
        "include": str(include),
        "output": str(args.output),
    }

    if not args.check_only:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        script = nsis_dir / "AyoJamaah.nsi"
        command = nsis_command(args, script, args.output)
        print(">> " + " ".join(shlex.quote(part) for part in command), file=sys.stderr)
        subprocess.run(command, check=True)
        if not args.output.is_file():
            raise ValueError(f"makensis selesai tetapi output tidak ditemukan: {args.output}")
        result["installer_bytes"] = args.output.stat().st_size

    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    try:
        main()
    except (ValueError, subprocess.CalledProcessError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
