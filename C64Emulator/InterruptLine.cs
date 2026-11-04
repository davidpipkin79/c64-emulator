using System.Threading;

namespace C64Emulator
{
    internal enum InterruptSource
    {
        Vic = 0,
        Cia1 = 1,
        Cia2 = 2
    }

    /// <summary>
    /// Models the C64's active-low IRQ line as a wired-OR of independent
    /// sources. A device asserts or releases its own source; the CPU samples
    /// the line only at instruction boundaries.
    /// </summary>
    internal sealed class InterruptLine
    {
        private int assertedIrqSources;
        private int assertedNmiSources;
        private int pendingNmi;

        public bool IsIrqAsserted => Volatile.Read(ref assertedIrqSources) != 0;

        /// <summary>
        /// The 6502 NMI input is edge-sensitive. Keep one edge latched until
        /// the CPU services it; a source that remains asserted must not create
        /// a new interrupt on every instruction.
        /// </summary>
        public bool IsNmiAsserted => Volatile.Read(ref pendingNmi) != 0;

        public void Set(InterruptSource source, bool asserted)
        {
            SetSource(ref assertedIrqSources, source, asserted);
        }

        /// <summary>
        /// CIA2 is wired to the CPU's NMI line on the C64, unlike CIA1's IRQ.
        /// </summary>
        public void SetNmi(InterruptSource source, bool asserted)
        {
            int mask = 1 << (int)source;

            if (asserted)
            {
                int previous = Interlocked.Or(ref assertedNmiSources, mask);

                // Only a low-to-high transition creates an NMI edge. If the
                // line was already high, leave the already-latched edge alone.
                if (previous == 0)
                    Volatile.Write(ref pendingNmi, 1);
            }
            else
            {
                int previous = Interlocked.And(ref assertedNmiSources, ~mask);

                // Once the NMI line is low again a later assertion is a new
                // edge and may interrupt once more.
                if ((previous & ~mask) == 0)
                    Volatile.Write(ref pendingNmi, 0);
            }
        }

        public void Clear()
        {
            Volatile.Write(ref assertedIrqSources, 0);
            Volatile.Write(ref assertedNmiSources, 0);
            Volatile.Write(ref pendingNmi, 0);
        }

        public void AcknowledgeNmi()
        {
            // Consume the latched edge but leave the source asserted. A
            // continuously asserted source must not retrigger after RTI.
            Volatile.Write(ref pendingNmi, 0);
        }

        private static void SetSource(
            ref int sources,
            InterruptSource source,
            bool asserted)
        {
            int mask = 1 << (int)source;

            if (asserted)
                Interlocked.Or(ref sources, mask);
            else
                Interlocked.And(ref sources, ~mask);
        }
    }

    /// <summary>
    /// The CPU's view of the address bus.
    /// </summary>
    internal interface IMemoryBus
    {
        byte Peek(ushort address);

        void Poke(ushort address, byte value);

        ushort PeekWord(ushort address);

        /// <summary>
        /// An internal CPU cycle. Buses that model timing use it to advance
        /// the machine; a plain memory has nothing to do.
        /// </summary>
        void Idle();
    }

    /// <summary>
    /// The VIC's view of memory. It sees RAM and the character ROM overlay,
    /// but not the I/O area or the KERNAL/BASIC ROMs.
    /// </summary>
    internal interface IVideoBus
    {
        byte Read(ushort address);
    }

    /// <summary>
    /// The I/O area behind the PLA.
    /// </summary>
    internal interface IIoPort
    {
        byte Peek(ushort address);

        void Poke(ushort address, byte value);
    }

    /// <summary>
    /// A device that can change the VIC's 16 KB bank, such as CIA2 port A.
    /// </summary>
    internal interface IVideoBank
    {
        ushort BankAddress { get; set; }
    }
}
