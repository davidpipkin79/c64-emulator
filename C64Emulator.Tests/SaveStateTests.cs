using System;
using System.IO;
using System.Threading;
using C64Emulator.Core;
using Xunit;

namespace C64Emulator.Tests;

public sealed class SaveStateTests
{
    [Fact]
    public void RoundTripRestoresCpuMemoryAndChips()
    {
        using C64Machine machine = CreateMachine();

        // Keep the CPU in a harmless RAM loop while the VIC, SID and CIAs
        // advance to a non-reset state.
        machine.Memory.Poke(0x1000, 0x4C);
        machine.Memory.Poke(0x1001, 0x00);
        machine.Memory.Poke(0x1002, 0x10);
        machine.Cpu.SetPC(0x1000);
        machine.RunCycles(20_000);

        machine.Cpu.SetPC(0x1234);
        machine.Cpu.A = 0x42;
        machine.Cpu.X = 0x11;
        machine.Cpu.Y = 0x22;
        machine.Cpu.S = 0xFD;
        machine.Cpu.P = 0x24;
        machine.Memory.Poke(0x0400, 0x41);
        machine.Memory.Poke(0xD020, 0x0E);
        machine.Memory.Poke(0xD400, 0x12);
        machine.Memory.Poke(0xDC04, 0x34);

        int rasterLine = machine.Vic.RasterLine;
        int rasterCycle = machine.Vic.RasterCycle;
        byte[] state = MachineState.Capture(machine, flushMedia: false);

        machine.Cpu.SetPC(0x2000);
        machine.Cpu.A = 0;
        machine.Cpu.X = 0;
        machine.Cpu.Y = 0;
        machine.Cpu.P = 0;
        machine.Memory.Poke(0x0400, 0x99);
        machine.Memory.Poke(0xD020, 0x00);
        machine.Memory.Poke(0xD400, 0x00);
        machine.RunCycles(5_000);

        MachineState.Restore(machine, state);

        Assert.Equal(0x1234, machine.Cpu.GetPC());
        Assert.Equal(0x42, machine.Cpu.A);
        Assert.Equal(0x11, machine.Cpu.X);
        Assert.Equal(0x22, machine.Cpu.Y);
        Assert.Equal(0xFD, machine.Cpu.S);
        Assert.Equal(0x24, machine.Cpu.P);
        Assert.Equal(0x41, machine.Memory.Peek(0x0400));
        Assert.Equal(0xFE, machine.Memory.Peek(0xD020));
        Assert.Equal(0x12, machine.Sid.GetRegister(0));
        Assert.Equal(0x34, machine.Memory.Peek(0xDC04));
        Assert.Equal(rasterLine, machine.Vic.RasterLine);
        Assert.Equal(rasterCycle, machine.Vic.RasterCycle);
    }

    [Fact]
    public void SaveStateFileRoundTrips()
    {
        using C64Machine machine = CreateMachine();
        machine.Cpu.SetPC(0x3456);
        machine.Cpu.A = 0x5A;
        machine.Memory.Poke(0x2000, 0x77);

        var path = Path.Combine(
            Path.GetTempPath(),
            "c64-state-" + Guid.NewGuid().ToString("N") + ".state");

        try
        {
            machine.SaveStateToFile(path);

            machine.Cpu.SetPC(0x0000);
            machine.Cpu.A = 0;
            machine.Memory.Poke(0x2000, 0x00);

            machine.LoadStateFromFile(path);

            Assert.Equal(0x3456, machine.Cpu.GetPC());
            Assert.Equal(0x5A, machine.Cpu.A);
            Assert.Equal(0x77, machine.Memory.Peek(0x2000));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PausesForSaveAndRewindWhileRunning()
    {
        using C64Machine machine = CreateMachine();

        var path = Path.Combine(
            Path.GetTempPath(),
            "c64-live-state-" + Guid.NewGuid().ToString("N") + ".state");

        machine.Start();

        try
        {
            // Let the frame loop produce a few rewind snapshots.
            Thread.Sleep(300);
            machine.SaveStateToFile(path);

            Assert.True(File.Exists(path));

            var deadline = Environment.TickCount64 + 2000;
            var rewound = false;
            string message = string.Empty;

            while (!rewound && Environment.TickCount64 < deadline)
            {
                Thread.Sleep(50);
                rewound = machine.RewindOneStep(out message);
            }

            Assert.True(rewound, message);

            machine.LoadStateFromFile(path);
        }
        finally
        {
            machine.Stop();

            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void RoundTripIncludesHardwareDriveState()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        var driveRom = Path.Combine(romDirectory, "dos1541.rom");

        if (!File.Exists(driveRom))
            return;

        using C64Machine machine = new(
            RomSet.Load(romDirectory),
            File.ReadAllBytes(driveRom));

        machine.TrueDriveEmulation = true;

        var drive = machine.Drive.RealDrive!;
        drive.Cpu.SetPC(0x0000);
        drive.Ram[0] = 0xEA;
        machine.RunCycles(5_000);

        drive.Cpu.SetPC(0x1000);
        drive.Ram[1] = 0x66;
        byte[] state = MachineState.Capture(machine, flushMedia: false);

        drive.Cpu.SetPC(0x0000);
        drive.Ram[1] = 0x00;
        MachineState.Restore(machine, state);

        Assert.Equal(0x1000, drive.Cpu.GetPC());
        Assert.Equal(0x66, drive.Ram[1]);
    }

    private static C64Machine CreateMachine()
    {
        return new C64Machine(
            RomSet.Create(
                new byte[AddressMap.KernalRomSize],
                new byte[AddressMap.BasicRomSize],
                new byte[AddressMap.CharacterRomSize]));
    }
}
