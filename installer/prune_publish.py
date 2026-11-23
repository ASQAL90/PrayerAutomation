"""Prunes unused framework files from a self-contained publish folder.

The app is published self-contained on purpose: the user must not install any
runtime. A self-contained publish still copies designer assemblies, debugger
support, satellite translations, and framework pieces the app never exercises.
This script removes an explicit deny-list after `dotnet publish` and before the
receipt/installer/zip steps, then fails loudly if a required file disappeared.

Run it on the publish output, never on a working build tree:

    python3 installer/prune_publish.py --source out/portable-x64 --arch x64
"""
from __future__ import annotations

import argparse
import fnmatch
import json
import shutil
import sys
from pathlib import Path

# Assemblies the app never loads at runtime. Keep this list explicit: when a
# .NET update renames or removes an entry, the report shows it instead of the
# installer silently growing again.
DENY_FILES = {
    # WinForms was replaced by a native Shell_NotifyIcon tray.
    "System.Windows.Forms.dll",
    "System.Windows.Forms.Primitives.dll",
    "System.Windows.Forms.Design.dll",
    "WindowsFormsIntegration.dll",
    "System.Drawing.Design.dll",
    # Designer and debugging support.
    "Microsoft.DiaSymReader.Native.amd64.dll",
    "Microsoft.DiaSymReader.Native.x86.dll",
    "mscordbi.dll",
    # WPF features the app does not use.
    "ReachFramework.dll",
    "System.Printing.dll",
    "PresentationFramework.Fluent.dll",
    "PresentationUI.dll",
    # Framework subsystems with no matching feature.
    "System.Data.Common.dll",
    "System.Private.DataContractSerialization.dll",
    "System.DirectoryServices.dll",
    "System.DirectoryServices.Protocols.dll",
    "Microsoft.VisualBasic.dll",
    "Microsoft.VisualBasic.Core.dll",
    "Microsoft.VisualBasic.Forms.dll",
    "Microsoft.CSharp.dll",
}

DENY_GLOBS = {
    "mscordaccore*.dll",
    "*.pdb",
}

REQUIRED_FILES = {
    "AyoJamaah.exe",
    "AyoJamaah.dll",
    "AyoJamaah.deps.json",
    "AyoJamaah.runtimeconfig.json",
    "prayer_core.dll",
    "PresentationFramework.dll",
    "PresentationCore.dll",
    "WindowsBase.dll",
    "System.Xaml.dll",
    "System.Private.CoreLib.dll",
    "System.Private.Xml.dll",
    "System.Security.Cryptography.dll",
    "System.Net.Http.dll",
    "System.Text.Json.dll",
    "coreclr.dll",
    "clrjit.dll",
    "wpfgfx_cor3.dll",
    "D3DCompiler_47_cor3.dll",
    "GeoTimeZone.dll",
    "TimeZoneConverter.dll",
    "System.Windows.Controls.Ribbon.dll",
    "Microsoft.Windows.SDK.NET.dll",
    "WinRT.Runtime.dll",
    "AyoJamaah.ico",
    "geonames/cities500.tsv.gz",
    "geonames/indonesia.tsv.gz",
    "geonames/indonesia.boundary.bin",
}


def ensure_publish_folder(source: Path, repo: Path) -> Path:
    """Refuses to prune anything that is not a self-contained publish output.

    This script deletes files. A wrong --source (repository root, home folder,
    drive root, or a folder that happens to contain a checkout) must fail before
    anything is touched.
    """
    resolved = source.resolve()
    if not resolved.is_dir():
        raise SystemExit(f"Folder publish tidak ditemukan: {resolved}")

    root = Path(resolved.anchor)
    home = Path.home().resolve()
    if resolved == root or resolved == home:
        raise SystemExit(f"Refusing to prune a filesystem or home root: {resolved}")
    if resolved == repo or resolved == repo.parent or resolved in repo.parents:
        raise SystemExit(f"Refusing to prune a repository folder: {resolved}")
    if (resolved / ".git").exists():
        raise SystemExit(f"Refusing to prune a folder that contains .git: {resolved}")
    if not (resolved / "AyoJamaah.exe").is_file():
        raise SystemExit(
            f"Refusing to prune a folder without AyoJamaah.exe (bukan output publish): {resolved}"
        )
    return resolved


