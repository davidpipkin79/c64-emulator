using System;
using System.IO;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class BootProbe
    {
        [Fact]
        public void BootsToReadyPrompt()
        {
            string romDirectory = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "C64Emulator", "Roms"));

            string kernal = Path.Combine(romDirectory, "kernal.rom");
            string basic = Path.Combine(romDirectory, "basic.rom");
            string character = Path.Combine(romDirectory, "char.rom");

            if (
                !File.Exists(kernal) ||
                !File.Exists(basic) ||
                !File.Exists(character))
            {
                return;
            }

            using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));

            machine.Cpu.Reset();

            // The KERNAL idles in its STOP/keyboard loop after READY, so a
            // fixed wall-clock delay is both slow and flaky. Run a bounded
            // number of cycles and inspect the screen as the boot progresses.
            for (int pass = 0; pass < 200; pass++)
            {
                machine.RunCycles(50_000);

                if (ContainsReady(machine.Memory))
                    return;
            }

            Assert.Fail(
                "The C64 did not reach READY within the boot budget. " +
                "PC=" + machine.Cpu.GetPC().ToString("X4"));
        }

        private static bool ContainsReady(Memory memory)
        {
            ReadOnlySpan<byte> expected = stackalloc byte[] { 18, 5, 1, 4, 25, 46 };

            for (int address = 0x0400; address <= 0x07E8 - expected.Length; address++)
            {
                bool matches = true;

                for (int i = 0; i < expected.Length; i++)
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
    }
}
