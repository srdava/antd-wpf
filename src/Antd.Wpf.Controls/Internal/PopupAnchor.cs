using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// Keeps a Popup glued under its owner when that owner sits inside a scroll container.
    ///
    /// A WPF Popup is its own HWND, so it is positioned once when it opens and then never
    /// again: scrolling the list it was opened from leaves the panel hovering over unrelated
    /// content while the field slides away. antd's dropdown is anchored to the trigger and is
    /// re-aligned on scroll, so we reproduce that by listening to every ancestor ScrollViewer
    /// and asking WPF to re-run placement.
    /// </summary>
    internal sealed class PopupAnchor
    {
        private static readonly DependencyProperty AnchorProperty = DependencyProperty.RegisterAttached(
            "Anchor", typeof(PopupAnchor), typeof(PopupAnchor), new PropertyMetadata(null));

        private readonly Popup _popup;
        private readonly FrameworkElement _target;
        private readonly List<ScrollViewer> _scrollers = new List<ScrollViewer>();
        private bool _nudging;

        private PopupAnchor(Popup popup, FrameworkElement target)
        {
            _popup = popup;
            _target = target;
        }

        /// <summary>
        /// Anchors <paramref name="popup"/> to the bottom edge of <paramref name="target"/>.
        /// Safe to call again after a template is re-applied; the previous wiring is dropped.
        /// </summary>
        public static void Attach(Popup popup, FrameworkElement target)
        {
            if (popup == null) return;

            var previous = popup.GetValue(AnchorProperty) as PopupAnchor;
            if (previous != null) previous.Detach();
            popup.ClearValue(AnchorProperty);

            if (target == null) return;

            // Placement stays whatever the template (or the owner) chose; only the target is
            // pinned, so a caret-relative panel like Mentions keeps its offsets.
            if (popup.PlacementTarget == null) popup.PlacementTarget = target;

            var anchor = new PopupAnchor(popup, target);
            popup.SetValue(AnchorProperty, anchor);

            anchor._target.Loaded += anchor.OnTargetLoaded;
            anchor._target.Unloaded += anchor.OnTargetUnloaded;
            popup.Opened += anchor.OnPopupOpened;
            if (target.IsLoaded) anchor.HookScrollViewers();
        }

        private void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            HookScrollViewers();
            Reposition();
        }

        /// <summary>
        /// Drops the ScrollViewer handlers. A ScrollViewer usually outlives the controls inside
        /// it, so leaving them subscribed would keep every closed picker alive.
        /// </summary>
        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            UnhookScrollViewers();
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            // The panel grows before any column is measured; re-run placement once it has a size.
            HookScrollViewers();
            Reposition();
        }

        /// <summary>Subscribes to every ScrollViewer between the target and the window.</summary>
        private void HookScrollViewers()
        {
            DependencyObject node = _target;
            while (node != null)
            {
                var scroller = node as ScrollViewer;
                if (scroller != null && !_scrollers.Contains(scroller))
                {
                    scroller.ScrollChanged += OnScrollChanged;
                    _scrollers.Add(scroller);
                }
                node = VisualTreeHelper.GetParent(node);
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Math.Abs(e.VerticalChange) < 0.001 && Math.Abs(e.HorizontalChange) < 0.001) return;
            Reposition();
        }

        /// <summary>
        /// Forces WPF to recompute placement. There is no public "reposition" call, and the
        /// popup only listens to offsets, so a sub-pixel nudge is what actually triggers it.
        /// </summary>
        private void Reposition()
        {
            if (!_popup.IsOpen || _nudging) return;

            _nudging = true;
            try
            {
                double offset = _popup.HorizontalOffset;
                _popup.HorizontalOffset = offset + 0.01;
                _popup.HorizontalOffset = offset;
            }
            finally
            {
                _nudging = false;
            }
        }

        private void Detach()
        {
            _target.Loaded -= OnTargetLoaded;
            _target.Unloaded -= OnTargetUnloaded;
            _popup.Opened -= OnPopupOpened;
            UnhookScrollViewers();
        }

        private void UnhookScrollViewers()
        {
            for (int i = 0; i < _scrollers.Count; i++)
            {
                _scrollers[i].ScrollChanged -= OnScrollChanged;
            }
            _scrollers.Clear();
        }
    }
}
