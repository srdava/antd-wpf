using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// antd ColorPicker: a swatch field with a saturation/value square, a hue bar and presets.
    ///
    /// WPF has no HSL/HSV picker built in, so the control keeps H/S/V as state and only converts
    /// to RGB when the value changes. The square is three stacked rectangles (hue, white, black
    /// gradients) which is the classic composition and avoids a per-pixel writeable bitmap.
    /// </summary>
    public class ColorPicker : Control
    {
        private FrameworkElement _saturation;
        private FrameworkElement _hue;
        private double _h = 217;
        private double _s = 0.91;
        private double _v = 1.0;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            Presets = DefaultPresets();
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(Color?), typeof(ColorPicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public Color? SelectedColor
        {
            get { return (Color?)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(true));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty PresetsProperty = DependencyProperty.Register(
            "Presets", typeof(IEnumerable<Color>), typeof(ColorPicker),
            new FrameworkPropertyMetadata(null));

        public IEnumerable<Color> Presets
        {
            get { return (IEnumerable<Color>)GetValue(PresetsProperty); }
            set { SetValue(PresetsProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(ColorPicker),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey HexTextKey = DependencyProperty.RegisterReadOnly(
            "HexText", typeof(string), typeof(ColorPicker), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty HexTextProperty = HexTextKey.DependencyProperty;

        public string HexText
        {
            get { return (string)GetValue(HexTextProperty); }
            private set { SetValue(HexTextKey, value); }
        }

        private static readonly DependencyPropertyKey SwatchBrushKey = DependencyProperty.RegisterReadOnly(
            "SwatchBrush", typeof(Brush), typeof(ColorPicker), new FrameworkPropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty SwatchBrushProperty = SwatchBrushKey.DependencyProperty;

        public Brush SwatchBrush
        {
            get { return (Brush)GetValue(SwatchBrushProperty); }
            private set { SetValue(SwatchBrushKey, value); }
        }

        private static readonly DependencyPropertyKey HueBrushKey = DependencyProperty.RegisterReadOnly(
            "HueBrush", typeof(Brush), typeof(ColorPicker), new FrameworkPropertyMetadata(Brushes.Blue));

        public static readonly DependencyProperty HueBrushProperty = HueBrushKey.DependencyProperty;

        /// <summary>Fully saturated colour at the current hue, the base layer of the square.</summary>
        public Brush HueBrush
        {
            get { return (Brush)GetValue(HueBrushProperty); }
            private set { SetValue(HueBrushKey, value); }
        }

        private static readonly DependencyPropertyKey MarkerXKey = DependencyProperty.RegisterReadOnly(
            "MarkerX", typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty MarkerXProperty = MarkerXKey.DependencyProperty;

        public double MarkerX
        {
            get { return (double)GetValue(MarkerXProperty); }
            private set { SetValue(MarkerXKey, value); }
        }

        private static readonly DependencyPropertyKey MarkerYKey = DependencyProperty.RegisterReadOnly(
            "MarkerY", typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty MarkerYProperty = MarkerYKey.DependencyProperty;

        public double MarkerY
        {
            get { return (double)GetValue(MarkerYProperty); }
            private set { SetValue(MarkerYKey, value); }
        }

        private static readonly DependencyPropertyKey HueOffsetKey = DependencyProperty.RegisterReadOnly(
            "HueOffset", typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty HueOffsetProperty = HueOffsetKey.DependencyProperty;

        public double HueOffset
        {
            get { return (double)GetValue(HueOffsetProperty); }
            private set { SetValue(HueOffsetKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearProperty = CanClearKey.DependencyProperty;

        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            private set { SetValue(CanClearKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_saturation != null)
            {
                _saturation.MouseLeftButtonDown -= OnSaturationDown;
                _saturation.MouseMove -= OnSaturationMove;
                _saturation.MouseLeftButtonUp -= OnSaturationUp;
            }
            if (_hue != null)
            {
                _hue.MouseLeftButtonDown -= OnHueDown;
                _hue.MouseMove -= OnHueMove;
                _hue.MouseLeftButtonUp -= OnHueUp;
            }

            _saturation = GetTemplateChild("PART_Saturation") as FrameworkElement;
            _hue = GetTemplateChild("PART_Hue") as FrameworkElement;
            var button = GetTemplateChild("PART_ClearButton") as Button;
            var popup = GetTemplateChild("PART_Popup") as System.Windows.Controls.Primitives.Popup;
            var field = GetTemplateChild("PART_Field") as FrameworkElement;
            PopupAnchor.Attach(popup, this);

            if (_saturation != null)
            {
                _saturation.MouseLeftButtonDown += OnSaturationDown;
                _saturation.MouseMove += OnSaturationMove;
                _saturation.MouseLeftButtonUp += OnSaturationUp;
            }
            if (_hue != null)
            {
                _hue.MouseLeftButtonDown += OnHueDown;
                _hue.MouseMove += OnHueMove;
                _hue.MouseLeftButtonUp += OnHueUp;
            }
            if (field != null) field.MouseLeftButtonUp += OnFieldClick;
            if (button != null) button.Click += OnClearClick;
            if (popup != null) popup.Closed += OnPopupClosed;

            SyncFromColor();
        }

        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedColorProperty, null);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            var popup = picker.GetTemplateChild("PART_Popup") as System.Windows.Controls.Primitives.Popup;
            if (popup != null) popup.IsOpen = picker.IsDropDownOpen;
            picker.UpdateVisualState();
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            picker.SyncFromColor();
        }

        // ---------------- pointer handling

        private bool _draggingSquare;
        private bool _draggingHue;

        private void OnSaturationDown(object sender, MouseButtonEventArgs e)
        {
            _draggingSquare = true;
            _saturation.CaptureMouse();
            ApplySquare(e.GetPosition(_saturation));
            e.Handled = true;
        }

        private void OnSaturationMove(object sender, MouseEventArgs e)
        {
            if (!_draggingSquare) return;
            ApplySquare(e.GetPosition(_saturation));
        }

        private void OnSaturationUp(object sender, MouseButtonEventArgs e)
        {
            _draggingSquare = false;
            if (_saturation != null) _saturation.ReleaseMouseCapture();
        }

        private void ApplySquare(Point point)
        {
            if (_saturation == null) return;

            double width = Math.Max(1, _saturation.ActualWidth);
            double height = Math.Max(1, _saturation.ActualHeight);

            _s = Clamp01(point.X / width);
            _v = 1 - Clamp01(point.Y / height);
            Commit();
        }

        private void OnHueDown(object sender, MouseButtonEventArgs e)
        {
            _draggingHue = true;
            _hue.CaptureMouse();
            ApplyHue(e.GetPosition(_hue));
            e.Handled = true;
        }

        private void OnHueMove(object sender, MouseEventArgs e)
        {
            if (!_draggingHue) return;
            ApplyHue(e.GetPosition(_hue));
        }

        private void OnHueUp(object sender, MouseButtonEventArgs e)
        {
            _draggingHue = false;
            if (_hue != null) _hue.ReleaseMouseCapture();
        }

        private void ApplyHue(Point point)
        {
            if (_hue == null) return;
            double height = Math.Max(1, _hue.ActualHeight);
            _h = Clamp01(point.Y / height) * 360;
            Commit();
        }

        /// <summary>Called by preset swatches in the template.</summary>
        internal void SelectPreset(Color color)
        {
            SetCurrentValue(SelectedColorProperty, color);
        }

        private void Commit()
        {
            AntdColor rgb = AntdColor.FromHsv(_h, _s, _v, 1.0);
            var color = Color.FromRgb(rgb.R, rgb.G, rgb.B);
            SetCurrentValue(SelectedColorProperty, color);
            UpdateDerived();
        }

        private void SyncFromColor()
        {
            Color? selected = SelectedColor;
            if (selected.HasValue)
            {
                Hsv hsv = ToHsv(selected.Value);
                _h = hsv.H;
                _s = hsv.S;
                _v = hsv.V;
            }

            SetValue(HexTextKey, selected.HasValue ? "#" + selected.Value.R.ToString("X2")
                + selected.Value.G.ToString("X2") + selected.Value.B.ToString("X2") : string.Empty);
            SetValue(CanClearKey, AllowClear && selected.HasValue && IsEnabled);
            UpdateDerived();
        }

        private void UpdateDerived()
        {
            SetValue(SwatchBrushKey, new SolidColorBrush(
                SelectedColor.HasValue ? SelectedColor.Value : Colors.Transparent));

            AntdColor hue = AntdColor.FromHsv(_h, 1, 1, 1);
            var hueBrush = new SolidColorBrush(Color.FromRgb(hue.R, hue.G, hue.B));
            hueBrush.Freeze();
            SetValue(HueBrushKey, hueBrush);

            SetValue(MarkerXKey, _s);
            SetValue(MarkerYKey, 1 - _v);
            SetValue(HueOffsetKey, _h / 360.0);
        }

        private static Hsv ToHsv(Color color)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = 0;
            if (delta > 0)
            {
                if (Math.Abs(max - r) < 0.0001) hue = 60 * (((g - b) / delta) % 6);
                else if (Math.Abs(max - g) < 0.0001) hue = 60 * (((b - r) / delta) + 2);
                else hue = 60 * (((r - g) / delta) + 4);
            }
            if (hue < 0) hue += 360;

            double saturation = max <= 0 ? 0 : delta / max;
            return new Hsv(hue, saturation, max);
        }

        private static double Clamp01(double value)
        {
            return value < 0 ? 0 : (value > 1 ? 1 : value);
        }

        private static IEnumerable<Color> DefaultPresets()
        {
            return new[]
            {
                Color.FromRgb(0x16, 0x77, 0xFF),
                Color.FromRgb(0x52, 0xC4, 0x1A),
                Color.FromRgb(0xFA, 0xAD, 0x14),
                Color.FromRgb(0xFF, 0x4D, 0x4F),
                Color.FromRgb(0x72, 0x2E, 0xD1),
                Color.FromRgb(0x13, 0xC2, 0xC2),
                Color.FromRgb(0xEB, 0x2F, 0x96)
            };
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            string borderKey = !IsEnabled ? "colorBorderDisabled"
                : (IsDropDownOpen ? "colorPrimary" : "colorBorder");

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(t.BorderRadius));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(MinHeightProperty, t.ControlHeight);
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(AntdSize.Middle), 4,
                Metrics.InputPaddingInline(AntdSize.Middle), 4));
        }
    }

    /// <summary>Preset swatch inside a ColorPicker popup.</summary>
    public class ColorPresetButton : Button
    {
        static ColorPresetButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPresetButton),
                new FrameworkPropertyMetadata(typeof(ColorPresetButton)));
        }

        public static readonly DependencyProperty SwatchProperty = DependencyProperty.Register(
            "Swatch", typeof(Color), typeof(ColorPresetButton),
            new FrameworkPropertyMetadata(Colors.Blue));

        public Color Swatch
        {
            get { return (Color)GetValue(SwatchProperty); }
            set { SetValue(SwatchProperty, value); }
        }

        public ColorPresetButton()
        {
            Click += OnClick;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            DependencyObject node = this;
            while (node != null)
            {
                var picker = node as ColorPicker;
                if (picker != null) { picker.SelectPreset(Swatch); break; }
                node = VisualTreeHelper.GetParent(node);
            }
            e.Handled = true;
        }
    }
}
