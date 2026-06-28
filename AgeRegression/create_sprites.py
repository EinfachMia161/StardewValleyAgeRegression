#!/usr/bin/env python3
"""Generate valid PNG sprite sheets for Stardew Valley mod."""
import struct
import zlib

def create_png(width, height, filename):
    """Create a transparent PNG with given dimensions."""
    # PNG signature
    png = b'\x89PNG\r\n\x1a\n'
    
    # IHDR chunk
    ihdr_data = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    ihdr = b'IHDR' + ihdr_data
    png += struct.pack('>I', len(ihdr_data)) + ihdr + struct.pack('>I', zlib.crc32(ihdr))
    
    # IDAT chunk - transparent pixels
    raw_data = b''
    for y in range(height):
        raw_data += b'\x00'  # filter type: none
        raw_data += b'\x00\x00\x00\x00' * width  # RGBA: transparent
    
    compressed = zlib.compress(raw_data, 9)
    idat = b'IDAT' + compressed
    png += struct.pack('>I', len(compressed)) + idat + struct.pack('>I', zlib.crc32(idat))
    
    # IEND chunk
    iend = b'IEND'
    png += struct.pack('>I', 0) + iend + struct.pack('>I', zlib.crc32(iend))
    
    with open(filename, 'wb') as f:
        f.write(png)
    print(f'Created {filename}: {width}x{height} ({len(png)} bytes)')

# Create sprite sheets
create_png(48, 16, 'assets/sprites/diapers.png')      # 3 sprites at 16x16
create_png(80, 16, 'assets/sprites/accessories.png')  # 5 sprites at 16x16
create_png(64, 16, 'assets/sprites/furniture.png')    # 4 sprites at 16x16
