using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum AntdTagColor
    {
        Default = 0,
        Processing = 1,
        Success = 2,
        Warning = 3,
        Error = 4
    }

    /// <summary>antd Tag: a small status label, optionally closable.</summary>
    public class Tag : ContentControl
    {
        static Tag()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tag), new FrameworkPropertyMetadata(typeof(Tag)));
        }

        public Tag()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(AntdTagColor), typeof(Tag),
            new FrameworkPropertyMetadata(AntdTagColor.Default, OnAppearanceChanged));

        public AntdTagColor Color
        {
            get { return (AntdTagColor)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty = DependencyProperty.Register(
            "Closable", typeof(bool), typeof(Tag),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(Tag),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        /// <summary>Raised after the user closes the tag. The tag hides itself.</summary>
        public event EventHandler Closed;

        private static readonly DependencyPropertyKey TagBackgroundKey = DependencyProperty.RegisterReadOnly(
            "TagBackground", typeof(Brush), typeof(Tag), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TagBackgroundProperty = TagBackgroundKey.DependencyProperty;

        public Brush TagBackground
        {
            get { return (Brush)GetValue(TagBackgroundProperty); }
            private set { SetValue(TagBackgroundKey, value); }
        }

        private static readonly DependencyPropertyKey TagForegroundKey = DependencyProperty.RegisterReadOnly(
            "TagForeground", typeof(Brush), typeof(Tag), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TagForegroundProperty = TagForegroundKey.DependencyProperty;

        public Brush TagForeground
        {
            get { return (Brush)GetValue(TagForegroundProperty); }
            private set { SetValue(TagForegroundKey, value); }
        }

        private static readonly DependencyPropertyKey TagBorderBrushKey = DependencyProperty.RegisterReadOnly(
            "TagBorderBrush", typeof(Brush), typeof(Tag), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TagBorderBrushProperty = TagBorderBrushKey.DependencyProperty;

        public Brush TagBorderBrush
        {
            get { return (Brush)GetValue(TagBorderBrushProperty); }
            private set { SetValue(TagBorderBrushKey, value); }
        }

        private static readonly DependencyPropertyKey TagFontSizeKey = DependencyProperty.RegisterReadOnly(
            "TagFontSize", typeof(double), typeof(Tag), new FrameworkPropertyMetadata(12.0));

        public static readonly DependencyProperty TagFontSizeProperty = TagFontSizeKey.DependencyProperty;

        public double TagFontSize
        {
            get { return (double)GetValue(TagFontSizeProperty); }
            private set { SetValue(TagFontSizeKey, value); }
        }

        private static readonly DependencyPropertyKey TagCornerRadiusKey = DependencyProperty.RegisterReadOnly(
            "TagCornerRadius", typeof(CornerRadius), typeof(Tag), new FrameworkPropertyMetadata(new CornerRadius(4)));

        public static readonly DependencyProperty TagCornerRadiusProperty = TagCornerRadiusKey.DependencyProperty;

        public CornerRadius TagCornerRadius
        {
            get { return (CornerRadius)GetValue(TagCornerRadiusProperty); }
            private set { SetValue(TagCornerRadiusKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Tag)d).UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var closeButton = GetTemplateChild("PART_CloseButton") as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                closeButton.Click -= OnCloseClicked;
                closeButton.Click += OnCloseClicked;
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            EventHandler handler = Closed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            string background;
            string foreground;
            string border;

            switch (Color)
            {
                case AntdTagColor.Processing:
                    background = "colorPrimaryBg";
                    foreground = "colorPrimary";
                    border = "colorPrimaryBorder";
                    break;
                case AntdTagColor.Success:
                    background = "colorSuccessBg";
                    foreground = "colorSuccess";
                    border = "colorSuccessBorder";
                    break;
                case AntdTagColor.Warning:
                    background = "colorWarningBg";
                    foreground = "colorWarning";
                    border = "colorWarningBorder";
                    break;
                case AntdTagColor.Error:
                    background = "colorErrorBg";
                    foreground = "colorError";
                    border = "colorErrorBorder";
                    break;
                default:
                    background = "colorFillQuaternary";
                    foreground = "colorText";
                    border = "colorBorder";
                    break;
            }

            SetValue(TagBackgroundKey, snapshot.Brush(background));
            SetValue(TagForegroundKey, snapshot.Brush(foreground));
            SetValue(TagBorderBrushKey, snapshot.Brush(border));
            SetValue(TagFontSizeKey, snapshot.Tokens.FontSizeSM);
            SetValue(TagCornerRadiusKey, new CornerRadius(snapshot.Tokens.BorderRadiusSM));
            SetCurrentValue(BorderThicknessProperty, new Thickness(Bordered ? snapshot.Tokens.LineWidth : 0));
        }
    }
}
