# Pipkinsoft C64 Emulator

A C64 emulator written in C# targeting **.NET 10** with an **SDL3** host. The
machine core is platform-independent; SDL is used only for the window,
textures, audio output, input and host timing.

## Requirements

- .NET SDK 10
- Windows (the project uses the SDL3-CS Windows native packages)
- The stock C64 ROM images in `C64Emulator/Roms/` (see below)

### ROM images are not included

The C64 KERNAL, BASIC and character ROMs and the 1541 DOS ROM are copyrighted
by Commodore/Cloanto, so they are **not** distributed with this repository and
are ignored by Git. Add your own legally obtained copies to
`C64Emulator/Roms/` before building or running:

| File | Size | Contents |
| --- | --- | --- |
| `kernal.rom` | 8192 bytes | KERNAL, `$E000-$FFFF` (required) |
| `basic.rom` | 8192 bytes | BASIC, `$A000-$BFFF` (required) |
| `char.rom` | 4096 bytes | Character generator, `$D000-$DFFF` (required) |
| `dos1541.rom` | 16384 bytes | 1541 DOS ROM, only for True Drive Emulation |

The files are not downloaded automatically. `C64Emulator/Roms/README.md` has
the same list with more detail. The project copies the ROMs and the window icon
to the output directory.

## Build and run

```bash
dotnet build C64Emulator.sln
dotnet run --project C64Emulator/C64Emulator.csproj
```

Tests:

```bash
dotnet test C64Emulator.Tests/C64Emulator.Tests.csproj
```

The boot test runs the real KERNAL/BASIC ROMs and waits for the `READY.` prompt.
It is skipped gracefully when the ROM files are not present.

## Controls

| Input | Action |
| --- | --- |
| `Alt+F5` | Save the quick state |
| `Alt+F6` | Rewind one snapshot step |
| `Alt+F7` | Load the quick state |
| `F9` | Hold for max-speed warp; release to return to normal speed |
| `F10` | Write a diagnostic dump to the working directory |
| `F11` | Switch the two joystick maps |
| `F12` | Open/close the emulator menu; **Display** sets VSync, scanlines and fullscreen mode, **Audio** sets device, volume and mute |
| Arrow keys / numpad | Default joystick key sets |
| `Esc` in a menu | Return to the emulator |
| `Left` / `Backspace` in a menu | Go back one level: parent folder, previous menu, or out of a disk or tape list |
| `Up` / `Down`, `Page Up` / `Page Down` | Move through the menu; hold Up/Down to repeat |
| `Enter` in a menu | Select the highlighted item |
| `Space` on a media file | Load and start the highlighted PRG, D64, T64, TAP or CRT |

The main menu's **Hot Keys** entry lists every shortcut inside the emulator,
including the ones below.

## Save states and rewind

`Alt+F5` saves a complete machine snapshot to `savestate.c64state` beside the
executable, and `Alt+F7` loads it. The main menu also has **Save State** and
**Load State** entries. Plain `F5`-`F7` are passed through to the emulated
C64 keyboard. The snapshot covers CPU registers and RAM, Color RAM,
the VIC, SID, both CIAs, the IEC bus, the virtual disk/tape mounts and the
optional hardware 1541. The VIC framebuffer and the host audio queue are not
stored; they are cleared and rebuilt after a restore.

The machine pauses while a snapshot is written or read, so a state always
lands between CPU instructions. `Alt+F6` (or **Rewind** in the menu) steps back
through an in-memory ring of snapshots taken every six frames while running at
normal speed. The buffer holds 60 snapshots, roughly seven seconds of history.
It is cleared by a reset, a program load, a true-drive toggle or a media mount
eject. Mounted disk, tape and cartridge images are recorded by path, so those
files must still exist when a saved state is loaded.

Snapshots have a format version. A state written by a different build version
is rejected with an error instead of being loaded into incompatible chip state.

Controller 1 and controller 2 are mapped through the **Input** menu. Physical
SDL joysticks and the configured key sets both feed the same CIA lines.

