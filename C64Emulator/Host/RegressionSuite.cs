using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using C64Emulator.Devices;
using C64Emulator.Media;
using C64Emulator.Video;

namespace C64Emulator.Host;

internal sealed record SuiteCase(
    string Name,
    string? MediaPath,
    long Cycles,
    string ReferencePath,
    int Tolerance,
    VideoStandard VideoStandard);

/// <summary>
/// Runs a JSON list of headless checkpoints and reports one summary result.
/// Each case gets a fresh machine, so suite order cannot affect the screen.
/// </summary>
internal static class RegressionSuite
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions ListingJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static int Run(string path)
    {
        return Run(path, recordMissing: false, recordAll: false);
    }

    public static int Run(string path, TextWriter output, TextWriter error)
    {
        return Run(
            path,
            recordMissing: false,
            recordAll: false,
            output,
            error);
    }

    public static int Run(string path, bool recordMissing)
    {
        return Run(path, recordMissing, recordAll: false);
    }

    public static int Run(
        string path,
        bool recordMissing,
        TextWriter output,
        TextWriter error)
    {
        return Run(path, recordMissing, recordAll: false, output, error);
    }

    public static int Run(string path, bool recordMissing, bool recordAll)
    {
        return Run(
            path,
            recordMissing,
            recordAll,
            selectedCaseNames: null);
    }

    public static int Run(
        string path,
        bool recordMissing,
        bool recordAll,
        IReadOnlyList<string>? selectedCaseNames)
    {
        return Run(
            path,
            recordMissing,
            recordAll,
            selectedCaseNames,
            Console.Out,
            Console.Error);
    }

    public static int Run(
        string path,
        bool recordMissing,
        bool recordAll,
        TextWriter output,
        TextWriter error)
    {
        return Run(
            path,
            recordMissing,
            recordAll,
            selectedCaseNames: null,
            output,
            error);
    }

    public static int Run(
        string path,
        bool recordMissing,
        bool recordAll,
        IReadOnlyList<string>? selectedCaseNames,
        TextWriter output,
        TextWriter error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (recordMissing && recordAll)
        {
            throw new ArgumentException(
                "Use either recordMissing or recordAll, not both.");
        }

        IReadOnlyList<SuiteCase> allCases;

        try
        {
            allCases = Load(path);
        }
        catch (Exception ex) when (
            ex is IOException or
            InvalidDataException or
            JsonException or
            ArgumentException or
            UnauthorizedAccessException or
            NotSupportedException)
        {
            error.WriteLine("c64: cannot load suite: " + ex.Message);
            return ExitCodes.Usage;
        }

        IReadOnlyList<SuiteCase> cases;

        try
        {
            cases = SelectCases(allCases, selectedCaseNames);
        }
        catch (ArgumentException ex)
        {
            error.WriteLine("c64: " + ex.Message);
            return ExitCodes.Usage;
        }

        var filtered = selectedCaseNames is { Count: > 0 };
        var scope = filtered
            ? $"{cases.Count} of {allCases.Count} case(s)"
            : $"{cases.Count} case(s)";
        var notes = new List<string>();

        if (filtered)
        {
            var names = new List<string>(cases.Count);

            foreach (var testCase in cases)
                names.Add(testCase.Name);

            notes.Add("selected: " + string.Join(", ", names));
        }

        if (recordAll)
            notes.Add("recording all baselines");
        else if (recordMissing)
            notes.Add("recording missing baselines");

        var note = notes.Count > 0
            ? " (" + string.Join("; ", notes) + ")"
            : string.Empty;

        output.WriteLine(
            $"c64: suite: {scope} from {path}{note}");

        var preflight = Preflight(
            cases: cases,
            recordMissing: recordMissing,
            recordAll: recordAll,
            output: output);

        if (preflight.FailedCases > 0)
        {
            WritePreflightSummary(
                output,
                preflight.FailedCases,
                preflight.MissingFiles,
                preflight.InvalidFiles);
            WriteSummary(
                output,
                passed: 0,
                recorded: 0,
                failed: preflight.FailedCases,
                recording: recordMissing || recordAll);
            return ExitCodes.FrameMismatch;
        }

        var passed = 0;
        var recorded = 0;
        var failures = 0;

        foreach (var testCase in cases)
        {
            string? temporaryPath = null;

            try
            {
                var shouldRecord =
                    recordAll ||
                    (recordMissing && !File.Exists(testCase.ReferencePath));

                if (shouldRecord)
                    temporaryPath = GetRecordingPath(testCase.ReferencePath);

                CommandLineOptions options = CommandLineOptions.ForSuiteCase(
                    testCase.MediaPath,
                    testCase.Cycles,
                    testCase.ReferencePath,
                    testCase.Tolerance,
                    testCase.VideoStandard,
                    temporaryPath);

                var caseError = new StringWriter();
                HeadlessResult result;

                try
                {
                    result = HeadlessRunner.Execute(
                        options,
                        TextWriter.Null,
                        caseError);
                }
                catch (Exception ex) when (
                    ex is IOException or
                    InvalidDataException or
                    UnauthorizedAccessException or
                    ArgumentException or
                    NotSupportedException)
                {
                    failures++;
                    output.WriteLine(
                        "FAIL {0}: {1}",
                        testCase.Name,
                        ex.Message);
                    continue;
                }

                if (result.ExitCode != ExitCodes.Success)
                {
                    failures++;
                    output.WriteLine(
                        "FAIL {0}: {1}",
                        testCase.Name,
                        Describe(result, caseError));
                    continue;
                }

                if (shouldRecord)
                {
                    if (
                        temporaryPath is null ||
                        !File.Exists(temporaryPath))
                    {
                        failures++;
                        output.WriteLine(
                            "FAIL {0}: the reference was not written",
                            testCase.Name);
                        continue;
                    }

                    try
                    {
                        File.Move(
                            temporaryPath,
                            testCase.ReferencePath,
                            overwrite: true);
                        recorded++;
                        output.WriteLine("RECORD " + testCase.Name);
                    }
                    catch (Exception ex) when (
                        ex is IOException or
                        UnauthorizedAccessException or
                        ArgumentException or
                        NotSupportedException)
                    {
                        failures++;
                        output.WriteLine(
                            "FAIL {0}: cannot replace reference: {1}",
                            testCase.Name,
                            ex.Message);
                    }

                    continue;
                }

                passed++;
                output.WriteLine("PASS " + testCase.Name);
            }
            finally
            {
                if (temporaryPath is not null)
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception ex) when (
                        ex is IOException or
                        UnauthorizedAccessException)
                    {
                    }
                }
            }
        }

        WriteSummary(
            output,
            passed,
            recorded,
            failures,
            recording: recordMissing || recordAll);

        // One code for CI: any failing case fails the suite. The per-case line
        // carries the specific reason (mismatch, missing media, bad reference).
        return failures == 0 ? ExitCodes.Success : ExitCodes.FrameMismatch;
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames)
    {
        return ListCases(path, selectedCaseNames, json: false);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json,
            relativePaths: false);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json,
        bool relativePaths)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json,
            relativePaths,
            checkFiles: false);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json,
        bool relativePaths,
        bool checkFiles)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json,
            relativePaths,
            checkFiles,
            Console.Out,
            Console.Error);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        TextWriter output,
        TextWriter error)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json: false,
            relativePaths: false,
            output,
            error);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json,
        TextWriter output,
        TextWriter error)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json,
            relativePaths: false,
            checkFiles: false,
            output,
            error);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json,
        bool relativePaths,
        TextWriter output,
        TextWriter error)
    {
        return ListCases(
            path,
            selectedCaseNames,
            json,
            relativePaths,
            checkFiles: false,
            output,
            error);
    }

    public static int ListCases(
        string path,
        IReadOnlyList<string>? selectedCaseNames,
        bool json,
        bool relativePaths,
        bool checkFiles,
        TextWriter output,
        TextWriter error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        IReadOnlyList<SuiteCase> cases;

        try
        {
            cases = Load(path);
        }
        catch (Exception ex) when (
            ex is IOException or
            InvalidDataException or
            JsonException or
            ArgumentException or
            UnauthorizedAccessException or
            NotSupportedException)
        {
            error.WriteLine("c64: cannot load suite: " + ex.Message);
            return ExitCodes.Usage;
        }

        try
        {
            cases = SelectCases(cases, selectedCaseNames);
        }
        catch (ArgumentException ex)
        {
            error.WriteLine("c64: " + ex.Message);
            return ExitCodes.Usage;
        }

        if (json)
        {
            var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(path)) ??
                Directory.GetCurrentDirectory();
            var listings = new List<Dictionary<string, object?>>(cases.Count);
            var missing = 0;

            foreach (var testCase in cases)
            {
                var media = testCase.MediaPath;
                var reference = testCase.ReferencePath;
                bool? mediaExists = null;
                var referenceExists = false;

                if (relativePaths)
                {
                    if (media is not null)
                        media = ToRelativePath(baseDirectory, media);

                    reference = ToRelativePath(baseDirectory, reference);
                }

                if (checkFiles)
                {
                    mediaExists = testCase.MediaPath is null
                        ? null
                        : File.Exists(testCase.MediaPath);
                    referenceExists = File.Exists(testCase.ReferencePath);

                    if (mediaExists == false || !referenceExists)
                        missing++;
                }

                var listing = new Dictionary<string, object?>
                {
                    ["name"] = testCase.Name,
                    ["cycles"] = testCase.Cycles,
                    ["media"] = media,
                    ["reference"] = reference,
                    ["tolerance"] = testCase.Tolerance,
                    ["videoStandard"] =
                        testCase.VideoStandard.ToString().ToLowerInvariant()
                };

                if (checkFiles)
                {
                    listing["mediaExists"] = mediaExists;
                    listing["referenceExists"] = referenceExists;
                }

                listings.Add(listing);
            }

            output.WriteLine(
                JsonSerializer.Serialize(listings, ListingJsonOptions));

            if (checkFiles && missing > 0)
                return ExitCodes.FrameMismatch;
        }
        else
        {
            foreach (var testCase in cases)
                output.WriteLine(testCase.Name);
        }

        return ExitCodes.Success;
    }

    public static IReadOnlyList<SuiteCase> Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var baseDirectory = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrEmpty(baseDirectory))
            baseDirectory = Directory.GetCurrentDirectory();

        var json = File.ReadAllText(fullPath);
        SuiteManifest? manifest = JsonSerializer.Deserialize<SuiteManifest>(
            json,
            JsonOptions);

        if (manifest?.Cases is null || manifest.Cases.Count == 0)
            throw new InvalidDataException("The suite has no cases.");

        var cases = new List<SuiteCase>(manifest.Cases.Count);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < manifest.Cases.Count; i++)
        {
            SuiteCaseDefinition? definition = manifest.Cases[i];

            if (definition is null)
                throw new InvalidDataException($"Case {i + 1} is empty.");

            var name = string.IsNullOrWhiteSpace(definition.Name)
                ? "case " + (i + 1).ToString(CultureInfo.InvariantCulture)
                : definition.Name.Trim();

            if (!names.Add(name))
                throw new InvalidDataException("Duplicate case name: " + name);

            if (definition.Cycles <= 0)
                throw new InvalidDataException(name + ": cycles must be positive.");

            if (definition.Tolerance is < 0 or > 255)
            {
                throw new InvalidDataException(
                    name + ": tolerance must be between 0 and 255.");
            }

            if (string.IsNullOrWhiteSpace(definition.Reference))
                throw new InvalidDataException(name + ": reference is required.");

            var media = string.IsNullOrWhiteSpace(definition.Media)
                ? null
                : ResolvePath(baseDirectory, definition.Media);
            var videoStandard = ParseVideoStandard(
                definition.VideoStandard,
                name);

            cases.Add(
                new SuiteCase(
                    name,
                    media,
                    definition.Cycles,
                    ResolvePath(baseDirectory, definition.Reference),
                    definition.Tolerance,
                    videoStandard));
        }

        return cases;
    }

    private static (int FailedCases, int MissingFiles, int InvalidFiles) Preflight(
        IReadOnlyList<SuiteCase> cases,
        bool recordMissing,
        bool recordAll,
        TextWriter output)
    {
        var failedCases = 0;
        var missingFiles = 0;
        var invalidFiles = 0;

        foreach (var testCase in cases)
        {
            var failed = false;

            if (testCase.MediaPath is not null)
            {
                if (!File.Exists(testCase.MediaPath))
                {
                    output.WriteLine(
                        "FAIL {0}: media file not found: {1}",
                        testCase.Name,
                        testCase.MediaPath);
                    failed = true;
                    missingFiles++;
                }
                else if (
                    !TryValidateMedia(testCase.MediaPath, out var mediaError))
                {
                    output.WriteLine(
                        "FAIL {0}: unrecognized media file: {1}: {2}",
                        testCase.Name,
                        testCase.MediaPath,
                        mediaError);
                    failed = true;
                    invalidFiles++;
                }
            }

            if (!recordAll)
            {
                if (!File.Exists(testCase.ReferencePath))
                {
                    if (!recordMissing)
                    {
                        output.WriteLine(
                            "FAIL {0}: reference file not found: {1}",
                            testCase.Name,
                            testCase.ReferencePath);
                        failed = true;
                        missingFiles++;
                    }
                }
                else
                {
                    var referenceCheck = ValidateReference(
                        testCase.ReferencePath,
                        testCase.VideoStandard,
                        out var referenceError);

                    if (referenceCheck != ReferenceCheck.Valid)
                    {
                        output.WriteLine(
                            referenceCheck == ReferenceCheck.WrongSize
                                ? "FAIL {0}: reference size mismatch: {1}: {2}"
                                : "FAIL {0}: unreadable or malformed reference: {1}: {2}",
                            testCase.Name,
                            testCase.ReferencePath,
                            referenceError);
                        failed = true;
                        invalidFiles++;
                    }
                }
            }

            if (failed)
                failedCases++;
        }

        return (failedCases, missingFiles, invalidFiles);
    }

    private static void WritePreflightSummary(
        TextWriter output,
        int failedCases,
        int missingFiles,
        int invalidFiles)
    {
        if (missingFiles > 0 && invalidFiles > 0)
        {
            output.WriteLine(
                "c64: suite: preflight found {0} missing file(s) and {1} invalid file(s) in {2} case(s); no cases were run",
                missingFiles,
                invalidFiles,
                failedCases);
        }
        else if (invalidFiles > 0)
        {
            output.WriteLine(
                "c64: suite: preflight found {0} invalid file(s) in {1} case(s); no cases were run",
                invalidFiles,
                failedCases);
        }
        else
        {
            output.WriteLine(
                "c64: suite: preflight found {0} missing file(s) in {1} case(s); no cases were run",
                missingFiles,
                failedCases);
        }
    }

    private static bool TryValidateMedia(string path, out string reason)
    {
        try
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".prg":
                    if (new FileInfo(path).Length < 2)
                    {
                        throw new InvalidDataException(
                            "The PRG is too short to contain a load address.");
                    }

                    break;

                case ".d64":
                    _ = D64Image.Load(path);
                    break;

                case ".t64":
                    _ = T64Image.Load(path);
                    break;

                case ".tap":
                    _ = TapImage.Load(path);
                    break;

                case ".crt":
                    _ = Cartridge.Load(path);
                    break;

                default:
                    var extension = Path.GetExtension(path);
                    throw new NotSupportedException(
                        "unsupported media type" +
                        (string.IsNullOrEmpty(extension)
                            ? " (no extension)"
                            : ": " + extension) +
                        "; expected .prg, .d64, .t64, .tap or .crt");
            }

            reason = string.Empty;
            return true;
        }
        catch (Exception ex) when (
            ex is IOException or
            InvalidDataException or
            UnauthorizedAccessException or
            ArgumentException or
            NotSupportedException or
            IndexOutOfRangeException)
        {
            reason = ex.Message;
            return false;
        }
    }

    private static ReferenceCheck ValidateReference(
        string path,
        VideoStandard videoStandard,
        out string reason)
    {
        try
        {
            var frame = FrameReader.LoadBmp(path);
            var timing = VideoTiming.For(videoStandard);
            var expectedWidth = timing.VisibleWidth;
            var expectedHeight = timing.VisibleLines;

            if (
                frame.GetLength(0) != expectedWidth ||
                frame.GetLength(1) != expectedHeight)
            {
                reason =
                    $"reference is {frame.GetLength(0)}x{frame.GetLength(1)}, " +
                    $"expected {expectedWidth}x{expectedHeight}";
                return ReferenceCheck.WrongSize;
            }

            reason = string.Empty;
            return ReferenceCheck.Valid;
        }
        catch (Exception ex) when (
            ex is IOException or
            InvalidDataException or
            UnauthorizedAccessException or
            ArgumentException or
            NotSupportedException or
            IndexOutOfRangeException)
        {
            reason = ex.Message;
            return ReferenceCheck.Unreadable;
        }
    }

    private enum ReferenceCheck
    {
        Valid,
        Unreadable,
        WrongSize
    }

    private static void WriteSummary(
        TextWriter output,
        int passed,
        int recorded,
        int failed,
        bool recording)
    {
        if (recording)
        {
            output.WriteLine(
                "c64: suite: {0} passed, {1} recorded, {2} failed",
                passed,
                recorded,
                failed);
        }
        else
        {
            output.WriteLine(
                "c64: suite: {0} passed, {1} failed",
                passed,
                failed);
        }
    }

    private static IReadOnlyList<SuiteCase> SelectCases(
        IReadOnlyList<SuiteCase> cases,
        IReadOnlyList<string>? selectedCaseNames)
    {
        if (selectedCaseNames is null || selectedCaseNames.Count == 0)
            return cases;

        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var available = new List<string>(cases.Count);

        foreach (var testCase in cases)
        {
            known.Add(testCase.Name);
            available.Add(testCase.Name);
        }

        var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in selectedCaseNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("--case requires a case name.");

            var trimmed = name.Trim();

            if (!known.Contains(trimmed))
            {
                throw new ArgumentException(
                    "unknown suite case '" + trimmed +
                    "'. Available cases: " +
                    string.Join(", ", available) + ".");
            }

            wanted.Add(trimmed);
        }

        var selected = new List<SuiteCase>(wanted.Count);

        foreach (var testCase in cases)
        {
            if (wanted.Contains(testCase.Name))
                selected.Add(testCase);
        }

        return selected;
    }

    private static string ToRelativePath(string baseDirectory, string path)
    {
        return Path.GetRelativePath(baseDirectory, path).Replace('\\', '/');
    }

    private static string ResolvePath(string baseDirectory, string path)
    {
        return Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(baseDirectory, path));
    }

    private static VideoStandard ParseVideoStandard(
        string? value,
        string caseName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return VideoStandard.Pal;

        var trimmed = value.Trim();

        if (string.Equals(trimmed, "pal", StringComparison.OrdinalIgnoreCase))
            return VideoStandard.Pal;

        if (string.Equals(trimmed, "ntsc", StringComparison.OrdinalIgnoreCase))
            return VideoStandard.Ntsc;

        throw new InvalidDataException(
            caseName +
            ": videoStandard must be 'pal' or 'ntsc', not '" +
            trimmed +
            "'.");
    }

    private static string GetRecordingPath(string referencePath)
    {
        var directory = Path.GetDirectoryName(referencePath) ??
            Directory.GetCurrentDirectory();
        var fileName = Path.GetFileName(referencePath);

        return Path.Combine(
            directory,
            "." + fileName + ".recording-" +
            Guid.NewGuid().ToString("N") + ".tmp");
    }

    private static string Describe(HeadlessResult result, StringWriter error)
    {
        if (
            result.ExitCode == ExitCodes.FrameMismatch &&
            result.Difference is { } difference)
        {
            return
                $"{difference.DifferingPixels} pixels differ above tolerance " +
                $"{result.Tolerance} (max delta {difference.MaxChannelDelta}), " +
                $"region x={difference.MinX}..{difference.MaxX}, " +
                $"y={difference.MinY}..{difference.MaxY}";
        }

        foreach (var line in error.ToString().Split('\n'))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("c64: ", StringComparison.Ordinal))
                return trimmed["c64: ".Length..];
        }

        return "failed with exit code " +
            result.ExitCode.ToString(CultureInfo.InvariantCulture);
    }

    private sealed class SuiteManifest
    {
        public List<SuiteCaseDefinition?>? Cases { get; set; }
    }

    private sealed class SuiteCaseDefinition
    {
        public string? Name { get; set; }
        public string? Media { get; set; }
        public long Cycles { get; set; }
        public string? Reference { get; set; }
        public int Tolerance { get; set; }
        public string? VideoStandard { get; set; }
    }
}
