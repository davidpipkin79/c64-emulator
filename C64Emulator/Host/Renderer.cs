using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using C64Emulator.Core;
using C64Emulator.Properties;
using C64Emulator.Video;
using SDL3;

namespace C64Emulator.Host;

internal sealed class Renderer : IDisposable
{

    private readonly C64Machine machine;
    private readonly Lock frameLock = new();
    private readonly Lock overlayLock = new();
    private readonly RgbColor invalidateColor = new RgbColor(1, 1, 1);
    private int[] framePixels = [];
    private RgbColor[,] lastRender = new RgbColor[0, 0];
    private RgbColor[,] screenSnapshot = new RgbColor[0, 0];
    private int windowedScale = 1;
    private readonly byte[] fontData;
    private readonly GCHandle fontDataHandle;
    private readonly Dictionary<string, TextTexture> textCache =
        new Dictionary<string, TextTexture>();
    private System.Timers.Timer? statusTimer;
    private System.Timers.Timer? fpsTimer;

    private IntPtr window;
    private IntPtr renderer;
    private IntPtr frameTexture;
    private IntPtr statusFont;
    private IntPtr fpsFont;
    private IntPtr menuFont;
    private int statusHeight;
    private int fpsHeight;
    private int menuHeight;

    private volatile bool disposed;
    private bool frameDirty = true;
    private bool fullscreen;
    private VideoStandard lastStandard;
    private volatile bool showFPS;

    private volatile bool statusEnabled;
    private volatile string statusText = string.Empty;
    private volatile string fpsText = string.Empty;

    private MenuEntry[] menuEntries = [];
    private int menuSelected;
    private int menuPosition;
    private bool menuVisible;
    private string menuHint = string.Empty;

    private string[]? errorLines;
    private bool errorVisible;

    private sealed class TextTexture
    {
        public IntPtr Texture;
        public int Width;
        public int Height;
    }

    private readonly struct MenuEntry(string text, int level)
    {
        public string Text { get; } = text;

        public int Level { get; } = level;
    }

    public Renderer(C64Machine machine)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        lastStandard = machine.VideoStandard;
        AllocateFrameBuffers();
        fontData = Resources.c64font;
        fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

