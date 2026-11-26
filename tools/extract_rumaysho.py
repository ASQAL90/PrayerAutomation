#!/usr/bin/env python3
"""Extract Dzikir & Doa content from Rumaysho.com into dzikir-doa.json.

Stdlib only. Raw HTML is cached in the temp directory and every stored field is
checked against the source text after whitespace normalisation.

Usage:
    python3 tools/extract_rumaysho.py            # download, extract, validate
    python3 tools/extract_rumaysho.py --use-cache
"""

from __future__ import annotations

import argparse
import html as html_module
import json
import re
import subprocess
import sys
from datetime import datetime, timezone
from html.parser import HTMLParser
from pathlib import Path
from urllib.request import Request, urlopen

REPO_ROOT = Path(__file__).resolve().parent.parent
OUTPUT = REPO_ROOT / "src" / "PrayerAutomation.UI" / "Data" / "dzikir-doa.json"
DEFAULT_CACHE = Path("/data/data/com.termux/files/usr/tmp/opencode/rumaysho")
CREDIT = "Rumaysho.Com — Muhammad Abduh Tuasikal"

DZIKIR_PAGI = "https://rumaysho.com/1636-bacaan-dzikir-pagi.html"
DZIKIR_PETANG = "https://rumaysho.com/1638-bacaan-dzikir-petang.html"
DOA_URLS = [
    "https://rumaysho.com/13118-kumpulan-doa-pembuka-pintu-rezeki.html",
    "https://rumaysho.com/23819-doa-saat-galau-berat-doa-untuk-kesedihan-yang-mendalam.html",
    "https://rumaysho.com/23361-waktu-berdoa-yang-paling-didengar.html",
    "https://rumaysho.com/23020-doa-meminta-rahmat-ampunan-selamat-dari-dosa.html",
    "https://rumaysho.com/22167-doa-agar-mendapatkan-ilham-berupa-petunjuk.html",
    "https://rumaysho.com/22120-doa-agar-diberi-kecukupan-dengan-yang-halal.html",
    "https://rumaysho.com/21390-doa-meminta-perlindungan-dari-jeleknya-pendengaran-penglihatan-lisan-hati-kemaluan.html",
    "https://rumaysho.com/21229-doa-meminta-perlindungan-dari-siksa-neraka-dari-kejelekan-kaya-dan-miskin.html",
    "https://rumaysho.com/21000-doa-agar-tidak-malas-disucikan-jiwa-diberi-hati-yang-khusyuk.html",
    "https://rumaysho.com/20957-doa-agar-nikmat-tidak-hilang-dan-musibah-tidak-datang-secara-mendadak.html",
    "https://rumaysho.com/20188-doa-agar-tidak-malas-dan-terbebas-dari-lilitan-utang.html",
    "https://rumaysho.com/20123-doa-meminta-hidayah-dan-istiqamah-di-atas-kebenaran.html",
    "https://rumaysho.com/19408-doa-dari-orang-yang-kesulitan.html",
    "https://rumaysho.com/9690-doa-ketika-hujan-deras.html",
]

# Judul deskriptif buatan pemilik; Arab, Latin, dan arti tetap dari sumber.
DOA_TITLES = {
    "doa-13118-01": "Doa memohon ilmu, rezeki halal, dan amal diterima",
    "doa-13118-02": "Doa memohon kecukupan rezeki halal",
    "doa-13118-03": "Doa memohon ampunan, rahmat, petunjuk, dan keselamatan",
    "doa-13118-04": "Doa memohon keberkahan harta, anak, dan umur panjang",
    "doa-23819-01": "Doa saat galau berat dan kesedihan mendalam",
    "doa-23020-01": "Doa memohon rahmat, ampunan, dan selamat dari dosa",
    "doa-22167-01": "Doa memohon ilham petunjuk dan perlindungan dari diri sendiri",
    "doa-22120-01": "Doa memohon kecukupan dengan yang halal",
    "doa-21390-01": "Doa perlindungan pendengaran, penglihatan, lisan, hati, dan kemaluan",
    "doa-21229-01": "Doa perlindungan dari siksa neraka serta kejelekan kaya dan miskin",
    "doa-21000-01": "Doa perlindungan dari malas, kikiran, dan agar hati khusyuk",
    "doa-20957-01": "Doa agar nikmat tidak hilang dan musibah tidak datang mendadak",
    "doa-20188-01": "Doa perlindungan dari malas, pengecut, dan lilitan utang",
    "doa-20123-01": "Doa memohon hidayah dan istiqamah di atas kebenaran",
    "doa-9690-01": "Doa Nabi agar hujan dihentikan saat membahayakan",
    "doa-9690-02": "Doa Nabi meminta hujan turun",
    "doa-9690-03": "Doa ketika hujan deras membawa bahaya",
}

