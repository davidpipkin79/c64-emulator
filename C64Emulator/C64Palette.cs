namespace C64Emulator
{
    /// <summary>
    /// The fixed 16-colour C64 palette. The values are a commonly used PAL
    /// approximation; they are deliberately kept separate from colour RAM so
    /// changing the palette does not alter emulated state.
    /// </summary>
    internal static class C64Palette
    {
        private static readonly RgbColor[] Colors =
        {
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
        };

        public static RgbColor Get(int index)
        {
            return Colors[index & 0x0F];
        }

        public static int ToIndex(RgbColor color)
        {
            for (int i = 0; i < Colors.Length; i++)
            {
                if (Colors[i] == color)
                    return i;
            }

            return 0;
        }
    }
}
