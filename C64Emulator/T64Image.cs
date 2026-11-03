using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace C64Emulator
{
    internal sealed class T64FileInfo
    {
        public T64FileInfo(
            string name,
            CbmFileType type,
            ushort startAddress,
            ushort endAddress,
            int dataOffset,
            int dataLength)
        {
            Name = name;
            Type = type;
            StartAddress = startAddress;
            EndAddress = endAddress;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string Name { get; }

        public CbmFileType Type { get; }

        public ushort StartAddress { get; }

        public ushort EndAddress { get; }

        public int DataOffset { get; }

        public int DataLength { get; }

        public bool IsProgram => Type == CbmFileType.Program;
    }

    /// <summary>
    /// A C64S T64 tape container. T64 stores decoded file payloads rather than
    /// tape pulses, so it is used as a virtual tape source, not as a datasette
    /// signal image.
    /// </summary>
    internal sealed class T64Image
    {
        private const int HeaderSize = 64;
        private const int RecordSize = 32;

        private static readonly string[] KnownSignatures =
        {
            "C64S tape image file",
            "C64S tape file",
            "C64 tape image file",
            "C64S image file"
        };

        private readonly byte[] data;

        private T64Image(
            byte[] data,
            int version,
            string tapeName,
            T64FileInfo[] directory)
        {
            this.data = data;
            Version = version;
            TapeName = tapeName;
            Directory = directory;
        }

        public int Version { get; }

        public string TapeName { get; }

        public IReadOnlyList<T64FileInfo> Directory { get; }

        public static T64Image Load(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return FromBytes(File.ReadAllBytes(path));
        }

        public static T64Image FromBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            if (bytes.Length < HeaderSize)
                throw new InvalidDataException("The file is too small to be a T64 image.");

            string signature = Encoding.ASCII
                .GetString(bytes, 0, 32)
                .TrimEnd('\0', ' ');

            if (!IsKnownSignature(signature))
            {
                throw new InvalidDataException(
                    "The file does not have a recognised T64 signature.");
            }

            int version = ReadUInt16(bytes, 32);
            int maxRecords = ReadUInt16(bytes, 34);
            int usedRecords = ReadUInt16(bytes, 36);
            int availableRecords = Math.Max(0, (bytes.Length - HeaderSize) / RecordSize);
            int recordCount = usedRecords > 0 ? usedRecords : maxRecords;

            if (recordCount <= 0)
                recordCount = Math.Min(1, availableRecords);

            if (maxRecords > 0)
                recordCount = Math.Min(recordCount, maxRecords);

            recordCount = Math.Min(recordCount, availableRecords);

            string tapeName = ReadName(bytes.AsSpan(40, 24));
            HashSet<int> dataOffsets = new HashSet<int>();

            for (int i = 0; i < recordCount; i++)
            {
                int record = HeaderSize + (i * RecordSize);
                int offset = ReadInt32(bytes, record + 8);

                if (bytes[record] != 0 && offset >= HeaderSize && offset < bytes.Length)
                    dataOffsets.Add(offset);
            }

            List<T64FileInfo> files = new List<T64FileInfo>();

            for (int i = 0; i < recordCount; i++)
            {
                int record = HeaderSize + (i * RecordSize);

                // Only normal tape files are supported. Header blocks, C64S
                // snapshots and digitised streams have different layouts.
                if (bytes[record] != 1)
                    continue;

                int offset = ReadInt32(bytes, record + 8);

                if (offset < HeaderSize || offset >= bytes.Length)
                    continue;

                int nextOffset = bytes.Length;

                foreach (int candidate in dataOffsets)
                {
                    if (candidate > offset && candidate < nextOffset)
                        nextOffset = candidate;
                }

                int availableLength = nextOffset - offset;

                if (availableLength <= 0)
                    continue;

                ushort startAddress = ReadUInt16(bytes, record + 2);
                ushort endAddress = ReadUInt16(bytes, record + 4);

                // The reported end address is often wrong. Prefer it when it
                // fits, otherwise use the next payload boundary.
                int reportedLength = endAddress > startAddress
                    ? endAddress - startAddress
                    : 0;
                int length = reportedLength > 0 && reportedLength <= availableLength
                    ? reportedLength
                    : availableLength;

                string name = ReadName(bytes.AsSpan(record + 16, 16));
                byte cbmType = bytes[record + 1];
                int typeValue = cbmType & 0x0F;
                CbmFileType type = typeValue is >= 1 and <= 4
                    ? (CbmFileType)typeValue
                    : CbmFileType.Program;

                files.Add(
                    new T64FileInfo(
                        name,
                        type,
                        startAddress,
                        endAddress,
                        offset,
                        length));
            }

            return new T64Image(bytes, version, tapeName, files.ToArray());
        }

        public byte[] ReadFile(T64FileInfo file)
        {
            ArgumentNullException.ThrowIfNull(file);

            byte[] payload = new byte[file.DataLength];
            Array.Copy(data, file.DataOffset, payload, 0, file.DataLength);
            return payload;
        }

        /// <summary>
        /// Builds the PRG form expected by the direct loader: a little-endian
        /// load address followed by the T64 payload.
        /// </summary>
        public byte[] ToPrg(T64FileInfo file)
        {
            byte[] payload = ReadFile(file);
            byte[] prg = new byte[payload.Length + 2];

            prg[0] = (byte)(file.StartAddress & 0xFF);
            prg[1] = (byte)(file.StartAddress >> 8);
            payload.CopyTo(prg, 2);
            return prg;
        }

        public T64FileInfo? FindFile(string name)
        {
            string pattern = (name ?? string.Empty).Trim();

            if (pattern.Length == 0 || pattern == "*")
                return FirstProgram();

            if (pattern == "$")
                return null;

            string normalized = pattern.ToUpperInvariant();

            foreach (T64FileInfo file in Directory)
            {
                if (Wildcard.Matches(file.Name.ToUpperInvariant(), normalized))
                    return file;
            }

            return null;
        }

        public T64FileInfo? FirstProgram()
        {
            foreach (T64FileInfo file in Directory)
            {
                if (file.IsProgram)
                    return file;
            }

            return null;
        }

        private static string ReadName(ReadOnlySpan<byte> bytes)
        {
            int terminator = bytes.IndexOf((byte)0);

            if (terminator >= 0)
                bytes = bytes.Slice(0, terminator);

            return Petscii.ToAscii(bytes);
        }

        private static bool IsKnownSignature(string signature)
        {
            foreach (string known in KnownSignatures)
            {
                if (signature.StartsWith(known, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static ushort ReadUInt16(byte[] bytes, int offset)
        {
            return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
        }

        private static int ReadInt32(byte[] bytes, int offset)
        {
            return bytes[offset] |
                (bytes[offset + 1] << 8) |
                (bytes[offset + 2] << 16) |
                (bytes[offset + 3] << 24);
        }
    }
}