BLOCK_TAGS = {
    "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "blockquote",
    "div", "table", "tr", "td", "br",
}
SKIP_TAGS = {"script", "style", "noscript", "svg"}


def norm(text: str) -> str:
    """Only collapse whitespace; characters themselves stay identical."""
    return re.sub(r"\s+", " ", html_module.unescape(text)).strip()


# Sumber menulis "1o" (huruf o) pada dua catatan; dibetulkan hanya pada teks
# ringkas "Dibaca ... x" dan diverifikasi dengan normalisasi yang sama.
NOTE_FIXES = {"Dibaca 1o x": "Dibaca 10 x"}


def fix_note(text: str) -> str:
    for wrong, right in NOTE_FIXES.items():
        text = text.replace(wrong, right)
    return text


def page_id(url: str) -> str:
    return url.split("/")[-1].split("-", 1)[0]


class TextCollector(HTMLParser):
    """All visible text of a document, for source-substring verification."""

    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.parts: list[str] = []
        self.skip = 0

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag in SKIP_TAGS:
            self.skip += 1

    def handle_endtag(self, tag: str) -> None:
        if tag in SKIP_TAGS and self.skip:
            self.skip -= 1

    def handle_data(self, data: str) -> None:
        if not self.skip:
            self.parts.append(data)


class ArticleBlocks(HTMLParser):
    """Block-level text of the first div.entry-content."""

    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.blocks: list[tuple[str, str]] = []
        self.stack: list[str] = []
        self.buf: list[str] = []
        self.depth = 0
        self.skip = 0

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag in SKIP_TAGS:
            self.skip += 1
            return
        if self.skip:
            return
        attrs_dict = dict(attrs)
        classes = (attrs_dict.get("class") or "").split()
        if tag == "div" and "entry-content" in classes and self.depth == 0:
            self.depth = 1
            return
        if self.depth:
            if tag == "div":
                self.depth += 1
            if tag == "br":
                self.buf.append("\n")
            elif tag in BLOCK_TAGS:
                self.stack.append(tag)

    def handle_endtag(self, tag: str) -> None:
        if tag in SKIP_TAGS and self.skip:
            self.skip -= 1
            return
        if self.skip or not self.depth:
            return
        if tag in BLOCK_TAGS and self.stack:
            idx = len(self.stack) - 1
            while idx >= 0 and self.stack[idx] != tag:
                idx -= 1
            if idx >= 0:
                text = norm(" ".join(self.buf[idx:]))
                self.buf = self.buf[:idx]
                self.stack = self.stack[:idx]
                if text:
                    self.blocks.append((tag, text))
        if tag == "div":
            self.depth -= 1

    def handle_data(self, data: str) -> None:
        if self.depth and not self.skip:
            self.buf.append(data)


def is_arabic(text: str) -> bool:
    letters = [c for c in text if c.isalpha()]
    if not letters:
        return False
    arabic = sum(
        1 for c in letters
        if "\u0600" <= c <= "\u06ff" or "\u0750" <= c <= "\u077f"
        or "\ufb50" <= c <= "\ufdff" or "\ufe70" <= c <= "\ufeff"
    )
    has_latin = any("a" <= c.lower() <= "z" for c in text)
    return arabic / len(letters) > 0.85 and not has_latin


