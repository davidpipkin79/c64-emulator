using System;
using C64Emulator.Input;

namespace C64Emulator.Devices;

public struct C64Key
{
    public int Row;
    public byte ColumnMask;
}

static class KeyboardMatrix
{
    public static bool ShiftPressed(KeyboardEventArgs k)
    {
        return
            ((k.Mod & ModifierKeys.LeftShift) == ModifierKeys.LeftShift) ||
            ((k.Mod & ModifierKeys.RightShift) == ModifierKeys.RightShift);
    }

    public static C64Key GetC64Key(KeyboardEventArgs k)
    {
        C64Key key = new C64Key();

        key.Row = 0;
        key.ColumnMask = 0;

        switch (k.Key)
        {
            case Key.Insert:
                key.Row = 0;
                key.ColumnMask = 0x01;
                break;
            
            case Key.Delete:
            case Key.Backspace:
                key.Row = 0;
                key.ColumnMask = 0x01;
                break;

            case Key.Return:
                key.Row = 0;
                key.ColumnMask = 0x02;
                break;

            case Key.LeftArrow:
                key.Row = 0;
                key.ColumnMask = 0x04;
                break;

            case Key.RightArrow:
                key.Row = 0;
                key.ColumnMask = 0x04;
                break;

            case Key.F7:
                key.Row = 0;
                key.ColumnMask = 0x08;
                break;

            case Key.F8:
                key.Row = 0;
                key.ColumnMask = 0x08;
                break;

            case Key.F1:
                key.Row = 0;
                key.ColumnMask = 0x10;
                break;

            case Key.F2:
                key.Row = 0;
                key.ColumnMask = 0x10;
                break;

            case Key.F3:
                key.Row = 0;
                key.ColumnMask = 0x20;
                break;

            case Key.F4:
                key.Row = 0;
                key.ColumnMask = 0x20;
                break;

            case Key.F5:
                key.Row = 0;
                key.ColumnMask = 0x40;
                break;

            case Key.F6:
                key.Row = 0;
                key.ColumnMask = 0x40;
                break;

            case Key.UpArrow:
                key.Row = 0;
                key.ColumnMask = 0x80;
                break;

            case Key.DownArrow:
                key.Row = 0;
                key.ColumnMask = 0x80;
                break;

            case Key.Three:
                key.Row = 1;
                key.ColumnMask = 0x01;
                break;

            case Key.W:
                key.Row = 1;
                key.ColumnMask = 0x02;
                break;

            case Key.A:
                key.Row = 1;
                key.ColumnMask = 0x04;
                break;

            case Key.Four:
                key.Row = 1;
                key.ColumnMask = 0x08;
                break;

            case Key.Z:
                key.Row = 1;
                key.ColumnMask = 0x10;
                break;

            case Key.S:
                key.Row = 1;
                key.ColumnMask = 0x20;
                break;

            case Key.E:
                key.Row = 1;
                key.ColumnMask = 0x40;
                break;

            case Key.LeftShift:
                key.Row = 1;
                key.ColumnMask = 0x80;
                break;

            case Key.Five:
                key.Row = 2;
                key.ColumnMask = 0x01;
                break;

            case Key.R:
                key.Row = 2;
                key.ColumnMask = 0x02;
                break;

            case Key.D:
                key.Row = 2;
                key.ColumnMask = 0x04;
                break;

            case Key.Six:
                key.Row = 2;
                key.ColumnMask = 0x08;
                break;

            case Key.C:
                key.Row = 2;
                key.ColumnMask = 0x10;
                break;

            case Key.F:
                key.Row = 2;
                key.ColumnMask = 0x20;
                break;

            case Key.T:
                key.Row = 2;
                key.ColumnMask = 0x40;
                break;

            case Key.X:
                key.Row = 2;
                key.ColumnMask = 0x80;
                break;

            case Key.Seven:
                key.Row = 3;
                key.ColumnMask = 0x01;
                break;

            case Key.Y:
                key.Row = 3;
                key.ColumnMask = 0x02;
                break;

            case Key.G:
                key.Row = 3;
                key.ColumnMask = 0x04;
                break;

            case Key.Eight:
                key.Row = 3;
                key.ColumnMask = 0x08;
                break;

            case Key.B:
                key.Row = 3;
                key.ColumnMask = 0x10;
                break;

            case Key.H:
                key.Row = 3;
                key.ColumnMask = 0x20;
                break;

            case Key.U:
                key.Row = 3;
                key.ColumnMask = 0x40;
                break;

            case Key.V:
                key.Row = 3;
                key.ColumnMask = 0x80;
                break;

            case Key.Nine:
                key.Row = 4;
                key.ColumnMask = 0x01;
                break;

            case Key.I:
                key.Row = 4;
                key.ColumnMask = 0x02;
                break;

            case Key.J:
                key.Row = 4;
                key.ColumnMask = 0x04;
                break;

            case Key.Zero:
                key.Row = 4;
                key.ColumnMask = 0x08;
                break;

            case Key.M:
                key.Row = 4;
                key.ColumnMask = 0x10;
                break;

            case Key.K:
                key.Row = 4;
                key.ColumnMask = 0x20;
                break;

            case Key.O:
                key.Row = 4;
                key.ColumnMask = 0x40;
                break;

            case Key.N:
                key.Row = 4;
                key.ColumnMask = 0x80;
                break;

            case Key.Equals:
                // '=' is an unshifted key; with Shift it produces '+'.
                key.Row = (ShiftPressed(k) ? 5 : 6);
                key.ColumnMask = (byte)(ShiftPressed(k) ? 0x01 : 0x20);
                break;

            case Key.P:
                key.Row = 5;
                key.ColumnMask = 0x02;
                break;

            case Key.L:
                key.Row = 5;
                key.ColumnMask = 0x04;
                break;

            case Key.Minus:
                key.Row = 5;
                key.ColumnMask = 0x08;
                break;

            case Key.Period:
                key.Row = 5;
                key.ColumnMask = 0x10;
                break;

            case Key.Semicolon:
                key.Row = (ShiftPressed(k) ? 5 : 6);
                key.ColumnMask = (byte)(ShiftPressed(k) ? 0x20 : 0x04);
                break;

            case Key.Two:
                key.Row = 7;
                key.ColumnMask = 0x08;
                break;

            case Key.Comma:
                key.Row = 5;
                key.ColumnMask = 0x80;
                break;

            case Key.LeftBracket:
                // The C64 has '@' where a PC keyboard has '['.
                key.Row = 5;
                key.ColumnMask = 0x40;
                break;

            case Key.Home:
                key.Row = 6;
                key.ColumnMask = 0x08;
                break;

            case Key.End:
                key.Row = 6;
                key.ColumnMask = 0x08;
                break;

            case Key.RightShift:
            case Key.CapsLock:
                key.Row = 6;
                key.ColumnMask = 0x10;
                break;

            case Key.Backslash:
                key.Row = 6;
                key.ColumnMask = 0x01; // pound sign
                break;

            case Key.Slash:
                key.Row = 6;
                key.ColumnMask = 0x80;
                break;

            case Key.One:
                key.Row = 7;
                key.ColumnMask = 0x01;
                break;

            case Key.BackQuote:
                key.Row = (ShiftPressed(k) ? 0 : 7);
                key.ColumnMask = (byte)(ShiftPressed(k) ? 0x00 : 0x02);
                break;

            case Key.LeftControl:
            case Key.RightControl:
                key.Row = 7;
                key.ColumnMask = 0x04;
                break;

            case Key.Space:
                key.Row = 7;
                key.ColumnMask = 0x10;
                break;

            case Key.PageUp:
            case Key.PageDown:
                key.Row = 7;
                key.ColumnMask = 0x20;
                break;

            case Key.Q:
                key.Row = 7;
                key.ColumnMask = 0x40;
                break;

            case Key.Escape:
                key.Row = 7;
                key.ColumnMask = 0x80;
                break;

            case Key.RightBracket:
                // The C64 has '*' where a PC keyboard has ']'.
                key.Row = 6;
                key.ColumnMask = 0x02;
                break;

            case Key.Quote:
                key.Row = (ShiftPressed(k) ? 7 : 3);
                key.ColumnMask = (byte)(ShiftPressed(k) ? 0x08 : 0x01);
                break;
        }

        return key;
    }
}
