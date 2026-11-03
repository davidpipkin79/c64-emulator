using System;

namespace C64Emulator
{
    /// <summary>
    /// The $D000-$DFFF I/O area. Each chip is mirrored across its 256-byte
    /// page, and the CIA registers repeat every 16 bytes. Addresses that do
    /// not decode to a chip read as open bus (0xFF here).
    /// </summary>
    internal sealed class IO : IIoPort
    {
        private const int VicPageSize = 0x400;
        private const int SidPageSize = 0x400;
        private const int CiaMirrorSize = 0x100;

        private readonly VIC vic;
        private readonly SID sid;
        private readonly ColorRam colorRam;
        private readonly CIA1 cia1;
        private readonly CIA2 cia2;

        public IO(VIC vic, SID sid, ColorRam colorRam, CIA1 cia1, CIA2 cia2)
        {
            this.vic = vic ?? throw new ArgumentNullException(nameof(vic));
            this.sid = sid ?? throw new ArgumentNullException(nameof(sid));
            this.colorRam = colorRam ?? throw new ArgumentNullException(nameof(colorRam));
            this.cia1 = cia1 ?? throw new ArgumentNullException(nameof(cia1));
            this.cia2 = cia2 ?? throw new ArgumentNullException(nameof(cia2));
        }

        public int Size => AddressMap.IoEnd - AddressMap.IoStart;

        public ushort Position => AddressMap.IoStart;

        // IIoPort addresses are offsets from $D000, not absolute bus addresses.
        public byte Peek(ushort address)
        {
            if (address >= Size)
                return 0xFF;

            if (address < VicPageSize)
            {
                int register = address & 0x3F;
                return register < VIC.RegisterCount ? vic.GetRegister(register) : (byte)0xFF;
            }

            if (address < SidPageSize + 0x400)
            {
                int register = (address - 0x400) & 0x1F;
                return register < SID.RegisterCount ? sid.GetRegister(register) : (byte)0xFF;
            }

            if (address < 0xC00)
                return colorRam.Read((ushort)(address - 0x800));

            if (address < 0xD00)
                return cia1.Peek((ushort)(address & 0x0F));

            if (address < 0xE00)
                return cia2.Peek((ushort)(address & 0x0F));

            return 0xFF;
        }

        public void Poke(ushort address, byte value)
        {
            if (address >= Size)
                return;

            if (address < VicPageSize)
            {
                int register = address & 0x3F;

                if (register < VIC.RegisterCount)
                    vic.SetRegister(register, value);

                return;
            }

            if (address < SidPageSize + 0x400)
            {
                int register = (address - 0x400) & 0x1F;

                if (register < SID.RegisterCount)
                    sid.SetRegister(register, value);

                return;
            }

            if (address < 0xC00)
            {
                colorRam.Write((ushort)(address - 0x800), value);
                return;
            }

            if (address < 0xD00)
            {
                cia1.Poke((ushort)(address & 0x0F), value);
                return;
            }

            if (address < 0xE00)
                cia2.Poke((ushort)(address & 0x0F), value);
        }
    }
}
