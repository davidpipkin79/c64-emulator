using System;
using C64Emulator.Video;

namespace C64Emulator.Host;

internal readonly record struct FrameDifference(
    bool Matches,
    long DifferingPixels,
    int MaxChannelDelta,
    int MinX,
    int MinY,
    int MaxX,
    int MaxY,
    int FirstX,
    int FirstY);

internal static class FrameComparer
{
    /// <summary>
    /// Compares two frames. A pixel counts as changed when its largest
    /// per-channel delta is greater than <paramref name="tolerance"/>.
    /// </summary>
    public static FrameDifference Compare(
        RgbColor[,] expected,
        RgbColor[,] actual,
        int tolerance = 0)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        if (tolerance is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance),
                "Tolerance must be between 0 and 255.");
        }

        if (
            expected.GetLength(0) != actual.GetLength(0) ||
            expected.GetLength(1) != actual.GetLength(1))
        {
            throw new ArgumentException(
                "Frames must have the same dimensions.",
                nameof(actual));
        }

        long differing = 0;
        var maxDelta = 0;
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = -1;
        var maxY = -1;
        var firstX = -1;
        var firstY = -1;

        for (var y = 0; y < expected.GetLength(1); y++)
        {
            for (var x = 0; x < expected.GetLength(0); x++)
            {
                var expectedPixel = expected[x, y];
                var actualPixel = actual[x, y];
                var delta = Math.Max(
                    Math.Abs(expectedPixel.R - actualPixel.R),
                    Math.Max(
                        Math.Abs(expectedPixel.G - actualPixel.G),
                        Math.Abs(expectedPixel.B - actualPixel.B)));

                if (delta <= tolerance)
                    continue;

                differing++;

                if (firstX < 0)
                {
                    firstX = x;
                    firstY = y;
                }

                if (delta > maxDelta)
                    maxDelta = delta;

                if (x < minX)
                    minX = x;

                if (x > maxX)
                    maxX = x;

                if (y < minY)
                    minY = y;

                if (y > maxY)
                    maxY = y;
            }
        }

        if (differing == 0)
            return new FrameDifference(true, 0, 0, 0, 0, 0, 0, -1, -1);

        return new FrameDifference(
            false,
            differing,
            maxDelta,
            minX,
            minY,
            maxX,
            maxY,
            firstX,
            firstY);
    }
}
