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
| `F9` | Hold for max-speed warp; release to return to normal speed |
| `F11` | Switch the two joystick maps |
| `F12` | Open/close the emulator menu; **Display** sets VSync and fullscreen mode, **Audio** sets device, volume and mute |
| Arrow keys / numpad | Default joystick key sets |
| `Esc` in a menu | Return to the emulator |

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
   `.d64`, `.t64` or `.crt` file.
2. A `.prg` is loaded and run immediately. A `.d64` or `.t64` is mounted and
   opens its list of programs; select one to load and run it. A `.crt` is
   mounted, the machine is reset, and the cartridge's autostart runs.
3. A mounted disk or tape stays available from the main menu under **Load
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
path. That path is experimental: it completes the OPEN/TALK handshake and
starts returning file data, but long transfers can still stall. Hold `F9` to skip long 1541 load periods; release it to return to normal
speed. There is no automatic drive-motor warp. The screen updates slowly while
warping, but the drive protocol continues normally.

### Tapes (T64)

T64 is a tape container, not a pulse-level tape image. Selecting a T64 mounts it
and lists its normal PRG entries. Direct menu launches and KERNAL `LOAD` calls
using device 1 (`LOAD"*",1` or `LOAD"NAME",1`) are serviced from the mounted
image. KERNAL `OPEN`/`CHKIN`/`CHRIN` channels can read file data, and
`OPEN"$",1` returns a directory stream. The parser tolerates the common bad
end-address metadata in existing T64 files by using payload boundaries where
necessary.

T64 entry types other than normal tape files are skipped. Games that install a
turbo loader or directly bit-bang the datasette will not work; T64 does not
preserve tape timing. Use TAP for that kind of compatibility.

### Cartridges (CRT)

Standard type-0 8K and 16K CRT images are supported. The cartridge ROM is mapped
at `$8000-$9FFF` (and `$A000-$BFFF` for a 16K image), where the KERNAL autostart
signature is checked after reset. Banked, flash, EasyFlash and Action Replay
cartridges need their own banking hardware and are rejected rather than loaded
incorrectly.

Disk and tape access use the KERNAL jump-table level by default rather than
emulating the IEC serial bus or datasette hardware. BASIC and KERNAL loaders
therefore work, including games that load additional files after startup. The
virtual devices are read-only: `SAVE`, disk commands/status writes and
`LOAD"$",8` are not implemented. (`OPEN"$",8`/`OPEN"$",1` can read a
directory stream.) Programs that install their own fastloader or bit-bang the
1541/datasette hardware will not see the device and will fail. The optional
**True Drive Emulation** setting runs the 1541 DOS at cycle level over IEC;
it is experimental and not required for normal D64 loading. GCR-level
protection, custom disk formats, tape pulses and error maps are not modeled.

## Architecture

```
Program
  └─ EmuWin (SDL host, input, audio, networking, menu)
       ├─ Renderer (SDL textures, overlays, FPS)
       ├─ AudioOutput (selectable SDL playback device, SID sample queue -> audio stream)
       ├─ Network / Socket (host/client image + input transport)
       └─ C64Machine (composition root + PAL clock)
            ├─ CPU6510  ── IMemoryBus
            ├─ Memory   ── PLA, RAM, ROM overlays, I/O decode
            ├─ VIC      ── IVideoBus, ColorRam, InterruptLine
            ├─ CIA1 / CIA2 ── timers, TOD, ports, keyboard/joystick lines
            ├─ SID      ── 3 voices, ADSR, ring/sync, resonant filter, 48 kHz mono queue
            ├─ C1541Drive ── D64 directory, file chains and KERNAL LOAD bridge
            ├─ TapeDrive ── T64 directory, program payloads and KERNAL LOAD bridge
            ├─ Cartridge ── standard 8K/16K CRT image and ROML/ROMH overlay
            └─ IO       ── $D000-$DFFF chip mirrors
```

Important boundaries:

- `CPU6510.Step()` executes exactly one instruction and returns its cycle
  count. It does not own a thread, wall-clock pacing, or device tick calls.
- `C64Machine` owns the emulation thread and advances CIA timers, the SID and
  the VIC by the cycles the CPU consumed.
- `InterruptLine` is a wired-OR of VIC/CIA IRQ sources. Devices assert and
  release their source; the CPU samples the line at instruction boundaries.
- `VIC` renders into a back buffer and swaps it into the front buffer at the end
  of a frame. `Renderer` copies the front buffer on the main thread, so SDL is
  never called from the emulation thread.
- `SID` synthesis runs on the emulation thread and only appends to a bounded
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
  chains, mount/eject, direct PRG launch, and KERNAL `LOAD`/`OPEN` channel
  support. An optional cycle-level 1541 (6502, DOS ROM, two VIAs and a GCR
  disk) is available through True Drive Emulation.
- Virtual tape drive for T64 containers: entry parsing, mount/eject, direct
  PRG launch, and device-1 KERNAL `LOAD`/`OPEN` channel support.
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
  runs the 1541 DOS, two VIAs and GCR disk over IEC, but long file transfers
  can still stall, so it is off by default. Custom fastloaders, GCR-level copy
  protection and direct IEC bit-banging are not supported.
- T64 access is a virtual tape, not datasette pulse emulation. Normal T64
  entries, KERNAL device-1 loads and read-only file channels work, but turbo
  loaders and games that manipulate the datasette lines directly are
  unsupported. TAP is not read.
- The virtual disk and tape devices are read-only. `SAVE`, write channels and
  `LOAD"$",8` directory loads are not implemented; use the menu or
  `OPEN"$",8` for a directory. The hardware 1541 has a write path, but it is
  not verified end to end.
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
