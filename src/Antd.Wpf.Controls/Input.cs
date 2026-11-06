using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// Ant Design input. Derives from TextBox so text editing, selection, IME and undo keep
    /// working; only the visual layer is replaced by the antd template.
    /// </summary>
    public class Input : TextBox
    {
        static Input()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Input), new FrameworkPropertyMetadata(typeof(Input)));
        }

        public Input()
        {
            IsEnabledChanged += OnIsEnabledChanged;
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        // ---------------------------------------------------------------- properties

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Input),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(AntdStatus), typeof(Input),
            new FrameworkPropertyMetadata(AntdStatus.None, OnAppearanceChanged));

        public AntdStatus Status
        {
            get { return (AntdStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(Input),
            new FrameworkPropertyMetadata(string.Empty));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(Input),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(
            "Prefix", typeof(object), typeof(Input),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public object Prefix
        {
            get { return GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        public static readonly DependencyProperty SuffixProperty = DependencyProperty.Register(
            "Suffix", typeof(object), typeof(Input),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public object Suffix
        {
            get { return GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Input),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderBrushProperty = DependencyProperty.Register(
            "PlaceholderBrush", typeof(Brush), typeof(Input),
            new FrameworkPropertyMetadata(Brushes.Gray));

        public Brush PlaceholderBrush
        {
            get { return (Brush)GetValue(PlaceholderBrushProperty); }
            private set { SetValue(PlaceholderBrushProperty, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(Input), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearProperty = CanClearKey.DependencyProperty;

        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            private set { SetValue(CanClearKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(Input), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        /// <summary>True when the placeholder should be painted over the empty content host.</summary>
        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private static readonly DependencyPropertyKey ContentHeightKey = DependencyProperty.RegisterReadOnly(
            "ContentHeight", typeof(double), typeof(Input), new FrameworkPropertyMetadata(22.0));

        public static readonly DependencyProperty ContentHeightProperty = ContentHeightKey.DependencyProperty;
        private static readonly DependencyPropertyKey HostHeightKey = DependencyProperty.RegisterReadOnly(
            "HostHeight", typeof(double), typeof(Input), new FrameworkPropertyMetadata(30.0));

        public static readonly DependencyProperty HostHeightProperty = HostHeightKey.DependencyProperty;

        /// <summary>
        /// Height of the text host. WPF copies TextBox.Padding into the host's own padding, so the
        /// host has to be taller than the text area by the vertical padding; giving it the text
        /// height (22) clipped the top of every glyph.
        /// </summary>
        public double HostHeight
        {
            get { return (double)GetValue(HostHeightProperty); }
            private set { SetValue(HostHeightKey, value); }
        }

        /// <summary>
        /// Height of the editable area: controlHeight - border - padding. Pinning it keeps the
        /// field exactly 32 tall even when the font's line spacing wants more room.
        /// </summary>
        public double ContentHeight
        {
            get { return (double)GetValue(ContentHeightProperty); }
            private set { SetValue(ContentHeightKey, value); }
        }

        private static readonly DependencyPropertyKey HasPrefixKey = DependencyProperty.RegisterReadOnly(
            "HasPrefix", typeof(bool), typeof(Input), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasPrefixProperty = HasPrefixKey.DependencyProperty;

        public bool HasPrefix
        {
            get { return (bool)GetValue(HasPrefixProperty); }
            private set { SetValue(HasPrefixKey, value); }
        }

        private static readonly DependencyPropertyKey HasSuffixKey = DependencyProperty.RegisterReadOnly(
            "HasSuffix", typeof(bool), typeof(Input), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasSuffixProperty = HasSuffixKey.DependencyProperty;

        public bool HasSuffix
        {
            get { return (bool)GetValue(HasSuffixProperty); }
            private set { SetValue(HasSuffixKey, value); }
        }

        private static readonly DependencyPropertyKey IsFocusedStateKey = DependencyProperty.RegisterReadOnly(
            "IsFocusedState", typeof(bool), typeof(Input), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsFocusedStateProperty = IsFocusedStateKey.DependencyProperty;

        /// <summary>True while the control owns keyboard focus; drives the antd focus glow.</summary>
        public bool IsFocusedState
        {
            get { return (bool)GetValue(IsFocusedStateProperty); }
            private set { SetValue(IsFocusedStateKey, value); }
        }

        // ---------------------------------------------------------------- behavior

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Input)d).UpdateVisualState();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateVisualState();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateVisualState();
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _prefixHost = GetTemplateChild("PrefixHost") as FrameworkElement;
            _accessory = GetTemplateChild("PART_Accessory") as FrameworkElement;

            var clearButton = GetTemplateChild("PART_ClearButton") as System.Windows.Controls.Button;
            if (clearButton != null)
            {
                clearButton.Click -= OnClearClicked;
                clearButton.Click += OnClearClicked;
            }
        }

        private FrameworkElement _prefixHost;
        private FrameworkElement _accessory;

        /// <summary>
        /// The chrome border no longer carries the padding (the text host applies TextBox.Padding
        /// itself), so the prefix and the suffix stack inset themselves to stay on the text's
        /// baseline instead of hugging the border.
        /// </summary>
        private void ApplyInsets()
        {
            Thickness padding = Padding;
            if (_prefixHost != null) _prefixHost.Margin = new Thickness(padding.Left, 0, 6, 0);
            if (_accessory != null) _accessory.Margin = new Thickness(0, 0, padding.Left, 0);
        }
        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
            Focus();
        }

        /// <summary>Recomputes every visual value from the active theme snapshot.</summary>
        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool focused = IsKeyboardFocusWithin;
            bool hover = IsMouseOver && IsEnabled;

            string borderKey;
            if (!IsEnabled)
            {
                borderKey = "colorBorderDisabled";
            }
            else if (Status == AntdStatus.Error)
            {
                borderKey = focused ? "colorError" : (hover ? "colorErrorHover" : "colorError");
            }
            else if (Status == AntdStatus.Warning)
            {
                borderKey = focused ? "colorWarning" : (hover ? "colorWarningHover" : "colorWarning");
            }
            else if (focused)
            {
                borderKey = "colorPrimary";
            }
            else if (hover)
            {
                borderKey = "colorPrimaryHover";
            }
            else
            {
                borderKey = "colorBorder";
            }

            string backgroundKey = IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled";
            string foregroundKey = IsEnabled ? "colorText" : "colorTextDisabled";

            double height = Metrics.ControlHeight(t, Size);
            double inline = Metrics.InputPaddingInline(Size);
            double fontSize = Metrics.FontSize(t, Size);

            SetCurrentValue(BackgroundProperty, snapshot.Brush(backgroundKey));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(foregroundKey));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(Metrics.Radius(t, Size)));
            SetCurrentValue(FontSizeProperty, fontSize);
            SetCurrentValue(MinHeightProperty, height);
            // antd: 4px vertical for the default size, 6.5 for large, 0 for small.
            double block = Size == AntdSize.Large ? 6.5 : (Size == AntdSize.Small ? 0 : 4);
            SetCurrentValue(PaddingProperty, new Thickness(inline, block, inline, block));
            SetValue(ContentHeightKey, height - 2 * t.LineWidth - 2 * block);
            SetValue(HostHeightKey, height - 2 * t.LineWidth);
            ApplyInsets();
            SetCurrentValue(PlaceholderBrushProperty, snapshot.Brush("colorTextPlaceholder"));
            SetValue(IsFocusedStateKey, focused && IsEnabled);
            SetValue(CanClearKey, AllowClear && !string.IsNullOrEmpty(Text) && IsEnabled);
            SetValue(ShowPlaceholderKey, !string.IsNullOrEmpty(Placeholder) && string.IsNullOrEmpty(Text));
            SetValue(HasPrefixKey, Prefix != null);
            SetValue(HasSuffixKey, Suffix != null);
        }
    }
}
