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
    /// antd Breadcrumb. Items are separated by a slash that belongs to the row before it, so the
    /// last item can drop it without a second template.
    /// </summary>
    public class Breadcrumb : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.List, "Breadcrumb");
        }
        static Breadcrumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumb), new FrameworkPropertyMetadata(typeof(Breadcrumb)));
        }

        public Breadcrumb()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        public static readonly DependencyProperty SeparatorProperty = DependencyProperty.Register(
            "Separator", typeof(string), typeof(Breadcrumb), new FrameworkPropertyMetadata("/"));

        public string Separator
        {
            get { return (string)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is BreadcrumbItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            RefreshLastFlags();
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs e)
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
                var item = ItemContainerGenerator.ContainerFromIndex(i) as BreadcrumbItem;
                if (item != null) item.SetIsLast(i == count - 1);
            }
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>One crumb. The last one is the current page and loses its separator.</summary>
    public class BreadcrumbItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Hyperlink, "BreadcrumbItem");
        }
        static BreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem), new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
        }

        public BreadcrumbItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty NavigateUrlProperty = DependencyProperty.Register(
            "NavigateUrl", typeof(string), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(string.Empty));

        /// <summary>Kept for API parity; the control does not open browsers by itself.</summary>
        public string NavigateUrl
        {
            get { return (string)GetValue(NavigateUrlProperty); }
            set { SetValue(NavigateUrlProperty, value); }
        }

        private static readonly DependencyPropertyKey IsLastKey = DependencyProperty.RegisterReadOnly(
            "IsLast", typeof(bool), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(true));

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
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsLast ? "colorText" : "colorTextSecondary"));
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }
}
