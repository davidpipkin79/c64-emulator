using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Media;
using Xunit;

namespace C64Emulator.Tests;

public sealed class TapTests
{
    private const byte ShortPulse = 0x30;
    private const byte MediumPulse = 0x42;
    private const byte LongPulse = 0x56;

    [Fact]
    public void DecodesStandardCbmBlocksIntoTapeFiles()
    {
        var image = CreateTap(
            ("GAME", 0x0801, new byte[] { 0x41, 0x42, 0x43 }),
            ("DATA", 0xC000, new byte[] { 0x44 }));
        TapImage tap = TapImage.FromBytes(image, "TEST TAPE");

        Assert.Equal("TEST TAPE", tap.TapeName);
        Assert.Equal(2, tap.Directory.Count);
        Assert.Equal("GAME", tap.Directory[0].Name);
        Assert.Equal(CbmFileType.Program, tap.Directory[0].Type);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, tap.ReadFile(tap.Directory[0]));
        Assert.Equal(
            new byte[] { 0x01, 0x08, 0x41, 0x42, 0x43 },
            tap.ToPrg(tap.Directory[0]));
        Assert.Equal("DATA", tap.FindFile("?ATA")?.Name);
    }

    [Fact]
    public void MountsTapThroughTheTapeDrive()
    {
        var tap = TapImage.FromBytes(
            CreateTap(("GAME", 0x0801, new byte[] { 0x41 })),
            "GAME TAPE");
        TapeDrive drive = new TapeDrive();

        drive.Mount(tap, "game.tap");

        Assert.True(drive.IsMounted);
        Assert.Equal("game.tap", drive.ImagePath);
        Assert.Single(drive.Directory);
        Assert.Equal("GAME", drive.FindFile("*")?.Name);
        Assert.Equal(
            new byte[] { 0x01, 0x08, 0x41 },
            drive.CreatePrg(drive.FindFile("GAME")!));

        drive.Eject();
        Assert.False(drive.IsMounted);
    }

    [Fact]
    public void RejectsNonTapImagesAndKeepsTurboOnlyPulseData()
    {
        Assert.Throws<InvalidDataException>(() => TapImage.FromBytes(new byte[32]));

        var invalid = CreateTap(("GAME", 0x0801, new byte[] { 0x41 }));
        invalid[12] = 2;
        Assert.Throws<InvalidDataException>(() => TapImage.FromBytes(invalid));

        var turbo = CreateRawTap(0x30, 0x42, 0x56, 0x30);
        TapImage image = TapImage.FromBytes(turbo, "TURBO");

        Assert.Empty(image.Directory);
        Assert.Equal(new[] { 0x180, 0x210, 0x2B0, 0x180 }, image.Pulses);
    }

    [Fact]
    public void PulsePlaybackDrivesTheCiaFlagLineOnlyWhileTheMotorRuns()
    {
        Cia1 cia = new Cia1(new InterruptLine());
        TapeDrive drive = new TapeDrive();
        drive.AttachReadLine(cia.SetFlag);
        drive.Mount(
            TapImage.FromBytes(CreateRawTap(0x30, 0x42, 0x56)),
            "test.tap");

        drive.Tick(0x180);
        Assert.Equal(0, cia.Peek(0x0D));

        drive.SetMotor(true);
        drive.Tick(0x180);
        Assert.Equal(0x10, cia.Peek(0x0D));

        drive.Tick(0x210);
        Assert.Equal(0x10, cia.Peek(0x0D));
    }

    [Fact]
    public void SaveStateRestoresPulsePositionAndMotor()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "c64-tape-" + Guid.NewGuid().ToString("N") + ".tap");

        try
        {
            File.WriteAllBytes(path, CreateRawTap(0x30, 0x42, 0x56));

            using C64Machine machine = new C64Machine(
                RomSet.Create(
                    new byte[AddressMap.KernalRomSize],
                    new byte[AddressMap.BasicRomSize],
                    new byte[AddressMap.CharacterRomSize]));
            machine.MountTape(path);
            machine.Tape.SetMotor(true);
            machine.Tape.Tick(0x180);

            Assert.Equal(1, machine.Tape.PulseIndex);
            Assert.True(machine.Tape.MotorOn);

            byte[] state = MachineState.Capture(machine, flushMedia: false);
            machine.Tape.SetMotor(false);
            machine.Tape.ResetPlayback();
            MachineState.Restore(machine, state);

            Assert.Equal(1, machine.Tape.PulseIndex);
            Assert.True(machine.Tape.MotorOn);

            machine.Tape.Tick(0x210);
            Assert.Equal(2, machine.Tape.PulseIndex);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }


    [Fact]
    public void KernalLoadsTapThroughTheDatasetteFlagLine()
    {
        var romDirectory = GetRomDirectory();

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateTap(("GAME", 0x0801, new byte[] { 0x41, 0x42 }));
        TapImage tap = TapImage.FromBytes(image, "GAME TAPE");

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.MountTape(tap);

        // LOAD"" is the KERNAL tape form that accepts the next header without
        // a filename comparison.
        machine.Memory.Poke(0xB7, 0);
        machine.Memory.Poke(0xB9, 1);
        machine.Memory.Poke(0xBA, 1);
        machine.Memory.Poke(0x0314, 0x31); // KERNAL IRQ vector
        machine.Memory.Poke(0x0315, 0xEA);
        machine.Memory.Poke(0x0330, 0xA5); // KERNAL LOAD vector
        machine.Memory.Poke(0x0331, 0xF4);
        machine.Memory.Poke(0x0328, 0xED); // KERNAL STOP vector
        machine.Memory.Poke(0x0329, 0xF6);
        machine.Memory.Poke(0xB2, 0x3C); // KERNAL tape buffer
        machine.Memory.Poke(0xB3, 0x03);
        machine.Cpu.X = 0x01;
        machine.Cpu.Y = 0x08;
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.A = 0;
        machine.Cpu.SetPC(0xFFD5);

        for (var i = 0; i < 40_000_000 && machine.Cpu.GetPC() != 0x1235; i++)
            machine.RunCycles(1);

        Assert.Equal(0x1235, machine.Cpu.GetPC());
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0, machine.Cpu.P & 0x01);
    }

    [Theory]
    [InlineData("GAME")]
    [InlineData("*")]
    public void KernalLoadsNamedTapThroughTheDatasetteFlagLine(string name)
    {
        var romDirectory = GetRomDirectory();

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateTap(("GAME", 0x0801, new byte[] { 0x41, 0x42 }));
        TapImage tap = TapImage.FromBytes(image, "GAME TAPE");

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.MountTape(tap);

        WriteFilename(machine, name);
        machine.Memory.Poke(0xB9, 1);
        machine.Memory.Poke(0xBA, 1);
        machine.Memory.Poke(0x0314, 0x31); // KERNAL IRQ vector
        machine.Memory.Poke(0x0315, 0xEA);
        machine.Memory.Poke(0x0330, 0xA5); // KERNAL LOAD vector
        machine.Memory.Poke(0x0331, 0xF4);
        machine.Memory.Poke(0x0328, 0xED); // KERNAL STOP vector
        machine.Memory.Poke(0x0329, 0xF6);
        machine.Memory.Poke(0xB2, 0x3C); // KERNAL tape buffer
        machine.Memory.Poke(0xB3, 0x03);
        machine.Cpu.X = 0x01;
        machine.Cpu.Y = 0x08;
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.A = 0;
        machine.Cpu.SetPC(0xFFD5);

        for (var i = 0; i < 40_000_000 && machine.Cpu.GetPC() != 0x1235; i++)
            machine.RunCycles(1);

        Assert.Equal(0x1235, machine.Cpu.GetPC());
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0, machine.Cpu.P & 0x01);
        Assert.Equal((byte)name.Length, machine.Memory.Peek(0xB7));
    }

    [Fact]
    public void TapMenuLoadSupportsWildcardsAndRejectsMissingNames()
    {
        TapImage tap = TapImage.FromBytes(
            CreateTap(
                ("LOADER", 0x0801, new byte[] { 0x01 }),
                ("GAME", 0x0801, new byte[] { 0x41, 0x42 })));

        using C64Machine machine = new C64Machine(
            RomSet.Create(
                new byte[AddressMap.KernalRomSize],
                new byte[AddressMap.BasicRomSize],
                new byte[AddressMap.CharacterRomSize]));
        machine.MountTape(tap);

        Assert.Equal("GAME", tap.FindFile("G*")?.Name);
        Assert.Equal("GAME", tap.FindFile("?AME")?.Name);

        machine.LoadTapeProgram("?AME");

        Assert.Equal(0xA659, machine.Cpu.GetPC());
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));

        FileNotFoundException error = Assert.Throws<FileNotFoundException>(
            () => machine.LoadTapeProgram("MISSING"));
        Assert.Contains("MISSING", error.Message);
    }

    private static string GetRomDirectory()
    {
        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));
    }

    private static void WriteFilename(C64Machine machine, string name)
    {
        // The KERNAL tape buffer is $033C-$03FB. Keep the name outside it so
        // loading the header does not overwrite the bytes being compared.
        const ushort address = 0x0900;

        machine.Memory.Poke(0xB7, (byte)name.Length);
        machine.Memory.Poke(0xBB, (byte)(address & 0xFF));
        machine.Memory.Poke(0xBC, (byte)(address >> 8));

        for (var i = 0; i < name.Length; i++)
            machine.Memory.Poke((ushort)(address + i), (byte)name[i]);
    }

    private static byte[] CreateTap(
        params (string Name, ushort Start, byte[] Payload)[] files)
    {
        List<byte> pulses = [];
        var firstBlock = true;

        foreach (var file in files)
        {
            AddLeader(pulses, firstBlock ? 16382 : 243);
            firstBlock = false;

            var header = new byte[192];
            header[0] = 1; // relocatable PRG
            header[1] = (byte)(file.Start & 0xFF);
            header[2] = (byte)(file.Start >> 8);

            var end = file.Start + file.Payload.Length;
            header[3] = (byte)(end & 0xFF);
            header[4] = (byte)(end >> 8);
            WriteName(header, file.Name);
            AddBlock(pulses, header, 0x89);
            AddGap(pulses);
            AddLeader(pulses, 5680);
            AddBlock(pulses, header, 0x09);
            AddGap(pulses);

            var data = new byte[192];
            file.Payload.CopyTo(data, 0);
            AddLeader(pulses, 5680);
            AddBlock(pulses, data, 0x89);
            AddGap(pulses);
            AddLeader(pulses, 5680);
            AddBlock(pulses, data, 0x09);
            AddGap(pulses);
        }

        var image = new byte[20 + pulses.Count];
        Encoding.ASCII.GetBytes("C64-TAPE-RAW").CopyTo(image, 0);
        image[12] = 0;
        image[16] = (byte)(pulses.Count & 0xFF);
        image[17] = (byte)((pulses.Count >> 8) & 0xFF);
        image[18] = (byte)((pulses.Count >> 16) & 0xFF);
        image[19] = (byte)((pulses.Count >> 24) & 0xFF);
        pulses.CopyTo(image, 20);
        return image;
    }

    private static byte[] CreateRawTap(params byte[] pulses)
    {
        var image = new byte[20 + pulses.Length];
        Encoding.ASCII.GetBytes("C64-TAPE-RAW").CopyTo(image, 0);
        image[12] = 0;
        image[16] = (byte)(pulses.Length & 0xFF);
        image[17] = (byte)((pulses.Length >> 8) & 0xFF);
        image[18] = (byte)((pulses.Length >> 16) & 0xFF);
        image[19] = (byte)((pulses.Length >> 24) & 0xFF);
        pulses.CopyTo(image, 20);
        return image;
    }

    private static void AddBlock(
        List<byte> pulses,
        byte[] payload,
        byte countdownStart)
    {
        for (var count = countdownStart; count >= countdownStart - 8; count--)
            AddByte(pulses, (byte)count);

        foreach (var value in payload)
            AddByte(pulses, value);

        byte checksum = 0;

        foreach (var value in payload)
            checksum ^= value;

        AddByte(pulses, checksum);
    }

    private static void AddLeader(List<byte> pulses, int count)
    {
        for (var i = 0; i < count; i++)
            pulses.Add(ShortPulse);
    }

    private static void AddGap(List<byte> pulses)
    {
        pulses.Add(LongPulse);

        for (var i = 0; i < 60; i++)
            pulses.Add(ShortPulse);
    }

    private static void AddByte(List<byte> pulses, byte value)
    {
        pulses.Add(LongPulse);
        pulses.Add(MediumPulse);

        for (var bit = 0; bit < 8; bit++)
        {
            if ((value & (1 << bit)) == 0)
            {
                pulses.Add(ShortPulse);
                pulses.Add(MediumPulse);
            }
            else
            {
                pulses.Add(MediumPulse);
                pulses.Add(ShortPulse);
            }
        }

        // The ninth tape bit is odd parity over the data byte and itself.
        var parity = (System.Numerics.BitOperations.PopCount(value) & 1) == 0;
        AddBit(pulses, parity);
    }

    private static void AddBit(List<byte> pulses, bool one)
    {
        pulses.Add(one ? MediumPulse : ShortPulse);
        pulses.Add(one ? ShortPulse : MediumPulse);
    }

    private static void WriteName(byte[] header, string name)
    {
        for (var i = 0; i < 16; i++)
            header[5 + i] = i < name.Length ? (byte)name[i] : (byte)0xA0;
    }
}
