using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Splitter: a draggable divider between two panes. It derives from GridSplitter so
    /// resizing, keyboard support and the cursor are handled by WPF; only the 1px bar and its
    /// hover highlight are ours.
    /// </summary>
    public class Splitter : GridSplitter
    {
        static Splitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Splitter), new FrameworkPropertyMetadata(typeof(Splitter)));
        }

        public Splitter()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            DragStarted += OnDragChanged;
            DragCompleted += OnDragChanged;
            UpdateVisualState();
        }

        private static readonly DependencyPropertyKey BarBrushKey = DependencyProperty.RegisterReadOnly(
            "BarBrush", typeof(System.Windows.Media.Brush), typeof(Splitter),
            new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Gray));

        public static readonly DependencyProperty BarBrushProperty = BarBrushKey.DependencyProperty;

        /// <summary>Divider colour: colorSplit normally, colorPrimary while dragged or hovered.</summary>
        public System.Windows.Media.Brush BarBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(BarBrushProperty); }
            private set { SetValue(BarBrushKey, value); }
        }

        private static readonly DependencyPropertyKey BarThicknessKey = DependencyProperty.RegisterReadOnly(
            "BarThickness", typeof(double), typeof(Splitter), new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty BarThicknessProperty = BarThicknessKey.DependencyProperty;

        public double BarThickness
        {
            get { return (double)GetValue(BarThicknessProperty); }
            private set { SetValue(BarThicknessKey, value); }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }

        private void OnDragChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            bool active = IsMouseOver || IsDragging;

            BarBrush = snapshot.Brush(active ? "colorPrimary" : "colorSplit");
            BarThickness = active ? 2 : 1;
            // GridSplitter has no Orientation; the direction it resizes tells us the bar shape.
            bool verticalBar = ResizeDirection != System.Windows.Controls.GridResizeDirection.Rows;
            Cursor = verticalBar ? Cursors.SizeWE : Cursors.SizeNS;
        }
    }
}
