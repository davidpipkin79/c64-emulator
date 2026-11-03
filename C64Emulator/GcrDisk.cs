using System;
using System.Collections.Generic;

namespace C64Emulator
{
    /// <summary>
    /// The disk-controller side of a 1541: a circular GCR track image, a
    /// rotating head, SYNC detection, byte-ready timing and the write path
    /// back into a D64 image.
    ///
    /// The drive's DOS sees bytes through VIA2 exactly as it would on real
    /// hardware: five $FF sync bytes are hidden from the byte stream, the
    /// SYNC input goes low while ten or more one-bits pass the head, and a
    /// byte-ready pulse occurs for every eight non-sync bits. Reading a
    /// sector therefore follows the same code path in the DOS as it does on
    /// a physical 1541.
    /// </summary>
    internal sealed class GcrDisk
    {
        public const int MaxTrack = 40;
        private const int DriveHz = 1_000_000;
        private const int HeaderGcrLength = 10;
        private const int DataGcrLength = 325;

        // 1541 speed zones, outer track first.
        private static readonly int[] zoneBitsPerSecond =
        {
            250000, 266667, 285714, 307692
        };

        // A header is ten GCR bytes encoding eight binary bytes:
        // $08, checksum, sector, track, ID2, ID1, $00, $00. A data block is
        // 325 GCR bytes encoding $07, 256 data bytes, the checksum and two
        // filler bytes, exactly as the 1541 DOS builds its write image.
        private readonly byte[] headerGcr = new byte[10];
        private readonly byte[] gcrData = new byte[325];

        private D64Image? image;
        private string? imagePath;
        private byte[]?[] tracks = new byte[]?[MaxTrack + 1];

        private int headHalfTrack;
        private int headBit;
        private int bitCounter;
        private int writeBitCounter;
        private ushort lastReadBits;
        private byte readByte;
        private byte writeValue;
        private byte writeShift;
        private bool writeLatchValid;
        private bool motorOn;
        private bool writeMode;
        private bool byteReady;
        private bool writeProtected;
        private int density;
        private int phase;
        private long bitAccumulator;

        private int headerStage;
        private int headerIndex;
        private int headerSyncRun;
        private int writeSyncRun;
        private int dataStage;
        private int dataCount;
        private int lastHeaderTrack;
        private int lastHeaderSector;

        public bool IsMounted => image is not null;

        public bool MotorOn => motorOn;

        public byte ReadByte => readByte;

        /// <summary>Diagnostic counters used by the drive tests.</summary>
        public long TotalBytesRead { get; private set; }

        public long TotalSyncs { get; private set; }

        /// <summary>Diagnostic access to a generated track image.</summary>
        public byte[]? TrackImageForTest(int track) =>
            track >= 0 && track < tracks.Length ? tracks[track] : null;

        public event Action<byte>? ByteProduced;

        /// <summary>Diagnostic event for every valid sector header seen.</summary>
        public event Action<int, int>? HeaderDecoded;

        public bool WriteProtected
        {
            get => writeProtected;
            set => writeProtected = value;
        }

        public bool WriteMode
        {
            get => writeMode;
            set
            {
                if (writeMode == value)
                    return;

                writeMode = value;
                writeBitCounter = 0;
                writeLatchValid = false;
                dataStage = 0;
                dataCount = 0;
            }
        }

        public bool Sync =>
            motorOn &&
            !writeMode &&
            CurrentTrackData is not null &&
            lastReadBits == 0x3FF;

        /// <summary>
        /// Input pins for VIA2 port B. Bit 4 is the write-protect photocell
        /// (1 = not protected) and bit 7 is the SYNC input (0 = sync found).
        /// </summary>
        public byte PortBInput =>
            (byte)((writeProtected ? 0x00 : 0x10) | (Sync ? 0x00 : 0x80));

        public void Mount(D64Image image, string? path)
        {
            this.image = image ?? throw new ArgumentNullException(nameof(image));
            imagePath = path;
            BuildTracks();
            ResetState();
        }

        public void Eject()
        {
            Flush();
            image = null;
            imagePath = null;
            tracks = new byte[]?[MaxTrack + 1];
            ResetState();
        }

        public void Reset()
        {
            ResetState();
            motorOn = false;
            density = 0;
            phase = 0;
        }

        public void Flush()
        {
            if (image is not null && imagePath is not null && image.IsDirty)
                image.Save(imagePath);
        }

        public bool ConsumeByteReady()
        {
            bool ready = byteReady;
            byteReady = false;
            return ready;
        }

        public void WriteByte(byte value)
        {
            writeValue = value;
            writeShift = value;
            writeLatchValid = true;
            writeBitCounter = 0;
        }

