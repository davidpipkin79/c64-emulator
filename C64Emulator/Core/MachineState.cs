using System;
using System.IO;
using System.Reflection;
using C64Emulator.Audio;
using C64Emulator.Cpu;
using C64Emulator.Devices;
using C64Emulator.Video;

namespace C64Emulator.Core;

/// <summary>
/// Versioned full-machine snapshot. The host pauses the emulation thread
/// before calling this, so every chip is captured between instructions.
/// The VIC framebuffer itself is not part of the state; it is cleared and
/// redrawn after a restore.
/// </summary>
internal static class MachineState
{
    // "C64S" in little-endian byte order.
    private const uint Magic = 0x53343643;
    private const int Version = 1;

    private const BindingFlags FieldFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    public static byte[] Capture(C64Machine machine, bool flushMedia)
    {
        if (flushMedia)
            machine.Drive.Flush();

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(machine.FrameCycles);

        StateSerializer.WriteString(writer, machine.Drive.ImagePath);
        StateSerializer.WriteString(writer, machine.Tape.ImagePath);
        StateSerializer.WriteString(writer, machine.Cartridge?.SourcePath);
        writer.Write(machine.Drive.TrueDriveEmulation);

        WriteCpu(writer, machine.Cpu);
        StateSerializer.WriteFields(writer, machine.Memory);
        StateSerializer.WriteFields(writer, machine.ColorRam);
        WriteVic(writer, machine.Vic);
        WriteSid(writer, machine.Sid);
        StateSerializer.WriteFields(
            writer,
            machine.Cia1,
            "keyset",
            "interruptSource",
            "useNmi");
        StateSerializer.WriteFields(
            writer,
            machine.Cia2,
            "interruptSource",
            "useNmi");
        StateSerializer.WriteFields(writer, machine.Interrupts);
        StateSerializer.WriteFields(writer, machine.Iec);
        WriteDrive(writer, machine.Drive);
        machine.WriteKernalState(writer);
        machine.Tape.WritePlayback(writer);

        writer.Flush();
        return stream.ToArray();
    }

    public static void Restore(C64Machine machine, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(machine);
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream stream = new(data, writable: false);
        using BinaryReader reader = new(stream);

        if (reader.ReadUInt32() != Magic)
            throw new InvalidDataException("Not a C64 save state.");

        int version = reader.ReadInt32();

        if (version != Version)
        {
            throw new InvalidDataException(
                $"Unsupported save-state version {version} (expected {Version}).");
        }

        int frameCycles = reader.ReadInt32();
        string? diskPath = StateSerializer.ReadString(reader);
        string? tapePath = StateSerializer.ReadString(reader);
        string? cartridgePath = StateSerializer.ReadString(reader);
        bool trueDrive = reader.ReadBoolean();

        RestoreMedia(machine, diskPath, tapePath, cartridgePath, trueDrive);

        ReadCpu(reader, machine.Cpu);
        StateSerializer.ReadFields(reader, machine.Memory);
        machine.Memory.RefreshConfiguration();
        StateSerializer.ReadFields(reader, machine.ColorRam);
        ReadVic(reader, machine.Vic);
        ReadSid(reader, machine.Sid);
        StateSerializer.ReadFields(
            reader,
            machine.Cia1,
            "keyset",
            "interruptSource",
            "useNmi");
        StateSerializer.ReadFields(
            reader,
            machine.Cia2,
            "interruptSource",
            "useNmi");
        machine.Cia2.RefreshPorts();
        StateSerializer.ReadFields(reader, machine.Interrupts);
        StateSerializer.ReadFields(reader, machine.Iec);
        ReadDrive(reader, machine.Drive);
        machine.ReadKernalState(reader);

        // Older states ended here and restart the tape at its leader.
        if (stream.Position < stream.Length)
            machine.Tape.ReadPlayback(reader);
        else
            machine.Tape.ResetPlayback();

        machine.RefreshTapeSense();

        machine.FrameCycles = frameCycles;
        machine.Vic.ClearFrameBuffers();
        machine.Sid.DiscardSamples();
        machine.ResetFrameReady();
    }

    private static void RestoreMedia(
        C64Machine machine,
        string? diskPath,
        string? tapePath,
        string? cartridgePath,
        bool trueDrive)
    {
        if (diskPath is null)
        {
            if (machine.Drive.IsMounted)
                machine.Drive.Eject();
        }
        else if (
            !string.Equals(
                machine.Drive.ImagePath,
                diskPath,
                StringComparison.OrdinalIgnoreCase))
        {
            machine.Drive.Mount(diskPath);
        }

        machine.Drive.TrueDriveEmulation = trueDrive;

        if (tapePath is null)
        {
            if (machine.Tape.IsMounted)
                machine.Tape.Eject();
        }
        else if (
            !string.Equals(
                machine.Tape.ImagePath,
                tapePath,
                StringComparison.OrdinalIgnoreCase))
        {
            machine.Tape.Mount(tapePath);
        }

        if (cartridgePath is null)
        {
            if (machine.Cartridge is not null)
                machine.Memory.AttachCartridge(null);
        }
        else if (
            !string.Equals(
                machine.Cartridge?.SourcePath,
                cartridgePath,
                StringComparison.OrdinalIgnoreCase))
        {
            machine.Memory.AttachCartridge(Cartridge.Load(cartridgePath));
        }
    }

