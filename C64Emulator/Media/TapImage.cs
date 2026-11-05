using System;
using System.Collections.Generic;
using System.IO;

namespace C64Emulator.Media;

internal sealed class TapFileInfo(
    string name,
    CbmFileType type,
    ushort startAddress,
    ushort endAddress,
    byte[] data) : ITapeFileInfo
{
    public string Name { get; } = name;

    public CbmFileType Type { get; } = type;

    public ushort StartAddress { get; } = startAddress;

    public ushort EndAddress { get; } = endAddress;

    public byte[] Data { get; } = data;

    public int DataLength => Data.Length;

    public bool IsProgram => Type == CbmFileType.Program;
}

/// <summary>
/// A raw TAP tape image. TAP stores datasette pulse widths, not files, so
/// the pulse stream is retained for datasette playback. Standard CBM pulse
/// encoding is also decoded into 192-byte blocks so the menu can list files
/// and T64-style virtual loads remain available.
/// </summary>
internal sealed class TapImage
{
    private const int HeaderSize = 20;
    private const int BlockPayloadSize = 192;
    private const int BlockCountdownSize = 9;
    private const int BlockSize = BlockCountdownSize + BlockPayloadSize + 1;

    private static ReadOnlySpan<byte> Signature => "C64-TAPE-RAW"u8;

    private TapImage(
        string tapeName,
        int version,
        List<int> pulses,
        TapFileInfo[] directory)
    {
        TapeName = tapeName;
        Version = version;
        Pulses = pulses;
        Directory = directory;
    }

    public string TapeName { get; }

    public int Version { get; }

    /// <summary>
    /// Pulse widths in C64 clock cycles, in the order they appear on the
    /// tape. These drive the datasette's FLAG line for custom loaders.
    /// </summary>
    public IReadOnlyList<int> Pulses { get; }

    public IReadOnlyList<TapFileInfo> Directory { get; }

