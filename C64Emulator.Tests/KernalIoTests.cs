using System;
using System.IO;
using System.Text;
using C64Emulator.Core;
using C64Emulator.Media;
using Xunit;

namespace C64Emulator.Tests;

public sealed class KernalIoTests
{
    [Fact]
    public void OpenChkinChrinReadsATapeFile()
    {
        using C64Machine machine = CreateMachineWithTape(
            ("DATA", new byte[] { 0x41, 0x42, 0x43 }));

        SetFilename(machine, "DATA");
        machine.Memory.Poke(0xB8, 1);
        machine.Memory.Poke(0xBA, 1);
        machine.Memory.Poke(0xB9, 0);

        Call(machine, 0xFFC0); // OPEN
        Assert.Equal(0, machine.Cpu.P & 0x01);
        Assert.Equal(0, machine.Cpu.A);

        machine.Cpu.X = 1;
        Call(machine, 0xFFC6); // CHKIN
        Assert.Equal(0, machine.Cpu.P & 0x01);

        Call(machine, 0xFFCF); // CHRIN
        Assert.Equal(0x41, machine.Cpu.A);

        Call(machine, 0xFFCF);
        Assert.Equal(0x42, machine.Cpu.A);

        Call(machine, 0xFFCF);
        Assert.Equal(0x43, machine.Cpu.A);
        Assert.NotEqual(0, machine.Memory.Peek(0x90) & 0x40);

        Call(machine, 0xFFCF);
        Assert.Equal(0, machine.Cpu.A);
    }

    [Fact]
    public void OpenDirectoryProducesReadableListing()
    {
        using C64Machine machine = CreateMachineWithTape(
            ("GAME", new byte[] { 0x01, 0x02 }));

        SetFilename(machine, "$");
        machine.Memory.Poke(0xB8, 1);
        machine.Memory.Poke(0xBA, 1);
        machine.Memory.Poke(0xB9, 0);

        Call(machine, 0xFFC0);
        Assert.Equal(0, machine.Cpu.A);
        Assert.Equal(0, machine.Cpu.P & 0x01);

        machine.Cpu.X = 1;
        Call(machine, 0xFFC6);

        StringBuilder listing = new StringBuilder();

        for (var i = 0; i < 100; i++)
        {
            Call(machine, 0xFFCF);
            listing.Append((char)machine.Cpu.A);

            if ((machine.Memory.Peek(0x90) & 0x40) != 0)
                break;
        }

        Assert.Contains("GAME", listing.ToString());
        Assert.Contains("PRG", listing.ToString());
    }

    [Fact]
    public void OpenMissingFileSetsCarryAndError()
    {
        using C64Machine machine = CreateMachineWithTape(
            ("GAME", new byte[] { 0x01 }));

        SetFilename(machine, "NOPE");
        machine.Memory.Poke(0xB8, 1);
        machine.Memory.Poke(0xBA, 1);
        machine.Memory.Poke(0xB9, 0);

        Call(machine, 0xFFC0);

        Assert.NotEqual(0, machine.Cpu.P & 0x01);
        Assert.Equal(4, machine.Cpu.A);
    }

    [Fact]
    public void CommandChannelReturnsDriveStatus()
    {
        using C64Machine machine = CreateMachine();
        machine.Drive.Mount(D64Image.FromBytes(CreateEmptyDisk()));

        machine.Memory.Poke(0xB7, 0);
        machine.Memory.Poke(0xB8, 15);
        machine.Memory.Poke(0xB9, 15);
        machine.Memory.Poke(0xBA, 8);

        Call(machine, 0xFFC0); // OPEN 15,8,15

        Assert.Equal(0, machine.Cpu.P & 0x01);
        Assert.Equal(0, machine.Cpu.A);

        machine.Cpu.X = 15;
        Call(machine, 0xFFC6); // CHKIN

        StringBuilder status = new StringBuilder();

        for (var i = 0; i < 32; i++)
        {
            Call(machine, 0xFFCF); // CHRIN
            var character = (char)machine.Cpu.A;
            status.Append(character);

            if (character == '\r')
                break;
        }

        Assert.StartsWith("00, OK", status.ToString());
    }

    [Fact]
    public void OpenWithoutMediaReportsDeviceNotPresent()
    {
        using C64Machine machine = CreateMachine();

        SetFilename(machine, "GAME");
        machine.Memory.Poke(0xB8, 1);
        machine.Memory.Poke(0xBA, 8);
        machine.Memory.Poke(0xB9, 0);

        Call(machine, 0xFFC0);

        Assert.NotEqual(0, machine.Cpu.P & 0x01);
        Assert.Equal(5, machine.Cpu.A);
    }

    private static byte[] CreateEmptyDisk()
    {
        var image = new byte[174848];
        var directory = DiskOffset(18, 1);

        image[directory] = 0;
        image[directory + 1] = 0xFF;

        return image;
    }

    private static int DiskOffset(int track, int sector)
    {
        var offset = 0;

        for (var current = 1; current < track; current++)
            offset += D64Image.SectorsPerTrack(current) * D64Image.SectorSize;

        return offset + (sector * D64Image.SectorSize);
    }

    private static C64Machine CreateMachineWithTape(
        params (string Name, byte[] Payload)[] files)
    {
        C64Machine machine = CreateMachine();
        T64Image tape = T64Image.FromBytes(CreateTape(files));
        machine.Tape.Mount(tape);
        return machine;
    }

    private static C64Machine CreateMachine()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            throw new InvalidOperationException("Test ROMs are not available.");

        return new C64Machine(RomSet.Load(romDirectory));
    }

    private static void Call(C64Machine machine, ushort address)
    {
        const ushort returnAddress = 0x1234;

        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, (byte)(returnAddress & 0xFF));
        machine.Memory.Poke(0x01FF, (byte)(returnAddress >> 8));
        machine.Cpu.P = (byte)(machine.Cpu.P & ~0x01);
        machine.Cpu.SetPC(address);
        machine.RunCycles(1);
    }

    private static void SetFilename(C64Machine machine, string name)
    {
        ushort address = 0x0370;

        machine.Memory.Poke(0xB7, (byte)name.Length);
        machine.Memory.Poke(0xBB, (byte)(address & 0xFF));
        machine.Memory.Poke(0xBC, (byte)(address >> 8));

        for (var i = 0; i < name.Length; i++)
            machine.Memory.Poke((ushort)(address + i), (byte)name[i]);
    }

    private static byte[] CreateTape(
        params (string Name, byte[] Payload)[] files)
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

            image[record] = 1;
            image[record + 1] = 0x82;
            WriteUInt16(image, record + 2, 0x0801);
            WriteUInt16(image, record + 4, 0x0801 + file.Payload.Length);
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
