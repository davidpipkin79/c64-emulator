using System;
using C64Emulator.Core;
using C64Emulator.Devices;
using Xunit;

namespace C64Emulator.Tests;

public sealed class OpenBusAndTodTests : IDisposable
{
    private readonly TestSystem system;

    public OpenBusAndTodTests()
    {
        system = new TestSystem(withPeripherals: true);
    }

    public void Dispose()
    {
        system.Dispose();
    }

    [Fact]
    public void UnmappedIoRead_ReturnsTheLastBusValue()
    {
        system.Memory.Poke(0x1000, 0x37);
        Assert.Equal(0x37, system.Memory.Peek(0x1000));

        // $DE00-$DFFF has no device with I/O visible, so the bus floats
        // and repeats the value of the previous access.
        Assert.Equal(0x37, system.Memory.Peek(0xDE00));
        Assert.Equal(0x37, system.Memory.Peek(0xDFFF));
    }

    [Fact]
    public void WritesDriveTheOpenBusValue()
    {
        system.Memory.Poke(0xDE00, 0x5A);

        Assert.Equal(0x5A, system.Memory.Peek(0xDE00));
    }

    [Fact]
    public void ExpansionPort_DecodesBothIoWindows()
    {
        FakeExpansionPort port = new();
        system.Io!.Expansion = port;

        system.Memory.Poke(0xDE00, 0x12);

        Assert.Equal(0xDE00, port.LastWriteAddress);
        Assert.Equal(0x12, port.LastWriteValue);

        port.ReadValue = 0x34;

        Assert.Equal(0x34, system.Memory.Peek(0xDE00));
        Assert.Equal(0xDE00, port.LastReadAddress);
        Assert.Equal(0x34, system.Memory.Peek(0xDFFF));
        Assert.Equal(0xDFFF, port.LastReadAddress);
    }

    [Fact]
    public void TodStartsAtMidnightOnEveryMachine()
    {
        Cia1 first = new(new InterruptLine());
        Cia1 second = new(new InterruptLine());

        Assert.Equal(0x00, first.Peek(0x08));
        Assert.Equal(0x00, first.Peek(0x09));
        Assert.Equal(0x00, first.Peek(0x0A));
        Assert.Equal(0x12, first.Peek(0x0B)); // 12 AM, BCD, no PM bit

        Assert.Equal(0x12, second.Peek(0x0B));
    }

    [Fact]
    public void TodSeed_IsAppliedImmediately()
    {
        Cia1 cia = new(new InterruptLine());

        // 12:34:56.7 PM, in tenths of a second since midnight.
        cia.SeedTod(((12 * 3600) + (34 * 60) + 56) * 10 + 7);

        Assert.Equal(0x07, cia.Peek(0x08));
        Assert.Equal(0x56, cia.Peek(0x09));
        Assert.Equal(0x34, cia.Peek(0x0A));
        Assert.Equal(0x92, cia.Peek(0x0B)); // PM | 12
    }

    private sealed class FakeExpansionPort : IExpansionPort
    {
        public bool DecodesIo => true;

        public byte ReadValue;

        public ushort LastReadAddress;

        public ushort LastWriteAddress;

        public byte LastWriteValue;

        public byte Read(ushort address)
        {
            LastReadAddress = address;
            return ReadValue;
        }

        public void Write(ushort address, byte value)
        {
            LastWriteAddress = address;
            LastWriteValue = value;
        }
    }
}
