using System;

namespace C64Emulator
{
    /// <summary>
    /// The C64's 1 KB colour RAM at $D800-$DBFF. Only the low nibble of each
    /// byte is meaningful; the upper bits read back as 1 on the real machine.
    /// </summary>
    internal sealed class ColorRam
    {
        private readonly byte[] values = new byte[AddressMap.ColorRamSize];

        public int Size => values.Length;

        public byte Read(ushort offset)
        {
            return (byte)(values[offset % AddressMap.ColorRamSize] | 0xF0);
        }

        public byte ReadNibble(ushort offset)
        {
            return (byte)(values[offset % AddressMap.ColorRamSize] & 0x0F);
        }

        public void Write(ushort offset, byte value)
        {
            values[offset % AddressMap.ColorRamSize] = (byte)(value & 0x0F);
        }

        public RgbColor GetColor(ushort offset)
        {
            return C64Palette.Get(values[offset % AddressMap.ColorRamSize]);
        }

        public void Clear()
        {
            Array.Clear(values);
        }
    }
}
