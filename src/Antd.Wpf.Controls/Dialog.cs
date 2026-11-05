using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// Shared plumbing for the two full-window dialogs (Modal and Drawer). Both are declared inline
    /// in a page but painted through a Popup that covers the owner window, so the mask can dim the
    /// whole surface and the card can sit outside its parent's clipping.
    /// </summary>
    public abstract class DialogBase : ContentControl
    {
        private Popup _popup;
        private FrameworkElement _surface;
        private FrameworkElement _card;
        private UIElement _mask;
        private Window _owner;
        private IInputElement _focusBeforeOpen;

        protected DialogBase()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            Loaded += OnDialogLoaded;
            Unloaded += OnDialogUnloaded;
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(DialogBase),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty = DependencyProperty.Register(
            "Closable", typeof(bool), typeof(DialogBase),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty MaskClosableProperty = DependencyProperty.Register(
            "MaskClosable", typeof(bool), typeof(DialogBase),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        /// <summary>Clicking the dimmed backdrop closes the dialog, antd's <c>maskClosable</c>.</summary>
        public bool MaskClosable
        {
            get { return (bool)GetValue(MaskClosableProperty); }
            set { SetValue(MaskClosableProperty, value); }
        }

        public static readonly DependencyProperty MaskBrushProperty = DependencyProperty.Register(
            "MaskBrush", typeof(Brush), typeof(DialogBase), new FrameworkPropertyMetadata(null));

        public Brush MaskBrush
        {
            get { return (Brush)GetValue(MaskBrushProperty); }
            set { SetValue(MaskBrushProperty, value); }
        }

        /// <summary>Raised after the dialog finished closing, whatever closed it.</summary>
        public event EventHandler Closed;

        protected Popup DialogPopup
        {
            get { return _popup; }
        }

        protected FrameworkElement DialogCard
        {
            get { return _card; }
        }

        protected Window OwnerWindow
        {
            get { return _owner; }
        }

        /// <summary>Size of the owner's client area; the mask has to cover exactly that.</summary>
        protected Size OwnerClientSize
        {
            get
            {
                if (_owner == null) return new Size(0, 0);

                FrameworkElement content = _owner.Content as FrameworkElement;
                double w = content != null && content.ActualWidth > 0 ? content.ActualWidth : _owner.ActualWidth;
                double h = content != null && content.ActualHeight > 0 ? content.ActualHeight : _owner.ActualHeight;
                if (w <= 0) w = _owner.Width;
                if (h <= 0) h = _owner.Height;
                if (double.IsNaN(w) || w <= 0) w = 640;
                if (double.IsNaN(h) || h <= 0) h = 480;
                return new Size(w, h);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DetachParts();

            _popup = GetTemplateChild("PART_Popup") as Popup;
            _surface = GetTemplateChild("PART_Surface") as FrameworkElement;
            _card = GetTemplateChild("PART_Card") as FrameworkElement;
            _mask = GetTemplateChild("PART_Mask") as UIElement;

            if (_surface != null)
            {
                _surface.PreviewKeyDown += OnSurfaceKeyDown;
            }
            if (_mask != null)
            {
                _mask.MouseLeftButtonDown += OnMaskClick;
            }
            if (_popup != null)
            {
                _popup.Closed += OnPopupClosed;
            }

            AttachParts();

            if (IsOpen) ApplyOpenState(true);
        }

        /// <summary>Hook up the named parts that only a concrete dialog knows about.</summary>
        protected virtual void AttachParts()
        {
        }

        protected virtual void DetachParts()
        {
        }

        protected virtual void OnVisualStateChanged()
        {
        }

        public virtual void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(MaskBrushProperty, snapshot.Brush("colorBgMask"));
            SetCurrentValue(BackgroundProperty, snapshot.Brush("colorBgElevated"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorText"));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, new Thickness(0));
            SetCurrentValue(BorderThicknessProperty, new Thickness(0));
            OnVisualStateChanged();
        }

        /// <summary>Opens the dialog (same as setting <see cref="IsOpen"/>).</summary>
        public void Open()
        {
            SetCurrentValue(IsOpenProperty, true);
        }

        /// <summary>Closes the dialog. <paramref name="restoreFocus"/> puts the caret back where it was.</summary>
        public virtual void Close()
        {
            SetCurrentValue(IsOpenProperty, false);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DialogBase)d).ApplyOpenState((bool)e.NewValue);
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DialogBase)d).UpdateVisualState();
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            if (_owner == null)
            {
                _owner = Window.GetWindow(this);
                if (_owner != null)
                {
                    _owner.SizeChanged += OnOwnerSizeChanged;
                    _owner.Closed += OnOwnerClosed;
                }
            }

            if (IsOpen) ApplyOpenState(true);
        }

        private void OnDialogUnloaded(object sender, RoutedEventArgs e)
        {
            if (_owner != null)
            {
                _owner.SizeChanged -= OnOwnerSizeChanged;
                _owner.Closed -= OnOwnerClosed;
            }
            _owner = null;
        }

        private void OnOwnerClosed(object sender, EventArgs e)
        {
            if (_popup != null) _popup.IsOpen = false;
        }

        private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SyncSurface();
        }

        protected void ApplyOpenState(bool open)
        {
            if (_popup == null) return;

            if (open)
            {
                EnsureOwner();
                if (_owner == null) return;

                PositionPopup();
                SyncSurface();
                _popup.IsOpen = true;

                _focusBeforeOpen = Keyboard.FocusedElement;
                AnimateIn();
                FocusFirst();
            }
            else
            {
                bool wasOpen = _popup.IsOpen;
                _popup.IsOpen = false;
                RestoreFocus();
                if (wasOpen) RaiseClosed();
            }
        }

        private void EnsureOwner()
        {
            if (_owner != null) return;
            _owner = Window.GetWindow(this);
            if (_owner != null)
            {
                _owner.SizeChanged += OnOwnerSizeChanged;
                _owner.Closed += OnOwnerClosed;
            }
        }

        protected void PositionPopup()
        {
            if (_popup == null || _owner == null) return;

            // Anchoring to the window (not to this control) keeps the mask glued to the client area
            // even when the dialog is declared deep inside a ScrollViewer.
            _popup.PlacementTarget = _owner;
            _popup.Placement = PlacementMode.Relative;
            _popup.HorizontalOffset = 0;
            _popup.VerticalOffset = 0;
        }

        private void SyncSurface()
        {
            if (_surface == null || _owner == null) return;

            Size size = OwnerClientSize;
            _surface.Width = size.Width;
            _surface.Height = size.Height;
        }

        private void RaiseClosed()
        {
            EventHandler handler = Closed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        }

        private void OnMaskClick(object sender, MouseButtonEventArgs e)
        {
            if (!MaskClosable) return;
            e.Handled = true;
            Close();
        }

        private void OnSurfaceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || !Closable) return;
            e.Handled = true;
            Close();
        }

        protected void FocusFirst()
        {
            if (_card == null) return;

            _card.Focus();
            _card.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private void RestoreFocus()
        {
            var element = _focusBeforeOpen as UIElement;
            _focusBeforeOpen = null;
            if (element != null && element.IsVisible && element.Focusable)
            {
                Keyboard.Focus(element);
            }
        }

        /// <summary>Entry motion. Exit is immediate, matching the rest of the library.</summary>
        protected virtual void AnimateIn()
        {
            if (_card == null) return;

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var scale = new ScaleTransform(0.9, 0.9);
            _card.RenderTransformOrigin = new Point(0.5, 0.5);
            _card.RenderTransform = scale;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease };
            var growX = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease };
            var growY = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease };

            _card.BeginAnimation(UIElement.OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, growX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, growY);
        }

        protected void SetCardTransform(Transform transform)
        {
            if (_card != null) _card.RenderTransform = transform;
        }
    }

    /// <summary>
    /// antd Modal. Measured: 520 wide, 20/24 container padding, 16/600 title with an 8px gap to the
    /// body, a 12px gap before the right-aligned footer, and an 8px radius.
    /// </summary>
    public class Modal : DialogBase
    {
        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal)));
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Modal),
            new FrameworkPropertyMetadata(null, OnChromeChanged));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof(object), typeof(Modal),
            new FrameworkPropertyMetadata(null, OnChromeChanged));

        /// <summary>Replaces the default OK/Cancel row. Null keeps the default buttons.</summary>
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty ShowFooterProperty = DependencyProperty.Register(
            "ShowFooter", typeof(bool), typeof(Modal),
            new FrameworkPropertyMetadata(true, OnChromeChanged));

        public bool ShowFooter
        {
            get { return (bool)GetValue(ShowFooterProperty); }
            set { SetValue(ShowFooterProperty, value); }
        }

        public static readonly DependencyProperty DialogWidthProperty = DependencyProperty.Register(
            "DialogWidth", typeof(double), typeof(Modal),
            new FrameworkPropertyMetadata(520d, OnCardLayoutChanged));

        /// <summary>Card width. antd's default 520.</summary>
        public double DialogWidth
        {
            get { return (double)GetValue(DialogWidthProperty); }
            set { SetValue(DialogWidthProperty, value); }
        }

        public static readonly DependencyProperty CenteredProperty = DependencyProperty.Register(
            "Centered", typeof(bool), typeof(Modal),
            new FrameworkPropertyMetadata(false, OnCardLayoutChanged));

        /// <summary>false sits the card 100px below the top edge, like antd's default `top`.</summary>
        public bool Centered
        {
            get { return (bool)GetValue(CenteredProperty); }
            set { SetValue(CenteredProperty, value); }
        }

        public static readonly DependencyProperty OkTextProperty = DependencyProperty.Register(
            "OkText", typeof(string), typeof(Modal), new FrameworkPropertyMetadata("OK"));

        public string OkText
        {
            get { return (string)GetValue(OkTextProperty); }
            set { SetValue(OkTextProperty, value); }
        }

        public static readonly DependencyProperty CancelTextProperty = DependencyProperty.Register(
            "CancelText", typeof(string), typeof(Modal), new FrameworkPropertyMetadata("Cancel"));

        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        public static readonly DependencyProperty OkLoadingProperty = DependencyProperty.Register(
            "OkLoading", typeof(bool), typeof(Modal), new FrameworkPropertyMetadata(false));

        public bool OkLoading
        {
            get { return (bool)GetValue(OkLoadingProperty); }
            set { SetValue(OkLoadingProperty, value); }
        }

        private static readonly DependencyPropertyKey CustomFooterVisibilityKey = DependencyProperty.RegisterReadOnly(
            "CustomFooterVisibility", typeof(Visibility), typeof(Modal),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty CustomFooterVisibilityProperty =
            CustomFooterVisibilityKey.DependencyProperty;

        /// <summary>Visible when a <see cref="Footer"/> object was supplied.</summary>
        public Visibility CustomFooterVisibility
        {
            get { return (Visibility)GetValue(CustomFooterVisibilityProperty); }
            private set { SetValue(CustomFooterVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey DefaultFooterVisibilityKey = DependencyProperty.RegisterReadOnly(
            "DefaultFooterVisibility", typeof(Visibility), typeof(Modal),
            new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty DefaultFooterVisibilityProperty =
            DefaultFooterVisibilityKey.DependencyProperty;

        /// <summary>Visible for the built-in OK / Cancel row.</summary>
        public Visibility DefaultFooterVisibility
        {
            get { return (Visibility)GetValue(DefaultFooterVisibilityProperty); }
            private set { SetValue(DefaultFooterVisibilityKey, value); }
        }

        private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Modal)d).UpdateVisualState();
        }

        protected override void OnVisualStateChanged()
        {
            SetValue(CustomFooterVisibilityKey,
                ShowFooter && Footer != null ? Visibility.Visible : Visibility.Collapsed);
            SetValue(DefaultFooterVisibilityKey,
                ShowFooter && Footer == null ? Visibility.Visible : Visibility.Collapsed);
        }

        /// <summary>Raised by the OK button. The dialog closes unless a handler sets <c>Handled</c>.</summary>
        public event EventHandler<ModalButtonEventArgs> Ok;

        public event EventHandler Canceled;

        private Button _okButton;
        private Button _cancelButton;
        private FrameworkElement _closeHost;

        protected override void AttachParts()
        {
            UpdateCardLayout();

            _okButton = GetTemplateChild("PART_Ok") as Button;
            if (_okButton != null) _okButton.Click += OnOkClick;

            _cancelButton = GetTemplateChild("PART_Cancel") as Button;
            if (_cancelButton != null) _cancelButton.Click += OnCancelClick;

            if (_closeHost != null) { _closeHost.MouseLeftButtonUp -= OnCloseClick; _closeHost = null; }
            _closeHost = GetTemplateChild("PART_Close") as FrameworkElement;
            if (_closeHost != null) _closeHost.MouseLeftButtonUp += OnCloseClick;
        }

        protected override void DetachParts()
        {
            if (_okButton != null) { _okButton.Click -= OnOkClick; _okButton = null; }
            if (_cancelButton != null) { _cancelButton.Click -= OnCancelClick; _cancelButton = null; }
            if (_closeHost != null) { _closeHost.MouseLeftButtonUp -= OnCloseClick; _closeHost = null; }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            var args = new ModalButtonEventArgs();
            EventHandler<ModalButtonEventArgs> handler = Ok;
            if (handler != null) handler(this, args);
            if (!args.Handled) Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            EventHandler handler = Canceled;
            if (handler != null) handler(this, EventArgs.Empty);
            Close();
        }

        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            EventHandler handler = Canceled;
            if (handler != null) handler(this, EventArgs.Empty);
            Close();
        }

        private static void OnCardLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Modal)d).UpdateCardLayout();
        }

        private void UpdateCardLayout()
        {
            FrameworkElement card = DialogCard;
            if (card == null) return;

            card.Width = DialogWidth;
            card.VerticalAlignment = Centered ? VerticalAlignment.Center : VerticalAlignment.Top;
            card.Margin = Centered ? new Thickness(0) : new Thickness(0, 100, 0, 0);
        }
    }

    /// <summary>Event data for <see cref="Modal.Ok"/>; set <see cref="Handled"/> to keep the dialog open.</summary>
    public class ModalButtonEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }

    /// <summary>
    /// antd Drawer. Measured at the default 378 width: a 57px header (16/24 padding plus a hairline),
    /// a 24px body padding, and a 49px footer above a hairline.
    /// </summary>
    public class Drawer : DialogBase
    {
        static Drawer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Drawer), new FrameworkPropertyMetadata(typeof(Drawer)));
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement", typeof(AntdPlacement), typeof(Drawer),
            new FrameworkPropertyMetadata(AntdPlacement.Right, OnCardLayoutChanged));

        /// <summary>Edge the panel slides in from.</summary>
        public AntdPlacement Placement
        {
            get { return (AntdPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Drawer), new FrameworkPropertyMetadata(null));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof(object), typeof(Drawer),
            new FrameworkPropertyMetadata(null, OnFooterChanged));

        /// <summary>Footer slot. Null removes the whole footer bar, like antd.</summary>
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Drawer)d).UpdateVisualState();
        }

        public static readonly DependencyProperty DrawerWidthProperty = DependencyProperty.Register(
            "DrawerWidth", typeof(double), typeof(Drawer),
            new FrameworkPropertyMetadata(378d, OnCardLayoutChanged));

        public double DrawerWidth
        {
            get { return (double)GetValue(DrawerWidthProperty); }
            set { SetValue(DrawerWidthProperty, value); }
        }

        public static readonly DependencyProperty DrawerHeightProperty = DependencyProperty.Register(
            "DrawerHeight", typeof(double), typeof(Drawer),
            new FrameworkPropertyMetadata(378d, OnCardLayoutChanged));

        public double DrawerHeight
        {
            get { return (double)GetValue(DrawerHeightProperty); }
            set { SetValue(DrawerHeightProperty, value); }
        }

        public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register(
            "FooterVisibility", typeof(Visibility), typeof(Drawer), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public Visibility FooterVisibility
        {
            get { return (Visibility)GetValue(FooterVisibilityProperty); }
            set { SetValue(FooterVisibilityProperty, value); }
        }

        private FrameworkElement _closeHost;

        protected override void AttachParts()
        {
            UpdateCardLayout();

            _closeHost = GetTemplateChild("PART_Close") as FrameworkElement;
            if (_closeHost != null) _closeHost.MouseLeftButtonUp += OnCloseClick;
        }

        protected override void DetachParts()
        {
            if (_closeHost != null) { _closeHost.MouseLeftButtonUp -= OnCloseClick; _closeHost = null; }
        }

        private static void OnCardLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Drawer)d).UpdateCardLayout();
        }

        private void UpdateCardLayout()
        {
            FrameworkElement card = DialogCard;
            if (card == null) return;

            switch (Placement)
            {
                case AntdPlacement.Left:
                    card.Width = DrawerWidth;
                    card.Height = double.NaN;
                    card.HorizontalAlignment = HorizontalAlignment.Left;
                    card.VerticalAlignment = VerticalAlignment.Stretch;
                    break;
                case AntdPlacement.Top:
                    card.Width = double.NaN;
                    card.Height = DrawerHeight;
                    card.HorizontalAlignment = HorizontalAlignment.Stretch;
                    card.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case AntdPlacement.Bottom:
                    card.Width = double.NaN;
                    card.Height = DrawerHeight;
                    card.HorizontalAlignment = HorizontalAlignment.Stretch;
                    card.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                default:
                    card.Width = DrawerWidth;
                    card.Height = double.NaN;
                    card.HorizontalAlignment = HorizontalAlignment.Right;
                    card.VerticalAlignment = VerticalAlignment.Stretch;
                    break;
            }
        }

        protected override void OnVisualStateChanged()
        {
            SetCurrentValue(FooterVisibilityProperty, Footer != null ? Visibility.Visible : Visibility.Collapsed);
        }

        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        protected override void AnimateIn()
        {
            if (DialogCard == null) return;

            double distance;
            switch (Placement)
            {
                case AntdPlacement.Left: distance = -DrawerWidth; break;
                case AntdPlacement.Top: distance = -DrawerHeight; break;
                case AntdPlacement.Bottom: distance = DrawerHeight; break;
                default: distance = DrawerWidth; break;
            }

            bool horizontal = Placement == AntdPlacement.Left || Placement == AntdPlacement.Right;
            var slide = new TranslateTransform(horizontal ? distance : 0, horizontal ? 0 : distance);
            SetCardTransform(slide);

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var duration = TimeSpan.FromMilliseconds(300);
            if (horizontal)
            {
                slide.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(distance, 0, duration) { EasingFunction = ease });
            }
            else
            {
                slide.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(distance, 0, duration) { EasingFunction = ease });
            }

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { EasingFunction = ease };
            DialogCard.BeginAnimation(UIElement.OpacityProperty, fade);
        }
    }
}
