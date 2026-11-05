using System;
using C64Emulator.Core;

namespace C64Emulator.Devices;

/// <summary>
/// Shared 6526 CIA behavior: two interval timers, the 24-hour TOD clock,
/// the interrupt control register, and the two I/O port latches.
/// Port-specific pin wiring lives in <see cref="Cia1"/> and
/// <see cref="Cia2"/>.
/// </summary>
internal abstract class Cia6526
{
    private const int TodClockCycles = 98500;

    private readonly InterruptLine interrupts;
    private readonly InterruptSource interruptSource;
    private readonly bool useNmi;

    private int todCycles;
    private int todTenths;
    private int todSeconds;
    private int todMinutes;
    private int todHours;
    private bool todPm;
    private bool todHalted;
    private bool todLatched;
    private int latchedTenths;
    private int latchedSeconds;
    private int latchedMinutes;
    private int latchedHours;
    private bool latchedPm;

    private int timerALatch;
    private int timerACounter;
    private bool timerARunning;
    private bool timerAOneShot;

    private int timerBLatch;
    private int timerBCounter;
    private bool timerBRunning;
    private bool timerBOneShot;
    private int timerBInputMode;

    private bool timerAOutputEnable;
    private bool timerAOutputToggle;
    private bool timerAOutputState;
    private bool timerAPulse;

    private bool timerBOutputEnable;
    private bool timerBOutputToggle;
    private bool timerBOutputState;
    private bool timerBPulse;

    private byte controlA;
    private byte controlB;
    private byte interruptFlags;
    private byte interruptMask;

    protected Cia6526(
        InterruptLine interrupts,
        InterruptSource interruptSource,
        bool useNmi = false)
    {
        this.interrupts = interrupts ?? throw new ArgumentNullException(nameof(interrupts));
        this.interruptSource = interruptSource;
        this.useNmi = useNmi;
        ResetTodToNow();
    }

    protected byte PortAOutput { get; set; }

    protected byte PortADirection { get; set; }

    protected byte PortBOutput { get; set; }

    protected byte PortBDirection { get; set; }

    protected bool TimerAOutputHigh => timerAOutputState || timerAPulse;

    protected bool TimerBOutputHigh => timerBOutputState || timerBPulse;

    public virtual void Reset()
    {
        timerALatch = 0;
        timerACounter = 0;
        timerARunning = false;
        timerAOneShot = false;
        timerBLatch = 0;
        timerBCounter = 0;
        timerBRunning = false;
        timerBOneShot = false;
        timerBInputMode = 0;
        timerAOutputEnable = false;
        timerAOutputToggle = false;
        timerAOutputState = false;
        timerAPulse = false;
        timerBOutputEnable = false;
        timerBOutputToggle = false;
        timerBOutputState = false;
        timerBPulse = false;
        controlA = 0;
        controlB = 0;
        interruptFlags = 0;
        interruptMask = 0;
        todCycles = 0;
        todHalted = false;
        todLatched = false;
        PortAOutput = 0;
        PortADirection = 0;
        PortBOutput = 0;
        PortBDirection = 0;

        if (useNmi)
            interrupts.SetNmi(interruptSource, false);
        else
            interrupts.Set(interruptSource, false);
    }

    public void ProcessTimers(int cycles)
    {
        if (cycles <= 0)
            return;

        timerAPulse = false;
        timerBPulse = false;

        TickTod(cycles);

        var timerAUnderflows = ProcessTimerA(cycles);
        ProcessTimerB(cycles, timerAUnderflows);
    }

