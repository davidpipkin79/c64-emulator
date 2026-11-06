using System;
using System.IO;
using C64Emulator.Video;

namespace C64Emulator.Host;

/// <summary>
/// Saves a VIC frame as an uncompressed 24-bit BMP. BMP needs no codec or
/// package and is understood by browsers, Pillow, ImageMagick and the Windows
/// image viewer, which keeps headless capture dependency-free.
/// </summary>
internal static class FrameWriter
{
    private const int FileHeaderSize = 14;
    private const int InfoHeaderSize = 40;

    public static void SaveBmp(string path, RgbColor[,] frame)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(frame);

        var width = frame.GetLength(0);
        var height = frame.GetLength(1);
        var rowSize = ((width * 3) + 3) & ~3;
        var pixelBytes = rowSize * height;

        using var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        using var writer = new BinaryWriter(stream);

        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(FileHeaderSize + InfoHeaderSize + pixelBytes);
        writer.Write(0);
        writer.Write(FileHeaderSize + InfoHeaderSize);

        writer.Write(InfoHeaderSize);
        writer.Write(width);
        writer.Write(height); // positive height: rows are stored bottom-up
        writer.Write((short)1);
        writer.Write((short)24);
        writer.Write(0); // BI_RGB
        writer.Write(pixelBytes);
        writer.Write(2835); // ~72 DPI
        writer.Write(2835);
        writer.Write(0);
        writer.Write(0);

        var padding = new byte[rowSize - (width * 3)];

        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var color = frame[x, y];

                writer.Write(color.B);
                writer.Write(color.G);
                writer.Write(color.R);
            }

            writer.Write(padding);
        }
    }
}
