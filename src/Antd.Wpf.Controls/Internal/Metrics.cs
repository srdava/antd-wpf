using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// Component level metrics. antd exposes these as component tokens (Button.paddingInline
    /// and friends) rather than global design tokens, so they live here instead of in
    /// Antd.Wpf.Tokens.
    /// </summary>
    internal static class Metrics
    {
        public static double ControlHeight(AliasToken t, AntdSize size)
        {
            if (size == AntdSize.Small) return t.ControlHeightSM;
            if (size == AntdSize.Large) return t.ControlHeightLG;
            return t.ControlHeight;
        }

        public static double FontSize(AliasToken t, AntdSize size)
        {
            return size == AntdSize.Large ? t.FontSizeLG : t.FontSize;
        }

        /// <summary>Button.paddingInline = 15, Button.paddingInlineSM = 7.</summary>
        public static double ButtonPaddingInline(AntdSize size)
        {
            return size == AntdSize.Small ? 7 : 15;
        }

        /// <summary>Input padding: 4px 11px / 0 7px / 6px 11px.</summary>
        public static double InputPaddingInline(AntdSize size)
        {
            return size == AntdSize.Small ? 7 : 11;
        }

        public static double Radius(AliasToken t, AntdSize size)
        {
            if (size == AntdSize.Small) return t.BorderRadiusSM;
            if (size == AntdSize.Large) return t.BorderRadiusLG;
            return t.BorderRadius;
        }
    }
}
