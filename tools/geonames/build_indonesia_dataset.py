#!/usr/bin/env python3
"""Build the bundled Indonesian place dataset from GeoNames ID.txt.

Input (download from https://download.geonames.org/export/dump/):
  ID.txt                GeoNames dump for Indonesia
  admin2Codes.txt       GeoNames admin2 code -> name map

Output format (UTF-8, tab separated, gzip):
  name <tab> regency <tab> province <tab> latitude <tab> longitude <tab> dem

The province is a proper Indonesian name derived from the GeoNames admin1 code,
not the English admin1CodesASCII name. Regency names are normalised to their
Indonesian form; the nine English entries in GeoNames are mapped explicitly.
Feature class P (populated place) is kept, matching the global cities1000 layer.

Usage:
  python3 build_indonesia_dataset.py --input DIR --output FILE
"""

from __future__ import annotations

import argparse
import gzip
import os
import sys

# GeoNames admin1 code -> official Indonesian province name (38 provinces, 2022+).
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

# The only English-style admin2 names in GeoNames for Indonesia.
REGENCY_OVERRIDES = {
    "East Halmahera Regency": "Kabupaten Halmahera Timur",
    "Gresik Regency": "Kabupaten Gresik",
    "Lahat Regency": "Kabupaten Lahat",
    "Morowali Regency": "Kabupaten Morowali",
    "Selayar Islands Regency": "Kabupaten Kepulauan Selayar",
    "South Tapanuli Regency": "Kabupaten Tapanuli Selatan",
    "Tojo Una-Una Regency": "Kabupaten Tojo Una-Una",
    "Wakatobi Regency": "Kabupaten Wakatobi",
    "West Pesisir Regency": "Kabupaten Pesisir Barat",
}


def load_admin2(path: str) -> dict[str, str]:
    mapping: dict[str, str] = {}
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            fields = line.rstrip("\n").split("\t")
            if len(fields) >= 2 and fields[0].startswith("ID."):
                mapping[fields[0]] = fields[1]
    return mapping


def clean(value: str) -> str:
    return " ".join(value.split())


def build(input_dir: str, output: str) -> int:
    admin2 = load_admin2(os.path.join(input_dir, "admin2Codes.txt"))
    rows: list[tuple[str, str, str, str, str, str]] = []
    skipped = 0

    with open(os.path.join(input_dir, "ID.txt"), encoding="utf-8") as handle:
        for line in handle:
            fields = line.rstrip("\n").split("\t")
            if len(fields) < 17:
                skipped += 1
                continue
            if fields[6] != "P":  # feature class: populated place
                continue
            latitude, longitude = fields[4], fields[5]
            if not latitude or not longitude:
                skipped += 1
                continue
            admin1_code = fields[10]
            province = PROVINCES.get(admin1_code)
            if province is None:
                skipped += 1
                continue
            name = clean(fields[1] or fields[2])
            if not name:
                skipped += 1
                continue
            regency_key = f"ID.{admin1_code}.{fields[11]}"
            regency = admin2.get(regency_key, "")
            regency = REGENCY_OVERRIDES.get(regency, regency)
            regency = clean(regency)
            dem = fields[16].strip()
            if dem:
                try:
                    value = int(dem)
                except ValueError:
                    dem = ""
                else:
                    dem = str(value) if -500 <= value <= 9000 else ""
            rows.append((name, regency, province, latitude, longitude, dem))

    rows.sort(key=lambda row: (row[2], row[1], row[0], float(row[3]), float(row[4])))

    os.makedirs(os.path.dirname(os.path.abspath(output)), exist_ok=True)
    with gzip.open(output, "wt", encoding="utf-8", newline="\n", compresslevel=9) as out:
        for row in rows:
            out.write("\t".join(row) + "\n")

    print(f"rows={len(rows)} skipped={skipped} bytes={os.path.getsize(output)}")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", required=True, help="Directory with ID.txt and admin2Codes.txt")
    parser.add_argument("--output", required=True, help="Destination indonesia.tsv.gz")
    args = parser.parse_args()
    return build(args.input, args.output)


if __name__ == "__main__":
    sys.exit(main())