INDONESIAN_WORDS = re.compile(
    r"\b(yang|dengan|kepada|dari|untuk|karena|adalah|beliau|radhiyallahu|"
    r"shallallahu|mengatakan|bersabda|artinya|faedah|hadits|baca|doa|"
    r"katakanlah|apabila|sesungguhnya|maka|ia|aku|kami|engkau)\b",
    re.IGNORECASE,
)


def looks_like_translit(text: str) -> bool:
    if is_arabic(text) or not re.search(r"[A-Za-z]", text):
        return False
    if text.startswith(("“", '"', "Artinya", "Faedah", "Baca", "Membaca", "[")):
        return False
    if INDONESIAN_WORDS.search(text):
        return False
    tokens = re.findall(r"[A-Za-z]+", text)
    return len(tokens) >= 3


def quoted(text: str) -> str | None:
    m = re.search(r"“(.*?)”", text)
    return norm(m.group(1)) if m else None


def note_from(text: str) -> str:
    m = re.search(r"\(Dibaca[^)]*\)", text)
    return fix_note(norm(m.group(0)[1:-1])) if m else ""


def title_from(raw_html: str) -> str:
    m = re.search(r"<title[^>]*>(.*?)</title>", raw_html, re.I | re.S)
    title = norm(m.group(1)) if m else ""
    return re.sub(r"\s*[|-]\s*Rumaysho\.Com\s*$", "", title, flags=re.I)


def arabic_dua_from(text: str) -> str:
    """Take the supplication itself out of a hadith sentence."""
    for opener, closer in (("(( ", " ))"), ("((", "))"), ("« ", " »"), ("«", "»")):
        if opener in text and closer in text:
            inner = text.split(opener, 1)[1].split(closer, 1)[0]
            inner = norm(inner)
            if is_arabic(inner):
                return inner
    for marker in ("قُلْ :", "قُلْ:", "يَقُوْلُ :", "يقول :"):
        if marker in text:
            tail = norm(text.split(marker, 1)[1])
            tail = re.split(r"\s+(?:رَوَاهُ|رواه|مُتَّفَقٌ|وَالْتِّرْمِذِي)", tail)[0]
            tail = tail.rstrip(" .")
            if is_arabic(tail):
                return tail
    match = re.search(r"(الل[َّه]مّ|اللَّهُمَّ|اللَّهُمّ|اللَّهُمَّ)", text)
    if match:
        tail = norm(text[match.start():])
        tail = re.split(r"\s+(?:رَوَاهُ|رواه)", tail)[0].rstrip(" .")
        if is_arabic(tail):
            return tail
    return ""


UPPERSPAN = re.compile(r"(?<![A-Za-z])([A-Z][A-Z\-'’‘ ,\.:\?;0-9]{9,})(?![A-Za-z])")


def upper_translit(text: str) -> str:
    candidates = [norm(m.group(1)).strip("‘’'\" “”") for m in UPPERSPAN.finditer(text)]
    candidates = [c for c in candidates if re.search(r"ALL[OA]+H|ROBB|RABBI|LAA|YA ", c)]
    return max(candidates, key=len) if candidates else ""


def artinya_from(text: str) -> str:
    """Ambil isi (artinya: ...) / [Artinya: ...], termasuk kurung bersarang."""
    m = re.search(r"([\(\[])\s*[Aa]rtinya:\s*", text)
    if not m:
        return ""
    opener, closer = ("(", ")") if m.group(1) == "(" else ("[", "]")
    depth = 1
    for i in range(m.end(), len(text)):
        if text[i] == opener:
            depth += 1
        elif text[i] == closer:
            depth -= 1
            if depth == 0:
                return norm(text[m.end():i])
    return ""


def starts_with_allahumma(text: str) -> bool:
    return bool(re.match(r"^الل[َّه]مّ|^اللَّهُمّ|^اللَّهُمَّ", text))


