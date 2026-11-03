using System;
using System.ComponentModel;
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
    /// Ant Design button. Visual states are resolved in code from design tokens instead of
    /// XAML triggers: WPF forbids Binding inside Setter.Value, so a token driven variant matrix
    /// cannot be expressed declaratively. Resolving to token *names* keeps the brush lookup on
    /// the frozen cache instead of allocating colors per state change.
    /// </summary>
    public class Button : System.Windows.Controls.Primitives.ButtonBase
    {
        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
        }

        private readonly DependencyPropertyDescriptor _isPressedDescriptor;
        private bool _pressed;

        public Button()
        {
            _isPressedDescriptor = DependencyPropertyDescriptor.FromProperty(IsPressedProperty, typeof(Button));
            _isPressedDescriptor.AddValueChanged(this, OnIsPressedChanged);

            IsEnabledChanged += OnIsEnabledChanged;
            Unloaded += OnUnloaded;
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        // ---------------------------------------------------------------- properties

        public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
            "Variant", typeof(ButtonVariant), typeof(Button),
            new FrameworkPropertyMetadata(ButtonVariant.Default, OnAppearanceChanged));

        public ButtonVariant Variant
        {
            get { return (ButtonVariant)GetValue(VariantProperty); }
            set { SetValue(VariantProperty, value); }
        }

        public static readonly DependencyProperty DangerProperty = DependencyProperty.Register(
            "Danger", typeof(bool), typeof(Button),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Danger
        {
            get { return (bool)GetValue(DangerProperty); }
            set { SetValue(DangerProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Button),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            "Shape", typeof(ButtonShape), typeof(Button),
            new FrameworkPropertyMetadata(ButtonShape.Default, OnAppearanceChanged));

        public ButtonShape Shape
        {
            get { return (ButtonShape)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(Button),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        /// <summary>Icon content: a Geometry, ImageSource, or any UIElement.</summary>
        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register(
            "IconPlacement", typeof(ButtonIconPlacement), typeof(Button),
            new FrameworkPropertyMetadata(ButtonIconPlacement.Start, OnAppearanceChanged));

        public ButtonIconPlacement IconPlacement
        {
            get { return (ButtonIconPlacement)GetValue(IconPlacementProperty); }
            set { SetValue(IconPlacementProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty = DependencyProperty.Register(
            "Loading", typeof(bool), typeof(Button),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            "Block", typeof(bool), typeof(Button),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        /// <summary>Stretch to the container width, matching antd's block button.</summary>
        public bool Block
        {
            get { return (bool)GetValue(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Button),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(Button),
            new FrameworkPropertyMetadata(1.0));

        /// <summary>Stroke width used by the dashed outline; mirrors the lineWidth token.</summary>
        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(
            "IconMargin", typeof(Thickness), typeof(Button),
            new FrameworkPropertyMetadata(new Thickness(0)));

        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        public static readonly DependencyProperty HasIconProperty = DependencyProperty.Register(
            "HasIcon", typeof(bool), typeof(Button),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool HasIcon
        {
            get { return (bool)GetValue(HasIconProperty); }
            private set { SetValue(HasIconProperty, value); }
        }

        private static readonly DependencyPropertyKey IsDashedKey = DependencyProperty.RegisterReadOnly(
            "IsDashed", typeof(bool), typeof(Button), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsDashedProperty = IsDashedKey.DependencyProperty;

        /// <summary>True for antd's dashed button; the template swaps the solid border for a dashed outline.</summary>
        public bool IsDashed
        {
            get { return (bool)GetValue(IsDashedProperty); }
            private set { SetValue(IsDashedKey, value); }
        }

        // ---------------------------------------------------------------- appearance

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Button)d).UpdateVisualState();
        }

        private void OnIsPressedChanged(object sender, EventArgs e)
        {
            _pressed = IsPressed;
            UpdateVisualState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_isPressedDescriptor != null)
            {
                _isPressedDescriptor.RemoveValueChanged(this, OnIsPressedChanged);
            }
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

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState();
        }

        /// <summary>Recomputes every visual value from the active theme snapshot.</summary>
        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool interactive = IsEnabled && !Loading;
            bool hover = interactive && IsMouseOver;
            bool pressed = interactive && _pressed;

            string backgroundKey;
            string foregroundKey;
            string borderKey;
            bool dashed = false;

            string main = Danger ? "colorError" : "colorPrimary";
            string mainHover = Danger ? "colorErrorHover" : "colorPrimaryHover";
            string mainActive = Danger ? "colorErrorActive" : "colorPrimaryActive";

            switch (Variant)
            {
                case ButtonVariant.Primary:
                    backgroundKey = pressed ? mainActive : (hover ? mainHover : main);
                    foregroundKey = "colorTextLightSolid";
                    borderKey = backgroundKey;
                    break;

                case ButtonVariant.Dashed:
                    dashed = true;
                    backgroundKey = "colorBgContainer";
                    foregroundKey = pressed ? mainActive : (hover ? mainHover : "colorText");
                    borderKey = pressed ? mainActive : (hover ? mainHover : "colorBorder");
                    break;

                case ButtonVariant.Default:
                    backgroundKey = "colorBgContainer";
                    foregroundKey = pressed ? mainActive : (hover ? mainHover : "colorText");
                    borderKey = pressed ? mainActive : (hover ? mainHover : "colorBorder");
                    break;

                case ButtonVariant.Text:
                    backgroundKey = pressed ? "colorBgTextActive" : (hover ? "colorBgTextHover" : null);
                    foregroundKey = pressed ? mainActive : (hover ? mainHover : "colorText");
                    borderKey = null;
                    break;

                default: // Link
                    backgroundKey = null;
                    foregroundKey = pressed ? mainActive : (hover ? mainHover : "colorLink");
                    borderKey = null;
                    break;
            }

            if (!IsEnabled)
            {
                foregroundKey = "colorTextDisabled";
                bool filled = Variant == ButtonVariant.Primary
                    || Variant == ButtonVariant.Default
                    || Variant == ButtonVariant.Dashed;
                backgroundKey = filled ? "colorBgContainerDisabled" : null;
                borderKey = filled ? "colorBorderDisabled" : null;
                dashed = Variant == ButtonVariant.Dashed;
            }

            double height = Metrics.ControlHeight(t, Size);
            double fontSize = Metrics.FontSize(t, Size);
            double radius = Shape == ButtonShape.Circle
                ? height / 2
                : (Shape == ButtonShape.Round ? height / 2 : Metrics.Radius(t, Size));

            double inline = Shape == ButtonShape.Circle ? 0 : Metrics.ButtonPaddingInline(Size);
            bool hasIcon = Icon != null;
            bool hasLabel = Content != null;

            SetCurrentValue(BackgroundProperty, BrushFor(snapshot, backgroundKey));
            SetCurrentValue(ForegroundProperty, BrushFor(snapshot, foregroundKey));
            SetCurrentValue(BorderBrushProperty, BrushFor(snapshot, borderKey));

            var thickness = borderKey == null || dashed ? new Thickness(0) : new Thickness(t.LineWidth);
            SetCurrentValue(BorderThicknessProperty, thickness);
            SetValue(IsDashedKey, dashed);

            SetCurrentValue(PaddingProperty, new Thickness(
                Shape == ButtonShape.Circle ? 0 : inline, 0,
                Shape == ButtonShape.Circle ? 0 : inline, 0));
            SetCurrentValue(FontSizeProperty, fontSize);
            SetCurrentValue(MinHeightProperty, height);
            SetCurrentValue(MinWidthProperty, Shape == ButtonShape.Circle ? height : 0.0);
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(radius));
            SetCurrentValue(OutlineThicknessProperty, t.LineWidth);
            SetCurrentValue(HorizontalAlignmentProperty, Block ? HorizontalAlignment.Stretch : HorizontalAlignment.Left);
            SetCurrentValue(CursorProperty, interactive ? Cursors.Hand : Cursors.Arrow);
            SetCurrentValue(OpacityProperty, Loading ? t.OpacityLoading : 1.0);
            SetCurrentValue(IconMarginProperty, new Thickness(0, 0, hasIcon && hasLabel ? 8 : 0, 0));
            SetCurrentValue(HasIconProperty, hasIcon);
        }

        private static Brush BrushFor(ThemeSnapshot snapshot, string tokenName)
        {
            return tokenName == null ? Brushes.Transparent : snapshot.Brush(tokenName);
        }
    }
}
