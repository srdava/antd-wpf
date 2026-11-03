using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>antd Statistic: a labelled number with optional prefix and suffix.</summary>
    public class Statistic : Control
    {
        static Statistic()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Statistic), new FrameworkPropertyMetadata(typeof(Statistic)));
        }

        public Statistic()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Statistic), new FrameworkPropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(Statistic),
            new FrameworkPropertyMetadata(0.0, OnAppearanceChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(
            "Precision", typeof(int), typeof(Statistic),
            new FrameworkPropertyMetadata(-1, OnAppearanceChanged));

        /// <summary>Decimal places. -1 keeps the natural representation.</summary>
        public int Precision
        {
            get { return (int)GetValue(PrecisionProperty); }
            set { SetValue(PrecisionProperty, value); }
        }

        public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(
            "Prefix", typeof(object), typeof(Statistic), new FrameworkPropertyMetadata(null));

        public object Prefix
        {
            get { return GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        public static readonly DependencyProperty SuffixProperty = DependencyProperty.Register(
            "Suffix", typeof(object), typeof(Statistic), new FrameworkPropertyMetadata(null));

        public object Suffix
        {
            get { return GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        private static readonly DependencyPropertyKey ValueTextKey = DependencyProperty.RegisterReadOnly(
            "ValueText", typeof(string), typeof(Statistic), new FrameworkPropertyMetadata("0"));

        public static readonly DependencyProperty ValueTextProperty = ValueTextKey.DependencyProperty;

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            private set { SetValue(ValueTextKey, value); }
        }

        private static readonly DependencyPropertyKey TitleBrushKey = DependencyProperty.RegisterReadOnly(
            "TitleBrush", typeof(Brush), typeof(Statistic), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TitleBrushProperty = TitleBrushKey.DependencyProperty;

        public Brush TitleBrush
        {
            get { return (Brush)GetValue(TitleBrushProperty); }
            private set { SetValue(TitleBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ValueBrushKey = DependencyProperty.RegisterReadOnly(
            "ValueBrush", typeof(Brush), typeof(Statistic), new FrameworkPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ValueBrushProperty = ValueBrushKey.DependencyProperty;

        public Brush ValueBrush
        {
            get { return (Brush)GetValue(ValueBrushProperty); }
            private set { SetValue(ValueBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ValueFontSizeKey = DependencyProperty.RegisterReadOnly(
            "ValueFontSize", typeof(double), typeof(Statistic), new FrameworkPropertyMetadata(24.0));

        public static readonly DependencyProperty ValueFontSizeProperty = ValueFontSizeKey.DependencyProperty;

        public double ValueFontSize
        {
            get { return (double)GetValue(ValueFontSizeProperty); }
            private set { SetValue(ValueFontSizeKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Statistic)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            string text = Precision >= 0
                ? Value.ToString("N" + Precision.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture)
                : Value.ToString("N0", CultureInfo.CurrentCulture);

            SetValue(ValueTextKey, text);
            SetValue(TitleBrushKey, snapshot.Brush("colorTextSecondary"));
            SetValue(ValueBrushKey, snapshot.Brush("colorTextHeading"));
            SetValue(ValueFontSizeKey, snapshot.Tokens.FontSizeHeading3);
        }
    }
}
