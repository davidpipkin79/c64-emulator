using System;
using Xunit;

namespace C64Emulator.Tests
{
    public sealed class MemoryMapTests : IDisposable
    {
        private readonly TestSystem system;

        public MemoryMapTests()
        {
            system = new TestSystem(withPeripherals: true);
        }

        public void Dispose()
        {
            system.Dispose();
        }

        [Fact]
        public void AllRamConfiguration_WritesUnderlyingRam()
        {
            SetConfiguration(0x00, 0x00);

            system.Memory.Poke(0xD020, 0xAA);

            Assert.Equal(0xAA, system.Memory.Read(0xD020));
            Assert.Equal(0xAA, system.Memory.Peek(0xD020));
            Assert.Equal(0x00, system.Vic!.GetRegister(0x20) & 0x0F);
        }

        [Fact]
        public void IoConfiguration_WritesTheDeviceNotRam()
        {
            SetConfiguration(0x07, 0x07);

            system.Memory.Poke(0xD020, 0x05);

            Assert.Equal(0x05, system.Vic!.GetRegister(0x20) & 0x0F);
            Assert.Equal(0x00, system.Memory.Read(0xD020));
        }

        [Fact]
        public void BasicRom_IsVisibleWhenHiramIsClear()
        {
            byte[] basic = new byte[AddressMap.BasicRomSize];
            basic[0] = 0x42;
            RomSet roms = RomSet.Create(
                new byte[AddressMap.KernalRomSize],
                basic,
                new byte[AddressMap.CharacterRomSize]);

            using TestSystem configured = new TestSystem(withPeripherals: true, roms);
            configured.Memory.Poke(0x0000, 0x07);
            configured.Memory.Poke(0x0001, 0x01); // HIRAM=0, LORAM=1, CHAREN=0

            Assert.Equal(0x42, configured.Memory.Peek(0xA000));
            Assert.NotEqual(0x42, configured.Memory.Peek(0xE000));
        }

        [Fact]
        public void CharacterRomConfiguration_WritesRamNotIo()
        {
            SetConfiguration(0x07, 0x03);

            system.Memory.Poke(0xD020, 0x0A);

            Assert.Equal(0x0A, system.Memory.Read(0xD020));
            Assert.Equal(0x00, system.Vic!.GetRegister(0x20) & 0x0F);
        }

        [Fact]
        public void WrappingWordReadCrossesPageButStaysInSixteenBits()
        {
            SetConfiguration(0x00, 0x00);
            system.Memory.Poke(0xFFFF, 0x34);
            system.Memory.Poke(0x0000, 0x12);

            Assert.Equal(0x1234, system.Memory.PeekWord(0xFFFF));
        }

        private void SetConfiguration(byte dataDirection, byte data)
        {
            system.Memory.Poke(0x0000, dataDirection);
            system.Memory.Poke(0x0001, data);
        }
    }
}
