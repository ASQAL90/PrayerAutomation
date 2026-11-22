#!/usr/bin/env python3
"""Build the v2 offline location datasets for Ayo Jama'ah 2.5.0.

Hybrid sources:
  * GeoNames (cities500.txt, ID.txt, admin2Codes.txt, alternateNamesV2.txt)
    supplies coordinates, DEM, feature codes, population, and aliases.
  * Kemendagri (cahyadsn/wilayah: wilayah.sql, wilayah_level_1_2.sql)
    supplies the official Indonesian hierarchy and regency names.

Output schema (UTF-8, tab separated, gzip):

cities500.tsv.gz -- 13 columns
  name, admin1, country, latitude, longitude, country_code,
  dem, sampled_elevation, feature_code, population, aliases,
  elevation_source, elevation_quality

indonesia.tsv.gz -- 15 columns, backward-compatible prefix with the old 6 columns
  name, regency, province, latitude, longitude, dem, sampled_elevation,
  feature_code, population, aliases, district, region_code, official_match,
  elevation_source, elevation_quality

Usage:
  python3 build_location_datasets.py --sources DIR --work DIR --output DIR
"""

from __future__ import annotations

import argparse
import gzip
import json
import os
import re
import sys
import unicodedata
from collections import defaultdict
from datetime import datetime, timezone

SNAPSHOT_DATE = "2026-11-22"

# GeoNames admin1 code -> official Indonesian province name.
PROVINCES = {
    "01": "Aceh",
    "02": "Bali",
    "03": "Bengkulu",
    "04": "Daerah Khusus Ibukota Jakarta",
    "05": "Jambi",
    "07": "Jawa Tengah",
    "08": "Jawa Timur",
    "10": "Daerah Istimewa Yogyakarta",
    "11": "Kalimantan Barat",
    "12": "Kalimantan Selatan",
    "13": "Kalimantan Tengah",
    "14": "Kalimantan Timur",
    "15": "Lampung",
    "17": "Nusa Tenggara Barat",
    "18": "Nusa Tenggara Timur",
    "21": "Sulawesi Tengah",
    "22": "Sulawesi Tenggara",
    "24": "Sumatera Barat",
    "26": "Sumatera Utara",
    "28": "Maluku",
    "29": "Maluku Utara",
    "30": "Jawa Barat",
    "31": "Sulawesi Utara",
    "32": "Sumatera Selatan",
    "33": "Banten",
    "34": "Gorontalo",
    "35": "Kepulauan Bangka Belitung",
    "36": "Papua",
    "37": "Riau",
    "38": "Sulawesi Selatan",
    "39": "Papua Barat",
    "40": "Kepulauan Riau",
    "41": "Sulawesi Barat",
    "42": "Kalimantan Utara",
    "PD": "Papua Barat Daya",
    "PT": "Papua Tengah",
    "PE": "Papua Pegunungan",
    "PS": "Papua Selatan",
}

INSERT_ROW = re.compile(r"\('([^']*)'\s*,\s*'((?:[^']|'')*)'")


def clean(value: str) -> str:
    return " ".join(value.split())


def normalize(value: str) -> str:
    """Normalize a name for matching: lowercase, strip diacritics and punctuation."""
    decomposed = unicodedata.normalize("NFD", value.casefold())
    stripped = "".join(c for c in decomposed if unicodedata.category(c) != "Mn")
    return " ".join(re.findall(r"[a-z0-9]+", stripped))


def read_lines(path: str):
    with open(path, encoding="utf-8", errors="replace") as handle:
        for line in handle:
            yield line.rstrip("\n")


def load_kemendagri(path: str) -> tuple[dict[str, str], dict[str, str]]:
    """Return (code -> official name) and (normalized regency name -> code)."""
    names: dict[str, str] = {}
    for line in read_lines(path):
        match = INSERT_ROW.search(line)
        if match:
            names[match.group(1)] = clean(match.group(2).replace("''", "'"))
    regencies: dict[str, str] = {}
    for code, name in names.items():
        if re.fullmatch(r"\d{2}\.\d{2}", code):
            regencies.setdefault(normalize(name), code)
    return names, regencies


