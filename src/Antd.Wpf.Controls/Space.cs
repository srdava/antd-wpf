using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Space: lays children out in a row or column with a uniform gap.
    /// A dedicated panel is used instead of StackPanel + margins so the gap never leaks
    /// onto the outer edges (the classic antd Space behaviour).
    /// </summary>
    public class Space : Panel
    {
        public Space()
        {
            ThemeLink.Attach(this, OnThemeChanged);
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(Space),
            new FrameworkPropertyMetadata(Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            "Spacing", typeof(double), typeof(Space),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Gap in DIPs. NaN means "use the antd size token" (16 by default).</summary>
        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Space),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnSizeChanged));

        /// <summary>antd Space size: small = 8, middle = 16, large = 24.</summary>
        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Space)d).ResolveSpacing();
        }

        private void OnThemeChanged()
        {
            ResolveSpacing();
        }

        private double _resolved = 16;

        private void ResolveSpacing()
        {
            double value = ThemeManager.Current.Snapshot.Tokens.Size;
            switch (Size)
            {
                case AntdSize.Small: value = ThemeManager.Current.Snapshot.Tokens.SizeXS; break;
                case AntdSize.Large: value = ThemeManager.Current.Snapshot.Tokens.SizeLG; break;
            }

            _resolved = value;
            InvalidateMeasure();
        }

        private double Gap
        {
            get { return double.IsNaN(Spacing) ? _resolved : Spacing; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            bool horizontal = Orientation == Orientation.Horizontal;
            double gap = Gap;
            double main = 0;
            double cross = 0;
            int visible = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed) continue;

                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size desired = child.DesiredSize;

                if (horizontal)
                {
                    if (visible > 0) main += gap;
                    main += desired.Width;
                    cross = Math.Max(cross, desired.Height);
                }
                else
                {
                    if (visible > 0) main += gap;
                    main += desired.Height;
                    cross = Math.Max(cross, desired.Width);
                }
                visible++;
            }

            return horizontal ? new Size(main, cross) : new Size(cross, main);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            bool horizontal = Orientation == Orientation.Horizontal;
            double gap = Gap;
            double offset = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed) continue;

                Size desired = child.DesiredSize;
                if (horizontal)
                {
                    child.Arrange(new Rect(offset, 0, desired.Width, Math.Max(desired.Height, finalSize.Height)));
                    offset += desired.Width + gap;
                }
                else
                {
                    child.Arrange(new Rect(0, offset, Math.Max(desired.Width, finalSize.Width), desired.Height));
                    offset += desired.Height + gap;
                }
            }

            return finalSize;
        }
    }
}
