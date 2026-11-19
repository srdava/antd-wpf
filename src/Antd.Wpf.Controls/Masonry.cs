using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Masonry: items flow into the shortest column. Heights differ per item, so the panel
    /// cannot use a uniform grid; it measures every child first, then balances.
    /// </summary>
    public class Masonry : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Masonry");
        }
        static Masonry()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Masonry), new FrameworkPropertyMetadata(typeof(Masonry)));
        }

        public Masonry()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(int), typeof(Masonry),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty GutterProperty = DependencyProperty.Register(
            "Gutter", typeof(double), typeof(Masonry),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Gap between columns and between rows.</summary>
        public double Gutter
        {
            get { return (double)GetValue(GutterProperty); }
            set { SetValue(GutterProperty, value); }
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>Balancing panel behind <see cref="Masonry"/>.</summary>
    public class MasonryPanel : Panel
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "MasonryPanel");
        }
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(int), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty GutterProperty = DependencyProperty.Register(
            "Gutter", typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double Gutter
        {
            get { return (double)GetValue(GutterProperty); }
            set { SetValue(GutterProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            int columns = Math.Max(1, Columns);
            double gutter = Math.Max(0, Gutter);
            double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            if (width <= 0) return new Size(0, 0);

            double columnWidth = (width - gutter * (columns - 1)) / columns;
            var heights = new double[columns];
            double widest = columnWidth;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                child.Measure(new Size(columnWidth, double.PositiveInfinity));

                int target = ShortestColumn(heights);
                double height = child.DesiredSize.Height;
                TagColumn(child, target);

                heights[target] += height + (heights[target] > 0 ? gutter : 0);
                widest = Math.Max(widest, child.DesiredSize.Width);
            }

            double total = 0;
            for (int i = 0; i < columns; i++) total = Math.Max(total, heights[i]);

            double measured = columnWidth * columns + gutter * (columns - 1);
            return new Size(double.IsInfinity(availableSize.Width) ? measured : width, total);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int columns = Math.Max(1, Columns);
            double gutter = Math.Max(0, Gutter);
            double columnWidth = (finalSize.Width - gutter * (columns - 1)) / columns;
            var heights = new double[columns];

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                int column = ColumnOf(child);
                if (column < 0) column = ShortestColumn(heights);

                double y = heights[column];
                child.Arrange(new Rect(column * (columnWidth + gutter), y, columnWidth, child.DesiredSize.Height));
                heights[column] = y + child.DesiredSize.Height + gutter;
            }

            double total = 0;
            for (int i = 0; i < columns; i++) total = Math.Max(total, heights[i] - gutter);
            return new Size(finalSize.Width, Math.Max(0, total));
        }

        private static int ShortestColumn(double[] heights)
        {
            int best = 0;
            for (int i = 1; i < heights.Length; i++)
            {
                if (heights[i] < heights[best]) best = i;
            }
            return best;
        }

        private static readonly DependencyProperty ColumnTagProperty = DependencyProperty.RegisterAttached(
            "ColumnTag", typeof(int), typeof(MasonryPanel), new PropertyMetadata(-1));

        private static void TagColumn(UIElement child, int column)
        {
            child.SetValue(ColumnTagProperty, column);
        }

        private static int ColumnOf(UIElement child)
        {
            return (int)child.GetValue(ColumnTagProperty);
        }
    }
}