def load_admin1(path: str) -> dict[str, str]:
    result: dict[str, str] = {}
    for line in read_lines(path):
        fields = line.split("\t")
        if len(fields) >= 2 and fields[0]:
            result[fields[0].upper()] = clean(fields[1])
    return result


def load_countries(path: str) -> dict[str, str]:
    result: dict[str, str] = {}
    for line in read_lines(path):
        if line.startswith("#") or not line.strip():
            continue
        fields = line.split("\t")
        if len(fields) > 4 and re.fullmatch(r"[A-Z]{2}", fields[0]):
            result[fields[0]] = clean(fields[4])
    return result


def load_admin2(path: str) -> dict[str, str]:
    result: dict[str, str] = {}
    for line in read_lines(path):
        fields = line.split("\t")
        if len(fields) >= 2 and fields[0].startswith("ID."):
            result[fields[0]] = clean(fields[1])
    return result


def load_aliases(path: str, wanted_ids: set[str]) -> dict[str, str]:
    """Keep human-readable alternate names for the requested GeoNames IDs."""
    result: dict[str, list[str]] = defaultdict(list)
    skipped_languages = {"link", "wkdt", "geonames", "pic", "email", "tel", "fax"}
    with open(path, encoding="utf-8", errors="replace") as handle:
        for line in handle:
            fields = line.rstrip("\n").split("\t")
            if len(fields) < 4 or fields[1] not in wanted_ids:
                continue
            language = fields[2].strip().lower()
            if language and language not in {"id", "en", "ms", "ar", "zh", "nl"}:
                continue
            if language in skipped_languages:
                continue
            name = clean(fields[3])
            if name and name not in result[fields[1]]:
                result[fields[1]].append(name)
    return {key: "|".join(value[:12]) for key, value in result.items()}


def valid_dem(value: str) -> str:
    try:
        number = int(value)
    except (TypeError, ValueError):
        return ""
    return str(number) if -500 <= number <= 9000 else ""


def collect_wanted_ids(paths: list[str]) -> set[str]:
    result: set[str] = set()
    for path in paths:
        for line in read_lines(path):
            fields = line.split("\t")
            if fields and fields[0].isdigit():
                result.add(fields[0])
    return result


def build_global(
    source_dir: str,
    aliases: dict[str, str],
    admin1: dict[str, str],
    countries: dict[str, str],
    output: str,
) -> dict[str, object]:
    rows: list[tuple[str, ...]] = []
    for line in read_lines(os.path.join(source_dir, "cities500.txt")):
        fields = line.split("\t")
        if len(fields) < 18 or fields[6] != "P":
            continue
        geoname_id = fields[0]
        name = clean(fields[1] or fields[2])
        if not name or not fields[4] or not fields[5]:
            continue
        country_code = fields[8].upper()
        admin1_code = fields[10]
        region = admin1.get(f"{country_code}.{admin1_code}", "")
        country = countries.get(country_code, clean(fields[2]))
        alias_text = aliases.get(geoname_id, "") or clean(fields[3]).replace(",", "|")
        rows.append((
            name,
            region,
            country,
            fields[4],
            fields[5],
            country_code,
            valid_dem(fields[16]),
            "",  # sampled_elevation
            clean(fields[7]),
            fields[14] if fields[14].isdigit() else "0",
            alias_text,
            "",  # elevation_source
            "",  # elevation_quality
        ))
    rows.sort(key=lambda row: (row[5], row[2], row[1], row[0]))
    write_gzip(output, rows)
    return {"rows": len(rows), "file": os.path.basename(output), "bytes": os.path.getsize(output)}


