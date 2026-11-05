using System;
using C64Emulator.Core;
using C64Emulator.Video;

namespace C64Emulator.Devices;

internal sealed class Cia2(
    InterruptLine interrupts,
    IVideoBank videoBank,
    IecBus? iec = null) : Cia6526(interrupts, InterruptSource.Cia2, useNmi: true)
{
    private const int CiaSize = 0x10;

    private readonly IVideoBank videoBank = videoBank ?? throw new ArgumentNullException(nameof(videoBank));
    private readonly IecBus? iec = iec;

    public int Size => CiaSize;

    public ushort Position => AddressMap.Cia2Start;

    public override void Reset()
    {
        base.Reset();
        UpdateVideoBank();
        iec?.SetC64(PortAOutput, PortADirection);
    }

    public byte Peek(ushort address)
    {
        switch (address)
        {
            case 0x00:
            {
                var value = ReadPort(PortAOutput, PortADirection);

                if (iec is not null)
                {
                    value = (byte)(
                        (value & 0x3F) |
                        (iec.ClockHigh ? 0x40 : 0x00) |
                        (iec.DataHigh ? 0x80 : 0x00));
                }

                return value;
            }
            case 0x01:
                return ApplyTimerOutputs(ReadPort(PortBOutput, PortBDirection));
            case 0x02:
                return PortADirection;
            case 0x03:
                return PortBDirection;
            default:
                return ReadRegister(address);
        }
    }

    public void Poke(ushort address, byte value)
    {
        switch (address)
        {
            case 0x00:
                PortAOutput = value;
                UpdateVideoBank();
                iec?.SetC64(PortAOutput, PortADirection);
                break;

            case 0x01:
                PortBOutput = value;
                break;

            case 0x02:
                PortADirection = value;
                UpdateVideoBank();
                iec?.SetC64(PortAOutput, PortADirection);
                break;

            case 0x03:
                PortBDirection = value;
                break;

            default:
                WriteRegister(address, value);
                break;
        }
    }

    private void UpdateVideoBank()
    {
        // CIA2 port A bits 0-1 drive the VIC bank through an inverter. Input
        // pins are pulled high, so the effective pin value is output for
        // driven bits and 1 for input bits.
        var pins = (byte)((PortAOutput & PortADirection) | (~PortADirection & 0xFF));
        videoBank.BankAddress = pins;
    }

    private static byte ReadPort(byte output, byte direction)
    {
        // Unconnected/open input pins are pulled high on the CIA.
        const byte inputPins = 0xFF;

        return (byte)((output & direction) | (inputPins & ~direction));
    }
}