The selected playback device, volume and mute state are stored in the same
settings file as the display, input and network preferences, so they survive a
restart. Device selection uses the SDL playback-device list and falls back to
the system default if the saved device is no longer present.

The **Display** menu has persisted VSync and scanline toggles and explicit
**PAL (50 Hz)** and **NTSC (60 Hz)** video standard choices. PAL is the
default; the active standard is marked `[X]`, and selecting the other choice
resets the machine and switches timing immediately.
Scanlines are off by default and only affect the host presentation; they do
not change the VIC frame, save states, screenshots or network frames. Menus
longer than the visible rows show a proportional scrollbar inside the right
edge of the display area, with long labels clipped before it. Fullscreen modes
are listed with their refresh rate, and modes matching the active standard are
listed first: 50 Hz for PAL (50.125 fps) and 60 Hz for NTSC (59.826 fps). The
selected resolution and refresh rate are restored on the next launch. PAL and
NTSC each remember their own windowed size, so switching back to a standard
restores the window it was last shown in; a **Windowed (last size)** entry
returns to that size directly. The presets use the C64's 4:3 presentation:
377 x 284 in PAL and 314 x 235 in NTSC at 1x. Fullscreen keeps that aspect,
so a widescreen display shows side bars instead of a stretched picture.
If SDL reports no matching mode, the menu says so; that support depends on
the monitor and graphics driver.

With VSync disabled, the host presents at the emulated frame cadence of the
active standard instead of using a fixed 10 ms loop, avoiding duplicate frame
submissions. Holding `F9` runs the machine uncapped; releasing it returns to
the standard's normal cadence. The VIC and SID keep running during warp, so the picture
and audio remain live while the CPU and 1541 use the host's full speed. The
host redraws at 30 Hz while warping and flushes the audio buffer when warp is
released.

## Loading programs, disks, tapes and cartridges

1. Press `F12`, choose **Load Program / Media**, and navigate to a `.prg`,
   `.d64`, `.t64`, `.tap` or `.crt` file.
2. A `.prg` is loaded and run immediately. A `.d64`, `.t64` or `.tap` is mounted
   and opens its list of programs; select one to load and run it. A `.crt` is
   mounted, the machine is reset, and the cartridge's autostart runs.
3. The browser reopens in the folder it showed last and remembers it across
   restarts. With a PRG, D64, T64, TAP or CRT highlighted, `Space` launches it
   without opening the disk or tape list: a PRG loads and runs, a D64 mounts
   and types `LOAD"*",8,1` followed by `RUN`, a T64 mounts and types
   `LOAD"*",1,1`, a TAP mounts and types `LOAD"",1,1` so the KERNAL reads the
   next pulse block, and a CRT mounts and starts its autostart. Both tape
   commands are followed by `RUN`.
   The bottom of the menu shows that shortcut while a media file is selected.
4. A mounted disk or tape stays available from the main menu under **Load
   Program / Media**, where it can be browsed or ejected. A mounted cartridge
   can be ejected from the same file browser.

### Disks (D64)

Selecting a D64 mounts it and lists every closed PRG in its directory. Standard
35- and 40-track D64 images are accepted; 40-track images must be at least
196,608 bytes. After mounting, BASIC/KERNAL loads such as `LOAD"*",8,1` and
`LOAD"NAME",8,1` are serviced from the image. KERNAL `OPEN`/`CHKIN`/`CHRIN`
channels can read file data, and `OPEN"$",8` returns a directory stream.

Loads use the virtual KERNAL disk path by default and complete immediately.
**True Drive Emulation** in the menu switches to the cycle-level 1541/IEC
path. That path is experimental: it runs the DOS, both VIAs and the GCR disk,
and standard multi-sector PRG loads complete at real 1541 speed. Hold `F9` to
skip long 1541 load periods; release it to return to normal speed. There is no
automatic drive-motor warp. The screen updates slowly while warping, but the
drive protocol continues normally.

### Tapes (T64 and TAP)

