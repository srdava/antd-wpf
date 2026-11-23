using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    public enum FormLayout
    {
        Vertical = 0,
        Horizontal = 1
    }

    /// <summary>
    /// antd Form. A lightweight container: it lays out <see cref="FormItem"/> children in a
    /// label/content grid and aggregates validation. It is not a full antd Form
    /// (no field dependency graph, no async rules) - see M2 notes for what is deferred.
    /// </summary>
    public class Form : Panel
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Form");
        }
        private readonly List<FormItem> _items = new List<FormItem>();

        public Form()
        {
            ThemeLink.Attach(this, InvalidateMeasure);
        }

        // Layout and LabelWidth live on FormItem and are inherited, so an item nested deeper still
        // picks them up. Form only re-exposes the same dependency properties.
        public static readonly DependencyProperty LayoutProperty = FormItem.LayoutProperty.AddOwner(
            typeof(Form), new FrameworkPropertyMetadata(FormLayout.Vertical,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public FormLayout Layout
        {
            get { return (FormLayout)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        public static readonly DependencyProperty LabelWidthProperty = FormItem.LabelWidthProperty.AddOwner(
            typeof(Form), new FrameworkPropertyMetadata(96.0,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Label column width used when <see cref="Layout"/> is Horizontal.</summary>
        public double LabelWidth
        {
            get { return (double)GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }

        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register(
            "ItemSpacing", typeof(double), typeof(Form),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemSpacing
        {
            get { return (double)GetValue(ItemSpacingProperty); }
            set { SetValue(ItemSpacingProperty, value); }
        }

        /// <summary>Validates every item and returns false when at least one is invalid.</summary>
        public bool Validate()
        {
            bool valid = true;
            foreach (UIElement child in InternalChildren)
            {
                var item = child as FormItem;
                if (item == null) continue;
                if (!item.Validate()) valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Validates every item including async validators. Items run one after another so a
        /// remote check does not stampede the server; the first failure still blocks submit.
        /// </summary>
        public async Task<bool> ValidateAsync()
        {
            bool valid = true;
            foreach (UIElement child in InternalChildren)
            {
                var item = child as FormItem;
                if (item == null) continue;
                if (!await item.ValidateAsync().ConfigureAwait(true)) valid = false;
            }
            return valid;
        }
        public void ResetErrors()
        {
            foreach (UIElement child in InternalChildren)
            {
                var item = child as FormItem;
                if (item != null) item.ClearError();
            }
        }

        private double Spacing
        {
            get
            {
                return double.IsNaN(ItemSpacing)
                    ? ThemeManager.Current.Snapshot.Tokens.SizeLG
                    : ItemSpacing;
            }
        }

        private void CollectItems()
        {
            _items.Clear();
            foreach (UIElement child in InternalChildren)
            {
                var item = child as FormItem;
                if (item != null) _items.Add(item);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            CollectItems();
            double spacing = Spacing;
            double total = 0;
            double width = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                FormItem item = _items[i];
                if (item.Visibility == Visibility.Collapsed) continue;

                item.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                total += item.DesiredSize.Height;
                if (item.DesiredSize.Width > width) width = item.DesiredSize.Width;
                if (i < _items.Count - 1) total += spacing;
            }

            if (double.IsInfinity(availableSize.Width) && width < 320) width = 320;
            return new Size(double.IsInfinity(availableSize.Width) ? width : availableSize.Width, total);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double spacing = Spacing;
            double y = 0;

            foreach (FormItem item in _items)
            {
                if (item.Visibility == Visibility.Collapsed) continue;
                item.Arrange(new Rect(0, y, finalSize.Width, item.DesiredSize.Height));
                y += item.DesiredSize.Height + spacing;
            }

            return finalSize;
        }
    }

    /// <summary>antd Form.Item: label row, content and error/help line.</summary>
    public class FormItem : ContentControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "FormItem");
        }
        static FormItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormItem), new FrameworkPropertyMetadata(typeof(FormItem)));
        }

        public FormItem()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            "Layout", typeof(FormLayout), typeof(FormItem),
            new FrameworkPropertyMetadata(FormLayout.Vertical,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnLayoutModeChanged));

        /// <summary>Inherited from the parent <see cref="Form"/>; can also be set per item.</summary>
        public FormLayout Layout
        {
            get { return (FormLayout)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            "LabelWidth", typeof(double), typeof(FormItem),
            new FrameworkPropertyMetadata(96.0,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnLayoutModeChanged));

        /// <summary>Width of the label column when <see cref="Layout"/> is Horizontal.</summary>
        public double LabelWidth
        {
            get { return (double)GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(FormItem), new FrameworkPropertyMetadata(null));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty RequiredProperty = DependencyProperty.Register(
            "Required", typeof(bool), typeof(FormItem),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool Required
        {
            get { return (bool)GetValue(RequiredProperty); }
            set { SetValue(RequiredProperty, value); }
        }

        public static readonly DependencyProperty HelpProperty = DependencyProperty.Register(
            "Help", typeof(string), typeof(FormItem),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public string Help
        {
            get { return (string)GetValue(HelpProperty); }
            set { SetValue(HelpProperty, value); }
        }

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(string), typeof(FormItem),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        /// <summary>Optional custom rule. Return null when valid, or the message to show.</summary>
        public Func<object, string> Validator { get; set; }

        /// <summary>
        /// Optional async check (uniqueness, server-side rules). Runs only after the synchronous
        /// required/custom checks pass; a null result means valid.
        /// </summary>
        public Func<object, Task<string>> AsyncValidator { get; set; }

        private static readonly DependencyPropertyKey IsValidatingKey = DependencyProperty.RegisterReadOnly(
            "IsValidating", typeof(bool), typeof(FormItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsValidatingProperty = IsValidatingKey.DependencyProperty;

        /// <summary>True while <see cref="AsyncValidator"/> is in flight; the demo shows a spinner.</summary>
        public bool IsValidating
        {
            get { return (bool)GetValue(IsValidatingProperty); }
            private set { SetValue(IsValidatingKey, value); }
        }

        private int _validationToken;


        private static readonly DependencyPropertyKey LabelBrushKey = DependencyProperty.RegisterReadOnly(
            "LabelBrush", typeof(Brush), typeof(FormItem), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty LabelBrushProperty = LabelBrushKey.DependencyProperty;

        public Brush LabelBrush
        {
            get { return (Brush)GetValue(LabelBrushProperty); }
            private set { SetValue(LabelBrushKey, value); }
        }

        private static readonly DependencyPropertyKey HelpBrushKey = DependencyProperty.RegisterReadOnly(
            "HelpBrush", typeof(Brush), typeof(FormItem), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty HelpBrushProperty = HelpBrushKey.DependencyProperty;

        public Brush HelpBrush
        {
            get { return (Brush)GetValue(HelpBrushProperty); }
            private set { SetValue(HelpBrushKey, value); }
        }

        private static readonly DependencyPropertyKey RequiredBrushKey = DependencyProperty.RegisterReadOnly(
            "RequiredBrush", typeof(Brush), typeof(FormItem), new FrameworkPropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty RequiredBrushProperty = RequiredBrushKey.DependencyProperty;

        public Brush RequiredBrush
        {
            get { return (Brush)GetValue(RequiredBrushProperty); }
            private set { SetValue(RequiredBrushKey, value); }
        }

        private static readonly DependencyPropertyKey MessageBrushKey = DependencyProperty.RegisterReadOnly(
            "MessageBrush", typeof(Brush), typeof(FormItem), new FrameworkPropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty MessageBrushProperty = MessageBrushKey.DependencyProperty;

        public Brush MessageBrush
        {
            get { return (Brush)GetValue(MessageBrushProperty); }
            private set { SetValue(MessageBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ActiveMessageKey = DependencyProperty.RegisterReadOnly(
            "ActiveMessage", typeof(string), typeof(FormItem), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ActiveMessageProperty = ActiveMessageKey.DependencyProperty;

        /// <summary>Error when present, otherwise the help text.</summary>
        public string ActiveMessage
        {
            get { return (string)GetValue(ActiveMessageProperty); }
            private set { SetValue(ActiveMessageKey, value); }
        }

        private static readonly DependencyPropertyKey HasMessageKey = DependencyProperty.RegisterReadOnly(
            "HasMessage", typeof(bool), typeof(FormItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasMessageProperty = HasMessageKey.DependencyProperty;

        public bool HasMessage
        {
            get { return (bool)GetValue(HasMessageProperty); }
            private set { SetValue(HasMessageKey, value); }
        }

        private static readonly DependencyPropertyKey HasLabelKey = DependencyProperty.RegisterReadOnly(
            "HasLabel", typeof(bool), typeof(FormItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasLabelProperty = HasLabelKey.DependencyProperty;

        public bool HasLabel
        {
            get { return (bool)GetValue(HasLabelProperty); }
            private set { SetValue(HasLabelKey, value); }
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormItem)d).UpdateVisualState();
        }

        private static void OnLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormItem)d).UpdateLayoutMode();
        }

        private ColumnDefinition _labelColumn;
        private FrameworkElement _labelRow;
        private FrameworkElement _contentHost;
        private FrameworkElement _messageText;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _labelColumn = GetTemplateChild("PART_LabelColumn") as ColumnDefinition;
            _labelRow = GetTemplateChild("PART_LabelRow") as FrameworkElement;
            _contentHost = GetTemplateChild("PART_Content") as FrameworkElement;
            _messageText = GetTemplateChild("PART_Message") as FrameworkElement;

            UpdateLayoutMode();
        }

        /// <summary>
        /// Switches between the stacked (label above field) and the two-column horizontal form.
        /// Row/column placement is set here rather than in template triggers because Setter.Value
        /// cannot carry a TemplateBinding for the label width.
        /// </summary>
        private void UpdateLayoutMode()
        {
            if (_labelColumn == null || _labelRow == null || _contentHost == null || _messageText == null) return;

            bool horizontal = Layout == FormLayout.Horizontal;

            _labelColumn.Width = horizontal ? new GridLength(Math.Max(0, LabelWidth)) : new GridLength(0);

            Grid.SetRow(_labelRow, 0);
            Grid.SetColumn(_labelRow, 0);
            Grid.SetColumnSpan(_labelRow, horizontal ? 1 : 2);
            // 5px pulls the 22px label line onto the centre of a 32px control.
            _labelRow.Margin = horizontal ? new Thickness(0, 5, 8, 0) : new Thickness(0, 0, 0, 8);

            Grid.SetRow(_contentHost, horizontal ? 0 : 1);
            Grid.SetColumn(_contentHost, horizontal ? 1 : 0);
            Grid.SetColumnSpan(_contentHost, horizontal ? 1 : 2);

            Grid.SetRow(_messageText, horizontal ? 1 : 2);
            Grid.SetColumn(_messageText, horizontal ? 1 : 0);
            Grid.SetColumnSpan(_messageText, horizontal ? 1 : 2);
        }

        public void ClearError()
        {
            SetCurrentValue(ErrorProperty, null);
        }

        /// <summary>Runs the built-in required check plus the optional custom validator.</summary>
        public bool Validate()
        {
            string message = null;

            if (Required && IsEmpty(Content))
            {
                message = AntdLocale.Current.RequiredField;
            }
            else if (Validator != null)
            {
                message = Validator(Content);
            }

            SetCurrentValue(ErrorProperty, message);
            return message == null;
        }

        /// <summary>
        /// Synchronous rules first, then the optional async validator. A newer call supersedes an
        /// older in-flight one, so fast typing cannot leave a stale error behind.
        /// </summary>
        public async Task<bool> ValidateAsync()
        {
            string message = null;

            if (Required && IsEmpty(Content))
            {
                message = AntdLocale.Current.RequiredField;
            }
            else if (Validator != null)
            {
                message = Validator(Content);
            }

            if (message != null || AsyncValidator == null)
            {
                _validationToken++;
                SetValue(IsValidatingKey, false);
                SetCurrentValue(ErrorProperty, message);
                return message == null;
            }

            int token = ++_validationToken;
            SetValue(IsValidatingKey, true);

            try
            {
                message = await AsyncValidator(Content).ConfigureAwait(true);
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            if (token != _validationToken) return false;

            SetValue(IsValidatingKey, false);
            SetCurrentValue(ErrorProperty, message);
            return message == null;
        }
        private static bool IsEmpty(object content)
        {
            if (content == null) return true;

            var input = content as System.Windows.Controls.TextBox;
            if (input != null) return string.IsNullOrWhiteSpace(input.Text);

            var number = content as InputNumber;
            if (number != null) return !number.Value.HasValue;

            var select = content as Select;
            if (select != null) return select.SelectedIndex < 0;

            var toggle = content as System.Windows.Controls.Primitives.ToggleButton;
            if (toggle != null) return false;

            return false;
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetValue(LabelBrushKey, snapshot.Brush("colorText"));
            SetValue(RequiredBrushKey, snapshot.Brush("colorError"));
            SetValue(HelpBrushKey, snapshot.Brush("colorTextTertiary"));
            SetValue(MessageBrushKey, snapshot.Brush(Error != null ? "colorError" : "colorTextTertiary"));

            string message = Error ?? Help;
            SetValue(ActiveMessageKey, message);
            SetValue(HasMessageKey, !string.IsNullOrEmpty(message));
            SetValue(HasLabelKey, !string.IsNullOrEmpty(Label));
            SetCurrentValue(FontSizeProperty, t.FontSize);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateVisualState();
        }
    }
}
