using System;
using C64Emulator.Core;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class VicTests : IDisposable
{
    private readonly TestSystem system;

    public VicTests()
    {
        system = new TestSystem(withPeripherals: true);
    }

    public void Dispose()
    {
        system.Dispose();
    }

    [Fact]
    public void RasterAndLightPenRegisters_ReadBackCorrectly()
    {
        Vic vic = system.Vic!;

        vic.SetRegister(0x12, 0x42);
        vic.Tick(63);

        Assert.Equal(0x01, vic.GetRegister(0x12));
        Assert.Equal(0x00, vic.GetRegister(0x13));
        Assert.Equal(0x00, vic.GetRegister(0x14));
    }

    [Fact]
    public void RasterCompare_SetsFlagAndAssertsIrq()
    {
        Vic vic = system.Vic!;

        vic.SetRegister(0x12, 0x02);
        vic.SetRegister(0x1A, 0x01);

        // Line 0 is 62 cycles, line 1 is 63, and the IRQ flag is raised
        // on cycle 1 of line 2 rather than at the line boundary.
        vic.Tick(63 * 2);
        Assert.False((vic.GetRegister(0x19) & 0x01) != 0);

        vic.Tick(1);
        Assert.True((vic.GetRegister(0x19) & 0x01) != 0);
        Assert.True(system.Interrupts.IsIrqAsserted);

        vic.SetRegister(0x19, 0x01);

        Assert.False(system.Interrupts.IsIrqAsserted);
    }

    [Fact]
    public void PalRasterCounter_UsesShortFirstLineAndLongLastLine()
    {
        Vic vic = system.Vic!;

        Assert.Equal(311 & 0xFF, vic.GetRegister(0x12));
        Assert.Equal(0x80, vic.GetRegister(0x11) & 0x80);

        vic.Tick(1);
        Assert.Equal(0, vic.GetRegister(0x12));

        vic.Tick(vic.FrameCycles - 1 - 64);
        Assert.Equal(311, vic.RasterLine);
        Assert.Equal(0, vic.RasterCycle);

        vic.Tick(63);
        Assert.Equal(311, vic.RasterLine);
        Assert.Equal(63, vic.RasterCycle);

        Assert.True(vic.Tick(1));
        Assert.Equal(0, vic.RasterLine);
        Assert.Equal(0, vic.RasterCycle);
    }

    [Fact]
    public void NtscRasterCounter_UsesSixtyFiveCycleLinesAnd263Rasters()
    {
        Vic vic = system.Vic!;
        vic.SetVideoStandard(VideoStandard.Ntsc);

        Assert.Equal(VideoStandard.Ntsc, vic.VideoStandard);
        Assert.Equal(263, vic.RasterLineCount);
        Assert.Equal(17095, vic.FrameCycles);
        Assert.Equal(235, vic.VisibleHeight);

        vic.Tick(vic.FrameCycles - 1);

        Assert.Equal(262, vic.RasterLine);
        Assert.Equal(64, vic.RasterCycle);

        Assert.True(vic.Tick(1));
        Assert.Equal(0, vic.RasterLine);
        Assert.Equal(0, vic.RasterCycle);
    }

    [Fact]
    public void NtscFrame_IsWiderAndShorterThanPal()
    {
        Vic vic = system.Vic!;

        Assert.Equal(403, vic.VisibleWidth);
        Assert.Equal(284, vic.VisibleHeight);
        Assert.Equal((403 * 284 + 1) / 2, vic.CapturePackedScreen().Length);

        vic.SetVideoStandard(VideoStandard.Ntsc);

        Assert.Equal(418, vic.VisibleWidth);
        Assert.Equal(235, vic.VisibleHeight);
        Assert.Equal((418 * 235 + 1) / 2, vic.CapturePackedScreen().Length);
    }

    [Fact]
    public void VideoTimings_PresentFourByThreeFrames()
    {
        Assert.Equal(0.9365, VideoTiming.Pal.PixelAspectRatio);
        Assert.Equal(0.75, VideoTiming.Ntsc.PixelAspectRatio);
        Assert.Equal(377, VideoTiming.Pal.PresentationWidth);
        Assert.Equal(314, VideoTiming.Ntsc.PresentationWidth);

        var palAspect =
            (double)VideoTiming.Pal.PresentationWidth /
            VideoTiming.Pal.VisibleLines;
        var ntscAspect =
            (double)VideoTiming.Ntsc.PresentationWidth /
            VideoTiming.Ntsc.VisibleLines;

        Assert.InRange(palAspect, (4.0 / 3) - 0.01, (4.0 / 3) + 0.01);
        Assert.InRange(ntscAspect, (4.0 / 3) - 0.01, (4.0 / 3) + 0.01);
    }

    [Fact]
    public void NtscVisibleFrame_WrapsPastRasterZero()
    {
        VideoTiming timing = VideoTiming.For(VideoStandard.Ntsc);

        Assert.True(timing.TryGetVisibleRow(41, out var top));
        Assert.Equal(0, top);
        Assert.True(timing.TryGetVisibleRow(262, out var beforeWrap));
        Assert.Equal(221, beforeWrap);
        Assert.True(timing.TryGetVisibleRow(0, out var wrapped));
        Assert.Equal(222, wrapped);
        Assert.True(timing.TryGetVisibleRow(12, out var bottom));
        Assert.Equal(234, bottom);
        Assert.False(timing.TryGetVisibleRow(13, out _));
        Assert.False(timing.TryGetVisibleRow(40, out _));
    }

    [Fact]
    public void DisplayWindow_UsesTheActiveStandardBorderGeometry()
    {
        Vic vic = system.Vic!;

        Assert.Equal(44, vic.DisplayLeft);
        Assert.Equal(42, vic.DisplayTop);
        Assert.Equal(320, vic.DisplayWidth);
        Assert.Equal(200, vic.DisplayHeight);

        vic.SetVideoStandard(VideoStandard.Ntsc);

        Assert.Equal(44, vic.DisplayLeft);
        Assert.Equal(10, vic.DisplayTop);
        Assert.Equal(320, vic.DisplayWidth);
        Assert.Equal(200, vic.DisplayHeight);
    }

    [Fact]
    public void NtscDisplay_StartsAtRasterFortyOneAboveTheBottomBorder()
    {
        Vic vic = system.Vic!;
        vic.SetVideoStandard(VideoStandard.Ntsc);
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);

        var screen = vic.CapturePackedScreen();

        // The NTSC visible frame starts at raster 41 and wraps through
        // raster 12, so the first display line ($33 = 51) is at row 10.
        Assert.Equal(418, vic.VisibleWidth);
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 41));
        Assert.Equal(0, PaletteIndexAt(screen, 44, 52 - 41));
    }

    [Fact]
    public void NtscBadlineShiftsBaAndAecOneCycleLater()
    {
        Vic vic = system.Vic!;
        vic.SetVideoStandard(VideoStandard.Ntsc);
        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 12);

        // The 6567R8 inserts one cycle at the start of each line, so the
        // BA lead-in runs on 13-15 and the c-accesses on 16-55.
        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);

        vic.Tick(1);
        Assert.Equal(13, vic.RasterCycle);
        Assert.True(vic.BaLow);
        Assert.False(vic.AecLow);

        vic.Tick(3);
        Assert.Equal(16, vic.RasterCycle);
        Assert.True(vic.AecLow);
        Assert.False(vic.IsCpuAccessAllowed(isWrite: true));

        vic.Tick(40);
        Assert.Equal(56, vic.RasterCycle);
        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);
    }

    [Fact]
    public void NtscSpriteFetchSlotsShiftOneCycleLater()
    {
        Vic vic = system.Vic!;
        vic.SetVideoStandard(VideoStandard.Ntsc);
        vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        vic.SetRegister(0x15, 0x01); // sprite 0
        vic.SetRegister(0x01, 50);   // starts on raster 51
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 54);

        Assert.False(vic.BaLow);

        vic.Tick(1);
        Assert.Equal(55, vic.RasterCycle);
        Assert.True(vic.BaLow);
        Assert.False(vic.AecLow);

        vic.Tick(3);
        Assert.Equal(58, vic.RasterCycle);
        Assert.True(vic.AecLow);

        vic.Tick(2);
        Assert.Equal(60, vic.RasterCycle);
        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);
    }

    [Fact]
    public void VerticalScroll_ShiftsDisplayWithinTwentyFiveRowWindow()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);

        var normal = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(normal, 44, 51 - 9));
        Assert.Equal(0, PaletteIndexAt(normal, 44, 52 - 9));

        vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        vic.Tick(vic.FrameCycles);

        var scrolled = vic.CapturePackedScreen();
        Assert.Equal(0, PaletteIndexAt(scrolled, 44, 51 - 9));
        Assert.Equal(1, PaletteIndexAt(scrolled, 44, 56 - 9));
    }

    [Fact]
    public void VerticalScroll_TwentyFourRowsRestAtTopOfApertureWithYScrollSeven()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x17); // DEN, 24 rows, YSCROLL 7
        vic.Tick(vic.FrameCycles);

        var screen = vic.CapturePackedScreen();

        // With RSEL clear the border opens at line 55, exactly where row 0
        // starts when YSCROLL is 7. Boulder Dash rests on this value.
        Assert.Equal(1, PaletteIndexAt(screen, 44, 55 - 9));
        Assert.Equal(0, PaletteIndexAt(screen, 44, 56 - 9));
        Assert.Equal(1, PaletteIndexAt(screen, 44, 63 - 9));
    }

    [Fact]
    public void TwentyFourRowFineScrollExposesRow24AtTheBottom()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // RSEL clear, YSCROLL 1: the 24-row aperture ends at line 246,
        // but the VIC has already fetched matrix row 24. Its first six
        // scanlines are visible above the bottom border.
        vic.SetRegister(0x11, 0x11);
        vic.Tick(vic.FrameCycles);

        var screen = vic.CapturePackedScreen();

        Assert.Equal(1, PaletteIndexAt(screen, 44, 241 - 9));
    }

    [Fact]
    public void ThirtyEightColumnModeScrollsWithoutShrinkingTheAperture()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        for (var row = 0; row < 8; row++)
            system.Memory.Poke((ushort)(0xC800 + row), 0xFF);

        vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        vic.SetRegister(0x16, 0x07); // 38 columns, XSCROLL 7
        vic.Tick(vic.FrameCycles);

        var screen = vic.CapturePackedScreen();

        // The left border still opens at pixel 51. XSCROLL 7 moves the
        // matrix underneath it by seven pixels instead of narrowing the
        // aperture by seven pixels.
        Assert.Equal(1, PaletteIndexAt(screen, 51, 51 - 9));
        Assert.Equal(0, PaletteIndexAt(screen, 50, 51 - 9));
    }

    [Fact]
    public void ModeSwitchRefreshesThePixelMemoryBaseWithoutD018Rewrite()
    {
        Vic vic = system.Vic!;

        // $D018=$12 selects matrix $C400, character data $C800 and
        // bitmap data $C000. The game changes $D011 between character
        // and bitmap mode without rewriting $D018.
        vic.SetRegister(0x18, 0x12);
        system.Memory.Poke(0xC800, 0xFF);
        system.ColorRam!.Write(0, 1);

        vic.SetRegister(0x21, 0x06);
        vic.SetRegister(0x11, 0x1B); // character mode, DEN, YSCROLL 3
        vic.SetRegister(0x16, 0x18); // 40 columns, multicolor
        vic.Tick(vic.FrameCycles);

        var character = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(character, 44, 51 - 9));

        // Bitmap row 0 contains all four multicolor pairs. The screen
        // color byte selects red and green; Color RAM selects yellow.
        system.Memory.Poke(0xC000, 0x1B);
        system.Memory.Poke(0xC400, 0x25);
        system.ColorRam.Write(0, 7);

        vic.SetRegister(0x11, 0x3B); // bitmap mode
        vic.Tick(vic.FrameCycles);

        var bitmap = vic.CapturePackedScreen();
        Assert.Equal(6, PaletteIndexAt(bitmap, 44, 51 - 9));
        Assert.Equal(2, PaletteIndexAt(bitmap, 46, 51 - 9));
        Assert.Equal(5, PaletteIndexAt(bitmap, 48, 51 - 9));
        Assert.Equal(7, PaletteIndexAt(bitmap, 50, 51 - 9));

        // Returning to character mode must restore the character base.
        system.Memory.Poke(0xC400, 0x00);
        system.ColorRam.Write(0, 1);
        vic.SetRegister(0x11, 0x1B);
        vic.Tick(vic.FrameCycles);

        var restored = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(restored, 44, 51 - 9));
    }

    [Fact]
    public void MulticolorCharacterIgnoresTheMulticolorFlagBitInTheColorIndex()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // Red (2) with the multicolor flag (0x08) set. The foreground
        // must be red, not palette index 10 (light red).
        for (var i = 0; i < 1000; i++)
            system.ColorRam!.Write((ushort)i, 0x0A);

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.SetRegister(0x16, 0x18); // 40 columns, multicolor
        vic.Tick(vic.FrameCycles);

        var screen = vic.CapturePackedScreen();

        Assert.Equal(2, PaletteIndexAt(screen, 44, 51 - 9));
    }

    [Fact]
    public void VerticalScroll_TwentyFourRowSequenceMovesTwoPixelsPerStep()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // Boulder Dash's upward sequence: $D011 values 7, 5, 3, 1 with
        // RSEL clear. Each step must move the matrix up exactly two pixels.
        byte[] values = [0x17, 0x15, 0x13, 0x11];
        int[] secondRowTop = [63, 61, 59, 57];

        for (var i = 0; i < values.Length; i++)
        {
            vic.SetRegister(0x11, values[i]);
            vic.Tick(vic.FrameCycles);

            var screen = vic.CapturePackedScreen();
            Assert.Equal(1, PaletteIndexAt(screen, 44, secondRowTop[i] - 9));

            if (i > 0)
            {
                Assert.Equal(
                    0,
                    PaletteIndexAt(screen, 44, secondRowTop[i - 1] - 9));
            }
        }
    }

    [Fact]
    public void SplitRasterBadlineKeepsTheCurrentCharacterRow()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // Rows 1 and 2 have different colours, so the rendered pixel
        // identifies which matrix row the VIC fetched.
        system.ColorRam!.Write(40, 2);
        system.ColorRam.Write(80, 3);

        TickToCycle(vic, 48, 0);
        vic.SetRegister(0x11, 0x1B); // 25 rows, YSCROLL 3
        TickToCycle(vic, 59, 16);

        // A mid-row split changes YSCROLL to 7, which makes line 63 a
        // badline only four scanlines after row 1 started. It must restart
        // row 1, not skip ahead to row 2 as a per-badline row counter did.
        TickToCycle(vic, 61, 26);
        vic.SetRegister(0x11, 0x17); // 24 rows, YSCROLL 7
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(2, PaletteIndexAt(screen, 44, 63 - 9));
    }

    [Fact]
    public void ScreenMatrixWritesAfterABadlineWaitForTheNextBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 16);

        // Column 0 has already had both its matrix and graphics fetch.
        // Replacing its character must not change the current row.
        system.Memory.Poke(0xC400, 0x01);
        system.Memory.Poke(0xC428, 0x01);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));

        // The next badline fetches row 1 from the updated matrix.
        Assert.Equal(0, PaletteIndexAt(screen, 44, 59 - 9));
    }

    [Fact]
    public void ColorRamWritesAfterABadlineWaitForTheNextBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 16);

        system.ColorRam!.Write(0, 2);
        system.ColorRam.Write(40, 2);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(2, PaletteIndexAt(screen, 44, 59 - 9));
    }

    [Fact]
    public void MatrixBaseChangesTakeEffectAtTheNextBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // $D018=$12 uses $C400 for the matrix. The alternate matrix at
        // $C000 has a different character in row 1.
        system.Memory.Poke(0xC028, 0x01);

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 16);

        vic.SetRegister(0x18, 0x02); // matrix $C000, characters $C800
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(0, PaletteIndexAt(screen, 44, 59 - 9));
    }

    [Fact]
    public void MatrixFetchIsStaggeredAcrossTheBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 15);

        // Cell 0 was fetched on cycle 14; cell 1 is fetched on cycle 15.
        // Changing cell 1 now affects the current row, while cell 0 keeps
        // the character it already latched.
        system.Memory.Poke(0xC401, 0x01);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(0, PaletteIndexAt(screen, 52, 51 - 9));
    }

    [Fact]
    public void MatrixBaseChangeAffectsLaterCellsOnTheSameBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        // Alternate matrix at $C000 has character 1 in column 1.
        system.Memory.Poke(0xC001, 0x01);

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 15);

        // Column 0 was fetched from $C400. Column 1 is fetched during
        // this cycle, from the newly selected $C000 matrix.
        vic.SetRegister(0x18, 0x02);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(0, PaletteIndexAt(screen, 52, 51 - 9));
    }

    [Fact]
    public void ColorFetchIsStaggeredAcrossTheBadline()
    {
        Vic vic = system.Vic!;
        ConfigureCharacterPattern();

        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 15);

        system.ColorRam!.Write(1, 2);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();
        Assert.Equal(1, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(2, PaletteIndexAt(screen, 52, 51 - 9));
    }

    [Fact]
    public void BadlineAssertsBaThreeCyclesBeforeAec()
    {
        Vic vic = system.Vic!;
        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 11);

        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);

        vic.Tick(1);
        Assert.Equal(12, vic.RasterCycle);
        Assert.True(vic.BaLow);
        Assert.False(vic.AecLow);
        Assert.False(vic.IsCpuAccessAllowed(isWrite: false));
        Assert.True(vic.IsCpuAccessAllowed(isWrite: true));

        vic.Tick(3);
        Assert.Equal(15, vic.RasterCycle);
        Assert.True(vic.AecLow);
        Assert.False(vic.IsCpuAccessAllowed(isWrite: false));
        Assert.False(vic.IsCpuAccessAllowed(isWrite: true));

        vic.Tick(40);
        Assert.Equal(55, vic.RasterCycle);
        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);
    }

    [Fact]
    public void SpriteDmaAssertsBaBeforeItsFetchSlots()
    {
        Vic vic = system.Vic!;
        vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        vic.SetRegister(0x15, 0x01); // sprite 0
        vic.SetRegister(0x01, 50);   // starts on raster 51
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 53);

        Assert.False(vic.BaLow);

        vic.Tick(1);
        Assert.Equal(54, vic.RasterCycle);
        Assert.True(vic.BaLow);
        Assert.False(vic.AecLow);
        Assert.True(vic.IsCpuAccessAllowed(isWrite: true));
        Assert.False(vic.IsCpuAccessAllowed(isWrite: false));

        vic.Tick(3);
        Assert.Equal(57, vic.RasterCycle);
        Assert.True(vic.AecLow);
        Assert.False(vic.IsCpuAccessAllowed(isWrite: true));

        vic.Tick(2);
        Assert.Equal(59, vic.RasterCycle);
        Assert.False(vic.BaLow);
        Assert.False(vic.AecLow);
    }

    [Fact]
    public void SpriteBaLowWrapsIntoTailOfPreviousLine()
    {
        Vic vic = system.Vic!;
        vic.SetRegister(0x11, 0x18); // DEN, 25 rows, YSCROLL 0
        vic.SetRegister(0x15, 0x08); // sprite 3
        vic.SetRegister(0x07, 50);   // starts on raster 51
        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 50, 59);

        Assert.False(vic.BaLow);

        vic.Tick(1);
        Assert.Equal(60, vic.RasterCycle);
        Assert.True(vic.BaLow);
        Assert.False(vic.AecLow);

        vic.Tick(2);
        Assert.Equal(62, vic.RasterCycle);
        Assert.True(vic.BaLow);

        vic.Tick(1);
        Assert.Equal(51, vic.RasterLine);
        Assert.Equal(0, vic.RasterCycle);
        Assert.True(vic.AecLow);
        Assert.False(vic.IsCpuAccessAllowed(isWrite: true));
    }

    [Fact]
    public void BadlineStealsFortyBusCycles()
    {
        Vic vic = system.Vic!;
        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.Tick(vic.FrameCycles);
        vic.TakeStolenCycles();

        TickToCycle(vic, 51, 0);
        vic.Tick(63);

        Assert.Equal(40, vic.TakeStolenCycles());
    }

    [Fact]
    public void SpriteDmaStealsTwoCyclesPerSprite()
    {
        Vic vic = system.Vic!;
        vic.SetRegister(0x11, 0x1B); // DEN, 25 rows, YSCROLL 3
        vic.SetRegister(0x15, 0x03); // sprites 0 and 1
        vic.SetRegister(0x01, 50);   // sprite 0 starts on raster 51
        vic.SetRegister(0x03, 50);   // sprite 1 starts on raster 51
        vic.Tick(vic.FrameCycles);

        // DMA starts on raster line 50, one line before the sprites are
        // visible, so clear the counter at the start of line 51 rather
        // than before the journey there.
        TickToCycle(vic, 51, 0);
        vic.TakeStolenCycles();
        vic.Tick(63);

        // 40 badline cycles + 2 cycles for each of the two sprites.
        Assert.Equal(44, vic.TakeStolenCycles());
    }

    [Fact]
    public void PackedScreen_HasTwoPixelsPerByte()
    {
        Vic vic = system.Vic!;
        var packed = vic.CapturePackedScreen();

        Assert.Equal((vic.VisibleWidth * vic.VisibleHeight + 1) / 2, packed.Length);
    }

    [Fact]
    public void MulticolorSpriteUsesTwentyFourOutputPixels()
    {
        Vic vic = system.Vic!;

        // $D018=$00: matrix and character area start at $C000. The sprite
        // pointer at $C3F8 selects sprite data at $C040.
        vic.SetRegister(0x18, 0x00);
        system.Memory.Poke(0xC3F8, 0x01);

        // Every two-bit pair is 01, so the entire sprite is color 1.
        for (var i = 0; i < 63; i++)
            system.Memory.Poke((ushort)(0xC040 + i), 0x55);

        vic.SetRegister(0x15, 0x01); // enable sprite 0
        vic.SetRegister(0x1C, 0x01); // sprite 0 is multicolor
        vic.SetRegister(0x27, 0x02); // sprite color
        vic.SetRegister(0x25, 0x05); // multicolor color 1
        vic.SetRegister(0x21, 0x00); // black background
        vic.SetRegister(0x00, 100);  // sprite X
        vic.SetRegister(0x01, 100);  // sprite Y

        vic.Tick(vic.FrameCycles);
        var screen = vic.CapturePackedScreen();
        var colored = 0;

        for (var x = 120; x < 168; x++)
        {
            if (PaletteIndexAt(screen, x, 101) == 5)
                colored++;
        }

        // Multicolour halves the horizontal resolution instead of
        // widening the sprite: 12 bit pairs, each two pixels wide, still
        // fill only the 24 pixels a sprite occupies. Treating it as a
        // width multiplier made DOTC's castles and raiders twice as wide.
        Assert.Equal(24, colored);
    }

    [Fact]
    public void DoubleWidthSpriteUsesFortyEightOutputPixels()
    {
        Vic vic = system.Vic!;

        vic.SetRegister(0x18, 0x00);
        system.Memory.Poke(0xC3F8, 0x01);

        for (var i = 0; i < 63; i++)
            system.Memory.Poke((ushort)(0xC040 + i), 0xFF);

        vic.SetRegister(0x15, 0x01); // enable sprite 0
        vic.SetRegister(0x1D, 0x01); // sprite 0 is double width
        vic.SetRegister(0x27, 0x02); // sprite color
        vic.SetRegister(0x21, 0x00); // black background
        vic.SetRegister(0x00, 100);  // sprite X
        vic.SetRegister(0x01, 100);  // sprite Y

        vic.Tick(vic.FrameCycles);
        var screen = vic.CapturePackedScreen();
        var colored = 0;

        for (var x = 120; x < 168; x++)
        {
            if (PaletteIndexAt(screen, x, 101) == 2)
                colored++;
        }

        Assert.Equal(48, colored);
    }

    [Fact]
    public void MulticolorDoubleWidthSpriteStaysFortyEightPixels()
    {
        Vic vic = system.Vic!;

        vic.SetRegister(0x18, 0x00);
        system.Memory.Poke(0xC3F8, 0x01);

        for (var i = 0; i < 63; i++)
            system.Memory.Poke((ushort)(0xC040 + i), 0x55);

        vic.SetRegister(0x15, 0x01); // enable sprite 0
        vic.SetRegister(0x1C, 0x01); // multicolour
        vic.SetRegister(0x1D, 0x01); // and double width
        vic.SetRegister(0x25, 0x05); // multicolour color 1
        vic.SetRegister(0x21, 0x00);
        vic.SetRegister(0x00, 100);
        vic.SetRegister(0x01, 100);

        vic.Tick(vic.FrameCycles);
        var screen = vic.CapturePackedScreen();
        var colored = 0;

        for (var x = 120; x < 168; x++)
        {
            if (PaletteIndexAt(screen, x, 101) == 5)
                colored++;
        }

        // 12 bit pairs, each four pixels wide once expansion is applied.
        Assert.Equal(48, colored);
    }

    [Fact]
    public void ForcedBadlineAtCycleFourteenAdvancesTheBitmapRow()
    {
        Vic vic = system.Vic!;

        // $D018=$10: matrix at $C400, bitmap at $C000.
        vic.SetRegister(0x18, 0x10);
        vic.SetRegister(0x11, 0x3B); // DEN, bitmap, YSCROLL 3
        vic.SetRegister(0x16, 0xD8); // 40 columns, multicolor bitmap
        vic.SetRegister(0x21, 0x00); // background
        system.Memory.Poke(0xC400, 0x2A); // colors 2 and 10
        system.ColorRam!.Write(0, 3);

        // The first cell has a distinct pattern on each bitmap row.
        byte[] rows = [0x00, 0x55, 0xAA, 0xFF, 0x55, 0xAA, 0xFF, 0x00];

        for (var row = 0; row < rows.Length; row++)
            system.Memory.Poke((ushort)(0xC000 + row), rows[row]);

        vic.Tick(vic.FrameCycles);
        TickToCycle(vic, 51, 62);

        // FLI routines write YSCROLL on cycle 14 to force a partial
        // badline for the current line. The source row stays at zero
        // while the row counter advances 0, 1, 2, 3, 4.
        byte[] scroll = [0x3C, 0x3D, 0x3E, 0x3F];

        for (var i = 0; i < scroll.Length; i++)
        {
            TickToCycle(vic, 52 + i, 14);
            vic.SetRegister(0x11, scroll[i]);
            TickToCycle(vic, 52 + i, 62);
        }

        vic.SetRegister(0x11, 0x38);
        vic.Tick(1);
        TickFrame(vic);

        var screen = vic.CapturePackedScreen();

        Assert.Equal(0, PaletteIndexAt(screen, 44, 51 - 9));
        Assert.Equal(2, PaletteIndexAt(screen, 44, 52 - 9));
        Assert.Equal(10, PaletteIndexAt(screen, 44, 53 - 9));
        Assert.Equal(3, PaletteIndexAt(screen, 44, 54 - 9));
        Assert.Equal(2, PaletteIndexAt(screen, 44, 55 - 9));
    }

    private void ConfigureCharacterPattern()
    {
        Vic vic = system.Vic!;

        // VIC bank 0 is $C000 at power-on. $D018=$12 puts the matrix at
        // $C400 and character data at $C800.
        vic.SetRegister(0x18, 0x12);
        system.Memory.Poke(0xC800, 0xFF);

        for (var i = 1; i < 8; i++)
            system.Memory.Poke((ushort)(0xC800 + i), 0);

        for (var i = 0; i < 1000; i++)
            system.ColorRam!.Write((ushort)i, 1);
    }

    private static void TickToCycle(Vic vic, int line, int cycle)
    {
        for (var i = 0; i < vic.FrameCycles; i++)
        {
            if (vic.RasterLine == line && vic.RasterCycle == cycle)
                return;

            vic.Tick(1);
        }

        Assert.Fail(
            "VIC did not reach raster line " + line + " cycle " + cycle + ".");
    }

    private static void TickFrame(Vic vic)
    {
        for (var i = 0; i < vic.FrameCycles; i++)
        {
            if (vic.Tick(1))
                return;
        }

        Assert.Fail("VIC did not complete a frame.");
    }

    private int PaletteIndexAt(byte[] packed, int x, int y)
    {
        var pixel = y * system.Vic!.VisibleWidth + x;
        var value = packed[pixel / 2];

        return (pixel & 1) == 0 ? value >> 4 : value & 0x0F;
    }
}
