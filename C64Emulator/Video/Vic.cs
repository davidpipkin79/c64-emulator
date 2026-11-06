using System;
using System.Threading;
using C64Emulator.Core;
using C64Emulator.Devices;

namespace C64Emulator.Video;

internal enum VideoMode : byte
{
    SimpleCharacterMode = 0x00,
    MultiColorCharacterMode = 0x10,
    SimpleBitMapMode = 0x20,
    MultiColorBitMapMode = 0x30,
    ExtendedColorCharacterMode = 0x40
}

internal struct Sprite
{
    public int X;
    public int Y;
    public bool Enabled;
    public int Width;
    public int Height;
    public int PixelScale;
    public bool MulticolorMode;
    public bool DoubleWidth;
    public bool TopMost;
    public RgbColor SpriteColor;
}

internal struct Pixel
{
    public RgbColor PixelColor;
    public bool Background;
}

/// <summary>
/// A scanline-oriented VIC-II model. One CPU cycle advances the video
/// pipeline by eight pixels, matching the PAL VIC's 8 MHz dot clock. The
/// renderer receives a completed frame through a double buffer, so the CPU
/// thread never hands out an array it is still drawing into.
/// </summary>
internal sealed class Vic : IVideoBank
{
    public const int RegisterCount = 47;
    public const int PixelsPerCycle = 8;

    // The longest raster line the emulator models (NTSC 6567R8).
    public const int MaxLineCycles = 65;

    // PAL pixel origin: cycle 0 starts 96 pixels before the left edge of
    // the visible window. The NTSC 6567R8 inserts one extra cycle at the
    // start of each line, so its accesses and pixels sit eight pixels
    // further along; firstLinePixel carries that per-standard shift.
    private const int FirstLinePixel = 96;
    private const int FirstDisplayLine = 48;
    private const int FirstScreenLine = 51;
    private const int LastScreenLine = 250;
    private const int FirstScreenPixel = 44;
    private const int LastScreenPixel = 363;
    private const int SpriteXOrigin = 20;

    private readonly IVideoBus bus;
    private readonly ColorRam colorRam;
    private readonly InterruptLine interrupts;
    private readonly byte[] registers = new byte[RegisterCount];
    private readonly Sprite[] sprites = new Sprite[8];
    private readonly Lock screenLock = new();
    private readonly Pixel emptyPixel;
    private volatile bool renderPixels = true;
    private bool frameRendered;
    private VideoTiming timing;
    private C64Palette palette;
    private int firstLinePixel = FirstLinePixel;

    // A badline fetches one complete 40-byte video-matrix row and its
    // matching Color RAM nibbles. The display pipeline then reuses those
    // latches until the next badline, so writes to screen or color RAM do
    // not retroactively change cells the VIC already fetched.
    private readonly byte[] matrixRow = new byte[40];
    private readonly byte[] colorRow = new byte[40];
    private readonly byte[] glyphRow = new byte[40];
    private readonly bool[] spriteDmaActive = new bool[8];
    private readonly int[] spriteDmaRemaining = new int[8];
    private readonly int[] spriteMemptr = new int[8];
    private readonly bool[] spriteExpandFlip = new bool[8];

    // Derived per-line timing/visibility masks. They are rebuilt from the
    // sprite state and are deliberately not serialized as save state.
    [NonSerialized] private ulong spriteFetchMask;
    [NonSerialized] private ulong spriteLeadMask;
    [NonSerialized] private byte lineSpriteMask;
    [NonSerialized] private readonly sbyte[] spriteFetchOwner = new sbyte[MaxLineCycles];

    // Sprite data for the next raster line is fetched in the sprite's
    // bus slots during the current line, then swapped in at the line
    // boundary. The graphics bytes are latched the same way: fetched on
    // their g-access cycle and displayed two cycles later.
    private readonly byte[] spriteData = new byte[8 * 3];
    private readonly byte[] spriteNextData = new byte[8 * 3];
    private int matrixRowStart;
    private int videoCounterBase;
    private bool denLatched;
    private bool lineIsBadline;
    private bool previousLineWasBadline;
    private int rowCounter;
    private int stolenCycles;

    private RgbColor[,] frontBuffer;
    private RgbColor[,] backBuffer;

    private ushort bankAddress;
    private ushort videoMatrixAddress;
    private ushort pixelMatrixAddress;

    private VideoMode currentVideoMode;
    private bool bitmapMode;
    private int rasterIntLine;
    private bool rasterIntEnabled;
    private bool spriteBackgroundCollide;
    private bool spriteSpriteCollide;
    private bool screenEnabled;
    private byte horizontalScroll;
    private byte verticalScroll;
    private byte textRows = 25;
    private byte textColumns = 40;
    private int firstScreenPixel = FirstScreenPixel;
    private int lastScreenPixel = LastScreenPixel;
    private int firstScreenLine = FirstScreenLine;
    private int lastScreenLine = LastScreenLine;

    private int currentCycle;
    private int currentHLine;
    private int rasterIrqCountdown;
    private bool borderFlipFlop;

    public Vic(
        IVideoBus bus,
        ColorRam colorRam,
        InterruptLine interrupts,
        VideoStandard standard = VideoStandard.Pal)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.colorRam = colorRam ?? throw new ArgumentNullException(nameof(colorRam));
        this.interrupts = interrupts ?? throw new ArgumentNullException(nameof(interrupts));

        timing = VideoTiming.For(standard);
        palette = C64Palette.For(standard);
        firstLinePixel = FirstLinePixel + timing.BusCycleOffset * PixelsPerCycle;
        frontBuffer = new RgbColor[timing.VisibleWidth, timing.VisibleLines];
        backBuffer = new RgbColor[timing.VisibleWidth, timing.VisibleLines];