T64 is a tape container, not a pulse-level tape image. Selecting a T64 mounts it
and lists its normal PRG entries. Direct menu launches and KERNAL `LOAD` calls
using device 1 (`LOAD"*",1` or `LOAD"NAME",1`) are serviced from the mounted
image. KERNAL `OPEN`/`CHKIN`/`CHRIN` channels can read file data, and
`OPEN"$",1` returns a directory stream. The parser tolerates the common bad
end-address metadata in existing T64 files by using payload boundaries where
necessary.

TAP stores the raw datasette pulse stream. A mounted TAP drives the datasette
READ line into CIA1's active-low FLAG input, and the processor port exposes the
motor and SENSE lines. Device-1 KERNAL loads therefore use the real ROM tape
routines rather than the virtual tape-file path. This is what lets a standard
KERNAL boot file start a custom turbo loader and lets that loader read its own
pulses. The parser still decodes standard CBM blocks for the menu, so normal
TAP files can be listed as well; selecting a listed file uses the convenience
virtual loader, while `Space` (or `LOAD"",1,1` from BASIC) plays the tape at
pulse level. `LOAD"*",1` is translated to the unnamed load on a TAP because
the stock KERNAL tape search treats `*` literally. TAP v0 and v1 are supported. TAP v2, recording to tape, and the
cassette write line are not implemented.

### Cartridges (CRT)

Standard type-0 8K and 16K CRT images are supported. The cartridge ROM is mapped
at `$8000-$9FFF` (and `$A000-$BFFF` for a 16K image), where the KERNAL autostart
signature is checked after reset. Banked, flash, EasyFlash and Action Replay
cartridges need their own banking hardware and are rejected rather than loaded
incorrectly.

Disk access uses the KERNAL jump-table level by default rather than emulating
the IEC serial bus; TAP access uses the datasette hardware path described
above. BASIC and KERNAL loaders therefore work, including games that load
additional files after startup.
The virtual disk supports KERNAL `SAVE` of PRG files into a mounted D64 when
it has a writable image path; existing files are not overwritten, and the
new directory entry and sector chain are persisted immediately. Tape devices
remain read-only, and drive command channels, write channels and `LOAD"$",8`
are not implemented. (`OPEN"$",8`/`OPEN"$",1` can read a directory stream.)
Programs that install their own fastloader or bit-bang the 1541 hardware will
not see the virtual disk and will fail. The optional **True Drive Emulation**
setting runs the 1541 DOS at cycle level over IEC; it is experimental and not
required for normal D64 loading. GCR-level protection, custom disk formats and
disk error maps are not modeled.

## Project layout

The emulator project is split by feature, with one namespace per folder.

| Folder | Namespace | Contents |
| --- | --- | --- |
| `Core/` | `C64Emulator.Core` | `C64Machine` composition root, memory/PLA, address map, color RAM, ROM images and interrupt line |
| `Cpu/` | `C64Emulator.Cpu` | 6510 core and the cycle-clocked CPU bus |
| `Video/` | `C64Emulator.Video` | VIC-II renderer, C64 palette and colour type |
| `Audio/` | `C64Emulator.Audio` | SID synthesis and the optional sound trace |
| `Devices/` | `C64Emulator.Devices` | CIAs, VIA, keyboard matrix, IEC bus, 1541 hardware, disk/tape devices and cartridges |
| `Media/` | `C64Emulator.Media` | D64/T64/TAP image parsing, GCR codec, PETSCII helpers and filename matching |
| `Host/` | `C64Emulator.Host` | SDL window, renderer, menu, audio output and persisted settings |
| `Input/` | `C64Emulator.Input` | SDL key mapping types shared by the host and the CIAs |
| `Networking/` | `C64Emulator.Networking` | host/client protocol and framed TCP transport |

`Program.cs` stays at the project root and only starts the SDL host.

## Architecture

