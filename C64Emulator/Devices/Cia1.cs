using System;
using C64Emulator.Core;
using C64Emulator.Host;
using C64Emulator.Input;

namespace C64Emulator.Devices;

public enum JoystickFunction : byte
{
    None = 0,
    Fire = 1,
    Up = 2,
    Down = 3,
    Left = 4,
    Right = 5,
    VerticalCenter = 6,
    HorizontalCenter = 7
}

public enum JoystickMap : byte
{
    None = 0,
    Joystick1 = 1,
    Joystick2 = 2,
    KeySet1 = 3,
    KeySet2 = 4
}

public struct KeySet
{
    public Key Up;
    public Key Right;
    public Key Down;
    public Key Left;
    public Key Fire;
}

internal sealed class Cia1 : Cia6526
{
    private const int CiaSize = 0x10;

    private bool joyPort1Up = false;
    private bool joyPort1Right = false;
    private bool joyPort1Down = false;
    private bool joyPort1Left = false;
    private bool joyPort1Fire = false;
    private bool joyPort2Up = false;
    private bool joyPort2Right = false;
    private bool joyPort2Down = false;
    private bool joyPort2Left = false;
    private bool joyPort2Fire = false;

    private readonly JoystickMap[] joyMap;
    private readonly KeySet[] keyset;
    private readonly byte[] keyRows;

    public Cia1(InterruptLine interrupts)
        : base(interrupts, InterruptSource.Cia1)
    {
        keyRows =
        [
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
        ];

        KeySet keyset1 = new KeySet();
        keyset1.Up = AppSettings.Current.KeySet1Up;
        keyset1.Right = AppSettings.Current.KeySet1Right;
        keyset1.Down = AppSettings.Current.KeySet1Down;
        keyset1.Left = AppSettings.Current.KeySet1Left;
        keyset1.Fire = AppSettings.Current.KeySet1Fire;

        KeySet keyset2 = new KeySet();
        keyset2.Up = AppSettings.Current.KeySet2Up;
        keyset2.Right = AppSettings.Current.KeySet2Right;
        keyset2.Down = AppSettings.Current.KeySet2Down;
        keyset2.Left = AppSettings.Current.KeySet2Left;
        keyset2.Fire = AppSettings.Current.KeySet2Fire;

        keyset = [keyset1, keyset2];

        joyMap =
        [
            (JoystickMap)AppSettings.Current.Joystick1Map,
            (JoystickMap)AppSettings.Current.Joystick2Map
        ];
    }

    public void KeyEvent(KeyboardEventArgs k, bool skipMapCheck)
    {
        processKey(k, skipMapCheck);
    }

    public void SwitchJoysticks()
    {
        JoystickMap jm1 = joyMap[0];
        joyMap[0] = joyMap[1];
        joyMap[1] = jm1;

        AppSettings.Current.Joystick1Map = (int)joyMap[0];
        AppSettings.Current.Joystick2Map = (int)joyMap[1];
    }

    public JoystickMap Joy1Map
    {
        get
        {
            return joyMap[0];
        }
        set
        {
            joyMap[0] = value;
            AppSettings.Current.Joystick1Map = (int)value;
        }
    }

    public JoystickMap Joy2Map
    {
        get
        {
            return joyMap[1];
        }
        set
        {
            joyMap[1] = value;
            AppSettings.Current.Joystick2Map = (int)value;
        }
    }

    public KeySet KeySet1
    {
        get
        {
            return keyset[0];
        }
    }

    public KeySet KeySet2
    {
        get
        {
            return keyset[1];
        }
    }

    public void SetKeySet(int ksnum, string function, Key key)
    {
        switch (function)
        {
            case "up":
                keyset[ksnum - 1].Up = key;
                if (ksnum == 1)
                    AppSettings.Current.KeySet1Up = key;
                else
                    AppSettings.Current.KeySet2Up = key;
                break;
            case "right":
                keyset[ksnum - 1].Right = key;
                if (ksnum == 1)
                    AppSettings.Current.KeySet1Right = key;
                else
                    AppSettings.Current.KeySet2Right = key;
                break;
            case "down": 
                keyset[ksnum - 1].Down = key;
                if (ksnum == 1)
                    AppSettings.Current.KeySet1Down = key;
                else
                    AppSettings.Current.KeySet2Down = key;
                break;
            case "left": 
                keyset[ksnum - 1].Left = key;
                if (ksnum == 1)
                    AppSettings.Current.KeySet1Left = key;
                else
                    AppSettings.Current.KeySet2Left = key;
                break;
            case "fire": 
                keyset[ksnum - 1].Fire = key;
                if (ksnum == 1)
                    AppSettings.Current.KeySet1Fire = key;
                else
                    AppSettings.Current.KeySet2Fire = key;
                break;
        }
    }

    public override void Reset()
    {
        base.Reset();
        Array.Fill(keyRows, (byte)0xFF);
        joyPort1Up = joyPort1Right = joyPort1Down = joyPort1Left = joyPort1Fire = false;
        joyPort2Up = joyPort2Right = joyPort2Down = joyPort2Left = joyPort2Fire = false;
    }

    public void JoyEvent(int joyNum, JoystickFunction j, bool pressed)
    {
        processJoy(joyNum, j, pressed);
    }

