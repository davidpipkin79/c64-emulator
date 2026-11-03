using System.IO;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class C64MachineWarpTests
    {
        [Fact]
        public void WarpOnlyRunsWhileFastForwardIsActive()
        {
            string romDirectory = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "C64Emulator", "Roms"));

            string driveRom = Path.Combine(romDirectory, "dos1541.rom");

            if (!File.Exists(driveRom))
                return;

            using C64Machine machine = new C64Machine(
                RomSet.Load(romDirectory),
                File.ReadAllBytes(driveRom));

            machine.TrueDriveEmulation = true;

            Drive1541 drive = machine.Drive.RealDrive!;

            // Keep the drive CPU in RAM and hold the motor on. A spinning
            // drive must not enable warp by itself.
            drive.Ram[0] = 0x4C; // JMP $0000
            drive.Ram[1] = 0x00;
            drive.Ram[2] = 0x00;
            drive.Cpu.SetPC(0x0000);
            drive.Disk.SetPortB(0x04); // Motor on.

            Assert.True(drive.Disk.MotorOn);
            Assert.False(machine.IsWarping);

            machine.FastForward = true;
            Assert.True(machine.IsWarping);

            machine.FastForward = false;
            Assert.False(machine.IsWarping);
        }

        [Fact]
        public void WarpKeepsSidSynthesisRunning()
        {
            using C64Machine machine = new C64Machine(
                RomSet.Create(
                    new byte[AddressMap.KernalRomSize],
                    new byte[AddressMap.BasicRomSize],
                    new byte[AddressMap.CharacterRomSize]));

            machine.FastForward = true;
            machine.RunCycles(100_000);

            short[] samples = new short[4096];

            Assert.True(machine.Sid.ReadSamples(samples) > 0);
        }
    }
}
