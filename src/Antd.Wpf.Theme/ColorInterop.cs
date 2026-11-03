using System;
using System.Windows.Media;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Theme
{
    /// <summary>Bridges platform independent token colors with WPF types.</summary>
    public static class ColorInterop
    {
        public static Color ToMediaColor(AntdColor color)
        {
            byte a = (byte)Math.Round(color.Alpha * 255);
            return Color.FromArgb(a, color.R, color.G, color.B);
        }

        public static SolidColorBrush ToFrozenBrush(AntdColor color)
        {
            var brush = new SolidColorBrush(ToMediaColor(color));
            brush.Freeze();
            return brush;
        }
    }
}