        emptyPixel.Background = true;
        emptyPixel.PixelColor = palette.Get(0);

        Reset();
    }

    public void Reset()
    {
        Array.Clear(registers);
        Array.Clear(frontBuffer);
        Array.Clear(backBuffer);

        for (var i = 0; i < sprites.Length; i++)
        {
            sprites[i] = new Sprite
            {
                X = 0,
                Y = 0,
                Width = 24,
                Height = 21,
                PixelScale = 1,
                Enabled = false,
                MulticolorMode = false,
                DoubleWidth = false,
                TopMost = true,
                SpriteColor = palette.Get(0)
            };
        }

        bankAddress = 0xC000; // power-on VIC bank 0
        rasterIntLine = 0;
        rasterIntEnabled = false;
        spriteBackgroundCollide = false;
        spriteSpriteCollide = false;
        screenEnabled = true;
        horizontalScroll = 0;
        verticalScroll = 0;
        textRows = 25;
        textColumns = 40;
        firstScreenPixel = FirstScreenPixel;
        lastScreenPixel = LastScreenPixel;
        firstScreenLine = FirstScreenLine;
        lastScreenLine = LastScreenLine;
        currentCycle = 0;
        currentHLine = 0;
        rasterIrqCountdown = 0;
        renderPixels = true;
        frameRendered = false;
        lineIsBadline = false;
        previousLineWasBadline = false;
        rowCounter = 0;
        stolenCycles = 0;
        currentVideoMode = VideoMode.SimpleCharacterMode;
        bitmapMode = false;
        Array.Clear(matrixRow);
        Array.Clear(colorRow);
        Array.Clear(glyphRow);
        Array.Clear(spriteDmaActive);
        Array.Clear(spriteDmaRemaining);
        Array.Clear(spriteMemptr);
        Array.Clear(spriteExpandFlip);
        Array.Fill(spriteFetchOwner, (sbyte)-1);
        spriteFetchMask = 0;
        spriteLeadMask = 0;
        lineSpriteMask = 0;
        Array.Clear(spriteData);
        Array.Clear(spriteNextData);
        matrixRowStart = 0;
        videoCounterBase = 0;
        denLatched = false;
        borderFlipFlop = true;

        UpdateVideoMatrixAddress();
        UpdatePixelMatrixAddress();
        interrupts.Set(InterruptSource.Vic, false);
    }

    internal void RefreshSpriteCachesForState()
    {
        RefreshSpriteCaches();
    }

    internal void ClearFrameBuffers()
    {
        lock (screenLock)
        {
            Array.Clear(frontBuffer);
            Array.Clear(backBuffer);
            frameRendered = false;
        }
    }

    public int RegistersPosition => AddressMap.VicStart;

    public ushort BankAddress
    {
        get => bankAddress;
        set
        {
            // CIA2 port A bits 0-1 are inverted before reaching the VIC.
            bankAddress = (ushort)((~value & 0x03) * 0x4000);
            UpdateVideoMatrixAddress();
            UpdatePixelMatrixAddress();
        }
    }

    public VideoMode CurrentVideoMode => currentVideoMode;

    public bool BitmapMode => bitmapMode;

    public byte HorizontalScroll => horizontalScroll;

    public byte VerticalScroll => verticalScroll;

    public int RasterLine => currentHLine;

    public int RasterCycle => currentCycle;

    public VideoStandard VideoStandard => timing.Standard;

    /// <summary>CPU clock of the active VIC model.</summary>
    public int ClockHz => timing.ClockHz;

    /// <summary>Total CPU cycles in one frame of the active model.</summary>
    public int FrameCycles => timing.FrameCycles;

    public int RasterLineCount => timing.RasterLines;

    /// <summary>Visible width in pixels, borders included.</summary>
    public int VisibleWidth => timing.VisibleWidth;

    /// <summary>Visible height in raster lines, borders included.</summary>
    public int VisibleHeight => timing.VisibleLines;

    /// <summary>Left edge of the standard 40-column display window.</summary>
    public int DisplayLeft => FirstScreenPixel;

    /// <summary>Top row of the display window in the visible frame.</summary>
    public int DisplayTop
    {
        get
        {
            timing.TryGetVisibleRow(FirstScreenLine, out var row);
            return row;
        }
    }

    /// <summary>Width of the 40-column display window in pixels.</summary>
    public int DisplayWidth => LastScreenPixel - FirstScreenPixel + 1;

    /// <summary>Height of the 25-row display window in pixels.</summary>
    public int DisplayHeight => LastScreenLine - FirstScreenLine + 1;

    public long FrameTicks => timing.FrameTicks;

    /// <summary>
    /// Switches between the PAL 6569 and NTSC 6567R8 models. The caller
    /// pauses the machine, so the frame buffers may be resized here. The
    /// register state is reset because the raster geometry changed.
    /// </summary>
    public void SetVideoStandard(VideoStandard standard)
    {
        if (timing.Standard == standard)
            return;

        timing = VideoTiming.For(standard);
        palette = C64Palette.For(standard);
        firstLinePixel = FirstLinePixel + timing.BusCycleOffset * PixelsPerCycle;

        lock (screenLock)
        {
            frontBuffer = new RgbColor[timing.VisibleWidth, timing.VisibleLines];
            backBuffer = new RgbColor[timing.VisibleWidth, timing.VisibleLines];

            Reset();
        }
    }

    /// <summary>
    /// When false, the VIC still advances raster, fetch, DMA, and interrupt
    /// state, but leaves the last completed picture in the front buffer.
    /// Used during warp to avoid redrawing every emulated frame.
    /// </summary>
    public bool RenderPixels
    {
        get => renderPixels;
        set => renderPixels = value;
    }

    /// <summary>
    /// BA is asserted during VIC fetches and for three cycles before each
    /// one. The 6510 may finish a write while BA is low, but it is halted
    /// before its next read.
    /// </summary>
    public bool BaLow => IsBaLow(currentCycle);

    /// <summary>
    /// AEC is low only while the VIC owns the bus. Both CPU reads and
    /// writes are blocked during those cycles.
    /// </summary>
    public bool AecLow => IsAecLow(currentCycle);

    /// <summary>
    /// Returns whether the 6510 may perform a bus access in the current
    /// cycle. Writes are allowed during the BA lead-in; AEC blocks both.
    /// </summary>
    public bool IsCpuAccessAllowed(bool isWrite)
    {
        return !AecLow && (isWrite || !BaLow);
    }

    public void CopyScreenTo(RgbColor[,] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (
            destination.GetLength(0) != VisibleWidth ||
            destination.GetLength(1) < VisibleHeight)
        {
            throw new ArgumentException(
                $"Destination must be {VisibleWidth} x {VisibleHeight} or larger.",
                nameof(destination));
        }

        lock (screenLock)
        {
            if (destination.GetLength(1) == frontBuffer.GetLength(1))
            {
                Array.Copy(frontBuffer, destination, frontBuffer.Length);
                return;
            }

            for (var y = 0; y < VisibleHeight; y++)
                for (var x = 0; x < VisibleWidth; x++)
                    destination[x, y] = frontBuffer[x, y];
        }
    }

    /// <summary>
    /// Replaces the display with a frame received from a network host.
    /// </summary>
    public void SetScreen(RgbColor[,] screen)
    {
        ArgumentNullException.ThrowIfNull(screen);

        if (
            screen.GetLength(0) != VisibleWidth ||
            screen.GetLength(1) != VisibleHeight)
        {
            throw new ArgumentException(
                $"Screen must be {VisibleWidth} x {VisibleHeight}.",
                nameof(screen));
        }

        lock (screenLock)
        {
            frontBuffer = screen;
            backBuffer = (RgbColor[,])screen.Clone();
        }
    }

    /// <summary>
    /// Advances the video pipeline by the requested CPU cycles. Returns true
    /// when a complete frame has been produced.
    /// </summary>
    public bool Tick(int cycles)
    {
        var frameCompleted = false;

        for (var cycle = 0; cycle < cycles; cycle++)
        {
            AdvanceRasterInterrupt();
            ProcessVideoCycle(currentCycle);

            currentCycle++;

            if (currentCycle >= GetLineCycles(currentHLine))
            {
                currentCycle = 0;
                currentHLine++;

                if (currentHLine >= timing.RasterLines)
                {
                    currentHLine = 0;
                    CompleteFrame();
                    frameCompleted = true;
                }

                BeginRasterLine();
                ScheduleRasterInterrupt();
            }
        }

        return frameCompleted;
    }

    /// <summary>
    /// Returns and clears the CPU bus cycles the VIC took since the last
    /// call. The machine uses this to advance the rest of the system while
    /// the CPU is stalled by badline or sprite DMA.
    /// </summary>
    public int TakeStolenCycles()
    {
        var taken = stolenCycles;
        stolenCycles = 0;
        return taken;
    }

    public byte GetRegister(int register)
    {
        switch (register)
        {
            case 0x11:
                return (byte)(
                    (registers[register] & 0x7F) |
                    ((GetRasterLineForRead() & 0x100) >> 1));

            case 0x12:
                return (byte)(GetRasterLineForRead() & 0xFF);

            case 0x13:
            case 0x14:
                // No light pen is connected.
                return 0;

            case 0x16:
                return (byte)(registers[register] | 0xC0);

            case 0x18:
                return (byte)(registers[register] | 0x01);

            case 0x19:
                return (byte)(registers[register] | 0x70);

            case 0x1A:
                return (byte)(registers[register] | 0xF0);

            case 0x1E:
            case 0x1F:
                var collision = registers[register];
                registers[register] = 0;
                UpdateIrq();
                return collision;

            case >= 0x20 and <= 0x2E:
                return (byte)(registers[register] | 0xF0);

            default:
                return registers[register];
        }
    }

    public void SetRegister(int register, byte value)
    {
        registers[register] = value;

        switch (register)
        {
            case 0x00:
            case 0x02:
            case 0x04:
            case 0x06:
            case 0x08:
            case 0x0A:
            case 0x0C:
            case 0x0E:
                var xSprite = register / 2;
                sprites[xSprite].X = (sprites[xSprite].X & 0x100) | value;
                break;

            case 0x01:
            case 0x03:
            case 0x05:
            case 0x07:
            case 0x09:
            case 0x0B:
            case 0x0D:
            case 0x0F:
                var ySprite = register / 2;
                sprites[ySprite].Y = value;
                break;

            case 0x10:
                for (var i = 0; i < 8; i++)
                {
                    sprites[i].X =
                        (sprites[i].X & 0xFF) | ((value & (1 << i)) << (8 - i));
                }
                break;

            case 0x11:
                verticalScroll = (byte)(value & 0x07);
                textRows = (value & 0x08) != 0 ? (byte)25 : (byte)24;
                screenEnabled = (value & 0x10) != 0;
                rasterIntLine = (rasterIntLine & 0xFF) | ((value & 0x80) << 1);
                UpdateVideoMode();
                UpdateScreenGeometry();
                UpdatePixelMatrixAddress();
                TryStartForcedBadline(value);
                CancelStaleRasterInterrupt();

                break;

            case 0x12:
                rasterIntLine = (rasterIntLine & 0x100) | value;
                CancelStaleRasterInterrupt();
                break;

            case 0x15:
                for (var i = 0; i < 8; i++)
                    sprites[i].Enabled = (value & (1 << i)) != 0;
                break;

            case 0x16:
                horizontalScroll = (byte)(value & 0x07);
                textColumns = (value & 0x08) != 0 ? (byte)40 : (byte)38;
                UpdateVideoMode();
                UpdateScreenGeometry();
                break;

            case 0x17:
                for (var i = 0; i < 8; i++)
                    sprites[i].Height = (value & (1 << i)) != 0 ? 42 : 21;
                break;

            case 0x18:
                UpdateVideoMatrixAddress();
                UpdatePixelMatrixAddress();

                break;

            case 0x19:
                registers[register] &= (byte)~value;
                UpdateIrq();
                break;

            case 0x1A:
                rasterIntEnabled = (value & 0x01) != 0;
                spriteBackgroundCollide = (value & 0x02) != 0;
                spriteSpriteCollide = (value & 0x04) != 0;
                UpdateIrq();
                break;

            case 0x1B:
                for (var i = 0; i < 8; i++)
                    sprites[i].TopMost = (value & (1 << i)) == 0;
                break;

            case 0x1C:
                for (var i = 0; i < 8; i++)
                {
                    sprites[i].MulticolorMode = (value & (1 << i)) != 0;
                    UpdateSpriteWidth(i);
                }
                break;

            case 0x1D:
                for (var i = 0; i < 8; i++)
                {
                    sprites[i].DoubleWidth = (value & (1 << i)) != 0;
                    UpdateSpriteWidth(i);
                }
                break;

            case >= 0x27 and <= 0x2E:
                sprites[register - 0x27].SpriteColor = palette.Get(value);
                break;
        }

        // Sprite Y, enable and height affect the next line's DMA lead-in
        // and the set of sprites that can appear on this raster line.
        if (
            (register >= 0x01 && register <= 0x0F && (register & 1) != 0) ||
            register == 0x15 ||
            register == 0x17)
        {
            RefreshSpriteCaches();
        }
    }

    public byte[] CapturePackedScreen()
    {
        var pixelCount = VisibleWidth * VisibleHeight;
        var packed = new byte[(pixelCount + 1) / 2];

        lock (screenLock)
        {
            var position = 0;

            for (var i = 0; i < pixelCount; i += 2)
            {
                var x = i % VisibleWidth;
                var y = i / VisibleWidth;
                var high = palette.ToIndex(frontBuffer[x, y]);
                var low = 0;

                if (i + 1 < pixelCount)
                {
                    var nextX = (i + 1) % VisibleWidth;
                    var nextY = (i + 1) / VisibleWidth;
                    low = palette.ToIndex(frontBuffer[nextX, nextY]);
                }

                packed[position++] = (byte)((high << 4) | low);
            }
        }

        return packed;
    }

    public void SetPackedScreen(byte[] packed)
    {
        ArgumentNullException.ThrowIfNull(packed);

        RgbColor[,] screen = new RgbColor[VisibleWidth, VisibleHeight];
        var pixelCount = VisibleWidth * VisibleHeight;
        var position = 0;

        for (var i = 0; i < pixelCount && position < packed.Length; i += 2)
        {
            var value = packed[position++];
            screen[i % VisibleWidth, i / VisibleWidth] = palette.Get(value >> 4);

            if (i + 1 < pixelCount)
            {
                screen[(i + 1) % VisibleWidth, (i + 1) / VisibleWidth] =
                    palette.Get(value & 0x0F);
            }
        }

        SetScreen(screen);
    }

    private void RenderPixel(int x, bool draw)
    {
        var y = currentHLine;

        if (!timing.TryGetVisibleRow(y, out var screenY))
            return;

        var screenX = x;

        if (screenX < 0 || screenX >= VisibleWidth)
            return;

        Pixel pixel = GetBackgroundPixel(x, y);
        var collisionMask = 0;
        var firstSprite = -1;
        var firstFrontSprite = -1;
        RgbColor firstSpriteColor = palette.Get(0);
        RgbColor firstFrontSpriteColor = palette.Get(0);
        var localX = x - SpriteXOrigin;

        for (var sprite = 0; sprite < 8; sprite++)
        {
            if ((lineSpriteMask & (1 << sprite)) == 0)
                continue;

            ref Sprite spriteState = ref sprites[sprite];

            if (
                localX < spriteState.X ||
                localX >= spriteState.X + spriteState.Width)
            {
                continue;
            }

            Pixel spritePixel = GetSpritePixel(sprite, localX);

            if (spritePixel.Background)
                continue;

            collisionMask |= 1 << sprite;

            if (firstSprite < 0)
            {
                firstSprite = sprite;
                firstSpriteColor = spritePixel.PixelColor;
            }

            if (sprites[sprite].TopMost && firstFrontSprite < 0)
            {
                firstFrontSprite = sprite;
                firstFrontSpriteColor = spritePixel.PixelColor;
            }
        }

        var spriteSpriteCollision =
            collisionMask != 0 && (collisionMask & (collisionMask - 1)) != 0;
        var spriteBackgroundCollision = collisionMask != 0 && !pixel.Background;

        if (spriteSpriteCollision)
            registers[0x1E] |= (byte)collisionMask;

        if (spriteBackgroundCollision)
            registers[0x1F] |= (byte)collisionMask;

        if (spriteSpriteCollision || spriteBackgroundCollision)
            UpdateIrq();

        var drawSprite = pixel.Background ? firstSprite : firstFrontSprite;
        RgbColor color;

        if (drawSprite < 0)
            color = pixel.PixelColor;
        else if (drawSprite == firstSprite)
            color = firstSpriteColor;
        else
            color = firstFrontSpriteColor;

        if (draw)
            backBuffer[screenX, screenY] = color;
    }

    private Pixel GetBackgroundPixel(int x, int y)
    {
        // The border is a flip-flop, not a fixed aperture. It opens at
        // the left edge of the display window when DEN is set and the
        // raster line is inside the window, and closes at the right edge.
        // Clearing DEN mid-line closes it for the rest of the line even
        // if DEN comes back, which is what border-opening tricks use.
        if (!screenEnabled)
            borderFlipFlop = true;
        else if (
            x == firstScreenPixel &&
            y >= firstScreenLine &&
            y <= lastScreenLine)
            borderFlipFlop = false;
        else if (x == lastScreenPixel + 1)
            borderFlipFlop = true;

        if (borderFlipFlop)
        {
            return new Pixel
            {
                Background = true,
                PixelColor = palette.Get(registers[0x20])
            };
        }

        // RSEL changes the fixed border aperture, but not where screen row 0
        // begins: the first display line is always $30 + YSCROLL. Deriving
        // the source row from the aperture conflated the two and made
        // 24-row scrolling move four pixels per YSCROLL step instead of
        // two (Boulder Dash rests at YSCROLL 7 with RSEL clear).
        var sourceX = x - horizontalScroll;
        var sourceY = y - (FirstDisplayLine + verticalScroll);

        if (
            sourceX < FirstScreenPixel ||
            sourceX > LastScreenPixel ||
            sourceY < 0 ||
            sourceY >= 25 * 8)
        {
            // The VIC fetches the full 25-row matrix regardless of RSEL.
            // With RSEL clear, fine scrolling exposes the start of row 24
            // in the bottom six scanlines of the 24-row aperture. Boulder
            // Dash relies on that during its YSCROLL 1 -> 7 wrap; clipping
            // at the nominal 24-row height made the bottom strip vanish
            // and pop back every coarse-scroll step.
            return new Pixel
            {
                Background = true,
                PixelColor = palette.Get(registers[0x21])
            };
        }

        return GetPixel(sourceX - FirstScreenPixel, sourceY);
    }

    private Pixel GetSpritePixel(int sprite, int x)
    {
        ref Sprite spriteState = ref sprites[sprite];

        if (
            x < spriteState.X ||
            x >= spriteState.X + spriteState.Width)
        {
            return emptyPixel;
        }

        var xOffset = x - spriteState.X;

        xOffset /= spriteState.PixelScale;

        // The three bytes shown on this raster line were fetched in the
        // sprite's bus slots during the previous line.
        var value = spriteData[sprite * 3 + xOffset / 8];
        var bitPosition = 7 - (xOffset % 8);

        if (spriteState.MulticolorMode)
        {
            value = (byte)((value >> ((bitPosition / 2) * 2)) & 0x03);

            return value switch
            {
                0 => emptyPixel,
                1 => new Pixel
                {
                    Background = false,
                    PixelColor = palette.Get(registers[0x25])
                },
                2 => new Pixel
                {
                    Background = false,
                    PixelColor = spriteState.SpriteColor
                },
                _ => new Pixel
                {
                    Background = false,
                    PixelColor = palette.Get(registers[0x26])
                }
            };
        }

        value = (byte)((value >> bitPosition) & 0x01);

        if (value == 0)
            return emptyPixel;

        return new Pixel
        {
            Background = false,
            PixelColor = spriteState.SpriteColor
        };
    }

    private Pixel GetPixel(int x, int y)
    {
        return currentVideoMode switch
        {
            VideoMode.SimpleCharacterMode => GetSimpleCharacterPixel(x, y),
            VideoMode.MultiColorCharacterMode => GetMultiColorCharacterPixel(x, y),
            VideoMode.ExtendedColorCharacterMode => GetExtendedColorCharacterPixel(x, y),
            VideoMode.SimpleBitMapMode => GetSimpleBitmapPixel(x, y),
            VideoMode.MultiColorBitMapMode => GetMultiColorBitmapPixel(x, y),
            _ => emptyPixel
        };
    }

    private Pixel GetSimpleCharacterPixel(int x, int y)
    {
        var cell = x / 8;
        var bit = x % 8;

        if ((glyphRow[cell] & (0x80 >> bit)) != 0)
        {
            return new Pixel
            {
                Background = false,
                PixelColor = palette.Get(colorRow[cell])
            };
        }

        return new Pixel
        {
            Background = true,
            PixelColor = palette.Get(registers[0x21])
        };
    }

    private Pixel GetMultiColorCharacterPixel(int x, int y)
    {
        var cell = x / 8;
        var bit = x % 8;

        if ((colorRow[cell] & 0x08) == 0)
            return GetSimpleCharacterPixel(x, y);

        var value = (glyphRow[cell] >> (2 * ((7 - bit) / 2))) & 0x03;

        return value switch
        {
            0 => new Pixel
            {
                Background = true,
                PixelColor = palette.Get(registers[0x21])
            },
            1 => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(registers[0x22])
            },
            2 => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(registers[0x23])
            },
            _ => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(colorRow[cell] & 0x07)
            }
        };
    }

    private Pixel GetExtendedColorCharacterPixel(int x, int y)
    {
        var cell = x / 8;
        var bit = x % 8;
        var backgroundIndex = (matrixRow[cell] >> 6) & 0x03;

        if ((glyphRow[cell] & (0x80 >> bit)) != 0)
        {
            return new Pixel
            {
                Background = false,
                PixelColor = palette.Get(colorRow[cell])
            };
        }

        return new Pixel
        {
            Background = true,
            PixelColor = palette.Get(registers[0x21 + backgroundIndex])
        };
    }

    private Pixel GetSimpleBitmapPixel(int x, int y)
    {
        var cell = x / 8;
        var color = matrixRow[cell];
        var bit = x % 8;

        if ((glyphRow[cell] & (0x80 >> bit)) != 0)
        {
            return new Pixel
            {
                Background = false,
                PixelColor = palette.Get(color >> 4)
            };
        }

        return new Pixel
        {
            Background = true,
            PixelColor = palette.Get(color & 0x0F)
        };
    }

    private Pixel GetMultiColorBitmapPixel(int x, int y)
    {
        var cell = x / 8;
        var color = matrixRow[cell];
        var bit = x % 8;
        var value = (glyphRow[cell] >> (2 * ((7 - bit) / 2))) & 0x03;

        return value switch
        {
            0 => new Pixel
            {
                Background = true,
                PixelColor = palette.Get(registers[0x21])
            },
            1 => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(color >> 4)
            },
            2 => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(color & 0x0F)
            },
            _ => new Pixel
            {
                Background = false,
                PixelColor = palette.Get(colorRow[cell])
            }
        };
    }

    private void ProcessVideoCycle(int cycle)
    {
        // VIC-II access table. The 40 c-accesses run in the VIC's second
        // phase, which is what steals the CPU during a badline, and the
        // matching g-accesses follow one cycle later in the normal first
        // phase. The NTSC 6567R8 has one extra cycle at the start of the
        // line, so both runs move one cycle later there: 15-54 and 16-55
        // on the 6569, 16-55 and 17-56 on the 6567R8. The fetched byte
        // reaches the shift register twelve pixels before it is displayed.
        var busOffset = timing.BusCycleOffset;
        var matrixFetchStart = 15 + busOffset;

        if (
            lineIsBadline &&
            cycle >= matrixFetchStart &&
            cycle < matrixFetchStart + 40)
        {
            FetchMatrixCell(cycle - matrixFetchStart);
            IncrementStolenCycles();
        }

        var glyphFetchStart = 16 + busOffset;

        if (cycle >= glyphFetchStart && cycle < glyphFetchStart + 40)
            FetchGlyphCell(cycle - glyphFetchStart);

        var fetchSprite = spriteFetchOwner[cycle];

        if (fetchSprite >= 0)
            FetchSpriteLine(fetchSprite);

        if ((spriteFetchMask & (1UL << cycle)) != 0)
            IncrementStolenCycles();

        // Sprite collisions are chip state, not a rendering side effect:
        // they must keep updating while pixels are not drawn (warp), or a
        // program polling $D01E/$D01F would miss them. With no sprite
        // active on this line a collision is impossible, so the per-pixel
        // work can still be skipped entirely.
        if (!renderPixels && lineSpriteMask == 0)
            return;

        if (renderPixels)
            frameRendered = true;

        var pixelStart = cycle * PixelsPerCycle - firstLinePixel;

        // The last cycle of a line runs into the right border. On NTSC the
        // extra cycle leaves two visible columns beyond the normal eight,
        // so the border is drawn out to the right edge of the picture.
        var pixelEnd = cycle == GetLineCycles(currentHLine) - 1
            ? VisibleWidth
            : pixelStart + PixelsPerCycle;

        for (var x = pixelStart; x < pixelEnd; x++)
        {
            if (x >= 0 && x < VisibleWidth)
                RenderPixel(x, draw: renderPixels);
        }
    }

    private void BeginRasterLine()
    {
        borderFlipFlop = true;

        if (currentHLine == FirstDisplayLine)
        {
            // DEN is sampled while raster $30 is active. VCBASE restarts
            // at the top of the matrix for the new frame.
            denLatched = false;
            videoCounterBase = 0;
            rowCounter = 0;
            previousLineWasBadline = false;
        }

        if (currentHLine == FirstDisplayLine && screenEnabled)
            denLatched = true;

        lineIsBadline =
            denLatched &&
            currentHLine >= FirstDisplayLine &&
            currentHLine <= 0xF7 &&
            (currentHLine & 0x07) == verticalScroll;

        // VCBASE advances when a character row completes, not on every
        // badline. During raster splits badlines can be closer than eight
        // scanlines; advancing the row at each fetch made the display
        // switch rows when the split landed mid-row.
        if (rowCounter == 7)
            videoCounterBase = (videoCounterBase + 40) & 0x03FF;

        if (lineIsBadline)
        {
            // A normal badline starts a new character row. Forced
            // badlines can occur on consecutive scanlines (FLI and other
            // raster tricks); those must keep advancing the row counter
            // instead of redrawing bitmap row zero on every line.
            rowCounter = previousLineWasBadline
                ? (rowCounter + 1) & 0x07
                : 0;
            matrixRowStart = videoCounterBase;
        }
        else
        {
            rowCounter = (rowCounter + 1) & 0x07;
        }

        previousLineWasBadline = lineIsBadline;
        UpdateSpriteDma();
    }

    private void FetchMatrixCell(int column)
    {
        var offset = (matrixRowStart + column) & 0x03FF;

        matrixRow[column] = bus.Read(
            (ushort)(videoMatrixAddress + offset));
        colorRow[column] = colorRam.ReadNibble((ushort)offset);
    }

    private void FetchGlyphCell(int column)
    {
        var cell = bitmapMode ? matrixRowStart + column : matrixRow[column];

        if (currentVideoMode == VideoMode.ExtendedColorCharacterMode)
            cell = matrixRow[column] & 0x3F;

        glyphRow[column] = bus.Read(
            (ushort)(pixelMatrixAddress + cell * 8 + rowCounter));
    }

    private void UpdateSpriteDma()
    {
        // A sprite's data for raster line L is fetched in its bus slots
        // during line L-1, so DMA has to be running one line before the
        // first visible line. Each active line advances the sprite's
        // 64-byte data block by one row (every other line when the sprite
        // is Y-expanded) and the fetched bytes are latched for display.
        for (var sprite = 0; sprite < sprites.Length; sprite++)
        {
            if (!sprites[sprite].Enabled)
            {
                spriteDmaActive[sprite] = false;
                spriteDmaRemaining[sprite] = 0;
                spriteMemptr[sprite] = 0;
                continue;
            }

            if (currentHLine == sprites[sprite].Y)
            {
                spriteDmaActive[sprite] = true;
                spriteDmaRemaining[sprite] = Math.Max(0, sprites[sprite].Height - 1);
                spriteMemptr[sprite] = 0;
                spriteExpandFlip[sprite] = false;
            }
            else if (spriteDmaRemaining[sprite] > 0)
            {
                spriteDmaActive[sprite] = true;
                spriteDmaRemaining[sprite]--;

                if (sprites[sprite].Height == 42)
                {
                    spriteExpandFlip[sprite] = !spriteExpandFlip[sprite];

                    if (!spriteExpandFlip[sprite])
                        spriteMemptr[sprite] = (spriteMemptr[sprite] + 3) & 0x3F;
                }
                else
                {
                    spriteMemptr[sprite] = (spriteMemptr[sprite] + 3) & 0x3F;
                }
            }
            else
            {
                spriteDmaActive[sprite] = false;
            }
        }

        Array.Copy(spriteNextData, spriteData, spriteNextData.Length);
        RefreshSpriteCaches();
    }

    private void RefreshSpriteCaches()
    {
        UpdateSpriteBusMasks();
        UpdateSpriteLineMask();
    }

    /// <summary>
    /// Builds the two bus masks once per raster line instead of scanning
    /// every sprite on every CPU access. The fetch mask covers the two
    /// AEC cycles, while the lead mask covers the three BA cycles before
    /// each fetch, including the tail of the previous line for sprite 3.
    /// </summary>
    private void UpdateSpriteBusMasks()
    {
        ulong fetch = 0;
        ulong lead = 0;
        var lineCycles = GetLineCycles(currentHLine);

        Array.Fill(spriteFetchOwner, (sbyte)-1);

        for (var sprite = 0; sprite < sprites.Length; sprite++)
        {
            if (spriteDmaActive[sprite])
            {
                var firstCycle = GetSpriteFetchCycle(sprite);

                fetch |= 1UL << firstCycle;
                fetch |= 1UL << (firstCycle + 1);
                spriteFetchOwner[firstCycle] = (sbyte)sprite;

                var leadStart = Math.Max(0, firstCycle - 3);

                for (var cycle = leadStart; cycle < firstCycle; cycle++)
                    lead |= 1UL << cycle;
            }

            if (!WillSpriteDmaBeActiveOnNextLine(sprite))
                continue;

            var nextFirstCycle = GetSpriteFetchCycle(sprite);

            if (nextFirstCycle >= 3)
                continue;

            for (
                var cycle = Math.Max(0, lineCycles + nextFirstCycle - 3);
                cycle < lineCycles;
                cycle++
            )
            {
                lead |= 1UL << cycle;
            }
        }

        spriteFetchMask = fetch;
        spriteLeadMask = lead;
    }

    private void UpdateSpriteLineMask()
    {
        byte mask = 0;
        var y = currentHLine;

        for (var sprite = 0; sprite < sprites.Length; sprite++)
        {
            ref Sprite spriteState = ref sprites[sprite];

            if (
                spriteState.Enabled &&
                y >= spriteState.Y + 1 &&
                y < spriteState.Y + 1 + spriteState.Height)
            {
                mask |= (byte)(1 << sprite);
            }
        }

        lineSpriteMask = mask;
    }

    /// <summary>
    /// Reads the three data bytes a sprite displays on the next raster
    /// line when its fetch slot comes up. The pointer and the data block
    /// are read once per line, exactly like the real chip, instead of
    /// being re-read for every rendered pixel.
    /// </summary>
    private void FetchSpriteLine(int sprite)
    {
        if (!spriteDmaActive[sprite])
            return;

        var pointerAddress = (ushort)(videoMatrixAddress + 0x3F8 + sprite);
        var spriteBase = (ushort)(
            bankAddress + bus.Read(pointerAddress) * 0x40);
        var memptr = spriteMemptr[sprite];

        for (var i = 0; i < 3; i++)
        {
            spriteNextData[sprite * 3 + i] = bus.Read(
                (ushort)(spriteBase + ((memptr + i) & 0x3F)));
        }
    }

    private bool IsAecLow(int cycle)
    {
        var busOffset = timing.BusCycleOffset;
        var matrixFetchStart = 15 + busOffset;

        if (
            lineIsBadline &&
            cycle >= matrixFetchStart &&
            cycle < matrixFetchStart + 40)
            return true;

        // NTSC lines are 65 cycles long, so the last one must stay usable.
        return (uint)cycle < MaxLineCycles &&
            (spriteFetchMask & (1UL << cycle)) != 0;
    }

    private bool IsBaLow(int cycle)
    {
        if (IsAecLow(cycle))
            return true;

        // The badline C-access run starts on cycle 15 (16 on NTSC), so BA
        // is asserted three cycles earlier to give the 6510 time to finish
        // a write.
        var leadStart = 12 + timing.BusCycleOffset;

        if (lineIsBadline && cycle >= leadStart && cycle < leadStart + 3)
            return true;

        return (uint)cycle < MaxLineCycles &&
            (spriteLeadMask & (1UL << cycle)) != 0;
    }

    private bool WillSpriteDmaBeActiveOnNextLine(int sprite)
    {
        if (!sprites[sprite].Enabled)
            return false;

        var nextLine = currentHLine + 1 >= timing.RasterLines
            ? 0
            : currentHLine + 1;

        return
            nextLine == sprites[sprite].Y ||
            spriteDmaRemaining[sprite] > 0;
    }

    private int GetSpriteFetchCycle(int sprite)
    {
        // Sprite p/s-access slots, one cycle later on the 6567R8.
        return timing.BusCycleOffset + sprite switch
        {
            0 => 57,
            1 => 59,
            2 => 61,
            3 => 0,
            4 => 2,
            5 => 4,
            6 => 6,
            _ => 8
        };
    }

    private void IncrementStolenCycles()
    {
        // The machine now stalls the CPU at each access instead of
        // draining this counter every instruction. Keep the legacy
        // accounting available for diagnostics without overflowing
        // during long runs.
        if (stolenCycles < int.MaxValue)
            stolenCycles++;
    }

    private void CompleteFrame()
    {
        if (!frameRendered)
            return;

        frameRendered = false;

        lock (screenLock)
        {
            (frontBuffer, backBuffer) = (backBuffer, frontBuffer);
        }
    }

    private void ScheduleRasterInterrupt()
    {
        if (currentHLine != rasterIntLine)
        {
            rasterIrqCountdown = 0;
            return;
        }

        // The VIC raises the raster IRQ flag on cycle 1 of the matching
        // line. Two countdown steps keep the flag out of cycle 0.
        rasterIrqCountdown = 2;
    }

    private void AdvanceRasterInterrupt()
    {
        if (rasterIrqCountdown <= 0)
            return;

        rasterIrqCountdown--;

        if (rasterIrqCountdown == 0)
        {
            registers[0x19] |= 0x01;
            UpdateIrq();
        }
    }

    private void CancelStaleRasterInterrupt()
    {
        if (currentHLine != rasterIntLine)
            rasterIrqCountdown = 0;
    }

    private int GetRasterLineForRead()
    {
        // At cycle 0 of line 0 the raster counter still reads the last
        // line of the previous frame on a PAL 6569.
        return currentHLine == 0 && currentCycle == 0
            ? timing.RasterLines - 1
            : currentHLine;
    }

    private int GetLineCycles(int line)
    {
        if (line == 0)
            return timing.FirstLineCycles;

        if (line == timing.RasterLines - 1)
            return timing.LastLineCycles;

        return timing.CyclesPerLine;
    }

    private void UpdateIrq()
    {
        var asserted =
            (rasterIntEnabled && (registers[0x19] & 0x01) != 0) ||
            (spriteBackgroundCollide && (registers[0x19] & 0x02) != 0) ||
            (spriteSpriteCollide && (registers[0x19] & 0x04) != 0);

        if (asserted)
            registers[0x19] |= 0x80;
        else
            registers[0x19] &= 0x7F;

        interrupts.Set(InterruptSource.Vic, asserted);
    }

    private void UpdateSpriteWidth(int sprite)
    {
        // Only X expansion ($D01D) changes a sprite's on-screen width: a
        // sprite is 24 pixels wide, or 48 when that bit is set.
        // Multicolour mode does not widen it -- it pairs source bits into
        // 12 colour pixels that are already two pixels wide on their own,
        // which GetSpritePixel handles. Treating multicolour as a width
        // multiplier drew every multicolour sprite (DOTC's castles and
        // raiders among them) twice as wide as it should be.
        var scale = sprites[sprite].DoubleWidth ? 2 : 1;

        sprites[sprite].PixelScale = scale;
        sprites[sprite].Width = 24 * scale;
    }

    private void UpdateVideoMode()
    {
        currentVideoMode = (VideoMode)(
            (registers[0x11] & 0x60) |
            (registers[0x16] & 0x10));

        bitmapMode = ((byte)currentVideoMode & 0x20) != 0;
    }

    private void UpdateScreenGeometry()
    {
        firstScreenLine = textRows == 24 ? FirstScreenLine + 4 : FirstScreenLine;
        lastScreenLine = textRows == 24 ? LastScreenLine - 4 : LastScreenLine;

        firstScreenPixel = textColumns == 38
            ? FirstScreenPixel + 7
            : FirstScreenPixel;
        lastScreenPixel = firstScreenPixel + (textColumns == 38 ? 304 : 320) - 1;
    }

    private void TryStartForcedBadline(byte value)
    {
        if (
            !denLatched ||
            !screenEnabled ||
            lineIsBadline ||
            currentHLine < FirstDisplayLine ||
            currentHLine > 0xF7 ||
            (currentHLine & 0x07) != (value & 0x07))
        {
            return;
        }

        // A raster routine can write YSCROLL on cycle 14 to force a
        // partial badline for the current line (FLI). That refreshes the
        // matrix and color latches but must not restart the bitmap row.
        lineIsBadline = true;
        matrixRowStart = videoCounterBase;
        previousLineWasBadline = true;
    }

    private void UpdateVideoMatrixAddress()
    {
        videoMatrixAddress = (ushort)(
            bankAddress + (registers[0x18] >> 4) * 0x400);
    }

    private void UpdatePixelMatrixAddress()
    {
        pixelMatrixAddress = bitmapMode
            ? (ushort)(bankAddress + (registers[0x18] & 0x08) * 0x400)
            : (ushort)(bankAddress + (registers[0x18] & 0x0E) * 0x400);
    }
}
