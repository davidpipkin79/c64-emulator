using System;
using System.IO;
using C64Emulator.Core;
using C64Emulator.Video;

namespace C64Emulator.Host;

internal readonly record struct HeadlessResult(
    int ExitCode,
    FrameDifference? Difference,
    int Tolerance);

/// <summary>
/// Runs the machine synchronously with no SDL window or audio device. Media
/// is started through <see cref="C64Machine.AutoStart"/>, the same code path
/// the file menu uses for Space.
/// </summary>
internal static class HeadlessRunner
{
    private const int DefaultRunSeconds = 10;
    private const int BootCycleBudget = 20_000_000;
    private const int BootCheckInterval = 50_000;

    public static int Run(CommandLineOptions options)
    {
        return Execute(options, Console.Out, Console.Error).ExitCode;
    }

    /// <summary>
    /// Runs one case and writes its diagnostics to the supplied writers. The
    /// regression suite uses this directly so several cases can share a
    /// process without touching the console.
    /// </summary>
    public static HeadlessResult Execute(
        CommandLineOptions options,
        TextWriter output,
        TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        C64Machine machine;

        try
        {
            machine = new C64Machine(RomSet.Load(AppContext.BaseDirectory));
        }
        catch (Exception ex) when (
            ex is IOException or
            InvalidDataException or
            ArgumentException)
        {
            error.WriteLine("c64: cannot load ROMs: " + ex.Message);
            return new HeadlessResult(
                ExitCodes.RomFailure,
                null,
                options.PixelTolerance);
        }

        if (machine.VideoStandard != options.VideoStandard)
            machine.VideoStandard = options.VideoStandard;

        // The constructor leaves the CPU in its power-on state; tests and the
        // windowed host both reset explicitly before running.
        machine.Cpu.Reset();

        if (options.MediaPath is not null && !File.Exists(options.MediaPath))
        {
            error.WriteLine("c64: media file not found: " + options.MediaPath);
            return new HeadlessResult(
                ExitCodes.MediaFailure,
                null,
                options.PixelTolerance);
        }

        // Fail before spending cycles when the reference itself is unusable.
        RgbColor[,]? expectedFrame = null;

        if (options.ExpectFramePath is not null)
        {
            try
            {
                expectedFrame = FrameReader.LoadBmp(options.ExpectFramePath);
            }
            catch (Exception ex) when (
                ex is IOException or
                InvalidDataException or
                ArgumentException or
                UnauthorizedAccessException or
                NotSupportedException)
            {
                error.WriteLine(
                    "c64: cannot read reference frame: " + ex.Message);
                return new HeadlessResult(
                    ExitCodes.ReferenceFailure,
                    null,
                    options.PixelTolerance);
            }
        }

        var captureFrames =
            options.ScreenshotPath is not null ||
            options.FramesDirectory is not null ||
            expectedFrame is not null;

        // Pixels are only composed when a picture is requested. Collisions
        // still run without rendering, so timing and $D01E/$D01F are
        // unaffected either way.
        machine.RenderFrames = captureFrames;
        machine.Vic.RenderPixels = captureFrames;

        using FrameDumper? dumper = captureFrames
            ? new FrameDumper(machine, options)
            : null;

        long bootCycles = 0;

        if (options.MediaPath is not null && NeedsBasicBoot(options.MediaPath))
        {
            bootCycles = BootToBasic(machine);

            // The KERNAL clears the keyboard buffer while it initialises, so
            // typing LOAD before READY would silently lose the command.
            if (bootCycles < 0)
            {
                error.WriteLine(
                    "c64: BASIC did not reach READY within " +
                    BootCycleBudget +
                    " cycles.");
                return new HeadlessResult(
                    ExitCodes.BootTimeout,
                    null,
                    options.PixelTolerance);
            }
        }

        if (options.MediaPath is not null)
        {
            try
            {
                output.WriteLine("c64: " + machine.AutoStart(options.MediaPath));
            }
            catch (Exception ex) when (
                ex is IOException or
                InvalidDataException or
                NotSupportedException or
                ArgumentException)
            {
                error.WriteLine("c64: cannot start media: " + ex.Message);
                return new HeadlessResult(
                    ExitCodes.MediaFailure,
                    null,
                    options.PixelTolerance);
            }
        }

        var limit = options.Cycles > 0
            ? options.Cycles
            : (long)machine.Vic.ClockHz * DefaultRunSeconds;

        long executed;

        try
        {
            executed = machine.RunCycles(limit);

            // The last completed frame, not a partially drawn one.
            dumper?.SaveFinalScreenshot();
        }
        catch (Exception ex)
        {
            error.WriteLine("c64: emulation failed: " + ex.Message);
            return new HeadlessResult(
                ExitCodes.RuntimeFailure,
                null,
                options.PixelTolerance);
        }

        output.WriteLine(
            "c64: boot={0} cycles, run={1} cycles, frames={2}, PC=${3:X4}",
            bootCycles,
            executed,
            machine.FrameCount,
            machine.Cpu.GetPC());

        FrameDifference? frameDifference = null;

        if (expectedFrame is not null)
        {
            RgbColor[,] actualFrame = new RgbColor[
                machine.Vic.VisibleWidth,
                machine.Vic.VisibleHeight];

            machine.Vic.CopyScreenTo(actualFrame);

            if (
                expectedFrame.GetLength(0) != actualFrame.GetLength(0) ||
                expectedFrame.GetLength(1) != actualFrame.GetLength(1))
            {
                error.WriteLine(
                    "c64: frame mismatch: reference is {0}x{1}, actual is {2}x{3}",
                    expectedFrame.GetLength(0),
                    expectedFrame.GetLength(1),
                    actualFrame.GetLength(0),
                    actualFrame.GetLength(1));
                return new HeadlessResult(
                    ExitCodes.FrameMismatch,
                    null,
                    options.PixelTolerance);
            }

            FrameDifference difference = FrameComparer.Compare(
                expectedFrame,
                actualFrame,
                options.PixelTolerance);

            frameDifference = difference;

            if (!difference.Matches)
            {
                error.WriteLine(
                    "c64: frame mismatch: {0} pixels differ above tolerance {1} " +
                    "(max channel delta {2}).",
                    difference.DifferingPixels,
                    options.PixelTolerance,
                    difference.MaxChannelDelta);

                error.WriteLine(
                    "c64: changed region x={0}..{1}, y={2}..{3} ({4}x{5}); " +
                    "first difference at ({6},{7}).",
                    difference.MinX,
                    difference.MaxX,
                    difference.MinY,
                    difference.MaxY,
                    difference.MaxX - difference.MinX + 1,
                    difference.MaxY - difference.MinY + 1,
                    difference.FirstX,
                    difference.FirstY);

                error.WriteLine(
                    "c64: use --screenshot to save the actual frame.");
                return new HeadlessResult(
                    ExitCodes.FrameMismatch,
                    difference,
                    options.PixelTolerance);
            }

            output.WriteLine(
                options.PixelTolerance > 0
                    ? $"c64: frame matches the reference within tolerance {options.PixelTolerance}."
                    : "c64: frame matches the reference.");
        }

        return new HeadlessResult(
            ExitCodes.Success,
            frameDifference,
            options.PixelTolerance);
    }

