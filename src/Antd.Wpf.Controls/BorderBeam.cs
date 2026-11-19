using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd BorderBeam: a light that travels around the edge of a card.
    ///
    /// WPF has no conic gradient, so the beam is a dashed stroke on a rounded rectangle whose
    /// dash offset is animated. The dash is the "comet", a blur adds the glow, and the base
    /// border keeps the shape readable while the beam is on the far side.
    /// </summary>
    public class BorderBeam : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Pane, "BorderBeam");
        }
        private double _beamOffset;

        static BorderBeam()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderBeam), new FrameworkPropertyMetadata(typeof(BorderBeam)));
        }

        public BorderBeam()
        {
            IsHitTestVisible = false;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(BorderBeam),
            new FrameworkPropertyMetadata(new CornerRadius(8), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BeamThicknessProperty = DependencyProperty.Register(
            "BeamThickness", typeof(double), typeof(BorderBeam),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double BeamThickness
        {
            get { return (double)GetValue(BeamThicknessProperty); }
            set { SetValue(BeamThicknessProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            "Duration", typeof(TimeSpan), typeof(BorderBeam),
            new FrameworkPropertyMetadata(TimeSpan.FromSeconds(3)));

        /// <summary>Time for one full trip around the border.</summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty BeamBrushProperty = DependencyProperty.Register(
            "BeamBrush", typeof(Brush), typeof(BorderBeam),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Beam colour. Null falls back to colorPrimary on the current theme.</summary>
        public Brush BeamBrush
        {
            get { return (Brush)GetValue(BeamBrushProperty); }
            set { SetValue(BeamBrushProperty, value); }
        }

        private static readonly DependencyPropertyKey BaseBrushKey = DependencyProperty.RegisterReadOnly(
            "BaseBrush", typeof(Brush), typeof(BorderBeam), new FrameworkPropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty BaseBrushProperty = BaseBrushKey.DependencyProperty;

        public Brush BaseBrush
        {
            get { return (Brush)GetValue(BaseBrushProperty); }
            private set { SetValue(BaseBrushKey, value); }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(Duration),
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };
            BeginAnimation(BeamProgressProperty, animation);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(BeamProgressProperty, null);
        }

        private static readonly DependencyProperty BeamProgressProperty = DependencyProperty.Register(
            "BeamProgress", typeof(double), typeof(BorderBeam),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((BorderBeam)d)._beamOffset = (double)e.NewValue));

        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            if (width <= 0 || height <= 0) return;

            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            BaseBrush = snapshot.Brush("colorBorderSecondary");

            double inset = BeamThickness / 2;
            var bounds = new Rect(inset, inset, Math.Max(0, width - BeamThickness), Math.Max(0, height - BeamThickness));
            double radius = EffectiveRadius(bounds);

            var geometry = new RectangleGeometry(bounds, radius, radius);
            geometry.Freeze();

            var basePen = new Pen(BaseBrush, 1);
            basePen.Freeze();
            drawingContext.DrawGeometry(null, basePen, geometry);

            Brush beam = BeamBrush ?? snapshot.Brush("colorPrimary");
            double perimeter = 2 * (bounds.Width + bounds.Height);

            // Dash values are multiples of the pen thickness, so convert the perimeter into those
            // units; the gap spans the rest of the loop, leaving one comet per trip.
            double unit = Math.Max(1, BeamThickness);
            double dash = Math.Max(12, perimeter * 0.18) / unit;
            double gap = Math.Max(1, (perimeter - dash * unit) / unit);

            var beamPen = new Pen(beam, BeamThickness)
            {
                DashStyle = new DashStyle(new[] { dash, gap }, -_beamOffset * (dash + gap)),
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            beamPen.Freeze();

            drawingContext.PushOpacity(0.95);
            drawingContext.DrawGeometry(null, beamPen, geometry);
            drawingContext.Pop();
        }

        private double EffectiveRadius(Rect bounds)
        {
            double radius = CornerRadius.TopLeft;
            return Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2);
        }
    }
}
