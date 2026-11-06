using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using C64Emulator.Audio;
using C64Emulator.Core;
using C64Emulator.Devices;
using C64Emulator.Input;
using C64Emulator.Media;
using C64Emulator.Networking;
using C64Emulator.Video;

namespace C64Emulator.Host;

internal sealed class MenuList(string text, string command, string[]? args = null, int level = 0)
{
    public string Text { get; set; } = text;

    public string Command { get; } = command;

    public string[] Args { get; } = args ?? [];

    public int Level { get; } = level;
}

/// <summary>
/// In-emulator menu. It is intentionally a plain state machine over the
/// renderer's text overlay rather than a second UI toolkit.
/// </summary>
internal sealed class Menu(Renderer renderer, C64Machine machine, Network network, AudioOutput audio)
{
    private int MaxItems => MaxItemsFor(machine.Vic.VisibleHeight);

    /// <summary>
    /// Rows the menu can show without running past the bottom of the
    /// display window. Both standards have the same 200-line window, so
    /// both fit nine rows at the normal spacing.
    /// </summary>
    public static int MaxItemsFor(int visibleHeight)
    {
        return visibleHeight >= 200 ? 9 : 6;
    }

    private readonly List<MenuList> menuList = [];
    private readonly Renderer renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    private readonly C64Machine machine = machine ?? throw new ArgumentNullException(nameof(machine));
    private readonly Network network = network ?? throw new ArgumentNullException(nameof(network));
    private readonly AudioOutput audio = audio ?? throw new ArgumentNullException(nameof(audio));

    private const int NavigationRepeatDelayMs = 400;
    private const int NavigationRepeatIntervalMs = 70;
    private static readonly long NavigationRepeatDelay =
        Stopwatch.Frequency * NavigationRepeatDelayMs / 1000;
    private static readonly long NavigationRepeatInterval =
        Stopwatch.Frequency * NavigationRepeatIntervalMs / 1000;

    private bool active;
    private Key? heldNavigationKey;
    private long nextNavigationRepeat;
    private int selectedItem;
    private int currentPosition;
    private string networkAddress = AppSettings.Current.LastHost;
    private bool settingKeyset;
    private string keysetText = string.Empty;

    // Where Left/Backspace go from the current menu, and the folder the
    // media browser is currently showing.
    private string backTarget = "return";
    private string mediaDirectory = string.Empty;
    private AudioDeviceInfo[] audioDevices = [];
    private int audioDeviceIndex;

    public event Action? MenuReturn;

    public event Action? MenuQuit;

    public bool Active
    {
        get => active;
        set
        {
            if (active == value)
                return;

            active = value;
            heldNavigationKey = null;

            if (active)
            {
                keysetText = string.Empty;
                settingKeyset = false;
                LoadMainMenu();
                renderer.DrawMenu(menuList, selectedItem, currentPosition, HintText);
            }
        }
    }

    public void KeyEvent(KeyboardEventArgs key)
    {
        if (settingKeyset)
        {
            if (!key.Down)
                CaptureKeySet(key);

            if (active)
                renderer.DrawMenu(menuList, selectedItem, currentPosition, HintText);

            return;
        }

        if (key.Key is Key.UpArrow or Key.DownArrow)
        {
            if (key.Down)
                BeginNavigationRepeat(key.Key);
            else if (heldNavigationKey == key.Key)
                heldNavigationKey = null;

            if (active)
                renderer.DrawMenu(menuList, selectedItem, currentPosition, HintText);

            return;
        }

        if (!key.Down)
            Navigate(key);

        if (active)
            renderer.DrawMenu(menuList, selectedItem, currentPosition, HintText);
    }

    /// <summary>
    /// Advances held Up/Down navigation independently of platform key-repeat
    /// timing. The host loop calls this while the menu is active.
    /// </summary>
    public void Update()
    {
        if (!active || settingKeyset || heldNavigationKey is not { } key)
            return;

        var now = Stopwatch.GetTimestamp();

        if (now < nextNavigationRepeat)
            return;

        nextNavigationRepeat = now + NavigationRepeatInterval;
        Navigate(new KeyboardEventArgs(key, ModifierKeys.None, false));
        renderer.DrawMenu(menuList, selectedItem, currentPosition, HintText);
    }

    public void ResetNavigation()
    {
        heldNavigationKey = null;
    }

    private void BeginNavigationRepeat(Key key)
    {
        if (heldNavigationKey == key)
            return;

        heldNavigationKey = key;
        nextNavigationRepeat = Stopwatch.GetTimestamp() + NavigationRepeatDelay;
        Navigate(new KeyboardEventArgs(key, ModifierKeys.None, false));
    }

