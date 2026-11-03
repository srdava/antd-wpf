using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum DividerOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>antd Divider: a rule between or around content.</summary>
    public class Divider : ContentControl
    {
        static Divider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Divider), new FrameworkPropertyMetadata(typeof(Divider)));
        }

        public Divider()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(DividerOrientation), typeof(Divider),
            new FrameworkPropertyMetadata(DividerOrientation.Horizontal, OnAppearanceChanged));

        public DividerOrientation Orientation
        {
            get { return (DividerOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty DashedProperty = DependencyProperty.Register(
            "Dashed", typeof(bool), typeof(Divider), new FrameworkPropertyMetadata(false));

        public bool Dashed
        {
            get { return (bool)GetValue(DashedProperty); }
            set { SetValue(DashedProperty, value); }
        }

        public static readonly DependencyProperty PlainTextProperty = DependencyProperty.Register(
            "PlainText", typeof(bool), typeof(Divider),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        /// <summary>antd plain text divider: normal weight, secondary colour.</summary>
        public bool PlainText
        {
            get { return (bool)GetValue(PlainTextProperty); }
            set { SetValue(PlainTextProperty, value); }
        }

        /// <summary>Convenience string content; mirrors antd's `children` for the common case.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Divider),
            new FrameworkPropertyMetadata(null, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Divider)d).Content = e.NewValue;
        }

        private static readonly DependencyPropertyKey LineBrushKey = DependencyProperty.RegisterReadOnly(
            "LineBrush", typeof(Brush), typeof(Divider), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty LineBrushProperty = LineBrushKey.DependencyProperty;

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            private set { SetValue(LineBrushKey, value); }
        }

        private static readonly DependencyPropertyKey TextBrushKey = DependencyProperty.RegisterReadOnly(
            "TextBrush", typeof(Brush), typeof(Divider), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TextBrushProperty = TextBrushKey.DependencyProperty;

        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            private set { SetValue(TextBrushKey, value); }
        }

        private static readonly DependencyPropertyKey TextFontWeightKey = DependencyProperty.RegisterReadOnly(
            "TextFontWeight", typeof(FontWeight), typeof(Divider), new FrameworkPropertyMetadata(FontWeights.Normal));

        public static readonly DependencyProperty TextFontWeightProperty = TextFontWeightKey.DependencyProperty;

        public FontWeight TextFontWeight
        {
            get { return (FontWeight)GetValue(TextFontWeightProperty); }
            private set { SetValue(TextFontWeightKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Divider)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            SetValue(LineBrushKey, snapshot.Brush("colorSplit"));
            SetValue(TextBrushKey, snapshot.Brush(PlainText ? "colorText" : "colorTextHeading"));
            SetValue(TextFontWeightKey, PlainText ? FontWeights.Normal : FontWeights.SemiBold);
        }
    }
}
