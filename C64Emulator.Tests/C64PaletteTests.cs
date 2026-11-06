using System;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class C64PaletteTests
{
    [Fact]
    public void PalettesUseDifferentNtscColours()
    {
        Assert.Equal(new RgbColor(0xBE, 0x35, 0x35), C64Palette.Pal.Get(2));
        Assert.Equal(new RgbColor(0x7C, 0x35, 0x2B), C64Palette.Ntsc.Get(2));
        Assert.NotEqual(C64Palette.Pal.Get(2), C64Palette.Ntsc.Get(2));

        Assert.Same(C64Palette.Pal, C64Palette.For(VideoStandard.Pal));
        Assert.Same(C64Palette.Ntsc, C64Palette.For(VideoStandard.Ntsc));
    }

    [Fact]
    public void PaletteIndicesRoundTripForBothStandards()
    {
        foreach (var standard in new[] { VideoStandard.Pal, VideoStandard.Ntsc })
        {
            C64Palette palette = C64Palette.For(standard);

            for (var i = 0; i < 16; i++)
                Assert.Equal(i, palette.ToIndex(palette.Get(i)));
        }
    }

    [Fact]
    public void VicRendersTheBorderWithTheActiveStandardPalette()
    {
        using var system = new TestSystem(withPeripherals: true);
        Vic vic = system.Vic!;

        vic.SetRegister(0x20, 2);
        vic.Tick(vic.FrameCycles);

        var palScreen = new RgbColor[vic.VisibleWidth, vic.VisibleHeight];
        vic.CopyScreenTo(palScreen);

        Assert.Equal(C64Palette.Pal.Get(2), palScreen[0, 0]);

        vic.SetVideoStandard(VideoStandard.Ntsc);
        vic.SetRegister(0x20, 2);
        vic.Tick(vic.FrameCycles);

        var ntscScreen = new RgbColor[vic.VisibleWidth, vic.VisibleHeight];
        vic.CopyScreenTo(ntscScreen);

        Assert.Equal(C64Palette.Ntsc.Get(2), ntscScreen[0, 0]);
    }

    [Fact]
    public void PackedScreensUseTheActiveStandardPalette()
    {
        using var system = new TestSystem(withPeripherals: true);
        Vic vic = system.Vic!;
        vic.SetVideoStandard(VideoStandard.Ntsc);

        var packed = new byte[(vic.VisibleWidth * vic.VisibleHeight + 1) / 2];
        Array.Fill(packed, (byte)0x22);
        vic.SetPackedScreen(packed);

        var screen = new RgbColor[vic.VisibleWidth, vic.VisibleHeight];
        vic.CopyScreenTo(screen);

        Assert.Equal(C64Palette.Ntsc.Get(2), screen[0, 0]);
        Assert.Equal(C64Palette.Ntsc.Get(2), screen[1, 0]);
        Assert.Equal(0x22, vic.CapturePackedScreen()[0]);
    }
}
