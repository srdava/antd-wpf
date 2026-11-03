using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// Ant Design icon. Inherits colour through TextElement.Foreground (the same DP as
    /// Control.Foreground) so it behaves like CSS `currentColor`, and defaults to 1em sizing.
    /// </summary>
    public class Icon : Control
    {
        static Icon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
        }

        public Icon()
        {
            ApplySize();
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(Geometry), typeof(Icon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Icon geometry, usually taken from <c>Antd.Wpf.Icons.AntdIcons</c>.</summary>
        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(Icon),
            new FrameworkPropertyMetadata(double.NaN, OnSizeChanged));

        /// <summary>Square edge length. NaN means "match the inherited font size" (1em).</summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SpinProperty = DependencyProperty.Register(
            "Spin", typeof(bool), typeof(Icon),
            new FrameworkPropertyMetadata(false));

        public bool Spin
        {
            get { return (bool)GetValue(SpinProperty); }
            set { SetValue(SpinProperty, value); }
        }

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Icon)d).ApplySize();
        }

        private void ApplySize()
        {
            double size = Size;
            if (double.IsNaN(size)) size = FontSize;
            if (double.IsNaN(size) || size <= 0) size = 14;
            SetCurrentValue(WidthProperty, size);
            SetCurrentValue(HeightProperty, size);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == FontSizeProperty)
            {
                ApplySize();
            }
        }
    }
}