    private void processJoy(int joyNum, JoystickFunction function, bool pressed)
    {
        var first = joyNum == 1;

        switch (function)
        {
            case JoystickFunction.Fire:
                if (first) joyPort1Fire = pressed; else joyPort2Fire = pressed;
                break;

            case JoystickFunction.Up:
                if (first) joyPort1Up = pressed; else joyPort2Up = pressed;
                break;

            case JoystickFunction.Right:
                if (first) joyPort1Right = pressed; else joyPort2Right = pressed;
                break;

            case JoystickFunction.Down:
                if (first) joyPort1Down = pressed; else joyPort2Down = pressed;
                break;

            case JoystickFunction.Left:
                if (first) joyPort1Left = pressed; else joyPort2Left = pressed;
                break;

            case JoystickFunction.VerticalCenter:
                if (first)
                {
                    joyPort1Up = false;
                    joyPort1Down = false;
                }
                else
                {
                    joyPort2Up = false;
                    joyPort2Down = false;
                }
                break;

            case JoystickFunction.HorizontalCenter:
                if (first)
                {
                    joyPort1Left = false;
                    joyPort1Right = false;
                }
                else
                {
                    joyPort2Left = false;
                    joyPort2Right = false;
                }
                break;

            case JoystickFunction.None:
                if (first)
                {
                    joyPort1Up = false;
                    joyPort1Right = false;
                    joyPort1Down = false;
                    joyPort1Left = false;
                    joyPort1Fire = false;
                }
                else
                {
                    joyPort2Up = false;
                    joyPort2Right = false;
                    joyPort2Down = false;
                    joyPort2Left = false;
                    joyPort2Fire = false;
                }
                break;
        }
    }

    private JoystickFunction checkMap(int joyNum, Key k)
    {
        var ksnum = -1;

        if (joyMap[joyNum - 1] == JoystickMap.KeySet1)
            ksnum = 0;
        else if (joyMap[joyNum - 1] == JoystickMap.KeySet2)
            ksnum = 1;

        if (ksnum > -1)
        {
            if (keyset[ksnum].Up == k)
                return JoystickFunction.Up;
            else if (keyset[ksnum].Right == k)
                return JoystickFunction.Right;
            else if (keyset[ksnum].Down == k)
                return JoystickFunction.Down;
            else if (keyset[ksnum].Left == k)
                return JoystickFunction.Left;
            else if (keyset[ksnum].Fire == k)
                return JoystickFunction.Fire;
        }

        return JoystickFunction.None;
    }

    private void processKey(KeyboardEventArgs k, bool skipMapCheck)
    {
        if (!skipMapCheck)
        {
            JoystickFunction jf = checkMap(1, k.Key);

            if (jf != JoystickFunction.None)
            {
                processJoy(1, jf, k.Down);
                return;
            }

            jf = checkMap(2, k.Key);

            if (jf != JoystickFunction.None)
            {
                processJoy(2, jf, k.Down);
                return;
            }
        }

        // Remote clients only send the shift state attached to each key,
        // not separate shift-key events. Apply it here without disturbing
        // locally-pressed shift keys.
        if (skipMapCheck)
        {
            if (KeyboardMatrix.ShiftPressed(k))
            {
                keyRows[1] &= 0x7F;
                keyRows[6] &= 0xEF;
            }
            else
            {
                keyRows[1] |= 0x80;
                keyRows[6] |= 0x10;
            }
        }

        C64Key key = KeyboardMatrix.GetC64Key(k);

        if (key.ColumnMask == 0)
            return;

        if (k.Down)
            keyRows[key.Row] &= (byte)(~key.ColumnMask);
        else
            keyRows[key.Row] |= key.ColumnMask;
    }

    public int Size => CiaSize;

    public ushort Position => AddressMap.Cia1Start;

    public byte Peek(ushort address)
    {
        switch (address)
        {
            case 0x00:
                return readPortA();
            case 0x01:
                return readPortB();
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
                break;
            case 0x01:
                PortBOutput = value;
                break;
            case 0x02:
                PortADirection = value;
                break;
            case 0x03:
                PortBDirection = value;
                break;
            default:
                WriteRegister(address, value);
                break;
        }
    }

    private byte readPortA()
    {
        var input =
            (byte)(
            0xE0 |
            (joyPort2Up ? 0 : 0x01) |
            (joyPort2Down ? 0 : 0x02) |
            (joyPort2Left ? 0 : 0x04) |
            (joyPort2Right ? 0 : 0x08) |
            (joyPort2Fire ? 0 : 0x10)
            );

        return
            (byte)(
            (PortAOutput & PortADirection) |
            (input & ~PortADirection)
            );
    }

    private byte readPortB()
    {
        // Port A drives the keyboard rows low; port B reads the columns.
        // Pull-up inputs read high when no row is selected.
        var selectedRows = (byte)(PortAOutput | ~PortADirection);
        byte input = 0xFF;

        for (var i = 0; i < 8; i++)
            if ((selectedRows & (1 << i)) == 0)
                input &= keyRows[i];

        if (joyPort1Up) input = (byte)(input & 0xFE);
        if (joyPort1Down) input = (byte)(input & 0xFD);
        if (joyPort1Left) input = (byte)(input & 0xFB);
        if (joyPort1Right) input = (byte)(input & 0xF7);
        if (joyPort1Fire) input = (byte)(input & 0xEF);

        var value =
            (byte)(
            (PortBOutput & PortBDirection) |
            (input & ~PortBDirection)
            );

        return ApplyTimerOutputs(value);
    }

}
