using System;
using System.IO;
using System.Text;
using C64Emulator;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Media;
using Xunit;

namespace C64Emulator.Tests;

public sealed class T64Tests
{
    [Fact]
    public void ParsesTapeEntriesAndBuildsPrgs()
    {
        var image = CreateTape(
            ("GAME", 0x0801, new byte[] { 0x41, 0x42, 0x43 }, null),
            ("DATA", 0xC000, new byte[] { 0x44 }, null));

        T64Image tape = T64Image.FromBytes(image);

        Assert.Equal("TEST TAPE", tape.TapeName);
        Assert.Equal(2, tape.Directory.Count);
        Assert.Equal("GAME", tape.Directory[0].Name);
        Assert.Equal(CbmFileType.Program, tape.Directory[0].Type);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, tape.ReadFile(tape.Directory[0]));
        Assert.Equal(
            new byte[] { 0x01, 0x08, 0x41, 0x42, 0x43 },
            tape.ToPrg(tape.Directory[0]));
    }

    [Fact]
    public void UsesPayloadBoundariesWhenTheEndAddressIsWrong()
    {
        var image = CreateTape(
            ("GAME", 0x0801, new byte[] { 0x41, 0x42, 0x43 }, 0xC3C6));
        T64Image tape = T64Image.FromBytes(image);

        Assert.Single(tape.Directory);
        Assert.Equal(3, tape.Directory[0].DataLength);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, tape.ReadFile(tape.Directory[0]));
    }

    [Fact]
    public void WildcardAndFirstProgramLookupsWork()
    {
        var image = CreateTape(
            ("LOADER", 0x0801, new byte[] { 0x01 }, null),
            ("GAME", 0x0801, new byte[] { 0x02 }, null));
        T64Image tape = T64Image.FromBytes(image);

        Assert.Equal("GAME", tape.FindFile("GAME*")?.Name);
        Assert.Equal("GAME", tape.FindFile("?AME")?.Name);
        Assert.Equal("LOADER", tape.FirstProgram()?.Name);
    }

    [Fact]
    public void TapeDriveMountsAndEjectsImages()
    {
        T64Image tape = T64Image.FromBytes(
            CreateTape(("GAME", 0x0801, new byte[] { 0x41 }, null)));
        TapeDrive drive = new TapeDrive();

        drive.Mount(tape, "game.t64");

        Assert.True(drive.IsMounted);
        Assert.Equal("game.t64", drive.ImagePath);
        Assert.Single(drive.Directory);
        Assert.Equal("GAME", drive.FindFile("*")?.Name);

        drive.Eject();
        Assert.False(drive.IsMounted);
    }

    [Fact]
    public void RejectsInvalidImages()
    {
        Assert.Throws<InvalidDataException>(() => T64Image.FromBytes(new byte[32]));

        var invalid = new byte[64];
        Assert.Throws<InvalidDataException>(() => T64Image.FromBytes(invalid));
    }

    [Fact]
    public void MenuLoadStartsAProgramFromTheMountedTape()
    {
        var romDirectory = GetRomDirectory();

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateTape(
            ("GAME", 0x0801, new byte[] { 0x41, 0x42 }, null));

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Tape.Mount(T64Image.FromBytes(image));
        machine.LoadTapeProgram("GAME");

        Assert.Equal(0xA659, machine.Cpu.GetPC());
        Assert.Equal(0x01, machine.Memory.Peek(0x2B));
        Assert.Equal(0x08, machine.Memory.Peek(0x2C));
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
    }

    [Fact]
    public void KernalTapeLoadTrapLoadsAProgramFromTheMountedTape()
    {
        var romDirectory = GetRomDirectory();

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateTape(
            ("GAME", 0x0801, new byte[] { 0x41, 0x42 }, null));

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Tape.Mount(T64Image.FromBytes(image));

        WriteFilename(machine, "GAME");
        machine.Memory.Poke(0xB9, 1);
        machine.Memory.Poke(0xBA, 1);

        // Simulate the return address pushed by JSR $FFD5.
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.A = 0;
        machine.Cpu.SetPC(0xFFD5);

        machine.RunCycles(1);

        Assert.Equal(0x1235, machine.Cpu.GetPC());
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0x02, machine.Cpu.X);
        Assert.Equal(0x08, machine.Cpu.Y);
        Assert.Equal(0, machine.Cpu.P & 0x01);
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
        ushort address = 0x0370;

        machine.Memory.Poke(0xB7, (byte)name.Length);
        machine.Memory.Poke(0xBB, (byte)(address & 0xFF));
        machine.Memory.Poke(0xBC, (byte)(address >> 8));

        for (var i = 0; i < name.Length; i++)
            machine.Memory.Poke((ushort)(address + i), (byte)name[i]);
    }

    private static byte[] CreateTape(
        params (string Name, ushort Start, byte[] Payload, ushort? End)[] files)
    {
        var dataOffset = 64 + (files.Length * 32);
        var dataLength = 0;

        foreach (var file in files)
            dataLength += file.Payload.Length;

        var image = new byte[dataOffset + dataLength];
        WriteAscii(image, 0, 32, "C64S tape image file");
        WriteUInt16(image, 32, 0x0100);
        WriteUInt16(image, 34, files.Length);
        WriteUInt16(image, 36, files.Length);
        WriteAscii(image, 40, 24, "TEST TAPE");

        var offset = dataOffset;

        for (var i = 0; i < files.Length; i++)
        {
            var record = 64 + (i * 32);
            var file = files[i];

            image[record] = 1; // normal tape file
            image[record + 1] = 0x82; // PRG
            WriteUInt16(image, record + 2, file.Start);
            WriteUInt16(
                image,
                record + 4,
                file.End ?? (ushort)(file.Start + file.Payload.Length));
            WriteInt32(image, record + 8, offset);
            WriteAscii(image, record + 16, 16, file.Name);

            file.Payload.CopyTo(image, offset);
            offset += file.Payload.Length;
        }

        return image;
    }

    private static void WriteAscii(byte[] image, int offset, int length, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        Array.Copy(bytes, 0, image, offset, Math.Min(bytes.Length, length));
    }

    private static void WriteUInt16(byte[] image, int offset, int value)
    {
        image[offset] = (byte)(value & 0xFF);
        image[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteInt32(byte[] image, int offset, int value)
    {
        image[offset] = (byte)(value & 0xFF);
        image[offset + 1] = (byte)((value >> 8) & 0xFF);
        image[offset + 2] = (byte)((value >> 16) & 0xFF);
        image[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
