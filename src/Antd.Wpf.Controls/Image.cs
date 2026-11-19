using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Image: a picture that opens a full-window preview when clicked. The preview is a
    /// Popup rather than an adorner so it escapes every clipping ancestor.
    /// </summary>
    public class Image : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Image, "Image");
        }
        private Popup _popup;
        private FrameworkElement _mask;

        static Image()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Image), new FrameworkPropertyMetadata(typeof(Image)));
        }

        public Image()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(Image), new FrameworkPropertyMetadata(null));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch", typeof(Stretch), typeof(Image), new FrameworkPropertyMetadata(Stretch.Uniform));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly DependencyProperty PreviewProperty = DependencyProperty.Register(
            "Preview", typeof(bool), typeof(Image), new FrameworkPropertyMetadata(true));

        /// <summary>Whether clicking opens the zoomed overlay.</summary>
        public bool Preview
        {
            get { return (bool)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        public static readonly DependencyProperty IsPreviewVisibleProperty = DependencyProperty.Register(
            "IsPreviewVisible", typeof(bool), typeof(Image),
            new FrameworkPropertyMetadata(false, OnPreviewChanged));

        public bool IsPreviewVisible
        {
            get { return (bool)GetValue(IsPreviewVisibleProperty); }
            set { SetValue(IsPreviewVisibleProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Image),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = AntdMotion.Configure(GetTemplateChild("PART_Popup") as Popup);
            _mask = GetTemplateChild("PART_Mask") as FrameworkElement;
            if (_popup != null) _popup.IsOpen = IsPreviewVisible;

            // The mask lives in its own popup window, so a click there never reaches this
            // control; wire it directly.
            if (_mask != null)
            {
                _mask.MouseLeftButtonUp -= OnMaskClick;
                _mask.MouseLeftButtonUp += OnMaskClick;
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!Preview) return;

            SetCurrentValue(IsPreviewVisibleProperty, true);
            e.Handled = true;
        }

        private static void OnPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = (Image)d;
            bool open = (bool)e.NewValue;

            if (image._popup != null) image._popup.IsOpen = open;
            if (open) image.SizeMask();
        }

        private void OnMaskClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetCurrentValue(IsPreviewVisibleProperty, false);
            e.Handled = true;
        }

        /// <summary>The mask has to cover the whole window; a Popup is not stretched by layout.</summary>
        private void SizeMask()
        {
            if (_mask == null) return;

            Window window = Window.GetWindow(this);
            if (window == null) return;

            _mask.Width = window.ActualWidth;
            _mask.Height = window.ActualHeight;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsPreviewVisible)
            {
                SetCurrentValue(IsPreviewVisibleProperty, false);
                e.Handled = true;
            }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush("colorBorderSecondary"));
        }
    }
}
