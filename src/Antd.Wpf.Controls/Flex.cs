using System;
using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum FlexDirection
    {
        Row = 0,
        RowReverse = 1,
        Column = 2,
        ColumnReverse = 3
    }

    public enum FlexJustify
    {
        Start = 0,
        Center = 1,
        End = 2,
        SpaceBetween = 3,
        SpaceAround = 4,
        SpaceEvenly = 5
    }

    public enum FlexAlign
    {
        Start = 0,
        Center = 1,
        End = 2,
        Stretch = 3
    }

    /// <summary>
    /// antd Flex. No `flex-grow` per child yet: WPF panels cannot negotiate proportions the
    /// way CSS does, so growth is expressed with an explicit <see cref="FillChild"/> or by
    /// wrapping in a Grid. Wrap, gap, justify and align cover the antd usage we need.
    /// </summary>
    public class Flex : Panel
    {
        public Flex()
        {
            ThemeLink.Attach(this, OnThemeChanged);
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction", typeof(FlexDirection), typeof(Flex),
            new FrameworkPropertyMetadata(FlexDirection.Row, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public FlexDirection Direction
        {
            get { return (FlexDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty JustifyProperty = DependencyProperty.Register(
            "Justify", typeof(FlexJustify), typeof(Flex),
            new FrameworkPropertyMetadata(FlexJustify.Start, FrameworkPropertyMetadataOptions.AffectsArrange));

        public FlexJustify Justify
        {
            get { return (FlexJustify)GetValue(JustifyProperty); }
            set { SetValue(JustifyProperty, value); }
        }

        public static readonly DependencyProperty AlignProperty = DependencyProperty.Register(
            "Align", typeof(FlexAlign), typeof(Flex),
            new FrameworkPropertyMetadata(FlexAlign.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange));

        public FlexAlign Align
        {
            get { return (FlexAlign)GetValue(AlignProperty); }
            set { SetValue(AlignProperty, value); }
        }

        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(
            "Wrap", typeof(bool), typeof(Flex),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool Wrap
        {
            get { return (bool)GetValue(WrapProperty); }
            set { SetValue(WrapProperty, value); }
        }

        public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
            "Gap", typeof(double), typeof(Flex),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Gap in DIPs. NaN means "use the antd size token" (16).</summary>
        public double Gap
        {
            get { return (double)GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }

        private double _resolvedGap = 16;

        private double EffectiveGap
        {
            get { return double.IsNaN(Gap) ? _resolvedGap : Gap; }
        }

        private void OnThemeChanged()
        {
            _resolvedGap = ThemeManager.Current.Snapshot.Tokens.Size;
            InvalidateMeasure();
        }

        private bool IsRow
        {
            get { return Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double gap = EffectiveGap;
            double main = 0;
            double cross = 0;
            int visible = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null || child.Visibility == Visibility.Collapsed) continue;
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Size desired = child.DesiredSize;
                double childMain = IsRow ? desired.Width : desired.Height;
                double childCross = IsRow ? desired.Height : desired.Width;

                if (visible > 0) main += gap;
                main += childMain;
                if (childCross > cross) cross = childCross;
                visible++;
            }

            return IsRow ? new Size(main, cross) : new Size(cross, main);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double gap = EffectiveGap;
            var children = new System.Collections.Generic.List<UIElement>();
            foreach (UIElement child in InternalChildren)
            {
                if (child != null && child.Visibility != Visibility.Collapsed) children.Add(child);
            }
            if (children.Count == 0) return finalSize;

            double totalGap = gap * (children.Count - 1);
            double used = 0;
            foreach (UIElement child in children)
            {
                used += IsRow ? child.DesiredSize.Width : child.DesiredSize.Height;
            }

            double free = (IsRow ? finalSize.Width : finalSize.Height) - used - totalGap;
            if (free < 0) free = 0;

            double offset = 0;
            double extraBetween = 0;

            switch (Justify)
            {
                case FlexJustify.Center: offset = free / 2; break;
                case FlexJustify.End: offset = free; break;
                case FlexJustify.SpaceBetween:
                    if (children.Count > 1) extraBetween = free / (children.Count - 1);
                    break;
                case FlexJustify.SpaceAround:
                    extraBetween = free / children.Count;
                    offset = extraBetween / 2;
                    break;
                case FlexJustify.SpaceEvenly:
                    extraBetween = free / (children.Count + 1);
                    offset = extraBetween;
                    break;
            }

            bool reverse = Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;
            if (reverse) children.Reverse();

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];
                Size desired = child.DesiredSize;
                double mainSize = IsRow ? desired.Width : desired.Height;

                double crossPos = 0;
                double crossSize = IsRow ? finalSize.Height : finalSize.Width;
                double childCross = IsRow ? desired.Height : desired.Width;

                switch (Align)
                {
                    case FlexAlign.Center: crossPos = (crossSize - childCross) / 2; break;
                    case FlexAlign.End: crossPos = crossSize - childCross; break;
                    case FlexAlign.Stretch: childCross = crossSize; break;
                }

                Rect rect = IsRow
                    ? new Rect(offset, crossPos, mainSize, childCross)
                    : new Rect(crossPos, offset, childCross, mainSize);

                child.Arrange(rect);
                offset += mainSize + gap + extraBetween;
            }

            return finalSize;
        }
    }
}
