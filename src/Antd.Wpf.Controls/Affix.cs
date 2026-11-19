using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Affix: keeps its content pinned to the top of the nearest ScrollViewer once the page
    /// scrolls past <see cref="OffsetTop"/>.
    ///
    /// WPF has no position:sticky, so the control tracks ScrollChanged and offsets itself with a
    /// RenderTransform. Layout space is preserved, which is what antd does too, and the transform
    /// keeps the element inside its parent's canvas so no reparenting is needed.
    /// </summary>
    public class Affix : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Affix");
        }
        private ScrollViewer _scroller;
        private double _origin = double.NaN;

        static Affix()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Affix), new FrameworkPropertyMetadata(typeof(Affix)));
        }

        public Affix()
        {
            RenderTransform = new TranslateTransform();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty OffsetTopProperty = DependencyProperty.Register(
            "OffsetTop", typeof(double), typeof(Affix), new FrameworkPropertyMetadata(0.0));

        /// <summary>Distance from the viewport top at which the content sticks.</summary>
        public double OffsetTop
        {
            get { return (double)GetValue(OffsetTopProperty); }
            set { SetValue(OffsetTopProperty, value); }
        }

        private static readonly DependencyPropertyKey IsPinnedKey = DependencyProperty.RegisterReadOnly(
            "IsPinned", typeof(bool), typeof(Affix), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPinnedProperty = IsPinnedKey.DependencyProperty;

        public bool IsPinned
        {
            get { return (bool)GetValue(IsPinnedProperty); }
            private set { SetValue(IsPinnedKey, value); }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _scroller = FindScroller(Parent);
            if (_scroller == null) return;

            _scroller.ScrollChanged += OnScrollChanged;
            // Pinned content has to paint above the siblings it scrolls over.
            Panel.SetZIndex(this, 100);
            UpdatePosition();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_scroller != null) _scroller.ScrollChanged -= OnScrollChanged;
            _scroller = null;
        }

        private static ScrollViewer FindScroller(DependencyObject node)
        {
            while (node != null)
            {
                var scroller = node as ScrollViewer;
                if (scroller != null) return scroller;
                node = VisualTreeHelper.GetParent(node);
            }
            return null;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var transform = RenderTransform as TranslateTransform;
            if (transform == null || _scroller == null) return;

            if (double.IsNaN(_origin))
            {
                _origin = TranslatePoint(new Point(0, 0), _scroller).Y + _scroller.VerticalOffset;
            }

            // The offset at which the element would naturally sit inside the viewport.
            double viewportTop = _origin - _scroller.VerticalOffset;
            double delta = OffsetTop - viewportTop;
            bool pinned = delta > 0;

            transform.Y = pinned ? delta : 0;
            IsPinned = pinned;
        }
    }
}