```
Program
  └─ EmuWin (SDL host, input, audio, networking, menu)
       ├─ Renderer (SDL textures, overlays, FPS)
       ├─ AudioOutput (selectable SDL playback device, Sid sample queue -> audio stream)
       ├─ Network / NetworkTransport (host/client image + input transport)
       └─ C64Machine (composition root + PAL clock)
            ├─ Cpu6510  ── IMemoryBus
            ├─ Memory   ── PLA, RAM, ROM overlays, I/O decode
            ├─ Vic      ── IVideoBus, ColorRam, InterruptLine
            ├─ Cia1 / Cia2 ── timers, TOD, ports, keyboard/joystick lines
            ├─ Sid      ── 3 voices, ADSR, ring/sync, resonant filter, 48 kHz mono queue
            ├─ DiskDrive ── D64 directory, file chains and KERNAL LOAD/SAVE bridge
            ├─ TapeDrive ── T64/TAP directory, program payloads and KERNAL LOAD bridge
            ├─ Cartridge ── standard 8K/16K CRT image and ROML/ROMH overlay
            └─ IoController ── $D000-$DFFF chip mirrors
```

Important boundaries:

- `Cpu6510.Step()` executes exactly one instruction and returns its cycle
  count. It does not own a thread, wall-clock pacing, or device tick calls.
- `C64Machine` owns the emulation thread and advances CIA timers, the SID and
  the VIC by the cycles the CPU consumed.
- `InterruptLine` is a wired-OR of VIC/CIA IRQ sources. Devices assert and
  release their source; the CPU samples the line at instruction boundaries.
  `CLI`/`SEI`/`PLP`/`RTI` delay IRQ recognition by one instruction, because the
  flag update lands after the 6502 has polled the line. NMI is edge-triggered
  and latches one edge until the CPU services it.
- `Vic` renders into a back buffer and swaps it into the front buffer at the end
  of a frame. `Renderer` copies the front buffer on the main thread, so SDL is
  never called from the emulation thread.
- `Sid` synthesis runs on the emulation thread and only appends to a bounded
  sample queue. `AudioOutput` drains that queue and calls SDL on the host thread.
- `Memory` keeps RAM underneath the ROM/I/O overlays. Writes while a ROM is
  banked in go to RAM; the PLA's LORAM/HIRAM/CHAREN bits are decoded exactly
  for the read path.

## Implemented hardware

- 6502/6510 instruction set including the common undocumented opcodes, decimal
  mode with NMOS flag behavior, page-crossing/pointer-wrap timing, NMI/IRQ and
  JAM handling.
- 64 KB memory map with KERNAL/BASIC/character ROM overlays and I/O mirroring.
  Addresses that no chip decodes return the last value driven on the data bus
  (open bus), including the $DE00-$DFFF cartridge windows, which can be handed
  to an expansion-port device.
- VIC-II text, multicolor text, extended-color text, standard bitmap and
  multicolor bitmap modes; sprites (including expansion, priority and
  collisions), raster compare/IRQ, border/background colors, X/Y hardware
  scroll, 38/40-column and 24/25-row geometry, cycle-staggered video-matrix,
  Color RAM and character/bitmap fetches, and BA/AEC bus arbitration with a
  three-cycle lead-in for badline and sprite fetches. CPU writes latch at the
  end of their bus cycle, and the full 25-row matrix is fetched even in 24-row
  mode, so fine scroll can expose the start of row 24 above the bottom border.
  VCBASE advances when an eight-scanline character row completes rather than on
  every badline, so raster splits that place badlines closer than eight lines
  keep the current row instead of jumping to an adjacent matrix row.
  The PAL raster counter uses the 62/63/64-cycle line pattern, while NTSC uses
  65-cycle lines throughout; both raise raster IRQs on cycle 1 of the matching
  line and report the previous line at cycle 0 of line 0.
- Two 6526 CIA timers, ICR behavior, TOD clock (BCD and latching; seeded at
  midnight for reproducible runs, with `SeedTod` for wall-clock use), keyboard
  matrix and both joystick ports.
- SID audio: three 24-bit oscillator voices with triangle, sawtooth, pulse,
  noise and combined waveforms; cycle-clocked ADSR envelopes with the chip's
  rate-counter and exponential-divider timing; ring modulation and sync;
  shared resonant low/band/high-pass filter with selectable 6581 (saturating,
  low cutoff) and 8580 (linear, brighter) models; 48 kHz mono SDL audio output
  with a persisted master volume and mute setting.
