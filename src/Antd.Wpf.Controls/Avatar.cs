using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    public enum AntdAvatarShape
    {
        Circle = 0,
        Square = 1
    }

    /// <summary>antd Avatar: image, initials or icon in a circular/square frame.</summary>
    public class Avatar : ContentControl
    {
        static Avatar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Avatar), new FrameworkPropertyMetadata(typeof(Avatar)));
        }

        public Avatar()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            "Shape", typeof(AntdAvatarShape), typeof(Avatar),
            new FrameworkPropertyMetadata(AntdAvatarShape.Circle, OnAppearanceChanged));

        public AntdAvatarShape Shape
        {
            get { return (AntdAvatarShape)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(Avatar),
            new FrameworkPropertyMetadata(32.0, OnAppearanceChanged));

        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(Avatar),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static readonly DependencyPropertyKey AvatarBackgroundKey = DependencyProperty.RegisterReadOnly(
            "AvatarBackground", typeof(Brush), typeof(Avatar), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty AvatarBackgroundProperty = AvatarBackgroundKey.DependencyProperty;

        public Brush AvatarBackground
        {
            get { return (Brush)GetValue(AvatarBackgroundProperty); }
            private set { SetValue(AvatarBackgroundKey, value); }
        }

        private static readonly DependencyPropertyKey AvatarForegroundKey = DependencyProperty.RegisterReadOnly(
            "AvatarForeground", typeof(Brush), typeof(Avatar), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty AvatarForegroundProperty = AvatarForegroundKey.DependencyProperty;

        public Brush AvatarForeground
        {
            get { return (Brush)GetValue(AvatarForegroundProperty); }
            private set { SetValue(AvatarForegroundKey, value); }
        }

        private static readonly DependencyPropertyKey AvatarCornerRadiusKey = DependencyProperty.RegisterReadOnly(
            "AvatarCornerRadius", typeof(CornerRadius), typeof(Avatar), new FrameworkPropertyMetadata(new CornerRadius(16)));

        public static readonly DependencyProperty AvatarCornerRadiusProperty = AvatarCornerRadiusKey.DependencyProperty;

        public CornerRadius AvatarCornerRadius
        {
            get { return (CornerRadius)GetValue(AvatarCornerRadiusProperty); }
            private set { SetValue(AvatarCornerRadiusKey, value); }
        }

        private static readonly DependencyPropertyKey AvatarFontSizeKey = DependencyProperty.RegisterReadOnly(
            "AvatarFontSize", typeof(double), typeof(Avatar), new FrameworkPropertyMetadata(14.0));

        public static readonly DependencyProperty AvatarFontSizeProperty = AvatarFontSizeKey.DependencyProperty;

        public double AvatarFontSize
        {
            get { return (double)GetValue(AvatarFontSizeProperty); }
            private set { SetValue(AvatarFontSizeKey, value); }
        }

        private static readonly DependencyPropertyKey HasImageKey = DependencyProperty.RegisterReadOnly(
            "HasImage", typeof(bool), typeof(Avatar), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasImageProperty = HasImageKey.DependencyProperty;

        public bool HasImage
        {
            get { return (bool)GetValue(HasImageProperty); }
            private set { SetValue(HasImageKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Avatar)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            double size = Size <= 0 ? 32 : Size;

            SetValue(AvatarBackgroundKey, snapshot.Brush("colorFillTertiary"));
            SetValue(AvatarForegroundKey, snapshot.Brush("colorText"));
            SetValue(AvatarCornerRadiusKey, new CornerRadius(Shape == AntdAvatarShape.Circle ? size / 2 : snapshot.Tokens.BorderRadiusLG));
            SetValue(AvatarFontSizeKey, size <= 24 ? snapshot.Tokens.FontSizeSM : snapshot.Tokens.FontSize);
            SetValue(HasImageKey, Source != null);

            SetCurrentValue(WidthProperty, size);
            SetCurrentValue(HeightProperty, size);
        }
    }
}
