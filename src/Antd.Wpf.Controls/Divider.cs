using System.Windows;
using System.Windows.Automation.Peers;
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
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Separator, "Divider");
        }
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

        private static readonly DependencyPropertyKey TextLineHeightKey = DependencyProperty.RegisterReadOnly(
            "TextLineHeight", typeof(double), typeof(Divider), new FrameworkPropertyMetadata(22.0));

        public static readonly DependencyProperty TextLineHeightProperty = TextLineHeightKey.DependencyProperty;

        /// <summary>antd renders divider text at lineHeight (22/14) scaled to the heading size.</summary>
        public double TextLineHeight
        {
            get { return (double)GetValue(TextLineHeightProperty); }
            private set { SetValue(TextLineHeightKey, value); }
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
            // antd: .ant-divider-inner-text uses fontSizeLG (16) and weight 500; plain text
            // drops the heading colour but keeps the larger size.
            FontWeight weight = PlainText ? FontWeights.Normal : FontWeights.Medium;
            double fontSize = PlainText ? snapshot.Tokens.FontSize : snapshot.Tokens.FontSizeLG;

            SetValue(TextFontWeightKey, weight);
            SetCurrentValue(FontWeightProperty, weight);
            SetCurrentValue(FontSizeProperty, fontSize);
            SetValue(TextLineHeightKey, snapshot.Tokens.FontHeight * (fontSize / snapshot.Tokens.FontSize));
        }
    }
}