- Virtual 1541 drive for standard D64 images: directory parsing, PRG file
  chains, mount/eject, direct PRG launch, KERNAL `LOAD`/`OPEN` channel support
  and PRG `SAVE` with immediate D64 write-back. An optional cycle-level 1541
  (6502, DOS ROM, two VIAs and a GCR disk) is available through True Drive
  Emulation.
- Virtual tape drive for T64 containers and standard CBM blocks in TAP images:
  entry parsing, mount/eject, direct PRG launch, and device-1 KERNAL
  `LOAD`/`OPEN` channel support.
- Versioned save states and a rewind ring buffer covering the CPU, RAM, VIC,
  SID, CIAs, IEC bus and the optional cycle-level 1541, with quick-save/load
  hot keys and menu entries.
- Standard 8K/16K CRT cartridges, including the `$8000`/`$A000` ROM overlay and
  a menu-driven mount/eject path.
- SDL3 window, aspect-correct scaling, VSync control, optional scanlines,
  refresh-rate-aware fullscreen modes (including 50 Hz PAL modes), embedded
  C64 font, FPS/status overlay and a text menu.
- Host/client screen and input transport on port `6464`.

The SID is clocked by the same PAL CPU cycles as the rest of the machine, and
its bounded sample queue is drained on the SDL host thread. Audio continues
during warp and is muted only while a network client is displaying the host
screen.

## Command line

With no arguments the emulator opens its window as before. Any arguments
select a synchronous headless run, which is useful for scripts and accuracy
checks:

```bash
C64Emulator --help
C64Emulator --headless --cycles 20000000
C64Emulator game.d64 --cycles 20000000
C64Emulator program.prg --cycles 500000
C64Emulator --headless --cycles 3000000 --screenshot ready.bmp
C64Emulator --headless --cycles 3000000 --video-standard ntsc --screenshot ntsc.bmp
C64Emulator --headless --cycles 3000000 --expect-frame ready.bmp --tolerance 8
C64Emulator game.d64 --cycles 20000000 --frames captures --every 250
C64Emulator --suite regression.json
C64Emulator --suite regression.json --record-missing
C64Emulator --suite regression.json --record-all
C64Emulator --suite regression.json --record-all --case ready
C64Emulator --suite regression.json --list-cases
C64Emulator --suite regression.json --list-cases --json
C64Emulator --suite regression.json --list-cases --json --relative-paths
C64Emulator --suite regression.json --list-cases --json --check-files
```

`--cycles` stops at the first instruction boundary at or after the requested
count, so a run is reproducible. A media path implies headless mode and, when
`--cycles` is omitted, runs for ten emulated seconds. Disks and tapes boot to
the BASIC prompt first and then get the same `LOAD"*",8,1` / `RUN` keystrokes
as the file menu's Space action, so boot loaders behave normally. PRGs are
injected and cartridges are mounted with a reset. Frame composition is skipped
headless unless a picture is requested, but sprite collisions and all chip
timing still run.

`--screenshot F` writes the last completed frame at the end of the run.
`--frames DIR` writes completed frames as `frame_000001.bmp`, `frame_000002.bmp`
and so on, saving one frame out of every `--every N` (default 25). Both use
uncompressed 24-bit BMP, so no image codec is needed and the files are
deterministic for a given media file and cycle count.

`--video-standard pal|ntsc` selects the VIC model for a standalone run. PAL is
the default; NTSC changes the clocks and produces 418x235 frames instead of
403x284, including screenshots, frame dumps and `--expect-frame` references.
It cannot be combined with `--suite`; use each case's `videoStandard` field
instead.

`--expect-frame F` compares the last completed frame, pixel for pixel, with
the BMP at F and exits `6` on any difference. `--tolerance N` (0-255,
default 0) allows up to N per-channel difference before a pixel counts as
changed, which absorbs small palette or timing shifts between builds. On a
mismatch the report includes the changed bounding box and the first changed
pixel. References are ordinary 24-bit BMPs, such as those written by
`--screenshot`, and must have the same dimensions. A reference that cannot be
read exits `7`.

