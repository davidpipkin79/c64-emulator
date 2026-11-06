using System;
using C64Emulator.Audio;
using C64Emulator.Devices;
using C64Emulator.Video;

namespace C64Emulator.Core;

/// <summary>
/// The $D000-$DFFF I/O area. Each chip is mirrored across its 256-byte
/// page, and the CIA registers repeat every 16 bytes. Addresses that do
/// not decode to a chip read the last value on the data bus, and the
/// $DE00-$DFFF cartridge windows go to an optional expansion device.
/// </summary>
internal sealed class IoController(
    Vic vic,
    Sid sid,
    ColorRam colorRam,
    Cia1 cia1,
    Cia2 cia2,
    BusLatch? dataBus = null) : IIoPort
{
    private const int VicPageSize = 0x400;
    private const int SidPageSize = 0x400;
    private const int CiaMirrorSize = 0x100;

    private readonly Vic vic = vic ?? throw new ArgumentNullException(nameof(vic));
    private readonly Sid sid = sid ?? throw new ArgumentNullException(nameof(sid));
    private readonly ColorRam colorRam = colorRam ?? throw new ArgumentNullException(nameof(colorRam));
    private readonly Cia1 cia1 = cia1 ?? throw new ArgumentNullException(nameof(cia1));
    private readonly Cia2 cia2 = cia2 ?? throw new ArgumentNullException(nameof(cia2));
    private readonly BusLatch busLatch = dataBus ?? new BusLatch();

    public int Size => AddressMap.IoEnd - AddressMap.IoStart;

    public ushort Position => AddressMap.IoStart;

    /// <summary>
    /// Optional device decoding $DE00-$DFFF. Null leaves the cartridge
    /// windows floating, as they are with no cartridge plugged in.
    /// </summary>
    public IExpansionPort? Expansion { get; set; }

    // IIoPort addresses are offsets from $D000, not absolute bus addresses.
    public byte Peek(ushort address)
    {
        var value = PeekCore(address);
        busLatch.Value = value;
        return value;
    }

    private byte PeekCore(ushort address)
    {
        if (address >= Size)
            return busLatch.Value;

        if (address < VicPageSize)
        {
            var register = address & 0x3F;
            return register < Vic.RegisterCount
                ? vic.GetRegister(register)
                : busLatch.Value;
        }

        if (address < SidPageSize + 0x400)
        {
            var register = (address - 0x400) & 0x1F;
            return register < Sid.RegisterCount
                ? sid.GetRegister(register)
                : busLatch.Value;
        }

        if (address < 0xC00)
            return colorRam.Read((ushort)(address - 0x800));

        if (address < 0xD00)
            return cia1.Peek((ushort)(address & 0x0F));

        if (address < 0xE00)
            return cia2.Peek((ushort)(address & 0x0F));

        if (Expansion is { DecodesIo: true } expansion)
            return expansion.Read((ushort)(AddressMap.IoStart + address));

        return busLatch.Value;
    }

    public void Poke(ushort address, byte value)
    {
        busLatch.Value = value;

        if (address >= Size)
            return;

        if (address < VicPageSize)
        {
            var register = address & 0x3F;

            if (register < Vic.RegisterCount)
                vic.SetRegister(register, value);

            return;
        }

        if (address < SidPageSize + 0x400)
        {
            var register = (address - 0x400) & 0x1F;

            if (register < Sid.RegisterCount)
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
        {
            cia2.Poke((ushort)(address & 0x0F), value);
            return;
        }

        if (Expansion is { DecodesIo: true } expansion)
            expansion.Write((ushort)(AddressMap.IoStart + address), value);
    }
}
