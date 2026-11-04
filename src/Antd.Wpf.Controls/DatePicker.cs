using System;
using System.Globalization;
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
    /// antd DatePicker: a bordered field that shows the selected date and drops a month panel.
    /// The panel is the existing <see cref="Calendar"/> control, so date maths and styling live
    /// in one place and a future RangePicker can reuse the same building block.
    /// </summary>
    public class DatePicker : Control
    {
        private Border _field;
        private Popup _popup;
        private Calendar _calendar;
        private Button _clear;
        private Button _today;

        static DatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(typeof(DatePicker)));
        }

        public DatePicker()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            "SelectedDate", typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            "Format", typeof(string), typeof(DatePicker),
            new FrameworkPropertyMetadata("yyyy-MM-dd", OnFormatChanged));

        /// <summary>.NET format string used to render the field text.</summary>
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(DatePicker),
            new FrameworkPropertyMetadata("Select date", OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(DatePicker),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(DatePicker),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(DatePicker),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey DisplayTextKey = DependencyProperty.RegisterReadOnly(
            "DisplayText", typeof(string), typeof(DatePicker), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;

        /// <summary>Formatted date, empty when nothing is selected.</summary>
        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            private set { SetValue(DisplayTextKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(DatePicker), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(DatePicker), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearProperty = CanClearKey.DependencyProperty;

        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            private set { SetValue(CanClearKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_field != null) _field.MouseLeftButtonUp -= OnFieldClick;
            if (_calendar != null) _calendar.SelectedDateChanged -= OnCalendarPicked;

            _field = GetTemplateChild("PART_Field") as Border;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _calendar = GetTemplateChild("PART_Calendar") as Calendar;
            _clear = GetTemplateChild("PART_ClearButton") as Button;
            _today = GetTemplateChild("PART_Today") as Button;

            if (_field != null) _field.MouseLeftButtonUp += OnFieldClick;
            if (_clear != null) _clear.Click += OnClearClick;
            if (_today != null) _today.Click += OnTodayClick;
            if (_popup != null) _popup.Closed += OnPopupClosed;
            if (_calendar != null)
            {
                _calendar.SelectedDateChanged += OnCalendarPicked;
                _calendar.SelectedDate = SelectedDate;
            }

            SyncText();
        }

        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedDateProperty, null);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnTodayClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedDateProperty, DateTime.Today);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnCalendarPicked(object sender, DateTime value)
        {
            SetCurrentValue(SelectedDateProperty, value);
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            if (picker._popup != null) picker._popup.IsOpen = picker.IsDropDownOpen;
            picker.UpdateVisualState();
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            if (picker._calendar != null) picker._calendar.SelectedDate = picker.SelectedDate;
            picker.SyncText();
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DatePicker)d).SyncText();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            picker.SyncText();
            picker.UpdateVisualState();
        }

        private void SyncText()
        {
            DateTime? value = SelectedDate;
            string text = value.HasValue
                ? value.Value.ToString(string.IsNullOrEmpty(Format) ? "yyyy-MM-dd" : Format, CultureInfo.CurrentCulture)
                : string.Empty;

            SetValue(DisplayTextKey, text);
            SetValue(ShowPlaceholderKey, string.IsNullOrEmpty(text));
            SetValue(CanClearKey, AllowClear && value.HasValue && IsEnabled);
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
}