`--suite F` runs a JSON list of checkpoints in one process, each with a fresh
machine and its own run budget. Paths are resolved relative to the suite file:

```json
{
  "cases": [
    {
      "name": "ready",
      "cycles": 3000000,
      "reference": "ready.bmp",
      "tolerance": 8
    },
    {
      "name": "defender-boot",
      "media": "defender.d64",
      "cycles": 20000000,
      "reference": "defender.bmp",
      "videoStandard": "ntsc"
    }
  ]
}
```

`cycles` and `reference` are required; `media` is optional and is started
exactly like a command-line media path; `tolerance` defaults to 0 and
`videoStandard` is `pal` (the default) or `ntsc`. The standard selects both the
emulated timing and the reference size: PAL frames are 403x284 and NTSC frames
are 418x235. Comments and trailing commas are accepted. Each case prints `PASS`, `RECORD` or `FAIL` with
the reason, followed by one `c64: suite: N passed, M failed` line. The suite
exits `0` only when every case passes, `6` when any case fails (including a
missing media or reference file), and `2` when the suite file is missing or
invalid.

Before any case runs, the suite checks the media and reference files for every
selected case. Missing files, unrecognized or malformed media, unreadable or
malformed reference BMPs, and references whose dimensions differ from the
expected frame for the case's video standard (403x284 PAL or 418x235 NTSC)
are all reported as `FAIL` lines; the run then stops without starting the
emulator and exits `6`. Media is parsed with the same
D64, T64, TAP and CRT loaders used for execution, and a PRG must contain a load
address. `--record-missing` validates references that already exist but skips
missing ones it will write. `--record-all` skips all reference checks.

With `--record-missing`, a case whose `reference` file does not exist runs
once and writes its final frame to that path, reported as `RECORD`; existing
references are still compared. `--record-all` is the force mode: it rewrites
every reference, including existing ones. Both modes write through a temporary
file and replace the reference only after a successful run, so a failed or
interrupted case leaves the previous baseline untouched. The summary adds a
recorded count (`N passed, R recorded, M failed`), and recorded cases count as
success, so a new or refreshed checkpoint needs one command.

`--case NAME` limits a suite run to the named cases; repeat it to select
several. Matching ignores case and the suite's order is kept. Use it alone to
check one screen, or with `--record-all` to refresh one baseline. An unknown
name exits `2` before any case runs.

`--list-cases` prints the suite's case names in manifest order and exits
without running the emulator. It accepts `--case` to list a subset and rejects
`--record-missing`/`--record-all`. The output is one name per line with no
header, so scripts can read it directly.

Add `--json` to print an array of case objects instead of names. Each object
has `name`, `cycles`, `media`, `reference`, `tolerance` and `videoStandard`;
paths are resolved to absolute paths and `media` is null for cases without
media. `--json` requires `--list-cases` and cannot be combined with recording
flags.

Add `--relative-paths` to print media and reference paths relative to the
suite file with `/` separators instead of absolute paths. It requires `--json`.
This keeps listings portable between machines and CI workspaces.

Add `--check-files` to include `mediaExists` and `referenceExists` booleans
and to exit `6` when any selected case has a missing media or reference file.
`mediaExists` is null for cases without media. It requires `--json`, and the
JSON is printed before the failure code.

Exit codes are stable: `0` success, `1` runtime failure, `2` invalid command
line, `3` media could not be started, `4` ROMs missing or invalid, `5` the
BASIC prompt was not reached, `6` a frame check, suite case or file preflight
failed, and `7` the reference frame could not be read.

## Known gaps and intentional limits

These are deliberately not presented as complete:

- The SID is a signal-level approximation, not a reSID port. The envelope uses
  the real rate-counter periods, exponential divider thresholds and hold-zero
  behavior (including the skipped rate-counter zero), but the one-cycle
  pipelining of register writes is not modeled. The 6581/8580 filters are SVF
  approximations with per-model cutoff curves and 6581 input saturation, not a
  transistor-level ladder. Combined waveforms are an AND of the digital outputs
  with per-model attenuation rather than measured waveform tables.