def removed_files(source: Path) -> list[Path]:
    """Returns files matched by the deny-list, without touching the disk."""
    matched: list[Path] = []
    for path in source.rglob("*"):
        if not path.is_file():
            continue
        name = path.name
        relative = path.relative_to(source)
        if len(relative.parts) > 1 and relative.parts[0] not in ("geonames",):
            # A subfolder with a non-English name is a satellite resource.
            if not relative.parts[0].startswith("zh-") and _looks_like_locale(relative.parts[0]):
                matched.append(path)
                continue
        if name in DENY_FILES or any(fnmatch.fnmatch(name, pattern) for pattern in DENY_GLOBS):
            matched.append(path)
    return matched


def _looks_like_locale(name: str) -> bool:
    return len(name) in (2, 5) and name[:2].isalpha() and name[:2].isascii()


def scrub_deps_json(source: Path) -> list[str]:
    """Removes missing-file entries from deps.json so the host doesn't probe them.

    Publish can flatten package assets to the app root while deps.json keeps
    the package-relative path (e.g. lib/net9.0/Foo.dll). In that case remap
    the key to the flattened basename instead of dropping the entry.
    """
    scrubbed: list[str] = []
    for deps_path in source.glob("*.deps.json"):
        data = json.loads(deps_path.read_text(encoding="utf-8"))
        changed = False
        for target in data.get("targets", {}).values():
            for lib_info in target.values():
                for section in ("runtime", "native", "resources", "runtimeTargets"):
                    entries = lib_info.get(section)
                    if not isinstance(entries, dict):
                        continue
                    new_entries = {}
                    for k, v in entries.items():
                        if (source / k).is_file():
                            new_entries[k] = v
                        else:
                            flat = Path(k).name
                            if flat != k and (source / flat).is_file():
                                new_entries[flat] = v
                                changed = True
                                scrubbed.append(f"{deps_path.name}:{section}:{k}->{flat}")
                            else:
                                changed = True
                                scrubbed.append(f"{deps_path.name}:{section}:{k}")
                    if len(new_entries) != len(entries) or new_entries != entries:
                        lib_info[section] = new_entries
        if changed:
            deps_path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n",
                                 encoding="utf-8")
    return scrubbed


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(description="Prune unused files from a portable publish")
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--arch", choices=("x64", "x86"), required=True)
    parser.add_argument("--report", type=Path, help="Optional JSON report path")
    parser.add_argument("--check-only", action="store_true",
                        help="List what would be removed without deleting")
    args = parser.parse_args()

    source = ensure_publish_folder(args.source, repo)

    before = sum(p.stat().st_size for p in source.rglob("*") if p.is_file())
    candidates = sorted(removed_files(source))

    if not args.check_only:
        for path in candidates:
            path.unlink(missing_ok=True)
        for directory in sorted({p.parent for p in candidates}, reverse=True):
            if directory != source and not any(directory.iterdir()):
                try:
                    directory.rmdir()
                except OSError:
                    pass

    scrubbed = [] if args.check_only else scrub_deps_json(source)

    missing = sorted(rel for rel in REQUIRED_FILES if not (source / rel).is_file())
    if missing:
        raise SystemExit("File wajib hilang setelah prune: " + ", ".join(missing))
    if (source / "System.Windows.Forms.dll").exists():
        raise SystemExit("System.Windows.Forms.dll masih ada; UseWindowsForms belum dimatikan?")

    after = sum(p.stat().st_size for p in source.rglob("*") if p.is_file())
    if args.check_only:
        after -= sum(path.stat().st_size for path in candidates)
    report = {
        "architecture": args.arch,
        "status": "CHECK" if args.check_only else "PASS",
        "source": str(source),
        "files_removed": len(candidates),
        "bytes_before": before,
        "bytes_after": after,
        "mib_before": round(before / 2**20, 2),
        "mib_after": round(after / 2**20, 2),
        "mib_saved": round((before - after) / 2**20, 2),
        "removed": [str(p.relative_to(source)) for p in candidates],
        "deps_scrubbed": scrubbed,
    }
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: report[k] for k in ("architecture", "status", "files_removed",
                                             "mib_before", "mib_after", "mib_saved")}))
    return 0


if __name__ == "__main__":
    sys.exit(main())
