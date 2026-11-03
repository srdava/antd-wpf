using System;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Color derivation helpers. antd has two functions named getAlphaColor with different
    /// semantics; both are reproduced here under distinct names to avoid the trap.
    /// </summary>
    public static class ColorAlgorithms
    {
        /// <summary>
        /// themes/*/colorAlgorithm.ts#getAlphaColor: set alpha on the color as-is.
        /// Used for colorText / colorFill / colorBgSpotlight families.
        /// </summary>
        public static AntdColor SetAlpha(AntdColor baseColor, double alpha)
        {
            return baseColor.WithAlpha(alpha);
        }

        /// <summary>
        /// themes/default/colorAlgorithm.ts#getSolidColor: HSL darken.
        /// themes/dark/colorAlgorithm.ts#getSolidColor: HSL lighten.
        /// </summary>
        public static AntdColor SolidColor(AntdColor baseColor, double brightness, bool lighten)
        {
            return lighten ? baseColor.Lighten(brightness) : baseColor.Darken(brightness);
        }

        /// <summary>
        /// theme/util/getAlphaColor.ts: express <paramref name="frontColor"/> as a translucent
        /// color over <paramref name="backgroundColor"/> so the composite matches the original.
        /// </summary>
        public static AntdColor AlphaBlend(AntdColor frontColor, AntdColor backgroundColor)
        {
            if (frontColor.Alpha < 1.0)
            {
                return frontColor;
            }

            const double step = 0.01;
            for (double alpha = step; alpha <= 1.0; alpha += step)
            {
                // The reference implementation rounds BEFORE the range check; skipping the
                // rounding lets float noise (255.00000000000003) pick a wrong alpha step.
                int r = AntdMath.RoundJsInt((frontColor.R - backgroundColor.R * (1 - alpha)) / alpha);
                int g = AntdMath.RoundJsInt((frontColor.G - backgroundColor.G * (1 - alpha)) / alpha);
                int b = AntdMath.RoundJsInt((frontColor.B - backgroundColor.B * (1 - alpha)) / alpha);

                if (IsStableColor(r) && IsStableColor(g) && IsStableColor(b))
                {
                    return new AntdColor(
                        (byte)r,
                        (byte)g,
                        (byte)b,
                        AntdMath.Round2(alpha));
                }
            }

            return frontColor.WithAlpha(1.0);
        }

        private static bool IsStableColor(int channel)
        {
            return channel >= 0 && channel <= 255;
        }
    }
}
