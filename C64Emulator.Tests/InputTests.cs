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

    [Theory]
    [InlineData('[', 5, 0x20, C64Shift.Pressed)]
    [InlineData(']', 6, 0x04, C64Shift.Pressed)]
    [InlineData('@', 5, 0x40, C64Shift.Released)]
    [InlineData(':', 5, 0x20, C64Shift.Released)]
    [InlineData(';', 6, 0x04, C64Shift.Released)]
    [InlineData('\'', 3, 0x01, C64Shift.Pressed)]
    [InlineData('+', 5, 0x01, C64Shift.Released)]
    [InlineData('^', 6, 0x40, C64Shift.Released)]
    [InlineData('\\', 6, 0x01, C64Shift.Released)]
    public void TypedCharacter_SelectsC64KeyThatPrintsIt(
        char character,
        int expectedRow,
        byte expectedMask,
        C64Shift expectedShift)
    {
        KeyboardEventArgs args = new KeyboardEventArgs(Key.None, ModifierKeys.None, true);
        args.KeyboardCharacter = character.ToString();

        C64Key mapped = KeyboardMatrix.GetC64Key(args);

        Assert.Equal(expectedRow, mapped.Row);
        Assert.Equal(expectedMask, mapped.ColumnMask);
        Assert.Equal(expectedShift, mapped.Shift);
    }

    [Fact]
    public void PositionalMapping_UsesTheKeyInTheSamePlace()
    {
        KeyboardEventArgs args = new KeyboardEventArgs(Key.LeftBracket, ModifierKeys.None, true);
        args.KeyboardCharacter = "[";

        C64Key mapped = KeyboardMatrix.GetC64Key(args, characterBased: false);

        Assert.Equal(5, mapped.Row);
        Assert.Equal(0x40, mapped.ColumnMask);
    }

    [Fact]
    public void TypedAtSign_ReleasesShiftSoTheC64PrintsAt()
    {
        Cia1 cia = system.Cia1!;
        cia.Poke(0x02, 0xFF); // Port A drives the rows.

        cia.KeyEvent(
            new KeyboardEventArgs(Key.LeftShift, ModifierKeys.LeftShift, true),
            false);

        KeyboardEventArgs at = new KeyboardEventArgs(Key.Two, ModifierKeys.LeftShift, true);
        at.KeyboardCharacter = "@";
        cia.KeyEvent(at, false);

        cia.Poke(0x00, 0xDF); // Select row 5, where '@' lives.
        Assert.Equal(0xBF, cia.Peek(0x01));

        cia.Poke(0x00, 0xFD); // Select row 1, where left shift lives.
        Assert.Equal(0xFF, cia.Peek(0x01));
    }

    [Fact]
    public void TypedQuote_PressesShiftSoTheC64PrintsQuote()
    {
        Cia1 cia = system.Cia1!;
        cia.Poke(0x02, 0xFF);

        KeyboardEventArgs quote = new KeyboardEventArgs(Key.Two, ModifierKeys.None, true);
        quote.KeyboardCharacter = "\"";
        cia.KeyEvent(quote, false);

        cia.Poke(0x00, 0xFD); // Select row 1.
        Assert.Equal(0x7F, cia.Peek(0x01));

        cia.Poke(0x00, 0x7F); // Select row 7, where the 2" key lives.
        Assert.Equal(0xF7, cia.Peek(0x01));
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
