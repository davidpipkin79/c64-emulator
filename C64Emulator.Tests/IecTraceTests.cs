using System;
using System.Collections.Generic;
using System.IO;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Input;
using C64Emulator.Media;
using Xunit;

namespace C64Emulator.Tests;

public sealed class IecTraceTests
{
    [Fact]
    public void TraceLoadCommand()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        var driveRom = Path.Combine(romDirectory, "dos1541.rom");

        if (!File.Exists(driveRom))
            return;

        using C64Machine machine = new C64Machine(
            RomSet.Load(romDirectory),
            File.ReadAllBytes(driveRom));

        machine.TrueDriveEmulation = true;

        var imagePath = Path.Combine(
            Path.GetTempPath(),
            "c64-trace-" + Guid.NewGuid().ToString("N") + ".d64");

        machine.Drive.Mount(D64Image.FromBytes(CreateTestDisk()), imagePath);
        machine.Drive.Reset();
        machine.Cpu.Reset();

        WaitForReady(machine);

        Type(machine, "LOAD\"TEST\",8,1");

        Drive1541 drive = machine.Drive.RealDrive!;
        List<string> trace = new List<string>();
        long step = 0;
        var tracing = false;

        machine.Iec.LinesChanged += () =>
        {
            if (!tracing || trace.Count >= 4000)
                return;

            trace.Add(
                step.ToString() +
                " ATN=" + (machine.Iec.AtnLow ? "0" : "1") +
                " CLK=" + (machine.Iec.ClockLow ? "0" : "1") +
                " DAT=" + (machine.Iec.DataLow ? "0" : "1") +
                " C64PC=" + machine.Cpu.GetPC().ToString("X4") +
                " DRVPC=" + drive.Cpu.GetPC().ToString("X4") +
                " PB=" + drive.Via1.PortBOutput.ToString("X2") +
                " 7C=" + drive.Ram[0x7C].ToString("X2") +
                " 82=" + drive.Ram[0x82].ToString("X2") +
                " 83=" + drive.Ram[0x83].ToString("X2") +
                " 84=" + drive.Ram[0x84].ToString("X2") +
                " 85=" + drive.Ram[0x85].ToString("X2") +
                " 98=" + drive.Ram[0x98].ToString("X2"));
        };

        machine.Cia1.KeyEvent(
            new KeyboardEventArgs(Key.Return, ModifierKeys.None, true),
            true);

        for (step = 0; step < 200_000; step++)
        {
            machine.RunCycles(1);

            if (!tracing && machine.Iec.AtnLow)
                tracing = true;

            if (tracing && trace.Count >= 4000)
                break;
        }

