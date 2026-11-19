using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Watermark: a tiled text pattern painted behind the content.
    /// The tile is built as a DrawingBrush so it costs one brush instead of N visual children.
    /// </summary>
    public class Watermark : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Pane, "Watermark");
        }
        static Watermark()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Watermark), new FrameworkPropertyMetadata(typeof(Watermark)));
        }

        public Watermark()
        {
            ThemeLink.Attach(this, OnThemeChanged);
            RebuildBrush();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Watermark),
            new FrameworkPropertyMetadata("Ant Design", OnBrushChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty RotateProperty = DependencyProperty.Register(
            "Rotate", typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(-22.0, OnBrushChanged));

        public double Rotate
        {
            get { return (double)GetValue(RotateProperty); }
            set { SetValue(RotateProperty, value); }
        }

        public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
            "Gap", typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(80.0, OnBrushChanged));

        public double Gap
        {
            get { return (double)GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }

        public static readonly DependencyProperty WatermarkFontSizeProperty = DependencyProperty.Register(
            "WatermarkFontSize", typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(double.NaN, OnBrushChanged));

        public double WatermarkFontSize
        {
            get { return (double)GetValue(WatermarkFontSizeProperty); }
            set { SetValue(WatermarkFontSizeProperty, value); }
        }

        private static readonly DependencyPropertyKey TileBrushKey = DependencyProperty.RegisterReadOnly(
            "TileBrush", typeof(Brush), typeof(Watermark), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TileBrushProperty = TileBrushKey.DependencyProperty;

        public Brush TileBrush
        {
            get { return (Brush)GetValue(TileBrushProperty); }
            private set { SetValue(TileBrushKey, value); }
        }

        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Watermark)d).RebuildBrush();
        }

        private void OnThemeChanged()
        {
            RebuildBrush();
        }

        private void RebuildBrush()
        {
            string text = Text;
            if (string.IsNullOrEmpty(text))
            {
                SetValue(TileBrushKey, null);
                return;
            }

            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            double fontSize = double.IsNaN(WatermarkFontSize) ? snapshot.Tokens.FontSize : WatermarkFontSize;
            double gap = Gap <= 0 ? 80 : Gap;

            var formatted = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(snapshot.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black,
                1.0);

            Geometry geometry = formatted.BuildGeometry(new Point(0, 0));
            Rect bounds = geometry.Bounds;
            double width = bounds.Width + gap;
            double height = bounds.Height + gap;

            var group = new DrawingGroup();
            using (DrawingContext context = group.Open())
            {
                context.PushTransform(new RotateTransform(Rotate, bounds.Width / 2, bounds.Height / 2));
                context.DrawGeometry(snapshot.Brush("colorTextQuaternary"), null, geometry);
                context.Pop();
            }

            var brush = new DrawingBrush(group)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, width, height),
                ViewportUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None,
                Opacity = 0.6
            };
            brush.Freeze();

            SetValue(TileBrushKey, brush);
        }
    }
}
