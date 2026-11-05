# Pipkinsoft C64 Emulator

A C64 emulator written in C# targeting **.NET 10** with an **SDL3** host. The
machine core is platform-independent; SDL is used only for the window,
textures, audio output, input and host timing.

## Requirements

- .NET SDK 10
- Windows (the project uses the SDL3-CS Windows native packages)
- The stock C64 ROM images in `C64Emulator/Roms/`:
  - `kernal.rom`
  - `basic.rom`
  - `char.rom`

The project copies the ROMs and the window icon to the output directory.

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
| `F5` | Save the quick state |
| `F6` | Rewind one snapshot step |
| `F7` | Load the quick state |
| `F9` | Hold for max-speed warp; release to return to normal speed |
| `F10` | Write a diagnostic dump to the working directory |
| `F11` | Switch the two joystick maps |
| `F12` | Open/close the emulator menu; **Display** sets VSync and fullscreen mode, **Audio** sets device, volume and mute |
| Arrow keys / numpad | Default joystick key sets |
| `Esc` in a menu | Return to the emulator |
| `Left` / `Backspace` in a menu | Go back one level: parent folder, previous menu, or out of a disk or tape list |
| `Up` / `Down`, `Page Up` / `Page Down` | Move through the menu |
| `Enter` in a menu | Select the highlighted item |
| `Space` on a media file | Load and start the highlighted PRG, D64, T64, TAP or CRT |

The main menu's **Hot Keys** entry lists every shortcut inside the emulator,
including the ones below.

## Save states and rewind

`F5` saves a complete machine snapshot to `savestate.c64state` beside the
executable, and `F7` loads it. The main menu also has **Save State** and
**Load State** entries. The snapshot covers CPU registers and RAM, Color RAM,
the VIC, SID, both CIAs, the IEC bus, the virtual disk/tape mounts and the
optional hardware 1541. The VIC framebuffer and the host audio queue are not
stored; they are cleared and rebuilt after a restore.

The machine pauses while a snapshot is written or read, so a state always
lands between CPU instructions. `F6` (or **Rewind** in the menu) steps back
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

The **Display** menu has a persisted VSync toggle. Fullscreen modes are listed
with their refresh rate, and modes at 50 Hz are listed first so a PAL C64
(50.125 fps) can run on a matching display cadence. The selected resolution and
refresh rate are restored on the next launch. If SDL reports no 50 Hz mode, the
menu says so; 50 Hz support depends on the monitor and graphics driver.

With VSync disabled, the host presents at the emulated 50.125 fps cadence
instead of using a fixed 10 ms loop, avoiding duplicate frame submissions.
Holding `F9` runs the machine uncapped; releasing it returns to the normal
50.125 fps cadence. The VIC and SID keep running during warp, so the picture
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
  The PAL raster counter uses the 62/63/64-cycle line pattern, raises raster
  IRQs on cycle 1 of the matching line, and reports the previous line at cycle
  0 of line 0.
- Two 6526 CIA timers, ICR behavior, TOD clock (BCD and latching), keyboard
  matrix and both joystick ports.
- SID audio: three 24-bit oscillator voices with triangle, sawtooth, pulse,
  noise and combined waveforms; ADSR envelopes; ring modulation and sync;
  shared resonant low/band/high-pass filter; 48 kHz mono SDL audio output
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
- SDL3 window, integer scaling, VSync control, refresh-rate-aware fullscreen
  modes (including 50 Hz PAL modes), embedded C64 font, FPS/status overlay and a
  text menu.
- Host/client screen and input transport on port `6464`.

The SID is clocked by the same PAL CPU cycles as the rest of the machine, and
its bounded sample queue is drained on the SDL host thread. Audio continues
during warp and is muted only while a network client is displaying the host
screen.

## Known gaps and intentional limits

These are deliberately not presented as complete:

- The SID is a digital approximation. The 6581's nonlinear filter, exact
  envelope counter quirks and all combined-waveform differences between the
  6581 and 8580 are not modeled.
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
- The VIC follows the PAL 6569 access table: the 40 c-accesses (video matrix
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
- PAL timing is used (`985248 Hz`, 50.125 Hz frames); NTSC is not selectable.
- Save states are versioned to the current snapshot format and are rejected by
  other build versions. Rewind history is memory-only and is cleared by a
  reset, program load or media change. Disk, tape and cartridge images are
  restored by path, so a moved or deleted image cannot be reattached.
