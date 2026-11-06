namespace C64Emulator.Core;

/// <summary>
/// A device that decodes the cartridge I/O windows at $DE00-$DFFF. Standard
/// ROM cartridges leave <see cref="DecodesIo"/> false; mappers that do decode
/// them (freezer buttons, flash cartridges, RAM expansions) implement this to
/// be wired into <see cref="IoController.Expansion"/>.
/// </summary>
internal interface IExpansionPort
{
    /// <summary>True when the device drives at least one of the I/O windows.</summary>
    bool DecodesIo { get; }

    byte Read(ushort address);

    void Write(ushort address, byte value);
}
