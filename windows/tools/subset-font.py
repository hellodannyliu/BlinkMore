#!/usr/bin/env python3
"""Rebuild the embedded Noto Sans SC subset after UI copy changes.

Requires the full Regular and Medium OTF files. Download them from
https://github.com/notofonts/noto-cjk/releases/tag/Sans2.004
(18_NotoSansSC.zip) and point NOTO_SANS_SC_DIR at the extracted folder.
"""

from __future__ import annotations

import os
import pathlib
import sys

from fontTools.subset import Subsetter
from fontTools.ttLib import TTFont


ROOT = pathlib.Path(__file__).resolve().parents[1]
SOURCE_DIR = pathlib.Path(os.environ.get("NOTO_SANS_SC_DIR", "/tmp/fonts/sc"))
OUTPUT_DIR = ROOT / "src" / "BlinkMore.App" / "Assets" / "Fonts"
WEIGHTS = ("Regular", "Medium")


def characters() -> str:
    chars: set[str] = {chr(code) for code in range(32, 127)}
    for path in ROOT.rglob("*"):
        if path.suffix.lower() not in {".cs", ".axaml", ".md", ".txt"}:
            continue
        chars.update(path.read_text(encoding="utf-8"))
    return "".join(sorted(chars))


def subset(weight: str, text: str) -> None:
    source = SOURCE_DIR / f"NotoSansSC-{weight}.otf"
    if not source.exists():
        raise SystemExit(f"Missing {source}. Set NOTO_SANS_SC_DIR.")
    font = TTFont(source)
    subsetter = Subsetter()
    subsetter.populate(text=text)
    subsetter.subset(font)
    if weight != "Regular":
        for record in font["name"].names:
            if record.nameID == 1:
                record.string = "Noto Sans SC"
            elif record.nameID == 2:
                record.string = weight
            elif record.nameID == 4:
                record.string = f"Noto Sans SC {weight}"
            elif record.nameID == 6:
                record.string = f"NotoSansSC-{weight}"
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    destination = OUTPUT_DIR / f"NotoSansSC-{weight}.ttf"
    font.save(destination)
    print(f"wrote {destination} ({destination.stat().st_size} bytes)")


def main() -> None:
    text = characters()
    for weight in WEIGHTS:
        subset(weight, text)


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(error, file=sys.stderr)
        raise
