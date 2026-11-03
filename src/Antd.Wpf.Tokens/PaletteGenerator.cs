using System;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Port of @ant-design/colors `generate()`. Produces the 10-step palette antd uses for
    /// every semantic color. Index 0 maps to antd's <c>palette[1]</c>.
    /// </summary>
    public static class PaletteGenerator
    {
        private const int HueStep = 2;
        private const double SaturationStep = 0.16;
        private const double SaturationStep2 = 0.05;
        private const double BrightnessStep1 = 0.05;
        private const double BrightnessStep2 = 0.15;
        private const int LightColorCount = 5;
        private const int DarkColorCount = 4;

        /// <summary>Dark theme remap table: { source index, mix percentage }.</summary>
        private static readonly int[,] DarkColorMap =
        {
            { 7, 15 }, { 6, 25 }, { 5, 30 }, { 5, 45 }, { 5, 65 },
            { 5, 85 }, { 4, 90 }, { 3, 95 }, { 2, 97 }, { 1, 98 }
        };

        public static AntdColor[] Generate(AntdColor seed)
        {
            Hsv hsv = seed.ToHsv();
            var patterns = new AntdColor[10];
            int cursor = 0;

            for (int i = LightColorCount; i > 0; i--)
            {
                patterns[cursor++] = AntdColor.FromHsv(
                    GetHue(hsv, i, true),
                    GetSaturation(hsv, i, true),
                    GetValue(hsv, i, true),
                    1.0);
            }

            patterns[cursor++] = seed;

            for (int i = 1; i <= DarkColorCount; i++)
            {
                patterns[cursor++] = AntdColor.FromHsv(
                    GetHue(hsv, i, false),
                    GetSaturation(hsv, i, false),
                    GetValue(hsv, i, false),
                    1.0);
            }

            return patterns;
        }

        public static AntdColor[] GenerateDark(AntdColor seed, AntdColor darkBackground)
        {
            AntdColor[] patterns = Generate(seed);
            var result = new AntdColor[10];
            for (int k = 0; k < 10; k++)
            {
                result[k] = darkBackground.Mix(patterns[DarkColorMap[k, 0]], DarkColorMap[k, 1]);
            }
            return result;
        }

        private static double GetHue(Hsv hsv, int i, bool light)
        {
            double rounded = AntdMath.RoundJs(hsv.H);
            double hue;
            if (rounded >= 60 && rounded <= 240)
            {
                hue = light ? rounded - HueStep * i : rounded + HueStep * i;
            }
            else
            {
                hue = light ? rounded + HueStep * i : rounded - HueStep * i;
            }

            if (hue < 0) hue += 360;
            else if (hue >= 360) hue -= 360;
            return hue;
        }

        private static double GetSaturation(Hsv hsv, int i, bool light)
        {
            if (hsv.H == 0 && hsv.S == 0)
            {
                return hsv.S;
            }

            double saturation;
            if (light)
            {
                saturation = hsv.S - SaturationStep * i;
            }
            else if (i == DarkColorCount)
            {
                saturation = hsv.S + SaturationStep;
            }
            else
            {
                saturation = hsv.S + SaturationStep2 * i;
            }

            if (saturation > 1) saturation = 1;

            if (light && i == LightColorCount && saturation > 0.1)
            {
                saturation = 0.1;
            }

            if (saturation < 0.06) saturation = 0.06;

            return AntdMath.Round2(saturation);
        }

        private static double GetValue(Hsv hsv, int i, bool light)
        {
            double value = light
                ? hsv.V + BrightnessStep1 * i
                : hsv.V - BrightnessStep2 * i;

            value = AntdMath.Clamp01(value);
            return AntdMath.Round2(value);
        }
    }
}
