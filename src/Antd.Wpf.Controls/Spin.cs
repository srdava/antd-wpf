using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>antd Spin: a loading indicator, optionally wrapping content.</summary>
    public class Spin : ContentControl
    {
        static Spin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spin), new FrameworkPropertyMetadata(typeof(Spin)));
        }

        public Spin()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SpinningProperty = DependencyProperty.Register(
            "Spinning", typeof(bool), typeof(Spin),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool Spinning
        {
            get { return (bool)GetValue(SpinningProperty); }
            set { SetValue(SpinningProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Spin),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty TipProperty = DependencyProperty.Register(
            "Tip", typeof(string), typeof(Spin), new FrameworkPropertyMetadata(null));

        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        private static readonly DependencyPropertyKey IndicatorBrushKey = DependencyProperty.RegisterReadOnly(
            "IndicatorBrush", typeof(Brush), typeof(Spin), new FrameworkPropertyMetadata(Brushes.Blue));

        public static readonly DependencyProperty IndicatorBrushProperty = IndicatorBrushKey.DependencyProperty;

        public Brush IndicatorBrush
        {
            get { return (Brush)GetValue(IndicatorBrushProperty); }
            private set { SetValue(IndicatorBrushKey, value); }
        }

        private static readonly DependencyPropertyKey TipBrushKey = DependencyProperty.RegisterReadOnly(
            "TipBrush", typeof(Brush), typeof(Spin), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TipBrushProperty = TipBrushKey.DependencyProperty;

        public Brush TipBrush
        {
            get { return (Brush)GetValue(TipBrushProperty); }
            private set { SetValue(TipBrushKey, value); }
        }

        private static readonly DependencyPropertyKey IndicatorSizeKey = DependencyProperty.RegisterReadOnly(
            "IndicatorSize", typeof(double), typeof(Spin), new FrameworkPropertyMetadata(20.0));

        public static readonly DependencyProperty IndicatorSizeProperty = IndicatorSizeKey.DependencyProperty;

        public double IndicatorSize
        {
            get { return (double)GetValue(IndicatorSizeProperty); }
            private set { SetValue(IndicatorSizeKey, value); }
        }

        private static readonly DependencyPropertyKey ShowsTipKey = DependencyProperty.RegisterReadOnly(
            "ShowsTip", typeof(bool), typeof(Spin), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShowsTipProperty = ShowsTipKey.DependencyProperty;

        public bool ShowsTip
        {
            get { return (bool)GetValue(ShowsTipProperty); }
            private set { SetValue(ShowsTipKey, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateVisualState();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Spin)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            double size = 20;
            if (Size == AntdSize.Small) size = 14;
            else if (Size == AntdSize.Large) size = 32;

            SetValue(IndicatorBrushKey, snapshot.Brush("colorPrimary"));
            SetValue(TipBrushKey, snapshot.Brush("colorPrimary"));
            SetValue(IndicatorSizeKey, size);
            SetValue(ShowsTipKey, Content != null && !string.IsNullOrEmpty(Tip));
        }
    }
}
