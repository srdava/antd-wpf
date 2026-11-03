using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Slider. Derives from WPF Slider so value coercion, keyboard steps and range logic
    /// stay native; the template swaps in antd's track and thumb through a Track element.
    /// </summary>
    public class Slider : System.Windows.Controls.Slider
    {
        static Slider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(typeof(Slider)));
        }

        public Slider()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Slider),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static readonly DependencyPropertyKey TrackThicknessKey = DependencyProperty.RegisterReadOnly(
            "TrackThickness", typeof(double), typeof(Slider), new FrameworkPropertyMetadata(4.0));

        public static readonly DependencyProperty TrackThicknessProperty = TrackThicknessKey.DependencyProperty;

        public double TrackThickness
        {
            get { return (double)GetValue(TrackThicknessProperty); }
            private set { SetValue(TrackThicknessKey, value); }
        }

        private static readonly DependencyPropertyKey ThumbSizeKey = DependencyProperty.RegisterReadOnly(
            "ThumbSize", typeof(double), typeof(Slider), new FrameworkPropertyMetadata(14.0));

        public static readonly DependencyProperty ThumbSizeProperty = ThumbSizeKey.DependencyProperty;

        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            private set { SetValue(ThumbSizeKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Slider)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetValue(TrackThicknessKey, Size == AntdSize.Small ? 2.0 : 4.0);
            SetValue(ThumbSizeKey, Size == AntdSize.Small ? 10.0 : 14.0);
            SetCurrentValue(MinHeightProperty, ThumbSize + t.SizeXS);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateVisualState();
        }
    }
}
