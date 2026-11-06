using System;
using System.Diagnostics;

namespace C64Emulator.Video;

/// <summary>
/// The VIC-II video systems the emulator models. PAL is the 6569 with 312
/// raster lines of 63 cycles; NTSC is the 6567R8 with 263 lines of 65.
/// </summary>
internal enum VideoStandard
{
    Pal = 0,
    Ntsc = 1
}

/// <summary>
/// Timing and geometry for one video standard. The VIC draws with the raster
/// numbers; the machine uses the clock and the frame length for host pacing
/// and for the SID, CIA time-of-day clock and 1541 speed ratio. The VIC's
/// pixels are not square, so the host squeezes the visible frame to its 4:3
/// presentation width.
/// </summary>
internal readonly struct VideoTiming
{
    private VideoTiming(
        VideoStandard standard,
        int clockHz,
        int cyclesPerLine,
        int rasterLines,
        int firstLineCycles,
        int lastLineCycles,
        int firstVisibleLine,
        int visibleLines,
        int visibleWidth,
        double pixelAspectRatio,
        int busCycleOffset)
    {
        Standard = standard;
        ClockHz = clockHz;
        CyclesPerLine = cyclesPerLine;
        RasterLines = rasterLines;
        FirstLineCycles = firstLineCycles;
        LastLineCycles = lastLineCycles;
        FirstVisibleLine = firstVisibleLine;
        VisibleLines = visibleLines;
        VisibleWidth = visibleWidth;
        PixelAspectRatio = pixelAspectRatio;
        BusCycleOffset = busCycleOffset;
    }

    /// <summary>PAL 6569: 985248 Hz, 63 cycles over 312 lines, 50.125 Hz.</summary>
    public static readonly VideoTiming Pal = new(
        VideoStandard.Pal,
        clockHz: 985_248,
        cyclesPerLine: 63,
        rasterLines: 312,
        firstLineCycles: 62,    // line 0 runs one cycle short
        lastLineCycles: 64,     // and the last line one cycle long
        firstVisibleLine: 9,
        visibleLines: 284,
        visibleWidth: 403,
        pixelAspectRatio: 0.9365,
        busCycleOffset: 0);

    /// <summary>NTSC 6567R8: 1022727 Hz, 65 cycles over 263 lines, 59.826 Hz.</summary>
    public static readonly VideoTiming Ntsc = new(
        VideoStandard.Ntsc,
        clockHz: 1_022_727,
        cyclesPerLine: 65,
        rasterLines: 263,
        firstLineCycles: 65,
        lastLineCycles: 65,
        // Visible from raster 41 through raster 12 of the next counter
        // cycle: 10 border lines above the $33-$FA display window and 25
        // below it, as on the 6567R8.
        firstVisibleLine: 41,
        visibleLines: 235,
        visibleWidth: 418,
        pixelAspectRatio: 0.75,
        busCycleOffset: 1);

    public VideoStandard Standard { get; }

    public int ClockHz { get; }

    public int CyclesPerLine { get; }

    public int RasterLines { get; }

    public int FirstLineCycles { get; }

    public int LastLineCycles { get; }

    /// <summary>First raster line in the visible picture, borders included.</summary>
    public int FirstVisibleLine { get; }

    /// <summary>Number of visible raster lines, borders included.</summary>
    public int VisibleLines { get; }

    /// <summary>Visible pixels per line, borders included.</summary>
    public int VisibleWidth { get; }

    /// <summary>Width of one VIC pixel relative to its height.</summary>
    public double PixelAspectRatio { get; }

    /// <summary>
    /// Width of the visible frame after correcting the VIC's non-square
    /// pixels. PAL and NTSC both present the full frame at 4:3.
    /// </summary>
    public int PresentationWidth =>
        (int)Math.Round(
            VisibleWidth * PixelAspectRatio,
            MidpointRounding.AwayFromZero);

    /// <summary>
    /// Extra bus cycles at the start of every raster line. The NTSC 6567R8
    /// inserts one cycle, so every access inside the line (c/g-accesses,
    /// sprite fetches and the BA lead-in) happens one cycle later than on
    /// the 6569. The pixel origin moves by the same amount, which keeps a
    /// fetched cell on the same screen pixels.
    /// </summary>
    public int BusCycleOffset { get; }

    /// <summary>
    /// Maps a raster line to its row in the visible frame. The NTSC frame
    /// starts at raster 41 and wraps through raster 12, so the bottom rows
    /// are drawn before the top rows of the same field.
    /// </summary>
    public bool TryGetVisibleRow(int rasterLine, out int row)
    {
        row = rasterLine - FirstVisibleLine;

        if (row < 0)
            row += RasterLines;

        return row >= 0 && row < VisibleLines;
    }

    public int FrameCycles =>
        (CyclesPerLine * RasterLines) -
        (CyclesPerLine - FirstLineCycles) -
        (CyclesPerLine - LastLineCycles);

    public double FrameRate => (double)ClockHz / FrameCycles;

    /// <summary>One frame in Stopwatch ticks, used for host pacing.</summary>
    public long FrameTicks => Stopwatch.Frequency * FrameCycles / ClockHz;

    public static VideoTiming For(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc ? Ntsc : Pal;
    }
}