    public static TapImage Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return FromBytes(
            File.ReadAllBytes(path),
            Path.GetFileNameWithoutExtension(path));
    }

    public static TapImage FromBytes(byte[] bytes, string? tapeName = null)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length < HeaderSize)
            throw new InvalidDataException(
                "The file is too small to be a TAP image.");

        if (!bytes.AsSpan(0, Signature.Length).SequenceEqual(Signature))
            throw new InvalidDataException(
                "The file does not have a C64-TAPE-RAW signature.");

        var version = bytes[12];

        if (version is not (0 or 1))
        {
            throw new InvalidDataException(
                "Only TAP versions 0 and 1 are supported; found " +
                version +
                ".");
        }

        var declaredLength =
            bytes[16] |
            (bytes[17] << 8) |
            (bytes[18] << 16) |
            (bytes[19] << 24);
        var dataLength = Math.Min(
            Math.Max(declaredLength, 0),
            bytes.Length - HeaderSize);

        if (dataLength <= 0)
            throw new InvalidDataException("The TAP image has no pulse data.");

        var pulses = DecodePulses(
            bytes.AsSpan(HeaderSize, dataLength),
            version);
        var decoded = DecodeBytes(pulses);
        var blocks = DecodeBlocks(decoded);
        var files = BuildDirectory(blocks);

        return new TapImage(
            tapeName ?? "TAP",
            version,
            pulses,
            [.. files]);
    }

    public byte[] ReadFile(TapFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return (byte[])file.Data.Clone();
    }

    public byte[] ToPrg(TapFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var prg = new byte[file.Data.Length + 2];
        prg[0] = (byte)(file.StartAddress & 0xFF);
        prg[1] = (byte)(file.StartAddress >> 8);
        file.Data.CopyTo(prg, 2);
        return prg;
    }

    public TapFileInfo? FindFile(string name)
    {
        var pattern = (name ?? string.Empty).Trim();

        if (pattern.Length == 0 || pattern == "*")
            return FirstProgram();

        if (pattern == "$")
            return null;

        var normalized = pattern.ToUpperInvariant();

        foreach (TapFileInfo file in Directory)
        {
            if (WildcardMatcher.Matches(file.Name.ToUpperInvariant(), normalized))
                return file;
        }

        return null;
    }

    public TapFileInfo? FirstProgram()
    {
        foreach (TapFileInfo file in Directory)
        {
            if (file.IsProgram)
                return file;
        }

        return null;
    }

    private static List<int> DecodePulses(
        ReadOnlySpan<byte> payload,
        int version)
    {
        List<int> pulses = new(payload.Length);

        for (var i = 0; i < payload.Length; i++)
        {
            var value = payload[i];

            if (value != 0)
            {
                pulses.Add(value * 8);
                continue;
            }

            if (version == 0)
            {
                // TAP v0 uses zero for any pulse longer than 255*8 cycles.
                pulses.Add(256 * 8);
                continue;
            }

            if (i + 3 >= payload.Length)
                throw new InvalidDataException(
                    "A TAP v1 overflow pulse is truncated.");

            var cycles =
                payload[i + 1] |
                (payload[i + 2] << 8) |
                (payload[i + 3] << 16);
            pulses.Add(cycles > 0 ? cycles : 1);
            i += 3;
        }

        return pulses;
    }

    /// <summary>
    /// Decodes CBM pulse pairs. A long/medium pair marks a byte, eight
    /// short/medium or medium/short pairs carry the bits LSB first, and the
    /// following pair is the parity bit.
    /// </summary>
    private static List<byte> DecodeBytes(List<int> pulses)
    {
        List<byte> bytes = [];
        var index = 0;

        while (index + 1 < pulses.Count)
        {
            if (
                Classify(pulses[index]) != PulseKind.Long ||
                Classify(pulses[index + 1]) != PulseKind.Medium)
            {
                index++;
                continue;
            }

            var next = index + 2;
            var value = 0;
            var valid = true;

            for (var bit = 0; bit < 8; bit++)
            {
                if (next + 1 >= pulses.Count)
                {
                    valid = false;
                    break;
                }

                var first = Classify(pulses[next]);
                var second = Classify(pulses[next + 1]);

                if (first == PulseKind.Short && second == PulseKind.Medium)
                {
                    // A zero bit.
                }
                else if (
                    first == PulseKind.Medium &&
                    second == PulseKind.Short)
                {
                    value |= 1 << bit;
                }
                else
                {
                    valid = false;
                    break;
                }

                next += 2;
            }

            if (!valid)
            {
                index++;
                continue;
            }

            // Ignore the parity pair. The next marker search starts after it.
            if (next + 1 < pulses.Count)
                next += 2;

            bytes.Add((byte)value);
            index = next;
        }

        return bytes;
    }

    private static List<TapeBlock> DecodeBlocks(List<byte> bytes)
    {
        List<TapeBlock> blocks = [];

        for (var index = 0; index + BlockSize <= bytes.Count;)
        {
            var primary = IsCountdown(bytes, index, 0x89);
            var backup = !primary && IsCountdown(bytes, index, 0x09);

            if (!primary && !backup)
            {
                index++;
                continue;
            }

            var payloadStart = index + BlockCountdownSize;
            var payload = new byte[BlockPayloadSize];
            bytes.CopyTo(payloadStart, payload, 0, BlockPayloadSize);

            byte checksum = 0;

            foreach (var value in payload)
                checksum ^= value;

            if (checksum == bytes[payloadStart + BlockPayloadSize])
            {
                blocks.Add(new TapeBlock(payload, primary));
            }

            index = payloadStart + BlockPayloadSize + 1;
        }

        return blocks;
    }

    private static List<TapFileInfo> BuildDirectory(List<TapeBlock> blocks)
    {
        List<TapeBlock> primary = [];

        foreach (TapeBlock block in blocks)
        {
            if (block.Primary)
                primary.Add(block);
        }

        if (primary.Count == 0)
            primary = blocks;

        List<TapFileInfo> files = [];
        var index = 0;

        while (index < primary.Count)
        {
            TapeBlock header = primary[index];
            var headerType = header.Payload[0] & 0x0F;

            if (headerType is < 1 or > 4)
            {
                index++;
                continue;
            }

            var start = ReadUInt16(header.Payload, 1);
            var end = ReadUInt16(header.Payload, 3);
            var expected = end > start ? end - start : 0;
            List<byte> data = [];
            var next = index + 1;

            while (next < primary.Count)
            {
                if (expected > 0 && data.Count >= expected)
                    break;

                if (expected == 0)
                {
                    var nextType = primary[next].Payload[0] & 0x0F;

                    if (nextType is >= 1 and <= 4)
                        break;
                }

                data.AddRange(primary[next].Payload);
                next++;
            }

            var length = expected > 0
                ? Math.Min(expected, data.Count)
                : data.Count;
            var payload = new byte[length];

            for (var i = 0; i < length; i++)
                payload[i] = data[i];

            var name = Petscii
                .ToAscii(header.Payload.AsSpan(5, 16))
                .Trim();
            var type = headerType is 1 or 3
                ? CbmFileType.Program
                : headerType == 2
                    ? CbmFileType.Sequential
                    : CbmFileType.User;

            files.Add(new TapFileInfo(name, type, start, end, payload));
            index = next > index + 1 ? next : index + 1;
        }

        return files;
    }

    private static bool IsCountdown(
        List<byte> bytes,
        int index,
        byte start)
    {
        for (var i = 0; i < BlockCountdownSize; i++)
        {
            if (bytes[index + i] != start - i)
                return false;
        }

        return true;
    }

    private static ushort ReadUInt16(byte[] bytes, int offset)
    {
        return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
    }

    private static PulseKind Classify(int cycles)
    {
        // These correspond to the TAP thresholds for the three CBM pulse
        // classes: short $24-$36, medium $37-$49 and long $4A+ (x8 cycles).
        var units = cycles / 8;

        if (units < 0x37)
            return PulseKind.Short;

        if (units < 0x4A)
            return PulseKind.Medium;

        return PulseKind.Long;
    }

    private sealed record TapeBlock(byte[] Payload, bool Primary);

    private enum PulseKind
    {
        Short,
        Medium,
        Long
    }
}
