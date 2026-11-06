namespace C64Emulator.Video;

/// <summary>
/// One 16-colour C64 palette. Palette values are host-side presentation data;
/// colour RAM continues to store only the 4-bit colour indices.
/// </summary>
internal sealed class C64Palette
{
    // The emulator's existing, deliberately saturated PAL approximation.
    public static C64Palette Pal { get; } = new(
    [
        new RgbColor(0x00, 0x00, 0x00), // 0 black
        new RgbColor(0xFF, 0xFF, 0xFF), // 1 white
        new RgbColor(0xBE, 0x35, 0x35), // 2 red
        new RgbColor(0x83, 0xF0, 0xDC), // 3 cyan
        new RgbColor(0xCC, 0x59, 0xC6), // 4 purple
        new RgbColor(0x59, 0xCD, 0x36), // 5 green
        new RgbColor(0x41, 0x37, 0xCD), // 6 blue
        new RgbColor(0xF7, 0xEE, 0x59), // 7 yellow
        new RgbColor(0xD1, 0x7F, 0x30), // 8 orange
        new RgbColor(0x91, 0x5F, 0x33), // 9 brown
        new RgbColor(0xF9, 0x9B, 0x97), // 10 light red
        new RgbColor(0x5B, 0x5B, 0x5B), // 11 dark grey
        new RgbColor(0x8E, 0x8E, 0x8E), // 12 medium grey
        new RgbColor(0xAC, 0xEA, 0x88), // 13 light green
        new RgbColor(0x75, 0xA1, 0xEC), // 14 light blue
        new RgbColor(0xCA, 0xCA, 0xCA)  // 15 light grey
    ]);

    // VICE data/C64/pepto-ntsc-sony.vpl: Pepto's NTSC calculation using the
    // Sony CXA2025AS US decoder matrix. Real NTSC output also depended on the
    // VIC revision and the monitor, so this is a representative table rather
    // than a universal one.
    public static C64Palette Ntsc { get; } = new(
    [
        new RgbColor(0x00, 0x00, 0x00), // 0 black
        new RgbColor(0xFF, 0xFF, 0xFF), // 1 white
        new RgbColor(0x7C, 0x35, 0x2B), // 2 red
        new RgbColor(0x5A, 0xA6, 0xB1), // 3 cyan
        new RgbColor(0x69, 0x41, 0x85), // 4 purple
        new RgbColor(0x5D, 0x86, 0x43), // 5 green
        new RgbColor(0x21, 0x2E, 0x78), // 6 blue
        new RgbColor(0xCF, 0xBE, 0x6F), // 7 yellow
        new RgbColor(0x89, 0x4A, 0x26), // 8 orange
        new RgbColor(0x5B, 0x33, 0x00), // 9 brown
        new RgbColor(0xAF, 0x64, 0x59), // 10 light red
        new RgbColor(0x43, 0x43, 0x43), // 11 dark grey
        new RgbColor(0x6B, 0x6B, 0x6B), // 12 medium grey
        new RgbColor(0xA0, 0xCB, 0x84), // 13 light green
        new RgbColor(0x56, 0x65, 0xB3), // 14 light blue
        new RgbColor(0x95, 0x95, 0x95)  // 15 light grey
    ]);

    private readonly RgbColor[] colors;

    private C64Palette(RgbColor[] colors)
    {
        this.colors = colors;
    }

    public static C64Palette For(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc ? Ntsc : Pal;
    }

    public RgbColor Get(int index)
    {
        return colors[index & 0x0F];
    }

    public int ToIndex(RgbColor color)
    {
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i] == color)
                return i;
        }

        return 0;
    }
}
