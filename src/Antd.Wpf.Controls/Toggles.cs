using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>antd Checkbox. Built on ToggleButton so IsChecked, three-state and keyboard work.</summary>
    public class Checkbox : ToggleButton
    {
        static Checkbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Checkbox), new FrameworkPropertyMetadata(typeof(Checkbox)));
        }

        public Checkbox()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Checkbox),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static readonly DependencyPropertyKey BoxBrushKey = DependencyProperty.RegisterReadOnly(
            "BoxBrush", typeof(Brush), typeof(Checkbox), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty BoxBrushProperty = BoxBrushKey.DependencyProperty;

        public Brush BoxBrush
        {
            get { return (Brush)GetValue(BoxBrushProperty); }
            private set { SetValue(BoxBrushKey, value); }
        }

        private static readonly DependencyPropertyKey BoxBorderBrushKey = DependencyProperty.RegisterReadOnly(
            "BoxBorderBrush", typeof(Brush), typeof(Checkbox), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty BoxBorderBrushProperty = BoxBorderBrushKey.DependencyProperty;

        public Brush BoxBorderBrush
        {
            get { return (Brush)GetValue(BoxBorderBrushProperty); }
            private set { SetValue(BoxBorderBrushKey, value); }
        }

        private static readonly DependencyPropertyKey CheckBrushKey = DependencyProperty.RegisterReadOnly(
            "CheckBrush", typeof(Brush), typeof(Checkbox), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty CheckBrushProperty = CheckBrushKey.DependencyProperty;

        public Brush CheckBrush
        {
            get { return (Brush)GetValue(CheckBrushProperty); }
            private set { SetValue(CheckBrushKey, value); }
        }

        private static readonly DependencyPropertyKey BoxSizeKey = DependencyProperty.RegisterReadOnly(
            "BoxSize", typeof(double), typeof(Checkbox), new FrameworkPropertyMetadata(16.0));

        public static readonly DependencyProperty BoxSizeProperty = BoxSizeKey.DependencyProperty;

        public double BoxSize
        {
            get { return (double)GetValue(BoxSizeProperty); }
            private set { SetValue(BoxSizeKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Checkbox)d).UpdateVisualState();
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            UpdateVisualState();
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            UpdateVisualState();
        }

        protected override void OnIndeterminate(RoutedEventArgs e)
        {
            base.OnIndeterminate(e);
            UpdateVisualState();
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


        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool active = IsChecked == true || IsChecked == null;
            bool hover = IsMouseOver && IsEnabled;

            string boxKey;
            string borderKey;
            if (!IsEnabled)
            {
                boxKey = active ? "colorBgContainerDisabled" : "colorBgContainer";
                borderKey = "colorBorderDisabled";
            }
            else if (active)
            {
                boxKey = hover ? "colorPrimaryHover" : "colorPrimary";
                borderKey = boxKey;
            }
            else
            {
                boxKey = "colorBgContainer";
                borderKey = hover ? "colorPrimaryHover" : "colorBorder";
            }

            SetValue(BoxBrushKey, snapshot.Brush(boxKey));
            SetValue(BoxBorderBrushKey, snapshot.Brush(borderKey));
            SetValue(CheckBrushKey, snapshot.Brush(IsEnabled ? "colorTextLightSolid" : "colorTextDisabled"));
            SetValue(BoxSizeKey, Size == AntdSize.Small ? 14.0 : t.ControlInteractiveSize);
            SetCurrentValue(FontSizeProperty, Metrics.FontSize(t, Size));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
        }
    }

    /// <summary>antd Radio. RadioButton already provides GroupName based exclusive selection.</summary>
    public class Radio : System.Windows.Controls.RadioButton
    {
        static Radio()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Radio), new FrameworkPropertyMetadata(typeof(Radio)));
        }

        public Radio()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        private static readonly DependencyPropertyKey RingBrushKey = DependencyProperty.RegisterReadOnly(
            "RingBrush", typeof(Brush), typeof(Radio), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty RingBrushProperty = RingBrushKey.DependencyProperty;

        public Brush RingBrush
        {
            get { return (Brush)GetValue(RingBrushProperty); }
            private set { SetValue(RingBrushKey, value); }
        }

        private static readonly DependencyPropertyKey DotBrushKey = DependencyProperty.RegisterReadOnly(
            "DotBrush", typeof(Brush), typeof(Radio), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty DotBrushProperty = DotBrushKey.DependencyProperty;

        public Brush DotBrush
        {
            get { return (Brush)GetValue(DotBrushProperty); }
            private set { SetValue(DotBrushKey, value); }
        }

        private static readonly DependencyPropertyKey RingSizeKey = DependencyProperty.RegisterReadOnly(
            "RingSize", typeof(double), typeof(Radio), new FrameworkPropertyMetadata(16.0));

        public static readonly DependencyProperty RingSizeProperty = RingSizeKey.DependencyProperty;

        public double RingSize
        {
            get { return (double)GetValue(RingSizeProperty); }
            private set { SetValue(RingSizeKey, value); }
        }


        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            UpdateVisualState();
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            UpdateVisualState();
        }

        protected override void OnIndeterminate(RoutedEventArgs e)
        {
            base.OnIndeterminate(e);
            UpdateVisualState();
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

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool selected = IsChecked == true;
            bool hover = IsMouseOver && IsEnabled;

            string ringKey;
            if (!IsEnabled) ringKey = "colorBorderDisabled";
            else if (selected) ringKey = hover ? "colorPrimaryHover" : "colorPrimary";
            else ringKey = hover ? "colorPrimaryHover" : "colorBorder";

            SetValue(RingBrushKey, snapshot.Brush(ringKey));
            SetValue(DotBrushKey, snapshot.Brush(IsEnabled ? "colorPrimary" : "colorTextDisabled"));
            SetValue(RingSizeKey, t.ControlInteractiveSize);
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
        }
    }

    /// <summary>antd Switch.</summary>
    public class Switch : ToggleButton
    {
        static Switch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Switch), new FrameworkPropertyMetadata(typeof(Switch)));
        }

        public Switch()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Switch),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.Register(
            "CheckedText", typeof(string), typeof(Switch), new FrameworkPropertyMetadata(null));

        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }

        public static readonly DependencyProperty UncheckedTextProperty = DependencyProperty.Register(
            "UncheckedText", typeof(string), typeof(Switch), new FrameworkPropertyMetadata(null));

        public string UncheckedText
        {
            get { return (string)GetValue(UncheckedTextProperty); }
            set { SetValue(UncheckedTextProperty, value); }
        }

        private static readonly DependencyPropertyKey TrackBrushKey = DependencyProperty.RegisterReadOnly(
            "TrackBrush", typeof(Brush), typeof(Switch), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TrackBrushProperty = TrackBrushKey.DependencyProperty;

        public Brush TrackBrush
        {
            get { return (Brush)GetValue(TrackBrushProperty); }
            private set { SetValue(TrackBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ThumbBrushKey = DependencyProperty.RegisterReadOnly(
            "ThumbBrush", typeof(Brush), typeof(Switch), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty ThumbBrushProperty = ThumbBrushKey.DependencyProperty;

        public Brush ThumbBrush
        {
            get { return (Brush)GetValue(ThumbBrushProperty); }
            private set { SetValue(ThumbBrushKey, value); }
        }

        private static readonly DependencyPropertyKey TrackWidthKey = DependencyProperty.RegisterReadOnly(
            "TrackWidth", typeof(double), typeof(Switch), new FrameworkPropertyMetadata(44.0));

        public static readonly DependencyProperty TrackWidthProperty = TrackWidthKey.DependencyProperty;

        public double TrackWidth
        {
            get { return (double)GetValue(TrackWidthProperty); }
            private set { SetValue(TrackWidthKey, value); }
        }

        private static readonly DependencyPropertyKey TrackHeightKey = DependencyProperty.RegisterReadOnly(
            "TrackHeight", typeof(double), typeof(Switch), new FrameworkPropertyMetadata(22.0));

        public static readonly DependencyProperty TrackHeightProperty = TrackHeightKey.DependencyProperty;

        public double TrackHeight
        {
            get { return (double)GetValue(TrackHeightProperty); }
            private set { SetValue(TrackHeightKey, value); }
        }

        private static readonly DependencyPropertyKey ThumbSizeKey = DependencyProperty.RegisterReadOnly(
            "ThumbSize", typeof(double), typeof(Switch), new FrameworkPropertyMetadata(18.0));

        public static readonly DependencyProperty ThumbSizeProperty = ThumbSizeKey.DependencyProperty;

        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            private set { SetValue(ThumbSizeKey, value); }
        }

        private static readonly DependencyPropertyKey ActiveTextKey = DependencyProperty.RegisterReadOnly(
            "ActiveText", typeof(string), typeof(Switch), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ActiveTextProperty = ActiveTextKey.DependencyProperty;

        public string ActiveText
        {
            get { return (string)GetValue(ActiveTextProperty); }
            private set { SetValue(ActiveTextKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Switch)d).UpdateVisualState();
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            UpdateVisualState();
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            UpdateVisualState();
        }

        protected override void OnIndeterminate(RoutedEventArgs e)
        {
            base.OnIndeterminate(e);
            UpdateVisualState();
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


        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            bool on = IsChecked == true;
            bool small = Size == AntdSize.Small;

            string trackKey;
            if (!IsEnabled) trackKey = "colorBgContainerDisabled";
            else if (on) trackKey = IsMouseOver ? "colorPrimaryHover" : "colorPrimary";
            else trackKey = IsMouseOver ? "colorTextTertiary" : "colorTextQuaternary";

            SetValue(TrackBrushKey, snapshot.Brush(trackKey));
            SetValue(ThumbBrushKey, Brushes.White);
            SetValue(TrackWidthKey, small ? 28.0 : 44.0);
            SetValue(TrackHeightKey, small ? 16.0 : 22.0);
            SetValue(ThumbSizeKey, small ? 12.0 : 18.0);
            SetValue(ActiveTextKey, on ? CheckedText : UncheckedText);
            SetCurrentValue(FontSizeProperty, snapshot.Tokens.FontSizeSM);
        }
    }
}
