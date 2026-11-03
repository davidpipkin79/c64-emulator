using System;
using System.IO;
using System.Text;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class DiskIntegrationTests
    {
        [Fact]
        public void KernalLoadUsesTheMountedDisk()
        {
            string romDirectory = GetRomDirectory();

            if (
                !File.Exists(Path.Combine(romDirectory, "kernal.rom")) ||
                !File.Exists(Path.Combine(romDirectory, "basic.rom")) ||
                !File.Exists(Path.Combine(romDirectory, "char.rom")))
            {
                return;
            }

            using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));

            machine.Drive.Mount(D64Image.FromBytes(CreateTestDisk()));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);

            Type(machine, "LOAD\"TEST\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            bool loaded = WaitUntil(
                machine,
                25_000_000,
                () =>
                    machine.Memory.Peek(0xC000) == 0x41 &&
                    machine.Memory.Peek(0xC001) == 0x42);

            Assert.True(loaded, Failure(machine, "LOAD did not complete"));
            Assert.Equal(0x00, machine.Memory.Peek(0xC1));
            Assert.Equal(0xC0, machine.Memory.Peek(0xC2));
        }

        [Fact]
        public void RealDriveStartsTheKernalLoadHandshake()
        {
            string romDirectory = GetRomDirectory();
            string driveRom = Path.Combine(romDirectory, "dos1541.rom");

            if (
                !File.Exists(Path.Combine(romDirectory, "kernal.rom")) ||
                !File.Exists(Path.Combine(romDirectory, "basic.rom")) ||
                !File.Exists(Path.Combine(romDirectory, "char.rom")) ||
                !File.Exists(driveRom))
            {
                return;
            }

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(driveRom));

            machine.TrueDriveEmulation = true;
            Assert.True(machine.Drive.HardwareDriveActive);

            machine.Drive.Mount(D64Image.FromBytes(CreateTestDisk()));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);

            Type(machine, "LOAD\"TEST\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            // The DOS completes the OPEN/TALK handshake and starts sending
            // the file. The default virtual path handles the complete load.
            bool started = WaitUntil(
                machine,
                25_000_000,
                () =>
                    machine.Memory.Peek(0xC000) == 0x41 &&
                    machine.Memory.Peek(0xC001) == 0x42);

            Assert.True(
                started,
                Failure(machine, "The real drive did not start the load"));
        }

        private static void WaitForReady(C64Machine machine)
        {
            for (int pass = 0; pass < 400; pass++)
            {
                machine.RunCycles(50_000);

                if (ContainsReady(machine.Memory))
                    return;
            }

            Assert.Fail(Failure(machine, "The C64 did not reach READY"));
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

        private static bool WaitUntil(
            C64Machine machine,
            int cycles,
            Func<bool> condition)
        {
            const int chunk = 50_000;

            for (int executed = 0; executed < cycles; executed += chunk)
            {
                if (condition())
                    return true;

                machine.RunCycles(chunk);
            }

            return condition();
        }

        private static void Type(C64Machine machine, string text)
        {
            foreach (char character in text)
            {
                switch (character)
                {
                    case ' ':
                        Press(machine, Key.Space, ModifierKeys.None);
                        break;

                    case '"':
                        PressShifted(machine, Key.Two);
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
                        if (character is >= 'A' and <= 'Z')
                        {
                            Press(
                                machine,
                                (Key)Enum.Parse(typeof(Key), character.ToString()),
                                ModifierKeys.None);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(
                                nameof(text),
                                "No test key mapping for '" + character + "'.");
                        }
                        break;
                }
            }
        }

        private static void PressShifted(C64Machine machine, Key key)
        {
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.LeftShift, ModifierKeys.LeftShift, true),
                true);
            machine.RunCycles(30_000);

            Press(machine, key, ModifierKeys.LeftShift);

            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.LeftShift, ModifierKeys.LeftShift, false),
                true);
            machine.RunCycles(30_000);
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

        private static string Failure(C64Machine machine, string message)
        {
            Drive1541? drive = machine.Drive.RealDrive;
            StringBuilder text = new StringBuilder(message);

            text.Append(" C64PC=").Append(machine.Cpu.GetPC().ToString("X4"));
            text.Append(" CIA2=").Append(machine.Cia2.Peek(0).ToString("X2"));
            text.Append(" KB=").Append(machine.Memory.Peek(0xC6).ToString("X2"));
            text.Append(" SCREEN=").Append(ReadScreen(machine.Memory));
            text.Append(" ATN=").Append(machine.Iec.AtnLow ? "L" : "H");
            text.Append(" CLK=").Append(machine.Iec.ClockLow ? "L" : "H");
            text.Append(" DATA=").Append(machine.Iec.DataLow ? "L" : "H");

            if (drive is not null)
            {
                text.Append(" DRVPC=").Append(drive.Cpu.GetPC().ToString("X4"));
                text.Append(" A=").Append(drive.Cpu.A.ToString("X2"));
                text.Append(" X=").Append(drive.Cpu.X.ToString("X2"));
                text.Append(" Y=").Append(drive.Cpu.Y.ToString("X2"));
                text.Append(" S=").Append(drive.Cpu.S.ToString("X2"));
                text.Append(" P=").Append(drive.Cpu.P.ToString("X2"));
                text.Append(" TRK=").Append(drive.Ram[0x22].ToString("X2"));
                text.Append(" ATNIN=").Append(drive.Ram[0x7C].ToString("X2"));
                text.Append(" EOI=").Append(drive.Ram[0x7D].ToString("X2"));
                text.Append(" CH=").Append(drive.Ram[0x82].ToString("X2"));
                text.Append(" SEC=").Append(drive.Ram[0x83].ToString("X2"));
                text.Append(" CMD=").Append(drive.Ram[0x84].ToString("X2"));
                text.Append(" TLK=").Append(drive.Ram[0x7A].ToString("X2"));
                text.Append(" LSN=").Append(drive.Ram[0x79].ToString("X2"));
                text.Append(" V1PA=").Append(drive.Via1.PortAOutput.ToString("X2"));
                text.Append(" DB=").Append(drive.Ram[0x85].ToString("X2"));
                text.Append(" ST=").Append(drive.Ram[0x90].ToString("X2"));
                text.Append(" V1PB=").Append(drive.Via1.PortBOutput.ToString("X2"));
                text.Append(" V1DDR=").Append(drive.Via1.DdrB.ToString("X2"));
                text.Append(" V2PB=").Append(drive.Via2.PortBOutput.ToString("X2"));
                text.Append(" V2DDR=").Append(drive.Via2.DdrB.ToString("X2"));
                text.Append(" ACR=").Append(drive.Via2.Acr.ToString("X2"));
            }

            return text.ToString();
        }

        private static string ReadScreen(Memory memory)
        {
            StringBuilder text = new StringBuilder();

            for (int address = 0x0400; address < 0x07E8; address++)
            {
                byte value = memory.Peek((ushort)address);
                char character = value switch
                {
                    0 => ' ',
                    >= 0x01 and <= 0x1A => (char)('A' + value - 1),
                    >= 0x20 and <= 0x5F => (char)value,
                    >= 0xC1 and <= 0xDA => (char)(value - 0x80),
                    _ => '.'
                };

                text.Append(character);
            }

            return text.ToString().Trim();
        }

        private static byte[] CreateTestDisk()
        {
            byte[] image = new byte[174848];
            int bam = SectorOffset(18, 0);

            image[bam] = 18;
            image[bam + 1] = 1;
            image[bam + 2] = 0x41;
            image[bam + 3] = 0x00;

            for (int track = 1; track <= 35; track++)
            {
                int entry = bam + 4 + ((track - 1) * 4);
                int sectors = D64Image.SectorsPerTrack(track);

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

            int directory = SectorOffset(18, 1);
            image[directory] = 0;
            image[directory + 1] = 0;
            image[directory + 2] = 0x82;
            image[directory + 3] = 1;
            image[directory + 4] = 0;
            WritePaddedName(image, directory + 5, 16, "TEST");
            image[directory + 30] = 1;
            image[directory + 31] = 0;

            int file = SectorOffset(1, 0);
            image[file] = 0;
            image[file + 1] = 0;
            image[file + 2] = 0x00;
            image[file + 3] = 0xC0;

            for (int i = 0; i < 252; i++)
                image[file + 4 + i] = (byte)(0x41 + (i % 26));

            return image;
        }

        private static void MarkUsed(byte[] image, int track, int sector)
        {
            int bam = SectorOffset(18, 0);
            int entry = bam + 4 + ((track - 1) * 4);

            image[entry]--;
            image[entry + 1 + (sector / 8)] &= (byte)~(1 << (sector % 8));
        }

        private static void WritePaddedName(
            byte[] image,
            int offset,
            int length,
            string name)
        {
            for (int i = 0; i < length; i++)
                image[offset + i] = i < name.Length ? (byte)name[i] : (byte)0xA0;
        }

        private static int SectorOffset(int track, int sector)
        {
            int offset = 0;

            for (int current = 1; current < track; current++)
                offset += D64Image.SectorsPerTrack(current) * D64Image.SectorSize;

            return offset + (sector * D64Image.SectorSize);
        }

        private static string GetRomDirectory()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "C64Emulator", "Roms"));
        }
    }
}
