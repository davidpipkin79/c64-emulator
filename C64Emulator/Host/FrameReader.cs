using System;
using System.IO;
using C64Emulator.Video;

namespace C64Emulator.Host;

/// <summary>
/// Reads uncompressed BMP references written by <see cref="FrameWriter"/>.
/// Both bottom-up (positive height) and top-down (negative height) files are
/// accepted so a reference converted by another tool still works.
/// </summary>
internal static class FrameReader
{
    public static RgbColor[,] LoadBmp(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var reader = new BinaryReader(stream);

        if (stream.Length < 54 || reader.ReadByte() != 'B' || reader.ReadByte() != 'M')
            throw new InvalidDataException("Not a BMP file.");

        reader.ReadInt32(); // file size
        reader.ReadInt32(); // reserved
        var pixelOffset = reader.ReadInt32();
        var headerSize = reader.ReadInt32();

        if (headerSize < 40)
            throw new InvalidDataException("Unsupported BMP header.");

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var planes = reader.ReadInt16();
        var bitsPerPixel = reader.ReadInt16();
        var compression = reader.ReadInt32();

        if (width <= 0 || height == 0 || height == int.MinValue)
            throw new InvalidDataException("BMP has an invalid size.");

        if (planes != 1 || compression != 0 || (bitsPerPixel != 24 && bitsPerPixel != 32))
        {
            throw new InvalidDataException(
                "Only uncompressed 24-bit or 32-bit BMP files are supported.");
        }

        var absoluteHeight = Math.Abs(height);
        var bytesPerPixel = bitsPerPixel / 8;
        var rowSize = ((width * bytesPerPixel) + 3) & ~3;
        var pixelBytes = (long)rowSize * absoluteHeight;

        if (pixelOffset < 0 || pixelOffset + pixelBytes > stream.Length)
            throw new InvalidDataException("BMP pixel data is truncated.");

        var frame = new RgbColor[width, absoluteHeight];
        var row = new byte[rowSize];

        stream.Position = pixelOffset;

        for (var fileRow = 0; fileRow < absoluteHeight; fileRow++)
        {
            if (stream.Read(row, 0, rowSize) != rowSize)
                throw new InvalidDataException("BMP pixel data is truncated.");

            var y = height > 0 ? absoluteHeight - 1 - fileRow : fileRow;

            for (var x = 0; x < width; x++)
            {
                var offset = x * bytesPerPixel;
                frame[x, y] = new RgbColor(
                    row[offset + 2],
                    row[offset + 1],
                    row[offset]);
            }
        }

        return frame;
    }
}
