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
    /// antd Collapse. Rows are separated by hairlines and the first/last one are rounded by the
    /// container, so each item only draws its own bottom edge and the last one drops it.
    /// </summary>
    public class Collapse : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Collapse");
        }
        static Collapse()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Collapse), new FrameworkPropertyMetadata(typeof(Collapse)));
        }

        public Collapse()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();

            // Items are added before the generator has built their containers, so the first
            // pass sees nothing; refresh once the containers exist.
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Collapse),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty AccordionProperty = DependencyProperty.Register(
            "Accordion", typeof(bool), typeof(Collapse), new FrameworkPropertyMetadata(false));

        /// <summary>Only one panel stays open at a time.</summary>
        public bool Accordion
        {
            get { return (bool)GetValue(AccordionProperty); }
            set { SetValue(AccordionProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(Collapse), new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CollapseItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CollapseItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            RefreshLastFlags();
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            Dispatcher.BeginInvoke(new System.Action(RefreshLastFlags));
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
                var item = ItemContainerGenerator.ContainerFromIndex(i) as CollapseItem;
                if (item == null) continue;
                item.SetIsFirst(i == 0);
                item.SetIsLast(i == count - 1);
            }
        }

        /// <summary>Called by a child when it opens; enforces <see cref="Accordion"/>.</summary>
        internal void OnItemExpanded(CollapseItem opened)
        {
            if (!Accordion) return;

            for (int i = 0; i < Items.Count; i++)
            {
                var item = ItemContainerGenerator.ContainerFromIndex(i) as CollapseItem;
                if (item != null && !ReferenceEquals(item, opened)) item.IsExpanded = false;
            }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Collapse)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(BorderThicknessProperty, new Thickness(Bordered ? t.LineWidth : 0));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush("colorBorder"));
            SetCurrentValue(BackgroundProperty, snapshot.Brush("colorBgContainer"));
            SetCurrentValue(FontSizeProperty, t.FontSize);
        }
    }

    /// <summary>A single disclosure row: a 46-tall header over a 16-padded body.</summary>
    public class CollapseItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "CollapseItem");
        }
        static CollapseItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapseItem), new FrameworkPropertyMetadata(typeof(CollapseItem)));
        }

        public CollapseItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(CollapseItem), new FrameworkPropertyMetadata(null));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(CollapseItem),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnExpandedChanged));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private FrameworkElement _header;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_header != null) _header.MouseLeftButtonUp -= OnHeaderClick;
            _header = GetTemplateChild("PART_Header") as FrameworkElement;
            if (_header != null)
            {
                _header.MouseLeftButtonUp += OnHeaderClick;
                _header.Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        private void OnHeaderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
            e.Handled = true;
        }

        private static readonly DependencyPropertyKey IsLastKey = DependencyProperty.RegisterReadOnly(
            "IsLast", typeof(bool), typeof(CollapseItem), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsLastProperty = IsLastKey.DependencyProperty;

        /// <summary>Set by the owning <see cref="Collapse"/>; hides the trailing hairline.</summary>
        public bool IsLast
        {
            get { return (bool)GetValue(IsLastProperty); }
            private set { SetValue(IsLastKey, value); }
        }

        internal void SetIsLast(bool value)
        {
            SetValue(IsLastKey, value);
        }

        private static readonly DependencyPropertyKey IsFirstKey = DependencyProperty.RegisterReadOnly(
            "IsFirst", typeof(bool), typeof(CollapseItem), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsFirstProperty = IsFirstKey.DependencyProperty;

        /// <summary>The first row rounds its header to match the container.</summary>
        public bool IsFirst
        {
            get { return (bool)GetValue(IsFirstProperty); }
            private set { SetValue(IsFirstKey, value); }
        }

        internal void SetIsFirst(bool value)
        {
            SetValue(IsFirstKey, value);
        }

        private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (CollapseItem)d;
            if (!(bool)e.NewValue) return;

            DependencyObject node = VisualTreeHelper.GetParent(item);
            while (node != null)
            {
                var collapse = node as Collapse;
                if (collapse != null)
                {
                    collapse.OnItemExpanded(item);
                    break;
                }
                node = VisualTreeHelper.GetParent(node);
            }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorText"));
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>
    /// antd List. Rows are separated by hairlines; the content of each row is whatever the
    /// caller templates, so no opinionated List.Item.Meta is imposed.
    /// </summary>
    public class List : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.List, "List");
        }
        static List()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(List), new FrameworkPropertyMetadata(typeof(List)));
        }

        public List()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(List),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(List), new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ListItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            RefreshLastFlags();
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            Dispatcher.BeginInvoke(new System.Action(RefreshLastFlags));
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
                var item = ItemContainerGenerator.ContainerFromIndex(i) as ListItem;
                if (item != null) item.SetIsLast(i == count - 1);
            }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((List)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(BorderThicknessProperty, new Thickness(Bordered ? t.LineWidth : 0));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush("colorBorderSecondary"));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(t.BorderRadiusLG));
            SetCurrentValue(BackgroundProperty, Bordered ? snapshot.Brush("colorBgContainer") : Brushes.Transparent);
            SetCurrentValue(FontSizeProperty, t.FontSize);
        }
    }

    /// <summary>One list row.</summary>
    public class ListItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ListItem, "ListItem");
        }
        static ListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListItem), new FrameworkPropertyMetadata(typeof(ListItem)));
        }

        public ListItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        private static readonly DependencyPropertyKey IsLastKey = DependencyProperty.RegisterReadOnly(
            "IsLast", typeof(bool), typeof(ListItem), new FrameworkPropertyMetadata(true));

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
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorText"));
            SetCurrentValue(PaddingProperty, new Thickness(t.Padding, t.PaddingSM, t.Padding, t.PaddingSM));
        }
    }
}
