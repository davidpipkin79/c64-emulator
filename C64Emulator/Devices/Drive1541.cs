using System;
using C64Emulator.Core;
using C64Emulator.Cpu;
using C64Emulator.Media;

namespace C64Emulator.Devices;

/// <summary>
/// A 1541 disk drive: a 6502 running the Commodore DOS ROM, 2 KB of RAM,
/// two 6522 VIAs, the GCR disk controller and the IEC port. The drive runs
/// on its own 1 MHz clock and is advanced by <see cref="Tick"/> from the
/// C64's bus cycle, so it responds to the host's serial routines at the
/// same time as the C64 CPU.
/// </summary>
internal sealed class Drive1541
{
    public const int RomSize = 16384;
    private const int ClockHz = 1_000_000;

    [System.NonSerialized]
    private int c64ClockHz = C64Machine.PalClockHz;

    // The host checks for the ATN acknowledge about 1 ms after asserting
    // ATN, so the acknowledge must not be released before that even when
    // the DOS is idle and switches to input immediately.
    private const int AtnaMinimumCycles = 1_500;

    private readonly byte[] rom;
    private readonly byte[] ram = new byte[0x800];
    private readonly InterruptLine interrupts = new InterruptLine();
    private readonly DriveBus bus;
    private readonly Cpu6510 cpu;
    private readonly Via6522 via1;
    private readonly Via6522 via2;
    private readonly GcrDisk disk = new GcrDisk();
    private readonly IecBus iec;

    private long accumulator;
    private long budget;
    private int pendingCycles;
    private int atnaMinimum;

    public Drive1541(byte[] rom, IecBus iec)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(iec);

        if (rom.Length != RomSize)
            throw new ArgumentException("The 1541 ROM must be 16 KB.", nameof(rom));

        var resetVector = rom[0x3FFC] | (rom[0x3FFD] << 8);

        if (resetVector < 0xC000)
            throw new ArgumentException("The image is not a 1541 DOS ROM.", nameof(rom));

        this.rom = rom;
        this.iec = iec;

        via1 = new Via6522(irq => interrupts.Set(InterruptSource.Cia1, irq));
        via2 = new Via6522(irq => interrupts.Set(InterruptSource.Cia2, irq));

        via1.PortAInput = () => 0xFF;
        via1.PortBInput = ReadIecPins;
        via1.PortBChanged = UpdateIecOutputs;

        via2.PortAInput = () => disk.ReadByte;
        via2.PortAWritten = value => disk.WriteByte(value);
        via2.PortBInput = () => disk.PortBInput;
        via2.PortBChanged = () => disk.SetPortB(via2.PortBOutput);
        via2.AcrChanged = OnAcrChanged;

        iec.AtnChanged += OnAtnChanged;

        bus = new DriveBus(ram, rom, via1, via2);
        cpu = new Cpu6510(bus, interrupts);

