using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using C64Emulator.Audio;
using C64Emulator.Core;
using C64Emulator.Input;
using C64Emulator.Video;

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

    private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

    private static readonly Lock SyncRoot = new();

    public static AppSettings Current { get; } = LoadFrom(SettingsFile);

    public bool Fullscreen { get; set; }

    /// <summary>
    /// In fullscreen, scale the picture to the display height instead of
    /// fitting the whole frame. The sides are cropped only when the display
    /// is narrower than the C64 picture.
    /// </summary>
    public bool FullscreenFillHeight { get; set; } = true;

    public int ResolutionWidth { get; set; } = 806;

    public int ResolutionHeight { get; set; } = 568;

    /// <summary>
    /// Windowed size last used with each video standard, so switching back
    /// restores that standard's own window instead of the other one's size.
    /// Zero means the standard has not been shown yet; the first switch then
    /// keeps the current integer zoom and stores the resulting size.
    /// </summary>
    public int PalWindowWidth { get; set; }

    public int PalWindowHeight { get; set; }

    public int NtscWindowWidth { get; set; }

    public int NtscWindowHeight { get; set; }

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
    /// Draw a subtle dark line between every other presented row. This is a
    /// host-only display effect and is off by default.
    /// </summary>
    public bool Scanlines { get; set; }

    /// <summary>
    /// PAL timing is the default; NTSC can be selected in the Display menu
    /// and is restored on the next start.
    /// </summary>
    public VideoStandard VideoStandard { get; set; } =
        C64Emulator.Video.VideoStandard.Pal;

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

    /// <summary>
    /// SID revision. The breadbin 6581 is the default; the 8580 in later
    /// C64C boards has a brighter, linear filter.
    /// </summary>
    public SidModel SidChipModel { get; set; } = SidModel.Mos6581;

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    internal static AppSettings LoadFrom(string settingsFile)
    {
        try
        {
            if (File.Exists(settingsFile))
            {
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(
                    File.ReadAllText(settingsFile),
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
        PalWindowWidth = PalWindowWidth > 0 && PalWindowHeight > 0
            ? Math.Max(VideoTiming.Pal.PresentationWidth, PalWindowWidth)
            : 0;
        PalWindowHeight = PalWindowWidth > 0
            ? Math.Max(VideoTiming.Pal.VisibleLines, PalWindowHeight)
            : 0;
        NtscWindowWidth = NtscWindowWidth > 0 && NtscWindowHeight > 0
            ? Math.Max(VideoTiming.Ntsc.PresentationWidth, NtscWindowWidth)
            : 0;
        NtscWindowHeight = NtscWindowWidth > 0
            ? Math.Max(VideoTiming.Ntsc.VisibleLines, NtscWindowHeight)
            : 0;
        DisplayRefreshRate = Math.Max(0, DisplayRefreshRate);
        AudioVolume = Math.Clamp(AudioVolume, 0, 100);
        AudioDevice = AudioDevice ?? string.Empty;

        if (!Enum.IsDefined(typeof(SidModel), SidChipModel))
            SidChipModel = SidModel.Mos6581;

        if (!Enum.IsDefined(typeof(C64Emulator.Video.VideoStandard), VideoStandard))
            VideoStandard = C64Emulator.Video.VideoStandard.Pal;
    }

    public void Save()
    {
        SaveTo(SettingsFile);
    }

    internal void SaveTo(string settingsFile)
    {
        lock (SyncRoot)
        {
            try
            {
                var directory = Path.GetDirectoryName(settingsFile);

                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var temporaryFile = settingsFile + ".tmp";

                File.WriteAllText(
                    temporaryFile,
                    JsonSerializer.Serialize(this, SerializerOptions)
                    );

                File.Move(temporaryFile, settingsFile, true);
            }
            catch
            {
                // Settings are not important enough to prevent shutdown.
            }
        }
    }
}
