using System;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Numeric helpers that reproduce JavaScript semantics.
    /// C# <see cref="Math.Round(double)"/> uses banker's rounding while JS Math.round
    /// rounds half away from zero, so every port of an antd formula must use these.
    /// </summary>
    public static class AntdMath
    {
        public static double RoundJs(double value)
        {
            return Math.Floor(value + 0.5);
        }

        public static int RoundJsInt(double value)
        {
            return (int)Math.Floor(value + 0.5);
        }

        public static byte ToByteClamped(double value)
        {
            if (double.IsNaN(value)) return 0;
            if (value <= 0) return 0;
            if (value >= 255) return 255;
            return (byte)Math.Floor(value + 0.5);
        }

        public static double Clamp01(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>Rounds to two decimals, matching `Math.round(x * 100) / 100`.</summary>
        public static double Round2(double value)
        {
            return Math.Floor(value * 100 + 0.5) / 100;
        }
    }
}
