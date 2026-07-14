#!/usr/bin/env python3
"""Generate valid PNG sprite sheets for the Age Regression mod.

Standard (see ``AgeRegression.UI.HudRenderer`` / ``SpriteReference``):
  * Sprites are 64x64 pixels.
  * Sheets are 13 columns wide.
  * A sprite's source rectangle is
    ``((index % 13) * 64, (index / 13) * 64)``.
  * Each wardrobe category has its own sheet; indices start at 0.

This script scaffolds transparent placeholder sheets so the mod loads
before real art is drawn. Replace the generated PNGs with authored art
at the same dimensions (832xN, 64x64 sprites, 13 columns).
"""
import struct
import zlib

SPRITE_DIM = 64
COLUMNS = 13


def create_png(sprite_count, filename):
    """Create a transparent 64x64 / 13-column sheet holding ``sprite_count`` slots."""
    width = COLUMNS * SPRITE_DIM
    rows = max(1, (sprite_count + COLUMNS - 1) // COLUMNS)
    height = rows * SPRITE_DIM

    # PNG signature
    png = b"\x89PNG\r\n\x1a\n"

    # IHDR chunk
    ihdr_data = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    ihdr = b"IHDR" + ihdr_data
    png += struct.pack(">I", len(ihdr_data)) + ihdr + struct.pack(">I", zlib.crc32(ihdr))

    # IDAT chunk - fully transparent pixels
    raw_data = b""
    for _ in range(height):
        raw_data += b"\x00"  # filter type: none
        raw_data += b"\x00\x00\x00\x00" * width  # RGBA: transparent
    compressed = zlib.compress(raw_data, 9)
    idat = b"IDAT" + compressed
    png += struct.pack(">I", len(compressed)) + idat + struct.pack(">I", zlib.crc32(idat))

    # IEND chunk
    iend = b"IEND"
    png += struct.pack(">I", 0) + iend + struct.pack(">I", zlib.crc32(iend))

    with open(filename, "wb") as f:
        f.write(png)
    print(f"Created {filename}: {width}x{height} ({len(png)} bytes, {sprite_count} slots)")


# One sheet per wardrobe category. Indices start at 0 independently per sheet.
create_png(8, "assets/sprites/diapers.png")  # diaper types
create_png(5, "assets/sprites/accessories.png")  # pacifier, bottle, mittens, bib, plushie
create_png(1, "assets/sprites/onesies.png")  # future category
create_png(4, "assets/sprites/furniture.png")  # crib, changing table, toy box, playpen
