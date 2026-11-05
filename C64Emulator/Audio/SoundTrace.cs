using System;
using System.IO;
using C64Emulator.Core;

namespace C64Emulator.Audio;

/// <summary>
/// Records every CPU write into the SID together with the writer's PC and
/// the processor-port value, which is what decides whether the game's
/// player is reading its shadow table from RAM or from BASIC ROM.
/// Enabled with the C64_SOUND_TRACE environment variable pointing at the
/// output file, so a normal interactive session can be traced.
/// </summary>
internal static class SoundTrace
{
    public static void AttachFromEnvironment(C64Machine machine)
    {
        if (machine is null)
            throw new ArgumentNullException(nameof(machine));

        var path = Environment.GetEnvironmentVariable("C64_SOUND_TRACE");

        if (!string.IsNullOrWhiteSpace(path))
            Attach(machine, path);
    }

    public static void Attach(C64Machine machine, string path)
    {
        // Diagnostic switch: the resident loader player calls the game's
        // music tick with BASIC banked in, which makes it read the ROM
        // copy of its shadow table. NOP that one JSR so the game's own
        // music engine can be heard, leaving the keyboard half of the
        // dispatcher working.
        var muteLoaderPlayer =
            Environment.GetEnvironmentVariable("C64_SOUND_MUTE_LOADER") == "1";
        var patched = false;

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        StreamWriter writer = new StreamWriter(fullPath, append: false)
        {
            AutoFlush = false
        };

        writer.WriteLine("# seq PC=$addr 01=$port D4xx=value");
        long writes = 0;

        machine.Sid.RegisterWriteHook = (register, value) =>
        {
            writes++;
            var pc = machine.Cpu.GetPC();
            var port = machine.Memory.Peek(0x0001);

            writer.WriteLine(
                $"{writes} PC={pc:X4} 01={port:X2} " +
                $"00={machine.Memory.Peek(0x0000):X2} " +
                $"D4{register:X2}={value:X2}");

            if ((writes & 0x3FF) == 0)
                writer.Flush();

            if (muteLoaderPlayer && !patched && pc is >= 0x85D0 and <= 0x85DF)
            {
                patched = true;
                var patchAddress = FindMusicCall(machine);

                if (patchAddress >= 0)
                {
                    machine.Memory.Poke((ushort)patchAddress, 0xEA);
                    machine.Memory.Poke((ushort)(patchAddress + 1), 0xEA);
                    machine.Memory.Poke((ushort)(patchAddress + 2), 0xEA);

                    writer.WriteLine(
                        $"# NOPed JSR $843E at ${patchAddress:X4}: " +
                        "resident loader music disabled");
                }
                else
                {
                    writer.WriteLine(
                        "# mute requested but no JSR $843E was found");
                }

                writer.Flush();
            }
        };

        // The engine's note state lives at $A851-$A8B2; recording its
        // writers shows whether a tune is ever started or only flushed.
        long tableWrites = 0;

        machine.Memory.TableWriteHook = (address, value) =>
        {
            tableWrites++;
            var pc = machine.Cpu.GetPC();

            writer.WriteLine(
                $"W{tableWrites} PC={pc:X4} " +
                $"01={machine.Memory.Peek(0x0001):X2} " +
                $"00={machine.Memory.Peek(0x0000):X2} " +
                $"A{address:X4}={value:X2}");

            if ((tableWrites & 0x0F) == 0)
                writer.Flush();
        };
    }

    /// <summary>
    /// Locates the dispatcher's music call by its opcode sequence rather
    /// than by a fixed address, so a relocated player is still handled.
    /// </summary>
    private static int FindMusicCall(C64Machine machine)
    {
        for (var address = 0x8000; address < 0x8800 - 2; address++)
        {
            if (
                machine.Memory.Read((ushort)address) == 0x20 &&
                machine.Memory.Read((ushort)(address + 1)) == 0x3E &&
                machine.Memory.Read((ushort)(address + 2)) == 0x84)
            {
                return address;
            }
        }

        return -1;
    }
}
