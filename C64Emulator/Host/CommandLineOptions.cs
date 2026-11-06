using System;
using System.Collections.Generic;
using System.Globalization;
using C64Emulator.Video;

namespace C64Emulator.Host;

internal sealed class CommandLineOptions
{
    /// <summary>
    /// Media to start. A path implies a headless run, because the windowed
    /// host has no way to accept a file before the KERNAL is at READY.
    /// </summary>
    public string? MediaPath { get; private init; }

    public bool Headless { get; private init; }

    /// <summary>
    /// Video standard for this run. Suite cases default to PAL.
    /// </summary>
    public VideoStandard VideoStandard { get; private init; } =
        C64Emulator.Video.VideoStandard.Pal;

    /// <summary>
    /// Requested emulated CPU cycles. Zero means the runner's default of ten
    /// emulated seconds for a media run.
    /// </summary>
    public long Cycles { get; private init; }

    /// <summary>Final completed frame written to this path, when set.</summary>
    public string? ScreenshotPath { get; private init; }

    /// <summary>Directory receiving completed frames, when set.</summary>
    public string? FramesDirectory { get; private init; }

    /// <summary>Save one frame out of this many when dumping.</summary>
    public int FrameInterval { get; private init; } = 25;

    /// <summary>
    /// Reference BMP the final frame must match. A mismatch exits with
    /// <see cref="ExitCodes.FrameMismatch"/>.
    /// </summary>
    public string? ExpectFramePath { get; private init; }

    /// <summary>
    /// JSON regression suite to run instead of a single case. Mutually
    /// exclusive with media and the single-run frame options.
    /// </summary>
    public string? SuitePath { get; private init; }

    /// <summary>
    /// With a suite, write reference frames that do not exist yet instead of
    /// failing those cases. Existing references are still compared.
    /// </summary>
    public bool RecordMissingReferences { get; private init; }

    /// <summary>
    /// With a suite, rewrite every reference frame, including existing ones.
    /// </summary>
    public bool RecordAllReferences { get; private init; }

    /// <summary>
    /// With a suite, run only the cases with these names. Empty means all.
    /// Matching is case-insensitive and order comes from the suite file.
    /// </summary>
    public IReadOnlyList<string> SelectedSuiteCases { get; private init; } =
        Array.Empty<string>();

    /// <summary>
    /// With a suite, print case names and exit without running the emulator.
    /// </summary>
    public bool ListCases { get; private init; }

    /// <summary>
    /// With --list-cases, emit case details as JSON instead of names.
    /// </summary>
    public bool ListCasesJson { get; private init; }

    /// <summary>
    /// With --json, print paths relative to the suite instead of absolute.
    /// </summary>
    public bool RelativePaths { get; private init; }

    /// <summary>
    /// With --json, report whether media and reference files exist.
    /// </summary>
    public bool CheckFiles { get; private init; }

    /// <summary>
    /// Per-channel difference allowed before a pixel counts as changed.
    /// Zero means the frame must match exactly.
    /// </summary>
    public int PixelTolerance { get; private init; }

    public bool ShowHelp { get; private init; }

    public const string Usage = """
        Usage: C64Emulator [options] [media]

          media             .prg, .d64, .t64, .tap or .crt to start headless
          --headless        run without a window or audio device
          --cycles N        stop after N emulated CPU cycles
          --screenshot F    save the final completed frame to F (.bmp)
          --frames DIR      save completed frames to DIR as .bmp files
          --every N         with --frames, save one frame out of N (default 25)
          --expect-frame F  exit 6 unless the final frame matches F
          --tolerance N     allow N per-channel delta (0-255, default 0)
          --video-standard  pal or ntsc for standalone runs (default pal)
          --suite F         run regression cases from a JSON suite
          --case NAME       with --suite, run only this case (repeatable)
          --list-cases      with --suite, print case names and exit
          --json            with --list-cases, print case details as JSON
          --relative-paths  with --json, print paths relative to the suite
          --check-files     with --json, report missing media/reference files
          --record-missing  with --suite, write absent reference frames
          --record-all      with --suite, rewrite every reference frame
          -h, --help        show this help

        A media path starts a headless run and stops after ten emulated seconds
        unless --cycles is given. Disks and tapes boot to BASIC first, then
        LOAD"*",8,1 and RUN are typed, exactly like the file menu's Space key.
        PRGs are injected, cartridges are mounted with a reset. Frames are
        24-bit BMP so automated checks need no image codec.
        --video-standard pal|ntsc selects the standalone VIC model (default pal).

        A suite preflights selected media and references before running cases.
        It exits 6 if any case fails.
        --case selects cases; --list-cases prints their names without running.
        --json prints cycles, media, reference and tolerance instead of names.
        --relative-paths with --json, prints paths relative to the suite.
        --check-files with --json, reports missing media and reference files.
        --record-missing writes missing references; --record-all rewrites all.

        Exit codes: 0 success, 1 runtime error, 2 usage, 3 media error,
                    4 ROM error, 5 BASIC boot timeout, 6 frame mismatch,
                    7 unreadable reference frame.
        """;

