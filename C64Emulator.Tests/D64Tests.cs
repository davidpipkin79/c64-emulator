using System;
using System.IO;
using C64Emulator;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Media;
using Xunit;

namespace C64Emulator.Tests;

public sealed class D64Tests
{
    [Fact]
    public void ReadsDirectoryAndFile()
    {
        var image = CreateImage(
            ("GAME", new byte[] { 0x01, 0x08, 0x41, 0x42 }),
            ("README", new byte[] { 0x01, 0x08, 0x43 }));

        D64Image disk = D64Image.FromBytes(image);

        Assert.Equal("TESTDISK", disk.DiskName);
        Assert.Equal(2, disk.Directory.Count);
        Assert.Equal("GAME", disk.Directory[0].Name);
        Assert.Equal(CbmFileType.Program, disk.Directory[0].Type);
        Assert.Equal(1, disk.Directory[0].Blocks);
        Assert.Equal(new byte[] { 0x01, 0x08, 0x41, 0x42 }, disk.ReadFile(disk.Directory[0]));
    }

    [Fact]
    public void LastSectorPaddingStaysInMemoryButNotInTheEndPointer()
    {
        var image = CreateImage(("GAME", new byte[] { 0x01, 0x08, 0x41, 0x42 }));
        image[Offset(1, 0) + 2 + 4] = 0xAA;

        D64Image disk = D64Image.FromBytes(image);
        D64FileInfo file = disk.Directory[0];

        Assert.Equal(new byte[] { 0x01, 0x08, 0x41, 0x42 }, disk.ReadFile(file));

        byte[] padded = disk.ReadFileWithPadding(file, out var usedLength);
        Assert.Equal(4, usedLength);
        Assert.Equal(D64Image.SectorSize - 2, padded.Length);
        Assert.Equal(0xAA, padded[4]);
    }

    [Fact]
    public void KernalLoadWritesTheLastSectorTail()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateImage(("GAME", new byte[] { 0x01, 0x08, 0x41, 0x42 }));
        image[Offset(1, 0) + 2 + 4] = 0xAA;

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Drive.Mount(D64Image.FromBytes(image));
        WriteFilename(machine, "GAME");
        machine.Memory.Poke(0xB9, 1);
        machine.Memory.Poke(0xBA, 8);
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.A = 0;
        machine.Cpu.SetPC(0xFFD5);

