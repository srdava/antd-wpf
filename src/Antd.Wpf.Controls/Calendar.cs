using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Tokens;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>One cell of the month grid handed to the item template.</summary>
    public sealed class CalendarDay
    {
        public CalendarDay(DateTime date, bool isOutsideMonth, bool isToday)
        {
            Date = date;
            Day = date.Day.ToString(CultureInfo.CurrentCulture);
            IsOutsideMonth = isOutsideMonth;
            IsToday = isToday;
            Key = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public DateTime Date { get; private set; }
        public string Day { get; private set; }
        public bool IsOutsideMonth { get; private set; }
        public bool IsToday { get; private set; }

        /// <summary>Stable identity so re-rendering a month keeps the selection.</summary>
        public string Key { get; private set; }

        public override string ToString()
        {
            return Key;
        }
    }

    /// <summary>
    /// antd Calendar: a month grid with prev/next navigation and single date selection.
    /// The grid is a ListBox over 42 <see cref="CalendarDay"/> items, so hover, selection and
    /// keyboard navigation come from the framework instead of a hand written hit tester.
    /// </summary>
    public class Calendar : Control
    {
        private ListBox _grid;
        private Button _prev;
        private Button _next;
        private bool _syncing;

        static Calendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(typeof(Calendar)));
        }

        public Calendar()
        {
            DisplayDate = DateTime.Today;
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty DisplayDateProperty = DependencyProperty.Register(
            "DisplayDate", typeof(DateTime), typeof(Calendar),
            new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.None, OnMonthChanged));

        /// <summary>Any date inside the month that is shown.</summary>
        public DateTime DisplayDate
        {
            get { return (DateTime)GetValue(DisplayDateProperty); }
            set { SetValue(DisplayDateProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            "SelectedDate", typeof(DateTime?), typeof(Calendar),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedChanged));

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        /// <summary>Raised after the user picks a day (not when set from code).</summary>
        public event EventHandler<DateTime> SelectedDateChanged;

        private static readonly DependencyPropertyKey DaysKey = DependencyProperty.RegisterReadOnly(
            "Days", typeof(ObservableCollection<CalendarDay>), typeof(Calendar),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DaysProperty = DaysKey.DependencyProperty;

        public ObservableCollection<CalendarDay> Days
        {
            get { return (ObservableCollection<CalendarDay>)GetValue(DaysProperty); }
            private set { SetValue(DaysKey, value); }
        }

        private static readonly DependencyPropertyKey WeekDaysKey = DependencyProperty.RegisterReadOnly(
            "WeekDays", typeof(string[]), typeof(Calendar), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty WeekDaysProperty = WeekDaysKey.DependencyProperty;

        public string[] WeekDays
        {
            get { return (string[])GetValue(WeekDaysProperty); }
            private set { SetValue(WeekDaysKey, value); }
        }

        private static readonly DependencyPropertyKey MonthLabelKey = DependencyProperty.RegisterReadOnly(
            "MonthLabel", typeof(string), typeof(Calendar), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty MonthLabelProperty = MonthLabelKey.DependencyProperty;

        public string MonthLabel
        {
            get { return (string)GetValue(MonthLabelProperty); }
            private set { SetValue(MonthLabelKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_prev != null) _prev.Click -= OnPreviousMonth;
            if (_next != null) _next.Click -= OnNextMonth;
            if (_grid != null) _grid.SelectionChanged -= OnGridSelectionChanged;

            _prev = GetTemplateChild("PART_PreviousMonth") as Button;
            _next = GetTemplateChild("PART_NextMonth") as Button;
            _grid = GetTemplateChild("PART_DayGrid") as ListBox;

            if (_prev != null) _prev.Click += OnPreviousMonth;
            if (_next != null) _next.Click += OnNextMonth;
            if (_grid != null) _grid.SelectionChanged += OnGridSelectionChanged;

            Rebuild();
        }

        private static void OnMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).Rebuild();
        }

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).SyncSelection();
        }

        private void OnPreviousMonth(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(DisplayDateProperty, DisplayDate.AddMonths(-1));
        }

        private void OnNextMonth(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(DisplayDateProperty, DisplayDate.AddMonths(1));
        }

        private void OnGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_syncing || _grid == null) return;

            var day = _grid.SelectedItem as CalendarDay;
            if (day == null) return;

            SetCurrentValue(SelectedDateProperty, day.Date);
            EventHandler<DateTime> handler = SelectedDateChanged;
            if (handler != null) handler(this, day.Date);
        }

        private void Rebuild()
        {
            DateTime month = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
            SetValue(MonthLabelKey, month.ToString("Y", CultureInfo.CurrentCulture));
            SetValue(WeekDaysKey, BuildWeekDays());

            var cells = new List<CalendarDay>(42);
            int offset = (int)month.DayOfWeek; // weeks start on the culture's first day, Sunday by default
            DateTime cursor = month.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                cells.Add(new CalendarDay(cursor, cursor.Month != month.Month, cursor.Date == DateTime.Today));
                cursor = cursor.AddDays(1);
            }

            Days = new ObservableCollection<CalendarDay>(cells);
            SyncSelection();
        }

        private static string[] BuildWeekDays()
        {
            var names = new string[7];
            DateTime start = new DateTime(2024, 1, 7); // a Sunday
            for (int i = 0; i < 7; i++)
            {
                names[i] = start.AddDays(i).ToString("ddd", CultureInfo.CurrentCulture);
            }
            return names;
        }

        private void SyncSelection()
        {
            if (_grid == null || Days == null) return;

            _syncing = true;
            try
            {
                DateTime? selected = SelectedDate;
                _grid.SelectedItem = null;

                if (selected.HasValue)
                {
                    foreach (CalendarDay day in Days)
                    {
                        if (day.Date.Date == selected.Value.Date)
                        {
                            _grid.SelectedItem = day;
                            break;
                        }
                    }
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, new Thickness(t.PaddingSM));
        }
    }
}