def build_indonesia(
    source_dir: str,
    aliases: dict[str, str],
    admin2: dict[str, str],
    kemendagri: dict[str, str],
    regency_codes: dict[str, str],
    output: str,
) -> dict[str, object]:
    rows: list[tuple[str, ...]] = []
    matched = 0
    for line in read_lines(os.path.join(source_dir, "ID.txt")):
        fields = line.split("\t")
        if len(fields) < 17 or fields[6] != "P":
            continue
        geoname_id = fields[0]
        name = clean(fields[1] or fields[2])
        if not name or not fields[4] or not fields[5]:
            continue
        province = PROVINCES.get(fields[10])
        if province is None:
            continue
        regency = clean(admin2.get(f"ID.{fields[10]}.{fields[11]}", ""))
        region_code = ""
        official_match = "0"
        key = normalize(regency)
        if key in regency_codes:
            region_code = regency_codes[key]
            regency = kemendagri[region_code]
            official_match = "1"
        alias_text = aliases.get(geoname_id, "") or clean(fields[3]).replace(",", "|")
        rows.append((
            name,
            regency,
            province,
            fields[4],
            fields[5],
            valid_dem(fields[16]),
            "",  # sampled_elevation
            clean(fields[7]),
            fields[14] if fields[14].isdigit() else "0",
            alias_text,
            "",  # district
            region_code,
            official_match,
            "",  # elevation_source
            "",  # elevation_quality
        ))
        if official_match == "1":
            matched += 1
    rows.sort(key=lambda row: (row[2], row[1], row[0], float(row[3]), float(row[4])))
    write_gzip(output, rows)
    return {
        "rows": len(rows),
        "file": os.path.basename(output),
        "bytes": os.path.getsize(output),
        "official_regency_matches": matched,
    }


def write_gzip(output: str, rows: list[tuple[str, ...]]) -> None:
    os.makedirs(os.path.dirname(os.path.abspath(output)), exist_ok=True)
    with gzip.open(output, "wt", encoding="utf-8", newline="\n", compresslevel=9) as handle:
        for row in rows:
            handle.write("\t".join(row) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sources", required=True, help="Directory with the downloaded source files")
    parser.add_argument("--work", required=True, help="Directory with extracted GeoNames TXT files")
    parser.add_argument("--output", required=True, help="Destination directory for the v2 datasets")
    args = parser.parse_args()

    print("[1/6] Loading Kemendagri hierarchy...", flush=True)
    kemendagri, regency_codes = load_kemendagri(os.path.join(args.sources, "wilayah.sql"))
    print(f"      wilayah={len(kemendagri)} regency_names={len(regency_codes)}", flush=True)

    print("[2/6] Loading GeoNames code maps...", flush=True)
    admin1 = load_admin1(os.path.join(args.sources, "admin1CodesASCII.txt"))
    countries = load_countries(os.path.join(args.sources, "countryInfo.txt"))
    admin2 = load_admin2(os.path.join(args.sources, "admin2Codes.txt"))

    print("[3/6] Collecting alternate names (large file, please wait)...", flush=True)
    wanted = collect_wanted_ids([
        os.path.join(args.work, "cities500.txt"),
        os.path.join(args.work, "ID.txt"),
    ])
    aliases = load_aliases(os.path.join(args.work, "alternateNamesV2.txt"), wanted)
    print(f"      ids={len(wanted)} with_aliases={len(aliases)}", flush=True)

    print("[4/6] Building global dataset...", flush=True)
    global_stats = build_global(
        args.work, aliases, admin1, countries,
        os.path.join(args.output, "cities500.tsv.gz"))

    print("[5/6] Building Indonesian dataset...", flush=True)
    indonesia_stats = build_indonesia(
        args.work, aliases, admin2, kemendagri, regency_codes,
        os.path.join(args.output, "indonesia.tsv.gz"))

    print("[6/6] Writing manifest...", flush=True)
    manifest = {
        "schema_version": 2,
        "snapshot_date": SNAPSHOT_DATE,
        "generated_at": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "sources": {
            "geonames": "https://download.geonames.org/export/dump/ (CC BY 4.0)",
            "kemendagri": "https://github.com/cahyadsn/wilayah (Kepmendagri 300.2.2-2430/2025)",
        },
        "datasets": {"global": global_stats, "indonesia": indonesia_stats},
    }
    with open(os.path.join(args.output, "dataset-manifest.json"), "w", encoding="utf-8") as handle:
        json.dump(manifest, handle, indent=2, ensure_ascii=False)
    print(json.dumps(manifest, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main())
