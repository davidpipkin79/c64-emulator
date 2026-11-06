namespace C64Emulator.Core;

/// <summary>
/// The 6510 data bus is not driven by every address the CPU selects. A read
/// from a hole in the memory map floats, and the value that comes back is the
/// last one that appeared on the bus. Programs use that behavior to detect
/// what is (not) plugged into the expansion port, so the bus value has to be
/// tracked rather than replaced with a fixed $FF.
/// </summary>
internal sealed class BusLatch
{
    public byte Value { get; set; } = 0xFF;
}