        try
        {
            Initialize();
        }
        catch
        {
            Kill();
            throw;
        }
    }

    private int FrameWidth => machine.Vic.VisibleWidth;

    private int FrameHeight => machine.Vic.VisibleHeight;

    /// <summary>
    /// Width of the visible frame after the VIC's non-square pixels are
    /// mapped to a 4:3 display. The frame texture and overlay coordinates
    /// remain in native VIC pixels.
    /// </summary>
    public int PresentationWidth =>
        VideoTiming.For(machine.VideoStandard).PresentationWidth;

    public int PresentationHeight => FrameHeight;

    /// <summary>
    /// Sizes the CPU-side frame storage to the active video standard. The
    /// renderer carries no fixed picture size: PAL frames are 403 x 284 and
    /// NTSC frames are 418 x 235.
    /// </summary>
    private void AllocateFrameBuffers()
    {
        var width = FrameWidth;
        var height = FrameHeight;

        framePixels = new int[width * height];
        lastRender = new RgbColor[width, height];
        screenSnapshot = new RgbColor[width, height];
    }

    private static int SavedWindowWidth(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc
            ? AppSettings.Current.NtscWindowWidth
            : AppSettings.Current.PalWindowWidth;
    }

    private static int SavedWindowHeight(VideoStandard standard)
    {
        return standard == VideoStandard.Ntsc
            ? AppSettings.Current.NtscWindowHeight
            : AppSettings.Current.PalWindowHeight;
    }

    private static void SaveWindowSize(
        VideoStandard standard,
        int width,
        int height)
    {
        if (standard == VideoStandard.Ntsc)
        {
            AppSettings.Current.NtscWindowWidth = width;
            AppSettings.Current.NtscWindowHeight = height;
        }
        else
        {
            AppSettings.Current.PalWindowWidth = width;
            AppSettings.Current.PalWindowHeight = height;
        }
    }

    /// <summary>
    /// Keeps a windowed size on the picture's aspect ratio. A size that
    /// already matches is preserved; an older arbitrary size is snapped to
    /// the nearest whole-number zoom so letterboxing leaves no bars.
    /// </summary>
    private static (int Width, int Height) FitWindowToFrame(
        VideoTiming timing,
        int width,
        int height)
    {
        var frameWidth = timing.PresentationWidth;
        var frameHeight = timing.VisibleLines;

        if (width > 0 && height > 0)
        {
            var frameAspect = (double)frameWidth / frameHeight;
            var windowAspect = (double)width / height;

            if (Math.Abs(windowAspect - frameAspect) / frameAspect <= 0.005)
            {
                return (
                    Math.Max(frameWidth, width),
                    Math.Max(frameHeight, height));
            }
        }

        var zoom = Math.Max(
            1,
            (int)Math.Round(
                Math.Min(
                    (double)Math.Max(width, 1) / frameWidth,
                    (double)Math.Max(height, 1) / frameHeight)));

        return (frameWidth * zoom, frameHeight * zoom);
    }

    /// <summary>
    /// Stores the window the outgoing video standard was showing, so it is
    /// restored when the user switches back to that standard.
    /// </summary>
    private bool SaveCurrentWindow(
        VideoStandard standard,
        out int width,
        out int height)
    {
        if (!SDL.GetWindowSize(window, out width, out height))
            return false;

        SaveWindowSize(standard, width, height);
        return true;
    }

    /// <summary>Windowed width the active video standard last used.</summary>
    public int LastWindowWidth
    {
        get
        {
            var width = SavedWindowWidth(machine.VideoStandard);
            var height = SavedWindowHeight(machine.VideoStandard);

            if (width <= 0 || height <= 0)
                return PresentationWidth;

            var size = FitWindowToFrame(
                VideoTiming.For(machine.VideoStandard),
                width,
                height);

            return size.Width;
        }
    }

    /// <summary>Windowed height the active video standard last used.</summary>
    public int LastWindowHeight
    {
        get
        {
            var width = SavedWindowWidth(machine.VideoStandard);
            var height = SavedWindowHeight(machine.VideoStandard);

            if (width <= 0 || height <= 0)
                return FrameHeight;

            var size = FitWindowToFrame(
                VideoTiming.For(machine.VideoStandard),
                width,
                height);

            return size.Height;
        }
    }

    /// <summary>Keeps interactive resizes on the picture's aspect ratio.</summary>
    private void ApplyAspectRatio()
    {
        var aspect = (float)PresentationWidth / FrameHeight;

        SDL.SetWindowAspectRatio(window, aspect, aspect);
    }

    /// <summary>
    /// Selects how the aspect-corrected 4:3 frame maps to the window.
    /// Windowed and fit fullscreen use letterboxing; fill-height fullscreen
    /// uses the whole display height, cropping the sides only when the
    /// display is narrower than the C64 picture.
    /// </summary>
    private void ApplyPresentation()
    {
        SDL.RendererLogicalPresentation presentation =
            SDL.RendererLogicalPresentation.Letterbox;

        if (
            fullscreen &&
            AppSettings.Current.FullscreenFillHeight &&
            SDL.GetWindowSize(window, out var width, out var height) &&
            width > 0 &&
            height > 0)
        {
            var screenAspect = (double)width / height;
            var frameAspect = (double)PresentationWidth / FrameHeight;

            if (screenAspect < frameAspect)
                presentation = SDL.RendererLogicalPresentation.Overscan;
        }

        SDL.SetRenderLogicalPresentation(
            renderer,
            PresentationWidth,
            FrameHeight,
            presentation
            );
    }

    private void Initialize()
    {
        VideoTiming timing = VideoTiming.For(machine.VideoStandard);
        var windowWidth = Math.Max(
            PresentationWidth,
            AppSettings.Current.ResolutionWidth);
        var windowHeight = Math.Max(
            FrameHeight,
            AppSettings.Current.ResolutionHeight);

        if (!AppSettings.Current.Fullscreen)
        {
            var savedWidth = SavedWindowWidth(machine.VideoStandard);
            var savedHeight = SavedWindowHeight(machine.VideoStandard);

            (windowWidth, windowHeight) = savedWidth > 0 && savedHeight > 0
                ? FitWindowToFrame(timing, savedWidth, savedHeight)
                : FitWindowToFrame(
                    timing,
                    AppSettings.Current.ResolutionWidth,
                    AppSettings.Current.ResolutionHeight);
        }

        if (
            !SDL.CreateWindowAndRenderer(
                "Pipkinsoft C64 Emulator",
                windowWidth,
                windowHeight,
                SDL.WindowFlags.Resizable,
                out window,
                out renderer
                )
            )
            throw new InvalidOperationException(
                "Unable to create the SDL window: " + SDL.GetError()
                );

        SDL.SetWindowMinimumSize(window, PresentationWidth, FrameHeight);
        ApplyAspectRatio();

        ApplyVSync(AppSettings.Current.VSync);

        createFrameTexture();

        loadWindowIcon();

        fpsFont = openFont(8);
        fpsHeight = TTF.GetFontHeight(fpsFont);
        configureFonts();

        statusTimer = new System.Timers.Timer(3000);
        statusTimer.AutoReset = false;
        statusTimer.Elapsed += statusTimer_Elapsed;

        fpsTimer = new System.Timers.Timer(1000);
        fpsTimer.Elapsed += fpsTimer_Elapsed;

        showFPS = AppSettings.Current.ShowSpeed;
        fpsTimer.Enabled = showFPS;

        Reset(
            AppSettings.Current.Fullscreen,
            windowWidth,
            windowHeight,
            AppSettings.Current.DisplayRefreshRate
            );
    }

    private void loadWindowIcon()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "c64icon.bmp");

        if (!File.Exists(path))
            return;

        var icon = SDL.LoadBMP(path);

        if (icon == IntPtr.Zero)
            return;

        SDL.SetWindowIcon(window, icon);
        SDL.DestroySurface(icon);
    }

    /// <summary>
    /// Rebuilds the overlay fonts after the machine switched video standard.
    /// Both standards use the same text sizes because their frames are wide
    /// enough; the menu rows compress vertically on the shorter NTSC frame.
    /// </summary>
    private void configureFonts()
    {
        clearTextCache();

        if (statusFont != IntPtr.Zero)
        {
            TTF.CloseFont(statusFont);
            statusFont = IntPtr.Zero;
        }

        if (menuFont != IntPtr.Zero)
        {
            TTF.CloseFont(menuFont);
            menuFont = IntPtr.Zero;
        }

        statusFont = openFont(15);
        menuFont = openFont(12);

        statusHeight = TTF.GetFontHeight(statusFont);
        menuHeight = TTF.GetFontHeight(menuFont);
    }

    private void clearTextCache()
    {
        foreach (TextTexture texture in textCache.Values)
            if (texture.Texture != IntPtr.Zero)
                SDL.DestroyTexture(texture.Texture);

        textCache.Clear();
    }

    private IntPtr openFont(int size)
    {
        var io = SDL.IOFromConstMem(
            fontDataHandle.AddrOfPinnedObject(),
            (UIntPtr)fontData.Length
            );

        if (io == IntPtr.Zero)
            throw new InvalidOperationException(
                "Unable to open the embedded font: " + SDL.GetError()
                );

        var font = TTF.OpenFontIO(io, true, size);

        if (font == IntPtr.Zero)
            throw new InvalidOperationException(
                "Unable to load the embedded font: " + SDL.GetError()
                );

        TTF.SetFontKerning(font, false);

        return font;
    }

    public bool VSyncEnabled { get; private set; }

    public bool SetVSync(bool enabled)
    {
        AppSettings.Current.VSync = enabled;
        ApplyVSync(enabled);

        return VSyncEnabled == enabled;
    }

    private void ApplyVSync(bool enabled)
    {
        var changed = SDL.SetRenderVSync(renderer, enabled ? 1 : 0);

        if (SDL.GetRenderVSync(renderer, out var vsync))
            VSyncEnabled = vsync != 0;
        else if (changed)
            VSyncEnabled = enabled;
    }

    /// <summary>
    /// Whether the host draws a dark line between every other presented row.
    /// This is a display-only effect; it does not change the VIC frame.
    /// </summary>
    public bool Scanlines =>
        AppSettings.Current.Scanlines;

    public void SetScanlines(bool enabled)
    {
        AppSettings.Current.Scanlines = enabled;
    }

    /// <summary>
    /// Whether fullscreen scales the picture to the display height instead
    /// of fitting the whole frame.
    /// </summary>
    public bool FullscreenFillHeight =>
        AppSettings.Current.FullscreenFillHeight;

    public void SetFullscreenFillHeight(bool enabled)
    {
        AppSettings.Current.FullscreenFillHeight = enabled;
        ApplyPresentation();
    }

    public bool Fullscreen
    {
        get
        {
            return fullscreen;
        }
    }

    public DisplaySize[] FullscreenModes
    {
        get
        {
            List<DisplaySize> sizes = [];

            if (window != IntPtr.Zero)
            {
                var display = SDL.GetDisplayForWindow(window);
                SDL.DisplayMode[]? modes = SDL.GetFullscreenDisplayModes(display, out var count);

                if (modes != null)
                {
                    foreach (SDL.DisplayMode mode in modes)
                    {
                        if (mode.W < PresentationWidth || mode.H < FrameHeight)
                            continue;

                        DisplaySize size = new DisplaySize(
                            mode.W,
                            mode.H,
                            RefreshRateOf(mode)
                            );

                        if (!sizes.Contains(size))
                            sizes.Add(size);

                    }
                }

                if (sizes.Count == 0)
                {
                    SDL.DisplayMode? desktop = SDL.GetDesktopDisplayMode(display);

                    if (desktop.HasValue)
                    {
                        sizes.Add(new DisplaySize(
                            desktop.Value.W,
                            desktop.Value.H,
                            RefreshRateOf(desktop.Value)
                            ));
                    }
                }
            }

            sizes.Sort(CompareFullscreenModes);

            return [.. sizes];
        }
    }

    public void Reset(bool fullscreen, int width, int height, int refreshRate = 0)
    {
        this.fullscreen = fullscreen;

        AppSettings.Current.Fullscreen = fullscreen;

        if (fullscreen)
        {
            var display = SDL.GetDisplayForWindow(window);
            SDL.DisplayMode[]? modes = SDL.GetFullscreenDisplayModes(display, out var count);
            SDL.DisplayMode? selected = null;

            if (modes != null)
            {
                foreach (SDL.DisplayMode mode in modes)
                {
                    if (mode.W != width || mode.H != height)
                        continue;

                    if (refreshRate > 0 && !MatchesRefreshRate(mode, refreshRate))
                        continue;

                    selected = mode;
                    break;
                }

                // The saved refresh rate may no longer be available. Fall
                // back to the requested resolution rather than refusing to
                // enter fullscreen at all.
                if (selected is null && refreshRate > 0)
                {
                    foreach (SDL.DisplayMode mode in modes)
                    {
                        if (mode.W == width && mode.H == height)
                        {
                            selected = mode;
                            break;
                        }
                    }
                }
            }

            if (selected.HasValue)
            {
                SDL.SetWindowFullscreenMode(window, selected.Value);
                AppSettings.Current.DisplayRefreshRate =
                    RefreshRateOf(selected.Value);
            }

            SDL.SetWindowFullscreen(window, true);
            SDL.HideCursor();

            AppSettings.Current.ResolutionWidth = width;
            AppSettings.Current.ResolutionHeight = height;
        }
        else
        {
            var (windowWidth, windowHeight) = FitWindowToFrame(
                VideoTiming.For(machine.VideoStandard),
                width,
                height);

            SDL.SetWindowFullscreen(window, false);
            SDL.SetWindowSize(window, windowWidth, windowHeight);
            SDL.ShowCursor();

            AppSettings.Current.ResolutionWidth = windowWidth;
            AppSettings.Current.ResolutionHeight = windowHeight;
            SaveWindowSize(machine.VideoStandard, windowWidth, windowHeight);

            windowedScale = Math.Max(
                1,
                Math.Min(
                    windowWidth / PresentationWidth,
                    windowHeight / FrameHeight));
        }

        ApplyAspectRatio();
        ApplyPresentation();

        // Some drivers reset renderer state when the fullscreen mode
        // changes, so reapply the requested VSync setting after the switch.
        ApplyVSync(AppSettings.Current.VSync);

        lock (frameLock)
        {
            resetLastBuffer();
            frameDirty = true;
        }

        lastStandard = machine.VideoStandard;
    }

    /// <summary>
    /// Rebuilds the frame texture and logical presentation after the machine
    /// switched between PAL and NTSC. Only the host thread may call this; the
    /// previous picture is no longer valid.
    /// </summary>
    public void ApplyVideoStandard()
    {
        if (disposed)
            return;

        VideoStandard standard = machine.VideoStandard;
        var previousWidth = 0;
        var previousHeight = 0;

        // Remember the window the outgoing standard was using before the
        // picture changes shape, so switching back restores it.
        if (!fullscreen)
        {
            SaveCurrentWindow(
                lastStandard,
                out previousWidth,
                out previousHeight);
        }

        var width = PresentationWidth;
        var height = FrameHeight;

        if (frameTexture != IntPtr.Zero)
        {
            SDL.DestroyTexture(frameTexture);
            frameTexture = IntPtr.Zero;
        }

        createFrameTexture();

        SDL.SetWindowMinimumSize(window, width, height);
        ApplyAspectRatio();

        lock (frameLock)
        {
            AllocateFrameBuffers();
            resetLastBuffer();
            frameDirty = true;
        }

        configureFonts();

        // Each standard keeps its own windowed size, so switching back to a
        // standard restores the window it was last shown in. The first time
        // a standard is used, keep the outgoing integer zoom instead of
        // jumping to a default size.
        if (!fullscreen)
        {
            var savedWidth = SavedWindowWidth(standard);
            var savedHeight = SavedWindowHeight(standard);
            var hasSavedSize = savedWidth > 0 && savedHeight > 0;
            var zoom = windowedScale;

            if (previousWidth > 0 && previousHeight > 0)
            {
                VideoTiming previousTiming = VideoTiming.For(lastStandard);

                zoom = Math.Max(
                    1,
                    Math.Min(
                        previousWidth / previousTiming.PresentationWidth,
                        previousHeight / previousTiming.VisibleLines));
            }

            int windowWidth;
            int windowHeight;

            if (hasSavedSize)
            {
                (windowWidth, windowHeight) = FitWindowToFrame(
                    VideoTiming.For(standard),
                    savedWidth,
                    savedHeight);
            }
            else
            {
                windowWidth = width * zoom;
                windowHeight = height * zoom;
            }

            SDL.SetWindowSize(window, windowWidth, windowHeight);

            AppSettings.Current.ResolutionWidth = windowWidth;
            AppSettings.Current.ResolutionHeight = windowHeight;
            SaveWindowSize(standard, windowWidth, windowHeight);

            windowedScale = Math.Max(
                1,
                Math.Min(windowWidth / width, windowHeight / height));
        }

        ApplyPresentation();
        lastStandard = standard;
    }

    private void createFrameTexture()
    {
        frameTexture = SDL.CreateTexture(
            renderer,
            SDL.PixelFormat.ARGB8888,
            SDL.TextureAccess.Streaming,
            FrameWidth,
            FrameHeight
            );

        if (frameTexture == IntPtr.Zero)
            throw new InvalidOperationException(
                "Unable to create the SDL texture: " + SDL.GetError()
                );

        SDL.SetTextureScaleMode(frameTexture, SDL.ScaleMode.Nearest);
        SDL.SetTextureBlendMode(frameTexture, SDL.BlendMode.None);
    }

    private int CompareFullscreenModes(DisplaySize a, DisplaySize b)
    {
        VideoStandard standard = machine.Vic.VideoStandard;

        if (a.IsPreferred(standard) != b.IsPreferred(standard))
            return a.IsPreferred(standard) ? -1 : 1;

        if (a.Width != b.Width)
            return b.Width.CompareTo(a.Width);

        if (a.Height != b.Height)
            return b.Height.CompareTo(a.Height);

        return b.RefreshRate.CompareTo(a.RefreshRate);
    }

    private static int RefreshRateOf(SDL.DisplayMode mode)
    {
        return Math.Max(0, Convert.ToInt32(mode.RefreshRate));
    }

    private static bool MatchesRefreshRate(SDL.DisplayMode mode, int refreshRate)
    {
        return Math.Abs(RefreshRateOf(mode) - refreshRate) <= 1;
    }

    private void statusTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        statusEnabled = false;
    }

    private void fpsTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        updateFpsText();
    }

    private void updateFpsText()
    {
        fpsText = "CPU Speed: " + machine.SpeedPercentage + "%";
    }

    public void DisplayStatus(string message)
    {
        if (disposed)
            return;

        statusText = message;
        statusEnabled = true;

        try
        {
            statusTimer?.Stop();
            statusTimer?.Start();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void ResetBuffer()
    {
        lock (frameLock)
        {
            resetLastBuffer();
        }

        lock (overlayLock)
        {
            menuVisible = false;
            errorVisible = false;
        }
    }

    private void resetLastBuffer()
    {
        for (var y = 0; y < lastRender.GetLength(1); y++)
            for (var x = 0; x < lastRender.GetLength(0); x++)
                lastRender[x, y] = invalidateColor;
    }

    public void ResetDrawState()
    {
        lock (frameLock)
        {
            resetLastBuffer();
            frameDirty = true;
        }
    }

    public bool DisplayFPS
    {
        get
        {
            return showFPS;
        }
        set
        {
            if (value == showFPS) return;

            showFPS = value;
            AppSettings.Current.ShowSpeed = value;

            updateFpsText();

            if (fpsTimer is not null)
                fpsTimer.Enabled = showFPS;
        }
    }

    /// <summary>
    /// Copies a completed emulated frame into the CPU-side frame buffer.
    /// This runs on the host thread and never touches SDL from the
    /// emulation thread.
    /// </summary>
    public void Draw()
    {
        if (disposed)
            return;

        var width = FrameWidth;
        var height = FrameHeight;

        // A video-standard switch from the network thread lands between
        // frames; ApplyVideoStandard rebuilds these buffers on this thread.
        if (
            screenSnapshot.GetLength(0) != width ||
            screenSnapshot.GetLength(1) != height)
        {
            return;
        }

        machine.Vic.CopyScreenTo(screenSnapshot);

        lock (frameLock)
        {
            var changed = false;

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    RgbColor color = screenSnapshot[x, y];

                    if (lastRender[x, y] == color)
                        continue;

                    lastRender[x, y] = color;
                    framePixels[y * width + x] = color.ToArgb();
                    changed = true;
                }

            if (changed)
                frameDirty = true;
        }
    }

    /// <summary>
    /// Uploads the frame and draws it with the overlays. This runs on the
    /// main thread only.
    /// </summary>
    public void Present()
    {
        if (disposed)
            return;

        bool upload;

        lock (frameLock)
        {
            upload = frameDirty;

            if (upload)
                frameDirty = !uploadFrame();
        }

        SDL.SetRenderDrawColor(renderer, 0, 0, 0, 255);
        SDL.RenderClear(renderer);

        // The texture is the VIC's visible frame, borders included. It is
        // stretched horizontally to the aspect-corrected logical width;
        // overlay coordinates are scaled the same way by fillRect/drawText.
        SDL.FRect destination = new SDL.FRect
        {
            X = 0,
            Y = 0,
            W = PresentationWidth,
            H = FrameHeight
        };

        SDL.RenderTexture(renderer, frameTexture, IntPtr.Zero, in destination);

        drawScanlines();
        drawOverlay();
        drawMenuHint();

        SDL.RenderPresent(renderer);
    }

    private void drawScanlines()
    {
        if (!Scanlines)
            return;

        SDL.SetRenderDrawBlendMode(renderer, SDL.BlendMode.Blend);
        SDL.SetRenderDrawColor(renderer, 0, 0, 0, 48);

        for (var y = 1; y < FrameHeight; y += 2)
        {
            SDL.FRect line = new SDL.FRect
            {
                X = 0,
                Y = y,
                W = PresentationWidth,
                H = 1
            };

            SDL.RenderFillRect(renderer, in line);
        }
    }

    private bool uploadFrame()
    {
        IntPtr pixels;
        int pitch;

        if (!SDL.LockTexture(frameTexture, IntPtr.Zero, out pixels, out pitch))
            return false;

        var width = FrameWidth;
        var height = FrameHeight;

        // The picture changed size before ApplyVideoStandard rebuilt the
        // frame buffer; skip this upload instead of reading past its end.
        if (framePixels.Length < width * height)
        {
            SDL.UnlockTexture(frameTexture);
            return false;
        }

        try
        {
            for (var y = 0; y < height; y++)
                Marshal.Copy(
                    framePixels,
                    y * width,
                    IntPtr.Add(pixels, y * pitch),
                    width
                    );
        }
        finally
        {
            SDL.UnlockTexture(frameTexture);
        }

        return true;
    }

    public void DrawMenu(
        List<MenuList> menu,
        int selectedItem,
        int currentPosition,
        string? hint = null)
    {
        if (disposed)
            return;

        MenuEntry[] entries = new MenuEntry[menu.Count];

        for (var i = 0; i < menu.Count; i++)
            entries[i] = new MenuEntry(menu[i].Text, menu[i].Level);

        lock (overlayLock)
        {
            menuEntries = entries;
            menuSelected = selectedItem;
            menuPosition = currentPosition;
            menuHint = hint ?? string.Empty;
            menuVisible = true;
            errorVisible = false;
        }
    }

    public void DrawError(string error)
    {
        if (disposed)
            return;

        List<string> lines = [];
        lines.Add("Error:");

        var remaining = error ?? string.Empty;

        while (remaining.Length > 0)
        {
            var length = Math.Min(33, remaining.Length);
            lines.Add(remaining.Substring(0, length));
            remaining = remaining.Substring(length);
        }

        lock (overlayLock)
        {
            errorLines = [.. lines];
            errorVisible = true;
            menuVisible = false;
        }
    }

    private void drawOverlay()
    {
        string status;
        bool statusOn;
        string fps;
        bool fpsOn;
        MenuEntry[] menu;
        int selected;
        int position;
        bool menuOn;
        string[]? error;
        bool errorOn;

        lock (overlayLock)
        {
            status = statusText;
            statusOn = statusEnabled;
            fps = fpsText;
            fpsOn = showFPS;
            menu = menuEntries;
            selected = menuSelected;
            position = menuPosition;
            menuOn = menuVisible;
            error = errorLines;
            errorOn = errorVisible;
        }

        if (errorOn && error != null)
        {
            drawErrorPanel(error);
            return;
        }

        if (menuOn && menu != null)
        {
            drawMenuPanel(menu, selected, position);
            return;
        }

        if (fpsOn && !string.IsNullOrEmpty(fps))
            drawFps(fps);

        if (statusOn && !string.IsNullOrEmpty(status))
            drawStatus(status);
    }

    private void drawStatus(string text)
    {
        var x = 10;
        var y = machine.Vic.VisibleHeight - statusHeight - 10;

        drawText(statusFont, text, new SDL.Color { R = 0, G = 100, B = 0, A = 255 }, x, y);
        drawText(statusFont, text, new SDL.Color { R = 144, G = 238, B = 144, A = 255 }, x + 1, y + 1);
    }

    private void drawFps(string text)
    {
        var x = FrameWidth - 100;
        var y = machine.Vic.VisibleHeight - fpsHeight - 10;

        drawText(fpsFont, text, new SDL.Color { R = 0, G = 0, B = 0, A = 255 }, x, y);
        drawText(fpsFont, text, new SDL.Color { R = 255, G = 255, B = 255, A = 255 }, x + 1, y + 1);
    }

    private void drawMenuHint()
    {
        string hint;
        bool visible;

        lock (overlayLock)
        {
            hint = menuHint;
            visible = menuVisible && !errorVisible;
        }

        if (!visible || string.IsNullOrEmpty(hint))
            return;

        // A footer band across the bottom border keeps the shortcuts
        // readable without crowding the menu rows above it.
        var height = machine.Vic.VisibleHeight;
        var bandY = height - menuHeight - 6;

        fillRect(0, bandY, FrameWidth, height - bandY, paletteColor(6));

        drawText(
            menuFont,
            hint,
            paletteColor(14),
            machine.Vic.DisplayLeft + 10,
            bandY + 3
            );
    }

    private void drawMenuPanel(MenuEntry[] menu, int selected, int position)
    {
        SDL.Color foreColor = paletteColor(14);
        SDL.Color backColor = paletteColor(6);
        var height = machine.Vic.VisibleHeight;
        var left = machine.Vic.DisplayLeft;
        var top = machine.Vic.DisplayTop;
        var displayWidth = machine.Vic.DisplayWidth;
        var displayHeight = machine.Vic.DisplayHeight;

        // The panel is a C64 screen: the border follows the active
        // standard (10 lines above and 25 below on NTSC, 42/42 on PAL)
        // and the dark area is the 320 x 200 display window.
        fillRect(0, 0, FrameWidth, height, foreColor);
        fillRect(left, top, displayWidth, displayHeight, backColor);

        var maxItems = Menu.MaxItemsFor(height);
        var rows = Math.Min(maxItems, menu.Length - position);
        var rowStep = menuHeight + 10;
        var firstY = top + 10;
        var lastY = top + displayHeight;
        var hasScrollbar = menu.Length > maxItems;
        const int scrollbarWidth = 4;
        var scrollbarX = left + displayWidth - scrollbarWidth - 2;
        var textRight = hasScrollbar
            ? scrollbarX - 4
            : left + displayWidth - 10;

        // Nine rows fit the 200-line display window in both standards;
        // compress the spacing only if a longer submenu would overflow.
        if (rows > 1 && firstY + (rows - 1) * rowStep + menuHeight > lastY)
        {
            rowStep = Math.Max(
                menuHeight,
                (lastY - firstY - menuHeight) / (rows - 1)
                );
        }

        for (
            var i = position;
            i < position + maxItems && i < menu.Length;
            i++
            )
        {
            float itemX = left + 10 + menu[i].Level * 20;
            float itemY = firstY + (i - position) * rowStep;
            float availableWidth = textRight - itemX;

            if (availableWidth <= 0)
                continue;

            TextTexture? text = getTextTexture(menuFont, menu[i].Text, foreColor);

            if (text == null)
                continue;

            if (i == selected)
            {
                var highlightWidth = Math.Min(
                    text.Width + 2,
                    availableWidth + 1);

                fillRect(itemX - 1, itemY, highlightWidth, menuHeight, foreColor);
                drawTextClipped(
                    menuFont,
                    menu[i].Text,
                    backColor,
                    itemX,
                    itemY,
                    availableWidth);
            }
            else
            {
                drawTextClipped(
                    menuFont,
                    menu[i].Text,
                    foreColor,
                    itemX,
                    itemY,
                    availableWidth);
            }
        }

        if (hasScrollbar)
        {
            drawMenuScrollbar(
                menu.Length,
                maxItems,
                position,
                scrollbarX,
                firstY,
                scrollbarWidth,
                displayHeight - 20);
        }
    }

    private void drawMenuScrollbar(
        int itemCount,
        int visibleItems,
        int position,
        float x,
        float y,
        float width,
        float height)
    {
        var maxPosition = Math.Max(1, itemCount - visibleItems);
        var clampedPosition = Math.Clamp(position, 0, maxPosition);
        var thumbHeight = Math.Max(
            12f,
            height * visibleItems / itemCount);
        var thumbTravel = Math.Max(0, height - thumbHeight);
        var thumbY = y + thumbTravel * clampedPosition / maxPosition;

        fillRect(x, y, width, height, paletteColor(11));
        fillRect(x, thumbY, width, thumbHeight, paletteColor(14));
    }

    private void drawErrorPanel(string[] lines)
    {
        SDL.Color foreColor = paletteColor(14);
        SDL.Color backColor = paletteColor(6);
        var height = machine.Vic.VisibleHeight;
        var left = machine.Vic.DisplayLeft;
        var top = machine.Vic.DisplayTop;
        var displayWidth = machine.Vic.DisplayWidth;
        var displayHeight = machine.Vic.DisplayHeight;

        fillRect(0, 0, FrameWidth, height, foreColor);
        fillRect(left, top, displayWidth, displayHeight, backColor);

        float lineY = top + 10;

        for (var i = 0; i < lines.Length; i++)
        {
            drawText(menuFont, lines[i], foreColor, left + 10, lineY);
            lineY += menuHeight + 5;
        }
    }

    private SDL.Color paletteColor(byte value)
    {
        RgbColor color = C64Palette.For(machine.VideoStandard).Get(value);
        return new SDL.Color { R = color.R, G = color.G, B = color.B, A = 255 };
    }

    /// <summary>
    /// Maps a native VIC x coordinate to the aspect-corrected logical
    /// canvas. Vertical coordinates and sizes are unchanged.
    /// </summary>
    private float ScaleX(float value)
    {
        return value * PresentationWidth / FrameWidth;
    }

    private void fillRect(float x, float y, float width, float height, SDL.Color color)
    {
        x = ScaleX(x);
        width = ScaleX(width);

        SDL.SetRenderDrawBlendMode(renderer, SDL.BlendMode.Blend);
        SDL.SetRenderDrawColor(renderer, color.R, color.G, color.B, 255);

        SDL.FRect rect = new SDL.FRect { X = x, Y = y, W = width, H = height };

        SDL.RenderFillRect(renderer, in rect);
    }

    private void drawText(IntPtr font, string text, SDL.Color color, float x, float y)
    {
        TextTexture? texture = getTextTexture(font, text, color);

        if (texture == null)
            return;

        SDL.FRect destination = new SDL.FRect
        {
            X = ScaleX(x),
            Y = y,
            W = ScaleX(texture.Width),
            H = texture.Height
        };

        SDL.RenderTexture(renderer, texture.Texture, IntPtr.Zero, in destination);
    }

    private void drawTextClipped(
        IntPtr font,
        string text,
        SDL.Color color,
        float x,
        float y,
        float maxWidth)
    {
        if (maxWidth <= 0)
            return;

        TextTexture? texture = getTextTexture(font, text, color);

        if (texture == null)
            return;

        var width = Math.Min(texture.Width, maxWidth);

        if (width <= 0)
            return;

        SDL.FRect source = new SDL.FRect
        {
            X = 0,
            Y = 0,
            W = width,
            H = texture.Height
        };
        SDL.FRect destination = new SDL.FRect
        {
            X = ScaleX(x),
            Y = y,
            W = ScaleX(width),
            H = texture.Height
        };

        SDL.RenderTexture(renderer, texture.Texture, in source, in destination);
    }

    private TextTexture? getTextTexture(IntPtr font, string text, SDL.Color color)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var key =
            font.ToInt64().ToString() + "|" +
            color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "|" +
            text;

        if (textCache.TryGetValue(key, out TextTexture? cached) && cached is not null)
            return cached;

        var length = Encoding.UTF8.GetByteCount(text);

        var surface = TTF.RenderTextSolid(
            font,
            text,
            (UIntPtr)length,
            color
            );

        if (surface == IntPtr.Zero)
            return null;

        var texture = SDL.CreateTextureFromSurface(renderer, surface);

        int width;
        int height;

        TTF.GetStringSize(font, text, (UIntPtr)length, out width, out height);

        SDL.DestroySurface(surface);

        if (texture == IntPtr.Zero)
            return null;

        SDL.SetTextureScaleMode(texture, SDL.ScaleMode.Nearest);
        SDL.SetTextureBlendMode(texture, SDL.BlendMode.Blend);

        TextTexture result = new TextTexture
        {
            Texture = texture,
            Width = width,
            Height = height
        };

        textCache[key] = result;

        return result;
    }

    public void Kill()
    {
        if (disposed)
            return;

        disposed = true;

        if (statusTimer != null)
        {
            statusTimer.Stop();
            statusTimer.Dispose();
        }

        if (fpsTimer != null)
        {
            fpsTimer.Stop();
            fpsTimer.Dispose();
        }

        clearTextCache();

        if (statusFont != IntPtr.Zero)
            TTF.CloseFont(statusFont);
        if (fpsFont != IntPtr.Zero)
            TTF.CloseFont(fpsFont);
        if (menuFont != IntPtr.Zero)
            TTF.CloseFont(menuFont);

        statusFont = IntPtr.Zero;
        fpsFont = IntPtr.Zero;
        menuFont = IntPtr.Zero;

        if (frameTexture != IntPtr.Zero)
        {
            SDL.DestroyTexture(frameTexture);
            frameTexture = IntPtr.Zero;
        }

        if (renderer != IntPtr.Zero)
        {
            SDL.DestroyRenderer(renderer);
            renderer = IntPtr.Zero;
        }

        if (window != IntPtr.Zero)
        {
            SDL.DestroyWindow(window);
            window = IntPtr.Zero;
        }

        if (fontDataHandle.IsAllocated)
            fontDataHandle.Free();
    }

    public void Dispose()
    {
        Kill();
    }
}
