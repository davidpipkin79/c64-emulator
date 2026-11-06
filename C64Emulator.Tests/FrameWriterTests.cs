using System;
using System.IO;
using C64Emulator.Host;
using C64Emulator.Video;
using Xunit;

namespace C64Emulator.Tests;

public sealed class FrameWriterTests
{
    [Fact]
    public void SaveBmp_WritesA24BitBottomUpImage()
    {
        RgbColor[,] frame = new RgbColor[2, 2];
        frame[0, 0] = new RgbColor(1, 2, 3); // top-left
        frame[1, 0] = new RgbColor(4, 5, 6); // top-right
        frame[0, 1] = new RgbColor(7, 8, 9); // bottom-left
        frame[1, 1] = new RgbColor(10, 11, 12); // bottom-right

        var path = TempPath();

        try
        {
            FrameWriter.SaveBmp(path, frame);

            var bytes = File.ReadAllBytes(path);

            Assert.Equal((byte)'B', bytes[0]);
            Assert.Equal((byte)'M', bytes[1]);
            Assert.Equal(54 + 16, bytes.Length); // 2-pixel rows pad to 8 bytes

            Assert.Equal(2, BitConverter.ToInt32(bytes, 18));
            Assert.Equal(2, BitConverter.ToInt32(bytes, 22));
            Assert.Equal(24, BitConverter.ToInt16(bytes, 28));
            Assert.Equal(16, BitConverter.ToInt32(bytes, 34));

            // BMP stores rows bottom-up, so the screen's bottom row is first.
            Assert.Equal(
                new byte[] { 9, 8, 7, 12, 11, 10, 0, 0 },
                bytes[54..62]);
            Assert.Equal(
                new byte[] { 3, 2, 1, 6, 5, 4, 0, 0 },
                bytes[62..70]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadBmp_RoundTripsA24BitImage()
    {
        RgbColor[,] frame = new RgbColor[3, 2];
        frame[0, 0] = new RgbColor(1, 2, 3);
        frame[1, 0] = new RgbColor(4, 5, 6);
        frame[2, 0] = new RgbColor(7, 8, 9);
        frame[0, 1] = new RgbColor(10, 11, 12);
        frame[1, 1] = new RgbColor(13, 14, 15);
        frame[2, 1] = new RgbColor(16, 17, 18);

        var path = TempPath();

        try
        {
            FrameWriter.SaveBmp(path, frame);

            RgbColor[,] loaded = FrameReader.LoadBmp(path);

            Assert.Equal(3, loaded.GetLength(0));
            Assert.Equal(2, loaded.GetLength(1));

            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 3; x++)
                    Assert.Equal(frame[x, y], loaded[x, y]);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadBmp_RejectsANonBmpFile()
    {
        var path = TempPath();

        try
        {
            File.WriteAllText(path, "not a bitmap");

            Assert.Throws<InvalidDataException>(() => FrameReader.LoadBmp(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Compare_ReportsThePixelAndChannelDifference()
    {
        RgbColor[,] expected = new RgbColor[2, 2];
        RgbColor[,] actual = new RgbColor[2, 2];

        actual[1, 0] = new RgbColor(10, 20, 30);

        var difference = FrameComparer.Compare(expected, actual);

        Assert.False(difference.Matches);
        Assert.Equal(1, difference.DifferingPixels);
        Assert.Equal(30, difference.MaxChannelDelta);
    }

    [Fact]
    public void Compare_IgnoresDifferencesWithinTolerance()
    {
        RgbColor[,] expected = new RgbColor[2, 2];
        RgbColor[,] actual = new RgbColor[2, 2];

        actual[0, 0] = new RgbColor(5, 0, 0);
        actual[1, 1] = new RgbColor(0, 6, 0);

        var within = FrameComparer.Compare(expected, actual, 6);
        var above = FrameComparer.Compare(expected, actual, 5);

        Assert.True(within.Matches);
        Assert.Equal(0, within.DifferingPixels);

        Assert.False(above.Matches);
        Assert.Equal(1, above.DifferingPixels);
        Assert.Equal(6, above.MaxChannelDelta);
        Assert.Equal((1, 1), (above.FirstX, above.FirstY));
    }

    [Fact]
    public void Compare_ReportsTheChangedRegion()
    {
        RgbColor[,] expected = new RgbColor[4, 3];
        RgbColor[,] actual = new RgbColor[4, 3];

        actual[1, 1] = new RgbColor(10, 0, 0);
        actual[2, 2] = new RgbColor(0, 20, 0);

        var difference = FrameComparer.Compare(expected, actual);

        Assert.False(difference.Matches);
        Assert.Equal(2, difference.DifferingPixels);
        Assert.Equal(20, difference.MaxChannelDelta);
        Assert.Equal(1, difference.MinX);
        Assert.Equal(1, difference.MinY);
        Assert.Equal(2, difference.MaxX);
        Assert.Equal(2, difference.MaxY);
        Assert.Equal((1, 1), (difference.FirstX, difference.FirstY));
    }

    [Fact]
    public void Compare_IdenticalFramesMatch()
    {
        RgbColor[,] frame = new RgbColor[2, 2];
        frame[0, 0] = new RgbColor(1, 2, 3);

        var difference = FrameComparer.Compare(frame, (RgbColor[,])frame.Clone());

        Assert.True(difference.Matches);
        Assert.Equal(0, difference.DifferingPixels);
        Assert.Equal(0, difference.MaxChannelDelta);
    }

    private static string TempPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "c64-frame-" + Guid.NewGuid().ToString("N") + ".bmp");
    }
}
