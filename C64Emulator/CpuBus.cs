using System;

namespace C64Emulator
{
    /// <summary>
    /// The clock interface used by <see cref="ClockedCpuBus"/>. Each access is
    /// one 6510 bus cycle; the implementation stalls reads while BA is low and
    /// stalls all accesses while AEC is low.
    /// </summary>
    internal interface ICpuBusClock
    {
        void WaitForAccess(bool isWrite);

        void CompleteAccess();
    }

    /// <summary>
    /// Wraps the CPU's view of memory so every read and write is individually
    /// clocked. Keeping this at the bus boundary means the instruction helpers
    /// do not need to know about VIC bus contention: a read that lands in a BA
    /// lead-in waits, while a write in the same window completes normally.
    /// </summary>
    internal sealed class ClockedCpuBus : IMemoryBus
    {
        private readonly IMemoryBus bus;
        private readonly ICpuBusClock clock;

        public ClockedCpuBus(IMemoryBus bus, ICpuBusClock clock)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public byte Peek(ushort address)
        {
            clock.WaitForAccess(isWrite: false);
            byte value = bus.Peek(address);
            clock.CompleteAccess();
            return value;
        }

        public void Poke(ushort address, byte value)
        {
            clock.WaitForAccess(isWrite: true);

            // A write is latched at the end of its bus cycle. The VIC renders
            // the current cycle from the old register values; the new value
            // must not retroactively shift the pixels that cycle already drew.
            clock.CompleteAccess();
            bus.Poke(address, value);
        }

        public ushort PeekWord(ushort address)
        {
            // A 16-bit fetch is two bus cycles on the 6510, so it must not
            // bypass the clock through the underlying bus implementation.
            return (ushort)(
                Peek(address) |
                (Peek((ushort)(address + 1)) << 8));
        }
    }
}
