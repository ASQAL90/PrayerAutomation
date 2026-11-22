#!/usr/bin/env python3
"""Add sampled Copernicus GLO-90 elevation to v2 location datasets.

Sampling uses the free Open-Meteo Elevation API (Copernicus DEM GLO-90, ~90 m).
A median filter over an N x N window suppresses isolated DSM spikes from roofs
and tree canopies. The original GeoNames DEM column is kept as a fallback.

The API is rate limited to 600 requests/minute with up to 100 coordinates per
request. This tool stays below the limit and supports resuming an interrupted
run by skipping rows that already carry a sampled elevation.

Usage:
  python3 sample_elevation.py --input DIR --output DIR [--window 3] [--rate 9]

Input files in DIR: cities500.tsv.gz, indonesia.tsv.gz
Output files in DIR: same names, with sampled_elevation/elevation_source/
elevation_quality columns filled.
"""

from __future__ import annotations

import argparse
import gzip
import json
import os
import statistics
import sys
import time
import urllib.parse
import urllib.request
from typing import Iterable

API_URL = "https://api.open-meteo.com/v1/elevation"
SOURCE = "copernicus-glo-90"
QUALITY = "median-3x3"
# One arcsecond of longitude at the equator is ~30 m; GLO-90 cells are ~90 m.
OFFSET_DEGREES = 0.00081


def read_rows(path: str) -> list[list[str]]:
    with gzip.open(path, "rt", encoding="utf-8") as handle:
        return [line.rstrip("\n").split("\t") for line in handle if line.strip()]


def write_rows(path: str, rows: Iterable[list[str]]) -> None:
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    with gzip.open(path, "wt", encoding="utf-8", newline="\n", compresslevel=9) as handle:
        for row in rows:
            handle.write("\t".join(row) + "\n")


def offsets(window: int) -> list[tuple[float, float]]:
    radius = window // 2
    return [(dx, dy) for dy in range(-radius, radius + 1)
            for dx in range(-radius, radius + 1)]


def fetch_elevations(latitudes: list[float], longitudes: list[float], attempts: int = 6) -> list[float | None]:
    query = urllib.parse.urlencode({
        "latitude": ",".join(f"{value:.6f}" for value in latitudes),
        "longitude": ",".join(f"{value:.6f}" for value in longitudes),
    })
    last_error: Exception | None = None
    for attempt in range(attempts):
        try:
            request = urllib.request.Request(
                f"{API_URL}?{query}",
                headers={"User-Agent": "AyoJamaah-DatasetBuilder/2.5.1"})
            with urllib.request.urlopen(request, timeout=45) as response:
                payload = json.load(response)
            values = payload.get("elevation", [])
            if not isinstance(values, list) or len(values) != len(latitudes):
                raise ValueError("unexpected elevation response length")
            return [None if value is None else float(value) for value in values]
        except Exception as ex:  # noqa: BLE001 - retry every transient failure
            last_error = ex
            time.sleep(min(30, 2 ** attempt))
    raise RuntimeError(f"elevation API failed after {attempts} attempts: {last_error}")


