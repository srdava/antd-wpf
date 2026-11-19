using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Icons;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>Alert severity, antd's `type` prop.</summary>
    public enum AlertType
    {
        Success = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>Result page state, antd's `status` prop.</summary>
    public enum ResultStatus
    {
        Success = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        NotFound = 4,
        ServerError = 5
    }

    /// <summary>
    /// antd Alert. Measured from real antd: the one-line form is 40 tall (8/12 padding, 1px border,
    /// 8 radius); the description form switches to 20/24 padding with a 16px title 8 below which
    /// the copy sits. Banner mode drops the border and the radius and keeps the 8/12 padding.
    /// </summary>
    public class Alert : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AlertAutomationPeer(this);
        }
        static Alert()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Alert), new FrameworkPropertyMetadata(typeof(Alert)));
        }

        public Alert()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(AlertType), typeof(Alert),
            new FrameworkPropertyMetadata(AlertType.Info, OnAppearanceChanged));

        public AlertType Type
        {
            get { return (AlertType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(Alert),
            new FrameworkPropertyMetadata(string.Empty, OnAppearanceChanged));

        /// <summary>The headline line. antd keeps it on one line; ours wraps when it runs out of room.</summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(Alert),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        /// <summary>Secondary copy. Setting it switches the whole alert to the roomy 20/24 layout.</summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            "ShowIcon", typeof(bool), typeof(Alert),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty = DependencyProperty.Register(
            "Closable", typeof(bool), typeof(Alert),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty BannerProperty = DependencyProperty.Register(
            "Banner", typeof(bool), typeof(Alert),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        /// <summary>Full-bleed strip: square corners, no side borders, no rounding.</summary>
        public bool Banner
        {
            get { return (bool)GetValue(BannerProperty); }
            set { SetValue(BannerProperty, value); }
        }

        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
            "Action", typeof(object), typeof(Alert), new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        /// <summary>Right-aligned action slot, antd's `action`.</summary>
        public object Action
        {
            get { return GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
            "IconGeometry", typeof(Geometry), typeof(Alert), new FrameworkPropertyMetadata(null));

        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            "IconBrush", typeof(Brush), typeof(Alert), new FrameworkPropertyMetadata(null));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            "IconSize", typeof(double), typeof(Alert), new FrameworkPropertyMetadata(14d));

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty MessageFontSizeProperty = DependencyProperty.Register(
            "MessageFontSize", typeof(double), typeof(Alert), new FrameworkPropertyMetadata(14d));

        public double MessageFontSize
        {
            get { return (double)GetValue(MessageFontSizeProperty); }
            set { SetValue(MessageFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MessageLineHeightProperty = DependencyProperty.Register(
            "MessageLineHeight", typeof(double), typeof(Alert), new FrameworkPropertyMetadata(22d));

        /// <summary>22 for the 14px line, 25 for the 16px title of the described form.</summary>
        public double MessageLineHeight
        {
            get { return (double)GetValue(MessageLineHeightProperty); }
            set { SetValue(MessageLineHeightProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Alert), new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey HasDescriptionKey = DependencyProperty.RegisterReadOnly(
            "HasDescription", typeof(bool), typeof(Alert), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasDescriptionProperty = HasDescriptionKey.DependencyProperty;

        public bool HasDescription
        {
            get { return (bool)GetValue(HasDescriptionProperty); }
            private set { SetValue(HasDescriptionKey, value); }
        }

        private static readonly DependencyPropertyKey IconVisibilityKey = DependencyProperty.RegisterReadOnly(
            "IconVisibility", typeof(Visibility), typeof(Alert), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IconVisibilityProperty = IconVisibilityKey.DependencyProperty;

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            private set { SetValue(IconVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey DescriptionVisibilityKey = DependencyProperty.RegisterReadOnly(
            "DescriptionVisibility", typeof(Visibility), typeof(Alert),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty DescriptionVisibilityProperty =
            DescriptionVisibilityKey.DependencyProperty;

        public Visibility DescriptionVisibility
        {
            get { return (Visibility)GetValue(DescriptionVisibilityProperty); }
            private set { SetValue(DescriptionVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey CloseVisibilityKey = DependencyProperty.RegisterReadOnly(
            "CloseVisibility", typeof(Visibility), typeof(Alert), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty CloseVisibilityProperty = CloseVisibilityKey.DependencyProperty;

        public Visibility CloseVisibility
        {
            get { return (Visibility)GetValue(CloseVisibilityProperty); }
            private set { SetValue(CloseVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey ActionVisibilityKey = DependencyProperty.RegisterReadOnly(
            "ActionVisibility", typeof(Visibility), typeof(Alert), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ActionVisibilityProperty = ActionVisibilityKey.DependencyProperty;

        public Visibility ActionVisibility
        {
            get { return (Visibility)GetValue(ActionVisibilityProperty); }
            private set { SetValue(ActionVisibilityKey, value); }
        }

        /// <summary>Raised after the close icon was pressed. The alert hides itself as well.</summary>
        public event EventHandler Closed;

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Alert)d).UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool described = !string.IsNullOrEmpty(Description) && !Banner;
            SetValue(HasDescriptionKey, described);
            SetValue(DescriptionVisibilityKey, described ? Visibility.Visible : Visibility.Collapsed);
            SetValue(IconVisibilityKey, ShowIcon ? Visibility.Visible : Visibility.Collapsed);
            SetValue(CloseVisibilityKey, Closable ? Visibility.Visible : Visibility.Collapsed);
            SetValue(ActionVisibilityKey, Action != null ? Visibility.Visible : Visibility.Collapsed);

            SetCurrentValue(BackgroundProperty, snapshot.Brush(BackgroundToken(Type)));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(BorderToken(Type)));
            SetCurrentValue(BorderThicknessProperty, new Thickness(Banner ? 0 : t.LineWidth));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(Banner ? 0 : t.BorderRadiusLG));
            SetCurrentValue(IconBrushProperty, snapshot.Brush(IconToken(Type)));
            SetCurrentValue(IconGeometryProperty, IconFor(Type));
            SetCurrentValue(IconSizeProperty, described ? 24d : 14d);
            SetCurrentValue(MessageFontSizeProperty, described ? t.FontSizeLG : t.FontSize);
            SetCurrentValue(MessageLineHeightProperty, described ? 25d : t.FontSize * t.LineHeight);
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(PaddingProperty, described ? new Thickness(24, 20, 24, 20) : new Thickness(12, 8, 12, 8));
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorText"));
            SetCurrentValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
        }

        private static string BackgroundToken(AlertType type)
        {
            switch (type)
            {
                case AlertType.Success: return "colorSuccessBg";
                case AlertType.Warning: return "colorWarningBg";
                case AlertType.Error: return "colorErrorBg";
                default: return "colorInfoBg";
            }
        }

        private static string BorderToken(AlertType type)
        {
            switch (type)
            {
                case AlertType.Success: return "colorSuccessBorder";
                case AlertType.Warning: return "colorWarningBorder";
                case AlertType.Error: return "colorErrorBorder";
                default: return "colorInfoBorder";
            }
        }

        private static string IconToken(AlertType type)
        {
            switch (type)
            {
                case AlertType.Success: return "colorSuccess";
                case AlertType.Warning: return "colorWarning";
                case AlertType.Error: return "colorError";
                default: return "colorInfo";
            }
        }

        private static Geometry IconFor(AlertType type)
        {
            switch (type)
            {
                case AlertType.Success: return AntdIcons.CheckCircle;
                case AlertType.Warning: return AntdIcons.ExclamationCircle;
                case AlertType.Error: return AntdIcons.CloseCircle;
                default: return AntdIcons.InfoCircle;
            }
        }

        /// <summary>Called by the template's close button.</summary>
        internal void HandleClose()
        {
            SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            EventHandler handler = Closed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElement close = GetTemplateChild("PART_Close") as FrameworkElement;
            if (close != null)
            {
                close.MouseLeftButtonUp -= OnCloseClick;
                close.MouseLeftButtonUp += OnCloseClick;
                AutomationProperties.SetName(close, AntdLocale.Current.Close);
            }
        }

        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            HandleClose();
        }
    }

    /// <summary>
    /// antd Result. Measured: 48/32 outer padding, a 72px status glyph, then title 24/32, subtitle
    /// 14/22 eight below, and the action row twenty-four below that. The title is a regular weight
    /// in antd 6, not bold.
    /// </summary>
    public class Result : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ResultAutomationPeer(this);
        }
        static Result()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Result), new FrameworkPropertyMetadata(typeof(Result)));
        }

        public Result()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(ResultStatus), typeof(Result),
            new FrameworkPropertyMetadata(ResultStatus.Info, OnAppearanceChanged));

        public ResultStatus Status
        {
            get { return (ResultStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Result),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register(
            "SubTitle", typeof(string), typeof(Result),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }

        public static readonly DependencyProperty ExtraProperty = DependencyProperty.Register(
            "Extra", typeof(object), typeof(Result), new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        /// <summary>Action row under the copy (usually buttons).</summary>
        public object Extra
        {
            get { return GetValue(ExtraProperty); }
            set { SetValue(ExtraProperty, value); }
        }

        public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
            "IconGeometry", typeof(Geometry), typeof(Result), new FrameworkPropertyMetadata(null));

        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            "IconBrush", typeof(Brush), typeof(Result), new FrameworkPropertyMetadata(null));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        /// <summary>404 / 500 are drawn as a big numeral, the same slot the glyph would take.</summary>
        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            "StatusText", typeof(string), typeof(Result), new FrameworkPropertyMetadata(string.Empty));

        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        private static readonly DependencyPropertyKey IconVisibilityKey = DependencyProperty.RegisterReadOnly(
            "IconVisibility", typeof(Visibility), typeof(Result), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IconVisibilityProperty = IconVisibilityKey.DependencyProperty;

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            private set { SetValue(IconVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey StatusTextVisibilityKey = DependencyProperty.RegisterReadOnly(
            "StatusTextVisibility", typeof(Visibility), typeof(Result), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty StatusTextVisibilityProperty = StatusTextVisibilityKey.DependencyProperty;

        public Visibility StatusTextVisibility
        {
            get { return (Visibility)GetValue(StatusTextVisibilityProperty); }
            private set { SetValue(StatusTextVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey SubTitleVisibilityKey = DependencyProperty.RegisterReadOnly(
            "SubTitleVisibility", typeof(Visibility), typeof(Result), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty SubTitleVisibilityProperty = SubTitleVisibilityKey.DependencyProperty;

        public Visibility SubTitleVisibility
        {
            get { return (Visibility)GetValue(SubTitleVisibilityProperty); }
            private set { SetValue(SubTitleVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey ExtraVisibilityKey = DependencyProperty.RegisterReadOnly(
            "ExtraVisibility", typeof(Visibility), typeof(Result), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ExtraVisibilityProperty = ExtraVisibilityKey.DependencyProperty;

        public Visibility ExtraVisibility
        {
            get { return (Visibility)GetValue(ExtraVisibilityProperty); }
            private set { SetValue(ExtraVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey ContentVisibilityKey = DependencyProperty.RegisterReadOnly(
            "ContentVisibility", typeof(Visibility), typeof(Result),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ContentVisibilityProperty = ContentVisibilityKey.DependencyProperty;

        /// <summary>Hidden so the trailing 24px gap does not appear on results without extra content.</summary>
        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            private set { SetValue(ContentVisibilityKey, value); }
        }

        private static readonly DependencyPropertyKey TitleVisibilityKey = DependencyProperty.RegisterReadOnly(
            "TitleVisibility", typeof(Visibility), typeof(Result), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty TitleVisibilityProperty = TitleVisibilityKey.DependencyProperty;

        public Visibility TitleVisibility
        {
            get { return (Visibility)GetValue(TitleVisibilityProperty); }
            private set { SetValue(TitleVisibilityKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Result)d).UpdateVisualState();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateVisualState();
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool numeral = Status == ResultStatus.NotFound || Status == ResultStatus.ServerError;
            SetValue(IconVisibilityKey, numeral ? Visibility.Collapsed : Visibility.Visible);
            SetValue(StatusTextVisibilityKey, numeral ? Visibility.Visible : Visibility.Collapsed);
            SetValue(TitleVisibilityKey, string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible);
            SetValue(SubTitleVisibilityKey, string.IsNullOrEmpty(SubTitle) ? Visibility.Collapsed : Visibility.Visible);
            SetValue(ExtraVisibilityKey, Extra != null ? Visibility.Visible : Visibility.Collapsed);
            SetValue(ContentVisibilityKey, Content != null ? Visibility.Visible : Visibility.Collapsed);

            SetCurrentValue(IconGeometryProperty, IconFor(Status));
            SetCurrentValue(IconBrushProperty, snapshot.Brush(IconToken(Status)));
            SetCurrentValue(StatusTextProperty, numeral
                ? (Status == ResultStatus.NotFound ? "404" : "500")
                : string.Empty);

            SetCurrentValue(PaddingProperty, new Thickness(32, 48, 32, 48));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, Brushes.Transparent);
            SetCurrentValue(ForegroundProperty, snapshot.Brush("colorText"));
        }

        private static string IconToken(ResultStatus status)
        {
            switch (status)
            {
                case ResultStatus.Success: return "colorSuccess";
                case ResultStatus.Warning: return "colorWarning";
                case ResultStatus.Error: return "colorError";
                case ResultStatus.NotFound:
                case ResultStatus.ServerError: return "colorTextTertiary";
                default: return "colorInfo";
            }
        }

        private static Geometry IconFor(ResultStatus status)
        {
            switch (status)
            {
                case ResultStatus.Success: return AntdIcons.CheckCircle;
                case ResultStatus.Warning: return AntdIcons.ExclamationCircle;
                case ResultStatus.Error: return AntdIcons.CloseCircle;
                case ResultStatus.NotFound: return AntdIcons.QuestionCircle;
                case ResultStatus.ServerError: return AntdIcons.Warning;
                default: return AntdIcons.InfoCircle;
            }
        }
    }
}
