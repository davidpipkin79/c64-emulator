using System;
using System.IO;
using System.Text.Json;
using C64Emulator.Host;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class RegressionSuiteTests
{
    [Fact]
    public void Load_ResolvesPathsRelativeToTheSuite()
    {
        var directory = CreateDirectory();
        var path = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                path,
                """
                {
                  // comments and trailing commas are allowed
                  "cases": [
                    {
                      "name": "ready",
                      "media": "disks/game.d64",
                      "cycles": 3000000,
                      "reference": "refs/ready.bmp",
                      "tolerance": 8,
                    }
                  ]
                }
                """);

            var cases = RegressionSuite.Load(path);

            Assert.Single(cases);
            Assert.Equal("ready", cases[0].Name);
            Assert.Equal(3_000_000L, cases[0].Cycles);
            Assert.Equal(8, cases[0].Tolerance);
            Assert.Equal(VideoStandard.Pal, cases[0].VideoStandard);
            Assert.Equal(
                Path.GetFullPath(Path.Combine(directory, "disks", "game.d64")),
                cases[0].MediaPath);
            Assert.Equal(
                Path.GetFullPath(Path.Combine(directory, "refs", "ready.bmp")),
                cases[0].ReferencePath);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Load_ParsesTheOptionalVideoStandard()
    {
        var directory = CreateDirectory();
        var path = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                path,
                """
                {
                  "cases": [
                    { "name": "pal", "cycles": 100000, "reference": "pal.bmp", "videoStandard": "pal" },
                    { "name": "ntsc", "cycles": 100000, "reference": "ntsc.bmp", "videoStandard": "NTSC" },
                    { "name": "default", "cycles": 100000, "reference": "default.bmp" }
                  ]
                }
                """);

            var cases = RegressionSuite.Load(path);

            Assert.Equal(VideoStandard.Pal, cases[0].VideoStandard);
            Assert.Equal(VideoStandard.Ntsc, cases[1].VideoStandard);
            Assert.Equal(VideoStandard.Pal, cases[2].VideoStandard);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Load_RejectsAnUnknownVideoStandard()
    {
        var directory = CreateDirectory();
        var path = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                path,
                """
                {
                  "cases": [
                    { "name": "bad", "cycles": 100000, "reference": "bad.bmp", "videoStandard": "secam" }
                  ]
                }
                """);

            var error = Assert.Throws<InvalidDataException>(
                () => RegressionSuite.Load(path));

            Assert.Contains("bad", error.Message);
            Assert.Contains("videoStandard must be 'pal' or 'ntsc'", error.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Load_RejectsAnEmptySuite()
    {
        var directory = CreateDirectory();
        var path = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(path, """{ "cases": [] }""");

            Assert.Throws<InvalidDataException>(() => RegressionSuite.Load(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Load_RejectsDuplicateCaseNames()
    {
        var directory = CreateDirectory();
        var path = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                path,
                """
                {
                  "cases": [
                    { "name": "same", "cycles": 100000, "reference": "a.bmp" },
                    { "name": "SAME", "cycles": 100000, "reference": "b.bmp" }
                  ]
                }
                """);

            Assert.Throws<InvalidDataException>(() => RegressionSuite.Load(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_ReportsOneSummaryForPassingCases()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "ready.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            HeadlessResult shot = HeadlessRunner.Execute(
                CommandLineOptions.Parse(
                    ["--headless", "--cycles", "100000", "--screenshot", reference]),
                TextWriter.Null,
                TextWriter.Null);

            Assert.Equal(ExitCodes.Success, shot.ExitCode);
            Assert.True(File.Exists(reference));

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "smoke", "cycles": 100000, "reference": "ready.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Contains("PASS smoke", output.ToString());
            Assert.Contains("1 passed, 0 failed", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightRejectsADifferentReferenceSize()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "tiny.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            RgbColor[,] tiny = new RgbColor[2, 2];
            FrameWriter.SaveBmp(reference, tiny);

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "bad", "cycles": 100000, "reference": "tiny.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Contains(
                "FAIL bad: reference size mismatch: " + reference,
                output.ToString());
            Assert.Contains(
                "reference is 2x2, expected 403x284",
                output.ToString());
            Assert.Contains(
                "preflight found 1 invalid file(s) in 1 case(s); no cases were run",
                output.ToString());
            Assert.Contains("0 passed, 1 failed", output.ToString());
            Assert.DoesNotContain("PASS", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordsMissingReferencesAndThenComparesThem()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "recorded.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "smoke", "cycles": 100000, "reference": "recorded.bmp" }
                  ]
                }
                """);

            var recordOutput = new StringWriter();
            var recordError = new StringWriter();

            var recordCode = RegressionSuite.Run(
                suite,
                recordMissing: true,
                recordOutput,
                recordError);

            Assert.Equal(ExitCodes.Success, recordCode);
            Assert.True(File.Exists(reference));
            Assert.Contains("RECORD smoke", recordOutput.ToString());
            Assert.Contains("0 passed, 1 recorded, 0 failed", recordOutput.ToString());
            Assert.DoesNotContain("preflight", recordOutput.ToString());
            Assert.DoesNotContain(
                "reference file not found",
                recordOutput.ToString());
            Assert.Equal("", recordError.ToString());

            var checkOutput = new StringWriter();
            var checkError = new StringWriter();

            var checkCode = RegressionSuite.Run(suite, checkOutput, checkError);

            Assert.Equal(ExitCodes.Success, checkCode);
            Assert.Contains("PASS smoke", checkOutput.ToString());
            Assert.Contains("1 passed, 0 failed", checkOutput.ToString());
            Assert.Equal("", checkError.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordAllRefreshesAnExistingReference()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "refresh.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            RgbColor[,] tiny = new RgbColor[2, 2];
            FrameWriter.SaveBmp(reference, tiny);
            var originalLength = new FileInfo(reference).Length;

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "refresh", "cycles": 100000, "reference": "refresh.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.True(new FileInfo(reference).Length > originalLength);
            Assert.Contains("RECORD refresh", output.ToString());
            Assert.Contains("0 passed, 1 recorded, 0 failed", output.ToString());
            Assert.Equal("", error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));

            var checkOutput = new StringWriter();
            var checkError = new StringWriter();

            var checkCode = RegressionSuite.Run(suite, checkOutput, checkError);

            Assert.Equal(ExitCodes.Success, checkCode);
            Assert.Contains("PASS refresh", checkOutput.ToString());
            Assert.Equal("", checkError.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordAllKeepsTheOldReferenceWhenTheCaseFails()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "keep.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            RgbColor[,] tiny = new RgbColor[2, 2];
            FrameWriter.SaveBmp(reference, tiny);
            var originalLength = new FileInfo(reference).Length;

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "keep",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "keep.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: true,
                output,
                error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal(originalLength, new FileInfo(reference).Length);
            Assert.Contains("FAIL keep", output.ToString());
            Assert.Contains(
                "preflight found 1 missing file(s) in 1 case(s); no cases were run",
                output.ToString());
            Assert.Contains("0 passed, 0 recorded, 1 failed", output.ToString());
            Assert.DoesNotContain("RECORD", output.ToString());
            Assert.Equal("", error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_DoesNotOverwriteAnExistingReferenceWhileRecording()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "tiny.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            RgbColor[,] tiny = new RgbColor[2, 2];
            FrameWriter.SaveBmp(reference, tiny);
            var originalLength = new FileInfo(reference).Length;

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "tiny", "cycles": 100000, "reference": "tiny.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: true,
                output,
                error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal(originalLength, new FileInfo(reference).Length);
            Assert.Contains("FAIL tiny", output.ToString());
            Assert.Contains("reference size mismatch", output.ToString());
            Assert.DoesNotContain("RECORD", output.ToString());
            Assert.Contains("0 passed, 0 recorded, 1 failed", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_DoesNotRecordWhenTheCaseCannotRun()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "unwritten.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "missing",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "unwritten.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: true,
                output,
                error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.False(File.Exists(reference));
            Assert.Contains("FAIL missing", output.ToString());
            Assert.Contains("media file not found", output.ToString());
            Assert.DoesNotContain(
                "reference file not found",
                output.ToString());
            Assert.Contains(
                "preflight found 1 missing file(s) in 1 case(s); no cases were run",
                output.ToString());
            Assert.Contains("0 passed, 0 recorded, 1 failed", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_CaseSelectionRunsOnlyTheSelectedCase()
    {
        var directory = CreateDirectory();
        var firstReference = Path.Combine(directory, "first.bmp");
        var secondReference = Path.Combine(directory, "second.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "first", "cycles": 100000, "reference": "first.bmp" },
                    {
                      "name": "second",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "second.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: true,
                recordAll: false,
                selectedCaseNames: new[] { "first" },
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.True(File.Exists(firstReference));
            Assert.False(File.Exists(secondReference));
            Assert.Contains("RECORD first", output.ToString());
            Assert.DoesNotContain("second", output.ToString());
            Assert.DoesNotContain("preflight", output.ToString());
            Assert.Contains("1 of 2 case(s)", output.ToString());
            Assert.Contains("selected: first", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_CaseSelectionWithRecordAllRefreshesOnlyThatCase()
    {
        var directory = CreateDirectory();
        var firstReference = Path.Combine(directory, "first.bmp");
        var secondReference = Path.Combine(directory, "second.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            RgbColor[,] tiny = new RgbColor[2, 2];
            FrameWriter.SaveBmp(firstReference, tiny);
            FrameWriter.SaveBmp(secondReference, tiny);
            var firstLength = new FileInfo(firstReference).Length;
            var secondLength = new FileInfo(secondReference).Length;

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "first", "cycles": 100000, "reference": "first.bmp" },
                    { "name": "second", "cycles": 100000, "reference": "second.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: true,
                selectedCaseNames: new[] { "second" },
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Equal(firstLength, new FileInfo(firstReference).Length);
            Assert.True(new FileInfo(secondReference).Length > secondLength);
            Assert.Contains("RECORD second", output.ToString());
            Assert.DoesNotContain("first", output.ToString());
            Assert.Contains("1 of 2 case(s)", output.ToString());
            Assert.Contains("selected: second", output.ToString());
            Assert.Contains("0 passed, 1 recorded, 0 failed", output.ToString());
            Assert.Equal("", error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_UnknownSelectedCaseIsAUsageError()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "boot", "cycles": 100000, "reference": "boot.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: false,
                selectedCaseNames: new[] { "missing" },
                output,
                error);

            Assert.Equal(ExitCodes.Usage, code);
            Assert.Equal("", output.ToString());
            Assert.Contains("unknown suite case 'missing'", error.ToString());
            Assert.Contains("Available cases: boot", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_PrintsNamesInManifestOrder()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "first",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "first.bmp"
                    },
                    {
                      "name": "second",
                      "cycles": 100000,
                      "reference": "second.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);

            using var reader = new StringReader(output.ToString());
            Assert.Equal("first", reader.ReadLine());
            Assert.Equal("second", reader.ReadLine());
            Assert.Null(reader.ReadLine());
            Assert.Equal("", error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.bmp"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_PrintsOnlySelectedNames()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "first", "cycles": 100000, "reference": "first.bmp" },
                    { "name": "second", "cycles": 100000, "reference": "second.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: new[] { "SECOND" },
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Equal("second", output.ToString().TrimEnd('\r', '\n'));
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_RejectsUnknownSelection()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "boot", "cycles": 100000, "reference": "boot.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: new[] { "missing" },
                output,
                error);

            Assert.Equal(ExitCodes.Usage, code);
            Assert.Equal("", output.ToString());
            Assert.Contains("unknown suite case 'missing'", error.ToString());
            Assert.Contains("Available cases: boot", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_JsonIncludesCaseDetails()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "ready",
                      "media": "disks/game.d64",
                      "cycles": 3000000,
                      "reference": "refs/ready.bmp",
                      "tolerance": 8
                    },
                    {
                      "name": "bare",
                      "cycles": 100000,
                      "reference": "bare.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                json: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Equal("", error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.bmp"));

            using var document = JsonDocument.Parse(output.ToString());
            JsonElement root = document.RootElement;

            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            Assert.Equal(2, root.GetArrayLength());

            JsonElement ready = root[0];
            Assert.False(ready.TryGetProperty("mediaExists", out _));
            Assert.False(ready.TryGetProperty("referenceExists", out _));
            Assert.Equal("ready", ready.GetProperty("name").GetString());
            Assert.Equal(3_000_000L, ready.GetProperty("cycles").GetInt64());
            Assert.Equal(
                Path.GetFullPath(
                    Path.Combine(directory, "disks", "game.d64")),
                ready.GetProperty("media").GetString());
            Assert.Equal(
                Path.GetFullPath(
                    Path.Combine(directory, "refs", "ready.bmp")),
                ready.GetProperty("reference").GetString());
            Assert.Equal(8, ready.GetProperty("tolerance").GetInt32());
            Assert.Equal(
                "pal",
                ready.GetProperty("videoStandard").GetString());

            JsonElement bare = root[1];
            Assert.Equal("bare", bare.GetProperty("name").GetString());
            Assert.Equal(100_000L, bare.GetProperty("cycles").GetInt64());
            Assert.Equal(
                JsonValueKind.Null,
                bare.GetProperty("media").ValueKind);
            Assert.Equal(
                Path.GetFullPath(Path.Combine(directory, "bare.bmp")),
                bare.GetProperty("reference").GetString());
            Assert.Equal(0, bare.GetProperty("tolerance").GetInt32());
            Assert.Equal(
                "pal",
                bare.GetProperty("videoStandard").GetString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_JsonCanPrintPathsRelativeToTheSuite()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "ready",
                      "media": "disks/game.d64",
                      "cycles": 3000000,
                      "reference": "refs/ready.bmp",
                      "tolerance": 8
                    },
                    {
                      "name": "bare",
                      "cycles": 100000,
                      "reference": "bare.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                json: true,
                relativePaths: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Equal("", error.ToString());

            using var document = JsonDocument.Parse(output.ToString());
            JsonElement root = document.RootElement;

            Assert.Equal(2, root.GetArrayLength());
            Assert.Equal(
                "disks/game.d64",
                root[0].GetProperty("media").GetString());
            Assert.Equal(
                "refs/ready.bmp",
                root[0].GetProperty("reference").GetString());
            Assert.Equal(
                JsonValueKind.Null,
                root[1].GetProperty("media").ValueKind);
            Assert.Equal(
                "bare.bmp",
                root[1].GetProperty("reference").GetString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_JsonIncludesTheVideoStandard()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "pal", "cycles": 100000, "reference": "pal.bmp" },
                    {
                      "name": "ntsc",
                      "cycles": 100000,
                      "reference": "ntsc.bmp",
                      "videoStandard": "ntsc"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                json: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Equal("", error.ToString());

            using var document = JsonDocument.Parse(output.ToString());
            JsonElement root = document.RootElement;

            Assert.Equal("pal", root[0].GetProperty("videoStandard").GetString());
            Assert.Equal("ntsc", root[1].GetProperty("videoStandard").GetString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ListCases_JsonPreflightReportsMissingFiles()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");
        var media = Path.Combine(directory, "media.d64");
        var presentReference = Path.Combine(directory, "present.bmp");
        var missingMedia = Path.Combine(directory, "missing.d64");
        var missingReference = Path.Combine(directory, "missing.bmp");
        var noMediaReference = Path.Combine(directory, "nomedia.bmp");

        try
        {
            File.WriteAllBytes(media, Array.Empty<byte>());
            File.WriteAllBytes(presentReference, Array.Empty<byte>());
            File.WriteAllBytes(noMediaReference, Array.Empty<byte>());

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "present",
                      "media": "media.d64",
                      "cycles": 100000,
                      "reference": "present.bmp"
                    },
                    {
                      "name": "missing",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "missing.bmp"
                    },
                    {
                      "name": "no-media",
                      "cycles": 100000,
                      "reference": "nomedia.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                json: true,
                relativePaths: false,
                checkFiles: true,
                output,
                error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal("", error.ToString());

            using var document = JsonDocument.Parse(output.ToString());
            JsonElement root = document.RootElement;

            Assert.Equal(3, root.GetArrayLength());

            JsonElement present = root[0];
            Assert.True(present.GetProperty("mediaExists").GetBoolean());
            Assert.True(present.GetProperty("referenceExists").GetBoolean());

            JsonElement missing = root[1];
            Assert.False(missing.GetProperty("mediaExists").GetBoolean());
            Assert.False(missing.GetProperty("referenceExists").GetBoolean());

            JsonElement noMedia = root[2];
            Assert.Equal(
                JsonValueKind.Null,
                noMedia.GetProperty("mediaExists").ValueKind);
            Assert.True(noMedia.GetProperty("referenceExists").GetBoolean());

            File.WriteAllBytes(missingMedia, Array.Empty<byte>());
            File.WriteAllBytes(missingReference, Array.Empty<byte>());

            var secondOutput = new StringWriter();
            var secondError = new StringWriter();

            var secondCode = RegressionSuite.ListCases(
                suite,
                selectedCaseNames: null,
                json: true,
                relativePaths: false,
                checkFiles: true,
                secondOutput,
                secondError);

            Assert.Equal(ExitCodes.Success, secondCode);
            Assert.Equal("", secondError.ToString());

            using var secondDocument = JsonDocument.Parse(secondOutput.ToString());

            Assert.True(
                secondDocument.RootElement[1]
                    .GetProperty("mediaExists")
                    .GetBoolean());
            Assert.True(
                secondDocument.RootElement[1]
                    .GetProperty("referenceExists")
                    .GetBoolean());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightReportsAllMissingFilesBeforeEmulation()
    {
        var directory = CreateDirectory();
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "alpha",
                      "media": "alpha.d64",
                      "cycles": 100000,
                      "reference": "alpha.bmp"
                    },
                    {
                      "name": "beta",
                      "media": "beta.d64",
                      "cycles": 100000,
                      "reference": "beta.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal("", error.ToString());

            var text = output.ToString();
            Assert.Contains(
                "FAIL alpha: media file not found: " +
                Path.Combine(directory, "alpha.d64"),
                text);
            Assert.Contains(
                "FAIL alpha: reference file not found: " +
                Path.Combine(directory, "alpha.bmp"),
                text);
            Assert.Contains(
                "FAIL beta: media file not found: " +
                Path.Combine(directory, "beta.d64"),
                text);
            Assert.Contains(
                "FAIL beta: reference file not found: " +
                Path.Combine(directory, "beta.bmp"),
                text);
            Assert.Contains(
                "preflight found 4 missing file(s) in 2 case(s); no cases were run",
                text);
            Assert.Contains("0 passed, 2 failed", text);
            Assert.DoesNotContain("PASS", text);
            Assert.DoesNotContain("RECORD", text);
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordAllDoesNotPreflightMissingReferences()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "recorded.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "smoke", "cycles": 100000, "reference": "recorded.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.True(File.Exists(reference));
            Assert.Contains("RECORD smoke", output.ToString());
            Assert.DoesNotContain("preflight", output.ToString());
            Assert.DoesNotContain(
                "reference file not found",
                output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightRejectsMalformedReference()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "bad.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(reference, "not a bitmap");

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "bad", "cycles": 100000, "reference": "bad.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal("", error.ToString());

            var text = output.ToString();
            Assert.Contains(
                "FAIL bad: unreadable or malformed reference: " + reference,
                text);
            Assert.Contains("Not a BMP file.", text);
            Assert.Contains(
                "preflight found 1 invalid file(s) in 1 case(s); no cases were run",
                text);
            Assert.Contains("0 passed, 1 failed", text);
            Assert.DoesNotContain("PASS", text);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightRejectsUnrecognizedAndMalformedMedia()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "ref.bmp");
        var unknown = Path.Combine(directory, "game.xyz");
        var badDisk = Path.Combine(directory, "bad.d64");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            FrameWriter.SaveBmp(reference, new RgbColor[403, 284]);
            File.WriteAllBytes(unknown, Array.Empty<byte>());
            File.WriteAllBytes(badDisk, new byte[100]);

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "unknown",
                      "media": "game.xyz",
                      "cycles": 100000,
                      "reference": "ref.bmp"
                    },
                    {
                      "name": "bad-disk",
                      "media": "bad.d64",
                      "cycles": 100000,
                      "reference": "ref.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal("", error.ToString());

            var text = output.ToString();
            Assert.Contains(
                "FAIL unknown: unrecognized media file: " + unknown,
                text);
            Assert.Contains("unsupported media type: .xyz", text);
            Assert.Contains(
                "FAIL bad-disk: unrecognized media file: " + badDisk,
                text);
            Assert.Contains("too small to be a D64 image", text);
            Assert.Contains(
                "preflight found 2 invalid file(s) in 2 case(s); no cases were run",
                text);
            Assert.Contains("0 passed, 2 failed", text);
            Assert.DoesNotContain("PASS", text);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightSummarizesMissingAndInvalidFiles()
    {
        var directory = CreateDirectory();
        var badReference = Path.Combine(directory, "bad.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(badReference, "not a bitmap");

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "missing",
                      "media": "missing.d64",
                      "cycles": 100000,
                      "reference": "missing.bmp"
                    },
                    {
                      "name": "bad",
                      "cycles": 100000,
                      "reference": "bad.bmp"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Equal("", error.ToString());

            var text = output.ToString();
            Assert.Contains(
                "preflight found 2 missing file(s) and 1 invalid file(s) in 2 case(s); no cases were run",
                text);
            Assert.Contains("0 passed, 2 failed", text);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordMissingStillValidatesAnExistingReference()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "bad.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(reference, "not a bitmap");

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "bad", "cycles": 100000, "reference": "bad.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: true,
                output,
                error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Contains(
                "unreadable or malformed reference",
                output.ToString());
            Assert.Contains(
                "preflight found 1 invalid file(s) in 1 case(s); no cases were run",
                output.ToString());
            Assert.DoesNotContain("RECORD", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RecordAllReplacesAMalformedReference()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "bad.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(reference, "not a bitmap");
            var originalLength = new FileInfo(reference).Length;

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    { "name": "bad", "cycles": 100000, "reference": "bad.bmp" }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(
                suite,
                recordMissing: false,
                recordAll: true,
                output,
                error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Contains("RECORD bad", output.ToString());
            Assert.DoesNotContain("preflight", output.ToString());
            Assert.True(new FileInfo(reference).Length > originalLength);
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_PreflightUsesTheCaseVideoStandard()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "pal.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            FrameWriter.SaveBmp(reference, new RgbColor[403, 284]);

            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "ntsc",
                      "cycles": 100000,
                      "reference": "pal.bmp",
                      "videoStandard": "ntsc"
                    }
                  ]
                }
                """);

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.FrameMismatch, code);
            Assert.Contains(
                "reference is 403x284, expected 418x235",
                output.ToString());
            Assert.Contains(
                "preflight found 1 invalid file(s) in 1 case(s); no cases were run",
                output.ToString());
            Assert.DoesNotContain("PASS", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_NtscCaseRecordsAndPasses()
    {
        var directory = CreateDirectory();
        var reference = Path.Combine(directory, "ntsc.bmp");
        var suite = Path.Combine(directory, "suite.json");

        try
        {
            File.WriteAllText(
                suite,
                """
                {
                  "cases": [
                    {
                      "name": "ntsc",
                      "cycles": 100000,
                      "reference": "ntsc.bmp",
                      "videoStandard": "ntsc"
                    }
                  ]
                }
                """);

            var recordOutput = new StringWriter();
            var recordError = new StringWriter();

            var recordCode = RegressionSuite.Run(
                suite,
                recordMissing: true,
                recordOutput,
                recordError);

            Assert.Equal(ExitCodes.Success, recordCode);
            Assert.True(File.Exists(reference));
            Assert.Contains("RECORD ntsc", recordOutput.ToString());
            Assert.Equal("", recordError.ToString());

            var frame = FrameReader.LoadBmp(reference);
            Assert.Equal(418, frame.GetLength(0));
            Assert.Equal(235, frame.GetLength(1));

            var output = new StringWriter();
            var error = new StringWriter();

            var code = RegressionSuite.Run(suite, output, error);

            Assert.Equal(ExitCodes.Success, code);
            Assert.Contains("PASS ntsc", output.ToString());
            Assert.Equal("", error.ToString());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Run_RejectsAMissingSuiteFile()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var path = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N") + ".json");

        var code = RegressionSuite.Run(path, output, error);

        Assert.Equal(ExitCodes.Usage, code);
        Assert.Contains("cannot load suite", error.ToString());
    }

    private static string CreateDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "c64-suite-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }
}
