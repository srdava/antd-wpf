using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>antd Badge: a count or dot attached to the top-right of its content.</summary>
    [ContentProperty("Content")]
    public class Badge : Control
    {
        static Badge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
        }

        public Badge()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(Badge), new FrameworkPropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof(int), typeof(Badge),
            new FrameworkPropertyMetadata(0, OnAppearanceChanged));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty DotProperty = DependencyProperty.Register(
            "Dot", typeof(bool), typeof(Badge),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Dot
        {
            get { return (bool)GetValue(DotProperty); }
            set { SetValue(DotProperty, value); }
        }

        public static readonly DependencyProperty ShowZeroProperty = DependencyProperty.Register(
            "ShowZero", typeof(bool), typeof(Badge),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool ShowZero
        {
            get { return (bool)GetValue(ShowZeroProperty); }
            set { SetValue(ShowZeroProperty, value); }
        }

        public static readonly DependencyProperty OverflowCountProperty = DependencyProperty.Register(
            "OverflowCount", typeof(int), typeof(Badge),
            new FrameworkPropertyMetadata(99, OnAppearanceChanged));

        public int OverflowCount
        {
            get { return (int)GetValue(OverflowCountProperty); }
            set { SetValue(OverflowCountProperty, value); }
        }

        public static readonly DependencyProperty CountColorProperty = DependencyProperty.Register(
            "CountColor", typeof(Brush), typeof(Badge), new FrameworkPropertyMetadata(null));

        /// <summary>Overrides the badge background; null means colorError.</summary>
        public Brush CountColor
        {
            get { return (Brush)GetValue(CountColorProperty); }
            set { SetValue(CountColorProperty, value); }
        }

        private static readonly DependencyPropertyKey DisplayTextKey = DependencyProperty.RegisterReadOnly(
            "DisplayText", typeof(string), typeof(Badge), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            private set { SetValue(DisplayTextKey, value); }
        }

        private static readonly DependencyPropertyKey IsBadgeVisibleKey = DependencyProperty.RegisterReadOnly(
            "IsBadgeVisible", typeof(bool), typeof(Badge), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsBadgeVisibleProperty = IsBadgeVisibleKey.DependencyProperty;

        public bool IsBadgeVisible
        {
            get { return (bool)GetValue(IsBadgeVisibleProperty); }
            private set { SetValue(IsBadgeVisibleKey, value); }
        }

        private static readonly DependencyPropertyKey BadgeBackgroundKey = DependencyProperty.RegisterReadOnly(
            "BadgeBackground", typeof(Brush), typeof(Badge), new FrameworkPropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty BadgeBackgroundProperty = BadgeBackgroundKey.DependencyProperty;

        public Brush BadgeBackground
        {
            get { return (Brush)GetValue(BadgeBackgroundProperty); }
            private set { SetValue(BadgeBackgroundKey, value); }
        }

        private static readonly DependencyPropertyKey BadgeFontSizeKey = DependencyProperty.RegisterReadOnly(
            "BadgeFontSize", typeof(double), typeof(Badge), new FrameworkPropertyMetadata(12.0));

        public static readonly DependencyProperty BadgeFontSizeProperty = BadgeFontSizeKey.DependencyProperty;

        public double BadgeFontSize
        {
            get { return (double)GetValue(BadgeFontSizeProperty); }
            private set { SetValue(BadgeFontSizeKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            bool visible = Dot || (Count > 0 || (ShowZero && Count == 0));
            SetValue(IsBadgeVisibleKey, visible);

            string text = Count > OverflowCount
                ? OverflowCount.ToString(CultureInfo.InvariantCulture) + "+"
                : Count.ToString(CultureInfo.InvariantCulture);
            SetValue(DisplayTextKey, Dot ? string.Empty : text);

            SetValue(BadgeBackgroundKey, CountColor ?? snapshot.Brush("colorError"));
            SetValue(BadgeFontSizeKey, snapshot.Tokens.FontSizeSM);
        }
    }
}