        Reset();
    }

    internal InterruptLine Interrupts => interrupts;

    public Cpu6510 Cpu => cpu;

    public Via6522 Via1 => via1;

    public Via6522 Via2 => via2;

    public GcrDisk Disk => disk;

    public byte[] Ram => ram;

    public bool IsMounted => disk.IsMounted;

    public void Mount(D64Image image, string? path)
    {
        disk.Mount(image, path);
    }

    public void Eject()
    {
        disk.Eject();
    }

    public void Flush()
    {
        disk.Flush();
    }

    public void Reset()
    {
        Array.Clear(ram);
        interrupts.Clear();

        via1.Reset();
        via2.Reset();

        // The 1541's reset circuit leaves these directions in place.
        via1.Write(0x02, 0x1A); // DDRB: DATA/CLK/ATNA outputs
        via1.Write(0x03, 0xFF); // DDRA
        via1.Write(0x00, 0x00); // all IEC outputs released
        via1.Write(0x0B, 0x00);

        via2.Write(0x02, 0x6F); // DDRB: stepper/motor/LED/density outputs
        via2.Write(0x03, 0x00); // DDRA: disk data input
        via2.Write(0x00, 0x00);
        via2.Write(0x0B, 0x00);

        disk.Reset();
        cpu.Reset();

        accumulator = 0;
        budget = 0;
        pendingCycles = 0;
        atnaMinimum = 0;

        UpdateIecOutputs();
    }

    /// <summary>
    /// Sets the C64 clock used to convert host cycles into the drive's own
    /// 1 MHz clock. An NTSC machine runs slightly faster, so the 1541 only
    /// keeps its real speed ratio when this follows the active standard.
    /// </summary>
    public void SetC64Clock(int hz)
    {
        if (hz > 0)
            c64ClockHz = hz;
    }

    /// <summary>
    /// Advances the drive by the number of C64 cycles just executed. The
    /// 1541's 6502 runs at a slightly different clock, so a fractional
    /// accumulator keeps the two machines in step over time.
    /// </summary>
    public void Tick(int c64Cycles)
    {
        if (c64Cycles <= 0)
            return;

        accumulator += (long)c64Cycles * ClockHz;
        var driveCycles = accumulator / c64ClockHz;

        accumulator %= c64ClockHz;
        budget += driveCycles;

        while (budget > 0)
        {
            // Advance the disk and timers by the previous instruction
            // before executing the next one. A byte that becomes ready
            // during that interval is then visible to the instruction
            // that polls for it.
            TickDevices(pendingCycles);
            pendingCycles = 0;

            var ready = disk.ConsumeByteReady();

            // Byte Ready is wired to the 6502 SO input, so it sets V on
            // every completed byte regardless of the VIA's ACR mode.
            if (ready)
                cpu.P |= 0x40;

            var executed = cpu.Step();

            if (executed <= 0)
            {
                if (cpu.IsJammed)
                {
                    budget = 0;
                    break;
                }

                executed = 1;
            }

            pendingCycles = executed;
            budget -= executed;
        }
    }

    private void TickDevices(int cycles)
    {
        if (cycles <= 0)
            return;

        disk.Tick(cycles);
        via1.Tick(cycles);
        via2.Tick(cycles);

        if (atnaMinimum > 0)
        {
            atnaMinimum -= cycles;

            if (atnaMinimum <= 0)
            {
                atnaMinimum = 0;
                UpdateIecOutputs();
            }
        }
    }

    private void OnAcrChanged(byte value)
    {
        disk.WriteMode = ((value >> 5) & 0x07) == 0x06;
    }

    private void OnAtnChanged(bool high)
    {
        // The drive's ATN input is inverted before it reaches VIA1 CA1,
        // so asserting ATN on the bus produces a positive edge at the
        // VIA. The DOS programs PCR for that positive edge and handles
        // the command in the CA1 interrupt routine.
        var ca1High = !high;

        via1.SetCa1(ca1High);

        if (ca1High)
        {
            via1.ForceInterrupt(0x02);
            atnaMinimum = AtnaMinimumCycles;
        }
        else
        {
            atnaMinimum = 0;
        }

        UpdateIecOutputs();
    }

    private byte ReadIecPins()
    {
        // The 1541 buffers all three IEC inputs through inverters: a line
        // pulled low on the bus reads as a high pin at VIA1.
        byte pins = 0x1A; // DATA/CLK outputs and ATNA read high

        if (iec.DataLow)
            pins |= 0x01;

        if (iec.ClockLow)
            pins |= 0x04;

        if (iec.AtnLow)
            pins |= 0x80;

        return pins;
    }

    private void UpdateIecOutputs()
    {
        var output = via1.PortBOutput;
        var direction = via1.DdrB;

        // PB4 switches the serial lines between output and input. While
        // it is clear the drive can drive DATA/CLK and acknowledge ATN;
        // the command handler sets it to receive the host's bytes.
        var atnaEnabled = (output & 0x10) == 0;

        var dataLow =
            (direction & 0x02) != 0 &&
            (output & 0x02) != 0;
        var clockLow =
            (direction & 0x08) != 0 &&
            (output & 0x08) != 0;

        // The gate array acknowledges ATN by pulling DATA low. It stays
        // low while the drive is still busy (lines switched to output)
        // and for a minimum period that covers the host's presence
        // check, then it releases so the host can send the command. A
        // drive that is busy therefore delays the transfer instead of
        // missing the command bytes.
        // The uploaded fast loader runs from RAM and toggles ATN on every
        // bit pair, so the automatic acknowledge must stay out of its way;
        // the DOS command path in ROM still needs it to answer the host.
        var fastLoaderActive = cpu.GetPC() < 0xC000;
        var atna =
            iec.AtnLow &&
            !fastLoaderActive &&
            (atnaEnabled || atnaMinimum > 0);

        iec.SetDrive(clockLow, dataLow, atna);
    }

    /// <summary>
    /// The drive's 6502 address space: 2 KB of RAM mirrored up to $7FFF,
    /// VIA1 at $1800, VIA2 at $1C00 (both mirrored), and the 16 KB DOS ROM
    /// at $C000-$FFFF.
    /// </summary>
    private sealed class DriveBus(
        byte[] ram,
        byte[] rom,
        Via6522 via1,
        Via6522 via2) : IMemoryBus
    {
        private readonly byte[] ram = ram;
        private readonly byte[] rom = rom;
        private readonly Via6522 via1 = via1;
        private readonly Via6522 via2 = via2;

        public byte Peek(ushort address)
        {
            if (address < 0x8000 && (address & 0x1C00) == 0x1800)
                return via1.Read(address & 0x0F);

            if (address < 0x8000 && (address & 0x1C00) == 0x1C00)
                return via2.Read(address & 0x0F);

            if (address < 0x8000)
                return ram[address & 0x07FF];

            if (address >= 0xC000)
                return rom[address - 0xC000];

            return 0xFF;
        }

        public void Poke(ushort address, byte value)
        {
            if (address < 0x8000 && (address & 0x1C00) == 0x1800)
            {
                via1.Write(address & 0x0F, value);
                return;
            }

            if (address < 0x8000 && (address & 0x1C00) == 0x1C00)
            {
                via2.Write(address & 0x0F, value);
                return;
            }

            if (address < 0x8000)
                ram[address & 0x07FF] = value;
        }

        public ushort PeekWord(ushort address)
        {
            return (ushort)(
                Peek(address) |
                (Peek((ushort)(address + 1)) << 8));
        }

        public void Idle()
        {
            // The drive's bus has no timing of its own to advance.
        }
    }
}
