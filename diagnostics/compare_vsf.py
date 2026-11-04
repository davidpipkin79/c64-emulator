"""Read a VICE .vsf snapshot and compare its C64 state with an emulator dump.

Usage: python compare_vsf.py <snapshot.vsf> [emulator-dump.bin]

The VSF container is a short signature followed by named modules, each with a
16-byte NUL-padded name and a little-endian 32-bit payload length.
"""

import struct
import sys

SIGNATURE = b"VICE Snapshot File\x1a\x1a"


def read_modules(path):
    data = open(path, "rb").read()
    if not data.startswith(SIGNATURE):
        raise SystemExit(f"{path}: not a VICE snapshot")

    modules = {}
    offset = len(SIGNATURE)

    while offset + 20 <= len(data):
        name = data[offset:offset + 16].split(b"\x00")[0].decode("latin1")
        (length,) = struct.unpack_from("<I", data, offset + 16)
        offset += 20

        if name == "" or length == 0 or offset + length > len(data):
            break

        modules[name] = data[offset:offset + length]
        offset += length

    return modules


def describe(modules):
    for name in sorted(modules):
        print(f"  {name:<12} {len(modules[name]):7d} bytes")


def peek_ram(modules):
    """VICE stores main RAM in the C64MEM module; some builds name it C64MEMSC."""
    for candidate in ("C64MEM", "C64MEMSC", "RAM"):
        if candidate in modules:
            return modules[candidate]
    raise SystemExit("no RAM module found: " + ", ".join(sorted(modules)))


def hexdump(ram, label, start, end):
    print(f"\n{label} ${start:04X}-${end - 1:04X}")
    for base in range(start, end, 16):
        chunk = ram[base:min(base + 16, end)]
        print(f"  {base:04X}: {chunk.hex(' ')}")


def compare(ram, dump):
    print("\nmismatching 256-byte pages (vice vs dump):")
    rows = []
    for page in range(256):
        v = ram[page * 256:(page + 1) * 256]
        d = dump[page * 256:(page + 1) * 256]
        if v != d:
            diff = sum(1 for a, b in zip(v, d) if a != b)
            rows.append((diff, page))

    for diff, page in sorted(rows, reverse=True)[:24]:
        print(f"  ${page:02X}00  {diff:3d} differing bytes")


def main():
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)

    modules = read_modules(sys.argv[1])
    print("modules:")
    describe(modules)

    ram = peek_ram(modules)

    print("\nstate at snapshot:")
    print(f"  $0001 processor port = ${ram[1]:02X}")
    print(f"  $FFFE IRQ vector     = ${ram[0xFFFE] | (ram[0xFFFF] << 8):04X}")
    print(f"  $FFFA NMI vector     = ${ram[0xFFFA] | (ram[0xFFFB] << 8):04X}")
    print(f"  $4008 music flag     = ${ram[0x4008]:02X}")
    print(f"  $87A6/$87A7 TB latch = ${ram[0x87A7]:02X}{ram[0x87A6]:02X}")
    print(f"  $0020-$0029 queue    = {' '.join(f'{b:02X}' for b in ram[0x20:0x2A])}")

    hexdump(ram, "engine state", 0xA851, 0xA8B3)
    hexdump(ram, "module data", 0x87A0, 0x8810)

    if len(sys.argv) >= 3:
        dump = open(sys.argv[2], "rb").read()
        if len(dump) != 0x10000:
            print(f"\ndump is {len(dump)} bytes, expected 65536; comparing overlap")
        width = min(len(dump), len(ram))
        compare(ram[:width], dump[:width])


if __name__ == "__main__":
    main()
