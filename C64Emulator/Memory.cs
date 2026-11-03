using System;

namespace C64Emulator
{
    /// <summary>
    /// The 64 KB address space and the PLA memory configuration. RAM is always
    /// present underneath the ROM and I/O overlays, which matters for writes
    /// while a ROM is banked in.
    /// </summary>
    internal sealed class Memory : IMemoryBus, IVideoBus
    {
        private readonly byte[] ram = new byte[AddressMap.RamSize];

        private RomSet? roms;
        private IIoPort? io;
        private Cartridge? cartridge;

        private bool loram;
        private bool hiram;
        private bool charen;

        public Memory()
        {
            ResetProcessorPort();
        }

        public int Size => ram.Length;

        private bool IoVisible => (hiram || loram) && charen;

        private bool CharacterRomVisible => (hiram || loram) && !charen;

        public bool IsKernalVisible => hiram;

        public Cartridge? Cartridge => cartridge;

        public void AttachCartridge(Cartridge? cart)
        {
            cartridge = cart;
        }

        public void Configure(RomSet romSet, IIoPort ioPort)
        {
            roms = romSet ?? throw new ArgumentNullException(nameof(romSet));
            io = ioPort ?? throw new ArgumentNullException(nameof(ioPort));
            UpdateMemoryConfiguration();
        }

        /// <summary>
        /// Power-on state of the 6510 processor port. The KERNAL expects the
        /// I/O area and both ROMs to be visible immediately after reset.
        /// </summary>
        public void ResetProcessorPort()
        {
            ram[AddressMap.ProcessorPortDirection] = 0xEF;
            ram[AddressMap.ProcessorPort] = 0x07;
            UpdateMemoryConfiguration();
        }

        public void Poke(ushort address, byte value)
        {
            if (IoVisible && address is >= AddressMap.IoStart and < AddressMap.IoEnd)
            {
                io?.Poke((ushort)(address - AddressMap.IoStart), value);
            }
            else
            {
                ram[address] = value;
            }

            if (address <= AddressMap.ProcessorPort)
                UpdateMemoryConfiguration();
        }

        public byte Peek(ushort address)
        {
            // Standard cartridges drive ROML (and ROMH for 16K images) onto
            // the bus regardless of the processor port. Writes still land in
            // the underlying RAM so a cartridge can be removed cleanly.
            if (cartridge is not null && cartridge.TryRead(address, out byte cartridgeValue))
                return cartridgeValue;

            if (roms is not null)
            {
                if (hiram && address >= AddressMap.KernalRomStart)
                    return roms.Kernal[address - AddressMap.KernalRomStart];

                // LORAM alone controls the BASIC overlay; HIRAM controls the
                // KERNAL overlay. Keeping these independent fixes the
                // HIRAM=0, LORAM=1 PLA configuration.
                if (
                    loram &&
                    address >= AddressMap.BasicRomStart &&
                    address < AddressMap.BasicRomStart + AddressMap.BasicRomSize)
                {
                    return roms.Basic[address - AddressMap.BasicRomStart];
                }

                if (
                    CharacterRomVisible &&
                    address >= AddressMap.CharacterRomStart &&
                    address < AddressMap.CharacterRomStart + AddressMap.CharacterRomSize)
                {
                    return roms.Character[address - AddressMap.CharacterRomStart];
                }
            }

            if (IoVisible && address is >= AddressMap.IoStart and < AddressMap.IoEnd)
                return io?.Peek((ushort)(address - AddressMap.IoStart)) ?? 0xFF;

            return ram[address];
        }

        public ushort PeekWord(ushort address)
        {
            return (ushort)(Peek(address) | (Peek((ushort)(address + 1)) << 8));
        }

        /// <summary>
        /// The VIC sees RAM and the character ROM overlay at $1000-$1FFF and
        /// $9000-$9FFF. It does not see the KERNAL/BASIC ROMs or I/O.
        /// </summary>
        public byte Read(ushort address)
        {
            if (roms is not null)
            {
                if (address is >= 0x1000 and < 0x2000)
                    return roms.Character[address - 0x1000];

                if (address is >= 0x9000 and < 0xA000)
                    return roms.Character[address - 0x9000];
            }

            return ram[address];
        }

        private void UpdateMemoryConfiguration()
        {
            byte direction = ram[AddressMap.ProcessorPortDirection];
            byte output = ram[AddressMap.ProcessorPort];

            loram = (direction & output & 0x01) != 0;
            hiram = (direction & output & 0x02) != 0;
            charen = (direction & output & 0x04) != 0;
        }
    }
}
