using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Icons;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Empty. M1 uses a neutral inbox glyph instead of the multi-part illustration;
    /// antd's own PRESENTED_IMAGE_SIMPLE is visually close and avoids shipping a large
    /// vector asset before the icon pipeline covers two-tone icons.
    /// </summary>
    public class Empty : Control
    {
        static Empty()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Empty), new FrameworkPropertyMetadata(typeof(Empty)));
        }

        public Empty()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(Empty),
            new FrameworkPropertyMetadata("No Data"));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof(Geometry), typeof(Empty),
            new FrameworkPropertyMetadata(AntdIcons.Inbox));

        /// <summary>Replaces the default glyph.</summary>
        public Geometry Image
        {
            get { return (Geometry)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        private static readonly DependencyPropertyKey DescriptionBrushKey = DependencyProperty.RegisterReadOnly(
            "DescriptionBrush", typeof(Brush), typeof(Empty), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty DescriptionBrushProperty = DescriptionBrushKey.DependencyProperty;

        public Brush DescriptionBrush
        {
            get { return (Brush)GetValue(DescriptionBrushProperty); }
            private set { SetValue(DescriptionBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ImageBrushKey = DependencyProperty.RegisterReadOnly(
            "ImageBrush", typeof(Brush), typeof(Empty), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ImageBrushProperty = ImageBrushKey.DependencyProperty;

        public Brush ImageBrush
        {
            get { return (Brush)GetValue(ImageBrushProperty); }
            private set { SetValue(ImageBrushKey, value); }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            SetValue(DescriptionBrushKey, snapshot.Brush("colorTextTertiary"));
            SetValue(ImageBrushKey, snapshot.Brush("colorTextQuaternary"));
        }
    }
}
