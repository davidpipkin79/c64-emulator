using System;
using C64Emulator.Host;
using Xunit;

namespace C64Emulator.Tests;

public sealed class CommandLineTests
{
    [Fact]
    public void NoArguments_StartsTheWindow()
    {
        CommandLineOptions options = CommandLineOptions.Parse([]);

        Assert.False(options.Headless);
        Assert.Null(options.MediaPath);
        Assert.Equal(0, options.Cycles);
    }

    [Fact]
    public void MediaPath_SelectsHeadlessWithTheDefaultRunLength()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["game.d64"]);

        Assert.True(options.Headless);
        Assert.Equal("game.d64", options.MediaPath);
        Assert.Equal(0, options.Cycles);
    }

    [Fact]
    public void Cycles_ImpliesHeadless()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["--cycles", "19656000"]);

        Assert.True(options.Headless);
        Assert.Equal(19_656_000L, options.Cycles);
        Assert.Null(options.MediaPath);
    }

    [Fact]
    public void Cycles_CanBeAttachedToTheOption()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["--cycles=1000"]);

        Assert.Equal(1000L, options.Cycles);
    }

    [Fact]
    public void Help_DoesNotNeedARunLimit()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["--help"]);

        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void Screenshot_ImpliesHeadless()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--cycles", "1000", "--screenshot", "ready.bmp"]);

        Assert.True(options.Headless);
        Assert.Equal("ready.bmp", options.ScreenshotPath);
    }

    [Fact]
    public void FrameDump_UsesTheDefaultInterval()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--cycles", "1000", "--frames", "captures"]);

        Assert.Equal("captures", options.FramesDirectory);
        Assert.Equal(25, options.FrameInterval);
    }

    [Fact]
    public void Every_SetsTheFrameInterval()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--cycles", "1000", "--frames", "captures", "--every", "50"]);

        Assert.Equal(50, options.FrameInterval);
    }

    [Fact]
    public void ExpectFrame_ImpliesHeadless()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--cycles", "1000", "--expect-frame", "ready.bmp"]);

        Assert.True(options.Headless);
        Assert.Equal("ready.bmp", options.ExpectFramePath);
    }

    [Fact]
    public void ExpectFrame_RequiresAValue()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--cycles", "1000", "--expect-frame"]));
    }

    [Fact]
    public void Tolerance_RequiresExpectFrame()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--cycles", "1000", "--tolerance", "8"]));
    }

    [Fact]
    public void Tolerance_IsParsedInRange()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--cycles", "1000", "--expect-frame", "ready.bmp", "--tolerance", "255"]);

        Assert.Equal(255, options.PixelTolerance);
    }

    [Theory]
    [InlineData("256")]
    [InlineData("-1")]
    public void Tolerance_RejectsOutOfRange(string value)
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--cycles", "1000", "--expect-frame", "ready.bmp", "--tolerance", value]));
    }

    [Fact]
    public void Suite_ImpliesHeadless()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--suite", "regression.json"]);

        Assert.True(options.Headless);
        Assert.Equal("regression.json", options.SuitePath);
    }

    [Fact]
    public void Suite_RequiresAValue()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--suite"]));
    }

    [Fact]
    public void Suite_RejectsOtherWork()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--suite", "regression.json", "--cycles", "1000"]));
    }

    [Fact]
    public void RecordMissing_RequiresSuite()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--record-missing"]));
    }

    [Fact]
    public void RecordMissing_IsParsedWithSuite()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--suite", "regression.json", "--record-missing"]);

        Assert.True(options.Headless);
        Assert.True(options.RecordMissingReferences);
        Assert.Equal("regression.json", options.SuitePath);
    }

    [Fact]
    public void RecordAll_RequiresSuite()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--record-all"]));
    }

    [Fact]
    public void RecordAll_IsParsedWithSuite()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--suite", "regression.json", "--record-all"]);

        Assert.True(options.Headless);
        Assert.True(options.RecordAllReferences);
        Assert.False(options.RecordMissingReferences);
        Assert.Equal("regression.json", options.SuitePath);
    }

    [Fact]
    public void RecordModes_AreMutuallyExclusive()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                [
                    "--suite",
                    "regression.json",
                    "--record-missing",
                    "--record-all"
                ]));
    }

    [Fact]
    public void Case_RequiresSuite()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--case", "boot"]));
    }

    [Fact]
    public void Case_IsParsedAndRepeatable()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            [
                "--suite",
                "regression.json",
                "--case",
                "boot",
                "--case",
                "game"
            ]);

        Assert.True(options.Headless);
        Assert.Equal(2, options.SelectedSuiteCases.Count);
        Assert.Equal("boot", options.SelectedSuiteCases[0]);
        Assert.Equal("game", options.SelectedSuiteCases[1]);
    }

    [Fact]
    public void ListCases_RequiresSuite()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--list-cases"]));
    }

    [Fact]
    public void ListCases_IsParsedWithSuiteAndCase()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            [
                "--suite",
                "regression.json",
                "--list-cases",
                "--case",
                "boot"
            ]);

        Assert.True(options.ListCases);
        Assert.Equal("regression.json", options.SuitePath);
        Assert.Equal("boot", options.SelectedSuiteCases[0]);
    }

    [Fact]
    public void ListCases_RejectsRecordingOptions()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                [
                    "--suite",
                    "regression.json",
                    "--list-cases",
                    "--record-missing"
                ]));

        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                [
                    "--suite",
                    "regression.json",
                    "--list-cases",
                    "--record-all"
                ]));
    }

    [Fact]
    public void Json_RequiresListCases()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--json"]));

        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--suite", "regression.json", "--json"]));
    }

    [Fact]
    public void Json_IsParsedWithListCases()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            [
                "--suite",
                "regression.json",
                "--list-cases",
                "--json",
                "--case",
                "boot"
            ]);

        Assert.True(options.ListCases);
        Assert.True(options.ListCasesJson);
        Assert.Equal("boot", options.SelectedSuiteCases[0]);
    }

    [Fact]
    public void RelativePaths_RequiresJson()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--relative-paths"]));

        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--suite", "regression.json", "--list-cases", "--relative-paths"]));
    }

    [Fact]
    public void RelativePaths_IsParsedWithJson()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            [
                "--suite",
                "regression.json",
                "--list-cases",
                "--json",
                "--relative-paths"
            ]);

        Assert.True(options.ListCases);
        Assert.True(options.ListCasesJson);
        Assert.True(options.RelativePaths);
    }

    [Fact]
    public void CheckFiles_RequiresJson()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--check-files"]));

        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--suite", "regression.json", "--list-cases", "--check-files"]));
    }

    [Fact]
    public void CheckFiles_IsParsedWithJson()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            [
                "--suite",
                "regression.json",
                "--list-cases",
                "--json",
                "--check-files"
            ]);

        Assert.True(options.ListCases);
        Assert.True(options.ListCasesJson);
        Assert.True(options.CheckFiles);
    }

    [Fact]
    public void VideoStandard_DefaultsToPal()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--headless", "--cycles", "1000"]);

        Assert.Equal(
            C64Emulator.Video.VideoStandard.Pal,
            options.VideoStandard);
    }

    [Theory]
    [InlineData("pal")]
    [InlineData("PAL")]
    [InlineData("ntsc")]
    [InlineData("NTSC")]
    public void VideoStandard_IsParsedForHeadlessRuns(string value)
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--headless", "--cycles", "1000", "--video-standard", value]);
        var expected = value.Equals(
            "ntsc",
            StringComparison.OrdinalIgnoreCase)
            ? C64Emulator.Video.VideoStandard.Ntsc
            : C64Emulator.Video.VideoStandard.Pal;

        Assert.Equal(expected, options.VideoStandard);
    }

    [Fact]
    public void VideoStandard_AcceptsTheEqualsForm()
    {
        CommandLineOptions options = CommandLineOptions.Parse(
            ["--headless", "--cycles", "1000", "--video-standard=ntsc"]);

        Assert.Equal(
            C64Emulator.Video.VideoStandard.Ntsc,
            options.VideoStandard);
    }

    [Fact]
    public void VideoStandard_RejectsAnUnknownValue()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                [
                    "--headless",
                    "--cycles",
                    "1000",
                    "--video-standard",
                    "secam"
                ]));
    }

    [Fact]
    public void VideoStandard_RequiresAValue()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--headless", "--cycles", "1000", "--video-standard"]));
    }

    [Fact]
    public void VideoStandard_CannotBeCombinedWithSuite()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--suite", "regression.json", "--video-standard", "ntsc"]));
    }

    [Fact]
    public void Every_RequiresFrames()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--cycles", "1000", "--every", "10"]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("x")]
    public void InvalidFrameInterval_IsRejected(string value)
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(
                ["--cycles", "1000", "--frames", "captures", "--every", value]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("1.5")]
    public void InvalidCycleCount_IsRejected(string value)
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--cycles", value]));
    }

    [Fact]
    public void MissingCycleValue_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--cycles"]));
    }

    [Fact]
    public void UnknownOption_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--warp"]));
    }

    [Fact]
    public void TwoMediaFiles_AreRejected()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["one.d64", "two.d64"]));
    }

    [Fact]
    public void HeadlessWithoutWork_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => CommandLineOptions.Parse(["--headless"]));
    }
}
