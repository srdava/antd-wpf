using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    public enum FloatButtonShape
    {
        Circle = 0,
        Square = 1
    }

    public enum FloatButtonPlacement
    {
        None = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }

    /// <summary>
    /// antd FloatButton. WPF has no fixed positioning, so the control maps antd's
    /// `placement` onto alignment plus a margin instead of floating outside the layout.
    /// </summary>
    public class FloatButton : System.Windows.Controls.Primitives.ButtonBase
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdInvokableAutomationPeer(this);
        }
        static FloatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatButton), new FrameworkPropertyMetadata(typeof(FloatButton)));
        }

        public FloatButton()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            "Shape", typeof(FloatButtonShape), typeof(FloatButton),
            new FrameworkPropertyMetadata(FloatButtonShape.Circle, OnAppearanceChanged));

        public FloatButtonShape Shape
        {
            get { return (FloatButtonShape)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement", typeof(FloatButtonPlacement), typeof(FloatButton),
            new FrameworkPropertyMetadata(FloatButtonPlacement.None, OnAppearanceChanged));

        public FloatButtonPlacement Placement
        {
            get { return (FloatButtonPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(FloatButton),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(FloatButton),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        /// <summary>Optional label shown under the icon (antd's `description`).</summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        private static readonly DependencyPropertyKey CornerRadiusKey = DependencyProperty.RegisterReadOnly(
            "CornerRadius", typeof(CornerRadius), typeof(FloatButton), new FrameworkPropertyMetadata(new CornerRadius(20)));

        public static readonly DependencyProperty CornerRadiusProperty = CornerRadiusKey.DependencyProperty;

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            private set { SetValue(CornerRadiusKey, value); }
        }

        private static readonly DependencyPropertyKey ShowsDescriptionKey = DependencyProperty.RegisterReadOnly(
            "ShowsDescription", typeof(bool), typeof(FloatButton), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShowsDescriptionProperty = ShowsDescriptionKey.DependencyProperty;

        public bool ShowsDescription
        {
            get { return (bool)GetValue(ShowsDescriptionProperty); }
            private set { SetValue(ShowsDescriptionKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FloatButton)d).UpdateVisualState();
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

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool hover = IsMouseOver && IsEnabled;
            string backgroundKey = hover ? "colorPrimaryHover" : "colorPrimary";

            SetCurrentValue(BackgroundProperty, snapshot.Brush(backgroundKey));
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorTextLightSolid"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(backgroundKey));
            SetCurrentValue(FontFamilyProperty, snapshot.FontFamily);
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(CursorProperty, IsEnabled ? Cursors.Hand : Cursors.Arrow);

            bool hasDescription = !string.IsNullOrEmpty(Description);
            SetValue(ShowsDescriptionKey, hasDescription);
            SetValue(CornerRadiusKey, new CornerRadius(
                Shape == FloatButtonShape.Circle ? (hasDescription ? t.BorderRadiusLG : 24) : t.BorderRadiusLG));

            const double extent = 48;
            SetCurrentValue(MinWidthProperty, hasDescription ? 64.0 : extent);
            SetCurrentValue(MinHeightProperty, hasDescription ? 56.0 : extent);

            ApplyPlacement(t);
        }

        private void ApplyPlacement(AliasToken t)
        {
            double gap = t.SizeLG;
            switch (Placement)
            {
                case FloatButtonPlacement.TopLeft:
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
                    SetCurrentValue(MarginProperty, new Thickness(gap, gap, 0, 0));
                    break;
                case FloatButtonPlacement.TopRight:
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
                    SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
                    SetCurrentValue(MarginProperty, new Thickness(0, gap, gap, 0));
                    break;
                case FloatButtonPlacement.BottomLeft:
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
                    SetCurrentValue(MarginProperty, new Thickness(gap, 0, 0, gap));
                    break;
                case FloatButtonPlacement.BottomRight:
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
                    SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
                    SetCurrentValue(MarginProperty, new Thickness(0, 0, gap, gap));
                    break;
                default:
                    SetCurrentValue(MarginProperty, new Thickness(0));
                    break;
            }
        }
    }
}