- D64 loads are virtual by default: standard KERNAL/BASIC `LOAD` calls are
  serviced directly from the image. The optional True Drive Emulation path
  runs the 1541 DOS, two VIAs and GCR disk over IEC. Standard multi-sector PRG
  loads complete at real 1541 speed, so it remains off by default. Custom
  fastloaders, GCR-level copy protection and direct IEC bit-banging are not
  supported.
- T64 access is a virtual tape, not datasette pulse emulation. Normal T64
  entries, KERNAL device-1 loads and read-only file channels work, but turbo
  loaders and games that manipulate the datasette lines directly are
  unsupported. TAP support decodes standard CBM blocks only; custom turbo
  pulse streams are rejected, and TAP recording/writing is not implemented.
- The virtual disk can save PRG files through the KERNAL path, but it does not
  implement overwrite, drive command channels, write channels or `LOAD"$",8`
  directory loads; use the menu or `OPEN"$",8` for a directory. The hardware
  1541 has a GCR write path for sector data, but it is not verified end to end.
- Only standard 8K/16K CRT cartridges are supported. Banked cartridges,
  EasyFlash, Action Replay and REU expansion are not emulated.
- The CPU issues every cycle in hardware order: opcode and operand fetches,
  the unfixed dummy reads of indexed addressing, the read-modify-write dummy
  write and the internal cycles of branches, stack and interrupt sequences.
  Each one advances the machine one cycle, and BA/AEC is checked at the access
  itself, so contention lands on the real cycle inside an instruction rather
  than at its boundary. The SID is clocked per cycle for the same reason.
- The VIC follows the 6569/6567 access table: the 40 c-accesses (video matrix
  and Color RAM) run on cycles 15-54 in the VIC's second phase, the matching
  g-accesses follow on 16-55, and the fetched byte reaches the screen twelve
  pixels later from the shift-register stage. Sprite pointers and the three
  bytes of a sprite row are fetched in the sprite bus slots and latched for
  the next raster line instead of being read from memory while drawing, and
  the border is a per-cycle flip-flop that opens at the display window edge.
  Still approximate: refresh and idle accesses are not exposed on the bus, and
  sprite collision timing follows the pixel pipeline rather than the sprite
  output cycle.
- The less common illegal 6502 opcodes are implemented approximately, and the
  unstable opcodes are not cycle-accurate.
- The network protocol is the emulator's own host/client protocol, not a
  general remote-desktop protocol. It is plain TCP and has no authentication.
- PAL is the default timing (`985248 Hz`, 50.125 Hz frames, 312 raster lines
  of 63 cycles). The Display menu switches to NTSC (`1022727 Hz`, 59.826 Hz
  frames, 263 raster lines of 65 cycles); the VIC geometry, SID clock, CIA
  time-of-day rate and 1541 speed ratio follow the selected standard, and the
  machine resets so the KERNAL redetects the frame rate at `$02A6`. The
  native VIC frames are 403 x 284 in PAL and 418 x 235 in NTSC, and the
  frame buffer, texture and network frames follow that size. The pixels are
  not square, so the host presents the border-to-border frame at 4:3:
  377 x 284 in PAL and 314 x 235 in NTSC. Windowed presets are whole-number
  zooms of that aspect-corrected frame, so the picture fills the window
  without letterbox bars. Fullscreen preserves 4:3; Fill Height is the
  default and uses the display height, leaving side bars on wider screens.
  Fit shows the complete frame, and Fill Height crops the sides only on
  displays narrower than 4:3. The active standard also selects its colour
  table: PAL keeps the existing approximation, while NTSC uses Pepto's Sony
  CXA2025AS NTSC palette. Colour RAM and network frames still carry 4-bit
  indices, so the palette remains a host-side presentation choice.
- Save states are versioned to the current snapshot format and are rejected by
  other build versions. Rewind history is memory-only and is cleared by a
  reset, program load or media change. Disk, tape and cartridge images are
  restored by path, so a moved or deleted image cannot be reattached.
