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
    /// antd Descriptions: a label/value grid. Each item spans one or more columns, and a value
    /// that would not fit on the current row starts a new one, exactly like antd.
    /// </summary>
    public class Descriptions : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Descriptions");
        }
        static Descriptions()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Descriptions), new FrameworkPropertyMetadata(typeof(Descriptions)));
        }

        public Descriptions()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
            "Column", typeof(int), typeof(Descriptions),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(Descriptions), new FrameworkPropertyMetadata(false));

        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DescriptionsItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is DescriptionsItem;
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(BorderBrushProperty, snapshot.Brush("colorBorderSecondary"));
        }
    }

    /// <summary>One label/value pair.</summary>
    public class DescriptionsItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "DescriptionsItem");
        }
        static DescriptionsItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionsItem), new FrameworkPropertyMetadata(typeof(DescriptionsItem)));
        }

        public DescriptionsItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(object), typeof(DescriptionsItem),
            new FrameworkPropertyMetadata(null, (d, e) => ((DescriptionsItem)d).SetValue(HasLabelKey, e.NewValue != null)));

        public object Label
        {
            get { return GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        private static readonly DependencyPropertyKey HasLabelKey = DependencyProperty.RegisterReadOnly(
            "HasLabel", typeof(bool), typeof(DescriptionsItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasLabelProperty = HasLabelKey.DependencyProperty;

        /// <summary>Drives the colon so an unlabelled pair does not print a stray ":"</summary>
        public bool HasLabel
        {
            get { return (bool)GetValue(HasLabelProperty); }
            private set { SetValue(HasLabelKey, value); }
        }

        public static readonly DependencyProperty SpanProperty = DependencyProperty.Register(
            "Span", typeof(int), typeof(DescriptionsItem),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>How many columns this pair occupies.</summary>
        public int Span
        {
            get { return (int)GetValue(SpanProperty); }
            set { SetValue(SpanProperty, value); }
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>
    /// Lays items out in a fixed number of columns. A row only takes an item when the remaining
    /// columns can hold its span; otherwise the item starts a new row.
    /// </summary>
    public class DescriptionsPanel : Panel
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "DescriptionsPanel");
        }
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(int), typeof(DescriptionsPanel),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            int columns = Math.Max(1, Columns);
            double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            double cell = width / columns;

            double y = 0;
            int used = 0;
            double rowHeight = 0;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                int span = Math.Min(columns, Math.Max(1, SpanOf(child)));

                if (used > 0 && used + span > columns)
                {
                    y += rowHeight;
                    used = 0;
                    rowHeight = 0;
                }

                double w = cell * span;
                child.Measure(new Size(w, double.PositiveInfinity));
                rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
                used += span;

                if (used >= columns)
                {
                    y += rowHeight;
                    used = 0;
                    rowHeight = 0;
                }
            }

            return new Size(width, y + rowHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int columns = Math.Max(1, Columns);
            double cell = finalSize.Width / columns;

            double y = 0;
            int used = 0;
            double rowHeight = 0;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                int span = Math.Min(columns, Math.Max(1, SpanOf(child)));

                if (used > 0 && used + span > columns)
                {
                    y += rowHeight;
                    used = 0;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(cell * used, y, cell * span, child.DesiredSize.Height));
                rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
                used += span;

                if (used >= columns)
                {
                    y += rowHeight;
                    used = 0;
                    rowHeight = 0;
                }
            }

            return finalSize;
        }

        private static int SpanOf(UIElement child)
        {
            var item = child as DescriptionsItem;
            return item == null ? 1 : item.Span;
        }
    }
}
