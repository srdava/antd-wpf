using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Card. The head is 56 tall with 24 inline padding and a hairline underneath; the body
    /// keeps 24 padding on all sides. Measured from real antd rather than guessed.
    /// </summary>
    public class Card : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Card");
        }
        static Card()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
        }

        public Card()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Card),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(object), typeof(Card), new FrameworkPropertyMetadata(null, OnHeaderChanged));

        /// <summary>Head text. Hiding it also removes the whole head row.</summary>
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ExtraProperty = DependencyProperty.Register(
            "Extra", typeof(object), typeof(Card), new FrameworkPropertyMetadata(null, OnHeaderChanged));

        /// <summary>Right-aligned slot in the head, antd's <c>extra</c>.</summary>
        public object Extra
        {
            get { return GetValue(ExtraProperty); }
            set { SetValue(ExtraProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(Card), new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        private static readonly DependencyPropertyKey HasHeaderKey = DependencyProperty.RegisterReadOnly(
            "HasHeader", typeof(bool), typeof(Card), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasHeaderProperty = HasHeaderKey.DependencyProperty;

        public bool HasHeader
        {
            get { return (bool)GetValue(HasHeaderProperty); }
            private set { SetValue(HasHeaderKey, value); }
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Card)d).UpdateVisualState();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Card)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool showHeader = Title != null || Extra != null;
            SetValue(HasHeaderKey, showHeader);

            SetCurrentValue(BorderThicknessProperty, new Thickness(Bordered ? t.LineWidth : 0));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush("colorBorderSecondary"));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(t.BorderRadiusLG));
            SetCurrentValue(BackgroundProperty, snapshot.Brush("colorBgContainer"));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }
}
