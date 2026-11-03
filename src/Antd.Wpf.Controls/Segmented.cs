using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Segmented. Built on ListBox so single selection, keyboard navigation and
    /// SelectedIndex come for free; only the visual layer is antd specific.
    /// </summary>
    public class Segmented : ListBox
    {
        static Segmented()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Segmented), new FrameworkPropertyMetadata(typeof(Segmented)));
        }

        public Segmented()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            "Block", typeof(bool), typeof(Segmented), new FrameworkPropertyMetadata(false));

        /// <summary>Stretch to the container width, matching antd's block segmented.</summary>
        public bool Block
        {
            get { return (bool)GetValue(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }

        private static readonly DependencyPropertyKey TrackCornerRadiusKey = DependencyProperty.RegisterReadOnly(
            "TrackCornerRadius", typeof(CornerRadius), typeof(Segmented), new FrameworkPropertyMetadata(new CornerRadius(6)));

        public static readonly DependencyProperty TrackCornerRadiusProperty = TrackCornerRadiusKey.DependencyProperty;

        public CornerRadius TrackCornerRadius
        {
            get { return (CornerRadius)GetValue(TrackCornerRadiusProperty); }
            private set { SetValue(TrackCornerRadiusKey, value); }
        }

        private static readonly DependencyPropertyKey ItemCornerRadiusKey = DependencyProperty.RegisterReadOnly(
            "ItemCornerRadius", typeof(CornerRadius), typeof(Segmented), new FrameworkPropertyMetadata(new CornerRadius(4)));

        public static readonly DependencyProperty ItemCornerRadiusProperty = ItemCornerRadiusKey.DependencyProperty;

        public CornerRadius ItemCornerRadius
        {
            get { return (CornerRadius)GetValue(ItemCornerRadiusProperty); }
            private set { SetValue(ItemCornerRadiusKey, value); }
        }

        private static readonly DependencyPropertyKey ItemFontSizeKey = DependencyProperty.RegisterReadOnly(
            "ItemFontSize", typeof(double), typeof(Segmented), new FrameworkPropertyMetadata(14.0));

        public static readonly DependencyProperty ItemFontSizeProperty = ItemFontSizeKey.DependencyProperty;

        public double ItemFontSize
        {
            get { return (double)GetValue(ItemFontSizeProperty); }
            private set { SetValue(ItemFontSizeKey, value); }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            SetValue(TrackCornerRadiusKey, new CornerRadius(snapshot.Tokens.BorderRadius));
            SetValue(ItemCornerRadiusKey, new CornerRadius(snapshot.Tokens.BorderRadiusSM));
            SetValue(ItemFontSizeKey, snapshot.Tokens.FontSize);
            SetCurrentValue(HorizontalAlignmentProperty,
                Block ? HorizontalAlignment.Stretch : HorizontalAlignment.Left);
        }
    }
}
