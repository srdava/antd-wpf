using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Layout. Lays out Header/Content/Footer vertically and pulls a Sider to the side
    /// when one is present (antd's `hasSider` detection).
    /// </summary>
    public class Layout : Panel
    {
        public Layout()
        {
            ThemeLink.Attach(this, ApplyBackground);
            ApplyBackground();
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(Layout),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private bool HasSider
        {
            get
            {
                foreach (UIElement child in InternalChildren)
                {
                    if (child is Sider) return true;
                }
                return false;
            }
        }

        private Orientation EffectiveOrientation
        {
            get { return HasSider ? Orientation.Horizontal : Orientation.Vertical; }
        }

        private void ApplyBackground()
        {
            SetCurrentValue(BackgroundProperty, ThemeManager.Current.Snapshot.Brush("colorBgLayout"));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            bool horizontal = EffectiveOrientation == Orientation.Horizontal;
            double main = 0;
            double cross = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed) continue;

                var sider = child as Sider;
                if (horizontal && sider != null)
                {
                    double width = sider.EffectiveWidth;
                    child.Measure(new Size(width, double.PositiveInfinity));
                    main += width;
                    cross = Math.Max(cross, child.DesiredSize.Height);
                }
                else if (horizontal)
                {
                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    main += child.DesiredSize.Width;
                    cross = Math.Max(cross, child.DesiredSize.Height);
                }
                else
                {
                    child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                    main += child.DesiredSize.Height;
                    cross = Math.Max(cross, child.DesiredSize.Width);
                }
            }

            return horizontal ? new Size(main, cross) : new Size(cross, main);
        }

        private static bool IsFlexible(UIElement child)
        {
            return child is Content || child is Layout;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            bool horizontal = EffectiveOrientation == Orientation.Horizontal;

            var children = new List<UIElement>();
            foreach (UIElement child in InternalChildren)
            {
                if (child != null && child.Visibility != Visibility.Collapsed) children.Add(child);
            }

            if (horizontal)
            {
                double fixedWidth = 0;
                int flexibleCount = 0;
                foreach (UIElement child in children)
                {
                    var sider = child as Sider;
                    if (sider != null) fixedWidth += sider.EffectiveWidth;
                    else flexibleCount++;
                }

                double flexible = flexibleCount == 0 ? 0 : Math.Max(0, finalSize.Width - fixedWidth) / flexibleCount;
                double x = 0;

                foreach (UIElement child in children)
                {
                    var sider = child as Sider;
                    double width = sider != null ? sider.EffectiveWidth : flexible;
                    child.Arrange(new Rect(x, 0, width, finalSize.Height));
                    x += width;
                }
            }
            else
            {
                // Content and nested Layouts absorb the leftover height (antd's flex: auto);
                // Header/Footer keep their intrinsic height.
                double fixedHeight = 0;
                int flexCount = 0;
                foreach (UIElement child in children)
                {
                    if (IsFlexible(child)) flexCount++;
                    else fixedHeight += child.DesiredSize.Height;
                }

                double flexible = flexCount == 0 ? 0 : Math.Max(0, finalSize.Height - fixedHeight) / flexCount;
                double y = 0;

                foreach (UIElement child in children)
                {
                    double height = IsFlexible(child) ? Math.Max(flexible, child.DesiredSize.Height) : child.DesiredSize.Height;
                    child.Arrange(new Rect(0, y, finalSize.Width, height));
                    y += height;
                }
            }

            return finalSize;
        }
    }

    /// <summary>antd Layout.Sider.</summary>
    public class Sider : ContentControl
    {
        static Sider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Sider),
                new FrameworkPropertyMetadata(typeof(Sider)));
        }

        public Sider()
        {
            ThemeLink.Attach(this, ApplyTheme);
            ApplyTheme();
        }

        public static readonly DependencyProperty SiderWidthProperty = DependencyProperty.Register(
            "SiderWidth", typeof(double), typeof(Sider),
            new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public double SiderWidth
        {
            get { return (double)GetValue(SiderWidthProperty); }
            set { SetValue(SiderWidthProperty, value); }
        }

        public static readonly DependencyProperty CollapsedProperty = DependencyProperty.Register(
            "Collapsed", typeof(bool), typeof(Sider),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public bool Collapsed
        {
            get { return (bool)GetValue(CollapsedProperty); }
            set { SetValue(CollapsedProperty, value); }
        }

        public static readonly DependencyProperty CollapsedWidthProperty = DependencyProperty.Register(
            "CollapsedWidth", typeof(double), typeof(Sider),
            new FrameworkPropertyMetadata(80.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public double CollapsedWidth
        {
            get { return (double)GetValue(CollapsedWidthProperty); }
            set { SetValue(CollapsedWidthProperty, value); }
        }

        /// <summary>Width actually used by the parent layout.</summary>
        public double EffectiveWidth
        {
            get { return Collapsed ? CollapsedWidth : SiderWidth; }
        }

        private void ApplyTheme()
        {
            SetCurrentValue(BackgroundProperty, ThemeManager.Current.Snapshot.Brush("colorBgContainer"));
        }
    }

    /// <summary>antd Layout.Header.</summary>
    public class Header : ContentControl
    {
        static Header()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Header),
                new FrameworkPropertyMetadata(typeof(Header)));
        }

        public Header()
        {
            ThemeLink.Attach(this, ApplyTheme);
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(MinHeightProperty, 64.0);
            SetCurrentValue(PaddingProperty, new Thickness(t.PaddingLG, 0, t.PaddingLG, 0));
            SetCurrentValue(BackgroundProperty, ThemeManager.Current.Snapshot.Brush("colorBgContainer"));
        }
    }

    /// <summary>antd Layout.Footer.</summary>
    public class Footer : ContentControl
    {
        static Footer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Footer),
                new FrameworkPropertyMetadata(typeof(Footer)));
        }

        public Footer()
        {
            ThemeLink.Attach(this, ApplyTheme);
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(PaddingProperty, new Thickness(t.PaddingLG));
            SetCurrentValue(BackgroundProperty, ThemeManager.Current.Snapshot.Brush("colorBgLayout"));
        }
    }

    /// <summary>antd Layout.Content: takes the remaining space of its parent layout.</summary>
    public class Content : ContentControl
    {
        static Content()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Content),
                new FrameworkPropertyMetadata(typeof(Content)));
        }

        public Content()
        {
            ThemeLink.Attach(this, ApplyTheme);
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            SetCurrentValue(PaddingProperty, new Thickness(ThemeManager.Current.Snapshot.Tokens.PaddingLG));
        }
    }
}
