using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// Shared plumbing for the floating panels (Tooltip / Popover / Dropdown): where the panel
    /// sits, what opens it, and keeping it glued to the trigger while the page scrolls.
    /// </summary>
    public abstract class OverlayBase : ContentControl
    {
        private Popup _popup;

        protected OverlayBase()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();

            // PreviewMouseUp, not MouseLeftButtonUp: the latter is a Direct event in WPF and
            // never reaches an ancestor, so a click on the trigger would go unnoticed. The
            // preview up tunnels through this element on the way down.
            AddHandler(PreviewMouseUpEvent, new MouseButtonEventHandler(OnOverlayClick), true);
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement", typeof(AntdPlacement), typeof(OverlayBase),
            new FrameworkPropertyMetadata(AntdPlacement.Bottom, OnPlacementChanged));

        public AntdPlacement Placement
        {
            get { return (AntdPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty TriggerProperty = DependencyProperty.Register(
            "Trigger", typeof(AntdTrigger), typeof(OverlayBase),
            new FrameworkPropertyMetadata(AntdTrigger.Hover));

        public AntdTrigger Trigger
        {
            get { return (AntdTrigger)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(OverlayBase),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnOpenChanged));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>Gap between the trigger and the panel, matching antd's 4px margin.</summary>
        protected const double Gap = 4;

        protected Popup Popup
        {
            get { return _popup; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_popup != null) _popup.Closed -= OnPopupClosed;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            if (_popup != null)
            {
                PopupAnchor.Attach(_popup, this);
                _popup.Closed += OnPopupClosed;
                _popup.IsOpen = IsOpen;
            }

            ApplyPlacement();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (Trigger == AntdTrigger.Hover) SetCurrentValue(IsOpenProperty, true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (Trigger == AntdTrigger.Hover && !IsMouseOver) SetCurrentValue(IsOpenProperty, false);
        }

        private void OnOverlayClick(object sender, MouseButtonEventArgs e)
        {
            if (Trigger != AntdTrigger.Click) return;
            if (e.ChangedButton != MouseButton.Left) return;

            // Open after the click finishes: a StaysOpen=false popup takes capture the moment it
            // opens, and the still-running mouse up would immediately count as an outside click
            // and close it again.
            Dispatcher.BeginInvoke(new System.Action(() => SetCurrentValue(IsOpenProperty, !IsOpen)));
        }

        private void OnPopupClosed(object sender, System.EventArgs e)
        {
            if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        }

        private static void OnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var overlay = (OverlayBase)d;
            if (overlay._popup != null) overlay._popup.IsOpen = (bool)e.NewValue;
        }

        private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OverlayBase)d).ApplyPlacement();
        }

        private void ApplyPlacement()
        {
            if (_popup == null) return;

            switch (Placement)
            {
                case AntdPlacement.Top:
                    _popup.Placement = PlacementMode.Top;
                    _popup.HorizontalOffset = 0;
                    _popup.VerticalOffset = -Gap;
                    break;
                case AntdPlacement.Left:
                    _popup.Placement = PlacementMode.Left;
                    _popup.HorizontalOffset = -Gap;
                    _popup.VerticalOffset = 0;
                    break;
                case AntdPlacement.Right:
                    _popup.Placement = PlacementMode.Right;
                    _popup.HorizontalOffset = Gap;
                    _popup.VerticalOffset = 0;
                    break;
                default:
                    _popup.Placement = PlacementMode.Bottom;
                    _popup.HorizontalOffset = 0;
                    _popup.VerticalOffset = Gap;
                    break;
            }
        }

        public virtual void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>
    /// antd Tooltip: a dark bubble on hover. Measured from antd, the bubble is 14px text on
    /// rgba(0,0,0,0.85), 6 by 8 padding and a 6 radius.
    /// </summary>
    public class Tooltip : OverlayBase
    {
        static Tooltip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tooltip), new FrameworkPropertyMetadata(typeof(Tooltip)));
        }

        public Tooltip()
        {
            Placement = AntdPlacement.Top;
            Trigger = AntdTrigger.Hover;
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Tooltip), new FrameworkPropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }

    /// <summary>antd Popover: a light card with an optional title, opened on click.</summary>
    public class Popover : OverlayBase
    {
        static Popover()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Popover), new FrameworkPropertyMetadata(typeof(Popover)));
        }

        public Popover()
        {
            Placement = AntdPlacement.Bottom;
            Trigger = AntdTrigger.Click;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Popover), new FrameworkPropertyMetadata(null, OnTitleChanged));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
            "Body", typeof(object), typeof(Popover), new FrameworkPropertyMetadata(null));

        /// <summary>Panel contents. <see cref="ContentControl.Content"/> stays the trigger.</summary>
        public object Body
        {
            get { return GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        private static readonly DependencyPropertyKey HasTitleKey = DependencyProperty.RegisterReadOnly(
            "HasTitle", typeof(bool), typeof(Popover), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasTitleProperty = HasTitleKey.DependencyProperty;

        /// <summary>Drives the head row so an untitled popover has no empty band.</summary>
        public bool HasTitle
        {
            get { return (bool)GetValue(HasTitleProperty); }
            private set { SetValue(HasTitleKey, value); }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popover = (Popover)d;
            popover.SetValue(HasTitleKey, e.NewValue != null);
        }
    }

    /// <summary>
    /// antd Dropdown: a trigger that opens a menu. Items come from <see cref="ItemsSource"/> and
    /// are rendered with a 5 by 12 row that highlights on hover.
    /// </summary>
    public class Dropdown : OverlayBase
    {
        static Dropdown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dropdown), new FrameworkPropertyMetadata(typeof(Dropdown)));
        }

        private ListBox _menu;

        public Dropdown()
        {
            Placement = AntdPlacement.Bottom;
            Trigger = AntdTrigger.Click;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_menu != null) _menu.SelectionChanged -= OnMenuSelectionChanged;
            _menu = GetTemplateChild("PART_Menu") as ListBox;
            if (_menu != null) _menu.SelectionChanged += OnMenuSelectionChanged;
        }

        /// <summary>
        /// The menu is a ListBox so hover and keyboard navigation come for free; picking a row
        /// means "invoke the item", so the selection is cleared straight away and never sticks.
        /// </summary>
        private void OnMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_menu == null || _menu.SelectedIndex < 0) return;

            object item = _menu.SelectedItem;
            _menu.SelectedIndex = -1;
            InvokeItem(item);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(Dropdown), new FrameworkPropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(Dropdown), new FrameworkPropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>Raised with the clicked item as <c>Parameter</c>.</summary>
        public event RoutedPropertyChangedEventHandler<object> ItemInvoked;

        internal void InvokeItem(object item)
        {
            SetCurrentValue(IsOpenProperty, false);
            RoutedPropertyChangedEventHandler<object> handler = ItemInvoked;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<object>(null, item));
            }
        }
    }

    /// <summary>
    /// antd Popconfirm. Measured: a 12px padded card (same surface as Popover), title 14/600 with the
    /// description 4 below, and a 24px-tall button row eight further down. antd reserves a 22px icon
    /// slot on the left of the copy, so the title is inset even when no icon is shown.
    /// </summary>
    public class Popconfirm : OverlayBase
    {
        static Popconfirm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Popconfirm), new FrameworkPropertyMetadata(typeof(Popconfirm)));
        }

        public Popconfirm()
        {
            Placement = AntdPlacement.Bottom;
            Trigger = AntdTrigger.Click;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Popconfirm),
            new FrameworkPropertyMetadata(null, OnTextChanged));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(object), typeof(Popconfirm),
            new FrameworkPropertyMetadata(null, OnTextChanged));

        public object Description
        {
            get { return GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ConfirmTextProperty = DependencyProperty.Register(
            "ConfirmText", typeof(string), typeof(Popconfirm), new FrameworkPropertyMetadata("OK"));

        public string ConfirmText
        {
            get { return (string)GetValue(ConfirmTextProperty); }
            set { SetValue(ConfirmTextProperty, value); }
        }

        public static readonly DependencyProperty CancelTextProperty = DependencyProperty.Register(
            "CancelText", typeof(string), typeof(Popconfirm), new FrameworkPropertyMetadata("Cancel"));

        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        public static readonly DependencyProperty ShowCancelProperty = DependencyProperty.Register(
            "ShowCancel", typeof(bool), typeof(Popconfirm), new FrameworkPropertyMetadata(true));

        public bool ShowCancel
        {
            get { return (bool)GetValue(ShowCancelProperty); }
            set { SetValue(ShowCancelProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            "ShowIcon", typeof(bool), typeof(Popconfirm), new FrameworkPropertyMetadata(false));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
            "IconGeometry", typeof(System.Windows.Media.Geometry), typeof(Popconfirm),
            new FrameworkPropertyMetadata(Antd.Wpf.Icons.AntdIcons.ExclamationCircle));

        public System.Windows.Media.Geometry IconGeometry
        {
            get { return (System.Windows.Media.Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            "IconBrush", typeof(System.Windows.Media.Brush), typeof(Popconfirm),
            new FrameworkPropertyMetadata(null));

        public System.Windows.Media.Brush IconBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        private static readonly DependencyPropertyKey DescriptionVisibilityKey = DependencyProperty.RegisterReadOnly(
            "DescriptionVisibility", typeof(Visibility), typeof(Popconfirm),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty DescriptionVisibilityProperty =
            DescriptionVisibilityKey.DependencyProperty;

        public Visibility DescriptionVisibility
        {
            get { return (Visibility)GetValue(DescriptionVisibilityProperty); }
            private set { SetValue(DescriptionVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey IconVisibilityKey = DependencyProperty.RegisterReadOnly(
            "IconVisibility", typeof(Visibility), typeof(Popconfirm),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty IconVisibilityProperty = IconVisibilityKey.DependencyProperty;

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            private set { SetValue(IconVisibilityKey, value); }
        }

        /// <summary>Raised when the confirm button is pressed; the bubble closes first.</summary>
        public event EventHandler Confirmed;

        public event EventHandler Canceled;

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Popconfirm)d).UpdateVisualState();
        }

        public override void UpdateVisualState()
        {
            base.UpdateVisualState();

            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            SetValue(DescriptionVisibilityKey,
                Description != null ? Visibility.Visible : Visibility.Collapsed);
            SetValue(IconVisibilityKey, ShowIcon ? Visibility.Visible : Visibility.Collapsed);
            SetCurrentValue(IconBrushProperty, snapshot.Brush("colorWarning"));
        }

        internal void HandleConfirm()
        {
            SetCurrentValue(IsOpenProperty, false);
            EventHandler handler = Confirmed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        internal void HandleCancel()
        {
            SetCurrentValue(IsOpenProperty, false);
            EventHandler handler = Canceled;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button ok = GetTemplateChild("PART_Ok") as Button;
            if (ok != null)
            {
                ok.Click -= OnOkClick;
                ok.Click += OnOkClick;
            }

            Button cancel = GetTemplateChild("PART_Cancel") as Button;
            if (cancel != null)
            {
                cancel.Click -= OnCancelClick;
                cancel.Click += OnCancelClick;
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            HandleConfirm();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            HandleCancel();
        }
    }
}
