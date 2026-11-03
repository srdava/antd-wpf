using System;
using System.Globalization;

namespace Antd.Wpf.Tokens
{
    /// <summary>HSV triple, matching FastColor#toHsv (hue is rounded to an integer).</summary>
    public struct Hsv
    {
        public double H;
        public double S;
        public double V;

        public Hsv(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    /// <summary>
    /// Platform independent color value. Deliberately does not use System.Windows.Media.Color
    /// so the token math can be unit tested and pre-computed off the UI thread.
    /// </summary>
    public struct AntdColor : IEquatable<AntdColor>
    {
        public byte R;
        public byte G;
        public byte B;

        /// <summary>Alpha in [0,1]. Kept as a double to preserve values such as 0.88 exactly.</summary>
        public double Alpha;

        public AntdColor(byte r, byte g, byte b, double alpha = 1.0)
        {
            R = r;
            G = g;
            B = b;
            Alpha = alpha < 0 ? 0 : (alpha > 1 ? 1 : alpha);
        }

        public static readonly AntdColor White = new AntdColor(255, 255, 255);
        public static readonly AntdColor Black = new AntdColor(0, 0, 0);
        public static readonly AntdColor Transparent = new AntdColor(0, 0, 0, 0);

        public bool IsTransparent
        {
            get { return Alpha <= 0; }
        }

        // ---------------------------------------------------------------- parsing

        public static AntdColor Parse(string value)
        {
            AntdColor result;
            if (!TryParse(value, out result))
            {
                throw new FormatException("Unsupported color literal: " + value);
            }
            return result;
        }

        public static bool TryParse(string value, out AntdColor color)
        {
            color = Black;
            if (string.IsNullOrEmpty(value)) return false;

            string text = value.Trim();

            if (string.Equals(text, "transparent", StringComparison.OrdinalIgnoreCase))
            {
                color = Transparent;
                return true;
            }

            if (text.Length > 0 && text[0] == '#')
            {
                string hex = text.Substring(1);
                if (hex.Length == 3)
                {
                    byte r = ParseHexByte(new string(hex[0], 2));
                    byte g = ParseHexByte(new string(hex[1], 2));
                    byte b = ParseHexByte(new string(hex[2], 2));
                    color = new AntdColor(r, g, b, 1.0);
                    return true;
                }
                if (hex.Length == 4)
                {
                    byte r = ParseHexByte(new string(hex[0], 2));
                    byte g = ParseHexByte(new string(hex[1], 2));
                    byte b = ParseHexByte(new string(hex[2], 2));
                    byte a = ParseHexByte(new string(hex[3], 2));
                    color = new AntdColor(r, g, b, a / 255.0);
                    return true;
                }
                if (hex.Length == 6)
                {
                    byte r = ParseHexByte(hex.Substring(0, 2));
                    byte g = ParseHexByte(hex.Substring(2, 2));
                    byte b = ParseHexByte(hex.Substring(4, 2));
                    color = new AntdColor(r, g, b, 1.0);
                    return true;
                }
                if (hex.Length == 8)
                {
                    byte r = ParseHexByte(hex.Substring(0, 2));
                    byte g = ParseHexByte(hex.Substring(2, 2));
                    byte b = ParseHexByte(hex.Substring(4, 2));
                    byte a = ParseHexByte(hex.Substring(6, 2));
                    color = new AntdColor(r, g, b, a / 255.0);
                    return true;
                }
                return false;
            }

            if (text.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                int open = text.IndexOf('(');
                int close = text.LastIndexOf(')');
                if (open < 0 || close < open) return false;

                string[] parts = text.Substring(open + 1, close - open - 1).Split(',');
                if (parts.Length != 3 && parts.Length != 4) return false;

                byte r, g, b;
                if (!TryParseChannel(parts[0], out r)) return false;
                if (!TryParseChannel(parts[1], out g)) return false;
                if (!TryParseChannel(parts[2], out b)) return false;

                double a2 = 1.0;
                if (parts.Length == 4 &&
                    !double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out a2))
                {
                    return false;
                }

                color = new AntdColor(r, g, b, a2);
                return true;
            }

            return false;
        }

        private static bool TryParseChannel(string text, out byte channel)
        {
            channel = 0;
            double value;
            if (!double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }
            channel = AntdMath.ToByteClamped(value);
            return true;
        }

        private static byte ParseHexByte(string hex)
        {
            return byte.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        // ---------------------------------------------------------------- output

        /// <summary>Lower-case hex without alpha, matching FastColor#toHexString.</summary>
        public string ToHex()
        {
            return "#" + R.ToString("x2", CultureInfo.InvariantCulture)
                       + G.ToString("x2", CultureInfo.InvariantCulture)
                       + B.ToString("x2", CultureInfo.InvariantCulture);
        }

        /// <summary>Matching FastColor#toRgbString: "rgb(r,g,b)" or "rgba(r,g,b,a)".</summary>
        public string ToRgbString()
        {
            string body = R.ToString(CultureInfo.InvariantCulture) + ","
                        + G.ToString(CultureInfo.InvariantCulture) + ","
                        + B.ToString(CultureInfo.InvariantCulture);
            if (Alpha >= 1.0)
            {
                return "rgb(" + body + ")";
            }
            return "rgba(" + body + "," + FormatAlpha(Alpha) + ")";
        }

        private static string FormatAlpha(double alpha)
        {
            double rounded = Math.Round(alpha, 6);
            string text = rounded.ToString("0.######", CultureInfo.InvariantCulture);
            return text;
        }

        // ---------------------------------------------------------------- spaces

        public double Max
        {
            get { return Math.Max(R, Math.Max(G, B)); }
        }

        public double Min
        {
            get { return Math.Min(R, Math.Min(G, B)); }
        }

        /// <summary>HSL lightness as computed by FastColor#getLightness.</summary>
        public double Lightness
        {
            get { return (Max + Min) / 510.0; }
        }

        /// <summary>FastColor#toHsv: hue is rounded, saturation is HSV saturation.</summary>
        public Hsv ToHsv()
        {
            double max = Max;
            double min = Min;
            double delta = max - min;

            double hue = 0;
            if (delta != 0)
            {
                if (R == max)
                {
                    hue = 60 * ((G - B) / delta + (G < B ? 6 : 0));
                }
                else if (G == max)
                {
                    hue = 60 * ((B - R) / delta + 2);
                }
                else
                {
                    hue = 60 * ((R - G) / delta + 4);
                }
                hue = AntdMath.RoundJs(hue);
            }

            double saturation = max == 0 ? 0 : delta / max;
            return new Hsv(hue, saturation, max / 255.0);
        }

        public static AntdColor FromHsv(double h, double s, double v, double alpha)
        {
            double hue = h % 360;
            if (hue < 0) hue += 360;
            s = AntdMath.Clamp01(s);
            v = AntdMath.Clamp01(v);

            double hh = hue / 60.0;
            int i = (int)Math.Floor(hh);
            double f = hh - i;

            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double r, g, b;
            switch (((i % 6) + 6) % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return new AntdColor(
                AntdMath.ToByteClamped(r * 255),
                AntdMath.ToByteClamped(g * 255),
                AntdMath.ToByteClamped(b * 255),
                alpha);
        }

        public static AntdColor FromHsl(double h, double s, double l, double alpha)
        {
            double hue = h % 360;
            if (hue < 0) hue += 360;
            l = AntdMath.Clamp01(l);
            s = AntdMath.Clamp01(s);

            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
            double m = l - c / 2;

            double r1, g1, b1;
            if (hue < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (hue < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (hue < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (hue < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (hue < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            return new AntdColor(
                AntdMath.ToByteClamped((r1 + m) * 255),
                AntdMath.ToByteClamped((g1 + m) * 255),
                AntdMath.ToByteClamped((b1 + m) * 255),
                alpha);
        }

        // ---------------------------------------------------------------- operations

        public AntdColor WithAlpha(double alpha)
        {
            return new AntdColor(R, G, B, alpha);
        }

        /// <summary>
        /// FastColor#lighten. Note it reconstructs HSL using the **HSV** saturation,
        /// which is what antd actually relies on. Deviating breaks token parity.
        /// </summary>
        public AntdColor Lighten(double amount)
        {
            Hsv hsv = ToHsv();
            double l = Lightness + amount / 100.0;
            if (l > 1) l = 1;
            return FromHsl(hsv.H, hsv.S, l, Alpha);
        }

        /// <summary>FastColor#darken, mirror of <see cref="Lighten"/>.</summary>
        public AntdColor Darken(double amount)
        {
            Hsv hsv = ToHsv();
            double l = Lightness - amount / 100.0;
            if (l < 0) l = 0;
            return FromHsl(hsv.H, hsv.S, l, Alpha);
        }

        /// <summary>FastColor#mix: linear RGB interpolation, amount in percent toward <paramref name="target"/>.</summary>
        public AntdColor Mix(AntdColor target, double amount)
        {
            double p = amount / 100.0;
            byte r = AntdMath.ToByteClamped((target.R - R) * p + R);
            byte g = AntdMath.ToByteClamped((target.G - G) * p + G);
            byte b = AntdMath.ToByteClamped((target.B - B) * p + B);
            double a = AntdMath.Round2((target.Alpha - Alpha) * p + Alpha);
            return new AntdColor(r, g, b, a);
        }

        // ---------------------------------------------------------------- equality

        public bool Equals(AntdColor other)
        {
            return R == other.R
                && G == other.G
                && B == other.B
                && Math.Abs(Alpha - other.Alpha) < 0.0000001;
        }

        public static bool operator ==(AntdColor left, AntdColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AntdColor left, AntdColor right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is AntdColor && Equals((AntdColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = R;
                hash = (hash * 397) ^ G;
                hash = (hash * 397) ^ B;
                hash = (hash * 397) ^ ((int)Math.Round(Alpha * 1000));
                return hash;
            }
        }

        public override string ToString()
        {
            return Alpha >= 1.0 ? ToHex() : ToRgbString();
        }
    }
}
