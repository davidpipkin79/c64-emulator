using System;

namespace C64Emulator
{
    /// <summary>
    /// An RGB colour used by the emulation core. The host converts this to the
    /// native SDL colour type, keeping System.Drawing out of the machine code.
    /// </summary>
    internal readonly struct RgbColor : IEquatable<RgbColor>
    {
        public RgbColor(byte red, byte green, byte blue)
        {
            R = red;
            G = green;
            B = blue;
        }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public int ToArgb()
        {
            return unchecked((int)0xFF000000 | (R << 16) | (G << 8) | B);
        }

        public bool Equals(RgbColor other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object? obj)
        {
            return obj is RgbColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }

        public static bool operator ==(RgbColor left, RgbColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RgbColor left, RgbColor right)
        {
            return !left.Equals(right);
        }
    }
}
