using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    public enum TextType
    {
        Default = 0,
        Secondary = 1,
        Success = 2,
        Warning = 3,
        Danger = 4
    }

    /// <summary>
    /// antd Typography.Text. Derives from TextBlock so wrapping, trimming and text rendering
    /// stay native; only colour and decoration are token driven.
    /// </summary>
    public class Text : TextBlock
    {
        public Text()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(TextType), typeof(Text),
            new FrameworkPropertyMetadata(TextType.Default, OnAppearanceChanged));

        public TextType Type
        {
            get { return (TextType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(
            "Disabled", typeof(bool), typeof(Text),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        public static readonly DependencyProperty StrongProperty = DependencyProperty.Register(
            "Strong", typeof(bool), typeof(Text),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Strong
        {
            get { return (bool)GetValue(StrongProperty); }
            set { SetValue(StrongProperty, value); }
        }

        public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register(
            "Italic", typeof(bool), typeof(Text),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Italic
        {
            get { return (bool)GetValue(ItalicProperty); }
            set { SetValue(ItalicProperty, value); }
        }

        public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register(
            "Underline", typeof(bool), typeof(Text),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Underline
        {
            get { return (bool)GetValue(UnderlineProperty); }
            set { SetValue(UnderlineProperty, value); }
        }

        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(
            "Code", typeof(bool), typeof(Text),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        /// <summary>antd inline code: monospace font on a subtle background chip.</summary>
        public bool Code
        {
            get { return (bool)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Text)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            string colorKey;
            switch (Type)
            {
                case TextType.Secondary: colorKey = "colorTextSecondary"; break;
                case TextType.Success: colorKey = "colorSuccess"; break;
                case TextType.Warning: colorKey = "colorWarning"; break;
                case TextType.Danger: colorKey = "colorError"; break;
                default: colorKey = "colorText"; break;
            }
            if (Disabled) colorKey = "colorTextDisabled";

            SetCurrentValue(ForegroundProperty, snapshot.Brush(colorKey));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(FontWeightProperty, Strong ? FontWeights.SemiBold : FontWeights.Normal);
            SetCurrentValue(FontStyleProperty, Italic ? FontStyles.Italic : FontStyles.Normal);
            SetCurrentValue(FontFamilyProperty, Code ? snapshot.FontFamilyCode : snapshot.FontFamily);
            SetCurrentValue(BackgroundProperty, Code ? snapshot.Brush("colorFillTertiary") : Brushes.Transparent);
            SetCurrentValue(PaddingProperty, Code ? new Thickness(2, 0, 2, 0) : new Thickness(0));

            TextDecorationCollection decorations = Underline || Code ? System.Windows.TextDecorations.Underline : null;
            SetCurrentValue(TextDecorationsProperty, decorations);
        }
    }

    /// <summary>antd Typography.Title: heading levels 1..5.</summary>
    public class Title : TextBlock
    {
        public Title()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
            "Level", typeof(int), typeof(Title),
            new FrameworkPropertyMetadata(1, OnLevelChanged));

        /// <summary>1 to 5, matching antd's h1..h5.</summary>
        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Title)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            int level = Level < 1 ? 1 : (Level > 5 ? 5 : Level);
            double size;
            double lineHeight;

            switch (level)
            {
                case 1: size = t.FontSizeHeading1; lineHeight = t.LineHeightHeading1; break;
                case 2: size = t.FontSizeHeading2; lineHeight = t.LineHeightHeading2; break;
                case 3: size = t.FontSizeHeading3; lineHeight = t.LineHeightHeading3; break;
                case 4: size = t.FontSizeHeading4; lineHeight = t.LineHeightHeading4; break;
                default: size = t.FontSizeHeading5; lineHeight = t.LineHeightHeading5; break;
            }

            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorTextHeading"));
            SetCurrentValue(FontSizeProperty, size);
            SetCurrentValue(LineHeightProperty, size * lineHeight);
            SetCurrentValue(LineStackingStrategyProperty, LineStackingStrategy.BlockLineHeight);
            SetCurrentValue(FontWeightProperty, FontWeights.SemiBold);
            SetCurrentValue(FontFamilyProperty, snapshot.FontFamily);
            SetCurrentValue(MarginProperty, new Thickness(0, 0, 0, level <= 2 ? t.SizeSM : t.SizeXS));
        }
    }

    /// <summary>antd Typography.Paragraph: a block of secondary body text.</summary>
    public class Paragraph : TextBlock
    {
        public Paragraph()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(TextType), typeof(Paragraph),
            new FrameworkPropertyMetadata(TextType.Default, OnAppearanceChanged));

        public TextType Type
        {
            get { return (TextType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Paragraph)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            string colorKey;
            switch (Type)
            {
                case TextType.Secondary: colorKey = "colorTextSecondary"; break;
                case TextType.Success: colorKey = "colorSuccess"; break;
                case TextType.Warning: colorKey = "colorWarning"; break;
                case TextType.Danger: colorKey = "colorError"; break;
                default: colorKey = "colorText"; break;
            }

            SetCurrentValue(ForegroundProperty, snapshot.Brush(colorKey));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(LineHeightProperty, t.FontHeight);
            SetCurrentValue(LineStackingStrategyProperty, LineStackingStrategy.BlockLineHeight);
            SetCurrentValue(TextWrappingProperty, TextWrapping.Wrap);
            SetCurrentValue(MarginProperty, new Thickness(0, 0, 0, t.Size));
            SetCurrentValue(FontFamilyProperty, snapshot.FontFamily);
        }
    }

    /// <summary>antd Typography.Link.</summary>
    public class Link : TextBlock
    {
        public Link()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            Cursor = Cursors.Hand;
            MouseEnter += delegate { UpdateVisualState(); };
            MouseLeave += delegate { UpdateVisualState(); };
            UpdateVisualState();
        }

        public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(
            "Disabled", typeof(bool), typeof(Link),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register(
            "Underline", typeof(bool), typeof(Link),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Underline
        {
            get { return (bool)GetValue(UnderlineProperty); }
            set { SetValue(UnderlineProperty, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Link)d).UpdateVisualState();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!Disabled) OnClick();
        }

        /// <summary>Raised when the link is activated; wire this instead of using a Command.</summary>
        protected virtual void OnClick()
        {
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            string key = Disabled ? "colorTextDisabled" : (IsMouseOver ? "colorLinkHover" : "colorLink");
            SetCurrentValue(ForegroundProperty, snapshot.Brush(key));
            SetCurrentValue(FontSizeProperty, snapshot.Tokens.FontSize);
            SetCurrentValue(FontFamilyProperty, snapshot.FontFamily);
            SetCurrentValue(CursorProperty, Disabled ? Cursors.Arrow : Cursors.Hand);
            SetCurrentValue(TextDecorationsProperty, Underline || (IsMouseOver && !Disabled) ? System.Windows.TextDecorations.Underline : null);
        }
    }
}
