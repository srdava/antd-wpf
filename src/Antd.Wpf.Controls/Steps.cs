using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>Where a step sits in the flow.</summary>
    public enum StepsStatus
    {
        Wait = 0,
        Process = 1,
        Finish = 2,
        Error = 3
    }

    /// <summary>
    /// antd Steps. Horizontal puts the label under the icon and draws a connector to the next
    /// step; vertical stacks the same pieces down the left edge.
    /// </summary>
    public class Steps : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Steps");
        }
        static Steps()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Steps), new FrameworkPropertyMetadata(typeof(Steps)));
        }

        public Steps()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(
            "Current", typeof(int), typeof(Steps),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Index of the step in progress; earlier ones become finished.</summary>
        public int Current
        {
            get { return (int)GetValue(CurrentProperty); }
            set { SetValue(CurrentProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction", typeof(Orientation), typeof(Steps),
            new FrameworkPropertyMetadata(Orientation.Horizontal, OnDirectionChanged));

        public Orientation Direction
        {
            get { return (Orientation)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StepsItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StepsItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            Refresh();
        }

        private void OnGeneratorStatusChanged(object sender, System.EventArgs e)
        {
            if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                Refresh();
            }
        }

        private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Steps)d).Refresh();
        }

        private void Refresh()
        {
            int count = Items.Count;
            for (int i = 0; i < count; i++)
            {
                var item = ItemContainerGenerator.ContainerFromIndex(i) as StepsItem;
                if (item == null) continue;

                item.SetPosition(i, count, Direction);

                // An explicit Status wins; otherwise the position relative to Current decides.
                if (item.ReadLocalValue(StepsItem.StatusProperty) == DependencyProperty.UnsetValue)
                {
                    item.SetStatus(i < Current ? StepsStatus.Finish
                        : (i == Current ? StepsStatus.Process : StepsStatus.Wait));
                }
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

    /// <summary>A single step: numbered or checked icon, title and description.</summary>
    public class StepsItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ListItem, "StepsItem");
        }
        static StepsItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StepsItem), new FrameworkPropertyMetadata(typeof(StepsItem)));
        }

        public StepsItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(StepsItem), new FrameworkPropertyMetadata(null));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(object), typeof(StepsItem), new FrameworkPropertyMetadata(null));

        public object Description
        {
            get { return GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(StepsStatus), typeof(StepsItem),
            new FrameworkPropertyMetadata(StepsStatus.Wait, OnStatusChanged));

        public StepsStatus Status
        {
            get { return (StepsStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        private static readonly DependencyPropertyKey IndexKey = DependencyProperty.RegisterReadOnly(
            "Index", typeof(int), typeof(StepsItem), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty IndexProperty = IndexKey.DependencyProperty;

        /// <summary>Zero-based position, shown as the step number.</summary>
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            private set { SetValue(IndexKey, value); }
        }

        private static readonly DependencyPropertyKey NumberKey = DependencyProperty.RegisterReadOnly(
            "Number", typeof(string), typeof(StepsItem), new FrameworkPropertyMetadata("1"));

        public static readonly DependencyProperty NumberProperty = NumberKey.DependencyProperty;

        /// <summary>The number printed in the icon (index + 1).</summary>
        public string Number
        {
            get { return (string)GetValue(NumberProperty); }
            private set { SetValue(NumberKey, value); }
        }

        private static readonly DependencyPropertyKey IsLastKey = DependencyProperty.RegisterReadOnly(
            "IsLast", typeof(bool), typeof(StepsItem), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsLastProperty = IsLastKey.DependencyProperty;

        public bool IsLast
        {
            get { return (bool)GetValue(IsLastProperty); }
            private set { SetValue(IsLastKey, value); }
        }

        private static readonly DependencyPropertyKey IsVerticalKey = DependencyProperty.RegisterReadOnly(
            "IsVertical", typeof(bool), typeof(StepsItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsVerticalProperty = IsVerticalKey.DependencyProperty;

        public bool IsVertical
        {
            get { return (bool)GetValue(IsVerticalProperty); }
            private set { SetValue(IsVerticalKey, value); }
        }

        internal void SetPosition(int index, int count, Orientation direction)
        {
            SetValue(IndexKey, index);
            SetValue(NumberKey, (index + 1).ToString(System.Globalization.CultureInfo.CurrentCulture));
            SetValue(IsLastKey, index == count - 1);
            SetValue(IsVerticalKey, direction == Orientation.Vertical);
        }

        internal void SetStatus(StepsStatus status)
        {
            SetCurrentValue(StatusProperty, status);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StepsItem)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }
}