        var path = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "diagnostics", "iec-trace.txt"));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, trace);

        machine.Drive.Eject();

        try
        {
            File.Delete(imagePath);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void TraceDriveProgress()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        var driveRom = Path.Combine(romDirectory, "dos1541.rom");

        if (!File.Exists(driveRom))
            return;

        using C64Machine machine = new C64Machine(
            RomSet.Load(romDirectory),
            File.ReadAllBytes(driveRom));

        machine.TrueDriveEmulation = true;

        var imagePath = Path.Combine(
            Path.GetTempPath(),
            "c64-progress-" + Guid.NewGuid().ToString("N") + ".d64");

        machine.Drive.Mount(D64Image.FromBytes(CreateTestDisk()), imagePath);
        machine.Drive.Reset();
        machine.Cpu.Reset();

        WaitForReady(machine);

        Type(machine, "LOAD\"TEST\",8,1");
        Press(machine, Key.Return, ModifierKeys.None);

        Drive1541 drive = machine.Drive.RealDrive!;
        List<string> trace = new List<string>();
        long lastBytes = 0;
        long lastSyncs = 0;
        long headers = 0;
        long validHeaders = 0;
        long dataReads = 0;
        var lastReads = string.Empty;

        drive.Via2.PortARead += value =>
        {
            dataReads++;
            lastReads = (lastReads + " " + value.ToString("X2"));

            if (lastReads.Length > 30)
                lastReads = lastReads[^30..];
        };
        var window = new byte[8];
        var windowFill = 0;

        drive.Disk.ByteProduced += value =>
        {
            if (windowFill < window.Length)
            {
                window[windowFill++] = value;

                if (windowFill == window.Length)
                {
                    if (window[0] == 0x52)
                    {
                        headers++;
                        Span<byte> decoded = stackalloc byte[6];
                        GcrCodec.Decode(window, decoded);
                        var checksum = (byte)(
                            decoded[2] ^
                            decoded[3] ^
                            decoded[4] ^
                            decoded[5]);

                        if (decoded[0] == 0x08 && decoded[1] == checksum)
                            validHeaders++;
                    }
                }
            }
            else
            {
                for (var i = 1; i < window.Length; i++)
                    window[i - 1] = window[i];

                window[^1] = value;

                if (window[0] == 0x52)
                {
                    headers++;
                    Span<byte> decoded = stackalloc byte[6];
                    GcrCodec.Decode(window, decoded);
                    var checksum = (byte)(
                        decoded[2] ^
                        decoded[3] ^
                        decoded[4] ^
                        decoded[5]);

                    if (decoded[0] == 0x08 && decoded[1] == checksum)
                        validHeaders++;
                }
            }
        };

        for (var chunk = 0; chunk < 200; chunk++)
        {
            machine.RunCycles(100_000);

            var bytes = drive.Disk.TotalBytesRead;
            var syncs = drive.Disk.TotalSyncs;
            var reads = dataReads;

            trace.Add(
                chunk.ToString() +
                " BYTES=" + (bytes - lastBytes) +
                " SYNCS=" + (syncs - lastSyncs) +
                " HDR52=" + headers +
                " HDRVALID=" + validHeaders +
                " PAREADS=" + reads +
                " C64PC=" + machine.Cpu.GetPC().ToString("X4") +
                " DRVPC=" + drive.Cpu.GetPC().ToString("X4") +
                " TRK=" + drive.Ram[0x80].ToString("X2") +
                " SEC=" + drive.Ram[0x81].ToString("X2") +
                " LINDX=" + drive.Ram[0x82].ToString("X2") +
                " 7C=" + drive.Ram[0x7C].ToString("X2") +
                " 6F=" + drive.Ram[0x6F].ToString("X2") +
                " J4=" + drive.Ram[0x04].ToString("X2") +
                " JOB=" + drive.Ram[0x45].ToString("X2") +
                " ID=" + drive.Ram[0x12].ToString("X2") + drive.Ram[0x13].ToString("X2") +
                " HEAD=" + drive.Disk.HeadHalfTrack +
                " DISKTRK=" + drive.Disk.CurrentTrack +
                " SYNC=" + drive.Disk.Sync +
                " BITS=" + drive.Ram[0x98].ToString("X2") +
                " TLK=" + drive.Ram[0x7A].ToString("X2") +
                " V2PB=" + drive.Via2.PortBOutput.ToString("X2"));

            lastBytes = bytes;
            lastSyncs = syncs;
            lastReads = string.Empty;
        }

        var path = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "diagnostics", "drive-progress.txt"));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, trace);

        machine.Drive.Eject();

        try
        {
            File.Delete(imagePath);
        }
        catch (IOException)
        {
        }
    }

    private static void WaitForReady(C64Machine machine)
    {
        ReadOnlySpan<byte> expected = [18, 5, 1, 4, 25, 46];

        for (var pass = 0; pass < 400; pass++)
        {
            machine.RunCycles(50_000);

            for (var address = 0x0400; address <= 0x07E8 - expected.Length; address++)
            {
                var matches = true;

                for (var i = 0; i < expected.Length; i++)
                {
                    if (machine.Memory.Peek((ushort)(address + i)) != expected[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                    return;
            }
        }

        Assert.Fail("The C64 did not reach READY.");
    }

    private static void Type(C64Machine machine, string text)
    {
        foreach (var character in text)
        {
            switch (character)
            {
                case ' ':
                    Press(machine, Key.Space, ModifierKeys.None);
                    break;

                case '"':
                    machine.Cia1.KeyEvent(
                        new KeyboardEventArgs(Key.LeftShift, ModifierKeys.LeftShift, true),
                        true);
                    machine.RunCycles(30_000);
                    Press(machine, Key.Two, ModifierKeys.LeftShift);
                    machine.Cia1.KeyEvent(
                        new KeyboardEventArgs(Key.LeftShift, ModifierKeys.LeftShift, false),
                        true);
                    machine.RunCycles(30_000);
                    break;

                case ',':
                    Press(machine, Key.Comma, ModifierKeys.None);
                    break;

                case '8':
                    Press(machine, Key.Eight, ModifierKeys.None);
                    break;

                case '1':
                    Press(machine, Key.One, ModifierKeys.None);
                    break;

                default:
                    Press(
                        machine,
                        (Key)Enum.Parse(typeof(Key), character.ToString()),
                        ModifierKeys.None);
                    break;
            }
        }
    }

    private static void Press(
        C64Machine machine,
        Key key,
        ModifierKeys modifiers)
    {
        machine.Cia1.KeyEvent(
            new KeyboardEventArgs(key, modifiers, true),
            true);
        machine.RunCycles(30_000);

        machine.Cia1.KeyEvent(
            new KeyboardEventArgs(key, modifiers, false),
            true);
        machine.RunCycles(30_000);
    }

    private static byte[] CreateTestDisk()
    {
        var image = new byte[174848];
        var bam = SectorOffset(18, 0);

        image[bam] = 18;
        image[bam + 1] = 1;
        image[bam + 2] = 0x41;
        image[bam + 3] = 0x00;

        for (var track = 1; track <= 35; track++)
        {
            var entry = bam + 4 + ((track - 1) * 4);
            var sectors = D64Image.SectorsPerTrack(track);

            image[entry] = (byte)sectors;
            image[entry + 1] = 0xFF;
            image[entry + 2] = 0xFF;
            image[entry + 3] = (byte)((1 << (sectors - 16)) - 1);
        }

        MarkUsed(image, 18, 0);
        MarkUsed(image, 18, 1);
        MarkUsed(image, 1, 0);

        WritePaddedName(image, bam + 0x90, 16, "TEST DISK");
        image[bam + 0xA2] = (byte)'0';
        image[bam + 0xA3] = (byte)'1';
        image[bam + 0xA5] = (byte)'2';
        image[bam + 0xA6] = (byte)'A';
        image[bam + 0xA7] = 0xA0;
        image[bam + 0xA8] = 0xA0;
        image[bam + 0xA9] = 0xA0;
        image[bam + 0xAA] = 0xA0;

        var directory = SectorOffset(18, 1);
        image[directory] = 0;
        image[directory + 1] = 0;
        image[directory + 2] = 0x82;
        image[directory + 3] = 1;
        image[directory + 4] = 0;
        WritePaddedName(image, directory + 5, 16, "TEST");
        image[directory + 30] = 1;
        image[directory + 31] = 0;

        var file = SectorOffset(1, 0);
        image[file] = 0;
        image[file + 1] = 0;
        image[file + 2] = 0x00;
        image[file + 3] = 0xC0;

        for (var i = 0; i < 252; i++)
            image[file + 4 + i] = (byte)(0x41 + (i % 26));

        return image;
    }

    private static void MarkUsed(byte[] image, int track, int sector)
    {
        var bam = SectorOffset(18, 0);
        var entry = bam + 4 + ((track - 1) * 4);

        image[entry]--;
        image[entry + 1 + (sector / 8)] &= (byte)~(1 << (sector % 8));
    }

    private static void WritePaddedName(
        byte[] image,
        int offset,
        int length,
        string name)
    {
        for (var i = 0; i < length; i++)
            image[offset + i] = i < name.Length ? (byte)name[i] : (byte)0xA0;
    }

    private static int SectorOffset(int track, int sector)
    {
        var offset = 0;

        for (var current = 1; current < track; current++)
            offset += D64Image.SectorsPerTrack(current) * D64Image.SectorSize;

        return offset + (sector * D64Image.SectorSize);
    }
}
