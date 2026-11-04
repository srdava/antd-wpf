using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Anchor: a list of links that follows a ScrollViewer and highlights the section you
    /// are currently reading. Clicking a link scrolls to its target.
    /// </summary>
    public class Anchor : ItemsControl
    {
        private ScrollViewer _hooked;

        static Anchor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Anchor), new FrameworkPropertyMetadata(typeof(Anchor)));
        }

        public Anchor()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty ScrollTargetProperty = DependencyProperty.Register(
            "ScrollTarget", typeof(ScrollViewer), typeof(Anchor),
            new FrameworkPropertyMetadata(null, OnScrollTargetChanged));

        /// <summary>The scroller whose position drives the highlight.</summary>
        public ScrollViewer ScrollTarget
        {
            get { return (ScrollViewer)GetValue(ScrollTargetProperty); }
            set { SetValue(ScrollTargetProperty, value); }
        }

        public static readonly DependencyProperty ActiveIndexProperty = DependencyProperty.Register(
            "ActiveIndex", typeof(int), typeof(Anchor), new FrameworkPropertyMetadata(-1));

        public int ActiveIndex
        {
            get { return (int)GetValue(ActiveIndexProperty); }
            private set { SetValue(ActiveIndexProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AnchorLink();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AnchorLink;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Hook();
            Refresh();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unhook();
        }

        private static void OnScrollTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var anchor = (Anchor)d;
            anchor.Unhook();
            anchor.Hook();
            anchor.Refresh();
        }

        private void Hook()
        {
            var target = ScrollTarget;
            if (target == null || _hooked != null || !IsLoaded) return;

            target.ScrollChanged += OnScrollChanged;
            _hooked = target;

            for (int i = 0; i < Items.Count; i++)
            {
                var link = ItemContainerGenerator.ContainerFromIndex(i) as AnchorLink;
                if (link != null) link.Click += OnLinkClick;
            }
        }

        private void Unhook()
        {
            if (_hooked != null) _hooked.ScrollChanged -= OnScrollChanged;
            _hooked = null;

            for (int i = 0; i < Items.Count; i++)
            {
                var link = ItemContainerGenerator.ContainerFromIndex(i) as AnchorLink;
                if (link != null) link.Click -= OnLinkClick;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Refresh();
        }

        private void OnLinkClick(object sender, RoutedEventArgs e)
        {
            var link = sender as AnchorLink;
            if (link == null || link.Target == null || _hooked == null) return;

            // Bring the section's top edge to the top of the viewport.
            try
            {
                Point offset = link.Target.TransformToAncestor(_hooked.Content as UIElement)
                    .Transform(new Point(0, 0));
                _hooked.ScrollToVerticalOffset(offset.Y);
            }
            catch (InvalidOperationException)
            {
                link.Target.BringIntoView();
            }
        }

        /// <summary>Picks the last section whose top edge has passed the viewport top.</summary>
        public void Refresh()
        {
            if (_hooked == null || _hooked.Content as UIElement == null) return;

            double top = _hooked.VerticalOffset;
            int active = -1;

            for (int i = 0; i < Items.Count; i++)
            {
                var link = ItemContainerGenerator.ContainerFromIndex(i) as AnchorLink;
                if (link == null || link.Target == null) continue;

                try
                {
                    Point point = link.Target.TransformToAncestor(_hooked.Content as UIElement)
                        .Transform(new Point(0, 0));
                    if (point.Y <= top + 4) active = i;
                }
                catch (InvalidOperationException)
                {
                    // Target is not inside the scroller; ignore it.
                }
            }

            SetCurrentValue(ActiveIndexProperty, active);

            for (int i = 0; i < Items.Count; i++)
            {
                var link = ItemContainerGenerator.ContainerFromIndex(i) as AnchorLink;
                if (link != null) link.SetIsActive(i == active);
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

    /// <summary>One anchor row: a text link with a left rail that turns primary when active.</summary>
    public class AnchorLink : ContentControl
    {
        static AnchorLink()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorLink),
                new FrameworkPropertyMetadata(typeof(AnchorLink)));
        }

        public AnchorLink()
        {
            Padding = new Thickness(16, 4, 16, 4);
            MinHeight = 0;
            HorizontalContentAlignment = HorizontalAlignment.Left;
            Cursor = System.Windows.Input.Cursors.Hand;
            AddHandler(PreviewMouseUpEvent, new System.Windows.Input.MouseButtonEventHandler(OnClick), true);
        }

        /// <summary>Raised when the link is clicked; the anchor uses it to scroll.</summary>
        public event RoutedEventHandler Click;

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left) return;

            RoutedEventHandler handler = Click;
            if (handler != null) handler(this, new RoutedEventArgs());
            e.Handled = true;
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(FrameworkElement), typeof(AnchorLink), new FrameworkPropertyMetadata(null));

        /// <summary>Element that the link scrolls to and whose position drives the highlight.</summary>
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(AnchorLink), new FrameworkPropertyMetadata(false));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActiveProperty, value); }
        }

        internal void SetIsActive(bool value)
        {
            SetValue(IsActiveProperty, value);
        }
    }
}