def extract_dzikir(raw_html: str, section_id: str, page_url: str) -> tuple[list[dict], list[str]]:
    blocks = ArticleBlocks()
    blocks.feed(raw_html)
    end = next(
        (i for i, (_, t) in enumerate(blocks.blocks)
         if t.startswith("Baca Juga:") or re.match(r"^\[1\]\s+HR\.", t)),
        len(blocks.blocks),
    )
    body = blocks.blocks[:end]
    heads = [
        i for i, (tag, t) in enumerate(body)
        if re.fullmatch(r"\[\d+\]", t) and tag in ("h3", "p")
        and not t.startswith("[1] HR")
    ]
    entries: list[dict] = []
    skipped: list[str] = []
    for n, start in enumerate(heads):
        stop = heads[n + 1] if n + 1 < len(heads) else len(body)
        group = [t for _, t in body[start + 1:stop]]
        number = re.fullmatch(r"\[(\d+)\]", body[start][1]).group(1)
        arabic = next((t for t in group if is_arabic(t)), "")
        latin = next(
            (t for t in group
             if t and not is_arabic(t) and not t.startswith(("“", '"'))
             and not t.startswith(("Artinya", "Faedah", "Membaca", "Baca", "["))),
            "",
        )
        meaning = next((q for t in group if (q := quoted(t))), "")
        note = note_from(" ".join(group))
        if not arabic or not meaning:
            skipped.append(f"[{number}] Arab/arti tidak lengkap")
            continue
        if not latin:
            # Sumber sengaja tidak memberi transliterasi untuk bacaan Al-Qur'an.
            skipped.append(f"[{number}] tidak ada transliterasi di sumber")
            continue
        entries.append({
            "id": f"{section_id}-{int(number):02d}",
            "arabic": arabic,
            "latin": latin,
            "meaning": meaning,
            "note": note,
            "source_url": page_url,
        })
    return entries, skipped


def extract_doa(raw_html: str, page_url: str) -> tuple[list[dict], list[str], list[str]]:
    blocks = ArticleBlocks()
    blocks.feed(raw_html)
    body = [(tag, t) for tag, t in blocks.blocks if tag not in ("div", "table", "tr", "td")]
    title = title_from(raw_html)
    pid = page_id(page_url)
    entries: list[dict] = []
    notes: list[str] = []
    used: set[int] = set()

    def add(arabic: str, latin: str, meaning: str, idx: int) -> None:
        if not arabic or not meaning:
            return
        if any(arabic in e["arabic"] or e["arabic"] in arabic for e in entries):
            return
        used.add(idx)
        entry_id = f"doa-{pid}-{len(entries) + 1:02d}"
        entries.append({
            "id": entry_id,
            "entry_title": DOA_TITLES.get(entry_id, title),
            "arabic": arabic,
            "latin": latin,
            "meaning": meaning,
            "note": "",
            "source_url": page_url,
            "source_title": title,
        })

    # Pola 1: Arab, transliterasi, (Artinya:), arti.
    for i in range(len(body) - 2):
        if i in used or not is_arabic(body[i][1]):
            continue
        latin = body[i + 1][1]
        if not looks_like_translit(latin):
            continue
        if body[i + 2][1].lower().startswith("artinya:"):
            meaning = quoted(body[i + 3][1]) if i + 3 < len(body) else ""
            meaning = meaning or artinya_from(body[i + 3][1]) if i + 3 < len(body) else ""
        else:
            meaning = quoted(body[i + 2][1]) or artinya_from(body[i + 2][1])
        add(body[i][1], latin, meaning, i)

    # Pola 2: doa di dalam hadits; Latin dan arti berada dalam satu paragraf.
    for i, (_, text) in enumerate(body):
        if i in used or not ("artinya:" in text.lower()):
            continue
        marker = re.search(r"[\(\[][Aa]rtinya:", text)
        if not marker:
            continue
        latin = upper_translit(text[:marker.start()])
        if not latin:
            for j in range(i - 1, max(-1, i - 4), -1):
                if j not in used and re.fullmatch(r"[A-Z][A-Z\-'’‘ ,\.:\?;0-9]{9,}", body[j][1]):
                    latin = body[j][1]
                    used.add(j)
                    break
        arabic = ""
        for j in range(i - 1, max(-1, i - 5), -1):
            if is_arabic(body[j][1]):
                arabic = arabic_dua_from(body[j][1])
                if arabic:
                    break
        add(arabic, latin, artinya_from(text), i)

    # Pola 3: Arab, arti dalam tanda kutip dengan transliterasi di awalnya.
    for i in range(1, len(body)):
        if i in used or not is_arabic(body[i - 1][1]):
            continue
        text = body[i][1]
        if not text.startswith("“") or "[" not in text or "]" not in text:
            continue
        prefix = text[1:text.index("[")].strip()
        meaning = norm(text[text.index("[") + 1:text.index("]")])
        if not looks_like_translit(prefix) or not meaning:
            continue
        add(body[i - 1][1], prefix.rstrip(" ."), meaning, i)

    # Pola 4: Arab + arti saja; Latin memang tidak ada di sumber (laporkan).
    for i in range(1, len(body)):
        if i in used or not is_arabic(body[i - 1][1]):
            continue
        text = body[i][1]
        if not starts_with_allahumma(body[i - 1][1]):
            continue
        if not text.startswith("“") or "QS." in text or "(QS" in text:
            continue
        meaning = quoted(text)
        if meaning and not looks_like_translit(meaning):
            add(body[i - 1][1], "", meaning, i)
            notes.append(f"{pid}: entri dengan Latin kosong (sumber tidak menyediakan): {meaning[:60]}…")
    return entries, notes, [title]


