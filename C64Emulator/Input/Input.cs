using System;
using C64Emulator.Host;
using SDL3;

namespace C64Emulator.Input;

/// <summary>
/// Keyboard keys used by the emulator. The numeric values match SDL 1.2
/// keycodes so the key values sent over the network protocol stay
/// compatible with older versions of the emulator.
/// </summary>
public enum Key
{
    None = 0,

    Backspace = 8,
    Tab = 9,
    Clear = 12,
    Return = 13,
    Pause = 19,
    Escape = 27,
    Space = 32,

    Quote = 39,
    Comma = 44,
    Minus = 45,
    Period = 46,
    Slash = 47,

    Zero = 48,
    One = 49,
    Two = 50,
    Three = 51,
    Four = 52,
    Five = 53,
    Six = 54,
    Seven = 55,
    Eight = 56,
    Nine = 57,

    Semicolon = 59,
    Equals = 61,

    A = 97,
    B = 98,
    C = 99,
    D = 100,
    E = 101,
    F = 102,
    G = 103,
    H = 104,
    I = 105,
    J = 106,
    K = 107,
    L = 108,
    M = 109,
    N = 110,
    O = 111,
    P = 112,
    Q = 113,
    R = 114,
    S = 115,
    T = 116,
    U = 117,
    V = 118,
    W = 119,
    X = 120,
    Y = 121,
    Z = 122,

    LeftBracket = 91,
    Backslash = 92,
    RightBracket = 93,
    BackQuote = 96,

    Delete = 127,

    Keypad0 = 256,
    Keypad1 = 257,
    Keypad2 = 258,
    Keypad3 = 259,
    Keypad4 = 260,
    Keypad5 = 261,
    Keypad6 = 262,
    Keypad7 = 263,
    Keypad8 = 264,
    Keypad9 = 265,
    KeypadPeriod = 266,
    KeypadDivide = 267,
    KeypadMultiply = 268,
    KeypadMinus = 269,
    KeypadPlus = 270,
    KeypadEnter = 271,
    KeypadEquals = 272,

    UpArrow = 273,
    DownArrow = 274,
    RightArrow = 275,
    LeftArrow = 276,
    Insert = 277,
    Home = 278,
    End = 279,
    PageUp = 280,
    PageDown = 281,

    F1 = 282,
    F2 = 283,
    F3 = 284,
    F4 = 285,
    F5 = 286,
    F6 = 287,
    F7 = 288,
    F8 = 289,
    F9 = 290,
    F10 = 291,
    F11 = 292,
    F12 = 293,
    F13 = 294,
    F14 = 295,
    F15 = 296,

    NumLock = 300,
    CapsLock = 301,
    ScrollLock = 302,
    RightShift = 303,
    LeftShift = 304,
    RightControl = 305,
    LeftControl = 306,
    RightAlt = 307,
    LeftAlt = 308,
    RightMeta = 309,
    LeftMeta = 310,
    LeftWindows = 311,
    RightWindows = 312,
    ModeSwitch = 313,
    Help = 315,
    PrintScreen = 316,
    SysReq = 317,
    Break = 318,
    Menu = 319,
    Power = 320,
    Euro = 321,
    Undo = 322
}

[Flags]
public enum ModifierKeys
{
    None = 0,
    LeftShift = 1,
    RightShift = 2,
    Shift = LeftShift | RightShift,
    LeftControl = 64,
    RightControl = 128,
    Control = LeftControl | RightControl,
    LeftAlt = 256,
    RightAlt = 512,
    Alt = LeftAlt | RightAlt,
    LeftMeta = 1024,
    RightMeta = 2048,
    Meta = LeftMeta | RightMeta,
    NumLock = 4096,
    CapsLock = 8192,
    Mode = 16384,
    ScrollLock = 32768
}

public sealed class KeyboardEventArgs(Key key, ModifierKeys mod, bool down) : EventArgs
{
    public Key Key { get; } = key;

