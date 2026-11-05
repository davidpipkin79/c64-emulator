namespace C64Emulator.Core;

/// <summary>
/// The CPU's view of the address bus.
/// </summary>
internal interface IMemoryBus
{
    byte Peek(ushort address);

    void Poke(ushort address, byte value);

    ushort PeekWord(ushort address);

    /// <summary>
    /// An internal CPU cycle. Buses that model timing use it to advance
    /// the machine; a plain memory has nothing to do.
    /// </summary>
    void Idle();
}

/// <summary>
/// The VIC's view of memory. It sees RAM and the character ROM overlay,
/// but not the I/O area or the KERNAL/BASIC ROMs.
/// </summary>
internal interface IVideoBus
{
    byte Read(ushort address);
}

/// <summary>
/// The I/O area behind the PLA.
/// </summary>
internal interface IIoPort
{
    byte Peek(ushort address);

    void Poke(ushort address, byte value);
}

/// <summary>
/// A device that can change the VIC's 16 KB bank, such as CIA2 port A.
/// </summary>
internal interface IVideoBank
{
    ushort BankAddress { get; set; }
}
