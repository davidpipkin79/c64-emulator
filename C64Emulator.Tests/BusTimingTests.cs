using System;
using C64Emulator.Core;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class BusTimingTests : IDisposable
{
    private readonly C64Machine machine;

    public BusTimingTests()
    {
        machine = new C64Machine(
            RomSet.Create(
                new byte[AddressMap.KernalRomSize],
                new byte[AddressMap.BasicRomSize],
                new byte[AddressMap.CharacterRomSize]));

        machine.Cpu.S = 0xFF;
        machine.Cpu.P = 0x20;
    }

    public void Dispose()
    {
        machine.Dispose();
    }

    [Fact]
    public void NopConsumesItsNominalTwoCycles()
    {
        WriteProgram(0xEA); // NOP

        machine.RunCycles(1);

        Assert.Equal(0, machine.Vic.RasterLine);
        Assert.Equal(2, machine.Vic.RasterCycle);
    }

    [Fact]
    public void BadlineLeadInStallsAnInstructionFetchRead()
    {
        machine.Vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        machine.Vic.Tick(machine.Vic.FrameCycles);
        TickToCycle(machine.Vic, 51, 12);

        WriteProgram(0xA9, 0x01); // LDA #$01

        // BA is low from cycle 12 through the 40 AEC cycles, so the
        // two-cycle instruction starts on cycle 55 and ends on 57.
        machine.RunCycles(1);

        Assert.Equal(1, machine.Cpu.A);
        Assert.Equal(51, machine.Vic.RasterLine);
        Assert.Equal(57, machine.Vic.RasterCycle);
    }

    [Fact]
    public void StoreCompletesDuringBaLeadIn()
    {
        machine.Vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        machine.Vic.SetRegister(0x15, 0x01); // sprite 0
        machine.Vic.SetRegister(0x01, 50);   // starts on raster 51
        machine.Vic.Tick(machine.Vic.FrameCycles);
        TickToCycle(machine.Vic, 51, 51);

        machine.Cpu.A = 0x42;
        WriteProgram(0x8D, 0x00, 0x04); // STA $0400

        // The opcode and two operand reads finish on cycle 54, exactly as
        // sprite 0's BA lead-in begins. A write is still allowed there.
        machine.RunCycles(1);

        Assert.Equal(0x42, machine.Memory.Peek(0x0400));
        Assert.Equal(51, machine.Vic.RasterLine);
        Assert.Equal(55, machine.Vic.RasterCycle);
    }

    [Fact]
    public void SwitchingToNtscUpdatesMachineClocksAndFrameLength()
    {
        machine.VideoStandard = VideoStandard.Ntsc;

        Assert.Equal(VideoStandard.Ntsc, machine.VideoStandard);
        Assert.Equal(1_022_727, machine.ClockFrequency);
        Assert.Equal(17_095, machine.CyclesPerFrame);
        Assert.Equal(1_022_727, machine.Sid.ClockFrequency);
        Assert.Equal(102_272, machine.Cia1.TodClockFrequency);
        Assert.Equal(235, machine.Vic.VisibleHeight);
        Assert.Equal(1_022_727, machine.Vic.ClockHz);
    }

    private void WriteProgram(params byte[] code)
    {
        const ushort start = 0x0200;

        for (var i = 0; i < code.Length; i++)
            machine.Memory.Poke((ushort)(start + i), code[i]);

        machine.Cpu.SetPC(start);
    }

    private static void TickToCycle(Vic vic, int line, int cycle)
    {
        for (var i = 0; i < vic.FrameCycles; i++)
        {
            if (vic.RasterLine == line && vic.RasterCycle == cycle)
                return;

            vic.Tick(1);
        }

        Assert.Fail(
            "VIC did not reach raster line " + line + " cycle " + cycle + ".");
    }
}
