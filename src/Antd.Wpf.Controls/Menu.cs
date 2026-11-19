using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>Layout of a <see cref="Menu"/>.</summary>
    public enum MenuMode
    {
        Vertical = 0,
        Horizontal = 1
    }

    /// <summary>
    /// antd Menu. Vertical menus expand sub-menus in place; a horizontal menu keeps its top level
    /// on one row and opens sub-menus in a popup. Selection always lands on a leaf.
    /// </summary>
    public class Menu : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Menu, "Menu");
        }
        static Menu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(typeof(Menu)));
        }

        public Menu()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof(MenuMode), typeof(Menu),
            new FrameworkPropertyMetadata(MenuMode.Vertical, OnModeChanged));

        public MenuMode Mode
        {
            get { return (MenuMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        private static readonly DependencyPropertyKey SelectedItemKey = DependencyProperty.RegisterReadOnly(
            "SelectedItem", typeof(MenuNode), typeof(Menu), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = SelectedItemKey.DependencyProperty;

        public MenuNode SelectedItem
        {
            get { return (MenuNode)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemKey, value); }
        }

        /// <summary>Raised with the newly selected node.</summary>
        public event RoutedPropertyChangedEventHandler<MenuNode> ItemSelected;

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuNode();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuNode;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ApplyDepth();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ApplyDepth();
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Menu)d).ApplyDepth();
        }

        private void ApplyDepth()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var node = ItemContainerGenerator.ContainerFromIndex(i) as MenuNode;
                if (node != null) node.SetDepth(0, Mode == MenuMode.Horizontal);
            }
        }

        internal void Select(MenuNode node)
        {
            MenuNode previous = SelectedItem;
            if (ReferenceEquals(previous, node)) return;

            if (previous != null) previous.SetIsActive(false);
            node.SetIsActive(true);
            SetValue(SelectedItemKey, node);

            RoutedPropertyChangedEventHandler<MenuNode> handler = ItemSelected;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<MenuNode>(previous, node));
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

    /// <summary>A menu entry; may own children, in which case clicking toggles instead of selecting.</summary>
    public class MenuNode : HeaderedItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ListItem, "MenuNode");
        }
        static MenuNode()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuNode), new FrameworkPropertyMetadata(typeof(MenuNode)));
        }

        public MenuNode()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            AddHandler(PreviewMouseUpEvent, new MouseButtonEventHandler(OnClick), true);
        }

        /// <summary>Convenience for building a tree in code.</summary>
        public void AddChild(MenuNode child)
        {
            Items.Add(child);
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(MenuNode),
            new FrameworkPropertyMetadata(false, OnExpandedChanged));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private static readonly DependencyPropertyKey HasChildrenKey = DependencyProperty.RegisterReadOnly(
            "HasChildren", typeof(bool), typeof(MenuNode), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasChildrenProperty = HasChildrenKey.DependencyProperty;

        public bool HasChildren
        {
            get { return (bool)GetValue(HasChildrenProperty); }
            private set { SetValue(HasChildrenKey, value); }
        }

        private static readonly DependencyPropertyKey IsActiveKey = DependencyProperty.RegisterReadOnly(
            "IsActive", typeof(bool), typeof(MenuNode), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsActiveProperty = IsActiveKey.DependencyProperty;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActiveKey, value); }
        }

        private static readonly DependencyPropertyKey DepthKey = DependencyProperty.RegisterReadOnly(
            "Depth", typeof(int), typeof(MenuNode), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty DepthProperty = DepthKey.DependencyProperty;

        public int Depth
        {
            get { return (int)GetValue(DepthProperty); }
            private set { SetValue(DepthKey, value); }
        }

        private static readonly DependencyPropertyKey PopupModeKey = DependencyProperty.RegisterReadOnly(
            "PopupMode", typeof(bool), typeof(MenuNode), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty PopupModeProperty = PopupModeKey.DependencyProperty;

        /// <summary>True for a top-level node of a horizontal menu: children open in a popup.</summary>
        public bool PopupMode
        {
            get { return (bool)GetValue(PopupModeProperty); }
            private set { SetValue(PopupModeKey, value); }
        }

        internal void SetIsActive(bool value)
        {
            SetValue(IsActiveKey, value);
        }

        internal void SetDepth(int depth, bool popupMode)
        {
            SetValue(DepthKey, depth);
            SetValue(PopupModeKey, popupMode && depth == 0);
            SetValue(PaddingProperty, new Thickness(16 + depth * 12, 0, 16, 0));

            for (int i = 0; i < Items.Count; i++)
            {
                var child = ItemContainerGenerator.ContainerFromIndex(i) as MenuNode;
                if (child != null) child.SetDepth(depth + 1, false);
                else if (Items[i] is MenuNode) ((MenuNode)Items[i]).SetDepth(depth + 1, false);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuNode();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuNode;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            SetValue(HasChildrenKey, Items.Count > 0);
            SetDepth(Depth, PopupMode);
        }

        private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var node = (MenuNode)d;
            if (node._popup != null) node._popup.IsOpen = (bool)e.NewValue;
        }

        private Popup _popup;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = GetTemplateChild("PART_Popup") as Popup;
            if (_popup != null) _popup.IsOpen = IsExpanded;
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (HasChildren)
            {
                SetCurrentValue(IsExpandedProperty, !IsExpanded);
                e.Handled = true;
                return;
            }

            DependencyObject node = VisualTreeHelper.GetParent(this);
            while (node != null)
            {
                var menu = node as Menu;
                if (menu != null)
                {
                    menu.Select(this);
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
            SetCurrentValue(PaddingProperty, new Thickness(16, 0, 16, 0));
        }
    }
}
