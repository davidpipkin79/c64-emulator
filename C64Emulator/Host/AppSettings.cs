using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using C64Emulator.Core;
using C64Emulator.Input;

namespace C64Emulator.Host;

internal sealed class AppSettings
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "C64Emulator"
        );

    private static readonly string SettingsFile = Path.Combine(
        SettingsDirectory,
        "settings.json"
        );

    private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();        private static readonly Lock SyncRoot = new();

    public static AppSettings Current { get; } = Load();

    public bool Fullscreen { get; set; }

    public int ResolutionWidth { get; set; } = 806;

    public int ResolutionHeight { get; set; } = 568;

    /// <summary>
    /// Fullscreen refresh rate in Hz. Zero selects the first mode with the
    /// requested resolution.
    /// </summary>
    public int DisplayRefreshRate { get; set; }

    /// <summary>
    /// Whether SDL render presentation should wait for the display refresh.
    /// A 50 Hz fullscreen mode plus VSync matches the PAL frame cadence.
    /// </summary>
    public bool VSync { get; set; } = true;

    /// <summary>
    /// Map host keys to the C64 key that prints the typed character (the
    /// default) instead of the C64 key that sits in the same place.
    /// </summary>
    public bool CharacterKeyboard { get; set; } = true;

    public int Joystick1Map { get; set; } = 3;

    public int Joystick2Map { get; set; }

    public Key KeySet1Up { get; set; } = Key.UpArrow;

    public Key KeySet1Right { get; set; } = Key.RightArrow;

    public Key KeySet1Down { get; set; } = Key.DownArrow;

    public Key KeySet1Left { get; set; } = Key.LeftArrow;

    public Key KeySet1Fire { get; set; } = Key.LeftControl;

    public Key KeySet2Up { get; set; } = Key.Keypad8;

    public Key KeySet2Right { get; set; } = Key.Keypad6;

    public Key KeySet2Down { get; set; } = Key.Keypad5;

    public Key KeySet2Left { get; set; } = Key.Keypad4;

    public Key KeySet2Fire { get; set; } = Key.LeftAlt;

    public string LastHost { get; set; } = string.Empty;

    /// <summary>
    /// Directory the media browser was last showing, so loading a program
    /// resumes in the folder that was left last.
    /// </summary>
    public string LastMediaDirectory { get; set; } = string.Empty;

    public bool ShowSpeed { get; set; }

    /// <summary>
    /// Use the cycle-level 1541 instead of the virtual KERNAL disk path.
    /// Off by default so D64 loads complete instantly.
    /// </summary>
    public bool TrueDriveEmulation { get; set; }

    public int AudioVolume { get; set; } = 80;

    public bool AudioMuted { get; set; }

    /// <summary>
    /// Playback device name. Empty means the system default device.
    /// </summary>
    public string AudioDevice { get; set; } = string.Empty;

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(
                    File.ReadAllText(SettingsFile),
                    SerializerOptions
                    );

                if (settings != null)
                {
                    settings.Normalize();
                    return settings;
                }
            }
        }
        catch
        {
            // Ignore missing, unreadable or corrupt settings and use defaults.
        }

        return new AppSettings();
    }

    private void Normalize()
    {
        LastHost = LastHost ?? string.Empty;
        LastMediaDirectory = LastMediaDirectory ?? string.Empty;
        ResolutionWidth = Math.Max(403, ResolutionWidth);
        ResolutionHeight = Math.Max(284, ResolutionHeight);
        DisplayRefreshRate = Math.Max(0, DisplayRefreshRate);
        AudioVolume = Math.Clamp(AudioVolume, 0, 100);
        AudioDevice = AudioDevice ?? string.Empty;
    }

    public void Save()
    {
        lock (SyncRoot)
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                var temporaryFile = SettingsFile + ".tmp";

                File.WriteAllText(
                    temporaryFile,
                    JsonSerializer.Serialize(this, SerializerOptions)
                    );

                File.Move(temporaryFile, SettingsFile, true);
            }
            catch
            {
                // Settings are not important enough to prevent shutdown.
            }
        }
    }
}
