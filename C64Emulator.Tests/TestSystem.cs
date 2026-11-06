using System;
using C64Emulator.Audio;
using C64Emulator.Core;
using C64Emulator.Cpu;
using C64Emulator.Devices;
using C64Emulator.Video;

namespace C64Emulator.Tests;

internal sealed class TestSystem : IDisposable
{
    public TestSystem(bool withPeripherals = false, RomSet? roms = null)
    {
        var dataBus = new BusLatch();
        Memory = new Memory(dataBus);
        Interrupts = new InterruptLine();
        Cpu = new Cpu6510(Memory, Interrupts);

        if (!withPeripherals)
            return;

        ColorRam = new ColorRam();
        Vic = new Vic(Memory, ColorRam, Interrupts);
        Sid = new Sid();
        Cia1 = new Cia1(Interrupts);
        Cia2 = new Cia2(Interrupts, Vic);
        Io = new IoController(Vic, Sid, ColorRam, Cia1, Cia2, dataBus);
        Memory.Configure(
            roms ?? RomSet.Create(
                new byte[AddressMap.KernalRomSize],
                new byte[AddressMap.BasicRomSize],
                new byte[AddressMap.CharacterRomSize]),
            Io);
    }

    public Memory Memory { get; }

    public InterruptLine Interrupts { get; }

    public Cpu6510 Cpu { get; }

    public ColorRam? ColorRam { get; }

    public Vic? Vic { get; }

    public Sid? Sid { get; }

    public Cia1? Cia1 { get; }

    public Cia2? Cia2 { get; }

    public IoController? Io { get; }

    public void Dispose()
    {
    }
}
