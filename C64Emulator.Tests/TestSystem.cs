using System;

namespace C64Emulator.Tests
{
    internal sealed class TestSystem : IDisposable
    {
        public TestSystem(bool withPeripherals = false, RomSet? roms = null)
        {
            Memory = new Memory();
            Interrupts = new InterruptLine();
            Cpu = new CPU6510(Memory, Interrupts);

            if (!withPeripherals)
                return;

            ColorRam = new ColorRam();
            Vic = new VIC(Memory, ColorRam, Interrupts);
            Sid = new SID();
            Cia1 = new CIA1(Interrupts);
            Cia2 = new CIA2(Interrupts, Vic);
            Io = new IO(Vic, Sid, ColorRam, Cia1, Cia2);
            Memory.Configure(
                roms ?? RomSet.Create(
                    new byte[AddressMap.KernalRomSize],
                    new byte[AddressMap.BasicRomSize],
                    new byte[AddressMap.CharacterRomSize]),
                Io);
        }

        public Memory Memory { get; }

        public InterruptLine Interrupts { get; }

        public CPU6510 Cpu { get; }

        public ColorRam? ColorRam { get; }

        public VIC? Vic { get; }

        public SID? Sid { get; }

        public CIA1? Cia1 { get; }

        public CIA2? Cia2 { get; }

        public IO? Io { get; }

        public void Dispose()
        {
        }
    }
}
