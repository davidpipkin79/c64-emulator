using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using C64Emulator.Properties;
using SDL3;

namespace C64Emulator
{
    internal sealed class Renderer : IDisposable
    {
        public const int C64WIDTH = 403;
        public const int C64HEIGHT = 284;
        private const int C64BORDER = 42;

        private readonly C64Machine machine;
        private readonly object frameLock = new object();
        private readonly object overlayLock = new object();
        private readonly int[] framePixels = new int[C64WIDTH * C64HEIGHT];
        private readonly RgbColor[,] lastRender = new RgbColor[C64WIDTH, C64HEIGHT];
        private readonly RgbColor[,] screenSnapshot = new RgbColor[C64WIDTH, C64HEIGHT];
        private readonly RgbColor invalidateColor = new RgbColor(1, 1, 1);
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
        private volatile bool showFPS;

        private volatile bool statusEnabled;
        private volatile string statusText = string.Empty;
        private volatile string fpsText = string.Empty;

        private MenuEntry[] menuEntries = new MenuEntry[0];
        private int menuSelected;
        private int menuPosition;
        private bool menuVisible;

        private string[]? errorLines;
        private bool errorVisible;

        private sealed class TextTexture
        {
            public IntPtr Texture;
            public int Width;
            public int Height;
        }

        private readonly struct MenuEntry
        {
            public MenuEntry(string text, int level)
            {
                Text = text;
                Level = level;
            }

            public string Text { get; }

            public int Level { get; }
        }

        public Renderer(C64Machine machine)
        {
            this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
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

        private void Initialize()
        {
            if (
                !SDL.CreateWindowAndRenderer(
                    "Pipkinsoft C64 Emulator",
                    Math.Max(C64WIDTH, AppSettings.Current.ResolutionWidth),
                    Math.Max(C64HEIGHT, AppSettings.Current.ResolutionHeight),
                    SDL.WindowFlags.Resizable,
                    out window,
                    out renderer
                    )
                )
                throw new InvalidOperationException(
                    "Unable to create the SDL window: " + SDL.GetError()
                    );

            SDL.SetWindowMinimumSize(window, C64WIDTH, C64HEIGHT);
            SDL.SetRenderLogicalPresentation(
                renderer,
                C64WIDTH,
                C64HEIGHT,
                SDL.RendererLogicalPresentation.IntegerScale
                );

            ApplyVSync(AppSettings.Current.VSync);

            frameTexture = SDL.CreateTexture(
                renderer,
                SDL.PixelFormat.ARGB8888,
                SDL.TextureAccess.Streaming,
                C64WIDTH,
                C64HEIGHT
                );

            if (frameTexture == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Unable to create the SDL texture: " + SDL.GetError()
                    );

            SDL.SetTextureScaleMode(frameTexture, SDL.ScaleMode.Nearest);
            SDL.SetTextureBlendMode(frameTexture, SDL.BlendMode.None);

            loadWindowIcon();

            statusFont = openFont(15);
            fpsFont = openFont(8);
            menuFont = openFont(12);

            statusHeight = TTF.GetFontHeight(statusFont);
            fpsHeight = TTF.GetFontHeight(fpsFont);
            menuHeight = TTF.GetFontHeight(menuFont);

            statusTimer = new System.Timers.Timer(3000);
            statusTimer.AutoReset = false;
            statusTimer.Elapsed += statusTimer_Elapsed;

            fpsTimer = new System.Timers.Timer(1000);
            fpsTimer.Elapsed += fpsTimer_Elapsed;

            showFPS = AppSettings.Current.ShowSpeed;
            fpsTimer.Enabled = showFPS;

            Reset(
                AppSettings.Current.Fullscreen,
                AppSettings.Current.ResolutionWidth,
                AppSettings.Current.ResolutionHeight,
                AppSettings.Current.DisplayRefreshRate
                );
        }

        private void loadWindowIcon()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "c64icon.bmp");

            if (!File.Exists(path))
                return;

            IntPtr icon = SDL.LoadBMP(path);

            if (icon == IntPtr.Zero)
                return;

            SDL.SetWindowIcon(window, icon);
            SDL.DestroySurface(icon);
        }

