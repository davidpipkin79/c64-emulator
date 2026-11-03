using System;
using System.IO;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class Drive1541Tests
    {
        [Fact]
        public void GcrRoundTripsAFullSector()
        {
            byte[] source = new byte[D64Image.SectorSize];
            Random random = new Random(1541);

            for (int i = 0; i < source.Length; i++)
                source[i] = (byte)random.Next(256);

            byte[] encoded = GcrCodec.Encode(source);

            Assert.Equal(320, encoded.Length);

            byte[] decoded = GcrCodec.Decode(encoded, source.Length);

            Assert.Equal(source, decoded);
        }

        [Fact]
        public void GcrNibbleTableMatchesThe1541Encoding()
        {
            Assert.Equal(0x0A, GcrCodec.EncodeNibble(0x0));
            Assert.Equal(0x0B, GcrCodec.EncodeNibble(0x1));
            Assert.Equal(0x12, GcrCodec.EncodeNibble(0x2));
            Assert.Equal(0x13, GcrCodec.EncodeNibble(0x3));
            Assert.Equal(0x15, GcrCodec.EncodeNibble(0xF));
        }

        [Fact]
        public void IecLinesAreOpenCollector()
        {
            IecBus bus = new IecBus();
            bool atnReleased = false;

            bus.AtnChanged += high => atnReleased |= high;

            bus.SetC64(0x08, 0x3C);
            Assert.True(bus.AtnLow);

            bus.SetDrive(clockLow: true, dataLow: false, atna: false);
            Assert.True(bus.ClockLow);
            Assert.False(bus.DataLow);

            bus.SetC64(0x00, 0x3C);
            Assert.False(bus.AtnLow);
            Assert.True(atnReleased);

            bus.SetDrive(clockLow: true, dataLow: true, atna: false);
            Assert.True(bus.ClockLow);
            Assert.True(bus.DataLow);

            bus.SetDrive(clockLow: false, dataLow: false, atna: false);
            Assert.True(bus.ClockHigh);
            Assert.True(bus.DataHigh);
        }

        [Fact]
        public void DriveBootsTheDosRom()
        {
            string romPath = Path.Combine(GetRomDirectory(), "dos1541.rom");

            if (!File.Exists(romPath))
                return;

            IecBus bus = new IecBus();
            Drive1541 drive = new Drive1541(File.ReadAllBytes(romPath), bus);

            Assert.Equal(0xEAA0, drive.Cpu.GetPC());

            drive.Tick(C64Machine.ClockHz * 3);

            ushort pc = drive.Cpu.GetPC();

            Assert.True(
                !drive.Cpu.IsJammed,
                "The 1541 CPU jammed at $" + pc.ToString("X4") + ".");
            Assert.True(
                pc >= 0xC000,
                "The 1541 DOS left ROM; PC=$" + pc.ToString("X4") + ".");
            Assert.Equal(0x1A, drive.Via1.DdrB);
            Assert.Equal(0x6F, drive.Via2.DdrB);
        }

        [Fact]
        public void DriveSeesAtnAndRunsItsCommandHandler()
        {
            string romPath = Path.Combine(GetRomDirectory(), "dos1541.rom");

            if (!File.Exists(romPath))
                return;

            IecBus bus = new IecBus();
            Drive1541 drive = new Drive1541(File.ReadAllBytes(romPath), bus);

            drive.Tick(C64Machine.ClockHz * 3);

            bus.SetC64(0x08, 0x3C);

            Assert.True(bus.AtnLow);
            Assert.True(drive.Via1.IrqAsserted);

            bool sawIrqVector = false;
            bool sawAtnHandler = false;
            bool sawFlag = false;

            for (int i = 0; i < 200_000; i++)
            {
                drive.Tick(1);

                ushort pc = drive.Cpu.GetPC();
                sawIrqVector |= pc == 0xFE67;
                sawAtnHandler |= pc == 0xE853;

                if (drive.Ram[0x7C] == 1)
                {
                    sawFlag = true;
                    break;
                }
            }

            Assert.True(
                sawFlag,
                "sawIrq=" + sawIrqVector +
                " sawAtn=" + sawAtnHandler +
                " PC=$" + drive.Cpu.GetPC().ToString("X4") +
                " P=$" + drive.Cpu.P.ToString("X2") +
                " PCR=$" + drive.Via1.Pcr.ToString("X2") +
                " IER=$" + drive.Via1.Ier.ToString("X2"));
        }

        [Fact]
        public void DriveAcknowledgesAtnOnTheDataLine()
        {
            string romPath = Path.Combine(GetRomDirectory(), "dos1541.rom");

            if (!File.Exists(romPath))
                return;

            IecBus bus = new IecBus();
            Drive1541 drive = new Drive1541(File.ReadAllBytes(romPath), bus);

            drive.Tick(C64Machine.ClockHz * 3);

            // The host asserts ATN and pulls CLK low to start a command.
            bus.SetC64(0x18, 0x3C);

            byte pins = drive.Via1.Read(0x00);

            Assert.True(
                (pins & 0x80) != 0,
                "ATN input did not read high at the VIA. PB=$" + pins.ToString("X2"));

            bool acknowledged = false;
            ushort lastPc = 0;

            for (int i = 0; i < 200_000; i++)
            {
                drive.Tick(1);
                lastPc = drive.Cpu.GetPC();

                if (bus.DataLow)
                {
                    acknowledged = true;
                    break;
                }
            }

            Assert.True(
                acknowledged,
                "The drive did not pull DATA low. PC=$" + lastPc.ToString("X4") +
                " 7C=$" + drive.Ram[0x7C].ToString("X2") +
                " PB=$" + drive.Via1.PortBOutput.ToString("X2") +
                " ATNIN=$" + drive.Ram[0x7C].ToString("X2"));
        }

        [Fact]
        public void GcrDiskProducesSyncAndBytes()
        {
            GcrDisk disk = new GcrDisk();
            disk.Mount(D64Image.FromBytes(CreateTestDisk()), null);

            // Motor on, phase 0.
            disk.SetPortB(0x04);

            int bytes = 0;
            bool sync = false;

            for (int i = 0; i < 500_000; i++)
            {
                disk.Tick(1);

                sync |= disk.Sync;

                if (disk.ConsumeByteReady())
                    bytes++;
            }

            Assert.True(sync, "No SYNC was detected on track 1.");
            Assert.True(bytes > 1000, "Only " + bytes + " bytes were read from track 1.");
        }

        [Fact]
        public void GcrSyncPulseLastsLongEnoughToPoll()
        {
            GcrDisk disk = new GcrDisk();
            disk.Mount(D64Image.FromBytes(CreateTestDisk()), null);
            disk.SetPortB(0x04);

            int run = 0;
            int longest = 0;

            for (int i = 0; i < 1_000_000; i++)
            {
                disk.Tick(1);

                if (disk.Sync)
                {
                    run++;
                    longest = Math.Max(longest, run);
                }
                else
                {
                    run = 0;
                }
            }

            Assert.True(
                longest > 20,
                "The longest SYNC pulse was only " + longest + " cycles.");
        }

        [Fact]
        public void DriveControllerReadsDirectorySector()
        {
            string romPath = Path.Combine(GetRomDirectory(), "dos1541.rom");

            if (!File.Exists(romPath))
                return;

            IecBus bus = new IecBus();
            Drive1541 drive = new Drive1541(File.ReadAllBytes(romPath), bus);
            byte[] imageBytes = CreateTestDisk();
            D64Image image = D64Image.FromBytes(imageBytes);

            drive.Mount(image, null);
            drive.Reset();
            drive.Tick(C64Machine.ClockHz);

            // Job 0: seek to track 18.
            drive.Ram[0x06] = 18;
            drive.Ram[0x07] = 0;
            drive.Ram[0x00] = 0xB0;

            RunUntilJobCompletes(drive, 0);

            Assert.Equal(1, drive.Ram[0x00]);

            // Job 0: seek to track 1, then read sector 0.
            drive.Ram[0x06] = 1;
            drive.Ram[0x07] = 0;
            drive.Ram[0x00] = 0xB0;

            RunUntilJobCompletes(drive, 0);

            drive.Ram[0x06] = 1;
            drive.Ram[0x07] = 0;
            drive.Ram[0x00] = 0x80;

            RunUntilJobCompletes(drive, 0);

            Assert.True(
                drive.Ram[0x00] == 1,
                "Track 1 read returned " + drive.Ram[0x00].ToString("X2") +
                " head=" + drive.Disk.HeadHalfTrack);

            // Job 0: read track 18 sector 0 into buffer 0.
            drive.Ram[0x06] = 18;
            drive.Ram[0x07] = 0;
            drive.Ram[0x00] = 0x80;

            RunUntilJobCompletes(drive, 0);

            Assert.True(
                drive.Ram[0x00] == 1,
                "Read job returned " + drive.Ram[0x00].ToString("X2") +
                " track=" + drive.Ram[0x80] +
                " sector=" + drive.Ram[0x81] +
                " drvtrk=" + drive.Ram[0x22] +
                " head=" + drive.Disk.HeadHalfTrack +
                " sync=" + drive.Disk.Sync +
                " syncs=" + drive.Disk.TotalSyncs +
                " bytes=" + drive.Disk.TotalBytesRead +
                " ddrb=" + drive.Via2.DdrB.ToString("X2") +
                " pb=" + drive.Via2.PortBOutput.ToString("X2") +
                " pc=" + drive.Cpu.GetPC().ToString("X4") +
                " v1t=" + drive.Via1.Read(0x05).ToString("X2") +
                " acr=" + drive.Via2.Acr.ToString("X2") +
                " write=" + drive.Disk.WriteMode +
                " pblow=" + drive.Via2.PortBReadsLow +
                " motor=" + drive.Disk.MotorOn +
                " head=" + drive.Disk.HeadHalfTrack);

            ReadOnlySpan<byte> expected = image.GetSector(18, 0);

            Assert.True(
                expected.SequenceEqual(
                    new ReadOnlySpan<byte>(drive.Ram, 0x0300, 256)),
                "The controller did not fill buffer 0 with sector 18/0.");
        }

        private static void RunUntilJobCompletes(Drive1541 drive, int job)
        {
            System.Collections.Generic.List<string> trace =
                new System.Collections.Generic.List<string>();

            long lastSyncs = 0;
            long lastBytes = 0;

            for (int i = 0; i < 500; i++)
            {
                drive.Tick(10_000);
                long syncs = drive.Disk.TotalSyncs;
                long bytesRead = drive.Disk.TotalBytesRead;

                trace.Add(
                    i +
                    " job=" + drive.Ram[job].ToString("X2") +
                    " pc=" + drive.Cpu.GetPC().ToString("X4") +
                    " head=" + drive.Disk.HeadHalfTrack +
                    " trk=" + drive.Ram[0x22] +
                    " steps=" + drive.Ram[0x4A] +
                    " drvst=" + drive.Ram[0x20].ToString("X2") +
                    " v2=" + drive.Via2.PortBOutput.ToString("X2") +
                    " syncs=" + (syncs - lastSyncs) +
                    " bytes=" + (bytesRead - lastBytes) +
                    " sync=" + drive.Disk.Sync);

                lastSyncs = syncs;
                lastBytes = bytesRead;

                if ((drive.Ram[job] & 0x80) == 0)
                    break;
            }

            File.WriteAllLines(
                Path.Combine(GetRomDirectory(), "..", "..", "diagnostics", "job-trace.txt"),
                trace);

            if ((drive.Ram[job] & 0x80) != 0)
            {

                Assert.Fail(
                    "Job " + job + " did not complete: " +
                    drive.Ram[job].ToString("X2") +
                    " PC=" + drive.Cpu.GetPC().ToString("X4") +
                    " head=" + drive.Disk.HeadHalfTrack);
            }
        }

        [Fact]
        public void GcrTrackProducesDecodableHeadersAndData()
        {
            byte[] imageBytes = CreateTestDisk();
            D64Image image = D64Image.FromBytes(imageBytes);
            GcrDisk disk = new GcrDisk();
            disk.Mount(image, null);

            // Motor on, then walk the head to track 18 (half-track 34).
            disk.SetPortB(0x04);

            for (int step = 1; step <= 34; step++)
                disk.SetPortB((byte)(0x04 | (step & 0x03)));

            Assert.Equal(18, disk.CurrentTrack);

            System.Collections.Generic.List<byte> bytes =
                new System.Collections.Generic.List<byte>();

            for (int i = 0; i < 1_500_000 && bytes.Count < 80_000; i++)
            {
                disk.Tick(1);

                if (disk.ConsumeByteReady())
                    bytes.Add(disk.ReadByte);
            }

            byte[] stream = bytes.ToArray();
            File.WriteAllBytes(
                Path.Combine(GetRomDirectory(), "..", "..", "diagnostics", "gcr-stream.bin"),
                stream);
            int headerIndex = -1;
            int headerSector = -1;
            string detail = string.Empty;

            for (int index = 0; index + 10 <= stream.Length; index++)
            {
                if (stream[index] != 0x52)
                    continue;

                Span<byte> header = stackalloc byte[6];
                GcrCodec.Decode(stream.AsSpan(index, 8), header);

                byte checksum = (byte)(
                    header[2] ^ header[3] ^ header[4] ^ header[5]);

                if (header[0] != 0x08 || header[1] != checksum)
                    continue;

                detail =
                    "track=" + header[3] +
                    " sector=" + header[2] +
                    " id=" + header[4].ToString("X2") + header[5].ToString("X2");

                if (header[3] != 18 || index + 344 > stream.Length)
                    continue;

                headerIndex = index;
                headerSector = header[2];
                break;
            }

            Assert.True(
                headerIndex >= 0,
                "No valid track-18 header was found in " + stream.Length +
                " bytes. " + detail);

            // The controller hides the sync bytes from the byte stream, but
            // the 0xFF byte that completed just before SYNC was detected is
            // still latched (the DOS discards it with a dummy read). After
            // the ten header bytes, nine gap bytes and that stray byte, the
            // 325-byte data block follows.
            int sync = headerIndex + 10 + 9 + 1;

            Span<byte> block = stackalloc byte[260];
            GcrCodec.Decode(stream.AsSpan(sync, 325), block);

            Assert.True(
                block[0] == 0x07,
                "Data block id was " + block[0].ToString("X2") +
                "; header at " + headerIndex +
                " bytes " + Convert.ToHexString(
                    stream.AsSpan(headerIndex, Math.Min(48, stream.Length - headerIndex))));

            byte dataChecksum = 0;

            for (int i = 1; i <= 256; i++)
                dataChecksum ^= block[i];

            Assert.Equal(dataChecksum, block[257]);

            ReadOnlySpan<byte> expected = image.GetSector(18, headerSector);

            Assert.True(
                expected.SequenceEqual(block.Slice(1, 256)),
                "The decoded data block did not match track 18 sector " +
                headerSector + ".");
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

            return image;
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