    private static bool NeedsBasicBoot(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() is ".d64" or ".t64" or ".tap";
    }

    /// <summary>
    /// Runs until the BASIC READY prompt appears. Returns the cycles spent,
    /// or -1 when the budget ran out or the CPU jammed.
    /// </summary>
    private static long BootToBasic(C64Machine machine)
    {
        long total = 0;

        while (total < BootCycleBudget)
        {
            var executed = machine.RunCycles(BootCheckInterval);

            if (executed == 0)
                return -1;

            total += executed;

            if (HasReadyPrompt(machine))
                return total;
        }

        return -1;
    }

    /// <summary>
    /// The KERNAL prints "READY." at the start of a screen row. Screen codes
    /// (not PETSCII) are used here. The screen is inspected without touching
    /// the open-bus latch.
    /// </summary>
    private static bool HasReadyPrompt(C64Machine machine)
    {
        ReadOnlySpan<byte> ready = [0x12, 0x05, 0x01, 0x04, 0x19];
        const int columns = 40;
        const int rows = 25;

        for (var row = 0; row < rows; row++)
        {
            var address = 0x0400 + (row * columns);

            for (var column = 0; column <= columns - ready.Length; column++)
            {
                var match = true;

                for (var i = 0; i < ready.Length; i++)
                {
                    if (machine.Memory.Inspect((ushort)(address + column + i)) != ready[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }
        }

        return false;
    }
}