        private IntPtr openFont(int size)
        {
            IntPtr io = SDL.IOFromConstMem(
                fontDataHandle.AddrOfPinnedObject(),
                (UIntPtr)fontData.Length
                );

            if (io == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Unable to open the embedded font: " + SDL.GetError()
                    );

            IntPtr font = TTF.OpenFontIO(io, true, size);

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
            bool changed = SDL.SetRenderVSync(renderer, enabled ? 1 : 0);

            if (SDL.GetRenderVSync(renderer, out int vsync))
                VSyncEnabled = vsync != 0;
            else if (changed)
                VSyncEnabled = enabled;
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
                List<DisplaySize> sizes = new List<DisplaySize>();

                if (window != IntPtr.Zero)
                {
                    uint display = SDL.GetDisplayForWindow(window);
                    SDL.DisplayMode[]? modes = SDL.GetFullscreenDisplayModes(display, out int count);

                    if (modes != null)
                    {
                        foreach (SDL.DisplayMode mode in modes)
                        {
                            if (mode.W < C64WIDTH || mode.H < C64HEIGHT)
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

                return sizes.ToArray();
            }
        }

        public void Reset(bool fullscreen, int width, int height, int refreshRate = 0)
        {
            this.fullscreen = fullscreen;

            AppSettings.Current.Fullscreen = fullscreen;
            AppSettings.Current.ResolutionWidth = width;
            AppSettings.Current.ResolutionHeight = height;

            if (fullscreen)
            {
                uint display = SDL.GetDisplayForWindow(window);
                SDL.DisplayMode[]? modes = SDL.GetFullscreenDisplayModes(display, out int count);
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
            }
            else
            {
                SDL.SetWindowFullscreen(window, false);
                SDL.SetWindowSize(
                    window,
                    Math.Max(C64WIDTH, width),
                    Math.Max(C64HEIGHT, height)
                    );
                SDL.ShowCursor();
            }

            // Some drivers reset renderer state when the fullscreen mode
            // changes, so reapply the requested VSync setting after the switch.
            ApplyVSync(AppSettings.Current.VSync);

            lock (frameLock)
            {
                resetLastBuffer();
                frameDirty = true;
            }
        }

        private static int CompareFullscreenModes(DisplaySize a, DisplaySize b)
        {
            if (a.IsFiftyHertz != b.IsFiftyHertz)
                return a.IsFiftyHertz ? -1 : 1;

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
            for (int y = 0; y < C64HEIGHT; y++)
                for (int x = 0; x < C64WIDTH; x++)
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

            machine.Vic.CopyScreenTo(screenSnapshot);

            lock (frameLock)
            {
                bool changed = false;

                for (int y = 0; y < C64HEIGHT; y++)
                    for (int x = 0; x < C64WIDTH; x++)
                    {
                        RgbColor color = screenSnapshot[x, y];

                        if (lastRender[x, y] == color)
                            continue;

                        lastRender[x, y] = color;
                        framePixels[y * C64WIDTH + x] = color.ToArgb();
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
            SDL.RenderTexture(renderer, frameTexture, IntPtr.Zero, IntPtr.Zero);

            drawOverlay();

            SDL.RenderPresent(renderer);
        }

        private bool uploadFrame()
        {
            IntPtr pixels;
            int pitch;

            if (!SDL.LockTexture(frameTexture, IntPtr.Zero, out pixels, out pitch))
                return false;

            try
            {
                for (int y = 0; y < C64HEIGHT; y++)
                    Marshal.Copy(
                        framePixels,
                        y * C64WIDTH,
                        IntPtr.Add(pixels, y * pitch),
                        C64WIDTH
                        );
            }
            finally
            {
                SDL.UnlockTexture(frameTexture);
            }

            return true;
        }

        public void DrawMenu(List<MenuList> menu, int selectedItem, int currentPosition)
        {
            if (disposed)
                return;

            MenuEntry[] entries = new MenuEntry[menu.Count];

            for (int i = 0; i < menu.Count; i++)
                entries[i] = new MenuEntry(menu[i].Text, menu[i].Level);

            lock (overlayLock)
            {
                menuEntries = entries;
                menuSelected = selectedItem;
                menuPosition = currentPosition;
                menuVisible = true;
                errorVisible = false;
            }
        }

        public void DrawError(string error)
        {
            if (disposed)
                return;

            List<string> lines = new List<string>();
            lines.Add("Error:");

            string remaining = error ?? string.Empty;

            while (remaining.Length > 0)
            {
                int length = Math.Min(33, remaining.Length);
                lines.Add(remaining.Substring(0, length));
                remaining = remaining.Substring(length);
            }

            lock (overlayLock)
            {
                errorLines = lines.ToArray();
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
            int x = 10;
            int y = C64HEIGHT - statusHeight - 10;

            drawText(statusFont, text, new SDL.Color { R = 0, G = 100, B = 0, A = 255 }, x, y);
            drawText(statusFont, text, new SDL.Color { R = 144, G = 238, B = 144, A = 255 }, x + 1, y + 1);
        }

        private void drawFps(string text)
        {
            int x = C64WIDTH - 100;
            int y = C64HEIGHT - fpsHeight - 10;

            drawText(fpsFont, text, new SDL.Color { R = 0, G = 0, B = 0, A = 255 }, x, y);
            drawText(fpsFont, text, new SDL.Color { R = 255, G = 255, B = 255, A = 255 }, x + 1, y + 1);
        }

        private void drawMenuPanel(MenuEntry[] menu, int selected, int position)
        {
            SDL.Color foreColor = paletteColor(14);
            SDL.Color backColor = paletteColor(6);

            fillRect(0, 0, C64WIDTH, C64HEIGHT, foreColor);
            fillRect(
                C64BORDER,
                C64BORDER,
                C64WIDTH - C64BORDER * 2,
                C64HEIGHT - C64BORDER * 2,
                backColor
                );

            for (
                int i = position;
                i < position + Menu.MaxItems && i < menu.Length;
                i++
                )
            {
                float itemX = C64BORDER + 10 + menu[i].Level * 20;
                float itemY = C64BORDER + 10 + (i - position) * (menuHeight + 10);

                TextTexture? text = getTextTexture(menuFont, menu[i].Text, foreColor);

                if (text == null)
                    continue;

                if (i == selected)
                {
                    fillRect(itemX - 1, itemY, text.Width + 2, menuHeight, foreColor);
                    drawText(menuFont, menu[i].Text, backColor, itemX, itemY);
                }
                else
                {
                    drawText(menuFont, menu[i].Text, foreColor, itemX, itemY);
                }
            }
        }

        private void drawErrorPanel(string[] lines)
        {
            SDL.Color foreColor = new SDL.Color { R = 117, G = 161, B = 236, A = 255 };
            SDL.Color backColor = new SDL.Color { R = 65, G = 55, B = 205, A = 255 };

            fillRect(0, 0, C64WIDTH, C64HEIGHT, foreColor);
            fillRect(
                C64BORDER,
                C64BORDER,
                C64WIDTH - C64BORDER * 2,
                C64HEIGHT - C64BORDER * 2,
                backColor
                );

            float lineY = C64BORDER + 10;

            for (int i = 0; i < lines.Length; i++)
            {
                drawText(menuFont, lines[i], foreColor, C64BORDER + 10, lineY);
                lineY += menuHeight + 5;
            }
        }

        private static SDL.Color paletteColor(byte value)
        {
            RgbColor color = C64Palette.Get(value);
            return new SDL.Color { R = color.R, G = color.G, B = color.B, A = 255 };
        }

        private void fillRect(float x, float y, float width, float height, SDL.Color color)
        {
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
                X = x,
                Y = y,
                W = texture.Width,
                H = texture.Height
            };

            SDL.RenderTexture(renderer, texture.Texture, IntPtr.Zero, in destination);
        }

        private TextTexture? getTextTexture(IntPtr font, string text, SDL.Color color)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string key =
                font.ToInt64().ToString() + "|" +
                color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "|" +
                text;

            if (textCache.TryGetValue(key, out TextTexture? cached) && cached is not null)
                return cached;

            int length = Encoding.UTF8.GetByteCount(text);

            IntPtr surface = TTF.RenderTextSolid(
                font,
                text,
                (UIntPtr)length,
                color
                );

            if (surface == IntPtr.Zero)
                return null;

            IntPtr texture = SDL.CreateTextureFromSurface(renderer, surface);

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

            foreach (TextTexture texture in textCache.Values)
                if (texture.Texture != IntPtr.Zero)
                    SDL.DestroyTexture(texture.Texture);

            textCache.Clear();

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
}
