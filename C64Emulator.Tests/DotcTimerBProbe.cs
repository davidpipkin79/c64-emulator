using System;
using C64Emulator.Devices;
using Xunit;

namespace C64Emulator.Tests;

public sealed class DotcTimerBProbe : IDisposable
{
    private readonly TestSystem system;

    public DotcTimerBProbe()
    {
        system = new TestSystem(withPeripherals: true);
    }

    public void Dispose()
    {
        system.Dispose();
    }

    [Fact]
    public void DotcTimerB_LatchFromModule_UnderflowsAndSetsIcrBit1()
    {
        Cia2 cia = system.Cia2!;

        // Exact sequence the music module's install performs at $8017-$8026.
        cia.Poke(0x0E, 0x00);
        cia.Poke(0x06, 0xD4);
        cia.Poke(0x07, 0x30);
        cia.Poke(0x0D, 0x83);
        cia.Poke(0x0F, 0x81);

        cia.ProcessTimers(13_000);

        var icr = cia.Peek(0x0D);

        Assert.True((icr & 0x02) != 0, $"Timer B never underflowed; ICR=${icr:X2}");
    }

    [Fact]
    public void DotcTimerB_AssertedNmi_ReachesTheProcessorLine()
    {
        Cia2 cia = system.Cia2!;

        cia.Poke(0x0E, 0x00);
        cia.Poke(0x06, 0xD4);
        cia.Poke(0x07, 0x30);
        cia.Poke(0x0D, 0x83);
        cia.Poke(0x0F, 0x81);

        cia.ProcessTimers(13_000);

        Assert.True(system.Interrupts.IsNmiAsserted, "Timer B fired without asserting NMI");
    }
}
