using System;
using System.Collections.Generic;
using System.Globalization;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Theme algorithms. antd allows combining them (for example dark + compact), so this is
    /// a flags enum: Default carries no flag, Dark and Compact can be OR-ed together.
    /// </summary>
    [System.Flags]
    public enum ThemeAlgorithm
    {
        Default = 0,
        Dark = 1,
        Compact = 2
    }

    /// <summary>
    /// Seed -> Map -> Alias derivation, ported line by line from ant-design:
    ///   themes/default|dark/index.ts, themes/*/colors.ts, shared/gen*.ts, util/alias.ts
    /// </summary>
    public static class TokenBuilder
    {
        private static readonly AntdColor DarkPaletteBackground = AntdColor.Parse("#141414");

        public static AliasToken Build(SeedToken seed, ThemeAlgorithm algorithm)
        {
            if (seed == null) throw new ArgumentNullException("seed");

            bool dark = (algorithm & ThemeAlgorithm.Dark) == ThemeAlgorithm.Dark;
            bool compact = (algorithm & ThemeAlgorithm.Compact) == ThemeAlgorithm.Compact;

            AntdColor bgBase = seed.ColorBgBase ?? (dark ? AntdColor.Black : AntdColor.White);
            AntdColor textBase = seed.ColorTextBase ?? (dark ? AntdColor.White : AntdColor.Black);
            AntdColor shadowBase = dark
                ? new AntdColor(255, 255, 255, 0.2)
                : AntdColor.Black;

            var map = new Dictionary<string, object>(StringComparer.Ordinal);

            // ---------------------------------------------------------- seed passthrough
            map["colorBgBase"] = bgBase;
            map["colorTextBase"] = textBase;
            map["colorShadow"] = shadowBase;

            map["fontFamily"] = seed.FontFamily;
            map["fontFamilyCode"] = seed.FontFamilyCode;
            map["lineType"] = seed.LineType;
            map["lineWidth"] = seed.LineWidth;

            map["motionUnit"] = seed.MotionUnit;
            map["motionBase"] = seed.MotionBase;
            map["motion"] = seed.Motion;
            map["motionEaseOutCirc"] = seed.MotionEaseOutCirc;
            map["motionEaseInOutCirc"] = seed.MotionEaseInOutCirc;
            map["motionEaseOut"] = seed.MotionEaseOut;
            map["motionEaseInOut"] = seed.MotionEaseInOut;
            map["motionEaseOutBack"] = seed.MotionEaseOutBack;
            map["motionEaseInBack"] = seed.MotionEaseInBack;
            map["motionEaseInQuint"] = seed.MotionEaseInQuint;
            map["motionEaseOutQuint"] = seed.MotionEaseOutQuint;

            map["sizeUnit"] = seed.SizeUnit;
            map["sizeStep"] = seed.SizeStep;
            map["sizePopupArrow"] = seed.SizePopupArrow;
            map["controlHeight"] = seed.ControlHeight;
            map["zIndexBase"] = seed.ZIndexBase;
            map["zIndexPopupBase"] = seed.ZIndexPopupBase;
            map["opacityImage"] = seed.OpacityImage;
            map["wireframe"] = seed.Wireframe;
            map["focusOutline"] = seed.FocusOutline;

            // ---------------------------------------------------------- preset palettes
            // NOTE: the `blue1..blue10` family is the raw generate() output, NOT the
            // remapped semantic palette used by generateColorPalettes(). Mixing the two
            // shifts steps 8..10 and is a classic porting mistake.
            foreach (KeyValuePair<string, AntdColor> preset in seed.EnumeratePresetColors())
            {
                AntdColor[] raw = dark
                    ? PaletteGenerator.GenerateDark(preset.Value, DarkPaletteBackground)
                    : PaletteGenerator.Generate(preset.Value);
                AntdColor[] semantic = Remap(raw, dark);

                map[preset.Key] = preset.Value;

                for (int i = 1; i <= 10; i++)
                {
                    map[preset.Key + "-" + i.ToString(CultureInfo.InvariantCulture)] = raw[i - 1];
                    map[preset.Key + i.ToString(CultureInfo.InvariantCulture)] = raw[i - 1];
                }

                // genColorMapToken: light uses palette[5] / palette[7];
                // dark/index.ts overrides preset hover/active with palette[7] / palette[5].
                map[preset.Key + "Hover"] = dark ? semantic[7] : semantic[5];
                map[preset.Key + "Active"] = dark ? semantic[5] : semantic[7];
            }

            // ---------------------------------------------------------- semantic colors
            AntdColor[] primary = GeneratePalette(seed.ColorPrimary, dark);
            AntdColor[] success = GeneratePalette(seed.ColorSuccess, dark);
            AntdColor[] warning = GeneratePalette(seed.ColorWarning, dark);
            AntdColor[] error = GeneratePalette(seed.ColorError, dark);
            AntdColor[] info = GeneratePalette(seed.ColorInfo, dark);
            AntdColor linkSeed = seed.ColorLink ?? seed.ColorInfo;
            AntdColor[] link = GeneratePalette(linkSeed, dark);

            map["colorPrimaryBg"] = primary[1];
            map["colorPrimaryBgHover"] = primary[2];
            map["colorPrimaryBorder"] = primary[3];
            map["colorPrimaryBorderHover"] = primary[4];
            map["colorPrimaryHover"] = primary[5];
            map["colorPrimary"] = primary[6];
            map["colorPrimaryActive"] = primary[7];
            map["colorPrimaryTextHover"] = primary[8];
            map["colorPrimaryText"] = primary[9];
            map["colorPrimaryTextActive"] = primary[10];

            map["colorSuccessBg"] = success[1];
            map["colorSuccessBgHover"] = success[2];
            map["colorSuccessBorder"] = success[3];
            map["colorSuccessBorderHover"] = success[4];
            map["colorSuccessHover"] = success[4];
            map["colorSuccess"] = success[6];
            map["colorSuccessActive"] = success[7];
            map["colorSuccessTextHover"] = success[8];
            map["colorSuccessText"] = success[9];
            map["colorSuccessTextActive"] = success[10];

            map["colorWarningBg"] = warning[1];
            map["colorWarningBgHover"] = warning[2];
            map["colorWarningBorder"] = warning[3];
            map["colorWarningBorderHover"] = warning[4];
            map["colorWarningHover"] = warning[4];
            map["colorWarning"] = warning[6];
            map["colorWarningActive"] = warning[7];
            map["colorWarningTextHover"] = warning[8];
            map["colorWarningText"] = warning[9];
            map["colorWarningTextActive"] = warning[10];

            map["colorErrorBg"] = error[1];
            map["colorErrorBgHover"] = error[2];
            map["colorErrorBgFilledHover"] = error[1].Mix(error[3], 50);
            map["colorErrorBgActive"] = error[3];
            map["colorErrorBorder"] = error[3];
            map["colorErrorBorderHover"] = error[4];
            map["colorErrorHover"] = error[5];
            map["colorError"] = error[6];
            map["colorErrorActive"] = error[7];
            map["colorErrorTextHover"] = error[8];
            map["colorErrorText"] = error[9];
            map["colorErrorTextActive"] = error[10];

            map["colorInfoBg"] = info[1];
            map["colorInfoBgHover"] = info[2];
            map["colorInfoBorder"] = info[3];
            map["colorInfoBorderHover"] = info[4];
            map["colorInfoHover"] = info[4];
            map["colorInfo"] = info[6];
            map["colorInfoActive"] = info[7];
            map["colorInfoTextHover"] = info[8];
            map["colorInfoText"] = info[9];
            map["colorInfoTextActive"] = info[10];

            map["colorLinkHover"] = link[4];
            map["colorLink"] = link[6];
            map["colorLinkActive"] = link[7];

            // ---------------------------------------------------------- neutral colors
            AddNeutralColors(map, bgBase, textBase, dark);

            map["colorBgMask"] = new AntdColor(0, 0, 0, 0.45);
            map["colorWhite"] = AntdColor.White;

            // ---------------------------------------------------------- font / size / height
            AddFontTokens(map, seed.FontSize);
            AddSizeTokens(map, seed.SizeUnit, seed.SizeStep);
            AddControlHeightTokens(map, seed.ControlHeight);
            AddCommonTokens(map, seed);

            // ---------------------------------------------------------- dark algorithm overrides
            if (dark)
            {
                map["colorPrimaryBg"] = map["colorPrimaryBorder"];
                map["colorPrimaryBgHover"] = map["colorPrimaryBorderHover"];
            }

            // ---------------------------------------------------------- compact algorithm
            // Runs after the light/dark algorithm and before the alias stage, because alias
            // tokens (fontHeight, padding, control padding) are derived from these values.
            if (compact)
            {
                ApplyCompact(map, seed);
            }

            // ---------------------------------------------------------- alias stage
            return BuildAlias(map, seed, dark, compact);
        }

        // ================================================================= helpers

        private static AntdColor[] GeneratePalette(AntdColor seed, bool dark)
        {
            AntdColor[] raw = dark
                ? PaletteGenerator.GenerateDark(seed, DarkPaletteBackground)
                : PaletteGenerator.Generate(seed);
            return Remap(raw, dark);
        }

        /// <summary>
        /// themes/default|dark/colors.ts#generateColorPalettes: maps the 10 raw steps onto the
        /// 1..10 semantic indices. The light and dark mappings differ on purpose.
        /// </summary>
        private static AntdColor[] Remap(AntdColor[] raw, bool dark)
        {
            var mapped = new AntdColor[11];
            if (!dark)
            {
                mapped[1] = raw[0];
                mapped[2] = raw[1];
                mapped[3] = raw[2];
                mapped[4] = raw[3];
                mapped[5] = raw[4];
                mapped[6] = raw[5];
                mapped[7] = raw[6];
                mapped[8] = raw[4];
                mapped[9] = raw[5];
                mapped[10] = raw[6];
            }
            else
            {
                mapped[1] = raw[0];
                mapped[2] = raw[1];
                mapped[3] = raw[2];
                mapped[4] = raw[3];
                mapped[5] = raw[6];
                mapped[6] = raw[5];
                mapped[7] = raw[4];
                mapped[8] = raw[6];
                mapped[9] = raw[5];
                mapped[10] = raw[4];
            }
            return mapped;
        }

        private static void AddNeutralColors(Dictionary<string, object> map, AntdColor bgBase, AntdColor textBase, bool dark)
        {
            if (!dark)
            {
                map["colorText"] = ColorAlgorithms.SetAlpha(textBase, 0.88);
                map["colorTextSecondary"] = ColorAlgorithms.SetAlpha(textBase, 0.65);
                map["colorTextTertiary"] = ColorAlgorithms.SetAlpha(textBase, 0.45);
                map["colorTextQuaternary"] = ColorAlgorithms.SetAlpha(textBase, 0.25);

                map["colorFill"] = ColorAlgorithms.SetAlpha(textBase, 0.15);
                map["colorFillSecondary"] = ColorAlgorithms.SetAlpha(textBase, 0.06);
                map["colorFillTertiary"] = ColorAlgorithms.SetAlpha(textBase, 0.04);
                map["colorFillQuaternary"] = ColorAlgorithms.SetAlpha(textBase, 0.02);

                map["colorBgSolid"] = ColorAlgorithms.SetAlpha(textBase, 1);
                map["colorBgSolidHover"] = ColorAlgorithms.SetAlpha(textBase, 0.75);
                map["colorBgSolidActive"] = ColorAlgorithms.SetAlpha(textBase, 0.95);

                map["colorBgLayout"] = ColorAlgorithms.SolidColor(bgBase, 4, false);
                map["colorBgContainer"] = ColorAlgorithms.SolidColor(bgBase, 0, false);
                map["colorBgElevated"] = ColorAlgorithms.SolidColor(bgBase, 0, false);
                map["colorBgSpotlight"] = ColorAlgorithms.SetAlpha(textBase, 0.85);
                map["colorBgBlur"] = AntdColor.Transparent;

                map["colorBorder"] = ColorAlgorithms.SolidColor(bgBase, 15, false);
                map["colorBorderDisabled"] = ColorAlgorithms.SolidColor(bgBase, 15, false);
                map["colorBorderSecondary"] = ColorAlgorithms.SolidColor(bgBase, 6, false);
            }
            else
            {
                map["colorText"] = ColorAlgorithms.SetAlpha(textBase, 0.85);
                map["colorTextSecondary"] = ColorAlgorithms.SetAlpha(textBase, 0.65);
                map["colorTextTertiary"] = ColorAlgorithms.SetAlpha(textBase, 0.45);
                map["colorTextQuaternary"] = ColorAlgorithms.SetAlpha(textBase, 0.25);

                map["colorFill"] = ColorAlgorithms.SetAlpha(textBase, 0.18);
                map["colorFillSecondary"] = ColorAlgorithms.SetAlpha(textBase, 0.12);
                map["colorFillTertiary"] = ColorAlgorithms.SetAlpha(textBase, 0.08);
                map["colorFillQuaternary"] = ColorAlgorithms.SetAlpha(textBase, 0.04);

                map["colorBgSolid"] = ColorAlgorithms.SetAlpha(textBase, 0.95);
                map["colorBgSolidHover"] = ColorAlgorithms.SetAlpha(textBase, 1);
                map["colorBgSolidActive"] = ColorAlgorithms.SetAlpha(textBase, 0.9);

                map["colorBgElevated"] = ColorAlgorithms.SolidColor(bgBase, 12, true);
                map["colorBgContainer"] = ColorAlgorithms.SolidColor(bgBase, 8, true);
                map["colorBgLayout"] = ColorAlgorithms.SolidColor(bgBase, 0, true);
                map["colorBgSpotlight"] = ColorAlgorithms.SolidColor(bgBase, 26, true);
                map["colorBgBlur"] = ColorAlgorithms.SetAlpha(textBase, 0.04);

                map["colorBorder"] = ColorAlgorithms.SolidColor(bgBase, 26, true);
                map["colorBorderDisabled"] = ColorAlgorithms.SolidColor(bgBase, 26, true);
                map["colorBorderSecondary"] = ColorAlgorithms.SolidColor(bgBase, 19, true);
            }
        }

        private static void AddFontTokens(Dictionary<string, object> map, double baseSize)
        {
            var sizes = new double[10];
            var lineHeights = new double[10];

            for (int index = 0; index < 10; index++)
            {
                double i = index - 1;
                double raw = baseSize * Math.Exp(i / 5.0);
                double intSize = index > 1 ? Math.Floor(raw) : Math.Ceiling(raw);
                double even = Math.Floor(intSize / 2) * 2;
                sizes[index] = even;
                lineHeights[index] = GetLineHeight(even);
            }

            sizes[1] = baseSize;
            lineHeights[1] = GetLineHeight(baseSize);

            double fontSizeMD = sizes[1];
            double fontSizeSM = sizes[0];
            double fontSizeLG = sizes[2];
            double lineHeight = lineHeights[1];
            double lineHeightSM = lineHeights[0];
            double lineHeightLG = lineHeights[2];

            map["fontSizeSM"] = fontSizeSM;
            map["fontSize"] = fontSizeMD;
            map["fontSizeLG"] = fontSizeLG;
            map["fontSizeXL"] = sizes[3];

            map["fontSizeHeading1"] = sizes[6];
            map["fontSizeHeading2"] = sizes[5];
            map["fontSizeHeading3"] = sizes[4];
            map["fontSizeHeading4"] = sizes[3];
            map["fontSizeHeading5"] = sizes[2];

            map["lineHeight"] = lineHeight;
            map["lineHeightLG"] = lineHeightLG;
            map["lineHeightSM"] = lineHeightSM;

            map["fontHeight"] = AntdMath.RoundJs(lineHeight * fontSizeMD);
            map["fontHeightLG"] = AntdMath.RoundJs(lineHeightLG * fontSizeLG);
            map["fontHeightSM"] = AntdMath.RoundJs(lineHeightSM * fontSizeSM);

            map["lineHeightHeading1"] = lineHeights[6];
            map["lineHeightHeading2"] = lineHeights[5];
            map["lineHeightHeading3"] = lineHeights[4];
            map["lineHeightHeading4"] = lineHeights[3];
            map["lineHeightHeading5"] = lineHeights[2];
        }

        private static double GetLineHeight(double fontSize)
        {
            return (fontSize + 8) / fontSize;
        }

        private static void AddSizeTokens(Dictionary<string, object> map, double sizeUnit, double sizeStep)
        {
            map["sizeXXL"] = sizeUnit * (sizeStep + 8);
            map["sizeXL"] = sizeUnit * (sizeStep + 4);
            map["sizeLG"] = sizeUnit * (sizeStep + 2);
            map["sizeMD"] = sizeUnit * (sizeStep + 1);
            map["sizeMS"] = sizeUnit * sizeStep;
            map["size"] = sizeUnit * sizeStep;
            map["sizeSM"] = sizeUnit * (sizeStep - 1);
            map["sizeXS"] = sizeUnit * (sizeStep - 2);
            map["sizeXXS"] = sizeUnit * (sizeStep - 3);
        }

        private static void AddControlHeightTokens(Dictionary<string, object> map, double controlHeight)
        {
            map["controlHeightSM"] = controlHeight * 0.75;
            map["controlHeightXS"] = controlHeight * 0.5;
            map["controlHeightLG"] = controlHeight * 1.25;
        }

        /// <summary>
        /// Port of themes/compact/index.ts + genCompactSizeMapToken.ts.
        /// Font scale is rebuilt from fontSizeSM and control heights shrink by 4.
        /// </summary>
        private static void ApplyCompact(Dictionary<string, object> map, SeedToken seed)
        {
            double baseFontSize = N(map, "fontSizeSM");
            double controlHeight = N(map, "controlHeight") - 4;

            double unit = seed.SizeUnit;
            double step = seed.SizeStep - 2;

            map["sizeXXL"] = unit * (step + 10);
            map["sizeXL"] = unit * (step + 6);
            map["sizeLG"] = unit * (step + 2);
            map["sizeMD"] = unit * (step + 2);
            map["sizeMS"] = unit * (step + 1);
            map["size"] = unit * step;
            map["sizeSM"] = unit * step;
            map["sizeXS"] = unit * (step - 1);
            map["sizeXXS"] = unit * (step - 1);

            AddFontTokens(map, baseFontSize);

            map["controlHeight"] = controlHeight;
            AddControlHeightTokens(map, controlHeight);
        }

        private static void AddCommonTokens(Dictionary<string, object> map, SeedToken seed)
        {
            if (seed.Motion)
            {
                map["motionDurationFast"] = FormatSeconds(seed.MotionBase + seed.MotionUnit);
                map["motionDurationMid"] = FormatSeconds(seed.MotionBase + seed.MotionUnit * 2);
                map["motionDurationSlow"] = FormatSeconds(seed.MotionBase + seed.MotionUnit * 3);
            }
            else
            {
                map["motionDurationFast"] = "0s";
                map["motionDurationMid"] = "0s";
                map["motionDurationSlow"] = "0s";
            }

            map["lineWidthBold"] = seed.LineWidth + 1;

            double radiusBase = seed.BorderRadius;
            double radiusLG = radiusBase;
            double radiusSM = radiusBase;
            double radiusXS = radiusBase;
            double radiusOuter = radiusBase;

            if (radiusBase < 6 && radiusBase >= 5) radiusLG = radiusBase + 1;
            else if (radiusBase < 16 && radiusBase >= 6) radiusLG = radiusBase + 2;
            else if (radiusBase >= 16) radiusLG = 16;

            if (radiusBase < 7 && radiusBase >= 5) radiusSM = 4;
            else if (radiusBase < 8 && radiusBase >= 7) radiusSM = 5;
            else if (radiusBase < 14 && radiusBase >= 8) radiusSM = 6;
            else if (radiusBase < 16 && radiusBase >= 14) radiusSM = 7;
            else if (radiusBase >= 16) radiusSM = 8;

            if (radiusBase < 6 && radiusBase >= 2) radiusXS = 1;
            else if (radiusBase >= 6) radiusXS = 2;

            if (radiusBase > 4 && radiusBase < 8) radiusOuter = 4;
            else if (radiusBase >= 8) radiusOuter = 6;

            map["borderRadius"] = radiusBase;
            map["borderRadiusXS"] = radiusXS;
            map["borderRadiusSM"] = radiusSM;
            map["borderRadiusLG"] = radiusLG;
            map["borderRadiusOuter"] = radiusOuter;
        }

        private static string FormatSeconds(double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        // ================================================================= alias stage

        private static AliasToken BuildAlias(Dictionary<string, object> map, SeedToken seed, bool dark, bool compact)
        {
            var alias = new Dictionary<string, object>(map, StringComparer.Ordinal);

            AntdColor colorBgContainer = C(map, "colorBgContainer");
            AntdColor colorFill = C(map, "colorFill");
            AntdColor colorFillSecondary = C(map, "colorFillSecondary");
            AntdColor colorFillTertiary = C(map, "colorFillTertiary");
            AntdColor colorFillQuaternary = C(map, "colorFillQuaternary");
            AntdColor colorText = C(map, "colorText");
            AntdColor colorTextSecondary = C(map, "colorTextSecondary");
            AntdColor colorTextTertiary = C(map, "colorTextTertiary");
            AntdColor colorTextQuaternary = C(map, "colorTextQuaternary");
            AntdColor colorBorderSecondary = C(map, "colorBorderSecondary");

            alias["colorFillContent"] = colorFillSecondary;
            alias["colorFillContentHover"] = colorFill;
            alias["colorFillAlter"] = colorFillQuaternary;
            alias["colorBgContainerDisabled"] = colorFillTertiary;

            alias["colorBorderBg"] = colorBgContainer;
            alias["colorSplit"] = ColorAlgorithms.AlphaBlend(colorBorderSecondary, colorBgContainer);

            alias["colorTextPlaceholder"] = colorTextQuaternary;
            alias["colorTextDisabled"] = colorTextQuaternary;
            alias["colorTextHeading"] = colorText;
            alias["colorTextLabel"] = colorTextSecondary;
            alias["colorTextDescription"] = colorTextTertiary;
            alias["colorTextLightSolid"] = C(map, "colorWhite");
            alias["colorHighlight"] = C(map, "colorError");
            alias["colorBgTextHover"] = colorFillSecondary;
            alias["colorBgTextActive"] = colorFill;

            alias["colorIcon"] = colorTextTertiary;
            alias["colorIconHover"] = colorText;

            alias["colorErrorOutline"] = ColorAlgorithms.AlphaBlend(C(map, "colorErrorBg"), colorBgContainer);
            alias["colorWarningOutline"] = ColorAlgorithms.AlphaBlend(C(map, "colorWarningBg"), colorBgContainer);
            alias["colorErrorAffix"] = C(map, "colorError");
            alias["colorWarningAffix"] = C(map, "colorWarning");

            double fontSize = N(map, "fontSize");
            double lineHeight = N(map, "lineHeight");
            double fontSizeSM = N(map, "fontSizeSM");
            double lineHeightSM = N(map, "lineHeightSM");
            double fontSizeLG = N(map, "fontSizeLG");
            double lineHeightLG = N(map, "lineHeightLG");

            alias["fontHeight"] = AntdMath.RoundJs(fontSize * lineHeight);
            alias["fontHeightSM"] = AntdMath.RoundJs(fontSizeSM * lineHeightSM);
            alias["fontHeightLG"] = AntdMath.RoundJs(fontSizeLG * lineHeightLG);
            alias["fontSizeIcon"] = fontSizeSM;

            double lineWidth = N(map, "lineWidth");
            bool focusOutline = (bool)map["focusOutline"];
            alias["lineWidthFocus"] = focusOutline ? lineWidth * 3 : 0.0;
            alias["controlOutlineWidth"] = lineWidth * 2;
            alias["controlInteractiveSize"] = N(map, "controlHeight") / 2;

            alias["controlItemBgHover"] = colorFillTertiary;
            alias["controlItemBgActive"] = C(map, "colorPrimaryBg");
            alias["controlItemBgActiveHover"] = C(map, "colorPrimaryBgHover");
            alias["controlItemBgActiveDisabled"] = colorFill;
            alias["controlTmpOutline"] = colorFillQuaternary;
            alias["controlOutline"] = ColorAlgorithms.AlphaBlend(C(map, "colorPrimaryBg"), colorBgContainer);

            alias["fontWeightStrong"] = 600.0;
            alias["opacityLoading"] = 0.65;

            alias["linkDecoration"] = "none";
            alias["linkHoverDecoration"] = "none";
            alias["linkFocusDecoration"] = "none";

            alias["controlPaddingHorizontal"] = 12.0;
            alias["controlPaddingHorizontalSM"] = 8.0;

            alias["paddingXXS"] = N(map, "sizeXXS");
            alias["paddingXS"] = N(map, "sizeXS");
            alias["paddingSM"] = N(map, "sizeSM");
            alias["padding"] = N(map, "size");
            alias["paddingMD"] = N(map, "sizeMD");
            alias["paddingLG"] = N(map, "sizeLG");
            alias["paddingXL"] = N(map, "sizeXL");

            alias["paddingContentHorizontalLG"] = N(map, "sizeLG");
            alias["paddingContentVerticalLG"] = N(map, "sizeMS");
            alias["paddingContentHorizontal"] = N(map, "sizeMS");
            alias["paddingContentVertical"] = N(map, "sizeSM");
            alias["paddingContentHorizontalSM"] = N(map, "size");
            alias["paddingContentVerticalSM"] = N(map, "sizeXS");

            alias["marginXXS"] = N(map, "sizeXXS");
            alias["marginXS"] = N(map, "sizeXS");
            alias["marginSM"] = N(map, "sizeSM");
            alias["margin"] = N(map, "size");
            alias["marginMD"] = N(map, "sizeMD");
            alias["marginLG"] = N(map, "sizeLG");
            alias["marginXL"] = N(map, "sizeXL");
            alias["marginXXL"] = N(map, "sizeXXL");

            // ---- shadows
            AntdColor shadowBase = C(map, "colorShadow");
            double shadowAlpha = shadowBase.Alpha;

            Func<double, string> shadowColor = alpha =>
                shadowBase.WithAlpha(shadowAlpha * alpha).ToRgbString();

            alias["boxShadow"] = ShadowStack(shadowColor,
                new[] { "0 6px 16px 0", "0 3px 6px -4px", "0 9px 28px 8px" },
                new[] { 0.08, 0.12, 0.05 });

            alias["boxShadowSecondary"] = alias["boxShadow"];

            alias["boxShadowTertiary"] = ShadowStack(shadowColor,
                new[] { "0 1px 2px 0", "0 1px 6px -1px", "0 2px 4px 0" },
                new[] { 0.05, 0.03, 0.03 });

            alias["boxShadowCard"] = ShadowStack(shadowColor,
                new[] { "0 1px 2px -2px", "0 3px 6px 0", "0 5px 12px 4px" },
                new[] { 0.16, 0.12, 0.09 });

            alias["boxShadowDrawerRight"] = ShadowStack(shadowColor,
                new[] { "-6px 0 16px 0", "-3px 0 6px -4px", "-9px 0 28px 8px" },
                new[] { 0.08, 0.12, 0.05 });

            alias["boxShadowDrawerLeft"] = ShadowStack(shadowColor,
                new[] { "6px 0 16px 0", "3px 0 6px -4px", "9px 0 28px 8px" },
                new[] { 0.08, 0.12, 0.05 });

            alias["boxShadowDrawerUp"] = ShadowStack(shadowColor,
                new[] { "0 6px 16px 0", "0 3px 6px -4px", "0 9px 28px 8px" },
                new[] { 0.08, 0.12, 0.05 });

            alias["boxShadowDrawerDown"] = ShadowStack(shadowColor,
                new[] { "0 -6px 16px 0", "0 -3px 6px -4px", "0 -9px 28px 8px" },
                new[] { 0.08, 0.12, 0.05 });

            alias["boxShadowPopoverArrow"] = "2px 2px 5px " + shadowColor(0.05);

            alias["boxShadowTabsOverflowLeft"] = "inset 10px 0 8px -8px " + shadowColor(0.08);
            alias["boxShadowTabsOverflowRight"] = "inset -10px 0 8px -8px " + shadowColor(0.08);
            alias["boxShadowTabsOverflowTop"] = "inset 0 10px 8px -8px " + shadowColor(0.08);
            alias["boxShadowTabsOverflowBottom"] = "inset 0 -10px 8px -8px " + shadowColor(0.08);

            alias["dropShadowPopover"] = "drop-shadow(0 6px 16px " + shadowColor(0.08) + ") "
                + "drop-shadow(0 3px 6px " + shadowColor(0.12) + ") "
                + "drop-shadow(0 9px 28px " + shadowColor(0.05) + ")";

            // ---- screens
            const double screenXS = 480;
            const double screenSM = 576;
            const double screenMD = 768;
            const double screenLG = 992;
            const double screenXL = 1200;
            const double screenXXL = 1600;
            const double screenXXXL = 1920;

            alias["screenXS"] = screenXS;
            alias["screenXSMin"] = screenXS;
            alias["screenXSMax"] = screenSM - 1;
            alias["screenSM"] = screenSM;
            alias["screenSMMin"] = screenSM;
            alias["screenSMMax"] = screenMD - 1;
            alias["screenMD"] = screenMD;
            alias["screenMDMin"] = screenMD;
            alias["screenMDMax"] = screenLG - 1;
            alias["screenLG"] = screenLG;
            alias["screenLGMin"] = screenLG;
            alias["screenLGMax"] = screenXL - 1;
            alias["screenXL"] = screenXL;
            alias["screenXLMin"] = screenXL;
            alias["screenXLMax"] = screenXXL - 1;
            alias["screenXXL"] = screenXXL;
            alias["screenXXLMin"] = screenXXL;
            alias["screenXXLMax"] = screenXXXL - 1;
            alias["screenXXXL"] = screenXXXL;
            alias["screenXXXLMin"] = screenXXXL;

            return new AliasToken(alias, DescribeAlgorithm(dark, compact));
        }

        private static string DescribeAlgorithm(bool dark, bool compact)
        {
            if (dark && compact) return "dark+compact";
            if (dark) return "dark";
            if (compact) return "compact";
            return "default";
        }

        private static string ShadowStack(Func<double, string> shadowColor, string[] geometry, double[] alphas)
        {
            var parts = new string[geometry.Length];
            for (int i = 0; i < geometry.Length; i++)
            {
                parts[i] = geometry[i] + " " + shadowColor(alphas[i]);
            }
            return "\n      " + string.Join(",\n      ", parts) + "\n    ";
        }

        private static AntdColor C(Dictionary<string, object> map, string key)
        {
            object value;
            if (map.TryGetValue(key, out value) && value is AntdColor) return (AntdColor)value;
            return AntdColor.Transparent;
        }

        private static double N(Dictionary<string, object> map, string key)
        {
            object value;
            if (map.TryGetValue(key, out value) && value is double) return (double)value;
            return 0;
        }
    }
}
