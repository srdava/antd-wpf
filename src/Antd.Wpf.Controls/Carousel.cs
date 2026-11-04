using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Carousel: one slide at a time with clickable dots. Auto-play uses a dispatcher timer
    /// that only runs while the control is loaded, so nothing ticks off-screen.
    /// </summary>
    public class Carousel : ItemsControl
    {
        private Panel _dots;
        private DispatcherTimer _timer;

        static Carousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Carousel), new FrameworkPropertyMetadata(typeof(Carousel)));
        }

        public Carousel()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Carousel),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(Carousel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionChanged));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        private static readonly DependencyPropertyKey SelectedItemKey = DependencyProperty.RegisterReadOnly(
            "SelectedItem", typeof(object), typeof(Carousel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = SelectedItemKey.DependencyProperty;

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemKey, value); }
        }

        public static readonly DependencyProperty AutoplayProperty = DependencyProperty.Register(
            "Autoplay", typeof(bool), typeof(Carousel), new FrameworkPropertyMetadata(false, OnAutoplayChanged));

        public bool Autoplay
        {
            get { return (bool)GetValue(AutoplayProperty); }
            set { SetValue(AutoplayProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            "Interval", typeof(int), typeof(Carousel), new FrameworkPropertyMetadata(3000, OnAutoplayChanged));

        /// <summary>Milliseconds between slides when <see cref="Autoplay"/> is on.</summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty ShowDotsProperty = DependencyProperty.Register(
            "ShowDots", typeof(bool), typeof(Carousel), new FrameworkPropertyMetadata(true));

        public bool ShowDots
        {
            get { return (bool)GetValue(ShowDotsProperty); }
            set { SetValue(ShowDotsProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _dots = GetTemplateChild("PART_Dots") as Panel;
            SyncSelection();
            RebuildDots();
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            SyncSelection();
            RebuildDots();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Carousel)d).SyncSelection();
        }

        private static void OnAutoplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (Carousel)d;
            carousel.StopTimer();
            carousel.StartTimer();
        }

        private void SyncSelection()
        {
            int count = Items.Count;
            if (count == 0)
            {
                SetValue(SelectedItemKey, null);
                return;
            }

            int index = Math.Min(count - 1, Math.Max(0, SelectedIndex));
            if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);
            SetValue(SelectedItemKey, Items[index]);
            RebuildDots();
        }

        private void RebuildDots()
        {
            if (_dots == null) return;

            _dots.Children.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                var dot = new CarouselDotButton { IsActive = i == SelectedIndex };
                int index = i;
                dot.Click += delegate { GoTo(index); };
                _dots.Children.Add(dot);
            }
        }

        private void GoTo(int index)
        {
            int count = Items.Count;
            if (count == 0) return;

            int target = ((index % count) + count) % count;
            SetCurrentValue(SelectedIndexProperty, target);
            RebuildDots();
        }

        private void StartTimer()
        {
            if (!Autoplay || !IsLoaded) return;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(200, Interval)) };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Tick -= OnTick;
            _timer = null;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Items.Count == 0) return;
            GoTo(SelectedIndex + 1);
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>
    /// One dot. It is a Control rather than a Button on purpose: Button's state machine forces
    /// a 32px minimum height, which turns a 3px dot into a block.
    /// </summary>
    public class CarouselDotButton : Control
    {
        static CarouselDotButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselDotButton),
                new FrameworkPropertyMetadata(typeof(CarouselDotButton)));
        }

        public CarouselDotButton()
        {
            Height = 3;
            MinHeight = 0;
            MinWidth = 0;
            Width = 16;
            Margin = new Thickness(4, 0, 0, 0);
            Cursor = System.Windows.Input.Cursors.Hand;
            AddHandler(PreviewMouseUpEvent,
                new System.Windows.Input.MouseButtonEventHandler(OnClick), true);
        }

        /// <summary>Raised when the dot is clicked.</summary>
        public event RoutedEventHandler Click;

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left) return;

            RoutedEventHandler handler = Click;
            if (handler != null) handler(this, new RoutedEventArgs());
            e.Handled = true;
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(CarouselDotButton), new FrameworkPropertyMetadata(false));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
    }
}
