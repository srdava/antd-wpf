using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum GridAlign
    {
        Top = 0,
        Middle = 1,
        Bottom = 2,
        Stretch = 3
    }

    public enum Breakpoint
    {
        Xs = 0,
        Sm = 1,
        Md = 2,
        Lg = 3,
        Xl = 4,
        Xxl = 5,
        Xxxl = 6
    }

    /// <summary>
    /// antd Col. Holds the 24-grid placement; <see cref="Row"/> reads it during measure.
    /// Responsive overrides use -1 for "not set".
    /// </summary>
    public class Col : ContentControl
    {
        static Col()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Col),
                new FrameworkPropertyMetadata(typeof(Col)));
        }

        public static readonly DependencyProperty SpanProperty = DependencyProperty.Register(
            "Span", typeof(int), typeof(Col),
            new FrameworkPropertyMetadata(24, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public int Span
        {
            get { return (int)GetValue(SpanProperty); }
            set { SetValue(SpanProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(int), typeof(Col),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public int Offset
        {
            get { return (int)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty XsProperty = RegisterResponsive("Xs");
        public static readonly DependencyProperty SmProperty = RegisterResponsive("Sm");
        public static readonly DependencyProperty MdProperty = RegisterResponsive("Md");
        public static readonly DependencyProperty LgProperty = RegisterResponsive("Lg");
        public static readonly DependencyProperty XlProperty = RegisterResponsive("Xl");
        public static readonly DependencyProperty XxlProperty = RegisterResponsive("Xxl");

        public int Xs { get { return (int)GetValue(XsProperty); } set { SetValue(XsProperty, value); } }
        public int Sm { get { return (int)GetValue(SmProperty); } set { SetValue(SmProperty, value); } }
        public int Md { get { return (int)GetValue(MdProperty); } set { SetValue(MdProperty, value); } }
        public int Lg { get { return (int)GetValue(LgProperty); } set { SetValue(LgProperty, value); } }
        public int Xl { get { return (int)GetValue(XlProperty); } set { SetValue(XlProperty, value); } }
        public int Xxl { get { return (int)GetValue(XxlProperty); } set { SetValue(XxlProperty, value); } }

        private static DependencyProperty RegisterResponsive(string name)
        {
            return DependencyProperty.Register(name, typeof(int), typeof(Col),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        }

        /// <summary>Resolves the span for the effective breakpoint, falling back to Span.</summary>
        public int ResolveSpan(Breakpoint breakpoint)
        {
            int value = Span;
            if (breakpoint >= Breakpoint.Xxl && Xxl >= 0) value = Xxl;
            else if (breakpoint >= Breakpoint.Xl && Xl >= 0) value = Xl;
            else if (breakpoint >= Breakpoint.Lg && Lg >= 0) value = Lg;
            else if (breakpoint >= Breakpoint.Md && Md >= 0) value = Md;
            else if (breakpoint >= Breakpoint.Sm && Sm >= 0) value = Sm;
            else if (Xs >= 0) value = Xs;

            if (value < 0) value = 0;
            if (value > 24) value = 24;
            return value;
        }
    }

    /// <summary>
    /// antd Row. A 24-column grid with gutter, wrapping and responsive spans.
    /// Implemented as a Panel because WPF has no equivalent of CSS grid placement.
    /// </summary>
    public class Row : Panel
    {
        private sealed class Placement
        {
            public UIElement Child;
            public Rect Bounds;
        }

        private readonly List<Placement> _placements = new List<Placement>();

        public static readonly DependencyProperty GutterProperty = DependencyProperty.Register(
            "Gutter", typeof(double), typeof(Row),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Horizontal gap between columns, in DIPs (antd recommends 16 + 8n).</summary>
        public double Gutter
        {
            get { return (double)GetValue(GutterProperty); }
            set { SetValue(GutterProperty, value); }
        }

        public static readonly DependencyProperty RowGutterProperty = DependencyProperty.Register(
            "RowGutter", typeof(double), typeof(Row),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Vertical gap between wrapped lines.</summary>
        public double RowGutter
        {
            get { return (double)GetValue(RowGutterProperty); }
            set { SetValue(RowGutterProperty, value); }
        }

        public static readonly DependencyProperty AlignProperty = DependencyProperty.Register(
            "Align", typeof(GridAlign), typeof(Row),
            new FrameworkPropertyMetadata(GridAlign.Top, FrameworkPropertyMetadataOptions.AffectsArrange));

        public GridAlign Align
        {
            get { return (GridAlign)GetValue(AlignProperty); }
            set { SetValue(AlignProperty, value); }
        }

        private static readonly DependencyPropertyKey BreakpointKey = DependencyProperty.RegisterReadOnly(
            "CurrentBreakpoint", typeof(Breakpoint), typeof(Row),
            new FrameworkPropertyMetadata(Breakpoint.Xs));

        public static readonly DependencyProperty CurrentBreakpointProperty = BreakpointKey.DependencyProperty;

        public Breakpoint CurrentBreakpoint
        {
            get { return (Breakpoint)GetValue(CurrentBreakpointProperty); }
            private set { SetValue(BreakpointKey, value); }
        }

        public static Breakpoint BreakpointFor(double width)
        {
            if (width >= 1920) return Breakpoint.Xxxl;
            if (width >= 1600) return Breakpoint.Xxl;
            if (width >= 1200) return Breakpoint.Xl;
            if (width >= 992) return Breakpoint.Lg;
            if (width >= 768) return Breakpoint.Md;
            if (width >= 576) return Breakpoint.Sm;
            return Breakpoint.Xs;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _placements.Clear();

            double available = double.IsInfinity(availableSize.Width) ? 960 : availableSize.Width;
            Breakpoint breakpoint = BreakpointFor(available);
            SetValue(BreakpointKey, breakpoint);

            double gutter = Gutter > 0 ? Gutter : 0;
            double rowGutter = RowGutter > 0 ? RowGutter : 0;

            var line = new List<Col>();
            var lines = new List<List<Col>>();
            int used = 0;

            foreach (UIElement child in InternalChildren)
            {
                var col = child as Col;
                if (col == null || col.Visibility == Visibility.Collapsed) continue;

                int span = col.ResolveSpan(breakpoint);
                int offset = Math.Max(0, col.Offset);
                if (used + offset + span > 24 && line.Count > 0)
                {
                    lines.Add(line);
                    line = new List<Col>();
                    used = 0;
                }
                used += offset + span;
                line.Add(col);
            }
            if (line.Count > 0) lines.Add(line);

            double totalHeight = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                List<Col> current = lines[i];
                double gapTotal = gutter * Math.Max(0, current.Count - 1);
                double unit = Math.Max(0, (available - gapTotal) / 24.0);
                double x = 0;
                double lineHeight = 0;

                foreach (Col col in current)
                {
                    int span = col.ResolveSpan(breakpoint);
                    int offset = Math.Max(0, col.Offset);
                    x += unit * offset;

                    double width = unit * span;
                    col.Measure(new Size(width, double.PositiveInfinity));
                    _placements.Add(new Placement { Child = col, Bounds = new Rect(x, totalHeight, width, col.DesiredSize.Height) });

                    if (col.DesiredSize.Height > lineHeight) lineHeight = col.DesiredSize.Height;
                    x += width + gutter;
                }

                for (int p = _placements.Count - current.Count; p < _placements.Count; p++)
                {
                    Placement placement = _placements[p];
                    placement.Bounds = new Rect(placement.Bounds.X, placement.Bounds.Y, placement.Bounds.Width, lineHeight);
                }

                totalHeight += lineHeight;
                if (i < lines.Count - 1) totalHeight += rowGutter;
            }

            return new Size(available, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (Placement placement in _placements)
            {
                Rect bounds = placement.Bounds;
                double height = bounds.Height;

                if (Align == GridAlign.Middle || Align == GridAlign.Bottom)
                {
                    double desired = placement.Child.DesiredSize.Height;
                    if (desired < height)
                    {
                        double offset = Align == GridAlign.Middle ? (height - desired) / 2 : height - desired;
                        bounds = new Rect(bounds.X, bounds.Y + offset, bounds.Width, desired);
                    }
                }

                placement.Child.Arrange(bounds);
            }
            return finalSize;
        }
    }
}
