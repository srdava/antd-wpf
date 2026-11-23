using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
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
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ComboBox, "DatePicker");
        }
        private Border _field;
        private Popup _popup;
        private Calendar _calendar;
        private Calendar _endCalendar;
        private Calendar _hookedStart;
        private Calendar _hookedEnd;
        private bool _syncingCalendar;

        private static readonly DependencyPropertyDescriptor CalendarDateDescriptor =
            DependencyPropertyDescriptor.FromProperty(Calendar.SelectedDateProperty, typeof(Calendar));
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

        public static readonly DependencyProperty IsRangeProperty = DependencyProperty.Register(
            "IsRange", typeof(bool), typeof(DatePicker),
            new FrameworkPropertyMetadata(false, OnRangeModeChanged));

        /// <summary>Two-month range mode: pick a start and an end instead of one date.</summary>
        public bool IsRange
        {
            get { return (bool)GetValue(IsRangeProperty); }
            set { SetValue(IsRangeProperty, value); }
        }

        public static readonly DependencyProperty RangeStartProperty = DependencyProperty.Register(
            "RangeStart", typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeValueChanged));

        public DateTime? RangeStart
        {
            get { return (DateTime?)GetValue(RangeStartProperty); }
            set { SetValue(RangeStartProperty, value); }
        }

        public static readonly DependencyProperty RangeEndProperty = DependencyProperty.Register(
            "RangeEnd", typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeValueChanged));

        public DateTime? RangeEnd
        {
            get { return (DateTime?)GetValue(RangeEndProperty); }
            set { SetValue(RangeEndProperty, value); }
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
            if (_hookedStart != null) CalendarDateDescriptor.RemoveValueChanged(_hookedStart, OnStartCalendarDate);
            if (_hookedEnd != null) CalendarDateDescriptor.RemoveValueChanged(_hookedEnd, OnEndCalendarDate);

            _field = GetTemplateChild("PART_Field") as Border;
            _popup = AntdMotion.Configure(GetTemplateChild("PART_Popup") as Popup);
            PopupAnchor.Attach(_popup, this);
            _calendar = GetTemplateChild("PART_Calendar") as Calendar;
            _endCalendar = GetTemplateChild("PART_EndCalendar") as Calendar;
            _clear = GetTemplateChild("PART_ClearButton") as Button;
            _today = GetTemplateChild("PART_Today") as Button;

            if (_today != null) _today.Content = AntdLocale.Current.Today;
            if (_endCalendar != null) _endCalendar.Visibility = IsRange ? Visibility.Visible : Visibility.Collapsed;
            if (_field != null) _field.MouseLeftButtonUp += OnFieldClick;
            if (_clear != null) _clear.Click += OnClearClick;
            if (_today != null) _today.Click += OnTodayClick;
            if (_popup != null) _popup.Closed += OnPopupClosed;

            // The calendars' SelectedDateChanged only fires for real user picks, so state changes are
            // observed on the property itself (same trick as Select's IsDropDownOpen).
            _hookedStart = _calendar;
            _hookedEnd = _endCalendar;
            if (_calendar != null) CalendarDateDescriptor.AddValueChanged(_calendar, OnStartCalendarDate);
            if (_endCalendar != null) CalendarDateDescriptor.AddValueChanged(_endCalendar, OnEndCalendarDate);

            SyncCalendars();
            SyncText();
        }

        /// <summary>Pushes the model values into the calendars without re-entering the pick handlers.</summary>
        private void SyncCalendars()
        {
            _syncingCalendar = true;
            try
            {
                if (_calendar != null) _calendar.SelectedDate = IsRange ? RangeStart : SelectedDate;
                if (_endCalendar != null) _endCalendar.SelectedDate = IsRange ? RangeEnd : null;
            }
            finally
            {
                _syncingCalendar = false;
            }
        }
        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        /// <summary>Clears the selection (both ends in range mode).</summary>
        public void Clear()
        {
            SetCurrentValue(SelectedDateProperty, null);

            if (IsRange)
            {
                SetCurrentValue(RangeStartProperty, (DateTime?)null);
                SetCurrentValue(RangeEndProperty, (DateTime?)null);
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            Clear();
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnTodayClick(object sender, RoutedEventArgs e)
        {
            if (IsRange)
            {
                SetCurrentValue(RangeStartProperty, (DateTime?)DateTime.Today);
                SetCurrentValue(RangeEndProperty, (DateTime?)DateTime.Today);
            }
            else
            {
                SetCurrentValue(SelectedDateProperty, DateTime.Today);
            }

            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private static void OnRangeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;
            if (picker._endCalendar != null)
            {
                picker._endCalendar.Visibility = picker.IsRange ? Visibility.Visible : Visibility.Collapsed;
                picker._endCalendar.SelectedDate = picker.RangeEnd;
            }

            picker.SyncText();
        }

        private static void OnRangeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DatePicker)d;

            // Setting a start beyond the current end drops the end; the pair stays ordered.
            if (picker.RangeStart.HasValue && picker.RangeEnd.HasValue && picker.RangeEnd.Value < picker.RangeStart.Value)
            {
                picker.SetCurrentValue(RangeEndProperty, (DateTime?)null);
            }

            picker.SyncCalendars();
            picker.SyncText();
        }

        private void OnStartCalendarDate(object sender, EventArgs e)
        {
            if (_syncingCalendar || _calendar == null) return;

            DateTime? picked = _calendar.SelectedDate;
            if (!picked.HasValue) return;

            DateTime value = picked.Value;

            if (!IsRange)
            {
                SetCurrentValue(SelectedDateProperty, (DateTime?)value);
                SetCurrentValue(IsDropDownOpenProperty, false);
                return;
            }

            // First click sets the start; a second, later click closes the range.
            if (RangeStart == null || RangeEnd != null || value < RangeStart.Value)
            {
                SetCurrentValue(RangeStartProperty, (DateTime?)value);
                SetCurrentValue(RangeEndProperty, (DateTime?)null);
            }
            else
            {
                SetCurrentValue(RangeEndProperty, (DateTime?)value);
                SetCurrentValue(IsDropDownOpenProperty, false);
            }

            SyncText();
        }

        private void OnEndCalendarDate(object sender, EventArgs e)
        {
            if (_syncingCalendar || _endCalendar == null) return;

            DateTime? picked = _endCalendar.SelectedDate;
            if (!picked.HasValue) return;

            DateTime value = picked.Value;

            if (RangeStart != null && value >= RangeStart.Value)
            {
                SetCurrentValue(RangeEndProperty, (DateTime?)value);
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
            else
            {
                SetCurrentValue(RangeStartProperty, (DateTime?)value);
                SetCurrentValue(RangeEndProperty, (DateTime?)null);
            }

            SyncText();
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
            picker.SyncCalendars();
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
            string pattern = string.IsNullOrEmpty(Format) ? "yyyy-MM-dd" : Format;
            string text;
            bool hasValue;

            if (IsRange)
            {
                string start = RangeStart.HasValue ? RangeStart.Value.ToString(pattern, CultureInfo.CurrentCulture) : string.Empty;
                string end = RangeEnd.HasValue ? RangeEnd.Value.ToString(pattern, CultureInfo.CurrentCulture) : string.Empty;
                text = RangeStart.HasValue || RangeEnd.HasValue ? start + " ~ " + end : string.Empty;
                hasValue = RangeStart.HasValue || RangeEnd.HasValue;
            }
            else
            {
                DateTime? value = SelectedDate;
                text = value.HasValue ? value.Value.ToString(pattern, CultureInfo.CurrentCulture) : string.Empty;
                hasValue = value.HasValue;
            }

            SetValue(DisplayTextKey, text);
            SetValue(ShowPlaceholderKey, string.IsNullOrEmpty(text));
            SetValue(CanClearKey, AllowClear && hasValue && IsEnabled);
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