    private void CaptureKeySet(KeyboardEventArgs key)
    {
        if (key.Key == Key.Escape)
        {
            settingKeyset = false;
            menuList[selectedItem].Text = keysetText;
            return;
        }

        if (
            key.Key == Key.CapsLock ||
            key.Key == Key.NumLock ||
            key.Key == Key.LeftWindows ||
            key.Key == Key.RightWindows ||
            key.Key == Key.LeftMeta ||
            key.Key == Key.RightMeta)
        {
            return;
        }

        MenuList item = menuList[selectedItem];
        machine.Cia1.SetKeySet(
            item.Command == "keyset1" ? 1 : 2,
            item.Args[0],
            key.Key);

        var text = item.Text;
        settingKeyset = false;
        item.Text = text.Substring(0, text.IndexOf(':') + 2) + key.Key;
    }

    private void Navigate(KeyboardEventArgs key)
    {
        switch (key.Key)
        {
            case Key.DownArrow:
                if (selectedItem < menuList.Count - 1)
                    selectedItem++;

                if (selectedItem == currentPosition + MaxItems)
                    currentPosition++;
                break;

            case Key.UpArrow:
                if (selectedItem > 0)
                    selectedItem--;

                if (selectedItem < currentPosition)
                    currentPosition--;
                break;

            case Key.PageDown:
                selectedItem = Math.Min(
                    selectedItem + MaxItems,
                    menuList.Count - 1);

                if (selectedItem >= currentPosition + MaxItems)
                    currentPosition = Math.Min(
                        currentPosition + MaxItems,
                        Math.Max(0, menuList.Count - MaxItems));
                break;

            case Key.PageUp:
                selectedItem = Math.Max(0, selectedItem - MaxItems);

                if (selectedItem < currentPosition)
                    currentPosition = Math.Max(0, currentPosition - MaxItems);
                break;

            case Key.Space:
                LaunchSelected();
                break;

            case Key.Return:
                MenuList selected = menuList[selectedItem];

                if (selected.Command.StartsWith("keyset", StringComparison.Ordinal))
                {
                    keysetText = selected.Text;
                    selected.Text =
                        selected.Text.Substring(0, selected.Text.IndexOf(':') + 2) +
                        "Press a key";
                    settingKeyset = true;
                }
                else
                {
                    ProcessCommand(selected.Command, selected.Args);
                }
                break;

            case Key.LeftArrow:
                if (ChangesWithLeftRight(menuList[selectedItem].Command))
                {
                    ProcessCommand(
                        menuList[selectedItem].Command + ".left",
                        menuList[selectedItem].Args);
                }
                else
                {
                    GoBack();
                }
                break;

            case Key.RightArrow:
                ProcessCommand(
                    menuList[selectedItem].Command + ".right",
                    menuList[selectedItem].Args);
                break;

            case Key.Escape:
                MenuReturn?.Invoke();
                break;

            case Key.Backspace:
                if (menuList[selectedItem].Command == "connect")
                    EditNetworkAddress(remove: true);
                else
                    GoBack();
                break;

            default:
                EditNetworkAddress(remove: false, key.KeyboardCharacter);
                break;
        }
    }

    private void EditNetworkAddress(bool remove, string? character = null)
    {
        if (menuList[selectedItem].Command != "connect")
            return;

        var address = menuList[selectedItem].Args[0];

        if (remove)
        {
            if (address.Length > 0)
                address = address[..^1];
        }
        else if (character is { Length: 1 })
        {
            var value = character[0];

            if (value is >= '!' and <= 'z')
                address += value;
        }

        menuList[selectedItem].Args[0] = address;
        menuList[selectedItem].Text = "Connect to Host: " + address;
    }

    private void LoadMainMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        backTarget = "return";

