using System;
using C64Emulator.Devices;
using C64Emulator.Input;
using Xunit;

namespace C64Emulator.Tests;

public sealed class InputTests : IDisposable
{
    private readonly TestSystem system;

    public InputTests()
    {
        system = new TestSystem(withPeripherals: true);
    }

    public void Dispose()
    {
        system.Dispose();
    }

    [Theory]
    [InlineData(Key.Six, 2, 0x08)]
    [InlineData(Key.Seven, 3, 0x01)]
    [InlineData(Key.Eight, 3, 0x08)]
    [InlineData(Key.Nine, 4, 0x01)]
    [InlineData(Key.Zero, 4, 0x08)]
    [InlineData(Key.Two, 7, 0x08)]
    [InlineData(Key.Minus, 5, 0x08)]
    [InlineData(Key.LeftBracket, 5, 0x40)]
    [InlineData(Key.RightBracket, 6, 0x02)]
    public void NumberAndPunctuationKeys_UseC64MatrixPositions(
        Key key,
        int expectedRow,
        byte expectedMask)
    {
        C64Key mapped = KeyboardMatrix.GetC64Key(
            new KeyboardEventArgs(key, ModifierKeys.None, true));

        Assert.Equal(expectedRow, mapped.Row);
        Assert.Equal(expectedMask, mapped.ColumnMask);
    }

    [Fact]
    public void ShiftEquals_ProducesPlusKeyWithoutShiftState()
    {
        C64Key mapped = KeyboardMatrix.GetC64Key(
            new KeyboardEventArgs(Key.Equals, ModifierKeys.LeftShift, true));

        Assert.Equal(5, mapped.Row);
        Assert.Equal(0x01, mapped.ColumnMask);
    }

    [Fact]
    public void Semicolon_UsesSeparateColonKeyWhenShifted()
    {
        C64Key normal = KeyboardMatrix.GetC64Key(
            new KeyboardEventArgs(Key.Semicolon, ModifierKeys.None, true));
        C64Key shifted = KeyboardMatrix.GetC64Key(
            new KeyboardEventArgs(Key.Semicolon, ModifierKeys.LeftShift, true));

        Assert.Equal(6, normal.Row);
        Assert.Equal(0x04, normal.ColumnMask);
        Assert.Equal(5, shifted.Row);
        Assert.Equal(0x20, shifted.ColumnMask);
    }

    [Fact]
    public void KeyboardScan_SeesPressedAndReleasedKey()
    {
        Cia1 cia = system.Cia1!;
        cia.Poke(0x02, 0xFF); // Port A drives the rows.
        cia.Poke(0x00, 0xFB); // Select row 2.

        cia.KeyEvent(
            new KeyboardEventArgs(Key.Six, ModifierKeys.None, true),
            false);

        Assert.Equal(0xF7, cia.Peek(0x01));

        cia.KeyEvent(
            new KeyboardEventArgs(Key.Six, ModifierKeys.None, false),
            false);

        Assert.Equal(0xFF, cia.Peek(0x01));
    }

    [Fact]
    public void JoystickTwoFire_PullsCia1PortALow()
    {
        Cia1 cia = system.Cia1!;
        cia.Poke(0x02, 0x00);

        cia.JoyEvent(2, JoystickFunction.Fire, true);

        Assert.Equal(0xEF, cia.Peek(0x00));
    }
}
