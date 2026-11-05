using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using C64Emulator.Audio;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Input;
using C64Emulator.Networking;
using C64Emulator.Video;
using SDL3;

namespace C64Emulator.Host;

/// <summary>
/// SDL host: window, event loop, joysticks and networking. Emulation runs on
/// its own thread; all SDL calls stay on this thread.
/// </summary>
internal sealed class EmuWin
{
    private Renderer? renderer;
    private AudioOutput? audio;
    private Network? network;
    private Menu? menu;
    private C64Machine? machine;

    private IntPtr joystick1;
    private IntPtr joystick2;
    private uint joystick1Id;
    private uint joystick2Id;

    private bool running;
    private bool sdlInitialized;
    private bool ttfInitialized;
    private volatile bool shuttingDown;
    private volatile bool clientScreenReady;
    private bool f9Held;

    public void Start()
    {
        if (
            !SDL.Init(
                SDL.InitFlags.Video |
                SDL.InitFlags.Joystick |
                SDL.InitFlags.Audio))
        {
            SDL.Quit();

            if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Joystick))
            {
                ShowError("Unable to initialize SDL: " + SDL.GetError());
                return;
            }
        }

        sdlInitialized = true;

        if (!TTF.Init())
        {
            ShowError("Unable to initialize SDL_ttf: " + SDL.GetError());
            SDL.Quit();
            sdlInitialized = false;
            return;
        }

        ttfInitialized = true;

        try
        {
            machine = new C64Machine(RomSet.Load(AppContext.BaseDirectory));
            SoundTrace.AttachFromEnvironment(machine);
            machine.TrueDriveEmulation = AppSettings.Current.TrueDriveEmulation;
            machine.Sid.OutputVolume = AppSettings.Current.AudioVolume;
            machine.Sid.Muted = AppSettings.Current.AudioMuted;
            renderer = new Renderer(machine);

            AudioOutput audioOutput = new AudioOutput(AppSettings.Current.AudioDevice);
            audio = audioOutput;

            if (!audioOutput.Available)
                renderer.DisplayStatus("Audio unavailable: " + (audioOutput.Error ?? "unknown error"));

            network = new Network();
            network.ScreenDataReceived += NetworkScreenDataReceived;
            network.KeyEventReceived += NetworkKeyEventReceived;
            network.JoyEventReceived += NetworkJoyEventReceived;
            network.ClientConnected += NetworkClientConnected;
            network.ClientConnectFailed += NetworkClientConnectFailed;
            network.ClientDisconnected += NetworkClientDisconnected;
            network.IncomingClient += NetworkIncomingClient;
            network.PartedClient += NetworkPartedClient;

            menu = new Menu(renderer, machine, network, audioOutput);
            menu.MenuReturn += MenuReturn;
            menu.MenuQuit += MenuQuit;

            OpenJoysticks();
            machine.Start();

            running = true;
            RunLoop();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            Cleanup();
        }
    }

    private void RunLoop()
    {
        var nextPresent = Stopwatch.GetTimestamp();
        var nextWarpDraw = nextPresent;

        while (running)
        {
            while (SDL.PollEvent(out SDL.Event sdlEvent))
                HandleEvent(sdlEvent);

            if (!running)
                break;

            var frameReady = machine?.ConsumeFrameReady() ?? false;

            if (clientScreenReady)
            {
                clientScreenReady = false;
                frameReady = true;
            }

            var drewFrame = false;

            if (frameReady)
            {
                var warping = machine?.IsWarping == true;
                var drawNow = Stopwatch.GetTimestamp();

                // In warp the emulation thread can produce frames much
                // faster than the display. Redraw at 30 Hz so visible
                // updates continue without letting drawing dominate the
                // host's maximum emulation speed.
                if (!warping || drawNow >= nextWarpDraw)
                {
                    renderer?.Draw();
                    drewFrame = true;

                    if (warping)
                        nextWarpDraw = drawNow + Stopwatch.Frequency / 30;
                }

                SendScreenToClients();
            }

            if (frameReady || machine?.IsWarping == true)
            {
                audio?.Pump(
                    machine?.Sid,
                    machine is null || network?.IsClient == true,
                    machine?.IsWarping == true);
            }

            if (renderer is null)
            {
                SDL.Delay(10);
                continue;
            }

            // Present each completed frame once, immediately. Waiting for
            // a separate timer submitted duplicate frames and added up to
            // several milliseconds of delay jitter, which shows up as an
            // uneven picture during hardware scrolling.
            var now = Stopwatch.GetTimestamp();
            var present = drewFrame;

            if (!present && now >= nextPresent)
            {
                // No new frame: keep menus, status text and the window
                // itself refreshed without submitting duplicates at the
                // full speed of the host loop.
                present = true;
            }

            if (present)
            {
                renderer.Present();
                nextPresent =
                    Stopwatch.GetTimestamp() + C64Machine.FrameDurationTicks;
            }
            else
            {
                SDL.Delay(1);
            }
        }
    }

    private void SendScreenToClients()
    {
        if (
            machine is null ||
            network is null ||
            !network.IsHosting ||
            network.HostNumClients == 0 ||
            !network.ImageReady)
        {
            return;
        }

        network.Send(NetworkCommand.Image, machine.Vic.CapturePackedScreen());
    }

    private void HandleEvent(SDL.Event sdlEvent)
    {
        switch ((SDL.EventType)sdlEvent.Type)
        {
            case SDL.EventType.Quit:
            case SDL.EventType.WindowCloseRequested:
                running = false;
                break;

            case SDL.EventType.WindowFocusLost:
                ReleaseWarp();
                break;

            case SDL.EventType.KeyDown:
                KeyEvent(sdlEvent.Key, true);
                break;

            case SDL.EventType.KeyUp:
                KeyEvent(sdlEvent.Key, false);
                break;

            case SDL.EventType.JoystickAxisMotion:
                JoystickAxisEvent(sdlEvent.JAxis);
                break;

            case SDL.EventType.JoystickButtonDown:
                JoystickButtonEvent(sdlEvent.JButton, true);
                break;

            case SDL.EventType.JoystickButtonUp:
                JoystickButtonEvent(sdlEvent.JButton, false);
                break;

            case SDL.EventType.JoystickAdded:
                JoystickAdded(sdlEvent.JDevice.Which);
                break;

            case SDL.EventType.JoystickRemoved:
                JoystickRemoved(sdlEvent.JDevice.Which);
                break;
        }
    }

    private void KeyEvent(SDL.KeyboardEvent keyboardEvent, bool down)
    {
        KeyboardEventArgs? key = KeyMapper.Create(keyboardEvent, down);

        if (key is not null)
            ProcessKey(key);
    }

    private void OpenJoysticks()
    {
        var joysticks = SDL.GetJoysticks(out var count);

        if (joysticks is null)
            return;

        if (count > 0)
        {
            joystick1 = SDL.OpenJoystick(joysticks[0]);

            if (joystick1 != IntPtr.Zero)
                joystick1Id = joysticks[0];
        }

        if (count > 1)
        {
            joystick2 = SDL.OpenJoystick(joysticks[1]);

            if (joystick2 != IntPtr.Zero)
                joystick2Id = joysticks[1];
        }
    }

    private void JoystickAdded(uint id)
    {
        if (joystick1 == IntPtr.Zero)
        {
            joystick1 = SDL.OpenJoystick(id);
            joystick1Id = joystick1 != IntPtr.Zero ? id : 0;
        }
        else if (joystick2 == IntPtr.Zero)
        {
            joystick2 = SDL.OpenJoystick(id);
            joystick2Id = joystick2 != IntPtr.Zero ? id : 0;
        }
    }

    private void JoystickRemoved(uint id)
    {
        if (id == joystick1Id)
        {
            if (joystick1 != IntPtr.Zero)
                SDL.CloseJoystick(joystick1);

            joystick1 = IntPtr.Zero;
            joystick1Id = 0;
            machine?.Cia1.JoyEvent(1, JoystickFunction.None, false);
        }
        else if (id == joystick2Id)
        {
            if (joystick2 != IntPtr.Zero)
                SDL.CloseJoystick(joystick2);

            joystick2 = IntPtr.Zero;
            joystick2Id = 0;
            machine?.Cia1.JoyEvent(2, JoystickFunction.None, false);
        }
    }

    private int PhysicalJoystick(uint id)
    {
        if (joystick1 != IntPtr.Zero && id == joystick1Id)
            return 1;

        if (joystick2 != IntPtr.Zero && id == joystick2Id)
            return 2;

        return 0;
    }

    private void JoystickAxisEvent(SDL.JoyAxisEvent axis)
    {
        if (machine is null)
            return;

        var joystick = PhysicalJoystick(axis.Which);

        if (joystick == 0)
            return;

        var value = NormalizeAxis(axis.Value);

        if (axis.Axis == 0)
        {
            if (value > 0)
                SendJoy(joystick, JoystickFunction.Right, true);
            else if (value < 0)
                SendJoy(joystick, JoystickFunction.Left, true);
            else
                SendJoy(joystick, JoystickFunction.HorizontalCenter, true);
        }
        else
        {
            if (value > 0)
                SendJoy(joystick, JoystickFunction.Down, true);
            else if (value < 0)
                SendJoy(joystick, JoystickFunction.Up, true);
            else
                SendJoy(joystick, JoystickFunction.VerticalCenter, true);
        }
    }

    private void JoystickButtonEvent(SDL.JoyButtonEvent button, bool pressed)
    {
        if (machine is null)
            return;

        var joystick = PhysicalJoystick(button.Which);

        if (joystick != 0)
            SendJoy(joystick, JoystickFunction.Fire, pressed);
    }

    private void SendJoy(int physicalJoystick, JoystickFunction function, bool pressed)
    {
        if (machine is null)
            return;

        if (network?.IsClient == true)
        {
            network.Send(
                NetworkCommand.JoyEvent,
                [
                    (byte)physicalJoystick,
                    (byte)function,
                    (byte)(pressed ? 1 : 0)
                ]);
            return;
        }

        JoystickMap map = (JoystickMap)physicalJoystick;

        if (machine.Cia1.Joy1Map == map)
            machine.Cia1.JoyEvent(1, function, pressed);

        if (machine.Cia1.Joy2Map == map)
            machine.Cia1.JoyEvent(2, function, pressed);
    }

    private static int NormalizeAxis(short value)
    {
        if (value > 8000)
            return 1;

        if (value < -8000)
            return -1;

        return 0;
    }

    private void MenuQuit()
    {
        running = false;
    }

    private void MenuReturn()
    {
        if (renderer is null || machine is null || network is null)
            return;

        if (menu is not null)
            menu.Active = false;

        renderer.ResetBuffer();
        renderer.Draw();

        if (!network.IsClient)
            machine.Paused = false;
    }

    private void NetworkJoyEventReceived(JoystickEventArgs joystick)
    {
        machine?.Cia1.JoyEvent(
            joystick.JoystickNumber,
            joystick.Function,
            joystick.Pressed);
    }

    private void NetworkPartedClient()
    {
        renderer?.DisplayStatus("Client disconnected");
    }

    private void NetworkIncomingClient()
    {
        renderer?.DisplayStatus("Client connected");
    }

    private void NetworkKeyEventReceived(KeyboardEventArgs key)
    {
        machine?.Cia1.KeyEvent(key, true);
    }

    private void NetworkClientConnectFailed(Exception exception)
    {
        renderer?.DisplayStatus("Connection failed: " + exception.Message);
    }

    private void NetworkClientDisconnected()
    {
        if (shuttingDown || machine is null)
            return;

        machine.HardReset();
        machine.Start();
        renderer?.DisplayStatus("Disconnected");
    }

    private void NetworkClientConnected()
    {
        if (shuttingDown || machine is null)
            return;

        machine.Stop();
        renderer?.ResetBuffer();
        renderer?.DisplayStatus("Connected to host");
    }

    private void NetworkScreenDataReceived(byte[] data)
    {
        machine?.Vic.SetPackedScreen(data);
        clientScreenReady = true;
    }

    private void ReleaseWarp()
    {
        var wasHeld = f9Held;

        f9Held = false;

        if (machine is not null)
        {
            machine.FastForward = false;

            if (wasHeld)
            {
                audio?.Flush();
                machine.Sid.DiscardSamples();
            }
        }
    }

    private void SaveStateHotkey()
    {
        if (machine is null || renderer is null)
            return;

        if (network?.IsClient == true)
        {
            renderer.DisplayStatus("Save state is unavailable as a client");
            return;
        }

        try
        {
            machine.SaveStateToFile();
            renderer.DisplayStatus("State saved");
        }
        catch (Exception exception)
        {
            renderer.DisplayStatus("Save state failed: " + exception.Message);
        }
    }

    private void LoadStateHotkey()
    {
        if (machine is null || renderer is null)
            return;

        if (network?.IsClient == true)
        {
            renderer.DisplayStatus("Load state is unavailable as a client");
            return;
        }

        try
        {
            machine.LoadStateFromFile();
            renderer.ResetBuffer();
            renderer.DisplayStatus("State loaded");
        }
        catch (Exception exception)
        {
            renderer.DisplayStatus("Load state failed: " + exception.Message);
        }
    }

    private void RewindHotkey()
    {
        if (machine is null || renderer is null)
            return;

        if (network?.IsClient == true)
        {
            renderer.DisplayStatus("Rewind is unavailable as a client");
            return;
        }

        try
        {
            if (!machine.RewindOneStep(out var message))
            {
                renderer.DisplayStatus(message);
                return;
            }

            renderer.ResetBuffer();
            renderer.DisplayStatus(message);
        }
        catch (Exception exception)
        {
            renderer.DisplayStatus("Rewind failed: " + exception.Message);
        }
    }

    private void DumpLiveDiagnostic()
    {
        if (machine is null)
            return;

        try
        {
            var directory = Path.Combine(
                "C:\\Users\\david\\Documents\\My Projects\\C64-Emulator",
                "diagnostics");
            Directory.CreateDirectory(directory);

            Vic vic = machine.Vic;
            var d011 = machine.Memory.Peek(0xD011);
            var d016 = machine.Memory.Peek(0xD016);
            var d018 = machine.Memory.Peek(0xD018);
            var matrixBase = vic.BankAddress + ((d018 >> 4) * 0x400);
            var pixelBase = (d011 & 0x20) != 0
                ? vic.BankAddress + ((d018 & 0x08) * 0x400)
                : vic.BankAddress + ((d018 & 0x0E) * 0x400);
            List<string> lines = new List<string>
            {
                $"PC={machine.Cpu.GetPC():X4} MODE={vic.CurrentVideoMode} " +
                $"BANK={vic.BankAddress:X4} D011={d011:X2} D016={d016:X2} " +
                $"D018={d018:X2} MATRIX={matrixBase:X4} PIXEL={pixelBase:X4}",
                $"RASTER={vic.RasterLine}/{vic.RasterCycle} " +
                $"YSCROLL={vic.VerticalScroll} XSCROLL={vic.HorizontalScroll} " +
                $"C1={machine.Memory.Peek(0xC1):X2} C2={machine.Memory.Peek(0xC2):X2} " +
                $"JAM={machine.Cpu.IsJammed} TRUE_DRIVE={machine.TrueDriveEmulation} " +
                $"VOL={machine.Sid.OutputVolume} MUTED={machine.Sid.Muted}"
            };

            for (var register = 0; register < 0x2F; register++)
                lines.Add($"VIC{register:X2}={vic.GetRegister(register):X2}");

            for (var sprite = 0; sprite < 8; sprite++)
            {
                lines.Add(
                    $"SPRITE{sprite} PTR=" +
                    machine.Memory.Peek((ushort)(matrixBase + 0x3F8 + sprite)).ToString("X2") +
                    $" X={machine.Memory.Peek((ushort)(0xD000 + sprite * 2)) |
                        ((machine.Memory.Peek(0xD010) & (1 << sprite)) != 0 ? 0x100 : 0):X3} " +
                    $"Y={machine.Memory.Peek((ushort)(0xD001 + sprite * 2)):X2} " +
                    $"COLOR={machine.Memory.Peek((ushort)(0xD027 + sprite)):X2}");
            }

            Drive1541? drive = machine.Drive.RealDrive;

            if (drive is not null)
            {
                lines.Add(
                    $"DRIVE PC={drive.Cpu.GetPC():X4} TRK={drive.Ram[0x22]:X2} " +
                    $"MOTOR={drive.Disk.MotorOn} BYTES={drive.Disk.TotalBytesRead} " +
                    $"HEAD={drive.Disk.HeadHalfTrack}");
            }

            lines.Add("SCREEN=" + Hex(machine, matrixBase, 1000));
            lines.Add("COLOR=" + HexColorRam(machine, 1000));
            File.WriteAllText(Path.Combine(directory, "live-state.txt"), string.Join("\n", lines));
            File.WriteAllBytes(
                Path.Combine(directory, "live-matrix.bin"),
                Read(machine, matrixBase, 1000));
            File.WriteAllBytes(
                Path.Combine(directory, "live-bitmap.bin"),
                Read(machine, pixelBase, 8000));
            WriteBmp(
                Path.Combine(directory, "live-state.bmp"),
                vic.CapturePackedScreen());
            renderer?.DisplayStatus("Diagnostic captured (live-state)");
        }
        catch (Exception exception)
        {
            renderer?.DisplayStatus("Diagnostic failed: " + exception.Message);
        }
    }

    private static byte[] Read(C64Machine machine, int address, int length)
    {
        var result = new byte[length];

        for (var i = 0; i < length; i++)
            result[i] = machine.Memory.Peek((ushort)(address + i));

        return result;
    }

    private static string Hex(C64Machine machine, int address, int length)
    {
        var values = new string[length];

        for (var i = 0; i < length; i++)
            values[i] = machine.Memory.Peek((ushort)(address + i)).ToString("X2");

        return string.Concat(values);
    }

    private static string HexColorRam(C64Machine machine, int length)
    {
        var values = new string[length];

        for (var i = 0; i < length; i++)
            values[i] = machine.ColorRam.ReadNibble((ushort)i).ToString("X1");

        return string.Concat(values);
    }

    private static void WriteBmp(string path, byte[] packed)
    {
        const int width = Vic.VisibleWidth;
        const int height = Vic.VisibleHeight;
        var stride = (width * 3 + 3) & ~3;

        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new BinaryWriter(stream);

        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(54 + stride * height);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((short)1);
        writer.Write((short)24);
        writer.Write(0);
        writer.Write(stride * height);
        writer.Write(2835);
        writer.Write(2835);
        writer.Write(0);
        writer.Write(0);

        for (var y = height - 1; y >= 0; y--)
        {
            var rowStart = stream.Position;

            for (var x = 0; x < width; x++)
            {
                var pixel = y * width + x;
                var value = packed[pixel / 2];
                var colorIndex = (pixel & 1) == 0 ? value >> 4 : value & 0x0F;
                RgbColor color = C64Palette.Get(colorIndex);
                writer.Write(color.B);
                writer.Write(color.G);
                writer.Write(color.R);
            }

            while (stream.Position - rowStart < stride)
                writer.Write((byte)0);
        }
    }

    private void ProcessKey(KeyboardEventArgs key)
    {
        if (machine is null || renderer is null || network is null)
            return;

        switch (key.Key)
        {
            case Key.F5:
                if (key.Down)
                    SaveStateHotkey();
                return;

            case Key.F6:
                if (key.Down)
                    RewindHotkey();
                return;

            case Key.F7:
                if (key.Down)
                    LoadStateHotkey();
                return;

            case Key.F9:
                if (key.Down)
                {
                    if (!f9Held)
                    {
                        f9Held = true;
                        renderer.DisplayStatus("Warp on (hold F9)");
                    }

                    machine.FastForward = true;
                }
                else
                {
                    ReleaseWarp();
                    renderer.DisplayStatus("Warp off");
                }
                return;

            case Key.F10:
                if (key.Down)
                    DumpLiveDiagnostic();
                return;

            case Key.F11:
                if (!key.Down)
                {
                    machine.Cia1.SwitchJoysticks();
                    renderer.DisplayStatus("Joysticks switched");
                }
                return;

            case Key.F12:
                if (!key.Down)
                {
                    if (menu?.Active == true)
                        MenuReturn();
                    else
                    {
                        if (!network.IsClient)
                            machine.Paused = true;

                        if (menu is not null)
                            menu.Active = true;
                    }
                }
                return;
        }

        if (menu?.Active == true)
        {
            menu.KeyEvent(key);
            return;
        }

        if (network.IsClient)
        {
            network.Send(
                NetworkCommand.KeyEvent,
                [
                    (byte)key.Key,
                    (byte)(KeyboardMatrix.ShiftPressed(key) ? 1 : 0),
                    (byte)(key.Down ? 1 : 0)
                ]);
            return;
        }

        machine.Cia1.KeyEvent(key, false);
    }

    private void Cleanup()
    {
        shuttingDown = true;

        AppSettings.Current.Save();

        if (machine is not null)
        {
            machine.Cia1.JoyEvent(1, JoystickFunction.None, false);
            machine.Cia1.JoyEvent(2, JoystickFunction.None, false);
        }

        if (joystick1 != IntPtr.Zero)
            SDL.CloseJoystick(joystick1);

        if (joystick2 != IntPtr.Zero)
            SDL.CloseJoystick(joystick2);

        joystick1 = IntPtr.Zero;
        joystick2 = IntPtr.Zero;

        machine?.Dispose();
        audio?.Dispose();

        if (network is not null)
        {
            if (network.IsHosting)
                network.HostEnd();

            if (network.IsClient)
                network.ClientDisconnect();
        }

        renderer?.Kill();

        if (ttfInitialized)
            TTF.Quit();

        if (sdlInitialized)
            SDL.Quit();
    }

    private static void ShowError(string message)
    {
        SDL.ShowSimpleMessageBox(
            SDL.MessageBoxFlags.Error,
            "C64 Emulator",
            message,
            IntPtr.Zero);
    }
}
