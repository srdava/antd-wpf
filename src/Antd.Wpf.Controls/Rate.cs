using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Rate. Stars are drawn directly so no icon dependency is needed and half values
    /// can be expressed by clipping the star rect.
    /// </summary>
    public class Rate : Control
    {
        private double _hoverValue = -1;

        static Rate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Rate), new FrameworkPropertyMetadata(typeof(Rate)));
        }

        public Rate()
        {
            ThemeLink.Attach(this, InvalidateVisual);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(Rate),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof(int), typeof(Rate),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty AllowHalfProperty = DependencyProperty.Register(
            "AllowHalf", typeof(bool), typeof(Rate), new FrameworkPropertyMetadata(false));

        public bool AllowHalf
        {
            get { return (bool)GetValue(AllowHalfProperty); }
            set { SetValue(AllowHalfProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(Rate), new FrameworkPropertyMetadata(false));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty StarSizeProperty = DependencyProperty.Register(
            "StarSize", typeof(double), typeof(Rate),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double StarSize
        {
            get { return (double)GetValue(StarSizeProperty); }
            set { SetValue(StarSizeProperty, value); }
        }

        public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
            "Gap", typeof(double), typeof(Rate),
            new FrameworkPropertyMetadata(8.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double Gap
        {
            get { return (double)GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            int count = Count <= 0 ? 5 : Count;
            return new Size(count * StarSize + (count - 1) * Gap, StarSize);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsReadOnly || !IsEnabled) return;

            Point point = e.GetPosition(this);
            double value = ValueFromPoint(point.X);
            if (Math.Abs(value - _hoverValue) > 0.001)
            {
                _hoverValue = value;
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverValue < 0) return;
            _hoverValue = -1;
            InvalidateVisual();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsReadOnly || !IsEnabled) return;

            double value = ValueFromPoint(e.GetPosition(this).X);
            SetCurrentValue(ValueProperty, value);
            InvalidateVisual();
        }

        private double ValueFromPoint(double x)
        {
            double unit = StarSize + Gap;
            int index = (int)Math.Floor(x / unit) + 1;
            if (index < 1) index = 1;
            if (index > Count) index = Count;

            double within = x - (index - 1) * unit;
            if (AllowHalf && within < StarSize / 2) return index - 0.5;
            return index;
        }

        protected override void OnRender(DrawingContext dc)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            Brush filled = snapshot.Brush(IsEnabled ? "gold" : "colorTextDisabled");
            Brush empty = snapshot.Brush("colorFill");

            double display = _hoverValue >= 0 ? _hoverValue : Value;
            int count = Count <= 0 ? 5 : Count;
            double unit = StarSize + Gap;

            for (int i = 0; i < count; i++)
            {
                var origin = new Point(i * unit, 0);
                Geometry star = BuildStar(origin, StarSize);

                double amount = display - i;
                if (amount <= 0)
                {
                    dc.DrawGeometry(empty, null, star);
                }
                else if (amount >= 1)
                {
                    dc.DrawGeometry(filled, null, star);
                }
                else
                {
                    dc.DrawGeometry(empty, null, star);
                    dc.PushClip(new RectangleGeometry(new Rect(origin.X, 0, StarSize * amount, StarSize)));
                    dc.DrawGeometry(filled, null, star);
                    dc.Pop();
                }
            }
        }

        /// <summary>Builds a five point star inscribed in a square of the given size.</summary>
        internal static Geometry BuildStar(Point origin, double size)
        {
            double outer = size / 2;
            double inner = outer * 0.48;
            var center = new Point(origin.X + outer, origin.Y + outer);

            var geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                for (int i = 0; i < 10; i++)
                {
                    double radius = i % 2 == 0 ? outer : inner;
                    double angle = -Math.PI / 2 + i * Math.PI / 5;
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    if (i == 0) context.BeginFigure(point, true, true);
                    else context.LineTo(point, true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }
}
