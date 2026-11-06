using System;
using System.IO;
using C64Emulator.Host;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class AppSettingsTests : IDisposable
{
    private readonly string directory;
    private readonly string settingsPath;

    public AppSettingsTests()
    {
        directory = Path.Combine(
            Path.GetTempPath(),
            "C64EmulatorTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        settingsPath = Path.Combine(directory, "settings.json");
    }

    public void Dispose()
    {
        Directory.Delete(directory, recursive: true);
    }

    [Theory]
    [InlineData("pal", "Pal")]
    [InlineData("ntsc", "Ntsc")]
    public void VideoStandard_IsSavedAndRestored(
        string savedStandard,
        string expectedStandard)
    {
        var settings = new AppSettings
        {
            VideoStandard = savedStandard == "ntsc"
                ? VideoStandard.Ntsc
                : VideoStandard.Pal
        };

        settings.SaveTo(settingsPath);
        AppSettings restored = AppSettings.LoadFrom(settingsPath);

        Assert.Equal(expectedStandard, restored.VideoStandard.ToString());
    }

    [Fact]
    public void LoadFrom_NormalizesAnUnknownVideoStandardToPal()
    {
        File.WriteAllText(
            settingsPath,
            """
            {
              "Fullscreen": true,
              "VideoStandard": 99
            }
            """);

        AppSettings restored = AppSettings.LoadFrom(settingsPath);

        Assert.Equal(VideoStandard.Pal, restored.VideoStandard);
        Assert.True(restored.Fullscreen);
    }

    [Theory]
    [InlineData("{\"Fullscreen\": true, \"VideoStandard\": \"Secam\"}")]
    [InlineData("{ this is not valid JSON")]
    public void LoadFrom_UsesDefaultsForUnusableSettings(string json)
    {
        File.WriteAllText(settingsPath, json);

        AppSettings restored = AppSettings.LoadFrom(settingsPath);

        Assert.Equal(VideoStandard.Pal, restored.VideoStandard);
        Assert.False(restored.Fullscreen);
    }

    [Fact]
    public void LoadFrom_UsesDefaultsWhenTheSettingsFileIsMissing()
    {
        Assert.False(File.Exists(settingsPath));

        AppSettings restored = AppSettings.LoadFrom(settingsPath);

        Assert.Equal(VideoStandard.Pal, restored.VideoStandard);
        Assert.False(restored.Fullscreen);
        Assert.Equal(806, restored.ResolutionWidth);
        Assert.Equal(568, restored.ResolutionHeight);
        Assert.True(restored.VSync);
        Assert.Equal(80, restored.AudioVolume);
    }
}
