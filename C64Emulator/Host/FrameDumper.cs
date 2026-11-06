using System;
using System.IO;
using C64Emulator.Core;
using C64Emulator.Video;

namespace C64Emulator.Host;

/// <summary>
/// Captures completed VIC frames during a headless run. Frame numbers come
/// from the machine's frame counter, so the same media and cycle budget
/// produce the same set of files regardless of host speed.
/// </summary>
internal sealed class FrameDumper : IDisposable
{
    private readonly C64Machine machine;
    private readonly string? screenshotPath;
    private readonly string? framesDirectory;
    private readonly int interval;
    private readonly RgbColor[,] buffer;
    private long completedFrames;
    private bool disposed;

    public FrameDumper(C64Machine machine, CommandLineOptions options)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        ArgumentNullException.ThrowIfNull(options);

        screenshotPath = options.ScreenshotPath;
        framesDirectory = options.FramesDirectory;
        interval = options.FrameInterval;
        buffer = new RgbColor[machine.Vic.VisibleWidth, machine.Vic.VisibleHeight];

        if (framesDirectory is not null)
            Directory.CreateDirectory(framesDirectory);

        if (screenshotPath is not null)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(screenshotPath));

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }

        machine.FrameCompleted += OnFrameCompleted;
    }

    /// <summary>Frames written to disk, including the final screenshot.</summary>
    public long FramesWritten { get; private set; }

    /// <summary>
    /// Saves the last completed frame. Called once at the end of the run, so
    /// a screenshot always exists even when no periodic frame was due.
    /// </summary>
    public void SaveFinalScreenshot()
    {
        if (screenshotPath is null)
            return;

        Capture();
        FrameWriter.SaveBmp(screenshotPath, buffer);
        FramesWritten++;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        machine.FrameCompleted -= OnFrameCompleted;
    }

    private void OnFrameCompleted()
    {
        completedFrames++;

        if (framesDirectory is null || (completedFrames - 1) % interval != 0)
            return;

        Capture();
        FrameWriter.SaveBmp(
            Path.Combine(framesDirectory, "frame_" + completedFrames.ToString("D6") + ".bmp"),
            buffer);
        FramesWritten++;
    }

    private void Capture()
    {
        machine.Vic.CopyScreenTo(buffer);
    }
}
