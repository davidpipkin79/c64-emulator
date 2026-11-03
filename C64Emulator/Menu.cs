using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace C64Emulator
{
    internal sealed class MenuList
    {
        public MenuList(string text, string command, string[]? args = null, int level = 0)
        {
            Text = text;
            Command = command;
            Args = args ?? Array.Empty<string>();
            Level = level;
        }

        public string Text { get; set; }

        public string Command { get; }

        public string[] Args { get; }

        public int Level { get; }
    }

    /// <summary>
    /// In-emulator menu. It is intentionally a plain state machine over the
    /// renderer's text overlay rather than a second UI toolkit.
    /// </summary>
    internal sealed class Menu
    {
        public const int MaxItems = 9;

        private readonly List<MenuList> menuList = new List<MenuList>();
        private readonly Renderer renderer;
        private readonly C64Machine machine;
        private readonly Network network;
        private readonly AudioOutput audio;

        private bool active;
        private int selectedItem;
        private int currentPosition;
        private string networkAddress;
        private bool settingKeyset;
        private string keysetText = string.Empty;
        private AudioDeviceInfo[] audioDevices = Array.Empty<AudioDeviceInfo>();
        private int audioDeviceIndex;

        public Menu(Renderer renderer, C64Machine machine, Network network, AudioOutput audio)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
            this.network = network ?? throw new ArgumentNullException(nameof(network));
            this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
            networkAddress = AppSettings.Current.LastHost;
        }

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

                if (active)
                {
                    keysetText = string.Empty;
                    settingKeyset = false;
                    LoadMainMenu();
                    renderer.DrawMenu(menuList, selectedItem, currentPosition);
                }
            }
        }

        public void KeyEvent(KeyboardEventArgs key)
        {
            if (!key.Down)
            {
                if (settingKeyset)
                {
                    CaptureKeySet(key);
                }
                else
                {
                    Navigate(key);
                }
            }

            if (active)
                renderer.DrawMenu(menuList, selectedItem, currentPosition);
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

            string text = item.Text;
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
                    ProcessCommand(
                        menuList[selectedItem].Command + ".left",
                        menuList[selectedItem].Args);
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
                    EditNetworkAddress(remove: true);
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

            string address = menuList[selectedItem].Args[0];

            if (remove)
            {
                if (address.Length > 0)
                    address = address[..^1];
            }
            else if (character is { Length: 1 })
            {
                char value = character[0];

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

            menuList.Add(new MenuList("Display", "display"));
            menuList.Add(new MenuList("Audio", "audio"));
            menuList.Add(new MenuList("Input", "input"));
            menuList.Add(new MenuList("Network", "network"));
            menuList.Add(new MenuList("Load Program / Media", "load"));
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
            menuList.Add(new MenuList("Display Mode:", string.Empty));
            menuList.Add(
                new MenuList(
                    "Windowed (403 x 284)",
                    "resolution",
                    new[] { "window", "403", "284" },
                    1));
            menuList.Add(
                new MenuList(
                    "Windowed (806 x 568)",
                    "resolution",
                    new[] { "window", "806", "568" },
                    1));

            DisplaySize[] fullscreenModes = renderer.FullscreenModes;
            bool hasFiftyHertzMode = false;

            foreach (DisplaySize mode in fullscreenModes)
            {
                string label;

                if (mode.IsFiftyHertz)
                {
                    hasFiftyHertzMode = true;
                    label = $"Fullscreen 50 Hz ({mode.Width} x {mode.Height})";
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
                        new[]
                        {
                            "fullscreen",
                            mode.Width.ToString(),
                            mode.Height.ToString(),
                            mode.RefreshRate.ToString()
                        },
                        1));
            }

            if (!hasFiftyHertzMode)
            {
                menuList.Add(
                    new MenuList(
                        "No 50 Hz fullscreen mode available",
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
                    new[] { networkAddress }));
        }

        private void LoadFileMenu(string directory)
        {
            menuList.Clear();
            selectedItem = 0;
            currentPosition = 0;

            menuList.Add(new MenuList("Back to Main Menu", "main"));

            if (machine.Drive.IsMounted)
            {
                string mounted = Path.GetFileName(machine.Drive.ImagePath ?? "Disk");
                menuList.Add(
                    new MenuList(
                        "Mounted Disk: " + Shorten(mounted, 24),
                        "diskmenu"));
            }

            if (machine.Tape.IsMounted)
            {
                string mounted = Path.GetFileName(machine.Tape.ImagePath ?? "Tape");
                menuList.Add(
                    new MenuList(
                        "Mounted Tape: " + Shorten(mounted, 24),
                        "tapemenu"));
            }

            if (machine.Cartridge is not null)
            {
                string mounted = machine.Cartridge.Name;
                menuList.Add(
                    new MenuList(
                        "Eject Cartridge: " + Shorten(mounted, 20),
                        "ejectcart"));
            }

            try
            {
                DirectoryInfo info = new DirectoryInfo(directory);

                if (info.Parent is not null)
                {
                    menuList.Add(
                        new MenuList(
                            "..",
                            "directory",
                            new[] { info.Parent.FullName }));
                }

                foreach (DirectoryInfo child in info
                    .EnumerateDirectories()
                    .OrderBy(child => child.Name, StringComparer.OrdinalIgnoreCase))
                {
                    menuList.Add(
                        new MenuList(
                            child.Name + Path.DirectorySeparatorChar,
                            "directory",
                            new[] { child.FullName }));
                }

                foreach (FileInfo file in info
                    .EnumerateFiles()
                    .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase))
                {
                    string? label = Path
                        .GetExtension(file.Name)
                        .ToLowerInvariant() switch
                    {
                        ".d64" => "[D64] " + Shorten(file.Name, 30),
                        ".t64" => "[T64] " + Shorten(file.Name, 30),
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
                                new[] { file.FullName }));
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

            string image = Path.GetFileName(machine.Drive.ImagePath ?? "Disk");
            menuList.Add(new MenuList("Back to Main Menu", "main"));
            menuList.Add(new MenuList("Eject Disk", "ejectdisk"));
            menuList.Add(new MenuList("Disk: " + Shorten(image, 32), string.Empty));

            int programs = 0;

            foreach (D64FileInfo file in machine.Drive.Directory)
            {
                if (file.Type != CbmFileType.Program)
                    continue;

                menuList.Add(
                    new MenuList(
                        Shorten(file.Name, 34),
                        "diskfile",
                        new[] { file.Name }));
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

            string image = Path.GetFileName(machine.Tape.ImagePath ?? "Tape");
            menuList.Add(new MenuList("Back to Main Menu", "main"));
            menuList.Add(new MenuList("Eject Tape", "ejecttape"));
            menuList.Add(new MenuList("Tape: " + Shorten(image, 32), string.Empty));

            int programs = 0;

            foreach (T64FileInfo file in machine.Tape.Directory)
            {
                if (file.Type != CbmFileType.Program)
                    continue;

                string name = string.IsNullOrWhiteSpace(file.Name)
                    ? "(unnamed)"
                    : file.Name;
                menuList.Add(
                    new MenuList(
                        Shorten(name, 34),
                        "tapefile",
                        new[] { file.Name }));
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

            KeySet keys = keyset == 1 ? machine.Cia1.KeySet1 : machine.Cia1.KeySet2;
            string prefix = "KeySet" + keyset;

            menuList.Add(new MenuList("Back to Input Menu", "input"));
            menuList.Add(new MenuList(prefix + " Up: " + keys.Up, "keyset" + keyset, new[] { "up" }));
            menuList.Add(new MenuList(prefix + " Right: " + keys.Right, "keyset" + keyset, new[] { "right" }));
            menuList.Add(new MenuList(prefix + " Down: " + keys.Down, "keyset" + keyset, new[] { "down" }));
            menuList.Add(new MenuList(prefix + " Left: " + keys.Left, "keyset" + keyset, new[] { "left" }));
            menuList.Add(new MenuList(prefix + " Fire: " + keys.Fire, "keyset" + keyset, new[] { "fire" }));
        }

        private void ProcessCommand(string command, string[] args)
        {
            switch (command)
            {
                case "display":
                    LoadDisplayMenu();
                    break;

                case "audio":
                    LoadAudioMenu();
                    break;

                case "input":
                    LoadInputMenu();
                    break;

                case "network":
                    LoadNetworkMenu();
                    break;

                case "load":
                    LoadFileMenu(AppContext.BaseDirectory);
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
                    bool enableVSync = !renderer.VSyncEnabled;
                    bool vsyncApplied = renderer.SetVSync(enableVSync);

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

                case "resolution":
                    if (args.Length >= 3)
                    {
                        int refreshRate = args.Length >= 4
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

                case "editkey1":
                    LoadKeySetMenu(1);
                    break;

                case "editkey2":
                    LoadKeySetMenu(2);
                    break;
            }
        }

        private void UpdateItem(string command, string text)
        {
            MenuList? item = menuList.Find(candidate => candidate.Command == command);

            if (item is not null)
                item.Text = text;
        }

        private void ChangeVolume(int delta)
        {
            int volume = Math.Clamp(AppSettings.Current.AudioVolume + delta, 0, 100);

            AppSettings.Current.AudioVolume = volume;
            machine.Sid.OutputVolume = volume;
            AppSettings.Current.Save();

            menuList[2].Text = "Volume: " + volume + "%";
        }

        private void ToggleMute()
        {
            bool muted = !AppSettings.Current.AudioMuted;

            AppSettings.Current.AudioMuted = muted;
            machine.Sid.Muted = muted;
            AppSettings.Current.Save();

            menuList[3].Text = "Mute: " + (muted ? "Yes" : "No");
        }

        private void SelectNextAudioDevice(int direction)
        {
            int next = (audioDeviceIndex + direction + audioDevices.Length) % audioDevices.Length;
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
            for (int i = 0; i < audioDevices.Length; i++)
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

        private static MapType NextMap(MapType map)
        {
            int next = (int)map + 1;
            return next > 4 ? MapType.None : (MapType)next;
        }

        private static MapType PreviousMap(MapType map)
        {
            int previous = (int)map - 1;
            return previous < 0 ? (MapType)4 : (MapType)previous;
        }
    }
}