    protected byte ReadRegister(ushort address)
    {
        switch (address)
        {
            case 0x04:
                return (byte)(timerACounter & 0xFF);

            case 0x05:
                return (byte)((timerACounter >> 8) & 0xFF);

            case 0x06:
                return (byte)(timerBCounter & 0xFF);

            case 0x07:
                return (byte)((timerBCounter >> 8) & 0xFF);

            case 0x08:
            {
                // Reading tenths releases the latch set by reading hours.
                var value = ToBcd(todLatched ? latchedTenths : todTenths);
                todLatched = false;
                return value;
            }

            case 0x09:
                return ToBcd(todLatched ? latchedSeconds : todSeconds);

            case 0x0A:
                return ToBcd(todLatched ? latchedMinutes : todMinutes);

            case 0x0B:
            {
                // Reading hours latches all four TOD registers. They stay
                // latched until tenths is read, so a read sequence cannot
                // straddle a carry.
                LatchTod();
                var value = (byte)((latchedPm ? 0x80 : 0) | ToBcd(latchedHours));
                return value;
            }

            case 0x0C:
                return 0;

            case 0x0D:
                return ReadInterrupt();

            case 0x0E:
                return (byte)(controlA & ~0x10);

            case 0x0F:
                return (byte)(controlB & ~0x10);

            default:
                return 0xFF;
        }
    }

    protected void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0x04:
                timerALatch = (timerALatch & 0xFF00) | value;
                if (!timerARunning)
                    timerACounter = timerALatch;
                break;

            case 0x05:
                timerALatch = (timerALatch & 0x00FF) | (value << 8);
                if (!timerARunning)
                    timerACounter = timerALatch;
                break;

            case 0x06:
                timerBLatch = (timerBLatch & 0xFF00) | value;
                if (!timerBRunning)
                    timerBCounter = timerBLatch;
                break;

            case 0x07:
                timerBLatch = (timerBLatch & 0x00FF) | (value << 8);
                if (!timerBRunning)
                    timerBCounter = timerBLatch;
                break;

            case 0x08:
                todTenths = value & 0x0F;
                todCycles = 0;
                todHalted = false;
                break;

            case 0x09:
                todSeconds = Clamp(FromBcd((byte)(value & 0x7F)), 0, 59);
                break;

            case 0x0A:
                todMinutes = Clamp(FromBcd((byte)(value & 0x7F)), 0, 59);
                break;

            case 0x0B:
                todHours = Clamp(FromBcd((byte)(value & 0x1F)), 1, 12);
                todPm = (value & 0x80) != 0;
                todHalted = true;
                break;

            case 0x0D:
                WriteInterrupt(value);
                break;

            case 0x0E:
                WriteControlA(value);
                break;

