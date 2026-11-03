using System.Collections.Generic;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Seed tokens (designer level). Mirrors components/theme/themes/seed.ts.
    /// Only seed tokens may be set by consumers; everything else is derived.
    /// </summary>
    public sealed class SeedToken
    {
        public const string DefaultFontFamily =
            "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, "
            + "'Noto Sans', sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', "
            + "'Noto Color Emoji'";

        public const string DefaultFontFamilyCode =
            "'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, Courier, monospace";

        private static readonly Dictionary<string, string> PresetColorDefaults = new Dictionary<string, string>
        {
            { "blue", "#1677FF" },
            { "purple", "#722ED1" },
            { "cyan", "#13C2C2" },
            { "green", "#52C41A" },
            { "magenta", "#EB2F96" },
            { "pink", "#EB2F96" },
            { "red", "#F5222D" },
            { "orange", "#FA8C16" },
            { "yellow", "#FADB14" },
            { "volcano", "#FA541C" },
            { "geekblue", "#2F54EB" },
            { "gold", "#FAAD14" },
            { "lime", "#A0D911" }
        };

        // ---- preset palettes
        public AntdColor Blue { get; set; }
        public AntdColor Purple { get; set; }
        public AntdColor Cyan { get; set; }
        public AntdColor Green { get; set; }
        public AntdColor Magenta { get; set; }
        public AntdColor Pink { get; set; }
        public AntdColor Red { get; set; }
        public AntdColor Orange { get; set; }
        public AntdColor Yellow { get; set; }
        public AntdColor Volcano { get; set; }
        public AntdColor Geekblue { get; set; }
        public AntdColor Gold { get; set; }
        public AntdColor Lime { get; set; }

        // ---- colors
        public AntdColor ColorPrimary { get; set; }
        public AntdColor ColorSuccess { get; set; }
        public AntdColor ColorWarning { get; set; }
        public AntdColor ColorError { get; set; }
        public AntdColor ColorInfo { get; set; }

        /// <summary>Null means "fall back to ColorInfo".</summary>
        public AntdColor? ColorLink { get; set; }

        /// <summary>Null means the algorithm default (#000 light / #fff dark).</summary>
        public AntdColor? ColorTextBase { get; set; }

        /// <summary>Null means the algorithm default (#fff light / #000 dark).</summary>
        public AntdColor? ColorBgBase { get; set; }

        // ---- typography
        public string FontFamily { get; set; }
        public string FontFamilyCode { get; set; }
        public double FontSize { get; set; }

        // ---- line
        public double LineWidth { get; set; }
        public string LineType { get; set; }

        // ---- motion
        public double MotionUnit { get; set; }
        public double MotionBase { get; set; }
        public bool Motion { get; set; }
        public string MotionEaseOutCirc { get; set; }
        public string MotionEaseInOutCirc { get; set; }
        public string MotionEaseOut { get; set; }
        public string MotionEaseInOut { get; set; }
        public string MotionEaseOutBack { get; set; }
        public string MotionEaseInBack { get; set; }
        public string MotionEaseInQuint { get; set; }
        public string MotionEaseOutQuint { get; set; }

        // ---- radius
        public double BorderRadius { get; set; }

        // ---- size
        public double SizeUnit { get; set; }
        public double SizeStep { get; set; }
        public double SizePopupArrow { get; set; }

        // ---- control
        public double ControlHeight { get; set; }

        // ---- misc
        public double ZIndexBase { get; set; }
        public double ZIndexPopupBase { get; set; }
        public double OpacityImage { get; set; }
        public bool Wireframe { get; set; }
        public bool FocusOutline { get; set; }

        public SeedToken()
        {
            Blue = AntdColor.Parse(PresetColorDefaults["blue"]);
            Purple = AntdColor.Parse(PresetColorDefaults["purple"]);
            Cyan = AntdColor.Parse(PresetColorDefaults["cyan"]);
            Green = AntdColor.Parse(PresetColorDefaults["green"]);
            Magenta = AntdColor.Parse(PresetColorDefaults["magenta"]);
            Pink = AntdColor.Parse(PresetColorDefaults["pink"]);
            Red = AntdColor.Parse(PresetColorDefaults["red"]);
            Orange = AntdColor.Parse(PresetColorDefaults["orange"]);
            Yellow = AntdColor.Parse(PresetColorDefaults["yellow"]);
            Volcano = AntdColor.Parse(PresetColorDefaults["volcano"]);
            Geekblue = AntdColor.Parse(PresetColorDefaults["geekblue"]);
            Gold = AntdColor.Parse(PresetColorDefaults["gold"]);
            Lime = AntdColor.Parse(PresetColorDefaults["lime"]);

            ColorPrimary = AntdColor.Parse("#1677ff");
            ColorSuccess = AntdColor.Parse("#52c41a");
            ColorWarning = AntdColor.Parse("#faad14");
            ColorError = AntdColor.Parse("#ff4d4f");
            ColorInfo = AntdColor.Parse("#1677ff");
            ColorLink = null;
            ColorTextBase = null;
            ColorBgBase = null;

            FontFamily = DefaultFontFamily;
            FontFamilyCode = DefaultFontFamilyCode;
            FontSize = 14;

            LineWidth = 1;
            LineType = "solid";

            MotionUnit = 0.1;
            MotionBase = 0;
            Motion = true;
            MotionEaseOutCirc = "cubic-bezier(0.08, 0.82, 0.17, 1)";
            MotionEaseInOutCirc = "cubic-bezier(0.78, 0.14, 0.15, 0.86)";
            MotionEaseOut = "cubic-bezier(0.215, 0.61, 0.355, 1)";
            MotionEaseInOut = "cubic-bezier(0.645, 0.045, 0.355, 1)";
            MotionEaseOutBack = "cubic-bezier(0.12, 0.4, 0.29, 1.46)";
            MotionEaseInBack = "cubic-bezier(0.71, -0.46, 0.88, 0.6)";
            MotionEaseInQuint = "cubic-bezier(0.755, 0.05, 0.855, 0.06)";
            MotionEaseOutQuint = "cubic-bezier(0.23, 1, 0.32, 1)";

            BorderRadius = 6;

            SizeUnit = 4;
            SizeStep = 4;
            SizePopupArrow = 16;

            ControlHeight = 32;

            ZIndexBase = 0;
            ZIndexPopupBase = 1000;
            OpacityImage = 1;
            Wireframe = false;
            FocusOutline = true;
        }

        public SeedToken Clone()
        {
            return (SeedToken)MemberwiseClone();
        }

        /// <summary>Preset palette base colors, ordered as antd declares them.</summary>
        public IEnumerable<KeyValuePair<string, AntdColor>> EnumeratePresetColors()
        {
            yield return new KeyValuePair<string, AntdColor>("blue", Blue);
            yield return new KeyValuePair<string, AntdColor>("purple", Purple);
            yield return new KeyValuePair<string, AntdColor>("cyan", Cyan);
            yield return new KeyValuePair<string, AntdColor>("green", Green);
            yield return new KeyValuePair<string, AntdColor>("magenta", Magenta);
            yield return new KeyValuePair<string, AntdColor>("pink", Pink);
            yield return new KeyValuePair<string, AntdColor>("red", Red);
            yield return new KeyValuePair<string, AntdColor>("orange", Orange);
            yield return new KeyValuePair<string, AntdColor>("yellow", Yellow);
            yield return new KeyValuePair<string, AntdColor>("volcano", Volcano);
            yield return new KeyValuePair<string, AntdColor>("geekblue", Geekblue);
            yield return new KeyValuePair<string, AntdColor>("gold", Gold);
            yield return new KeyValuePair<string, AntdColor>("lime", Lime);
        }
    }
}
