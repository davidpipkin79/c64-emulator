using System;
using System.IO;
using System.Text;
using C64Emulator.Core;

namespace C64Emulator.Devices;

/// <summary>
/// A standard 8K or 16K C64 cartridge image. Only type 0 hardware is
/// supported; banked and flash cartridges need their own banking logic.
/// </summary>
internal sealed class Cartridge
{
    public const int RomSize = 0x2000;

    private readonly byte[] romLow;
    private readonly byte[]? romHigh;

    private Cartridge(
        string name,
        int hardwareType,
        bool exRom,
        bool game,
        byte[] romLow,
        byte[]? romHigh)
    {
        Name = name;
        HardwareType = hardwareType;
        ExRom = exRom;
        Game = game;
        this.romLow = romLow;
        this.romHigh = romHigh;
    }

    public string Name { get; }

    public int HardwareType { get; }

    public bool ExRom { get; }

    public bool Game { get; }

    public bool HasHighRom => romHigh is not null;

    public static Cartridge Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return FromBytes(File.ReadAllBytes(path));
    }

    public static Cartridge FromBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length < 0x40)
            throw new InvalidDataException("The file is too small to be a CRT image.");

        var signature = Encoding.ASCII
            .GetString(bytes, 0, 16)
            .TrimEnd('\0', ' ');

        if (!signature.StartsWith("C64 CARTRIDGE", StringComparison.Ordinal))
            throw new InvalidDataException("The file does not have a CRT signature.");

        var headerLength = ReadUInt32(bytes, 0x10);

        if (headerLength < 0x40 || headerLength > bytes.Length)
            throw new InvalidDataException("The CRT header length is invalid.");

        int hardwareType = ReadUInt16(bytes, 0x16);
        var exRom = bytes[0x18] != 0;
        var game = bytes[0x19] != 0;
        var name = Encoding.ASCII
            .GetString(bytes, 0x20, 32)
            .TrimEnd('\0', ' ');
        byte[]? romLow = null;
        byte[]? romHigh = null;
        var offset = headerLength;

        while (offset + 16 <= bytes.Length)
        {
            if (Encoding.ASCII.GetString(bytes, offset, 4) != "CHIP")
                break;

            var packetLength = ReadInt32(bytes, offset + 4);

            if (packetLength < 16 || offset + packetLength > bytes.Length)
                throw new InvalidDataException("The CRT contains an invalid chip packet.");

            int chipType = ReadUInt16(bytes, offset + 8);
            int loadAddress = ReadUInt16(bytes, offset + 12);
            int imageSize = ReadUInt16(bytes, offset + 14);

            if (imageSize < 0 || 16 + imageSize > packetLength)
                throw new InvalidDataException("The CRT chip image is truncated.");

            if (chipType == 0)
            {
                var dataOffset = offset + 16;

                if (loadAddress == 0x8000)
                {
                    romLow = new byte[RomSize];
                    var lowLength = Math.Min(imageSize, RomSize);
                    Array.Copy(bytes, dataOffset, romLow, 0, lowLength);

                    if (imageSize > RomSize)
                    {
                        romHigh = new byte[RomSize];
                        Array.Copy(
                            bytes,
                            dataOffset + RomSize,
                            romHigh,
                            0,
                            Math.Min(imageSize - RomSize, RomSize));
                    }
                }
                else if (loadAddress == 0xA000)
                {
                    romHigh = new byte[RomSize];
                    Array.Copy(
                        bytes,
                        dataOffset,
                        romHigh,
                        0,
                        Math.Min(imageSize, RomSize));
                }
            }

            offset += packetLength;
        }

        if (romLow is null && romHigh is null)
            throw new InvalidDataException("The CRT contains no supported ROM chips.");

        if (hardwareType != 0)
            throw new InvalidDataException(
                "Only standard 8K/16K CRT cartridges are supported.");

        if (exRom)
            throw new InvalidDataException(
                "This CRT requires an unsupported cartridge memory configuration.");

        return new Cartridge(
            name,
            hardwareType,
            exRom,
            game,
            romLow ?? new byte[RomSize],
            romHigh);
    }

    public bool TryRead(ushort address, out byte value)
    {
        if (address is >= 0x8000 and < 0xA000)
        {
            value = romLow[address - 0x8000];
            return true;
        }

        if (romHigh is not null && address is >= 0xA000 and < 0xC000)
        {
            value = romHigh[address - 0xA000];
            return true;
        }

        value = 0;
        return false;
    }

    private static ushort ReadUInt16(byte[] bytes, int offset)
    {
        return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
    }

    private static int ReadUInt32(byte[] bytes, int offset)
    {
        return bytes[offset] |
            (bytes[offset + 1] << 8) |
            (bytes[offset + 2] << 16) |
            (bytes[offset + 3] << 24);
    }

    private static int ReadInt32(byte[] bytes, int offset)
    {
        return ReadUInt32(bytes, offset);
    }
}