def download(url: str, cache: Path, use_cache: bool) -> str:
    cache.mkdir(parents=True, exist_ok=True)
    target = cache / f"{page_id(url)}.html"
    if use_cache and target.exists():
        return target.read_text("utf-8", errors="replace")
    request = Request(url, headers={
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                      "(KHTML, like Gecko) Chrome/124.0 Safari/537.36",
        "Accept": "text/html,application/xhtml+xml",
        "Accept-Language": "id-ID,id;q=0.9,en-US;q=0.8",
    })
    last_error: Exception | None = None
    for _ in range(2):
        try:
            with urlopen(request, timeout=40) as response:
                raw = response.read()
            target.write_bytes(raw)
            return raw.decode("utf-8", errors="replace")
        except Exception as exc:  # noqa: BLE001 - jaringan dapat gagal berbagai cara
            last_error = exc
    # Situs kadang menolak urllib; curl (bukan dependensi aplikasi) dipakai sebagai fallback.
    try:
        subprocess.run(
            ["curl", "-sSL", "--compressed", "--retry", "2", "-A", "Mozilla/5.0",
             "-o", str(target), url],
            check=True,
        )
        return target.read_text("utf-8", errors="replace")
    except (OSError, subprocess.CalledProcessError) as exc:
        raise RuntimeError(f"Gagal mengunduh {url}: {last_error}; curl: {exc}") from exc


