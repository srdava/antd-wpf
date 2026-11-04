using System;
using System.Collections.Generic;
using System.Text;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>Error correction level of a QR symbol.</summary>
    public enum QrEcc
    {
        Low = 0,
        Medium = 1,
        Quartile = 2,
        High = 3
    }

    /// <summary>
    /// Minimal QR Code encoder: byte mode, versions 1-10, all four error correction levels.
    ///
    /// The project forbids third-party dependencies, so this follows ISO/IEC 18004 directly:
    /// build the bit stream, split it into blocks, append Reed-Solomon parity, interleave, then
    /// paint the matrix and pick the mask with the lowest penalty score.
    /// </summary>
    internal static class QrEncoder
    {
        private const int MinVersion = 1;
        private const int MaxVersion = 10;

        // Total codewords (data + parity) per version, index 0 unused.
        private static readonly int[] TotalCodewords = { 0, 26, 44, 70, 100, 134, 172, 196, 242, 292, 346 };

        // Alignment pattern centres, index 0 unused.
        private static readonly int[][] AlignmentCentres =
        {
            null,
            new int[0],
            new[] { 6, 18 },
            new[] { 6, 22 },
            new[] { 6, 26 },
            new[] { 6, 30 },
            new[] { 6, 34 },
            new[] { 6, 22, 38 },
            new[] { 6, 24, 42 },
            new[] { 6, 26, 46 },
            new[] { 6, 28, 50 }
        };

        /// <summary>Per version and ECC level: parity codewords per block, then block groups.</summary>
        private struct BlockSpec
        {
            public int EcPerBlock;
            public int Group1Blocks;
            public int Group1Data;
            public int Group2Blocks;
            public int Group2Data;
        }

        private static readonly BlockSpec[,] Specs = new BlockSpec[MaxVersion + 1, 4];

        private static readonly byte[] Exp = new byte[512];
        private static readonly byte[] Log = new byte[256];

        static QrEncoder()
        {
            BuildGaloisTables();
            BuildSpecs();
        }

        private static void BuildSpecs()
        {
            // Low
            Set(1, QrEcc.Low, 7, 1, 19, 0, 0);
            Set(2, QrEcc.Low, 10, 1, 34, 0, 0);
            Set(3, QrEcc.Low, 15, 1, 55, 0, 0);
            Set(4, QrEcc.Low, 20, 1, 80, 0, 0);
            Set(5, QrEcc.Low, 26, 1, 108, 0, 0);
            Set(6, QrEcc.Low, 18, 2, 68, 0, 0);
            Set(7, QrEcc.Low, 20, 2, 78, 0, 0);
            Set(8, QrEcc.Low, 24, 2, 97, 0, 0);
            Set(9, QrEcc.Low, 30, 2, 116, 0, 0);
            Set(10, QrEcc.Low, 18, 2, 68, 2, 69);

            // Medium
            Set(1, QrEcc.Medium, 10, 1, 16, 0, 0);
            Set(2, QrEcc.Medium, 16, 1, 28, 0, 0);
            Set(3, QrEcc.Medium, 26, 1, 44, 0, 0);
            Set(4, QrEcc.Medium, 18, 2, 32, 0, 0);
            Set(5, QrEcc.Medium, 24, 2, 43, 0, 0);
            Set(6, QrEcc.Medium, 16, 4, 27, 0, 0);
            Set(7, QrEcc.Medium, 18, 4, 31, 0, 0);
            Set(8, QrEcc.Medium, 22, 2, 38, 2, 39);
            Set(9, QrEcc.Medium, 22, 3, 36, 2, 37);
            Set(10, QrEcc.Medium, 26, 4, 43, 1, 44);

            // Quartile
            Set(1, QrEcc.Quartile, 13, 1, 13, 0, 0);
            Set(2, QrEcc.Quartile, 22, 1, 22, 0, 0);
            Set(3, QrEcc.Quartile, 18, 2, 17, 0, 0);
            Set(4, QrEcc.Quartile, 26, 2, 24, 0, 0);
            Set(5, QrEcc.Quartile, 18, 2, 15, 2, 16);
            Set(6, QrEcc.Quartile, 24, 4, 19, 0, 0);
            Set(7, QrEcc.Quartile, 18, 2, 14, 4, 15);
            Set(8, QrEcc.Quartile, 22, 4, 18, 2, 19);
            Set(9, QrEcc.Quartile, 20, 4, 16, 4, 17);
            Set(10, QrEcc.Quartile, 24, 6, 19, 2, 20);

            // High
            Set(1, QrEcc.High, 17, 1, 9, 0, 0);
            Set(2, QrEcc.High, 28, 1, 16, 0, 0);
            Set(3, QrEcc.High, 22, 2, 13, 0, 0);
            Set(4, QrEcc.High, 16, 4, 9, 0, 0);
            Set(5, QrEcc.High, 22, 2, 11, 2, 12);
            Set(6, QrEcc.High, 28, 4, 15, 0, 0);
            Set(7, QrEcc.High, 26, 4, 13, 1, 14);
            Set(8, QrEcc.High, 26, 4, 14, 2, 15);
            Set(9, QrEcc.High, 24, 4, 12, 4, 13);
            Set(10, QrEcc.High, 28, 6, 15, 2, 16);
        }

        private static void Set(int version, QrEcc ecc, int ec, int g1Blocks, int g1Data, int g2Blocks, int g2Data)
        {
            Specs[version, (int)ecc] = new BlockSpec
            {
                EcPerBlock = ec,
                Group1Blocks = g1Blocks,
                Group1Data = g1Data,
                Group2Blocks = g2Blocks,
                Group2Data = g2Data
            };
        }

        /// <summary>Encodes <paramref name="text"/> and returns the module matrix (true = dark).</summary>
        public static bool[,] Encode(string text, QrEcc ecc)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text ?? string.Empty);

            int version = ChooseVersion(payload.Length, ecc);
            BlockSpec spec = Specs[version, (int)ecc];

            int dataCodewords = spec.Group1Blocks * spec.Group1Data + spec.Group2Blocks * spec.Group2Data;
            byte[] data = BuildData(payload, version, dataCodewords);
            byte[] all = Interleave(data, spec, version);

            int size = 17 + version * 4;
            var matrix = new bool[size, size];
            var reserved = new bool[size, size];

            DrawFunctionPatterns(matrix, reserved, version);

            // Try every mask and keep the least penalised one; the format bits depend on it.
            bool[,] best = null;
            int bestPenalty = int.MaxValue;
            int bestMask = 0;

            for (int mask = 0; mask < 8; mask++)
            {
                var candidate = new bool[size, size];
                Array.Copy(matrix, candidate, matrix.Length);

                PlaceData(candidate, reserved, all, mask);
                DrawFormat(candidate, ecc, mask);

                int penalty = Score(candidate);
                if (penalty < bestPenalty)
                {
                    bestPenalty = penalty;
                    bestMask = mask;
                    best = candidate;
                }
            }

            DrawFormat(best, ecc, bestMask);
            return best;
        }

        private static int ChooseVersion(int byteCount, QrEcc ecc)
        {
            for (int version = MinVersion; version <= MaxVersion; version++)
            {
                BlockSpec spec = Specs[version, (int)ecc];
                int capacityBits = (spec.Group1Blocks * spec.Group1Data + spec.Group2Blocks * spec.Group2Data) * 8;
                int countBits = version <= 9 ? 8 : 16;
                if (4 + countBits + byteCount * 8 <= capacityBits) return version;
            }

            throw new ArgumentException("Text is too long for a version 1-10 QR code.", "text");
        }

        private static byte[] BuildData(byte[] payload, int version, int dataCodewords)
        {
            var bits = new List<bool>(dataCodewords * 8);

            Append(bits, 0x4, 4);                                  // byte mode
            Append(bits, payload.Length, version <= 9 ? 8 : 16);   // character count

            foreach (byte b in payload) Append(bits, b, 8);

            int capacity = dataCodewords * 8;
            int terminator = Math.Min(4, capacity - bits.Count);
            for (int i = 0; i < terminator; i++) bits.Add(false);

            while (bits.Count % 8 != 0) bits.Add(false);

            var data = new byte[dataCodewords];
            for (int i = 0; i < bits.Count / 8; i++)
            {
                int value = 0;
                for (int bit = 0; bit < 8; bit++) value = (value << 1) | (bits[i * 8 + bit] ? 1 : 0);
                data[i] = (byte)value;
            }

            for (int i = bits.Count / 8, pad = 0; i < dataCodewords; i++, pad++)
            {
                data[i] = (byte)(pad % 2 == 0 ? 0xEC : 0x11);
            }

            return data;
        }

        private static void Append(List<bool> bits, int value, int count)
        {
            for (int i = count - 1; i >= 0; i--) bits.Add(((value >> i) & 1) == 1);
        }

        private static byte[] Interleave(byte[] data, BlockSpec spec, int version)
        {
            var blocks = new List<byte[]>();
            var parities = new List<byte[]>();

            int offset = 0;
            for (int i = 0; i < spec.Group1Blocks; i++)
            {
                var block = new byte[spec.Group1Data];
                Array.Copy(data, offset, block, 0, block.Length);
                offset += block.Length;
                blocks.Add(block);
                parities.Add(ReedSolomon(block, spec.EcPerBlock));
            }
            for (int i = 0; i < spec.Group2Blocks; i++)
            {
                var block = new byte[spec.Group2Data];
                Array.Copy(data, offset, block, 0, block.Length);
                offset += block.Length;
                blocks.Add(block);
                parities.Add(ReedSolomon(block, spec.EcPerBlock));
            }

            int maxData = 0;
            for (int i = 0; i < blocks.Count; i++) maxData = Math.Max(maxData, blocks[i].Length);

            var result = new List<byte>(TotalCodewords[version]);
            for (int i = 0; i < maxData; i++)
            {
                for (int b = 0; b < blocks.Count; b++)
                {
                    if (i < blocks[b].Length) result.Add(blocks[b][i]);
                }
            }
            for (int i = 0; i < spec.EcPerBlock; i++)
            {
                for (int b = 0; b < parities.Count; b++) result.Add(parities[b][i]);
            }

            return result.ToArray();
        }

        // ---------------------------------------------------------------- Reed-Solomon over GF(256)

        private static void BuildGaloisTables()
        {
            int x = 1;
            for (int i = 0; i < 255; i++)
            {
                Exp[i] = (byte)x;
                Log[x] = (byte)i;
                x <<= 1;
                if (x >= 0x100) x ^= 0x11D;
            }
            for (int i = 255; i < 512; i++) Exp[i] = Exp[i - 255];
        }

        private static byte Multiply(byte a, byte b)
        {
            if (a == 0 || b == 0) return 0;
            return Exp[Log[a] + Log[b]];
        }

        private static byte[] ReedSolomon(byte[] data, int parity)
        {
            byte[] generator = Generator(parity);

            var result = new byte[parity];
            foreach (byte value in data)
            {
                byte factor = (byte)(value ^ result[0]);
                Array.Copy(result, 1, result, 0, result.Length - 1);
                result[result.Length - 1] = 0;

                for (int i = 0; i < parity; i++)
                {
                    result[i] ^= Multiply(generator[i], factor);
                }
            }

            return result;
        }

        private static byte[] Generator(int degree)
        {
            byte[] result = { 1 };
            for (int i = 0; i < degree; i++)
            {
                var next = new byte[result.Length + 1];
                for (int j = 0; j < result.Length; j++)
                {
                    next[j] ^= result[j];
                    next[j + 1] ^= Multiply(result[j], Exp[i]);
                }
                result = next;
            }

            // Drop the leading 1; callers only need the coefficients below it.
            var trimmed = new byte[result.Length - 1];
            Array.Copy(result, 1, trimmed, 0, trimmed.Length);
            return trimmed;
        }

        // ---------------------------------------------------------------- matrix painting

        private static void DrawFunctionPatterns(bool[,] matrix, bool[,] reserved, int version)
        {
            int size = matrix.GetLength(0);

            // Timing lines.
            for (int i = 0; i < size; i++)
            {
                Set(matrix, reserved, 6, i, i % 2 == 0);
                Set(matrix, reserved, i, 6, i % 2 == 0);
            }

            // Finder patterns plus separators.
            DrawFinder(matrix, reserved, 3, 3);
            DrawFinder(matrix, reserved, size - 4, 3);
            DrawFinder(matrix, reserved, 3, size - 4);

            // Alignment patterns.
            int[] centres = AlignmentCentres[version];
            for (int i = 0; i < centres.Length; i++)
            {
                for (int j = 0; j < centres.Length; j++)
                {
                    bool corner = (i == 0 && j == 0) || (i == 0 && j == centres.Length - 1)
                        || (i == centres.Length - 1 && j == 0);
                    if (corner) continue;
                    DrawAlignment(matrix, reserved, centres[i], centres[j]);
                }
            }

            // Reserve both format copies and the dark module. Forgetting the second copy lets
            // the data placement consume those modules, which shifts every following codeword.
            for (int i = 0; i < 9; i++)
            {
                if (i != 6)
                {
                    Set(matrix, reserved, 8, i, false);
                    Set(matrix, reserved, i, 8, false);
                }
            }
            for (int i = 0; i < 8; i++) Set(matrix, reserved, 8, size - 1 - i, false);
            for (int i = 8; i < 15; i++) Set(matrix, reserved, size - 15 + i, 8, false);
            Set(matrix, reserved, size - 8, 8, true);

            // Version information for version 7 and above.
            if (version >= 7)
            {
                int bits = VersionBits(version);
                for (int i = 0; i < 18; i++)
                {
                    bool bit = ((bits >> i) & 1) == 1;
                    int a = size - 11 + i % 3;
                    int b = i / 3;
                    Set(matrix, reserved, a, b, bit);
                    Set(matrix, reserved, b, a, bit);
                }
            }
        }

        private static void Set(bool[,] matrix, bool[,] reserved, int row, int col, bool dark)
        {
            matrix[row, col] = dark;
            reserved[row, col] = true;
        }

        private static void DrawFinder(bool[,] matrix, bool[,] reserved, int row, int col)
        {
            for (int r = -4; r <= 4; r++)
            {
                for (int c = -4; c <= 4; c++)
                {
                    int rr = row + r, cc = col + c;
                    if (rr < 0 || cc < 0 || rr >= matrix.GetLength(0) || cc >= matrix.GetLength(1)) continue;

                    bool dark = Math.Max(Math.Abs(r), Math.Abs(c)) != 2
                        && Math.Max(Math.Abs(r), Math.Abs(c)) != 4;
                    Set(matrix, reserved, rr, cc, dark);
                }
            }
        }

        private static void DrawAlignment(bool[,] matrix, bool[,] reserved, int row, int col)
        {
            for (int r = -2; r <= 2; r++)
            {
                for (int c = -2; c <= 2; c++)
                {
                    bool dark = Math.Max(Math.Abs(r), Math.Abs(c)) != 1;
                    Set(matrix, reserved, row + r, col + c, dark);
                }
            }
        }

        private static void PlaceData(bool[,] matrix, bool[,] reserved, byte[] data, int mask)
        {
            int size = matrix.GetLength(0);
            int bitIndex = 0;
            bool upwards = true;

            for (int right = size - 1; right >= 1; right -= 2)
            {
                if (right == 6) right = 5;

                for (int step = 0; step < size; step++)
                {
                    int row = upwards ? size - 1 - step : step;

                    for (int column = 0; column < 2; column++)
                    {
                        int col = right - column;
                        if (reserved[row, col]) continue;

                        bool bit = bitIndex < data.Length * 8
                            && ((data[bitIndex >> 3] >> (7 - (bitIndex & 7))) & 1) == 1;
                        bitIndex++;

                        matrix[row, col] = bit ^ MaskApplies(mask, row, col);
                    }
                }

                upwards = !upwards;
            }
        }

        private static bool MaskApplies(int mask, int row, int col)
        {
            switch (mask)
            {
                case 0: return (row + col) % 2 == 0;
                case 1: return row % 2 == 0;
                case 2: return col % 3 == 0;
                case 3: return (row + col) % 3 == 0;
                case 4: return (row / 2 + col / 3) % 2 == 0;
                case 5: return (row * col) % 2 + (row * col) % 3 == 0;
                case 6: return ((row * col) % 2 + (row * col) % 3) % 2 == 0;
                default: return ((row + col) % 2 + (row * col) % 3) % 2 == 0;
            }
        }

        private static void DrawFormat(bool[,] matrix, QrEcc ecc, int mask)
        {
            int size = matrix.GetLength(0);

            // ECC bits: L=01, M=00, Q=11, H=10.
            int eccBits;
            switch (ecc)
            {
                case QrEcc.Low: eccBits = 1; break;
                case QrEcc.Quartile: eccBits = 3; break;
                case QrEcc.High: eccBits = 2; break;
                default: eccBits = 0; break;
            }

            int data = (eccBits << 3) | mask;
            int bits = data << 10;
            for (int i = 14; i >= 10; i--)
            {
                if (((bits >> i) & 1) == 1) bits ^= 0x537 << (i - 10);
            }
            // bits now holds only the 10-bit remainder, so the data bits have to be put back.
            int format = ((data << 10) | bits) ^ 0x5412;

            for (int i = 0; i < 15; i++)
            {
                bool bit = ((format >> i) & 1) == 1;

                // Around the top-left finder: bits 0-5 run down column 8, bits 9-14 run left
                // along row 8. Getting these two groups the wrong way round produces a symbol
                // that looks plausible but never scans.
                if (i < 6) matrix[i, 8] = bit;
                else if (i == 6) matrix[7, 8] = bit;
                else if (i == 7) matrix[8, 8] = bit;
                else if (i == 8) matrix[8, 7] = bit;
                else matrix[8, 14 - i] = bit;

                // The mirror copy: right half of row 8, then the bottom of column 8.
                if (i < 8) matrix[8, size - 1 - i] = bit;
                else matrix[size - 15 + i, 8] = bit;
            }

            matrix[size - 8, 8] = true;
        }

        private static int VersionBits(int version)
        {
            int value = version << 12;
            for (int i = 17; i >= 12; i--)
            {
                if (((value >> i) & 1) == 1) value ^= 0x1F25 << (i - 12);
            }
            return (version << 12) | value;
        }

        // ---------------------------------------------------------------- mask scoring

        private static int Score(bool[,] matrix)
        {
            int size = matrix.GetLength(0);
            int penalty = 0;

            // Rule 1: runs of five or more equal modules.
            for (int i = 0; i < size; i++)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    int run = 1;
                    for (int j = 1; j < size; j++)
                    {
                        bool current = pass == 0 ? matrix[i, j] : matrix[j, i];
                        bool previous = pass == 0 ? matrix[i, j - 1] : matrix[j - 1, i];

                        if (current == previous)
                        {
                            run++;
                        }
                        else
                        {
                            if (run >= 5) penalty += 3 + (run - 5);
                            run = 1;
                        }
                    }
                    if (run >= 5) penalty += 3 + (run - 5);
                }
            }

            // Rule 2: 2x2 blocks of one colour.
            for (int row = 0; row < size - 1; row++)
            {
                for (int col = 0; col < size - 1; col++)
                {
                    bool v = matrix[row, col];
                    if (v == matrix[row, col + 1] && v == matrix[row + 1, col] && v == matrix[row + 1, col + 1])
                    {
                        penalty += 3;
                    }
                }
            }

            // Rule 3: finder-like patterns.
            int[] pattern = { 1, 0, 1, 1, 1, 0, 1 };
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size - 6; col++)
                {
                    if (Matches(matrix, row, col, pattern, true)) penalty += 40;
                    if (Matches(matrix, row, col, pattern, false)) penalty += 40;
                }
            }

            // Rule 4: deviation from a 50/50 balance.
            int dark = 0;
            foreach (bool module in matrix) if (module) dark++;
            int percent = dark * 100 / (size * size);
            penalty += Math.Abs(percent - 50) / 5 * 10;

            return penalty;
        }

        private static bool Matches(bool[,] matrix, int row, int col, int[] pattern, bool horizontal)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                bool value = horizontal ? matrix[row, col + i] : matrix[col + i, row];
                if ((pattern[i] == 1) != value) return false;
            }
            return true;
        }
    }
}