        menuList.Add(new MenuList("Display", "display"));
        menuList.Add(new MenuList("Audio", "audio"));
        menuList.Add(new MenuList("Input", "input"));
        menuList.Add(new MenuList("Hot Keys", "hotkeys"));
        menuList.Add(new MenuList("Network", "network"));
        menuList.Add(new MenuList("Load Program / Media", "load"));
        menuList.Add(new MenuList("Save State", "savestate"));
        menuList.Add(new MenuList("Load State", "loadstate"));
        menuList.Add(new MenuList("Rewind", "rewind"));
        menuList.Add(new MenuList("Soft Reset", "softreset"));
        menuList.Add(new MenuList("Hard Reset", "hardreset"));
        menuList.Add(
            new MenuList(
                "True Drive Emulation: " +
                (machine.TrueDriveEmulation ? "Yes" : "No"),
                "truedrive"));
        menuList.Add(new MenuList("Exit Menu", "return"));
        menuList.Add(new MenuList("Quit", "quit"));
    }

    private void LoadDisplayMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(
            new MenuList(
                "Display FPS: " + (renderer.DisplayFPS ? "Yes" : "No"),
                "fps"));
        menuList.Add(
            new MenuList(
                "VSync: " + (renderer.VSyncEnabled ? "Yes" : "No"),
                "vsync"));
        menuList.Add(
            new MenuList(
                VideoStandardText(machine.VideoStandard),
                "videostandard"));
        menuList.Add(
            new MenuList(
                "Scanlines: " + (renderer.Scanlines ? "On" : "Off"),
                "scanlines"));
        menuList.Add(
            new MenuList(
                "Fullscreen Scaling: " +
                (renderer.FullscreenFillHeight ? "Fill Height" : "Fit"),
                "fullscreenfill"));
        menuList.Add(new MenuList("Display Mode:", string.Empty));

        // The VIC's pixels are not square, so the full border-to-border
        // frame presents at 4:3: 377 x 284 in PAL and 314 x 235 in NTSC.
        var frameWidth = renderer.PresentationWidth;
        var frameHeight = renderer.PresentationHeight;

        // Whole-number zooms keep the window on the picture's aspect, so
        // the frame fills it without letterbox bars.
        for (var scale = 1; scale <= 4; scale++)
        {
            menuList.Add(
                new MenuList(
                    $"Windowed ({frameWidth * scale} x {frameHeight * scale})",
                    "resolution",
                    [
                        "window",
                        (frameWidth * scale).ToString(),
                        (frameHeight * scale).ToString()
                    ],
                    1));
        }

        menuList.Add(
            new MenuList(
                "Windowed (last size)",
                "resolution",
                [
                    "window",
                    renderer.LastWindowWidth.ToString(),
                    renderer.LastWindowHeight.ToString()
                ],
                1));

        DisplaySize[] fullscreenModes = renderer.FullscreenModes;
        var hasPreferredMode = false;

        foreach (DisplaySize mode in fullscreenModes)
        {
            string label;

            if (mode.IsPreferred(machine.VideoStandard))
            {
                hasPreferredMode = true;
                label = machine.VideoStandard == VideoStandard.Ntsc
                    ? $"Fullscreen 60 Hz ({mode.Width} x {mode.Height})"
                    : $"Fullscreen 50 Hz ({mode.Width} x {mode.Height})";
            }
            else if (mode.IsSixtyHertz)
            {
                label = $"Fullscreen 60 Hz ({mode.Width} x {mode.Height})";
            }
            else if (mode.RefreshRate > 0)
            {
                label =
                    $"Fullscreen ({mode.Width} x {mode.Height} @ " +
                    $"{mode.RefreshRate} Hz)";
            }
            else
            {
                label = $"Fullscreen ({mode.Width} x {mode.Height})";
            }

            menuList.Add(
                new MenuList(
                    label,
                    "resolution",
                    [
                        "fullscreen",
                        mode.Width.ToString(),
                        mode.Height.ToString(),
                        mode.RefreshRate.ToString()
                    ],
                    1));
        }

        if (!hasPreferredMode)
        {
            menuList.Add(
                new MenuList(
                    machine.VideoStandard == VideoStandard.Ntsc
                        ? "No 60 Hz fullscreen mode available"
                        : "No 50 Hz fullscreen mode available",
                    string.Empty,
                    level: 1));
        }
    }

    private void LoadAudioMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;
        audioDevices = AudioOutput.GetPlaybackDevices();
        audioDeviceIndex = FindAudioDeviceIndex();

        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(
            new MenuList(
                "Device: " + Shorten(audioDevices[audioDeviceIndex].Name),
                "audiodevice"));
        menuList.Add(
            new MenuList(
                "Volume: " + AppSettings.Current.AudioVolume + "%",
                "volume"));
        menuList.Add(
            new MenuList(
                "Mute: " + (AppSettings.Current.AudioMuted ? "Yes" : "No"),
                "mute"));
    }

    private void LoadInputMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(
            new MenuList(
                "Joystick 1 Map: " + machine.Cia1.Joy1Map,
                "joy1map"));
        menuList.Add(
            new MenuList(
                "Joystick 2 Map: " + machine.Cia1.Joy2Map,
                "joy2map"));
        menuList.Add(new MenuList("Edit KeySet1", "editkey1"));
        menuList.Add(new MenuList("Edit KeySet2", "editkey2"));
        menuList.Add(new MenuList(KeyboardMappingText(), "keyboard"));
    }

    private void LoadNetworkMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(
            new MenuList(
                network.IsHosting ? "Stop Hosting" : "Start Hosting",
                network.IsHosting ? "stophost" : "starthost"));
        menuList.Add(
            new MenuList(
                network.IsClient
                    ? "Disconnect from Host"
                    : "Connect to Host: " + networkAddress,
                network.IsClient ? "disconnect" : "connect",
                [networkAddress]));
    }

    private void LoadFileMenu(string directory)
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        menuList.Add(new MenuList("Back to Main Menu", "main"));

        if (machine.Drive.IsMounted)
        {
            var mounted = Path.GetFileName(machine.Drive.ImagePath ?? "Disk");
            menuList.Add(
                new MenuList(
                    "Mounted Disk: " + Shorten(mounted, 24),
                    "diskmenu"));
        }

        if (machine.Tape.IsMounted)
        {
            var mounted = Path.GetFileName(machine.Tape.ImagePath ?? "Tape");
            menuList.Add(
                new MenuList(
                    "Mounted Tape: " + Shorten(mounted, 24),
                    "tapemenu"));
        }

        if (machine.Cartridge is not null)
        {
            var mounted = machine.Cartridge.Name;
            menuList.Add(
                new MenuList(
                    "Eject Cartridge: " + Shorten(mounted, 20),
                    "ejectcart"));
        }

        try
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            mediaDirectory = info.FullName;
            backTarget = "up";
            RememberDirectory(info);

            if (info.Parent is not null)
            {
                menuList.Add(
                    new MenuList(
                        "..",
                        "directory",
                        [info.Parent.FullName]));
            }

            foreach (DirectoryInfo child in info
                .EnumerateDirectories()
                .OrderBy(child => child.Name, StringComparer.OrdinalIgnoreCase))
            {
                menuList.Add(
                    new MenuList(
                        child.Name + Path.DirectorySeparatorChar,
                        "directory",
                        [child.FullName]));
            }

            foreach (FileInfo file in info
                .EnumerateFiles()
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase))
            {
                var label = Path
                    .GetExtension(file.Name)
                    .ToLowerInvariant() switch
                {
                    ".d64" => "[D64] " + Shorten(file.Name, 30),
                    ".t64" => "[T64] " + Shorten(file.Name, 30),
                    ".tap" => "[TAP] " + Shorten(file.Name, 30),
                    ".crt" => "[CRT] " + Shorten(file.Name, 30),
                    ".prg" => "[PRG] " + Shorten(file.Name, 30),
                    _ => null
                };

                if (label is not null)
                {
                    menuList.Add(
                        new MenuList(
                            label,
                            "loadfile",
                            [file.FullName]));
                }
            }
        }
        catch (Exception ex)
        {
            menuList.Add(
                new MenuList(
                    "Unable to read directory: " + ex.Message,
                    string.Empty));
        }
    }

    private void LoadDiskMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        if (!machine.Drive.IsMounted)
        {
            LoadMainMenu();
            return;
        }

        backTarget = "browse";

        var image = Path.GetFileName(machine.Drive.ImagePath ?? "Disk");
        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(new MenuList("Eject Disk", "ejectdisk"));
        menuList.Add(new MenuList("Disk: " + Shorten(image, 32), string.Empty));

        var programs = 0;

        foreach (D64FileInfo file in machine.Drive.Directory)
        {
            if (file.Type != CbmFileType.Program)
                continue;

            menuList.Add(
                new MenuList(
                    Shorten(file.Name, 34),
                    "diskfile",
                    [file.Name]));
            programs++;
        }

        if (programs == 0)
            menuList.Add(new MenuList("No PRG files on disk", string.Empty));
    }

    private void LoadTapeMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        if (!machine.Tape.IsMounted)
        {
            LoadMainMenu();
            return;
        }

        backTarget = "browse";

        var image = Path.GetFileName(machine.Tape.ImagePath ?? "Tape");
        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(new MenuList("Eject Tape", "ejecttape"));
        menuList.Add(new MenuList("Tape: " + Shorten(image, 32), string.Empty));

        var programs = 0;

        foreach (ITapeFileInfo file in machine.Tape.Directory)
        {
            if (file.Type != CbmFileType.Program)
                continue;

            var name = string.IsNullOrWhiteSpace(file.Name)
                ? "(unnamed)"
                : file.Name;
            menuList.Add(
                new MenuList(
                    Shorten(name, 34),
                    "tapefile",
                    [file.Name]));
            programs++;
        }

        if (programs == 0)
            menuList.Add(new MenuList("No PRG files on tape", string.Empty));
    }

    private void LoadKeySetMenu(int keyset)
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;

        backTarget = "input";

        KeySet keys = keyset == 1 ? machine.Cia1.KeySet1 : machine.Cia1.KeySet2;
        var prefix = "KeySet" + keyset;

        menuList.Add(new MenuList("Back to Input Menu", "input"));
        menuList.Add(new MenuList(prefix + " Up: " + keys.Up, "keyset" + keyset, ["up"]));
        menuList.Add(new MenuList(prefix + " Right: " + keys.Right, "keyset" + keyset, ["right"]));
        menuList.Add(new MenuList(prefix + " Down: " + keys.Down, "keyset" + keyset, ["down"]));
        menuList.Add(new MenuList(prefix + " Left: " + keys.Left, "keyset" + keyset, ["left"]));
        menuList.Add(new MenuList(prefix + " Fire: " + keys.Fire, "keyset" + keyset, ["fire"]));
    }

    private void LoadHotKeysMenu()
    {
        menuList.Clear();
        selectedItem = 0;
        currentPosition = 0;
        backTarget = "main";

        menuList.Add(new MenuList("Back to Main Menu", "main"));
        menuList.Add(new MenuList("F5: Save quick state", string.Empty));
        menuList.Add(new MenuList("F6: Rewind one step", string.Empty));
        menuList.Add(new MenuList("F7: Load quick state", string.Empty));
        menuList.Add(new MenuList("F9 (hold): Warp / fast forward", string.Empty));
        menuList.Add(new MenuList("F10: Save diagnostic dump", string.Empty));
        menuList.Add(new MenuList("F11: Swap joysticks", string.Empty));
        menuList.Add(new MenuList("F12 / Esc: Open or close menu", string.Empty));
        menuList.Add(new MenuList("Up / Down: Move (hold to repeat), Page: Jump", string.Empty));
        menuList.Add(new MenuList("Enter: Select   Space: Launch", string.Empty));
        menuList.Add(new MenuList("Left / Backspace: Go back", string.Empty));
        menuList.Add(new MenuList("Left / Right: Change a setting", string.Empty));
    }

    private void ProcessCommand(string command, string[] args)
    {
        switch (command)
        {
            case "display":
                backTarget = "main";
                LoadDisplayMenu();
                break;

            case "audio":
                backTarget = "main";
                LoadAudioMenu();
                break;

            case "input":
                backTarget = "main";
                LoadInputMenu();
                break;

            case "hotkeys":
                backTarget = "main";
                LoadHotKeysMenu();
                break;

            case "network":
                backTarget = "main";
                LoadNetworkMenu();
                break;

            case "load":
                LoadFileMenu(ResolveMediaDirectory());
                break;

            case "loadfile":
                if (args.Length == 0)
                    break;

                try
                {
                    switch (Path.GetExtension(args[0]).ToLowerInvariant())
                    {
                        case ".prg":
                            machine.LoadProgram(args[0]);
                            MenuReturn?.Invoke();
                            renderer.DisplayStatus("Loaded " + Path.GetFileName(args[0]));
                            break;

                        case ".d64":
                            machine.MountDisk(args[0]);
                            LoadDiskMenu();
                            renderer.DisplayStatus("Mounted " + Path.GetFileName(args[0]));
                            break;

                        case ".t64":
                        case ".tap":
                            machine.MountTape(args[0]);
                            LoadTapeMenu();
                            renderer.DisplayStatus("Mounted " + Path.GetFileName(args[0]));
                            break;

                        case ".crt":
                            machine.MountCartridge(args[0]);
                            MenuReturn?.Invoke();
                            renderer.DisplayStatus(
                                "Mounted cartridge " + Path.GetFileName(args[0]));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    renderer.DisplayStatus("Load failed: " + ex.Message);
                }
                break;

            case "diskmenu":
                LoadDiskMenu();
                break;

            case "ejectdisk":
                machine.EjectDisk();
                LoadMainMenu();
                renderer.DisplayStatus("Disk ejected");
                break;

            case "ejectcart":
                machine.EjectCartridge();
                LoadMainMenu();
                renderer.DisplayStatus("Cartridge ejected");
                break;

            case "tapemenu":
                LoadTapeMenu();
                break;

            case "ejecttape":
                machine.EjectTape();
                LoadMainMenu();
                renderer.DisplayStatus("Tape ejected");
                break;

            case "tapefile":
                if (args.Length > 0)
                {
                    try
                    {
                        machine.LoadTapeProgram(args[0]);
                        MenuReturn?.Invoke();
                        renderer.DisplayStatus("Loaded " + args[0]);
                    }
                    catch (Exception ex)
                    {
                        renderer.DisplayStatus("Tape load failed: " + ex.Message);
                    }
                }
                break;

            case "diskfile":
                if (args.Length > 0)
                {
                    try
                    {
                        machine.LoadDiskProgram(args[0]);
                        MenuReturn?.Invoke();
                        renderer.DisplayStatus("Loaded " + args[0]);
                    }
                    catch (Exception ex)
                    {
                        renderer.DisplayStatus("Disk load failed: " + ex.Message);
                    }
                }
                break;

            case "savestate":
                try
                {
                    machine.SaveStateToFile();
                    renderer.DisplayStatus("State saved");
                }
                catch (Exception ex)
                {
                    renderer.DisplayStatus("Save state failed: " + ex.Message);
                }
                break;

            case "loadstate":
                try
                {
                    machine.LoadStateFromFile();
                    MenuReturn?.Invoke();
                    renderer.DisplayStatus("State loaded");
                }
                catch (Exception ex)
                {
                    renderer.DisplayStatus("Load state failed: " + ex.Message);
                }
                break;

            case "rewind":
                try
                {
                    if (machine.RewindOneStep(out var message))
                    {
                        MenuReturn?.Invoke();
                        renderer.DisplayStatus(message);
                    }
                    else
                    {
                        renderer.DisplayStatus(message);
                    }
                }
                catch (Exception ex)
                {
                    renderer.DisplayStatus("Rewind failed: " + ex.Message);
                }
                break;

            case "softreset":
                machine.SoftReset();
                MenuReturn?.Invoke();
                break;

            case "hardreset":
                machine.HardReset();
                MenuReturn?.Invoke();
                break;

            case "truedrive":
                machine.TrueDriveEmulation = !machine.TrueDriveEmulation;
                AppSettings.Current.TrueDriveEmulation =
                    machine.TrueDriveEmulation;
                AppSettings.Current.Save();
                UpdateItem(
                    "truedrive",
                    "True Drive Emulation: " +
                    (machine.TrueDriveEmulation ? "Yes" : "No"));
                renderer.DisplayStatus(
                    machine.TrueDriveEmulation
                        ? "Hardware 1541 enabled"
                        : "Virtual fast disk enabled");
                break;

            case "return":
                MenuReturn?.Invoke();
                break;

            case "quit":
                MenuQuit?.Invoke();
                break;

            case "main":
                LoadMainMenu();
                break;

            case "fps.left":
            case "fps.right":
                renderer.DisplayFPS = !renderer.DisplayFPS;
                menuList[1].Text = "Display FPS: " + (renderer.DisplayFPS ? "Yes" : "No");
                break;

            case "vsync":
            case "vsync.left":
            case "vsync.right":
                var enableVSync = !renderer.VSyncEnabled;
                var vsyncApplied = renderer.SetVSync(enableVSync);

                menuList[2].Text =
                    "VSync: " + (renderer.VSyncEnabled ? "Yes" : "No");
                AppSettings.Current.Save();

                if (!vsyncApplied)
                {
                    renderer.DisplayStatus(
                        enableVSync
                            ? "VSync is not supported by this display driver"
                            : "VSync could not be disabled by this display driver");
                }
                break;

            case "videostandard":
            case "videostandard.left":
            case "videostandard.right":
                ToggleVideoStandard();
                break;

            case "scanlines":
            case "scanlines.left":
            case "scanlines.right":
                ToggleScanlines();
                break;

            case "fullscreenfill":
            case "fullscreenfill.left":
            case "fullscreenfill.right":
                ToggleFullscreenFillHeight();
                break;

            case "resolution":
                if (args.Length >= 3)
                {
                    var refreshRate = args.Length >= 4
                        ? int.Parse(args[3])
                        : 0;

                    renderer.Reset(
                        args[0] == "fullscreen",
                        int.Parse(args[1]),
                        int.Parse(args[2]),
                        refreshRate);
                    AppSettings.Current.Save();
                }
                break;

            case "audiodevice":
            case "audiodevice.right":
                SelectNextAudioDevice(1);
                break;

            case "audiodevice.left":
                SelectNextAudioDevice(-1);
                break;

            case "volume":
            case "volume.right":
                ChangeVolume(5);
                break;

            case "volume.left":
                ChangeVolume(-5);
                break;

            case "mute":
            case "mute.left":
            case "mute.right":
                ToggleMute();
                break;

            case "starthost":
                MenuReturn?.Invoke();

                if (network.IsClient)
                    network.ClientDisconnect();

                network.HostBegin(Network.NetworkPort);
                machine.Start();
                renderer.DisplayStatus("Started hosting");
                break;

            case "stophost":
                MenuReturn?.Invoke();
                network.HostEnd();
                renderer.DisplayStatus("Stopped hosting");
                break;

            case "connect":
                MenuReturn?.Invoke();
                AppSettings.Current.LastHost = args[0];

                if (network.IsHosting)
                    network.HostEnd();

                network.ClientConnect(args[0], Network.NetworkPort);
                renderer.DisplayStatus("Trying to connect to host...");
                break;

            case "disconnect":
                MenuReturn?.Invoke();
                network.ClientDisconnect();
                break;

            case "directory":
                if (args.Length > 0)
                    LoadFileMenu(args[0]);
                break;

            case "joy1map.left":
                machine.Cia1.Joy1Map = PreviousMap(machine.Cia1.Joy1Map);
                menuList[1].Text = "Joystick 1 Map: " + machine.Cia1.Joy1Map;
                break;

            case "joy1map.right":
                machine.Cia1.Joy1Map = NextMap(machine.Cia1.Joy1Map);
                menuList[1].Text = "Joystick 1 Map: " + machine.Cia1.Joy1Map;
                break;

            case "joy2map.left":
                machine.Cia1.Joy2Map = PreviousMap(machine.Cia1.Joy2Map);
                menuList[2].Text = "Joystick 2 Map: " + machine.Cia1.Joy2Map;
                break;

            case "joy2map.right":
                machine.Cia1.Joy2Map = NextMap(machine.Cia1.Joy2Map);
                menuList[2].Text = "Joystick 2 Map: " + machine.Cia1.Joy2Map;
                break;

            case "keyboard":
            case "keyboard.left":
            case "keyboard.right":
                ToggleKeyboardMapping();
                break;

            case "editkey1":
                LoadKeySetMenu(1);
                break;

            case "editkey2":
                LoadKeySetMenu(2);
                break;
        }
    }

    /// <summary>
    /// One-line help for the highlighted item. Only media files can be
    /// launched directly, so only they show a shortcut.
    /// </summary>
    private string HintText
    {
        get
        {
            if (selectedItem >= menuList.Count)
                return string.Empty;

            MenuList selected = menuList[selectedItem];

            if (selected.Command == "directory")
                return "Enter: Open   Backspace: Back";

            if (selected.Command != "loadfile" || selected.Args.Length == 0)
                return string.Empty;

            return Path.GetExtension(selected.Args[0]).ToLowerInvariant() switch
            {
                ".d64" => "Space: Launch   Enter: Explore disk",
                ".t64" => "Space: Launch   Enter: Explore tape",
                ".tap" => "Space: Launch   Enter: Explore tape",
                ".prg" => "Space: Launch",
                ".crt" => "Space: Launch",
                _ => string.Empty
            };
        }
    }

    private void LaunchSelected()
    {
        if (selectedItem >= menuList.Count)
            return;

        MenuList selected = menuList[selectedItem];

        if (selected.Command != "loadfile" || selected.Args.Length == 0)
            return;

        LaunchFile(selected.Args[0]);
    }

    /// <summary>
    /// Loads the highlighted media without opening its own list. A disk or
    /// tape is booted the way a real C64 would: the BASIC command is typed
    /// into the keyboard buffer, so a boot loader sees the same state it
    /// would after a manual LOAD followed by RUN.
    /// </summary>
    private void LaunchFile(string path)
    {
        var name = Path.GetFileName(path);

        try
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".prg":
                    machine.LoadProgram(path);
                    MenuReturn?.Invoke();
                    renderer.DisplayStatus("Loaded " + name);
                    break;

                case ".d64":
                    machine.MountDisk(path);
                    MenuReturn?.Invoke();
                    machine.TypeText("LOAD\"*\",8,1\rRUN\r");
                    renderer.DisplayStatus("Mounted " + name + ": LOAD\"*\",8,1");
                    break;

                case ".t64":
                    machine.MountTape(path);
                    MenuReturn?.Invoke();
                    machine.TypeText("LOAD\"*\",1,1\rRUN\r");
                    renderer.DisplayStatus("Mounted " + name + ": LOAD\"*\",1,1");
                    break;

                case ".tap":
                    machine.MountTape(path);
                    MenuReturn?.Invoke();
                    machine.TypeText("LOAD\"\",1,1\rRUN\r");
                    renderer.DisplayStatus("Mounted " + name + ": LOAD\"\",1,1");
                    break;

                case ".crt":
                    machine.MountCartridge(path);
                    MenuReturn?.Invoke();
                    renderer.DisplayStatus("Mounted cartridge " + name);
                    break;
            }
        }
        catch (Exception ex)
        {
            renderer.DisplayStatus("Load failed: " + ex.Message);
        }
    }

    /// <summary>
    /// Left arrow and Backspace step out one level. A setting keeps its
    /// sideways adjustment, a folder goes to its parent, a disk or tape list
    /// returns to the browser, and the main menu closes.
    /// </summary>
    private void GoBack()
    {
        switch (backTarget)
        {
            case "return":
                MenuReturn?.Invoke();
                break;

            case "browse":
                LoadFileMenu(ResolveMediaDirectory());
                break;

            case "up":
                if (
                    !string.IsNullOrEmpty(mediaDirectory) &&
                    Directory.GetParent(mediaDirectory) is { } parent)
                {
                    LoadFileMenu(parent.FullName);
                }
                else
                {
                    LoadMainMenu();
                }
                break;

            default:
                ProcessCommand(backTarget, []);
                break;
        }
    }

    private static string KeyboardMappingText()
    {
        return
            "Keyboard: " +
            (AppSettings.Current.CharacterKeyboard ? "Character" : "Positional");
    }

    private void ToggleKeyboardMapping()
    {
        AppSettings.Current.CharacterKeyboard = !AppSettings.Current.CharacterKeyboard;
        AppSettings.Current.Save();
        UpdateItem("keyboard", KeyboardMappingText());
        renderer.DisplayStatus(
            AppSettings.Current.CharacterKeyboard
                ? "Keyboard types characters"
                : "Keyboard uses C64 key positions");
    }

    /// <summary>
    /// Switches between PAL and NTSC. The machine resets so the KERNAL can
    /// redetect the frame rate at $02A6, and the renderer rebuilds its frame
    /// texture for the new picture height.
    /// </summary>
    private void ToggleVideoStandard()
    {
        VideoStandard next = machine.VideoStandard == VideoStandard.Pal
            ? VideoStandard.Ntsc
            : VideoStandard.Pal;

        machine.VideoStandard = next;
        AppSettings.Current.VideoStandard = next;
        renderer.ApplyVideoStandard();
        AppSettings.Current.Save();

        // Rebuild the display menu so the window presets and the fullscreen
        // modes follow the new frame size and refresh rate, keeping the
        // highlight on the video-standard item.
        var index = selectedItem;

        LoadDisplayMenu();
        selectedItem = Math.Min(index, menuList.Count - 1);
        currentPosition = Math.Clamp(
            selectedItem - (MaxItems - 1),
            0,
            Math.Max(0, menuList.Count - MaxItems));

        renderer.DisplayStatus(
            next == VideoStandard.Ntsc
                ? "NTSC timing: 1022727 Hz, 59.83 Hz frames"
                : "PAL timing: 985248 Hz, 50.13 Hz frames");
    }

    private void ToggleScanlines()
    {
        var enabled = !renderer.Scanlines;

        renderer.SetScanlines(enabled);
        AppSettings.Current.Save();
        UpdateItem(
            "scanlines",
            "Scanlines: " + (enabled ? "On" : "Off"));
        renderer.DisplayStatus(
            enabled ? "Scanlines enabled" : "Scanlines disabled");
    }

    private void ToggleFullscreenFillHeight()
    {
        var enabled = !renderer.FullscreenFillHeight;

        renderer.SetFullscreenFillHeight(enabled);
        AppSettings.Current.Save();
        UpdateItem(
            "fullscreenfill",
            "Fullscreen Scaling: " + (enabled ? "Fill Height" : "Fit"));
        renderer.DisplayStatus(
            enabled
                ? "Fullscreen scales to fill the display height"
                : "Fullscreen fits the whole picture");
    }

    private static string VideoStandardText(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc
            ? "Video Standard: NTSC (60 Hz)"
            : "Video Standard: PAL (50 Hz)";
    }

    /// <summary>
    /// Settings that answer Left and Right keep those keys instead of using
    /// them to step back out of the menu.
    /// </summary>
    private static bool ChangesWithLeftRight(string command)
    {
        return command is
            "fps" or
            "vsync" or
            "videostandard" or
            "scanlines" or
            "fullscreenfill" or
            "audiodevice" or
            "volume" or
            "mute" or
            "keyboard";
    }

    private static string ResolveMediaDirectory()
    {
        var directory = AppSettings.Current.LastMediaDirectory;

        return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)
            ? directory
            : AppContext.BaseDirectory;
    }

    private static void RememberDirectory(DirectoryInfo info)
    {
        if (!info.Exists)
            return;

        if (
            string.Equals(
                AppSettings.Current.LastMediaDirectory,
                info.FullName,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AppSettings.Current.LastMediaDirectory = info.FullName;
        AppSettings.Current.Save();
    }

    private void UpdateItem(string command, string text)
    {
        MenuList? item = menuList.Find(candidate => candidate.Command == command);

        if (item is not null)
            item.Text = text;
    }

    private void ChangeVolume(int delta)
    {
        var volume = Math.Clamp(AppSettings.Current.AudioVolume + delta, 0, 100);

        AppSettings.Current.AudioVolume = volume;
        machine.Sid.OutputVolume = volume;
        AppSettings.Current.Save();

        menuList[2].Text = "Volume: " + volume + "%";
    }

    private void ToggleMute()
    {
        var muted = !AppSettings.Current.AudioMuted;

        AppSettings.Current.AudioMuted = muted;
        machine.Sid.Muted = muted;
        AppSettings.Current.Save();

        menuList[3].Text = "Mute: " + (muted ? "Yes" : "No");
    }

    private void SelectNextAudioDevice(int direction)
    {
        var next = (audioDeviceIndex + direction + audioDevices.Length) % audioDevices.Length;
        AudioDeviceInfo device = audioDevices[next];

        if (audio.SelectDevice(device.Id))
        {
            audioDeviceIndex = next;
            AppSettings.Current.AudioDevice = device.IsDefault ? string.Empty : device.Name;
            AppSettings.Current.Save();
        }
        else
        {
            renderer.DisplayStatus(
                "Audio device unavailable: " + (audio.Error ?? "unknown error"));

            audioDevices = AudioOutput.GetPlaybackDevices();
            audioDeviceIndex = FindAudioDeviceIndex();
        }

        menuList[1].Text = "Device: " + Shorten(audioDevices[audioDeviceIndex].Name);
    }

    private int FindAudioDeviceIndex()
    {
        for (var i = 0; i < audioDevices.Length; i++)
        {
            if (audioDevices[i].Id == audio.CurrentDeviceId)
                return i;
        }

        return 0;
    }

    private static string Shorten(string text, int maxLength = 38)
    {
        if (text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }

    private static JoystickMap NextMap(JoystickMap map)
    {
        var next = (int)map + 1;
        return next > 4 ? JoystickMap.None : (JoystickMap)next;
    }

    private static JoystickMap PreviousMap(JoystickMap map)
    {
        var previous = (int)map - 1;
        return previous < 0 ? (JoystickMap)4 : (JoystickMap)previous;
    }
}
