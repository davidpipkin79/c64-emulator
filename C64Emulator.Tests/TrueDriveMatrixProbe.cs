using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace C64Emulator.Tests
{
    public sealed class TrueDriveMatrixProbe
    {
        private readonly ITestOutputHelper output;

        public TrueDriveMatrixProbe(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(1, 0, (byte)'0', (byte)'1')]
        [InlineData(34, 0, (byte)'0', (byte)'1')]
        [InlineData(34, 10, (byte)'0', (byte)'1')]
        [InlineData(34, 0, (byte)' ', (byte)' ')]
        public void LoadOneSector(int track, int sector, byte id1, byte id2)
        {
            string romDirectory = GetRomDirectory();
            string driveRom = Path.Combine(romDirectory, "dos1541.rom");

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(driveRom));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.FromBytes(BuildDisk(track, sector, id1, id2)));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);
            Type(machine, "LOAD\"TEST\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            bool loaded = false;

            for (int cycles = 0; cycles < 20_000_000; cycles += 100_000)
            {
                if (
                    machine.Memory.Peek(0x0801) == 0x41 &&
                    machine.Memory.Peek(0x0802) == 0x42)
                {
                    loaded = true;
                    break;
                }

                machine.RunCycles(100_000);
            }

            Drive1541 drive = machine.Drive.RealDrive!;
            output.WriteLine(
                $"track={track}/{sector} id={id1:X2}/{id2:X2} loaded={loaded} " +
                $"C64PC={machine.Cpu.GetPC():X4} C1={machine.Memory.Peek(0xC1):X2} " +
                $"C2={machine.Memory.Peek(0xC2):X2} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                $"DRVPC={drive.Cpu.GetPC():X4} TRK={drive.Ram[0x22]:X2} " +
                $"HEAD={drive.Disk.HeadHalfTrack} MOTOR={drive.Disk.MotorOn} " +
                $"BYTES={drive.Disk.TotalBytesRead} SYNCS={drive.Disk.TotalSyncs} " +
                $"V1PB={drive.Via1.PortBOutput:X2} V1DDR={drive.Via1.DdrB:X2} " +
                $"V2PB={drive.Via2.PortBOutput:X2} V2DDR={drive.Via2.DdrB:X2} " +
                $"ST={drive.Ram[0x90]:X2} TLK={drive.Ram[0x7A]:X2} " +
                $"SCREEN={ReadScreenTail(machine.Memory)}");

            Assert.True(
                loaded,
                $"Load failed at track {track}/{sector}, ID {id1:X2}/{id2:X2}.");
        }

        [Fact]
        public void LoadAfterReadyAndRun()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            WaitForReady(machine);

            D64FileInfo file = machine.Drive.FindFile("DEFENDER OTC.+")!;
            byte[] program = machine.Drive.ReadFile(file);
            int address = program[0] | (program[1] << 8);
            int length = program.Length - 2;
            output.WriteLine(
                $"PROG len={program.Length} data={length} " +
                $"end={(address + length - 1):X4}");
            output.WriteLine(
                "PROG tail: " + string.Join(
                    " ", program.Skip(Math.Max(0, program.Length - 24))
                        .Select(b => b.ToString("X2"))));

            for (int i = 0; i < length; i++)
                machine.Memory.Poke((ushort)(address + i), program[i + 2]);

            output.WriteLine("POSTLOAD 2875: " + Hex(machine.Memory, 0x2875, 12));

            int end = address + length - 1;
            machine.Memory.Poke(0x2B, 0x01);
            machine.Memory.Poke(0x2C, 0x08);
            machine.Memory.Poke(0x2D, (byte)end);
            machine.Memory.Poke(0x2E, (byte)(end >> 8));
            machine.Memory.Poke(0xC1, 0x01);
            machine.Memory.Poke(0xC2, 0x08);
            machine.Memory.Poke(0xC3, (byte)end);
            machine.Memory.Poke(0xC4, (byte)(end >> 8));
            machine.Memory.Poke(0xAE, (byte)end);
            machine.Memory.Poke(0xAF, (byte)(end >> 8));

            Type(machine, "RUN");
            Press(machine, Key.Return, ModifierKeys.None);

            Drive1541 drive = machine.Drive.RealDrive!;

            bool sawEntry = false;
            bool sawLow = false;

            for (int i = 0; i < 200_000 && !machine.Cpu.IsJammed; i++)
            {
                machine.RunCycles(10);
                ushort pc = machine.Cpu.GetPC();

                if (!sawEntry && pc >= 0x0800 && pc <= 0x0810)
                {
                    sawEntry = true;
                    output.WriteLine(
                        $"entry PC={pc:X4} S={machine.Cpu.S:X2} A={machine.Cpu.A:X2} " +
                        $"X={machine.Cpu.X:X2} Y={machine.Cpu.Y:X2} " +
                        $"M0100={machine.Memory.Peek(0x0100):X2} " +
                        $"M0101={machine.Memory.Peek(0x0101):X2} " +
                        $"M286C={machine.Memory.Peek(0x286C):X2} " +
                        $"M286D={machine.Memory.Peek(0x286D):X2} " +
                        $"M286E={machine.Memory.Peek(0x286E):X2}");
                }

                if (!sawLow && pc < 0x0100)
                {
                    sawLow = true;
                    output.WriteLine(
                        $"low PC={pc:X4} S={machine.Cpu.S:X2} A={machine.Cpu.A:X2} " +
                        $"X={machine.Cpu.X:X2} Y={machine.Cpu.Y:X2} JAM={machine.Cpu.IsJammed} " +
                        $"M0100={machine.Memory.Peek(0x0100):X2} " +
                        $"M0101={machine.Memory.Peek(0x0101):X2}");
                }
            }

            output.WriteLine(
                $"trace-end PC={machine.Cpu.GetPC():X4} S={machine.Cpu.S:X2} " +
                $"JAM={machine.Cpu.IsJammed} entry={sawEntry} low={sawLow} " +
                $"M0100={machine.Memory.Peek(0x0100):X2} " +
                $"M0101={machine.Memory.Peek(0x0101):X2} " +
                $"DRVPC={drive.Cpu.GetPC():X4} TRK={drive.Ram[0x22]:X2} " +
                $"MOTOR={drive.Disk.MotorOn} BYTES={drive.Disk.TotalBytesRead} " +
                $"SCREEN={ReadScreenTail(machine.Memory)}");
        }

        [Fact]
        public void DirectBootKernalReset()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            machine.LoadDiskProgram("DEFENDER OTC.+");

            // Perform a normal KERNAL boot so the screen, IRQ and BASIC
            // editor are initialized, then restore the loaded BASIC program.
            machine.HardReset();
            machine.RunCycles(2_000_000);
            machine.Memory.Poke(0x2B, 0x01);
            machine.Memory.Poke(0x2C, 0x08);
            machine.Memory.Poke(0x2D, 0xBD);
            machine.Memory.Poke(0x2E, 0x28);

            output.WriteLine(
                $"boot PC={machine.Cpu.GetPC():X4} " +
                $"SCREEN={ReadScreenTail(machine.Memory)}");

            Type(machine, "RUN");
            Press(machine, Key.Return, ModifierKeys.None);

            Drive1541 drive = machine.Drive.RealDrive!;

            for (int second = 0; second < 30; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);
                output.WriteLine(
                    $"t={second + 1} PC={machine.Cpu.GetPC():X4} " +
                    $"JAM={machine.Cpu.IsJammed} DRVPC={drive.Cpu.GetPC():X4} " +
                    $"TRK={drive.Ram[0x22]:X2} HEAD={drive.Disk.HeadHalfTrack} " +
                    $"MOTOR={drive.Disk.MotorOn} BYTES={drive.Disk.TotalBytesRead} " +
                    $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                    $"ZP4A={machine.Memory.Peek(0x4A):X2} " +
                    $"SCREEN={ReadScreenTail(machine.Memory)}");

                if (machine.Cpu.IsJammed)
                    break;
            }
        }

        [Fact]
        public void DirectSysWithKernalStack()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            machine.LoadDiskProgram("DEFENDER OTC.+");

            // The boot stub reads the stack pointer and uses it as an index,
            // so reproduce the KERNAL SYS stack frame VICE shows at $080B.
            machine.Cpu.S = 0xF6;
            machine.Memory.Poke(0x01F7, 0xAD);
            machine.Memory.Poke(0x01F8, 0xA7);
            machine.Memory.Poke(0x01, 0x37);
            machine.Cpu.SetPC(0x080B);

            Drive1541 drive = machine.Drive.RealDrive!;

            for (int second = 0; second < 30; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);
                output.WriteLine(
                    $"t={second + 1} PC={machine.Cpu.GetPC():X4} " +
                    $"S={machine.Cpu.S:X2} JAM={machine.Cpu.IsJammed} " +
                    $"DRVPC={drive.Cpu.GetPC():X4} TRK={drive.Ram[0x22]:X2} " +
                    $"HEAD={drive.Disk.HeadHalfTrack} MOTOR={drive.Disk.MotorOn} " +
                    $"BYTES={drive.Disk.TotalBytesRead} " +
                    $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                    $"ZP4A={machine.Memory.Peek(0x4A):X2} " +
                    $"SCREEN={ReadScreenTail(machine.Memory)}");

                if (machine.Cpu.IsJammed)
                    break;
            }
        }

        [Fact]
        public void DirectBootAndRun()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            machine.LoadDiskProgram("DEFENDER OTC.+");

            // The menu load enters BASIC directly; give the interpreter time
            // to settle, then submit RUN instead of waiting for a banner.
            machine.RunCycles(2_000_000);
            Type(machine, "RUN");
            Press(machine, Key.Return, ModifierKeys.None);

            Drive1541 drive = machine.Drive.RealDrive!;

            for (int second = 0; second < 30; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);
                output.WriteLine(
                    $"t={second + 1} PC={machine.Cpu.GetPC():X4} " +
                    $"JAM={machine.Cpu.IsJammed} DRVPC={drive.Cpu.GetPC():X4} " +
                    $"TRK={drive.Ram[0x22]:X2} HEAD={drive.Disk.HeadHalfTrack} " +
                    $"MOTOR={drive.Disk.MotorOn} BYTES={drive.Disk.TotalBytesRead} " +
                    $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                    $"ZP4A={machine.Memory.Peek(0x4A):X2} " +
                    $"SCREEN={ReadScreenTail(machine.Memory)}");

                if (machine.Cpu.IsJammed)
                    break;
            }
        }

        [Fact]
        public void LoadActualDotcTwoSectors()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();
            byte[] original = File.ReadAllBytes(diskPath);
            byte[] synthetic = BuildDisk(34, 0, (byte)' ', (byte)' ');
            int first = SectorOffset(34, 0);
            int second = SectorOffset(34, 10);

            Array.Copy(original, first, synthetic, first, 256);
            Array.Copy(original, second, synthetic, second, 256);
            synthetic[first] = 34;
            synthetic[first + 1] = 10;
            synthetic[second] = 0;
            synthetic[second + 1] = 0;

            int directory = SectorOffset(18, 1);
            synthetic[directory + 3] = 34;
            synthetic[directory + 4] = 0;
            synthetic[directory + 30] = 2;

            output.WriteLine(
                $"synthetic dir type={synthetic[directory + 2]:X2} " +
                $"start={synthetic[directory + 3]}/{synthetic[directory + 4]} " +
                $"blocks={synthetic[directory + 30]} " +
                $"chain={synthetic[first]}/{synthetic[first + 1]}" +
                $"->{synthetic[second]}/{synthetic[second + 1]}");

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.FromBytes(synthetic));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);
            Type(machine, "LOAD\"TEST\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            bool loaded = false;
            Drive1541 drive = machine.Drive.RealDrive!;
            Queue<string> trace = new Queue<string>();
            Queue<string> headers = new Queue<string>();

            drive.Disk.HeaderDecoded += (track, sector) =>
            {
                if (headers.Count == 200)
                    headers.Dequeue();

                headers.Enqueue($"{track}/{sector}");
            };

            machine.Iec.LinesChanged += () =>
            {
                if (trace.Count == 600)
                    trace.Dequeue();

                trace.Enqueue(
                    $"PC={machine.Cpu.GetPC():X4} DRVPC={drive.Cpu.GetPC():X4} " +
                    $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                    $"A5={machine.Memory.Peek(0xA5):X2} A4={machine.Memory.Peek(0xA4):X2} " +
                    $"D98={drive.Ram[0x98]:X2} D23={drive.Ram[0x23]:X2} " +
                    $"PB={drive.Via1.PortBOutput:X2}");
            };

            for (int cycles = 0; cycles < 10_000_000; cycles += 100_000)
            {
                if (
                    machine.Memory.Peek(0xC1) == 0x01 &&
                    machine.Memory.Peek(0xC2) == 0x08)
                {
                    loaded = true;
                    break;
                }

                machine.RunCycles(100_000);
            }

            output.WriteLine(
                $"two-sector loaded={loaded} M801={machine.Memory.Peek(0x0801):X2} " +
                $"M8FB={machine.Memory.Peek(0x08FB):X2} " +
                $"M8FC={machine.Memory.Peek(0x08FC):X2} " +
                $"M8FD={machine.Memory.Peek(0x08FD):X2} " +
                $"EXPECT2={original[second + 2]:X2} " +
                $"M9F8={machine.Memory.Peek(0x09F8):X2} " +
                $"C64PC={machine.Cpu.GetPC():X4} " +
                $"DRV={drive.Ram[0x22]:X2}/{drive.Ram[0x23]:X2} " +
                $"DRVPC={drive.Cpu.GetPC():X4} BYTES={drive.Disk.TotalBytesRead} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                $"CIA1ICR={machine.Cia1.Peek(0x0D):X2} " +
                $"CIA1CRB={machine.Cia1.Peek(0x0F):X2} " +
                $"CIA1TB={machine.Cia1.Peek(0x07):X2}{machine.Cia1.Peek(0x06):X2} " +
                $"CIA2PA={machine.Cia2.Peek(0):X2} " +
                $"V1PB={drive.Via1.PortBOutput:X2} V1DDR={drive.Via1.DdrB:X2}");

            if (!loaded)
            {
                output.WriteLine("headers: " + string.Join(" ", headers));
                output.WriteLine("IEC trace (last 600 changes):");

                foreach (string line in trace)
                    output.WriteLine("  " + line);
            }
        }

        [Fact]
        public void LoadActualDotcBootSectorExplicitly()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();
            byte[] original = File.ReadAllBytes(diskPath);
            byte[] synthetic = BuildDisk(34, 0, (byte)' ', (byte)' ');
            int source = SectorOffset(34, 0);
            int destination = SectorOffset(34, 0);

            synthetic[destination] = 0;
            synthetic[destination + 1] = 0;
            synthetic[destination + 2] = 0x01;
            synthetic[destination + 3] = 0x08;
            Array.Copy(
                original,
                source + 4,
                synthetic,
                destination + 4,
                252);

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.FromBytes(synthetic));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);
            Type(machine, "LOAD\"TEST\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            bool loaded = false;

            for (int cycles = 0; cycles < 10_000_000; cycles += 100_000)
            {
                if (
                    machine.Memory.Peek(0x0801) == 0x0B &&
                    machine.Memory.Peek(0x0802) == 0x08)
                {
                    loaded = true;
                    break;
                }

                machine.RunCycles(100_000);
            }

            Drive1541 drive = machine.Drive.RealDrive!;
            output.WriteLine(
                $"actual-boot loaded={loaded} M801={machine.Memory.Peek(0x0801):X2} " +
                $"M802={machine.Memory.Peek(0x0802):X2} C64PC={machine.Cpu.GetPC():X4} " +
                $"DRV={drive.Ram[0x22]:X2}/{drive.Ram[0x23]:X2} " +
                $"DRVPC={drive.Cpu.GetPC():X4} BYTES={drive.Disk.TotalBytesRead} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow}");
        }

        [Fact]
        public void LoadFullSectorWithWildcard()
        {
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.FromBytes(BuildDisk(34, 0, (byte)' ', (byte)' ')));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);
            Type(machine, "LOAD\"*\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            bool loaded = false;

            for (int cycles = 0; cycles < 10_000_000; cycles += 100_000)
            {
                if (
                    machine.Memory.Peek(0x0801) == 0x41 &&
                    machine.Memory.Peek(0x0802) == 0x42)
                {
                    loaded = true;
                    break;
                }

                machine.RunCycles(100_000);
            }

            Drive1541 drive = machine.Drive.RealDrive!;
            output.WriteLine(
                $"wildcard loaded={loaded} M801={machine.Memory.Peek(0x0801):X2} " +
                $"M802={machine.Memory.Peek(0x0802):X2} C64PC={machine.Cpu.GetPC():X4} " +
                $"DRV={drive.Ram[0x22]:X2}/{drive.Ram[0x23]:X2} " +
                $"DRVPC={drive.Cpu.GetPC():X4} BYTES={drive.Disk.TotalBytesRead} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow}");
        }

        [Fact]
        public void TraceDotcDosSector()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            WaitForReady(machine);
            Type(machine, "LOAD\"*\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            Drive1541 drive = machine.Drive.RealDrive!;

            for (int second = 0; second < 12; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);
                output.WriteLine(
                    $"t={second + 1} C64PC={machine.Cpu.GetPC():X4} " +
                    $"C1={machine.Memory.Peek(0xC1):X2} C2={machine.Memory.Peek(0xC2):X2} " +
                    $"M801={machine.Memory.Peek(0x0801):X2} " +
                    $"DRV={drive.Ram[0x22]:X2}/{drive.Ram[0x23]:X2} " +
                    $"DRVPC={drive.Cpu.GetPC():X4} " +
                    $"HEAD={drive.Disk.HeadHalfTrack} MOTOR={drive.Disk.MotorOn} " +
                    $"BYTES={drive.Disk.TotalBytesRead} ST={drive.Ram[0x90]:X2}");
            }

            output.WriteLine("SCREEN=" + ReadScreenTail(machine.Memory));
        }

        [Fact]
        public void TraceDotcSoundWrites()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            WaitForReady(machine);

            D64FileInfo file = machine.Drive.FindFile("DEFENDER OTC.+")!;
            byte[] program = machine.Drive.ReadFile(file);
            int address = program[0] | (program[1] << 8);
            int length = program.Length - 2;

            for (int i = 0; i < length; i++)
                machine.Memory.Poke((ushort)(address + i), program[i + 2]);

            int end = address + length - 1;
            machine.Memory.Poke(0x2B, 0x01);
            machine.Memory.Poke(0x2C, 0x08);
            machine.Memory.Poke(0x2D, (byte)end);
            machine.Memory.Poke(0x2E, (byte)(end >> 8));

            Type(machine, "RUN");
            Press(machine, Key.Return, ModifierKeys.None);

            var byPc = new Dictionary<ushort, int>();
            var byPort = new Dictionary<byte, int>();
            var recent = new Queue<string>();
            long total = 0;

            machine.Sid.RegisterWriteHook = (register, value) =>
            {
                total++;
                ushort pc = machine.Cpu.GetPC();
                byte port = machine.Memory.Peek(0x0001);
                byPc[pc] = byPc.TryGetValue(pc, out int n) ? n + 1 : 1;
                byPort[port] = byPort.TryGetValue(port, out int p) ? p + 1 : 1;

                if (recent.Count == 30)
                    recent.Dequeue();

                recent.Enqueue($"{pc:X4}/{port:X2}:{register:X2}={value:X2}");
            };

            for (int second = 1; second <= 260; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);

                string shadow = string.Concat(
                    Enumerable.Range(0, 24).Select(
                        i => machine.Memory.Read((ushort)(0xA851 + i)).ToString("X2")));

                output.WriteLine(
                    $"t={second} PC={machine.Cpu.GetPC():X4} " +
                    $"JAM={machine.Cpu.IsJammed} 01={machine.Memory.Peek(0x01):X2} " +
                    $"writes={total} " +
                    $"ports={string.Join(",", byPort.OrderByDescending(kv => kv.Value).Take(4).Select(kv => $"{kv.Key:X2}:{kv.Value}"))} " +
                    $"pcs={string.Join(",", byPc.OrderByDescending(kv => kv.Value).Take(4).Select(kv => $"{kv.Key:X4}:{kv.Value}"))} " +
                    $"shadow={shadow} SID00={machine.Sid.GetRegister(0):X2} " +
                    $"SID18={machine.Sid.GetRegister(0x18):X2} " +
                    $"SCREEN={ReadScreenTail(machine.Memory)}");

                if (machine.Cpu.IsJammed)
                    break;
            }

            output.WriteLine("recent: " + string.Join(" ", recent));
        }

        [Fact]
        public void DecodeFirstDotcHeader()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            D64Image image = D64Image.Load(diskPath);
            GcrDisk disk = new GcrDisk();
            disk.Mount(image, null);

            foreach (int track in new[] { 18, 22, 34, 35 })
            {
                byte[]? data = disk.TrackImageForTest(track);

                if (data is null)
                    continue;

                Span<byte> header = stackalloc byte[8];
                GcrCodec.Decode(data.AsSpan(17, 10), header);
                byte checksum = (byte)(header[2] ^ header[3] ^ header[4] ^ header[5]);
                output.WriteLine(
                    $"track {track}: header={Convert.ToHexString(header)} " +
                    $"type={header[0]:X2} checksum={checksum:X2}/{header[1]:X2}");
            }
        }

        [Fact]
        public void ReadDotcTrackDirectly()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.Drive.Mount(D64Image.Load(diskPath));
            GcrDisk disk = machine.Drive.RealDrive!.Disk;
            HashSet<(int Track, int Sector)> headers = new();
            disk.HeaderDecoded += (track, sector) => headers.Add((track, sector));

            // Step the head from track 1 to track 34, then leave the motor on.
            for (int i = 0; i < 66; i++)
                disk.SetPortB((byte)(0x04 | ((i + 1) & 3)));

            disk.SetPortB(0x04);
            disk.Tick(4_000_000);

            output.WriteLine(
                $"direct headers={headers.Count} bytes={disk.TotalBytesRead} " +
                $"syncs={disk.TotalSyncs} head={disk.HeadHalfTrack} " +
                $"track={disk.CurrentTrack} motor={disk.MotorOn} write={disk.WriteMode} " +
                $"seen=" + string.Join(
                    " ",
                    headers.OrderBy(x => x.Track).ThenBy(x => x.Sector)
                        .Select(x => x.Track + "/" + x.Sector)));
        }

        [Fact]
        public void InspectDotcGcrTracks()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();
            D64Image image = D64Image.Load(diskPath);

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.Drive.Mount(image);
            GcrDisk disk = machine.Drive.RealDrive!.Disk;

            output.WriteLine(
                $"tracks={image.TrackCount} id={string.Join(",", image.DiskIdBytes)} " +
                $"track34={disk.TrackImageForTest(34)?.Length} " +
                $"track22={disk.TrackImageForTest(22)?.Length} " +
                $"write={disk.WriteMode}");

            foreach (int track in new[] { 1, 18, 22, 34, 35 })
            {
                byte[]? data = disk.TrackImageForTest(track);

                if (data is null)
                {
                    output.WriteLine($"track {track}: null");
                    continue;
                }

                int ff = data.Count(value => value == 0xFF);
                output.WriteLine(
                    $"track {track}: len={data.Length} ff={ff} " +
                    $"start={Convert.ToHexString(data.AsSpan(0, Math.Min(48, data.Length)))}");
            }

            disk.SetPortB(0x04);
            bool sawSync = false;
            int bytes = 0;

            for (int i = 0; i < 4_000_000; i++)
            {
                disk.Tick(1);
                sawSync |= disk.Sync;

                if (disk.ConsumeByteReady())
                    bytes++;
            }

            output.WriteLine(
                $"track1 direct sync={sawSync} bytes={bytes} totalSyncs={disk.TotalSyncs} " +
                $"head={disk.HeadHalfTrack} track={disk.CurrentTrack} motor={disk.MotorOn}");
        }

        [Fact]
        public void TraceDotcHeaders()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();
            string driveRom = Path.Combine(romDirectory, "dos1541.rom");

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(driveRom));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();

            Drive1541 drive = machine.Drive.RealDrive!;
            HashSet<(int Track, int Sector)> headers = new();
            drive.Disk.HeaderDecoded += (track, sector) => headers.Add((track, sector));

            WaitForReady(machine);
            Type(machine, "LOAD\"*\",8,1");
            Press(machine, Key.Return, ModifierKeys.None);

            for (int second = 0; second < 12; second++)
            {
                machine.RunCycles(C64Machine.ClockHz);
                output.WriteLine(
                    $"t={second + 1} C64PC={machine.Cpu.GetPC():X4} " +
                    $"C1={machine.Memory.Peek(0xC1):X2} C2={machine.Memory.Peek(0xC2):X2} " +
                    $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                    $"DRVPC={drive.Cpu.GetPC():X4} TRK={drive.Ram[0x22]:X2} " +
                    $"HEAD={drive.Disk.HeadHalfTrack} MOTOR={drive.Disk.MotorOn} " +
                    $"BYTES={drive.Disk.TotalBytesRead} SYNCS={drive.Disk.TotalSyncs} " +
                    $"HEADERS={headers.Count}");
            }

            output.WriteLine(
                "seen: " + string.Join(
                    " ",
                    headers.OrderBy(x => x.Track).ThenBy(x => x.Sector)
                        .Select(x => x.Track + "/" + x.Sector)));
        }

        [Fact]
        public void TraceDotcDecompressor()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            WaitForReady(machine);

            D64FileInfo file = machine.Drive.FindFile("DEFENDER OTC.+")!;
            byte[] program = machine.Drive.ReadFile(file);
            int address = program[0] | (program[1] << 8);
            int length = program.Length - 2;

            output.WriteLine(
                $"PROG len={program.Length} data={length} " +
                $"end={(address + length - 1):X4}");
            output.WriteLine(
                "PROG tail: " + string.Join(
                    " ", program.Skip(Math.Max(0, program.Length - 24))
                        .Select(b => b.ToString("X2"))));

            for (int i = 0; i < length; i++)
                machine.Memory.Poke((ushort)(address + i), program[i + 2]);

            output.WriteLine("POSTLOAD 2875: " + Hex(machine.Memory, 0x2875, 12));

            int end = address + length - 1;
            machine.Memory.Poke(0x2B, 0x01);
            machine.Memory.Poke(0x2C, 0x08);
            machine.Memory.Poke(0x2D, (byte)end);
            machine.Memory.Poke(0x2E, (byte)(end >> 8));
            machine.Memory.Poke(0xC1, 0x01);
            machine.Memory.Poke(0xC2, 0x08);
            machine.Memory.Poke(0xC3, (byte)end);
            machine.Memory.Poke(0xC4, (byte)(end >> 8));
            machine.Memory.Poke(0xAE, (byte)end);
            machine.Memory.Poke(0xAF, (byte)(end >> 8));

            Type(machine, "RUN");
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Return, ModifierKeys.None, true),
                true);

            bool tracing = true;
            int logged = 0;
            var recent = new Queue<string>();

            for (int i = 0; i < 2_000_000; i++)
            {
                if (i == 5_000)
                {
                    machine.Cia1.KeyEvent(
                        new KeyboardEventArgs(Key.Return, ModifierKeys.None, false),
                        true);
                }

                ushort pc = machine.Cpu.GetPC();
                recent.Enqueue(
                    $"{pc:X4} S={machine.Cpu.S:X2} P={machine.Cpu.P:X2} " +
                    $"A={machine.Cpu.A:X2} X={machine.Cpu.X:X2} Y={machine.Cpu.Y:X2}");
                if (recent.Count > 100)
                    recent.Dequeue();

                bool interesting =
                    pc == 0x080B ||
                    pc == 0x2848 ||
                    pc == 0x286A ||
                    pc == 0x287A ||
                    pc == 0x0100 ||
                    pc == 0x0101 ||
                    pc == 0x0125 ||
                    pc == 0x0142 ||
                    pc == 0x014A ||
                    pc == 0x1900 ||
                    pc == 0x0009;

                if (tracing && interesting)
                {
                    output.WriteLine(
                        $"{pc:X4} A={machine.Cpu.A:X2} X={machine.Cpu.X:X2} " +
                        $"Y={machine.Cpu.Y:X2} S={machine.Cpu.S:X2} " +
                        $"P={machine.Cpu.P:X2} " +
                        $"AE={PeekWord(machine.Memory, 0xAE):X4} " +
                        $"FE={PeekWord(machine.Memory, 0xFE):X4} " +
                        $"FB={machine.Memory.Peek(0xFB):X2} " +
                        $"FC={machine.Memory.Peek(0xFC):X2} " +
                        $"FD={machine.Memory.Peek(0xFD):X2} " +
                        $"A7={machine.Memory.Peek(0xA7):X2} " +
                        $"0123={machine.Memory.Peek(0x0123):X2} " +
                        $"0124={machine.Memory.Peek(0x0124):X2} " +
                        $"0100={machine.Memory.Peek(0x0100):X2} " +
                        $"0101={machine.Memory.Peek(0x0101):X2} " +
                        $"0009={machine.Memory.Peek(0x0009):X2}");
                    logged++;
                }

                machine.RunCycles(1);

                if (machine.Cpu.IsJammed)
                {
                    output.WriteLine(
                        $"JAM after PC={pc:X4} now={machine.Cpu.GetPC():X4} " +
                        $"S={machine.Cpu.S:X2} " +
                        $"AE={PeekWord(machine.Memory, 0xAE):X4} " +
                        $"FE={PeekWord(machine.Memory, 0xFE):X4}");
                    output.WriteLine("01=" + machine.Memory.Peek(0x01).ToString("X2"));
                    output.WriteLine("RECENT:");
                    foreach (string entry in recent)
                        output.WriteLine("  " + entry);
                    output.WriteLine("STACK: " + Hex(machine.Memory, 0x01E0, 32));
                    output.WriteLine("ZP00:  " + Hex(machine.Memory, 0x0000, 16));
                    output.WriteLine("VEC314: " + Hex(machine.Memory, 0x0314, 6));
                    output.WriteLine("VECFFA: " + Hex(machine.Memory, 0xFFFA, 6));
                    output.WriteLine("2875: " + Hex(machine.Memory, 0x2875, 12));
                    output.WriteLine("0140: " + Hex(machine.Memory, 0x0140, 16));
                    output.WriteLine("1900: " + Hex(machine.Memory, 0x1900, 48));
                    output.WriteLine("52D8: " + Hex(machine.Memory, 0x52D8, 15));
                    output.WriteLine("FF80: " + Hex(machine.Memory, 0xFF80, 24));
                    output.WriteLine("0334: " + Hex(machine.Memory, 0x0334, 52));
                    break;
                }
            }

            output.WriteLine(
                $"FINAL PC={machine.Cpu.GetPC():X4} S={machine.Cpu.S:X2} " +
                $"JAM={machine.Cpu.IsJammed} 01={machine.Memory.Peek(0x01):X2}");
            output.WriteLine("FINAL 1900: " + Hex(machine.Memory, 0x1900, 48));
            output.WriteLine("FINAL 52D8: " + Hex(machine.Memory, 0x52D8, 16));
            output.WriteLine("FINAL 0801: " + Hex(machine.Memory, 0x0801, 24));
            output.WriteLine("FINAL 0400: " + Hex(machine.Memory, 0x0400, 80));
            machine.RunCycles(1_000_000);
            output.WriteLine(
                $"AFTER PC={machine.Cpu.GetPC():X4} S={machine.Cpu.S:X2} " +
                $"JAM={machine.Cpu.IsJammed} 01={machine.Memory.Peek(0x01):X2}");
            output.WriteLine("AFTER 0400: " + Hex(machine.Memory, 0x0400, 80));

            byte[] beforeFire = machine.Vic.CapturePackedScreen();
            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, true);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, true);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Space, ModifierKeys.None, true),
                true);
            machine.RunCycles(100_000);
            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, false);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, false);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Space, ModifierKeys.None, false),
                true);
            long driveBytesBefore = machine.Drive.RealDrive?.Disk.TotalBytesRead ?? -1;
            bool driveMotorBefore = machine.Drive.RealDrive?.Disk.MotorOn ?? false;
            machine.RunCycles(8_000_000);
            byte[] afterFire = machine.Vic.CapturePackedScreen();
            long driveBytesAfter = machine.Drive.RealDrive?.Disk.TotalBytesRead ?? -1;
            bool driveMotorAfter = machine.Drive.RealDrive?.Disk.MotorOn ?? false;
            int fireDiff = 0;

            for (int i = 0; i < beforeFire.Length; i++)
            {
                if (beforeFire[i] != afterFire[i])
                    fireDiff++;
            }

            output.WriteLine(
                $"FIRE PC={machine.Cpu.GetPC():X4} diff={fireDiff} " +
                $"JAM={machine.Cpu.IsJammed} 01={machine.Memory.Peek(0x01):X2} " +
                $"drive={driveBytesBefore}->{driveBytesAfter} " +
                $"motor={driveMotorBefore}->{driveMotorAfter}");
            output.WriteLine("FIRE 0400: " + Hex(machine.Memory, 0x0400, 80));
            output.WriteLine("FIRE D011: " + Hex(machine.Memory, 0xD011, 4));

            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, true);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, true);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Return, ModifierKeys.None, true),
                true);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Space, ModifierKeys.None, true),
                true);
            long secondBytesBefore = machine.Drive.RealDrive?.Disk.TotalBytesRead ?? -1;
            bool secondMotorBefore = machine.Drive.RealDrive?.Disk.MotorOn ?? false;
            machine.RunCycles(100_000);
            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, false);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, false);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Return, ModifierKeys.None, false),
                true);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.Space, ModifierKeys.None, false),
                true);
            machine.RunCycles(8_000_000);
            byte[] afterSecond = machine.Vic.CapturePackedScreen();
            long secondBytesAfter = machine.Drive.RealDrive?.Disk.TotalBytesRead ?? -1;
            bool secondMotorAfter = machine.Drive.RealDrive?.Disk.MotorOn ?? false;
            int secondDiff = 0;

            for (int i = 0; i < afterFire.Length; i++)
            {
                if (afterFire[i] != afterSecond[i])
                    secondDiff++;
            }

            output.WriteLine(
                $"SECOND PC={machine.Cpu.GetPC():X4} diff={secondDiff} " +
                $"JAM={machine.Cpu.IsJammed} 01={machine.Memory.Peek(0x01):X2} " +
                $"drive={secondBytesBefore}->{secondBytesAfter} " +
                $"motor={secondMotorBefore}->{secondMotorAfter}");
            output.WriteLine("SECOND 0400: " + Hex(machine.Memory, 0x0400, 80));
            output.WriteLine("LOADER 4000: " + Hex(machine.Memory, 0x4000, 64));
            output.WriteLine("LOADER DC00: " + Hex(machine.Memory, 0xDC00, 4));
            output.WriteLine("LOADER DD00: " + Hex(machine.Memory, 0xDD00, 4));
            output.WriteLine("LOADER 403D: " + Hex(machine.Memory, 0x403D, 32));
            output.WriteLine("LOADER 4080: " + Hex(machine.Memory, 0x4080, 96));
        }

        private static int PeekWord(Memory memory, ushort address)
        {
            return memory.Peek(address) | (memory.Peek((ushort)(address + 1)) << 8);
        }

        [Fact]
        public void TryDotcLoaderMenu()
        {
            const string diskPath =
                @"C:\Users\david\Documents\My Projects\C64Roms\dotc-e.d64";
            string romDirectory = GetRomDirectory();

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(Path.Combine(romDirectory, "dos1541.rom")));

            machine.TrueDriveEmulation = true;
            machine.Drive.Mount(D64Image.Load(diskPath));
            machine.Drive.Reset();
            machine.Cpu.Reset();
            WaitForReady(machine);

            machine.LoadDiskProgram("DEFENDER OTC.+");
            machine.RunCycles(6_000_000);
            output.WriteLine(
                $"BOOT PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed}");
            output.WriteLine(
                $"BOOT BA={machine.Memory.Peek(0xBA):X2} B7={machine.Memory.Peek(0xB7):X2} " +
                $"B8={machine.Memory.Peek(0xB8):X2} B9={machine.Memory.Peek(0xB9):X2} " +
                $"90={machine.Memory.Peek(0x90):X2}");
            output.WriteLine("BOOT SCREEN:\n" + ScreenRows(machine.Memory));

            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, true);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, true);
            machine.RunCycles(100_000);
            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, false);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, false);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"LOADER PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn}");
            output.WriteLine(
                $"LOADER BA={machine.Memory.Peek(0xBA):X2} B7={machine.Memory.Peek(0xB7):X2} " +
                $"B8={machine.Memory.Peek(0xB8):X2} B9={machine.Memory.Peek(0xB9):X2} " +
                $"90={machine.Memory.Peek(0x90):X2}");
            output.WriteLine("LOADER SCREEN:\n" + ScreenRows(machine.Memory));

            byte loaderDevice = machine.Memory.Peek(0xBA);

            if (loaderDevice != 8)
                machine.Memory.Poke(0xBA, 8);

            List<string> iecTrace = new List<string>();
            machine.Iec.LinesChanged += () =>
            {
                if (iecTrace.Count >= 80)
                    return;

                iecTrace.Add(
                    $"PC={machine.Cpu.GetPC():X4} " +
                    $"ATN={(machine.Iec.AtnLow ? 0 : 1)} " +
                    $"CLK={(machine.Iec.ClockLow ? 0 : 1)} " +
                    $"DAT={(machine.Iec.DataLow ? 0 : 1)} " +
                    $"DRVPC={machine.Drive.RealDrive!.Cpu.GetPC():X4} " +
                    $"BA={machine.Memory.Peek(0xBA):X2} " +
                    $"B7={machine.Memory.Peek(0xB7):X2} " +
                    $"B9={machine.Memory.Peek(0xB9):X2} " +
                    $"90={machine.Memory.Peek(0x90):X2} " +
                    $"PB={machine.Drive.RealDrive!.Via1.PortBOutput:X2}");
            };

            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.One, ModifierKeys.None, true),
                true);
            machine.RunCycles(2_000_000);
            machine.Cia1.KeyEvent(
                new KeyboardEventArgs(Key.One, ModifierKeys.None, false),
                true);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"ONE PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn} " +
                $"BA={machine.Memory.Peek(0xBA):X2} " +
                $"B7={machine.Memory.Peek(0xB7):X2} " +
                $"B8={machine.Memory.Peek(0xB8):X2} " +
                $"B9={machine.Memory.Peek(0xB9):X2} " +
                $"90={machine.Memory.Peek(0x90):X2}");
            output.WriteLine("ONE SCREEN:\n" + ScreenRows(machine.Memory));

            for (int i = 0; i < 10; i++)
                Press(machine, Key.DownArrow, ModifierKeys.None);

            Press(machine, Key.Return, ModifierKeys.None);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"KEYMENU PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn}");
            output.WriteLine("KEYMENU SCREEN:\n" + ScreenRows(machine.Memory));
            output.WriteLine("KEYMENU 34B4: " + Hex(machine.Memory, 0x34B4, 96));
            output.WriteLine(
                $"KEYMENU DC00={machine.Memory.Peek(0xDC00):X2} " +
                $"DC01={machine.Memory.Peek(0xDC01):X2} " +
                $"D011={machine.Memory.Peek(0xD011):X2} " +
                $"D016={machine.Memory.Peek(0xD016):X2} " +
                $"D018={machine.Memory.Peek(0xD018):X2} " +
                $"D019={machine.Memory.Peek(0xD019):X2} " +
                $"D01A={machine.Memory.Peek(0xD01A):X2} " +
                $"01={machine.Memory.Peek(0x01):X2}");

            for (int i = 0; i < 3; i++)
            {
                machine.RunCycles(5_000_000);
                output.WriteLine(
                    $"WAIT{i} PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                    $"S={machine.Cpu.S:X2} P={machine.Cpu.P:X2} " +
                    $"A={machine.Cpu.A:X2} X={machine.Cpu.X:X2} Y={machine.Cpu.Y:X2} " +
                    $"01={machine.Memory.Peek(0x01):X2} D011={machine.Memory.Peek(0xD011):X2} " +
                    $"D019={machine.Memory.Peek(0xD019):X2} D01A={machine.Memory.Peek(0xD01A):X2} " +
                    $"IRQ={PeekWord(machine.Memory, 0x0314):X4} " +
                    $"BRK={PeekWord(machine.Memory, 0x0316):X4} " +
                    $"NMI={PeekWord(machine.Memory, 0xFFFA):X4} " +
                    $"RESET={PeekWord(machine.Memory, 0xFFFC):X4} " +
                    $"HIRQ={PeekWord(machine.Memory, 0xFFFE):X4} " +
                    $"DC0D={machine.Memory.Peek(0xDC0D):X2} DD0D={machine.Memory.Peek(0xDD0D):X2}");
                output.WriteLine(
                    $"WAIT{i} STACK: " +
                    Hex(machine.Memory, (ushort)(0x0100 + machine.Cpu.S - 8), 24));
            }

            Press(machine, Key.Space, ModifierKeys.None);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"SPACE PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead}");
            output.WriteLine("SPACE SCREEN:\n" + ScreenRows(machine.Memory));

            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, true);
            machine.RunCycles(200_000);
            machine.Cia1.JoyEvent(1, JoystickFunction.Fire, false);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"FIRE1 PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead}");
            output.WriteLine("FIRE1 SCREEN:\n" + ScreenRows(machine.Memory));

            for (int i = 0; i < 10; i++)
            {
                machine.Cia1.JoyEvent(2, JoystickFunction.Down, true);
                machine.RunCycles(100_000);
                machine.Cia1.JoyEvent(2, JoystickFunction.Down, false);
                machine.RunCycles(100_000);
            }

            byte releaseValue = machine.Memory.Peek(0xF1);
            output.WriteLine(
                $"FORCE F1={releaseValue:X2} PC={machine.Cpu.GetPC():X4} " +
                $"DD00={machine.Memory.Peek(0xDD00):X2} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow}");
            machine.Memory.Poke(0xDD00, releaseValue);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"FORCED PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn} " +
                $"DD00={machine.Memory.Peek(0xDD00):X2} " +
                $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow}");
            output.WriteLine("FORCED SCREEN:\n" + ScreenRows(machine.Memory));

            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, true);
            machine.RunCycles(200_000);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, false);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"JOYMENU PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn}");
            output.WriteLine("JOYMENU SCREEN:\n" + ScreenRows(machine.Memory));
            for (int stage = 0; stage < 8; stage++)
            {
                machine.RunCycles(10_000_000);
                output.WriteLine(
                    $"POST{stage} PC={machine.Cpu.GetPC():X4} " +
                    $"JAM={machine.Cpu.IsJammed} " +
                    $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                    $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn}");
                output.WriteLine("POST SCREEN:\n" + ScreenRows(machine.Memory));

                if (stage == 0)
                {
                    output.WriteLine("POST 3692: " + Hex(machine.Memory, 0x3692, 64));
                    output.WriteLine(
                        $"POST CIA: DC00={machine.Memory.Peek(0xDC00):X2} " +
                        $"DC01={machine.Memory.Peek(0xDC01):X2} " +
                        $"DD00={machine.Memory.Peek(0xDD00):X2} " +
                        $"DD01={machine.Memory.Peek(0xDD01):X2} " +
                        $"DD0D={machine.Memory.Peek(0xDD0D):X2} " +
                        $"DD0E={machine.Memory.Peek(0xDD0E):X2} " +
                        $"DD0F={machine.Memory.Peek(0xDD0F):X2} " +
                        $"01={machine.Memory.Peek(0x01):X2}");
                    output.WriteLine(
                        $"POST IEC: ATN={machine.Iec.AtnLow} " +
                        $"CLK={machine.Iec.ClockLow} DAT={machine.Iec.DataLow} " +
                        $"DRVPC={machine.Drive.RealDrive?.Cpu.GetPC():X4} " +
                        $"V1PB={machine.Drive.RealDrive?.Via1.PortBOutput:X2} " +
                        $"V2PB={machine.Drive.RealDrive?.Via2.PortBOutput:X2}");
                    output.WriteLine(
                        "DRIVE 05C0: " +
                        HexBytes(machine.Drive.RealDrive!.Ram, 0x05C0, 128));
                    output.WriteLine(
                        $"C64 F0={machine.Memory.Peek(0xF0):X2} " +
                        $"F1={machine.Memory.Peek(0xF1):X2} " +
                        $"F2={machine.Memory.Peek(0xF2):X2} " +
                        $"F3={machine.Memory.Peek(0xF3):X2} " +
                        $"DD02={machine.Memory.Peek(0xDD02):X2} " +
                        $"DC02={machine.Memory.Peek(0xDC02):X2}");

                    DumpPostStall(machine, output);

                }
            }

            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, true);
            machine.RunCycles(200_000);
            machine.Cia1.JoyEvent(2, JoystickFunction.Fire, false);
            machine.RunCycles(10_000_000);
            output.WriteLine(
                $"POSTFIRE PC={machine.Cpu.GetPC():X4} JAM={machine.Cpu.IsJammed} " +
                $"BYTES={machine.Drive.RealDrive?.Disk.TotalBytesRead} " +
                $"MOTOR={machine.Drive.RealDrive?.Disk.MotorOn}");
            output.WriteLine("POSTFIRE SCREEN:\n" + ScreenRows(machine.Memory));
            output.WriteLine("IEC TRACE:");

            foreach (string line in iecTrace)
                output.WriteLine("  " + line);
            output.WriteLine("LOADER 4000: " + Hex(machine.Memory, 0x4000, 768));
        }

        private static void DumpPostStall(
            C64Machine machine,
            ITestOutputHelper output)
        {
            string diagnostics = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "diagnostics"));

            Directory.CreateDirectory(diagnostics);

            byte[] c64 = new byte[0x1000];

            for (int i = 0; i < c64.Length; i++)
                c64[i] = machine.Memory.Peek((ushort)(0x3000 + i));

            File.WriteAllBytes(Path.Combine(diagnostics, "dotc-post-c64-3000.bin"), c64);

            Drive1541 drive = machine.Drive.RealDrive!;
            File.WriteAllBytes(Path.Combine(diagnostics, "dotc-post-drive-ram.bin"), drive.Ram);

            output.WriteLine("POST MICROTRACE");

            StringBuilder trace = new StringBuilder();

            for (int i = 0; i < 500; i++)
            {
                machine.RunCycles(1);

                if (i < 120 || i % 25 == 0)
                {
                    trace.AppendLine(
                        $"  {i} C64={machine.Cpu.GetPC():X4} " +
                        $"DD00={machine.Memory.Peek(0xDD00):X2} " +
                        $"DD02={machine.Memory.Peek(0xDD02):X2} " +
                        $"IEC={machine.Iec.AtnLow}/{machine.Iec.ClockLow}/{machine.Iec.DataLow} " +
                        $"DRV={drive.Cpu.GetPC():X4} V1PB={drive.Via1.PortBOutput:X2} " +
                        $"V1DDR={drive.Via1.DdrB:X2} V2PB={drive.Via2.PortBOutput:X2} " +
                        $"V2IN={drive.Disk.PortBInput:X2} TRK={drive.Ram[0x22]:X2} " +
                        $"HEAD={drive.Disk.HeadHalfTrack} SYNC={drive.Disk.Sync} " +
                        $"BYTES={drive.Disk.TotalBytesRead} SYNCS={drive.Disk.TotalSyncs} " +
                        $"DATA={drive.Disk.ReadByte:X2} P={drive.Cpu.P:X2} " +
                        $"A={drive.Cpu.A:X2} X={drive.Cpu.X:X2} Y={drive.Cpu.Y:X2} " +
                        $"S={drive.Cpu.S:X2}");
                }
            }

            File.WriteAllText(
                Path.Combine(diagnostics, "dotc-post-microtrace.txt"),
                trace.ToString());
            output.WriteLine(trace.ToString());
        }

        private static string ScreenRows(Memory memory)
        {
            StringBuilder text = new StringBuilder();

            for (int row = 0; row < 25; row++)
            {
                for (int column = 0; column < 40; column++)
                {
                    byte value = (byte)(
                        memory.Peek((ushort)(0x0400 + (row * 40) + column)) & 0x7F);
                    text.Append(value is >= 0x20 and < 0x7F ? (char)value : '.');
                }

                text.AppendLine();
            }

            return text.ToString();
        }

        private static string Hex(Memory memory, ushort address, int length)
        {
            return string.Join(
                " ",
                Enumerable.Range(0, length)
                    .Select(i => memory.Peek((ushort)(address + i)).ToString("X2")));
        }

        private static string HexBytes(byte[] memory, int address, int length)
        {
            return string.Join(
                " ",
                Enumerable.Range(0, length)
                    .Select(i => memory[address + i].ToString("X2")));
        }

        private static byte[] BuildDisk(
            int fileTrack,
            int fileSector,
            byte id1,
            byte id2)
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

            WritePaddedName(image, bam + 0x90, 16, "TEST DISK");
            image[bam + 0xA2] = id1;
            image[bam + 0xA3] = id2;
            image[bam + 0xA5] = (byte)'2';
            image[bam + 0xA6] = (byte)'A';

            for (int i = 0xA7; i <= 0xAA; i++)
                image[bam + i] = 0xA0;

            int directory = SectorOffset(18, 1);
            image[directory] = 0;
            image[directory + 1] = 0xFF;
            image[directory + 2] = 0x82;
            image[directory + 3] = (byte)fileTrack;
            image[directory + 4] = (byte)fileSector;
            WritePaddedName(image, directory + 5, 16, "TEST");
            image[directory + 30] = 1;
            image[directory + 31] = 0;

            int file = SectorOffset(fileTrack, fileSector);
            image[file] = 0;
            image[file + 1] = 0;
            image[file + 2] = 0x01;
            image[file + 3] = 0x08;

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

        private static void WaitForReady(C64Machine machine)
        {
            for (int pass = 0; pass < 500; pass++)
            {
                machine.RunCycles(50_000);

                if (ContainsReady(machine.Memory))
                    return;
            }

            Assert.Fail("The C64 did not reach READY.");
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

                    case '*':
                        Press(machine, Key.RightBracket, ModifierKeys.None);
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

        private static string ReadScreenTail(Memory memory)
        {
            StringBuilder text = new StringBuilder();

            for (int address = 0x07C0; address < 0x07E8; address++)
            {
                byte value = memory.Peek((ushort)address);
                text.Append(value is >= 0x20 and < 0x7F ? (char)value : '.');
            }

            return text.ToString().Trim();
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
