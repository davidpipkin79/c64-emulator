namespace C64Emulator.Video;

/// <summary>
/// An RGB colour used by the emulation core. The host converts this to the
/// native SDL colour type, keeping System.Drawing out of the machine code.
/// </summary>
internal readonly record struct RgbColor(byte R, byte G, byte B)
{
    public int ToArgb()
    {
        return unchecked((int)0xFF000000 | (R << 16) | (G << 8) | B);
    }
}