    public ModifierKeys Mod { get; } = mod;

    public bool Down { get; } = down;

    public string KeyboardCharacter { get; set; } = string.Empty;
}

internal static class KeyMapper
{
    public static KeyboardEventArgs? Create(SDL.KeyboardEvent e, bool down)
    {
        Key key = FromScancode(e.Scancode);

        if (key == Key.None)
            return null;

        KeyboardEventArgs args = new KeyboardEventArgs(key, FromMod(e.Mod), down);
        args.KeyboardCharacter = GetCharacter(e.Key, e.Mod);

        return args;
    }

    /// <summary>
    /// Maps a physical SDL scancode to the emulator key. Scancodes are used
    /// instead of keycodes so shift and keyboard layout cannot change which
    /// C64 matrix key is pressed.
    /// </summary>
    public static Key FromScancode(SDL.Scancode scancode)
    {
        var code = (int)scancode;

        if (code >= (int)SDL.Scancode.A && code <= (int)SDL.Scancode.Z)
            return (Key)((int)Key.A + code - (int)SDL.Scancode.A);

        if (code >= (int)SDL.Scancode.Alpha1 && code <= (int)SDL.Scancode.Alpha9)
            return (Key)((int)Key.One + code - (int)SDL.Scancode.Alpha1);

        switch (scancode)
        {
            case SDL.Scancode.Alpha0: return Key.Zero;
            case SDL.Scancode.Return: return Key.Return;
            case SDL.Scancode.Escape: return Key.Escape;
            case SDL.Scancode.Backspace: return Key.Backspace;
            case SDL.Scancode.Tab: return Key.Tab;
            case SDL.Scancode.Space: return Key.Space;

            case SDL.Scancode.Minus: return Key.Minus;
            case SDL.Scancode.Equals: return Key.Equals;
            case SDL.Scancode.Leftbracket: return Key.LeftBracket;
            case SDL.Scancode.Rightbracket: return Key.RightBracket;
            case SDL.Scancode.Backslash: return Key.Backslash;
            case SDL.Scancode.NonUsHash: return Key.Backslash;
            case SDL.Scancode.NonUsBackSlash: return Key.Backslash;
            case SDL.Scancode.Semicolon: return Key.Semicolon;
            case SDL.Scancode.Apostrophe: return Key.Quote;
            case SDL.Scancode.Grave: return Key.BackQuote;
            case SDL.Scancode.Comma: return Key.Comma;
            case SDL.Scancode.Period: return Key.Period;
            case SDL.Scancode.Slash: return Key.Slash;

            case SDL.Scancode.Capslock: return Key.CapsLock;
            case SDL.Scancode.NumLockClear: return Key.NumLock;
            case SDL.Scancode.Scrolllock: return Key.ScrollLock;

            case SDL.Scancode.F1: return Key.F1;
            case SDL.Scancode.F2: return Key.F2;
            case SDL.Scancode.F3: return Key.F3;
            case SDL.Scancode.F4: return Key.F4;
            case SDL.Scancode.F5: return Key.F5;
            case SDL.Scancode.F6: return Key.F6;
            case SDL.Scancode.F7: return Key.F7;
            case SDL.Scancode.F8: return Key.F8;
            case SDL.Scancode.F9: return Key.F9;
            case SDL.Scancode.F10: return Key.F10;
            case SDL.Scancode.F11: return Key.F11;
            case SDL.Scancode.F12: return Key.F12;
            case SDL.Scancode.F13: return Key.F13;
            case SDL.Scancode.F14: return Key.F14;
            case SDL.Scancode.F15: return Key.F15;

            case SDL.Scancode.Printscreen: return Key.PrintScreen;
            case SDL.Scancode.Pause: return Key.Pause;
            case SDL.Scancode.Insert: return Key.Insert;
            case SDL.Scancode.Home: return Key.Home;
            case SDL.Scancode.Pageup: return Key.PageUp;
            case SDL.Scancode.Delete: return Key.Delete;
            case SDL.Scancode.End: return Key.End;
            case SDL.Scancode.Pagedown: return Key.PageDown;
            case SDL.Scancode.Right: return Key.RightArrow;
            case SDL.Scancode.Left: return Key.LeftArrow;
            case SDL.Scancode.Down: return Key.DownArrow;
            case SDL.Scancode.Up: return Key.UpArrow;

            case SDL.Scancode.Kp1: return Key.Keypad1;
            case SDL.Scancode.Kp2: return Key.Keypad2;
            case SDL.Scancode.Kp3: return Key.Keypad3;
            case SDL.Scancode.Kp4: return Key.Keypad4;
            case SDL.Scancode.Kp5: return Key.Keypad5;
            case SDL.Scancode.Kp6: return Key.Keypad6;
            case SDL.Scancode.Kp7: return Key.Keypad7;
            case SDL.Scancode.Kp8: return Key.Keypad8;
            case SDL.Scancode.Kp9: return Key.Keypad9;
            case SDL.Scancode.Kp0: return Key.Keypad0;
            case SDL.Scancode.KpPeriod: return Key.KeypadPeriod;
            case SDL.Scancode.KpDivide: return Key.KeypadDivide;
            case SDL.Scancode.KpMultiply: return Key.KeypadMultiply;
            case SDL.Scancode.KpMinus: return Key.KeypadMinus;
            case SDL.Scancode.KpPlus: return Key.KeypadPlus;
            case SDL.Scancode.KpEnter: return Key.KeypadEnter;
            case SDL.Scancode.KpEquals: return Key.KeypadEquals;

            case SDL.Scancode.LShift: return Key.LeftShift;
            case SDL.Scancode.RShift: return Key.RightShift;
            case SDL.Scancode.LCtrl: return Key.LeftControl;
            case SDL.Scancode.RCtrl: return Key.RightControl;
            case SDL.Scancode.LAlt: return Key.LeftAlt;
            case SDL.Scancode.RAlt: return Key.RightAlt;
            case SDL.Scancode.LGUI: return Key.LeftWindows;
            case SDL.Scancode.RGUI: return Key.RightWindows;
            case SDL.Scancode.Application: return Key.Menu;
        }

        return Key.None;
    }