        public void SetPortB(byte value)
        {
            int newPhase = value & 0x03;

            if (newPhase != phase)
            {
                int delta = (newPhase - phase + 4) & 3;

                if (delta == 1)
                    headHalfTrack = Math.Min(headHalfTrack + 1, (MaxTrack - 1) * 2);
                else if (delta == 3)
                    headHalfTrack = Math.Max(headHalfTrack - 1, 0);

                phase = newPhase;
                OnHeadMoved();
            }

            bool newMotor = (value & 0x04) != 0;

            if (motorOn != newMotor)
            {
                motorOn = newMotor;

                if (!motorOn)
                {
                    byteReady = false;
                    lastReadBits = 0;
                }
            }

            density = (value >> 5) & 0x03;
        }

        public void Tick(int cycles)
        {
            if (cycles <= 0 || !motorOn)
                return;

            byte[]? track = CurrentTrackData;

            if (track is null)
                return;

            int bitsPerSecond = BitsPerSecondForTrack(headHalfTrack);

            for (int i = 0; i < cycles; i++)
            {
                bitAccumulator += bitsPerSecond;

                while (bitAccumulator >= DriveHz)
                {
                    bitAccumulator -= DriveHz;
                    ShiftBit(track);
                }
            }
        }

        /// <summary>The current half-track position, where 0 is track 1.</summary>
        public int HeadHalfTrack => headHalfTrack;

        /// <summary>The one-based track currently under the head.</summary>
        public int CurrentTrack =>
            Math.Clamp((headHalfTrack / 2) + 1, 1, MaxTrack);

        private byte[]? CurrentTrackData
        {
            get
            {
                if (image is null || (headHalfTrack & 1) != 0)
                    return null;

                int track = CurrentTrack;

                if (track < 1 || track > MaxTrack)
                    return null;

                return tracks[track];
            }
        }

        private void ShiftBit(byte[] track)
        {
            int byteIndex = (headBit >> 3) % track.Length;
            int mask = 1 << (7 - (headBit & 7));

            if (writeMode)
            {
                int outputBit = (writeShift & 0x80) != 0 ? 1 : 0;
                writeShift = (byte)(writeShift << 1);

                if (outputBit != 0)
                    track[byteIndex] |= (byte)mask;
                else
                    track[byteIndex] &= (byte)~mask;

                lastReadBits = (ushort)(((lastReadBits << 1) | outputBit) & 0x3FF);
                headBit = (headBit + 1) % (track.Length * 8);

                if (++writeBitCounter == 8)
                {
                    writeBitCounter = 0;
                    byteReady = true;

                    if (writeLatchValid)
                    {
                        FeedHeader(writeValue);
                        FeedWrite(writeValue);
                        writeLatchValid = false;
                    }
                }

                return;
            }

            int inputBit = (track[byteIndex] & mask) != 0 ? 1 : 0;
            lastReadBits = (ushort)(((lastReadBits << 1) | inputBit) & 0x3FF);
            headBit = (headBit + 1) % (track.Length * 8);

            if (lastReadBits == 0x3FF)
            {
                // Ten one-bits is the sync mark. The shifter is cleared and
                // no data byte is produced for the sync run itself.
                bitCounter = 0;
                TotalSyncs++;
                return;
            }

            if (++bitCounter == 8)
            {
                bitCounter = 0;
                readByte = (byte)(lastReadBits & 0xFF);
                byteReady = true;
                TotalBytesRead++;
                ByteProduced?.Invoke(readByte);
                FeedHeader(readByte);
            }
        }

        private void FeedHeader(byte value)
        {
            if (value == 0xFF)
            {
                headerSyncRun++;
                headerStage = 0;
                return;
            }

            int sync = headerSyncRun;
            headerSyncRun = 0;

            if (sync >= 5)
            {
                headerStage = 1;
                headerIndex = 0;
            }

            if (headerStage != 1)
                return;

            headerGcr[headerIndex++] = value;

            if (headerIndex < HeaderGcrLength)
                return;

            headerStage = 0;
            Span<byte> header = stackalloc byte[8];
            GcrCodec.Decode(headerGcr, header);

            if (header[0] != 0x08)
                return;

            byte checksum = (byte)(header[2] ^ header[3] ^ header[4] ^ header[5]);

            if (checksum != header[1])
                return;

            lastHeaderSector = header[2];
            lastHeaderTrack = header[3];
            HeaderDecoded?.Invoke(lastHeaderTrack, lastHeaderSector);
        }

        private void FeedWrite(byte value)
        {
            if (value == 0xFF)
            {
                writeSyncRun++;
                dataStage = 0;
                return;
            }

            int sync = writeSyncRun;
            writeSyncRun = 0;

            if (sync >= 5)
            {
                dataStage = 1;
                dataCount = 0;
            }

            if (dataStage != 1)
                return;

            gcrData[dataCount++] = value;

            if (dataCount < DataGcrLength)
                return;

            dataStage = 0;
            CommitSector();
        }

