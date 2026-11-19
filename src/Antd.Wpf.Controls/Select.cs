using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Select. Built on ComboBox so ItemsSource, selection, keyboard handling and
    /// popup placement come from the framework; only the visual tree is antd's.
    /// Multi-select and remote search are not covered yet (see M2 notes).
    /// </summary>
    public class Select : ComboBox
    {
        static Select()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Select), new FrameworkPropertyMetadata(typeof(Select)));
        }

        public Select()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Select),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        /// <summary>
        /// Same radius the Input uses. Driving it from a DP (instead of a markup extension in
        /// the template) keeps every bordered field on one code path, so they cannot drift apart.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Select),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(AntdStatus), typeof(Select),
            new FrameworkPropertyMetadata(AntdStatus.None, OnAppearanceChanged));

        public AntdStatus Status
        {
            get { return (AntdStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(Select),
            new FrameworkPropertyMetadata("Please select"));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        private static readonly DependencyPropertyKey HasSelectionKey = DependencyProperty.RegisterReadOnly(
            "HasSelection", typeof(bool), typeof(Select), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasSelectionProperty = HasSelectionKey.DependencyProperty;

        public bool HasSelection
        {
            get { return (bool)GetValue(HasSelectionProperty); }
            private set { SetValue(HasSelectionKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearSelectionKey = DependencyProperty.RegisterReadOnly(
            "CanClearSelection", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearSelectionProperty = CanClearSelectionKey.DependencyProperty;

        public bool CanClearSelection
        {
            get { return (bool)GetValue(CanClearSelectionProperty); }
            private set { SetValue(CanClearSelectionKey, value); }
        }

        private static readonly DependencyPropertyKey PlaceholderBrushKey = DependencyProperty.RegisterReadOnly(
            "PlaceholderBrush", typeof(Brush), typeof(Select), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty PlaceholderBrushProperty = PlaceholderBrushKey.DependencyProperty;

        public Brush PlaceholderBrush
        {
            get { return (Brush)GetValue(PlaceholderBrushProperty); }
            private set { SetValue(PlaceholderBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ArrowBrushKey = DependencyProperty.RegisterReadOnly(
            "ArrowBrush", typeof(Brush), typeof(Select), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ArrowBrushProperty = ArrowBrushKey.DependencyProperty;

        public Brush ArrowBrush
        {
            get { return (Brush)GetValue(ArrowBrushProperty); }
            private set { SetValue(ArrowBrushKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Select)d).UpdateVisualState();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateVisualState();
        }

        protected override void OnDropDownOpened(System.EventArgs e)
        {
            base.OnDropDownOpened(e);
            UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PopupAnchor.Attach(AntdMotion.Configure(GetTemplateChild("PART_Popup") as System.Windows.Controls.Primitives.Popup), this);

            var clear = GetTemplateChild("PART_ClearButton") as System.Windows.Controls.Button;
            if (clear != null)
            {
                clear.Click -= OnClearClicked;
                clear.Click += OnClearClicked;
            }
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedIndexProperty, -1);
            e.Handled = true;
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool focused = IsKeyboardFocusWithin || IsDropDownOpen;
            string borderKey;
            if (!IsEnabled) borderKey = "colorBorderDisabled";
            else if (Status == AntdStatus.Error) borderKey = "colorError";
            else if (Status == AntdStatus.Warning) borderKey = "colorWarning";
            else if (focused) borderKey = "colorPrimary";
            else if (IsMouseOver) borderKey = "colorPrimaryHover";
            else borderKey = "colorBorder";

            bool hasSelection = SelectedIndex >= 0;

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(FontSizeProperty, Metrics.FontSize(t, Size));
            SetCurrentValue(MinHeightProperty, Metrics.ControlHeight(t, Size));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(Metrics.Radius(t, Size)));
            double block = Size == AntdSize.Large ? 6.5 : (Size == AntdSize.Small ? 0 : 4);
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(Size), block,
                Metrics.InputPaddingInline(Size), block));

            SetValue(HasSelectionKey, hasSelection);
            SetValue(CanClearSelectionKey, AllowClear && hasSelection && IsEnabled);
            SetValue(PlaceholderBrushKey, snapshot.Brush("colorTextPlaceholder"));
            SetValue(ArrowBrushKey, snapshot.Brush(IsEnabled ? "colorTextTertiary" : "colorTextDisabled"));
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }
    }
}
