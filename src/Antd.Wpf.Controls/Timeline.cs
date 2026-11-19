using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Timeline: a dot column with a rail, then the content. The rail is drawn by every item
    /// except the last, so the stack never trails a stray line.
    /// </summary>
    public class Timeline : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.List, "Timeline");
        }
        static Timeline()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));
        }

        public Timeline()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TimelineItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TimelineItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            RefreshLastFlags();
        }

        private void OnGeneratorStatusChanged(object sender, System.EventArgs e)
        {
            if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                RefreshLastFlags();
            }
        }

        private void RefreshLastFlags()
        {
            int count = Items.Count;
            for (int i = 0; i < count; i++)
            {
                var item = ItemContainerGenerator.ContainerFromIndex(i) as TimelineItem;
                if (item != null) item.SetIsLast(i == count - 1);
            }
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>One timeline entry: a dot, an optional rail and the content.</summary>
    public class TimelineItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ListItem, "TimelineItem");
        }
        static TimelineItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineItem), new FrameworkPropertyMetadata(typeof(TimelineItem)));
        }

        public TimelineItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty DotBrushProperty = DependencyProperty.Register(
            "DotBrush", typeof(Brush), typeof(TimelineItem), new FrameworkPropertyMetadata(Brushes.DodgerBlue));

        /// <summary>Overrides the dot colour, e.g. error red for a failed step.</summary>
        public Brush DotBrush
        {
            get { return (Brush)GetValue(DotBrushProperty); }
            set { SetValue(DotBrushProperty, value); }
        }

        public static readonly DependencyProperty DotSizeProperty = DependencyProperty.Register(
            "DotSize", typeof(double), typeof(TimelineItem), new FrameworkPropertyMetadata(10.0));

        public double DotSize
        {
            get { return (double)GetValue(DotSizeProperty); }
            set { SetValue(DotSizeProperty, value); }
        }

        private static readonly DependencyPropertyKey IsLastKey = DependencyProperty.RegisterReadOnly(
            "IsLast", typeof(bool), typeof(TimelineItem), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsLastProperty = IsLastKey.DependencyProperty;

        public bool IsLast
        {
            get { return (bool)GetValue(IsLastProperty); }
            private set { SetValue(IsLastKey, value); }
        }

        internal void SetIsLast(bool value)
        {
            SetValue(IsLastKey, value);
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(ForegroundProperty, ThemeManager.Current.Snapshot.Brush("colorText"));
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }
}
