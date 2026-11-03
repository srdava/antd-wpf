using System.Collections.Generic;

namespace Antd.Wpf.Tokens
{
    /// <summary>
    /// Fully derived token set (alias level). Immutable once built.
    /// Values are kept in a string-keyed table so parity tests can compare the whole set
    /// against antd's <c>theme.getDesignToken()</c>, while the strongly typed accessors below
    /// exist so component code never has to use magic strings.
    /// </summary>
    public sealed class AliasToken
    {
        private readonly Dictionary<string, object> _values;

        internal AliasToken(Dictionary<string, object> values, string algorithmName)
        {
            _values = values;
            AlgorithmName = algorithmName;
        }

        public string AlgorithmName { get; private set; }

        public IReadOnlyDictionary<string, object> Values
        {
            get { return _values; }
        }

        public object Get(string key)
        {
            object value;
            return _values.TryGetValue(key, out value) ? value : null;
        }

        public bool TryGet(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }

        public AntdColor Color(string key)
        {
            object value;
            if (_values.TryGetValue(key, out value) && value is AntdColor)
            {
                return (AntdColor)value;
            }
            return AntdColor.Transparent;
        }

        public double Number(string key)
        {
            object value;
            if (_values.TryGetValue(key, out value) && value is double)
            {
                return (double)value;
            }
            return 0;
        }

        public string Text(string key)
        {
            object value;
            if (_values.TryGetValue(key, out value))
            {
                return value as string;
            }
            return null;
        }

        // ------------------------------------------------------------ colors

        public AntdColor ColorPrimary { get { return Color("colorPrimary"); } }
        public AntdColor ColorPrimaryHover { get { return Color("colorPrimaryHover"); } }
        public AntdColor ColorPrimaryActive { get { return Color("colorPrimaryActive"); } }
        public AntdColor ColorPrimaryBg { get { return Color("colorPrimaryBg"); } }
        public AntdColor ColorPrimaryBgHover { get { return Color("colorPrimaryBgHover"); } }
        public AntdColor ColorPrimaryBorder { get { return Color("colorPrimaryBorder"); } }
        public AntdColor ColorPrimaryBorderHover { get { return Color("colorPrimaryBorderHover"); } }
        public AntdColor ColorPrimaryText { get { return Color("colorPrimaryText"); } }

        public AntdColor ColorSuccess { get { return Color("colorSuccess"); } }
        public AntdColor ColorWarning { get { return Color("colorWarning"); } }
        public AntdColor ColorError { get { return Color("colorError"); } }
        public AntdColor ColorErrorHover { get { return Color("colorErrorHover"); } }
        public AntdColor ColorErrorActive { get { return Color("colorErrorActive"); } }
        public AntdColor ColorErrorBg { get { return Color("colorErrorBg"); } }
        public AntdColor ColorErrorBorder { get { return Color("colorErrorBorder"); } }
        public AntdColor ColorInfo { get { return Color("colorInfo"); } }
        public AntdColor ColorLink { get { return Color("colorLink"); } }
        public AntdColor ColorLinkHover { get { return Color("colorLinkHover"); } }
        public AntdColor ColorLinkActive { get { return Color("colorLinkActive"); } }

        public AntdColor ColorText { get { return Color("colorText"); } }
        public AntdColor ColorTextSecondary { get { return Color("colorTextSecondary"); } }
        public AntdColor ColorTextTertiary { get { return Color("colorTextTertiary"); } }
        public AntdColor ColorTextQuaternary { get { return Color("colorTextQuaternary"); } }
        public AntdColor ColorTextDisabled { get { return Color("colorTextDisabled"); } }
        public AntdColor ColorTextPlaceholder { get { return Color("colorTextPlaceholder"); } }
        public AntdColor ColorTextHeading { get { return Color("colorTextHeading"); } }
        public AntdColor ColorTextLightSolid { get { return Color("colorTextLightSolid"); } }

        public AntdColor ColorBgBase { get { return Color("colorBgBase"); } }
        public AntdColor ColorBgContainer { get { return Color("colorBgContainer"); } }
        public AntdColor ColorBgContainerDisabled { get { return Color("colorBgContainerDisabled"); } }
        public AntdColor ColorBgElevated { get { return Color("colorBgElevated"); } }
        public AntdColor ColorBgLayout { get { return Color("colorBgLayout"); } }
        public AntdColor ColorBgMask { get { return Color("colorBgMask"); } }
        public AntdColor ColorBgSpotlight { get { return Color("colorBgSpotlight"); } }

        public AntdColor ColorBorder { get { return Color("colorBorder"); } }
        public AntdColor ColorBorderSecondary { get { return Color("colorBorderSecondary"); } }
        public AntdColor ColorBorderDisabled { get { return Color("colorBorderDisabled"); } }
        public AntdColor ColorSplit { get { return Color("colorSplit"); } }

