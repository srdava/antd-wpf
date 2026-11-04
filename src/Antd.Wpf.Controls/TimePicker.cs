using System;
using System.Collections.Generic;
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
    /// antd TimePicker: a field that drops three scrollable columns (hour / minute / second).
    /// Scroll snapping is left to the ListBox: selecting a row is enough because antd's picker
    /// also commits on click, not on scroll stop.
    /// </summary>
    public class TimePicker : Control
    {
        private Border _field;
        private Popup _popup;
        private ListBox _hours;
        private ListBox _minutes;
        private ListBox _seconds;
        private Button _clear;
        private Button _now;
        private bool _syncing;

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        public TimePicker()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
            "SelectedTime", typeof(TimeSpan?), typeof(TimePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

        public TimeSpan? SelectedTime
        {
            get { return (TimeSpan?)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            "Format", typeof(string), typeof(TimePicker),
            new FrameworkPropertyMetadata("HH:mm:ss", OnFormatChanged));

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register(
            "ShowSeconds", typeof(bool), typeof(TimePicker),
            new FrameworkPropertyMetadata(true, OnFormatChanged));

        /// <summary>Hides the seconds column and the ":ss" part of the text.</summary>
        public bool ShowSeconds
        {
            get { return (bool)GetValue(ShowSecondsProperty); }
            set { SetValue(ShowSecondsProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(TimePicker),
            new FrameworkPropertyMetadata("Select time", OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(TimePicker),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(TimePicker),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(TimePicker),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey DisplayTextKey = DependencyProperty.RegisterReadOnly(
            "DisplayText", typeof(string), typeof(TimePicker), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            private set { SetValue(DisplayTextKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(TimePicker), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(TimePicker), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearProperty = CanClearKey.DependencyProperty;

        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            private set { SetValue(CanClearKey, value); }
        }

        private static readonly DependencyPropertyKey HoursKey = DependencyProperty.RegisterReadOnly(
            "Hours", typeof(IEnumerable<int>), typeof(TimePicker), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HoursProperty = HoursKey.DependencyProperty;

        public IEnumerable<int> Hours
        {
            get { return (IEnumerable<int>)GetValue(HoursProperty); }
            private set { SetValue(HoursKey, value); }
        }

        private static readonly DependencyPropertyKey SixtyKey = DependencyProperty.RegisterReadOnly(
            "Sixty", typeof(IEnumerable<int>), typeof(TimePicker), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SixtyProperty = SixtyKey.DependencyProperty;

        /// <summary>0..59, used by both the minute and second columns.</summary>
        public IEnumerable<int> Sixty
        {
            get { return (IEnumerable<int>)GetValue(SixtyProperty); }
            private set { SetValue(SixtyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DetachHandlers();

            _field = GetTemplateChild("PART_Field") as Border;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _hours = GetTemplateChild("PART_Hours") as ListBox;
            _minutes = GetTemplateChild("PART_Minutes") as ListBox;
            _seconds = GetTemplateChild("PART_Seconds") as ListBox;
            _clear = GetTemplateChild("PART_ClearButton") as Button;
            _now = GetTemplateChild("PART_Now") as Button;

            if (_field != null) _field.MouseLeftButtonUp += OnFieldClick;
            if (_clear != null) _clear.Click += OnClearClick;
            if (_now != null) _now.Click += OnNowClick;
            if (_popup != null) _popup.Closed += OnPopupClosed;

            if (_hours != null) _hours.SelectionChanged += OnColumnChanged;
            if (_minutes != null) _minutes.SelectionChanged += OnColumnChanged;
            if (_seconds != null) _seconds.SelectionChanged += OnColumnChanged;

            Hours = BuildRange(24);
            Sixty = BuildRange(60);

            SyncFromValue();
        }

        private void DetachHandlers()
        {
            if (_field != null) _field.MouseLeftButtonUp -= OnFieldClick;
            if (_clear != null) _clear.Click -= OnClearClick;
            if (_now != null) _now.Click -= OnNowClick;
            if (_popup != null) _popup.Closed -= OnPopupClosed;
            if (_hours != null) _hours.SelectionChanged -= OnColumnChanged;
            if (_minutes != null) _minutes.SelectionChanged -= OnColumnChanged;
            if (_seconds != null) _seconds.SelectionChanged -= OnColumnChanged;
        }

        private static IEnumerable<int> BuildRange(int count)
        {
            var values = new int[count];
            for (int i = 0; i < count; i++) values[i] = i;
            return values;
        }

        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedTimeProperty, null);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnNowClick(object sender, RoutedEventArgs e)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            SetCurrentValue(SelectedTimeProperty, new TimeSpan(now.Hours, now.Minutes, now.Seconds));
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnColumnChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_syncing) return;
            if (_hours == null || _minutes == null) return;

            int hour = _hours.SelectedIndex < 0 ? 0 : _hours.SelectedIndex;
            int minute = _minutes.SelectedIndex < 0 ? 0 : _minutes.SelectedIndex;
            int second = _seconds == null || _seconds.SelectedIndex < 0 ? 0 : _seconds.SelectedIndex;

            SetCurrentValue(SelectedTimeProperty, new TimeSpan(hour, minute, second));
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (TimePicker)d;
            if (picker._popup != null) picker._popup.IsOpen = picker.IsDropDownOpen;
            picker.UpdateVisualState();
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (TimePicker)d;
            picker.SyncFromValue();
            picker.UpdateVisualState();
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimePicker)d).SyncText();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (TimePicker)d;
            picker.SyncText();
            picker.UpdateVisualState();
        }

        private void SyncFromValue()
        {
            if (_hours == null || _minutes == null) return;

            _syncing = true;
            try
            {
                TimeSpan value = SelectedTime ?? TimeSpan.Zero;
                bool has = SelectedTime.HasValue;

                _hours.SelectedIndex = has ? value.Hours : -1;
                _minutes.SelectedIndex = has ? value.Minutes : -1;
                if (_seconds != null) _seconds.SelectedIndex = has ? value.Seconds : -1;
            }
            finally
            {
                _syncing = false;
            }

            SyncText();
            ScrollColumnsIntoView();
        }

        private void ScrollColumnsIntoView()
        {
            if (_hours != null && _hours.SelectedItem != null) _hours.ScrollIntoView(_hours.SelectedItem);
            if (_minutes != null && _minutes.SelectedItem != null) _minutes.ScrollIntoView(_minutes.SelectedItem);
            if (_seconds != null && _seconds.SelectedItem != null) _seconds.ScrollIntoView(_seconds.SelectedItem);
        }

        private void SyncText()
        {
            TimeSpan? value = SelectedTime;
            string format = string.IsNullOrEmpty(Format) ? "HH:mm:ss" : Format;
            if (!ShowSeconds && format == "HH:mm:ss") format = "HH:mm";

            string text = value.HasValue
                ? DateTime.Today.Add(value.Value).ToString(format, CultureInfo.CurrentCulture)
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
