using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd InputNumber. Derives from TextBox so typing, selection and IME keep working; the
    /// template adds the stepper buttons on the right.
    /// </summary>
    public class InputNumber : TextBox
    {
        static InputNumber()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InputNumber), new FrameworkPropertyMetadata(typeof(InputNumber)));
        }

        public InputNumber()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double?), typeof(InputNumber),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(double), typeof(InputNumber),
            new FrameworkPropertyMetadata(double.MinValue, OnValueChanged));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(InputNumber),
            new FrameworkPropertyMetadata(double.MaxValue, OnValueChanged));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            "Step", typeof(double), typeof(InputNumber), new FrameworkPropertyMetadata(1.0));

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(
            "Precision", typeof(int), typeof(InputNumber),
            new FrameworkPropertyMetadata(-1, OnValueChanged));

        /// <summary>Decimal places. -1 keeps the parsed representation.</summary>
        public int Precision
        {
            get { return (int)GetValue(PrecisionProperty); }
            set { SetValue(PrecisionProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(InputNumber),
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
            "Size", typeof(AntdSize), typeof(InputNumber),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static readonly DependencyPropertyKey StepUpEnabledKey = DependencyProperty.RegisterReadOnly(
            "StepUpEnabled", typeof(bool), typeof(InputNumber), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty StepUpEnabledProperty = StepUpEnabledKey.DependencyProperty;

        public bool StepUpEnabled
        {
            get { return (bool)GetValue(StepUpEnabledProperty); }
            private set { SetValue(StepUpEnabledKey, value); }
        }

        private static readonly DependencyPropertyKey StepDownEnabledKey = DependencyProperty.RegisterReadOnly(
            "StepDownEnabled", typeof(bool), typeof(InputNumber), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty StepDownEnabledProperty = StepDownEnabledKey.DependencyProperty;

        public bool StepDownEnabled
        {
            get { return (bool)GetValue(StepDownEnabledProperty); }
            private set { SetValue(StepDownEnabledKey, value); }
        }

        private bool _syncing;

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InputNumber)d).UpdateVisualState();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var input = (InputNumber)d;
            if (!input._syncing) input.SyncTextFromValue();
            input.UpdateVisualState();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_syncing) return;

            double parsed;
            _syncing = true;
            try
            {
                if (string.IsNullOrWhiteSpace(Text))
                {
                    SetCurrentValue(ValueProperty, null);
                }
                else if (double.TryParse(Text, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
                {
                    SetCurrentValue(ValueProperty, Clamp(parsed));
                }
            }
            finally
            {
                _syncing = false;
            }

            UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Hook("PART_UpButton", delegate { StepBy(Step); });
            Hook("PART_DownButton", delegate { StepBy(-Step); });
        }

        private void Hook(string partName, RoutedEventHandler handler)
        {
            var button = GetTemplateChild(partName) as System.Windows.Controls.Primitives.ButtonBase;
            if (button == null) return;
            button.Click -= handler;
            button.Click += handler;
            button.Focusable = false;
        }

        private void StepBy(double delta)
        {
            if (!IsEnabled) return;
            double current = Value ?? 0;
            SetCurrentValue(ValueProperty, Clamp(current + delta));
            SyncTextFromValue();
            CaretIndex = Text == null ? 0 : Text.Length;
        }

        private double Clamp(double value)
        {
            if (value < Minimum) value = Minimum;
            if (value > Maximum) value = Maximum;
            return value;
        }

        private void SyncTextFromValue()
        {
            double? value = Value;
            string text = value.HasValue
                ? (Precision >= 0
                    ? value.Value.ToString("F" + Precision.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture)
                    : value.Value.ToString(CultureInfo.CurrentCulture))
                : string.Empty;

            if (Text == text) return;

            _syncing = true;
            try
            {
                Text = text;
            }
            finally
            {
                _syncing = false;
            }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            double height = Metrics.ControlHeight(t, Size);

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(IsFocused ? "colorPrimary" : "colorBorder"));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(FontSizeProperty, Metrics.FontSize(t, Size));
            SetCurrentValue(MinHeightProperty, height);
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(Metrics.Radius(t, Size)));
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(Size), 0, 0, 0));
            SetCurrentValue(CaretBrushProperty, snapshot.Brush("colorText"));

            double? value = Value;
            SetValue(StepUpEnabledKey, !value.HasValue || value.Value < Maximum);
            SetValue(StepDownEnabledKey, !value.HasValue || value.Value > Minimum);
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateVisualState();
        }
    }
}