            case 0x0F:
                WriteControlB(value);
                break;
        }
    }

    protected byte ApplyTimerOutputs(byte value)
    {
        if (timerAOutputEnable)
            value = (byte)((value & ~0x40) | (TimerAOutputHigh ? 0x40 : 0));

        if (timerBOutputEnable)
            value = (byte)((value & ~0x80) | (TimerBOutputHigh ? 0x80 : 0));

        return value;
    }

    private void WriteControlA(byte value)
    {
        var start = (value & 0x01) != 0;
        var forceLoad = (value & 0x10) != 0;

        timerAOutputEnable = (value & 0x02) != 0;
        timerAOutputToggle = (value & 0x04) != 0;
        timerAOneShot = (value & 0x08) != 0;

        if (forceLoad)
            timerACounter = timerALatch;

        if (start && !timerARunning)
            timerACounter = timerALatch;

        timerARunning = start;
        controlA = value;
    }

    private void WriteControlB(byte value)
    {
        var start = (value & 0x01) != 0;
        var forceLoad = (value & 0x10) != 0;

        timerBOutputEnable = (value & 0x02) != 0;
        timerBOutputToggle = (value & 0x04) != 0;
        timerBOneShot = (value & 0x08) != 0;
        timerBInputMode = (value >> 5) & 0x03;

        if (forceLoad)
            timerBCounter = timerBLatch;

        if (start && !timerBRunning)
            timerBCounter = timerBLatch;

        timerBRunning = start;
        controlB = value;
    }

    private int ProcessTimerA(int cycles)
    {
        if (!timerARunning)
            return 0;

        var underflows = 0;
        timerACounter -= cycles;

        while (timerACounter < 0)
        {
            underflows++;
            SetInterruptFlag(0x01);
            PulseTimerAOutput();

            if (timerAOneShot)
            {
                timerARunning = false;
                timerACounter = 0;
                break;
            }

            timerACounter += timerALatch + 1;

            // A zero latch underflows every cycle. Bound the loop so a
            // malformed control write cannot stall this instruction.
            if (underflows > 32)
                break;
        }

        return underflows;
    }

    private void ProcessTimerB(int cycles, int timerAUnderflows)
    {
        if (!timerBRunning)
            return;

        int elapsed;

        if (timerBInputMode == 2 || timerBInputMode == 3)
            elapsed = timerAUnderflows;
        else if (timerBInputMode == 1)
            elapsed = 0; // CNT has no clock source in this core.
        else
            elapsed = cycles;

        if (elapsed <= 0)
            return;

        timerBCounter -= elapsed;
        var underflows = 0;

        while (timerBCounter < 0)
        {
            underflows++;
            SetInterruptFlag(0x02);
            PulseTimerBOutput();

            if (timerBOneShot)
            {
                timerBRunning = false;
                timerBCounter = 0;
                break;
            }

            timerBCounter += timerBLatch + 1;

            if (underflows > 32)
                break;
        }
    }

    private void PulseTimerAOutput()
    {
        if (!timerAOutputEnable)
            return;

        if (timerAOutputToggle)
            timerAOutputState = !timerAOutputState;
        else
            timerAPulse = true;
    }

    private void PulseTimerBOutput()
    {
        if (!timerBOutputEnable)
            return;

        if (timerBOutputToggle)
            timerBOutputState = !timerBOutputState;
        else
            timerBPulse = true;
    }

    private void SetInterruptFlag(byte flag)
    {
        interruptFlags |= flag;
        UpdateIrq();
    }

    private byte ReadInterrupt()
    {
        var status = (byte)(interruptFlags & 0x1F);

        if ((interruptFlags & interruptMask) != 0)
            status |= 0x80;

        interruptFlags = 0;

        if (useNmi)
            interrupts.SetNmi(interruptSource, false);
        else
            interrupts.Set(interruptSource, false);

        return status;
    }

    private void WriteInterrupt(byte value)
    {
        var bits = (byte)(value & 0x1F);

        if ((value & 0x80) != 0)
            interruptMask |= bits;
        else
            interruptMask &= (byte)~bits;

        UpdateIrq();
    }

    private void UpdateIrq()
    {
        var asserted = (interruptFlags & interruptMask) != 0;

        if (useNmi)
            interrupts.SetNmi(interruptSource, asserted);
        else
            interrupts.Set(interruptSource, asserted);
    }

    private void LatchTod()
    {
        if (todLatched)
            return;

        todLatched = true;
        latchedTenths = todTenths;
        latchedSeconds = todSeconds;
        latchedMinutes = todMinutes;
        latchedHours = todHours;
        latchedPm = todPm;
    }

    private void TickTod(int cycles)
    {
        if (todHalted)
            return;

        todCycles += cycles;

        while (todCycles >= TodClockCycles)
        {
            todCycles -= TodClockCycles;
            AdvanceTod();
        }
    }

    private void AdvanceTod()
    {
        todTenths++;

        if (todTenths < 10)
            return;

        todTenths = 0;
        todSeconds++;

        if (todSeconds < 60)
            return;

        todSeconds = 0;
        todMinutes++;

        if (todMinutes < 60)
            return;

        todMinutes = 0;
        todHours++;

        if (todHours == 12)
        {
            // 11 AM -> 12 PM and 11 PM -> 12 AM; 12 -> 1 keeps the
            // current half of the day.
            todPm = !todPm;
            return;
        }

        if (todHours > 12)
            todHours = 1;
    }

    private void ResetTodToNow()
    {
        DateTime now = DateTime.Now;

        todTenths = now.Millisecond / 100;
        todSeconds = now.Second;
        todMinutes = now.Minute;
        todHours = now.Hour % 12;

        if (todHours == 0)
            todHours = 12;

        todPm = now.Hour >= 12;
        todCycles = 0;
        todHalted = false;
        todLatched = false;
    }

    private static byte ToBcd(int value)
    {
        return (byte)(((value / 10) << 4) | (value % 10));
    }

    private static int FromBcd(byte value)
    {
        return ((value >> 4) & 0x0F) * 10 + (value & 0x0F);
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Clamp(value, minimum, maximum);
    }
}