        public AntdColor ColorFill { get { return Color("colorFill"); } }
        public AntdColor ColorFillSecondary { get { return Color("colorFillSecondary"); } }
        public AntdColor ColorFillTertiary { get { return Color("colorFillTertiary"); } }
        public AntdColor ColorFillQuaternary { get { return Color("colorFillQuaternary"); } }
        public AntdColor ColorFillAlter { get { return Color("colorFillAlter"); } }

        public AntdColor ControlItemBgHover { get { return Color("controlItemBgHover"); } }
        public AntdColor ControlItemBgActive { get { return Color("controlItemBgActive"); } }
        public AntdColor ControlOutline { get { return Color("controlOutline"); } }

        // ------------------------------------------------------------ typography

        public string FontFamily { get { return Text("fontFamily"); } }
        public string FontFamilyCode { get { return Text("fontFamilyCode"); } }
        public double FontSize { get { return Number("fontSize"); } }
        public double FontSizeSM { get { return Number("fontSizeSM"); } }
        public double FontSizeLG { get { return Number("fontSizeLG"); } }
        public double FontSizeXL { get { return Number("fontSizeXL"); } }
        public double FontSizeIcon { get { return Number("fontSizeIcon"); } }
        public double LineHeight { get { return Number("lineHeight"); } }
        public double LineHeightSM { get { return Number("lineHeightSM"); } }
        public double LineHeightLG { get { return Number("lineHeightLG"); } }
        public double FontHeight { get { return Number("fontHeight"); } }
        public double FontHeightSM { get { return Number("fontHeightSM"); } }
        public double FontHeightLG { get { return Number("fontHeightLG"); } }
        public double FontWeightStrong { get { return Number("fontWeightStrong"); } }

        // ------------------------------------------------------------ geometry

        public double BorderRadius { get { return Number("borderRadius"); } }
        public double BorderRadiusXS { get { return Number("borderRadiusXS"); } }
        public double BorderRadiusSM { get { return Number("borderRadiusSM"); } }
        public double BorderRadiusLG { get { return Number("borderRadiusLG"); } }
        public double BorderRadiusOuter { get { return Number("borderRadiusOuter"); } }

        public double LineWidth { get { return Number("lineWidth"); } }
        public double LineWidthBold { get { return Number("lineWidthBold"); } }
        public double LineWidthFocus { get { return Number("lineWidthFocus"); } }
        public double ControlOutlineWidth { get { return Number("controlOutlineWidth"); } }
        public double ControlInteractiveSize { get { return Number("controlInteractiveSize"); } }

        public double ControlHeight { get { return Number("controlHeight"); } }
        public double ControlHeightXS { get { return Number("controlHeightXS"); } }
        public double ControlHeightSM { get { return Number("controlHeightSM"); } }
        public double ControlHeightLG { get { return Number("controlHeightLG"); } }

        public double SizeXXS { get { return Number("sizeXXS"); } }
        public double SizeXS { get { return Number("sizeXS"); } }
        public double SizeSM { get { return Number("sizeSM"); } }
        public double Size { get { return Number("size"); } }
        public double SizeMS { get { return Number("sizeMS"); } }
        public double SizeMD { get { return Number("sizeMD"); } }
        public double SizeLG { get { return Number("sizeLG"); } }
        public double SizeXL { get { return Number("sizeXL"); } }
        public double SizeXXL { get { return Number("sizeXXL"); } }

        public double PaddingXXS { get { return Number("paddingXXS"); } }
        public double PaddingXS { get { return Number("paddingXS"); } }
        public double PaddingSM { get { return Number("paddingSM"); } }
        public double Padding { get { return Number("padding"); } }
        public double PaddingMD { get { return Number("paddingMD"); } }
        public double PaddingLG { get { return Number("paddingLG"); } }
        public double PaddingXL { get { return Number("paddingXL"); } }
        public double ControlPaddingHorizontal { get { return Number("controlPaddingHorizontal"); } }
        public double ControlPaddingHorizontalSM { get { return Number("controlPaddingHorizontalSM"); } }

        // ------------------------------------------------------------ motion / effects

        public string MotionDurationFast { get { return Text("motionDurationFast"); } }
        public string MotionDurationMid { get { return Text("motionDurationMid"); } }
        public string MotionDurationSlow { get { return Text("motionDurationSlow"); } }
        public string MotionEaseOut { get { return Text("motionEaseOut"); } }
        public string MotionEaseInOut { get { return Text("motionEaseInOut"); } }
        public string MotionEaseOutQuint { get { return Text("motionEaseOutQuint"); } }

        public string BoxShadow { get { return Text("boxShadow"); } }
        public string BoxShadowSecondary { get { return Text("boxShadowSecondary"); } }
        public string BoxShadowTertiary { get { return Text("boxShadowTertiary"); } }

        public double OpacityLoading { get { return Number("opacityLoading"); } }
    }
}