def sample_dataset(path: str, output_path: str, window: int, rate: float, limit: int | None,
                   sampled_index: int, source_index: int, quality_index: int,
                   population_index: int, min_population: int, top: int,
                   focus: list[str]) -> dict[str, object]:
    rows = read_rows(path)
    points = [(float(row[3]), float(row[4])) for row in rows]
    offsets_list = offsets(window)

    selected = set()
    for index, row in enumerate(rows):
        population = int(row[population_index]) if row[population_index].isdigit() else 0
        if min_population > 0 and population < min_population:
            continue
        selected.add(index)
    if top > 0:
        ranked = sorted(selected, key=lambda index: (
            -int(rows[index][population_index]) if rows[index][population_index].isdigit() else 0,
            rows[index][0]))
        selected = set(ranked[:top])
    for item in focus:
        try:
            focus_lat, focus_lon = (float(value) for value in item.split(","))
        except ValueError:
            continue
        nearest = min(range(len(rows)), key=lambda index:
                      (points[index][0] - focus_lat) ** 2 + (points[index][1] - focus_lon) ** 2)
        selected.add(nearest)

    # Flatten centre + neighbours so one API batch covers many settlements.
    tasks: list[tuple[int, int]] = []  # (row index, offset index)
    for row_index in sorted(selected):
        for offset_index, (dx, dy) in enumerate(offsets_list):
            tasks.append((row_index, offset_index))

    if limit is not None:
        tasks = tasks[:limit]

    samples: dict[int, list[float]] = {index: [] for index in range(len(rows))}
    delay = 1.0 / rate
    last_request = 0.0
    batches = (len(tasks) + 99) // 100
    print(f"      rows={len(rows)} selected={len(selected)} tasks={len(tasks)} batches={batches}", flush=True)

    for batch_index, start in enumerate(range(0, len(tasks), 100), start=1):
        chunk = tasks[start:start + 100]
        latitudes = []
        longitudes = []
        for row_index, offset_index in chunk:
            latitude, longitude = points[row_index]
            dx, dy = offsets_list[offset_index]
            latitudes.append(max(-90.0, min(90.0, latitude + dy * OFFSET_DEGREES)))
            longitudes.append(max(-180.0, min(180.0, longitude + dx * OFFSET_DEGREES)))
        elapsed = time.monotonic() - last_request
        if elapsed < delay:
            time.sleep(delay - elapsed)
        values = fetch_elevations(latitudes, longitudes)
        for (row_index, _), value in zip(chunk, values):
            if value is not None:
                samples[row_index].append(value)
        last_request = time.monotonic()
        if batch_index % 100 == 0:
            print(f"      batch {batch_index}/{batches}", flush=True)

    filled = 0
    for index, row in enumerate(rows):
        values = samples[index]
        if values:
            median = int(round(statistics.median(values)))
            if -500 <= median <= 9000:
                row[sampled_index] = str(median)
                row[source_index] = SOURCE
                row[quality_index] = QUALITY
                filled += 1
        elif row[source_index] == "" and row[sampled_index] == "":
            row[source_index] = "geonames-dem"
            row[quality_index] = "dem-3as-area"
    write_rows(output_path, rows)
    print(f"      filled={filled}/{len(rows)} -> {output_path}", flush=True)
    return {"rows": len(rows), "filled": filled, "file": os.path.basename(output_path)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", required=True, help="Directory with the v2 gzip datasets")
    parser.add_argument("--output", required=True, help="Directory for sampled datasets")
    parser.add_argument("--window", type=int, default=3, help="Median window size (odd, default 3)")
    parser.add_argument("--rate", type=float, default=9.0, help="Requests per second (default 9, below 600/min)")
    parser.add_argument("--limit", type=int, default=None, help="Limit tasks for a smoke run")
    parser.add_argument("--only", choices=("global", "indonesia", "both"), default="both")
    parser.add_argument("--min-population", type=int, default=0,
                        help="Only sample rows at or above this population (default 0 = all)")
    parser.add_argument("--top", type=int, default=0,
                        help="When set, sample only the N largest settlements by population")
    parser.add_argument("--focus", action="append", default=[], metavar="LAT,LON",
                        help="Always sample the nearest row to this coordinate (repeatable)")
    args = parser.parse_args()

    if args.window < 1 or args.window % 2 == 0:
        parser.error("--window must be a positive odd number")
    if args.rate <= 0 or args.rate > 10:
        parser.error("--rate must be between 0 and 10 requests/second")

    jobs = []
    if args.only in ("global", "both"):
        jobs.append(("cities500.tsv.gz", "global", 7, 11, 12, 9))
    if args.only in ("indonesia", "both"):
        jobs.append(("indonesia.tsv.gz", "indonesia", 6, 13, 14, 8))

    result: dict[str, object] = {}
    for filename, label, sampled, source, quality, population in jobs:
        print(f"[{label}] sampling {filename}...", flush=True)
        result[label] = sample_dataset(
            os.path.join(args.input, filename),
            os.path.join(args.output, filename),
            args.window, args.rate, args.limit, sampled, source, quality,
            population, args.min_population, args.top, args.focus)
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
