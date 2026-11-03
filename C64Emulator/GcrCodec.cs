using System;

namespace C64Emulator
{
    /// <summary>
    /// Commodore group-coded recording. Each data nibble becomes a five-bit
    /// code with no more than two consecutive zero bits, and the codes are
    /// concatenated most-significant-bit first. A 256-byte sector therefore
    /// occupies exactly 320 bytes on the track.
    /// </summary>
    internal static class GcrCodec
    {
        private static readonly byte[] encodeTable =
        {
            0x0A, 0x0B, 0x12, 0x13, 0x0E, 0x0F, 0x16, 0x17,
            0x09, 0x19, 0x1A, 0x1B, 0x0D, 0x1D, 0x1E, 0x15
        };

        private static readonly byte[] decodeTable = CreateDecodeTable();

        public static byte EncodeNibble(int nibble)
        {
            return encodeTable[nibble & 0x0F];
        }

        public static byte[] Encode(ReadOnlySpan<byte> data)
        {
            byte[] output = new byte[(data.Length * 10 + 7) / 8];
            int bitPosition = 0;

            foreach (byte value in data)
            {
                AppendCode(output, ref bitPosition, encodeTable[value >> 4]);
                AppendCode(output, ref bitPosition, encodeTable[value & 0x0F]);
            }

            return output;
        }

        public static byte[] Decode(ReadOnlySpan<byte> gcr, int byteCount)
        {
            byte[] output = new byte[byteCount];
            Decode(gcr, output);
            return output;
        }

        public static void Decode(ReadOnlySpan<byte> gcr, Span<byte> destination)
        {
            int bitPosition = 0;

            for (int i = 0; i < destination.Length; i++)
            {
                int high = ReadCode(gcr, ref bitPosition);
                int low = ReadCode(gcr, ref bitPosition);
                destination[i] = (byte)((high << 4) | low);
            }
        }

        private static void AppendCode(byte[] output, ref int bitPosition, byte code)
        {
            for (int bit = 4; bit >= 0; bit--)
            {
                if ((code & (1 << bit)) != 0)
                    output[bitPosition >> 3] |= (byte)(0x80 >> (bitPosition & 7));

                bitPosition++;
            }
        }

        private static int ReadCode(ReadOnlySpan<byte> input, ref int bitPosition)
        {
            int code = 0;

            for (int i = 0; i < 5; i++)
            {
                int bit =
                    (input[bitPosition >> 3] >> (7 - (bitPosition & 7))) & 1;
                code = (code << 1) | bit;
                bitPosition++;
            }

            byte decoded = decodeTable[code];
            return decoded == 0xFF ? 0 : decoded;
        }

        private static byte[] CreateDecodeTable()
        {
            byte[] table = new byte[32];
            Array.Fill(table, (byte)0xFF);

            for (int nibble = 0; nibble < 16; nibble++)
                table[encodeTable[nibble]] = (byte)nibble;

            return table;
        }
    }
}