    private static void WriteCpu(BinaryWriter writer, Cpu6510 cpu)
    {
        Cpu6510.Registers registers =
            (Cpu6510.Registers)GetField(cpu, "registers");

        writer.Write(registers.PC);
        writer.Write(registers.S);
        writer.Write(registers.P);
        writer.Write(registers.A);
        writer.Write(registers.X);
        writer.Write(registers.Y);
        writer.Write((bool)GetField(cpu, "jammed"));
    }

    private static void ReadCpu(BinaryReader reader, Cpu6510 cpu)
    {
        Cpu6510.Registers registers = new()
        {
            PC = reader.ReadUInt16(),
            S = reader.ReadByte(),
            P = reader.ReadByte(),
            A = reader.ReadByte(),
            X = reader.ReadByte(),
            Y = reader.ReadByte()
        };

        typeof(Cpu6510)
            .GetField("registers", FieldFlags)!
            .SetValue(cpu, registers);
        typeof(Cpu6510)
            .GetField("jammed", FieldFlags)!
            .SetValue(cpu, reader.ReadBoolean());
    }

    private static void WriteVic(BinaryWriter writer, Vic vic)
    {
        StateSerializer.WriteFields(writer, vic, "sprites");

        var sprites = (Array)GetField(vic, "sprites");

        foreach (object? value in sprites)
        {
            Sprite sprite = (Sprite)value!;
            writer.Write(sprite.X);
            writer.Write(sprite.Y);
            writer.Write(sprite.Enabled);
            writer.Write(sprite.Width);
            writer.Write(sprite.Height);
            writer.Write(sprite.PixelScale);
            writer.Write(sprite.MulticolorMode);
            writer.Write(sprite.DoubleWidth);
            writer.Write(sprite.TopMost);
            writer.Write(sprite.SpriteColor.R);
            writer.Write(sprite.SpriteColor.G);
            writer.Write(sprite.SpriteColor.B);
        }
    }

    private static void ReadVic(BinaryReader reader, Vic vic)
    {
        StateSerializer.ReadFields(reader, vic, "sprites");

        var sprites = (Array)GetField(vic, "sprites");

        for (var i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Enabled = reader.ReadBoolean(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                PixelScale = reader.ReadInt32(),
                MulticolorMode = reader.ReadBoolean(),
                DoubleWidth = reader.ReadBoolean(),
                TopMost = reader.ReadBoolean(),
                SpriteColor = new RgbColor(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte())
            };

            sprites.SetValue(sprite, i);
        }

        // The per-line bus and visibility masks are derived caches. Rebuild
        // them from the restored raster position and sprite registers.
        vic.RefreshSpriteCachesForState();
    }

    private static void WriteSid(BinaryWriter writer, Sid sid)
    {
        StateSerializer.WriteFields(
            writer,
            sid,
            "voices",
            "filter",
            "scratch",
            "queue",
            "queueHead",
            "queueTail",
            "queueCount",
            "outputVolume",
            "muted");

        var voices = (Array)GetField(sid, "voices");

        foreach (object? voice in voices)
            StateSerializer.WriteFields(writer, voice!);

        StateSerializer.WriteFields(writer, GetField(sid, "filter"));
    }

    private static void ReadSid(BinaryReader reader, Sid sid)
    {
        StateSerializer.ReadFields(
            reader,
            sid,
            "voices",
            "filter",
            "scratch",
            "queue",
            "queueHead",
            "queueTail",
            "queueCount",
            "outputVolume",
            "muted");

        var voices = (Array)GetField(sid, "voices");

        foreach (object? voice in voices)
            StateSerializer.ReadFields(reader, voice!);

        StateSerializer.ReadFields(reader, GetField(sid, "filter"));
    }

    private static void WriteDrive(BinaryWriter writer, DiskDrive drive)
    {
        Drive1541? realDrive = drive.RealDrive;
        writer.Write(realDrive is not null);

        if (realDrive is null)
            return;

        StateSerializer.WriteFields(writer, realDrive, "rom");
        StateSerializer.WriteFields(writer, realDrive.Interrupts);
        WriteCpu(writer, realDrive.Cpu);
        StateSerializer.WriteFields(writer, realDrive.Via1);
        StateSerializer.WriteFields(writer, realDrive.Via2);
        StateSerializer.WriteFields(writer, realDrive.Disk, "imagePath");
    }

    private static void ReadDrive(BinaryReader reader, DiskDrive drive)
    {
        if (!reader.ReadBoolean())
            return;

        Drive1541 realDrive = drive.RealDrive ??
            throw new InvalidDataException(
                "The save state contains a hardware 1541, but no drive ROM is loaded.");

        StateSerializer.ReadFields(reader, realDrive, "rom");
        StateSerializer.ReadFields(reader, realDrive.Interrupts);
        ReadCpu(reader, realDrive.Cpu);
        StateSerializer.ReadFields(reader, realDrive.Via1);
        StateSerializer.ReadFields(reader, realDrive.Via2);
        StateSerializer.ReadFields(reader, realDrive.Disk, "imagePath");
        realDrive.Disk.RebuildTracks();
    }

    private static object GetField(object target, string name)
    {
        FieldInfo? field = target
            .GetType()
            .GetField(name, FieldFlags);

        if (field is null)
        {
            throw new InvalidOperationException(
                $"State field {target.GetType().Name}.{name} was not found.");
        }

        return field.GetValue(target)!;
    }
}