        machine.RunCycles(1);

        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0xAA, machine.Memory.Peek(0x0803));
        Assert.Equal(0x03, machine.Memory.Peek(0xC3));
        Assert.Equal(0x08, machine.Memory.Peek(0xC4));
    }

    [Fact]
    public void WildcardAndFirstProgramLookupsWork()
    {
        var image = CreateImage(
            ("LOADER", new byte[] { 0x01, 0x08, 0x01 }),
            ("GAME", new byte[] { 0x01, 0x08, 0x02 }));

        D64Image disk = D64Image.FromBytes(image);

        Assert.Equal("GAME", disk.FindFile("GAME*")?.Name);
        Assert.Equal("GAME", disk.FindFile("?AME")?.Name);
        Assert.Equal("LOADER", disk.FirstProgram()?.Name);
    }

    [Fact]
    public void DriveMountsAndEjectsImages()
    {
        D64Image disk = D64Image.FromBytes(
            CreateImage(("GAME", new byte[] { 0x01, 0x08, 0x41 })));

        DiskDrive drive = new DiskDrive();
        drive.Mount(disk, "game.d64");

        Assert.True(drive.IsMounted);
        Assert.Equal("game.d64", drive.ImagePath);
        Assert.Single(drive.Directory);
        Assert.Equal("GAME", drive.FindFile("*")?.Name);

        drive.Eject();
        Assert.False(drive.IsMounted);
    }

    [Fact]
    public void RejectsInvalidImages()
    {
        Assert.Throws<InvalidDataException>(() => D64Image.FromBytes(new byte[100]));
    }

    [Fact]
    public void SaveFileWritesDirectoryAndPersists()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "c64-save-" + Guid.NewGuid().ToString("N") + ".d64");
        var imageBytes = CreateWritableDisk();

        try
        {
            File.WriteAllBytes(path, imageBytes);
            D64Image disk = D64Image.Load(path);
            byte[] program = [0x01, 0x08, 0x41, 0x42, 0x43];

            disk.SaveFile("NEW", program);

            Assert.True(disk.IsDirty);
            D64FileInfo saved = Assert.Single(disk.Directory);
            Assert.Equal("NEW", saved.Name);
            Assert.Equal(program, disk.ReadFile(saved));

            disk.Save(path);
            D64Image reloaded = D64Image.Load(path);
            D64FileInfo? reloadedFile = reloaded.FindFile("NEW");

            Assert.NotNull(reloadedFile);
            Assert.Equal(program, reloaded.ReadFile(reloadedFile));
            Assert.False(reloaded.IsDirty);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveFileWritesAndReadsAMultiSectorChain()
    {
        D64Image disk = D64Image.FromBytes(CreateWritableDisk());
        byte[] program = new byte[602];
        program[0] = 0x01;
        program[1] = 0x08;

        for (var i = 2; i < program.Length; i++)
            program[i] = (byte)(i & 0xFF);

        disk.SaveFile("BIG", program);
        D64FileInfo saved = Assert.Single(disk.Directory);

        Assert.Equal(3, saved.Blocks);
        Assert.Equal(program, disk.ReadFile(saved));
    }

    [Fact]
    public void KernalSaveTrapWritesProgramToMountedDisk()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Drive.Mount(D64Image.FromBytes(CreateWritableDisk()));
        WriteFilename(machine, "NEW");
        machine.Memory.Poke(0xBA, 8);

        machine.Memory.Poke(0xFB, 0x01);
        machine.Memory.Poke(0xFC, 0x08);
        machine.Memory.Poke(0x0801, 0x41);
        machine.Memory.Poke(0x0802, 0x42);
        machine.Memory.Poke(0x0803, 0x43);
        machine.Memory.Poke(0x0804, 0x44);
        machine.Memory.Poke(0x0805, 0x45);

        machine.Cpu.A = 0xFB;
        machine.Cpu.X = 0x06;
        machine.Cpu.Y = 0x08;
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.SetPC(0xFFD8);

        machine.RunCycles(1);

        Assert.Equal(0x1235, machine.Cpu.GetPC());
        Assert.Equal(0, machine.Cpu.P & 0x01);
        D64FileInfo? saved = machine.Drive.FindFile("NEW");

        Assert.NotNull(saved);
        Assert.Equal(
            new byte[] { 0x01, 0x08, 0x41, 0x42, 0x43, 0x44, 0x45 },
            machine.Drive.ReadFile(saved));
    }

    [Fact]
    public void MenuLoadStartsAProgramFromTheMountedDisk()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateImage(
            ("GAME", new byte[] { 0x01, 0x08, 0x41, 0x42 }));

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Drive.Mount(D64Image.FromBytes(image));
        machine.LoadDiskProgram("GAME");

        Assert.Equal(0xA659, machine.Cpu.GetPC());
        Assert.Equal(0x01, machine.Memory.Peek(0x2B));
        Assert.Equal(0x08, machine.Memory.Peek(0x2C));
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0x01, machine.Memory.Peek(0xC1));
        Assert.Equal(0x08, machine.Memory.Peek(0xC2));
        Assert.Equal(0x03, machine.Memory.Peek(0xC3));
        Assert.Equal(0x08, machine.Memory.Peek(0xC4));
        Assert.Equal(0x03, machine.Memory.Peek(0xAE));
        Assert.Equal(0x08, machine.Memory.Peek(0xAF));
        Assert.Equal(0x08, machine.Memory.Peek(0xBA));
    }

    [Fact]
    public void MenuLoadedBasicProgramRunsAfterBoot()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        // 10 PRINT "HI" followed by the BASIC end marker.
        byte[] basicProgram =
        [
            0x01, 0x08,
            0x0B, 0x08, 0x0A, 0x00,
            0x99, 0x22, 0x48, 0x49, 0x22, 0x00,
            0x00, 0x00
        ];

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Cpu.Reset();

        for (var pass = 0; pass < 200; pass++)
        {
            machine.RunCycles(50_000);

            if (ContainsScreenCodes(machine.Memory, 18, 5, 1, 4, 25, 46))
                break;
        }

        Assert.True(ContainsScreenCodes(machine.Memory, 18, 5, 1, 4, 25, 46));

        D64Image disk = D64Image.FromBytes(CreateImage(("GAME", basicProgram)));
        machine.Drive.Mount(disk);
        machine.LoadDiskProgram("GAME");

        for (var pass = 0; pass < 200; pass++)
        {
            machine.RunCycles(50_000);

            if (ContainsScreenCodes(machine.Memory, 8, 9))
                return;
        }

        Assert.Fail("The BASIC program loaded from the disk did not run.");
    }

    [Fact]
    public void KernalLoadTrapLoadsAProgramFromTheMountedDisk()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var image = CreateImage(
            ("GAME", new byte[] { 0x01, 0x08, 0x41, 0x42 }));

        using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
        machine.Drive.Mount(D64Image.FromBytes(image));

        WriteFilename(machine, "GAME");
        machine.Memory.Poke(0xB9, 1); // load at the file's address
        machine.Memory.Poke(0xBA, 8);

        // Simulate the return address pushed by JSR $FFD5. JSR pushes
        // high then low, so the low byte is at $01FE and S is $FD.
        machine.Cpu.S = 0xFD;
        machine.Memory.Poke(0x01FE, 0x34);
        machine.Memory.Poke(0x01FF, 0x12);
        machine.Cpu.A = 0;
        machine.Cpu.SetPC(0xFFD5);

        machine.RunCycles(1);

        Assert.Equal(0x1235, machine.Cpu.GetPC());
        Assert.Equal(0x41, machine.Memory.Peek(0x0801));
        Assert.Equal(0x42, machine.Memory.Peek(0x0802));
        Assert.Equal(0x01, machine.Memory.Peek(0xC1));
        Assert.Equal(0x08, machine.Memory.Peek(0xC2));
        Assert.Equal(0x03, machine.Memory.Peek(0xC3));
        Assert.Equal(0x08, machine.Memory.Peek(0xC4));
        Assert.Equal(0x03, machine.Cpu.X);
        Assert.Equal(0x08, machine.Cpu.Y);
        Assert.Equal(0, machine.Cpu.P & 0x01);
    }

    private static bool ContainsScreenCodes(Memory memory, params byte[] expected)
    {
        for (var address = 0x0400; address <= 0x07E8 - expected.Length; address++)
        {
            var matches = true;

            for (var i = 0; i < expected.Length; i++)
            {
                if (memory.Peek((ushort)(address + i)) != expected[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                return true;
        }

        return false;
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

    private static byte[] CreateWritableDisk()
    {
        var image = CreateImage();
        var bam = Offset(18, 0);

        for (var track = 1; track <= 35; track++)
        {
            var entry = bam + 4 + ((track - 1) * 4);
            var sectors = D64Image.SectorsPerTrack(track);

            image[entry] = (byte)sectors;

            for (var sector = 0; sector < sectors; sector++)
            {
                image[entry + 1 + (sector >> 3)] |=
                    (byte)(1 << (sector & 7));
            }
        }

        return image;
    }

    private static byte[] CreateImage(params (string Name, byte[] Data)[] files)
    {
        var image = new byte[174848];
        WritePetscii(image, Offset(18, 0) + 0x90, 16, "TESTDISK");
        image[Offset(18, 0) + 0xA2] = (byte)'0';
        image[Offset(18, 0) + 0xA3] = (byte)'1';

        var directory = Offset(18, 1);
        image[directory] = 0;
        image[directory + 1] = 0xFF;

        for (var i = 0; i < files.Length; i++)
        {
            var entry = directory + (i * 32);

            image[entry + 2] = 0x82; // closed PRG
            image[entry + 3] = 1;
            image[entry + 4] = (byte)i;
            WritePetscii(image, entry + 5, 16, files[i].Name);
            image[entry + 30] = 1;

            var sector = Offset(1, i);
            image[sector] = 0;
            // The final-sector count includes the track link byte.
            image[sector + 1] = (byte)(files[i].Data.Length + 1);
            files[i].Data.CopyTo(image, sector + 2);
        }

        return image;
    }

    private static int Offset(int track, int sector)
    {
        var offset = 0;

        for (var current = 1; current < track; current++)
            offset += D64Image.SectorsPerTrack(current) * D64Image.SectorSize;

        return offset + (sector * D64Image.SectorSize);
    }

    private static void WritePetscii(byte[] image, int offset, int length, string value)
    {
        for (var i = 0; i < length; i++)
            image[offset + i] = i < value.Length ? (byte)value[i] : (byte)0xA0;
    }
}
