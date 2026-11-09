# C64 ROM images (not included)

The KERNAL, BASIC and character ROMs and the 1541 DOS ROM are copyrighted works
owned by Commodore/Cloanto. They are **not** part of this repository: the `.rom`
files are ignored by Git (see `C64Emulator/Roms/*.rom` in the root
`.gitignore`), so this folder is empty in a fresh clone.

Add your own legally obtained copies here, named exactly as listed, before
building or running the emulator.

| File | Size | Contents | Needed |
| --- | --- | --- | --- |
| `kernal.rom` | 8192 bytes | KERNAL ROM, `$E000-$FFFF` | Required |
| `basic.rom` | 8192 bytes | BASIC ROM, `$A000-$BFFF` | Required |
| `char.rom` | 4096 bytes | Character generator ROM, `$D000-$DFFF` | Required |
| `dos1541.rom` | 16384 bytes | 1541 DOS ROM | Only for **True Drive Emulation** |

## How the files are used

- The emulator project copies `Roms/*.rom` next to the executable at build time,
  and the host loads them from its own directory at startup.
- Headless runs exit with code `4` when the ROMs are missing or have the wrong
  size.
- The test project reads the ROMs from this folder; tests that need real ROM
  content are skipped when the files are absent.

## Please do not commit them

Do not `git add -f` these files and do not paste ROM dumps into issues or pull
requests. They remain a local, per-machine prerequisite.