    public static CommandLineOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? media = null;
        string? screenshot = null;
        string? frames = null;
        string? expectFrame = null;
        string? suite = null;
        var recordMissing = false;
        var recordAll = false;
        var listCases = false;
        var json = false;
        var relativePaths = false;
        var checkFiles = false;
        var videoStandard = C64Emulator.Video.VideoStandard.Pal;
        var videoStandardGiven = false;
        var selectedCases = new List<string>();
        var frameInterval = 25;
        var intervalGiven = false;
        var tolerance = 0;
        var toleranceGiven = false;
        var headless = false;
        long cycles = 0;
        var help = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg is "-h" or "--help")
            {
                help = true;
                continue;
            }

            if (arg == "--headless")
            {
                headless = true;
                continue;
            }

            if (arg == "--cycles")
            {
                cycles = ParseCycles(RequireValue(args, ref i, "--cycles"));
                continue;
            }

            if (arg.StartsWith("--cycles=", StringComparison.Ordinal))
            {
                cycles = ParseCycles(arg["--cycles=".Length..]);
                continue;
            }

            if (arg == "--screenshot")
            {
                screenshot = RequireValue(args, ref i, "--screenshot");
                continue;
            }

            if (arg == "--frames")
            {
                frames = RequireValue(args, ref i, "--frames");
                continue;
            }

            if (arg == "--every")
            {
                frameInterval = ParsePositiveInt(
                    RequireValue(args, ref i, "--every"),
                    "--every");
                intervalGiven = true;
                continue;
            }

            if (arg == "--expect-frame")
            {
                expectFrame = RequireValue(args, ref i, "--expect-frame");
                continue;
            }

            if (arg == "--tolerance")
            {
                tolerance = ParseTolerance(RequireValue(args, ref i, "--tolerance"));
                toleranceGiven = true;
                continue;
            }

            if (arg == "--suite")
            {
                suite = RequireValue(args, ref i, "--suite");
                continue;
            }

            if (arg == "--case")
            {
                selectedCases.Add(RequireValue(args, ref i, "--case"));
                continue;
            }

            if (arg == "--list-cases")
            {
                listCases = true;
                continue;
            }

            if (arg == "--json")
            {
                json = true;
                continue;
            }

            if (arg == "--relative-paths")
            {
                relativePaths = true;
                continue;
            }

            if (arg == "--check-files")
            {
                checkFiles = true;
                continue;
            }

            if (arg == "--video-standard")
            {
                videoStandard = ParseVideoStandard(
                    RequireValue(args, ref i, "--video-standard"));
                videoStandardGiven = true;
                continue;
            }

            if (arg.StartsWith("--video-standard=", StringComparison.Ordinal))
            {
                videoStandard = ParseVideoStandard(
                    arg["--video-standard=".Length..]);
                videoStandardGiven = true;
                continue;
            }

            if (arg == "--record-missing")
            {
                recordMissing = true;
                continue;
            }

            if (arg == "--record-all")
            {
                recordAll = true;
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
                throw new ArgumentException("Unknown option: " + arg);

            if (media is not null)
                throw new ArgumentException("Only one media file can be started.");

            media = arg;
        }

        if (help)
            return new CommandLineOptions { ShowHelp = true };

        if (intervalGiven && frames is null)
            throw new ArgumentException("--every requires --frames.");

        if (toleranceGiven && expectFrame is null)
            throw new ArgumentException("--tolerance requires --expect-frame.");

        if (
            suite is not null &&
            (media is not null ||
             cycles > 0 ||
             screenshot is not null ||
             frames is not null ||
             expectFrame is not null ||
             toleranceGiven))
        {
            throw new ArgumentException(
                "--suite cannot be combined with media, --cycles, --screenshot, " +
                "--frames, --expect-frame or --tolerance.");
        }

        if (recordMissing && recordAll)
        {
            throw new ArgumentException(
                "Use either --record-missing or --record-all.");
        }

        if (recordMissing && suite is null)
            throw new ArgumentException("--record-missing requires --suite.");

        if (recordAll && suite is null)
            throw new ArgumentException("--record-all requires --suite.");

        if (selectedCases.Count > 0 && suite is null)
            throw new ArgumentException("--case requires --suite.");

        if (listCases && suite is null)
            throw new ArgumentException("--list-cases requires --suite.");

        if (listCases && (recordMissing || recordAll))
        {
            throw new ArgumentException(
                "--list-cases cannot be combined with recording options.");
        }

        if (json && !listCases)
            throw new ArgumentException("--json requires --list-cases.");

        if (relativePaths && !json)
            throw new ArgumentException("--relative-paths requires --json.");

        if (checkFiles && !json)
            throw new ArgumentException("--check-files requires --json.");

        if (videoStandardGiven && suite is not null)
        {
            throw new ArgumentException(
                "--video-standard cannot be combined with --suite; " +
                "set videoStandard per case in the suite.");
        }

        if (
            media is not null ||
            cycles > 0 ||
            screenshot is not null ||
            frames is not null ||
            expectFrame is not null ||
            suite is not null)
        {
            headless = true;
        }

        if (headless && cycles == 0 && media is null && suite is null)
            throw new ArgumentException("--headless needs --cycles or a media file.");

        return new CommandLineOptions
        {
            MediaPath = media,
            Headless = headless,
            Cycles = cycles,
            ScreenshotPath = screenshot,
            FramesDirectory = frames,
            FrameInterval = frameInterval,
            ExpectFramePath = expectFrame,
            SuitePath = suite,
            RecordMissingReferences = recordMissing,
            RecordAllReferences = recordAll,
            ListCases = listCases,
            ListCasesJson = json,
            RelativePaths = relativePaths,
            CheckFiles = checkFiles,
            SelectedSuiteCases = selectedCases,
            PixelTolerance = tolerance,
            VideoStandard = videoStandard
        };
    }

    /// <summary>
    /// Builds the options for one regression-suite case. The suite file has
    /// already validated the values.
    /// </summary>
    public static CommandLineOptions ForSuiteCase(
        string? mediaPath,
        long cycles,
        string referencePath,
        int tolerance,
        VideoStandard videoStandard,
        string? screenshotPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referencePath);

        if (cycles <= 0)
            throw new ArgumentOutOfRangeException(nameof(cycles));

        if (tolerance is < 0 or > 255)
            throw new ArgumentOutOfRangeException(nameof(tolerance));

        if (!Enum.IsDefined(typeof(VideoStandard), videoStandard))
            throw new ArgumentOutOfRangeException(nameof(videoStandard));

        return new CommandLineOptions
        {
            MediaPath = mediaPath,
            Headless = true,
            Cycles = cycles,
            ScreenshotPath = screenshotPath,
            ExpectFramePath = screenshotPath is null ? referencePath : null,
            PixelTolerance = screenshotPath is null ? tolerance : 0,
            VideoStandard = videoStandard
        };
    }

    private static string RequireValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException(option + " requires a value.");

        var value = args[++index];

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(option + " requires a value.");

        return value;
    }

    private static long ParseCycles(string value)
    {
        if (
            !long.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var cycles) ||
            cycles <= 0)
        {
            throw new ArgumentException("--cycles must be a positive integer.");
        }

        return cycles;
    }

    private static VideoStandard ParseVideoStandard(string value)
    {
        var trimmed = value.Trim();

        if (string.Equals(trimmed, "pal", StringComparison.OrdinalIgnoreCase))
            return VideoStandard.Pal;

        if (string.Equals(trimmed, "ntsc", StringComparison.OrdinalIgnoreCase))
            return VideoStandard.Ntsc;

        throw new ArgumentException(
            "--video-standard must be 'pal' or 'ntsc'.");
    }

    private static int ParseTolerance(string value)
    {
        if (
            !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var tolerance) ||
            tolerance > 255)
        {
            throw new ArgumentException("--tolerance must be between 0 and 255.");
        }

        return tolerance;
    }

    private static int ParsePositiveInt(string value, string option)
    {
        if (
            !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var result) ||
            result <= 0)
        {
            throw new ArgumentException(option + " must be a positive integer.");
        }

        return result;
    }
}
