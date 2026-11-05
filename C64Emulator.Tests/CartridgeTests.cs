using System;
using System.IO;
using System.Text;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class CartridgeTests
{
    [Fact]
    public void ParsesAnEightKilobyteCartridge()
    {
        var image = CreateCrt(
            ("TEST CART", 0x8000, new byte[] { 0xC3, 0xC2, 0xCD, 0x38, 0x30 }));

        Cartridge cartridge = Cartridge.FromBytes(image);

        Assert.Equal("TEST CART", cartridge.Name);
        Assert.False(cartridge.ExRom);
        Assert.True(cartridge.Game);
        Assert.True(cartridge.TryRead(0x8000, out var first));
        Assert.Equal(0xC3, first);
        Assert.False(cartridge.TryRead(0xA000, out _));
    }

    [Fact]
    public void ParsesAFullSixteenKilobyteCartridge()
    {
        var low = new byte[0x2000];
        var high = new byte[0x2000];
        low[0] = 0x11;
        high[0] = 0x22;

        var image = CreateCrt(("BIG CART", 0x8000, low), ("BIG CART", 0xA000, high));
        Cartridge cartridge = Cartridge.FromBytes(image);

        Assert.True(cartridge.HasHighRom);
        Assert.True(cartridge.TryRead(0x8000, out var lowValue));
        Assert.True(cartridge.TryRead(0xA000, out var highValue));
        Assert.Equal(0x11, lowValue);
        Assert.Equal(0x22, highValue);
    }

    [Fact]
    public void AContiguousSixteenKilobyteChipIsSplitAcrossRoms()
    {
        var rom = new byte[0x4000];
        rom[0] = 0xAA;
        rom[0x2000] = 0xBB;

        Cartridge cartridge = Cartridge.FromBytes(
            CreateCrt(("CONTIGUOUS", 0x8000, rom)));

        Assert.True(cartridge.TryRead(0x8000, out var lowValue));
        Assert.True(cartridge.TryRead(0xA000, out var highValue));
        Assert.Equal(0xAA, lowValue);
        Assert.Equal(0xBB, highValue);
    }

    [Fact]
    public void CartridgeOverlayWinsOverRamUntilEjected()
    {
        Memory memory = new Memory();
        Cartridge cartridge = Cartridge.FromBytes(
            CreateCrt(("OVERLAY", 0x8000, new byte[] { 0x42 })));

        memory.AttachCartridge(cartridge);
        Assert.Equal(0x42, memory.Peek(0x8000));

        memory.Poke(0x8000, 0x99);
        Assert.Equal(0x42, memory.Peek(0x8000));

        memory.AttachCartridge(null);
        Assert.Equal(0x99, memory.Peek(0x8000));
    }

    [Fact]
    public void SixteenKilobyteCartridgeOverridesBasicRom()
    {
        var basic = new byte[0x2000];
        Array.Fill(basic, (byte)0x55);

        using TestSystem system = new TestSystem(
            withPeripherals: true,
            RomSet.Create(new byte[0x2000], basic, new byte[0x1000]));

        var high = new byte[0x2000];
        high[0] = 0x22;

        Cartridge cartridge = Cartridge.FromBytes(
            CreateCrt(
                ("BASIC OVERRIDE", 0x8000, new byte[0x2000]),
                ("BASIC OVERRIDE", 0xA000, high)));

        system.Memory.AttachCartridge(cartridge);
        Assert.Equal(0x22, system.Memory.Peek(0xA000));

        system.Memory.AttachCartridge(null);
        Assert.Equal(0x55, system.Memory.Peek(0xA000));
    }

    [Fact]
    public void StandardCartridgeAutostartsFromKernal()
    {
        var romDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "C64Emulator", "Roms"));

        if (!File.Exists(Path.Combine(romDirectory, "kernal.rom")))
            return;

        var rom = new byte[0x2000];
        rom[0] = 0x09;
        rom[1] = 0x80;
        rom[2] = 0x09;
        rom[3] = 0x80;

        // The KERNAL compares $8004-$8008 with the high-bit PETSCII
        // signature stored at $FD10.
        rom[4] = 0xC3; // C
        rom[5] = 0xC2; // B
        rom[6] = 0xCD; // M
        rom[7] = 0x38; // 8
        rom[8] = 0x30; // 0

        // LDA #2 : STA $D020 : JMP $8009
        byte[] code = [0xA9, 0x02, 0x8D, 0x20, 0xD0, 0x4C, 0x09, 0x80];
        code.CopyTo(rom, 9);

        var path = Path.Combine(
            Path.GetTempPath(),
            "c64-cart-" + Guid.NewGuid().ToString("N") + ".crt");

        try
        {
            File.WriteAllBytes(path, CreateCrt(("AUTOSTART", 0x8000, rom)));

            using C64Machine machine = new C64Machine(RomSet.Load(romDirectory));
            machine.MountCartridge(path);
            machine.RunCycles(2_000_000);

            // Color registers return $F0 in their unused high nibble.
            Assert.Equal(2, machine.Vic.GetRegister(0x20) & 0x0F);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void RejectsUnsupportedHardwareAndBadSignatures()
    {
        var unsupported = CreateCrt(
            [("UNSUPPORTED", 0x8000, new byte[] { 1 })],
            hardwareType: 1);

        Assert.Throws<InvalidDataException>(() => Cartridge.FromBytes(unsupported));
        Assert.Throws<InvalidDataException>(
            () => Cartridge.FromBytes(Encoding.ASCII.GetBytes("not a cartridge")));
    }

    private static byte[] CreateCrt(
        params (string Name, int LoadAddress, byte[] Data)[] chips)
    {
        return CreateCrt(chips, hardwareType: 0);
    }

    private static byte[] CreateCrt(
        (string Name, int LoadAddress, byte[] Data)[] chips,
        int hardwareType)
    {
        var total = 0x40;

        foreach (var chip in chips)
            total += 16 + chip.Data.Length;

        var image = new byte[total];
        WriteAscii(image, 0, 16, "C64 CARTRIDGE   ");
        WriteInt32(image, 0x10, 0x40);
        WriteUInt16(image, 0x14, 1);
        WriteUInt16(image, 0x16, hardwareType);
        image[0x18] = 0;
        image[0x19] = (byte)(chips.Length == 1 && chips[0].Data.Length <= 0x2000 ? 1 : 0);
        WriteAscii(image, 0x20, 32, chips.Length > 0 ? chips[0].Name : "TEST");

        var offset = 0x40;

        foreach (var chip in chips)
        {
            WriteAscii(image, offset, 4, "CHIP");
            WriteInt32(image, offset + 4, 16 + chip.Data.Length);
            WriteUInt16(image, offset + 8, 0);
            WriteUInt16(image, offset + 10, 0);
            WriteUInt16(image, offset + 12, chip.LoadAddress);
            WriteUInt16(image, offset + 14, chip.Data.Length);
            chip.Data.CopyTo(image, offset + 16);
            offset += 16 + chip.Data.Length;
        }

        return image;
    }

    private static void WriteAscii(byte[] image, int offset, int length, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        Array.Copy(bytes, 0, image, offset, Math.Min(bytes.Length, length));
    }

    private static void WriteUInt16(byte[] image, int offset, int value)
    {
        image[offset] = (byte)(value & 0xFF);
        image[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteInt32(byte[] image, int offset, int value)
    {
        image[offset] = (byte)(value & 0xFF);
        image[offset + 1] = (byte)((value >> 8) & 0xFF);
        image[offset + 2] = (byte)((value >> 16) & 0xFF);
        image[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
