using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Skeleton: grey placeholder blocks. Drawn directly so the row count and the
    /// avatar/title toggles do not need one template branch per combination.
    /// </summary>
    public class Skeleton : Control
    {
        private const double BarHeight = 16;
        private const double Gap = 16;
        private static readonly double AvatarSize = 32;

        static Skeleton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Skeleton), new FrameworkPropertyMetadata(typeof(Skeleton)));
        }

        public Skeleton()
        {
            ThemeLink.Attach(this, OnThemeChanged);
            UpdateAnimation();
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            "Rows", typeof(int), typeof(Skeleton),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty AvatarProperty = DependencyProperty.Register(
            "Avatar", typeof(bool), typeof(Skeleton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Avatar
        {
            get { return (bool)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(bool), typeof(Skeleton),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Title
        {
            get { return (bool)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register(
            "Active", typeof(bool), typeof(Skeleton),
            new FrameworkPropertyMetadata(false, OnActiveChanged));

        /// <summary>antd's animated skeleton: a subtle opacity pulse.</summary>
        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Skeleton)d).UpdateAnimation();
        }

        private void OnThemeChanged()
        {
            InvalidateVisual();
        }

        private void UpdateAnimation()
        {
            if (Active)
            {
                var animation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.4,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1200)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1.0;
            }
        }

        private int VisibleRows
        {
            get { return Rows < 0 ? 0 : Rows; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double height = 0;
            int bars = (Title ? 1 : 0) + VisibleRows;
            if (bars > 0) height = bars * BarHeight + (bars - 1) * Gap;

            double avatarExtent = Avatar ? AvatarSize + Gap : 0;
            height = Math.Max(height, Avatar ? AvatarSize : 0);

            double width = double.IsInfinity(availableSize.Width) ? 300 : availableSize.Width;
            return new Size(width + avatarExtent, height);
        }

        protected override void OnRender(DrawingContext dc)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            Brush brush = snapshot.Brush("colorFillTertiary");
            double radius = snapshot.Tokens.BorderRadiusSM;

            double left = 0;
            if (Avatar)
            {
                dc.DrawEllipse(brush, null,
                    new Point(AvatarSize / 2, Math.Min(AvatarSize / 2, ActualHeight / 2)), AvatarSize / 2, AvatarSize / 2);
                left = AvatarSize + Gap;
            }

            double width = Math.Max(0, ActualWidth - left);
            double y = 0;

            if (Title)
            {
                dc.DrawRoundedRectangle(brush, null, new Rect(left, y, width * 0.4, BarHeight), radius, radius);
                y += BarHeight + Gap;
            }

            for (int i = 0; i < VisibleRows; i++)
            {
                double factor = i == VisibleRows - 1 && VisibleRows > 2 ? 0.6 : 1.0;
                dc.DrawRoundedRectangle(brush, null, new Rect(left, y, width * factor, BarHeight), radius, radius);
                y += BarHeight + Gap;
            }
        }
    }
}
