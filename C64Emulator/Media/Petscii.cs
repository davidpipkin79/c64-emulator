using System;

namespace C64Emulator.Media;

/// <summary>
/// Converts the subset of PETSCII used for disk and tape filenames.
/// </summary>
internal static class Petscii
{
    public static char ToAscii(byte value)
    {
        return value switch
        {
            0xA0 => ' ',
            >= 0xC1 and <= 0xDA => (char)(value - 0x80),
            >= 0x41 and <= 0x5A => (char)value,
            >= 0x61 and <= 0x7A => (char)value,
            >= 0x20 and <= 0x7E => (char)value,
            _ => '?'
        };
    }

    public static string ToAscii(ReadOnlySpan<byte> bytes)
    {
        var characters = new char[bytes.Length];

        for (var i = 0; i < bytes.Length; i++)
            characters[i] = ToAscii(bytes[i]);

        return new string(characters).TrimEnd(' ');
    }
}
