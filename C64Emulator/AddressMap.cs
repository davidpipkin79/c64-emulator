namespace C64Emulator
{
    /// <summary>
    /// Addresses and sizes that are fixed by the C64 hardware. Keeping these
    /// in one place avoids repeating magic numbers across the chip emulation.
    /// </summary>
    internal static class AddressMap
    {
        public const int RamSize = 0x10000;

        public const ushort BasicRomStart = 0xA000;
        public const int BasicRomSize = 0x2000;

        public const ushort KernalRomStart = 0xE000;
        public const int KernalRomSize = 0x2000;

        public const ushort CharacterRomStart = 0xD000;
        public const int CharacterRomSize = 0x1000;

        public const ushort VicStart = 0xD000;
        public const ushort SidStart = 0xD400;
        public const ushort ColorRamStart = 0xD800;
        public const ushort Cia1Start = 0xDC00;
        public const ushort Cia2Start = 0xDD00;

        public const ushort IoStart = 0xD000;
        public const ushort IoEnd = 0xE000;

        public const int ColorRamSize = 0x400;

        public const ushort ResetVector = 0xFFFC;
        public const ushort IrqVector = 0xFFFE;
        public const ushort NmiVector = 0xFFFA;

        public const ushort ProcessorPort = 0x0001;
        public const ushort ProcessorPortDirection = 0x0000;
    }
}
