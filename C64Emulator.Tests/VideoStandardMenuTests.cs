using C64Emulator.Host;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class VideoStandardMenuTests
{
    [Fact]
    public void Choices_MarkPalWhenItIsActive()
    {
        var choices = VideoStandardMenu.Choices(VideoStandard.Pal);

        Assert.Equal("videostandard", VideoStandardMenu.Command);
        Assert.Equal(2, choices.Length);
        Assert.Equal("pal", choices[0].Argument);
        Assert.Equal(
            "Video Standard: [X] PAL (50 Hz)",
            choices[0].Text);
        Assert.Equal("ntsc", choices[1].Argument);
        Assert.Equal(
            "Video Standard: [ ] NTSC (60 Hz)",
            choices[1].Text);
    }

    [Fact]
    public void Choices_MarkNtscWhenItIsActive()
    {
        var choices = VideoStandardMenu.Choices(VideoStandard.Ntsc);

        Assert.Equal(
            "Video Standard: [ ] PAL (50 Hz)",
            choices[0].Text);
        Assert.Equal(
            "Video Standard: [X] NTSC (60 Hz)",
            choices[1].Text);
    }

    [Theory]
    [InlineData("pal", "Pal")]
    [InlineData("PAL", "Pal")]
    [InlineData("ntsc", "Ntsc")]
    [InlineData("NTSC", "Ntsc")]
    public void TryParse_AcceptsTheMenuArguments(
        string argument,
        string expected)
    {
        Assert.True(VideoStandardMenu.TryParse(argument, out var standard));
        Assert.Equal(expected, standard.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("secam")]
    [InlineData(null)]
    public void TryParse_RejectsUnknownArguments(string? argument)
    {
        Assert.False(VideoStandardMenu.TryParse(argument, out var standard));
        Assert.Equal(VideoStandard.Pal, standard);
    }
}