    private static ModifierKeys FromMod(SDL.Keymod mod)
    {
        ModifierKeys result = ModifierKeys.None;

        if ((mod & SDL.Keymod.LShift) != 0)
            result |= ModifierKeys.LeftShift;
        if ((mod & SDL.Keymod.RShift) != 0)
            result |= ModifierKeys.RightShift;
        if ((mod & SDL.Keymod.LCtrl) != 0)
            result |= ModifierKeys.LeftControl;
        if ((mod & SDL.Keymod.RCtrl) != 0)
            result |= ModifierKeys.RightControl;
        if ((mod & SDL.Keymod.LAlt) != 0)
            result |= ModifierKeys.LeftAlt;
        if ((mod & SDL.Keymod.RAlt) != 0)
            result |= ModifierKeys.RightAlt;
        if ((mod & SDL.Keymod.Num) != 0)
            result |= ModifierKeys.NumLock;
        if ((mod & SDL.Keymod.Caps) != 0)
            result |= ModifierKeys.CapsLock;
        if ((mod & SDL.Keymod.Scroll) != 0)
            result |= ModifierKeys.ScrollLock;

        return result;
    }

    private static string GetCharacter(SDL.Keycode keycode, SDL.Keymod mod)
    {
        var code = (int)keycode;
        var shift =
            (mod & SDL.Keymod.LShift) != 0 ||
            (mod & SDL.Keymod.RShift) != 0;

        if (code >= 'a' && code <= 'z' && shift)
            code -= 'a' - 'A';

        if (code >= 32 && code <= 126)
            return ((char)code).ToString();

        return string.Empty;
    }
}
