using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum AntdProgressType
    {
        Line = 0,
        Circle = 1
    }

    public enum AntdProgressStatus
    {
        Normal = 0,
        Active = 1,
        Success = 2,
        Exception = 3
    }

    /// <summary>
    /// antd Progress. Drawn in <see cref="OnRender"/> rather than templated: the bar geometry
    /// is proportional to Percent, which would otherwise need star-column gymnastics and a
    /// separate arc builder for the circle type.
    /// </summary>
    public class Progress : Control
    {
        private const double LineThickness = 8;

        static Progress()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Progress), new FrameworkPropertyMetadata(typeof(Progress)));
        }

        public Progress()
        {
            ThemeLink.Attach(this, OnThemeChanged);
            OnThemeChanged();
        }

        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(
            "Percent", typeof(double), typeof(Progress),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Percent
        {
            get { return (double)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(AntdProgressType), typeof(Progress),
            new FrameworkPropertyMetadata(AntdProgressType.Line, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public AntdProgressType Type
        {
            get { return (AntdProgressType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(AntdProgressStatus), typeof(Progress),
            new FrameworkPropertyMetadata(AntdProgressStatus.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public AntdProgressStatus Status
        {
            get { return (AntdProgressStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(
            "ShowInfo", typeof(bool), typeof(Progress),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowInfo
        {
            get { return (bool)GetValue(ShowInfoProperty); }
            set { SetValue(ShowInfoProperty, value); }
        }

        public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register(
            "StrokeWidth", typeof(double), typeof(Progress),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeWidth
        {
            get { return (double)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public static readonly DependencyProperty CircleSizeProperty = DependencyProperty.Register(
            "CircleSize", typeof(double), typeof(Progress),
            new FrameworkPropertyMetadata(120.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double CircleSize
        {
            get { return (double)GetValue(CircleSizeProperty); }
            set { SetValue(CircleSizeProperty, value); }
        }

        private void OnThemeChanged()
        {
            // FontSize is layout state: changing it from OnRender would invalidate layout while
            // painting and could re-arrange this control wider than its parent.
            SetCurrentValue(FontSizeProperty, ThemeManager.Current.Snapshot.Tokens.FontSize);
            InvalidateMeasure();
            InvalidateVisual();
        }

        private double Thickness
        {
            get { return double.IsNaN(StrokeWidth) ? (Type == AntdProgressType.Line ? LineThickness : 6) : StrokeWidth; }
        }

        private string InfoText
        {
            get
            {
                if (!ShowInfo) return string.Empty;
                if (Status == AntdProgressStatus.Success) return "100%";
                if (Status == AntdProgressStatus.Exception) return "!";
                return Math.Round(Percent, 0).ToString(CultureInfo.InvariantCulture) + "%";
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            double fontSize = snapshot.Tokens.FontSize;

            if (Type == AntdProgressType.Circle)
            {
                return new Size(CircleSize, CircleSize);
            }

            double infoWidth = ShowInfo ? MeasureInfoWidth(fontSize) + snapshot.Tokens.SizeXS : 0;
            double width = double.IsInfinity(availableSize.Width) ? 200 + infoWidth : availableSize.Width;
            return new Size(width, Math.Max(Thickness, fontSize));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            double percent = Math.Max(0, Math.Min(100, Percent)) / 100.0;

            Brush fill = ResolveFillBrush(snapshot);
            Brush trail = snapshot.Brush("colorFillSecondary");

            if (Type == AntdProgressType.Circle)
            {
                DrawCircle(drawingContext, snapshot, percent, fill, trail);
            }
            else
            {
                DrawLine(drawingContext, snapshot, percent, fill, trail);
            }
        }

        private Brush ResolveFillBrush(ThemeSnapshot snapshot)
        {
            if (Status == AntdProgressStatus.Exception) return snapshot.Brush("colorError");
            if (Status == AntdProgressStatus.Success) return snapshot.Brush("colorSuccess");
            return snapshot.Brush("colorPrimary");
        }

        private void DrawLine(DrawingContext dc, ThemeSnapshot snapshot, double percent, Brush fill, Brush trail)
        {
            double fontSize = snapshot.Tokens.FontSize;
            double infoWidth = ShowInfo ? MeasureInfoWidth(fontSize) + snapshot.Tokens.SizeXS : 0;
            double barWidth = Math.Max(0, ActualWidth - infoWidth);
            double height = Thickness;
            double top = (ActualHeight - height) / 2;
            double radius = height / 2;

            dc.DrawRoundedRectangle(trail, null, new Rect(0, top, barWidth, height), radius, radius);
            if (percent > 0)
            {
                double filled = Math.Max(height, barWidth * percent);
                dc.DrawRoundedRectangle(fill, null, new Rect(0, top, filled, height), radius, radius);
            }

            if (!ShowInfo || infoWidth <= 0) return;
            FormattedText text = CreateText(InfoText, fontSize, snapshot.Brush("colorText"));
            dc.DrawText(text, new Point(ActualWidth - infoWidth + snapshot.Tokens.SizeXS,
                (ActualHeight - text.Height) / 2));
        }

        private void DrawCircle(DrawingContext dc, ThemeSnapshot snapshot, double percent, Brush fill, Brush trail)
        {
            double stroke = Thickness;
            double radius = (Math.Min(ActualWidth, ActualHeight) - stroke) / 2;
            if (radius <= 0) return;

            var center = new Point(ActualWidth / 2, ActualHeight / 2);
            dc.DrawEllipse(null, new Pen(trail, stroke), center, radius, radius);

            if (percent > 0.0001)
            {
                var geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    double startAngle = -90;
                    double sweep = 359.999 * percent;
                    Point start = PointOnCircle(center, radius, startAngle);
                    Point end = PointOnCircle(center, radius, startAngle + sweep);

                    context.BeginFigure(start, false, false);
                    context.ArcTo(end, new Size(radius, radius), 0,
                        sweep > 180, SweepDirection.Clockwise, true, false);
                }
                geometry.Freeze();
                dc.DrawGeometry(null, new Pen(fill, stroke) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, geometry);
            }

            if (!ShowInfo) return;
            double fontSize = snapshot.Tokens.FontSizeXL;
            FormattedText text = CreateText(InfoText, fontSize, snapshot.Brush("colorText"));
            dc.DrawText(text, new Point((ActualWidth - text.Width) / 2, (ActualHeight - text.Height) / 2));
        }

        private static Point PointOnCircle(Point center, double radius, double angleDegrees)
        {
            double radians = angleDegrees * Math.PI / 180.0;
            return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        }

        private double MeasureInfoWidth(double fontSize)
        {
            string text = string.IsNullOrEmpty(InfoText) ? "100%" : InfoText;
            return CreateText(text, fontSize, Brushes.Black).WidthIncludingTrailingWhitespace;
        }

        private FormattedText CreateText(string text, double fontSize, Brush brush)
        {
            return new FormattedText(
                text ?? string.Empty,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                fontSize,
                brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }
}