        private void CommitSector()
        {
            if (image is null)
                return;

            int track = lastHeaderTrack;
            int sector = lastHeaderSector;

            if (
                track < 1 ||
                track > image.TrackCount ||
                sector < 0 ||
                sector >= D64Image.SectorsPerTrack(track))
            {
                return;
            }

            Span<byte> block = stackalloc byte[260];
            GcrCodec.Decode(gcrData, block);

            if (block[0] != 0x07)
                return;

            byte checksum = 0;

            for (int i = 1; i <= D64Image.SectorSize; i++)
                checksum ^= block[i];

            if (checksum != block[257])
                return;

            image.SetSector(track, sector, block.Slice(1, D64Image.SectorSize));

            if (imagePath is not null)
                image.Save(imagePath);
        }

        private void OnHeadMoved()
        {
            bitCounter = 0;
            writeBitCounter = 0;
            lastReadBits = 0;
            readByte = 0;
            byteReady = false;
            writeLatchValid = false;
            bitAccumulator = 0;
            headerStage = 0;
            headerIndex = 0;
            headerSyncRun = 0;
            writeSyncRun = 0;
            dataStage = 0;
            dataCount = 0;

            byte[]? track = CurrentTrackData;

            if (track is null)
                headBit = 0;
            else
                headBit %= track.Length * 8;
        }

        private void ResetState()
        {
            headHalfTrack = 0;
            headBit = 0;
            bitCounter = 0;
            writeBitCounter = 0;
            lastReadBits = 0;
            readByte = 0;
            writeValue = 0;
            writeShift = 0;
            writeLatchValid = false;
            writeMode = false;
            byteReady = false;
            bitAccumulator = 0;
            headerStage = 0;
            headerIndex = 0;
            headerSyncRun = 0;
            writeSyncRun = 0;
            dataStage = 0;
            dataCount = 0;
            lastHeaderTrack = 0;
            lastHeaderSector = 0;
        }

        private void BuildTracks()
        {
            tracks = new byte[]?[MaxTrack + 1];

            if (image is null)
                return;

            for (int track = 1; track <= image.TrackCount; track++)
                tracks[track] = BuildTrack(image, track);
        }

        private static byte[] BuildTrack(D64Image image, int track)
        {
            int size = TrackSize(track);
            byte[] result = new byte[size];
            Array.Fill(result, (byte)0x55);

            // Use the raw BAM bytes, including zeroes. The display string
            // turns those into "??", which makes the GCR header checksum
            // disagree with the BAM and the DOS rejects every sector.
            byte[] diskId = image.DiskIdBytes;
            byte id1 = diskId.Length > 0 ? diskId[0] : (byte)0x00;
            byte id2 = diskId.Length > 1 ? diskId[1] : (byte)0x00;

            int position = 12;
            int sectors = D64Image.SectorsPerTrack(track);

            for (int sector = 0; sector < sectors; sector++)
            {
                byte checksum = (byte)(track ^ sector ^ id2 ^ id1);
                byte[] header = GcrCodec.Encode(
                    new byte[]
                    {
                        0x08,
                        checksum,
                        (byte)sector,
                        (byte)track,
                        id2,
                        id1,
                        0x00,
                        0x00
                    });

                for (int i = 0; i < 5; i++)
                    result[position++] = 0xFF;

                header.CopyTo(result, position);
                position += header.Length;

                for (int i = 0; i < 9; i++)
                    result[position++] = 0x55;

                // Data block: $07, 256 data bytes, checksum and two filler
                // bytes, all encoded into 325 GCR bytes.
                ReadOnlySpan<byte> payload = image.GetSector(track, sector);
                byte[] block = new byte[260];
                block[0] = 0x07;
                payload.CopyTo(block.AsSpan(1));

                byte dataChecksum = 0;

                foreach (byte value in payload)
                    dataChecksum ^= value;

                block[257] = dataChecksum;

                byte[] encoded = GcrCodec.Encode(block);

                for (int i = 0; i < 5; i++)
                    result[position++] = 0xFF;

                encoded.CopyTo(result, position);
                position += encoded.Length;

                for (int i = 0; i < 8; i++)
                    result[position++] = 0x55;
            }

            if (position > size)
                throw new InvalidOperationException(
                    "GCR track " + track + " does not fit in " + size + " bytes.");

            return result;
        }

        private static int TrackDensity(int track)
        {
            if (track <= 17)
                return 3;

            if (track <= 24)
                return 2;

            if (track <= 30)
                return 1;

            return 0;
        }

        private static int TrackSize(int track)
        {
            // One revolution at 300 RPM is 200 ms, so the byte count is the
            // zone bit rate divided by 40. BitsPerSecondForTrack expects a
            // half-track, where track 1 begins at half-track 0.
            return (int)Math.Round(
                BitsPerSecondForTrack((track - 1) * 2) / 40.0);
        }

        private static int BitsPerSecondForTrack(int halfTrack)
        {
            int track = Math.Clamp((halfTrack / 2) + 1, 1, MaxTrack);
            return zoneBitsPerSecond[TrackDensity(track)];
        }
    }
}
