#!/usr/bin/env python3
"""Build the bundled Indonesia land-boundary asset from Natural Earth.

Source: Natural Earth 1:10m Admin 0 Countries (public domain)
  https://www.naturalearthdata.com/
  https://github.com/nvkelso/natural-earth-vector

The script keeps only the Indonesia feature (ISO_A3 = IDN) and writes every
exterior ring as a standalone polygon in a compact binary format:

  magic  "AJB1"                    4 bytes
  int32  polygonCount              little endian
  per polygon:
    int32 pointCount               little endian
    pointCount x (double lon, double lat)

The resulting file is ~0.3 MB and is used for a fast point-in-polygon test so
the Indonesian place layer is only used for coordinates inside the country.

Usage:
  python3 build_indonesia_boundary.py --input idn10.geojson --output indonesia.boundary.bin
"""

from __future__ import annotations

import argparse
import json
import os
import struct
import sys


def rings_from_geometry(geometry: dict) -> list[list[list[float]]]:
    if geometry["type"] == "Polygon":
        return [geometry["coordinates"][0]]
    if geometry["type"] == "MultiPolygon":
        return [polygon[0] for polygon in geometry["coordinates"]]
    raise ValueError(f"unsupported geometry type: {geometry['type']}")


def build(input_path: str, output_path: str) -> int:
    with open(input_path, encoding="utf-8") as handle:
        data = json.load(handle)

    features = data.get("features", [data])
    feature = next(
        (
            item
            for item in features
            if item.get("properties", {}).get("ISO_A3") == "IDN"
            or item.get("properties", {}).get("ADMIN") == "Indonesia"
        ),
        None,
    )
    if feature is None:
        raise SystemExit("Indonesia feature not found in the Natural Earth file")

    rings = rings_from_geometry(feature["geometry"])
    # Drop degenerate rings; keep coordinate order as supplied.
    rings = [ring for ring in rings if len(ring) >= 3]

    os.makedirs(os.path.dirname(os.path.abspath(output_path)), exist_ok=True)
    with open(output_path, "wb") as out:
        out.write(b"AJB1")
        out.write(struct.pack("<i", len(rings)))
        total = 0
        for ring in rings:
            out.write(struct.pack("<i", len(ring)))
            for lon, lat in ring:
                out.write(struct.pack("<dd", float(lon), float(lat)))
            total += len(ring)

    print(
        f"polygons={len(rings)} points={total} bytes={os.path.getsize(output_path)}"
    )
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", required=True, help="Natural Earth Indonesia GeoJSON")
    parser.add_argument("--output", required=True, help="Destination indonesia.boundary.bin")
    args = parser.parse_args()
    return build(args.input, args.output)


if __name__ == "__main__":
    sys.exit(main())