def validate(data: dict, source_texts: dict[str, str]) -> tuple[bool, list[str], list[str]]:
    errors: list[str] = []
    warnings: list[str] = []
    expected = ["dzikir-pagi", "dzikir-petang", "doa"]
    if data.get("schema") != 1:
        errors.append("schema harus 1")
    sections = data.get("sections", [])
    if [s.get("id") for s in sections] != expected:
        errors.append(f"section ids harus {expected}, dapat {[s.get('id') for s in sections]}")
    seen: set[str] = set()
    for section in sections:
        sid = section.get("id", "?")
        for entry in section.get("entries", []):
            eid = entry.get("id", "")
            if not eid or eid in seen:
                errors.append(f"{sid}: id kosong/duplikat: {eid!r}")
            seen.add(eid)
            for field in ("arabic", "meaning"):
                if not entry.get(field):
                    errors.append(f"{eid}: {field} kosong")
            if sid.startswith("dzikir") and not entry.get("latin"):
                errors.append(f"{eid}: latin kosong untuk dzikir")
            if sid == "doa" and not entry.get("latin"):
                warnings.append(f"{eid}: latin kosong (source_url={entry.get('source_url')})")
            if sid == "doa" and entry.get("entry_title") != DOA_TITLES.get(eid):
                errors.append(f"{eid}: entry_title harus memakai judul deskriptif yang disetujui")
            url = entry.get("source_url")
            if not (isinstance(url, str) and url.startswith("https://rumaysho.com/")):
                errors.append(f"{eid}: source_url tidak valid: {url!r}")
            source = source_texts.get(url, "")
            if not source:
                errors.append(f"{eid}: teks sumber tidak tersedia untuk verifikasi")
                continue
            for field in ("arabic", "latin", "meaning", "note"):
                value = entry.get(field, "")
                expected_value = fix_note(value) if field == "note" else value
                source_for_field = fix_note(source) if field == "note" else source
                if value and norm(expected_value) not in source_for_field:
                    errors.append(f"{eid}: {field} tidak ditemukan di HTML sumber: {value[:70]!r}")
    doa_ids = {
        entry.get("id")
        for section in sections
        if section.get("id") == "doa"
        for entry in section.get("entries", [])
    }
    if doa_ids != set(DOA_TITLES):
        errors.append(f"daftar doa berubah: {sorted(doa_ids)}")
    return not errors, errors, warnings


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--use-cache", action="store_true", help="pakai HTML yang sudah diunduh")
    parser.add_argument("--cache-dir", type=Path, default=DEFAULT_CACHE)
    args = parser.parse_args()

    all_urls = [DZIKIR_PAGI, DZIKIR_PETANG, *DOA_URLS]
    raws: dict[str, str] = {}
    for url in all_urls:
        raws[url] = download(url, args.cache_dir, args.use_cache)
        print(f"unduh: {url} ({len(raws[url])} karakter)")

    pagi, skipped_pagi = extract_dzikir(raws[DZIKIR_PAGI], "dzikir-pagi", DZIKIR_PAGI)
    petang, skipped_petang = extract_dzikir(raws[DZIKIR_PETANG], "dzikir-petang", DZIKIR_PETANG)

    doa_entries: list[dict] = []
    doa_notes: list[str] = []
    doa_per_page: list[tuple[str, int]] = []
    for url in DOA_URLS:
        entries, notes, _ = extract_doa(raws[url], url)
        doa_entries.extend(entries)
        doa_notes.extend(notes)
        doa_per_page.append((url, len(entries)))

    data = {
        "schema": 1,
        "generated_at": datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z"),
        "sections": [
            {
                "id": "dzikir-pagi",
                "title": "Dzikir pagi",
                "source_url": DZIKIR_PAGI,
                "credit": CREDIT,
                "entries": pagi,
            },
            {
                "id": "dzikir-petang",
                "title": "Dzikir petang",
                "source_url": DZIKIR_PETANG,
                "credit": CREDIT,
                "entries": petang,
            },
            {
                "id": "doa",
                "title": "Doa",
                "source_url": None,
                "credit": CREDIT,
                "entries": doa_entries,
            },
        ],
    }

    source_texts: dict[str, str] = {}
    for url, raw in raws.items():
        collector = TextCollector()
        collector.feed(raw)
        source_texts[url] = norm(" ".join(collector.parts))

    ok, errors, warnings = validate(data, source_texts)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"dzikir-pagi : {len(pagi)} entri")
    print(f"dzikir-petang: {len(petang)} entri")
    print(f"doa         : {len(doa_entries)} entri")
    for url, count in doa_per_page:
        suffix = "" if count else " — tidak ada Arab+Latin+Arti yang bisa diekstrak di halaman ini"
        print(f"  doa {page_id(url)}: {count} entri{suffix}")
    if skipped_pagi:
        print("dzikir-pagi dilewati:", "; ".join(skipped_pagi))
    if skipped_petang:
        print("dzikir-petang dilewati:", "; ".join(skipped_petang))
    for note in doa_notes:
        print("catatan doa:", note)
    for warning in warnings:
        print("PERINGATAN:", warning)
    for error in errors:
        print("ERROR:", error)
    print(f"validasi: {'LOLOS' if ok else 'GAGAL'}")
    print(f"output: {OUTPUT}")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
