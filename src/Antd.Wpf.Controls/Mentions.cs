using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Mentions: a text box that offers options after a trigger character (default '@').
    /// The popup is anchored at the caret using TextBox.GetRectFromCharacterIndex, which is the
    /// WPF equivalent of antd's caret tracking.
    /// </summary>
    public class Mentions : Control
    {
        private TextBox _editor;
        private Popup _popup;
        private ListBox _list;
        private bool _updating;

        static Mentions()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Mentions), new FrameworkPropertyMetadata(typeof(Mentions)));
        }

        public Mentions()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Mentions),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(Mentions),
            new FrameworkPropertyMetadata(null, OnSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty TriggerCharProperty = DependencyProperty.Register(
            "TriggerChar", typeof(char), typeof(Mentions), new FrameworkPropertyMetadata('@'));

        /// <summary>Character that opens the suggestion list.</summary>
        public char TriggerChar
        {
            get { return (char)GetValue(TriggerCharProperty); }
            set { SetValue(TriggerCharProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(Mentions),
            new FrameworkPropertyMetadata("Type @ to mention", OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(Mentions),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Mentions),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(Mentions), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private int _tokenStart = -1;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_editor != null)
            {
                _editor.TextChanged -= OnEditorTextChanged;
                _editor.PreviewKeyDown -= OnEditorKeyDown;
                _editor.SelectionChanged -= OnEditorSelectionChanged;
            }

            _editor = GetTemplateChild("PART_Editor") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _list = GetTemplateChild("PART_Suggestions") as ListBox;

            if (_editor != null)
            {
                _editor.TextChanged += OnEditorTextChanged;
                _editor.PreviewKeyDown += OnEditorKeyDown;
                _editor.SelectionChanged += OnEditorSelectionChanged;

                _updating = true;
                _editor.Text = Text ?? string.Empty;
                _updating = false;
                if (_popup != null) _popup.PlacementTarget = _editor;
            }

            // The panel is offset to the caret, so it is anchored to the editor itself and only
            // asked to re-run placement when the page scrolls.
            PopupAnchor.Attach(_popup, _editor);
            if (_popup != null) _popup.Closed += OnPopupClosed;

            UpdateState();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mentions = (Mentions)d;
            if (mentions._editor != null && mentions._editor.Text != (string)e.NewValue)
            {
                mentions._updating = true;
                mentions._editor.Text = (string)e.NewValue ?? string.Empty;
                mentions._updating = false;
            }
            mentions.UpdateState();
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Mentions)d).UpdateState();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Mentions)d).UpdateState();
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mentions = (Mentions)d;
            if (mentions._popup != null) mentions._popup.IsOpen = mentions.IsDropDownOpen;
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updating) return;

            _updating = true;
            SetCurrentValue(TextProperty, _editor.Text);
            _updating = false;

            UpdateState();
        }

        private void OnEditorSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            string text = Text ?? string.Empty;
            SetValue(ShowPlaceholderKey, string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Placeholder));

            if (_editor == null) return;

            string token = CurrentToken(text, _editor.CaretIndex, out _tokenStart);
            List<object> matches = token == null ? new List<object>() : Filter(token);

            if (_list != null)
            {
                _list.ItemsSource = matches;
                _list.SelectedIndex = matches.Count > 0 ? 0 : -1;
            }

            bool open = matches.Count > 0 && _editor.IsKeyboardFocusWithin;
            SetCurrentValue(IsDropDownOpenProperty, open);

            if (open) PositionPopup();
        }

        /// <summary>Text typed after the last trigger character, or null when not in a mention.</summary>
        private string CurrentToken(string text, int caret, out int start)
        {
            start = -1;
            if (caret < 0 || caret > text.Length) return null;

            for (int i = caret - 1; i >= 0; i--)
            {
                char c = text[i];
                if (c == TriggerChar)
                {
                    start = i;
                    return text.Substring(i + 1, caret - i - 1);
                }
                if (char.IsWhiteSpace(c) || c == '\n') return null;
            }
            return null;
        }

        private List<object> Filter(string token)
        {
            var result = new List<object>();
            if (ItemsSource == null) return result;

            foreach (object item in ItemsSource)
            {
                string label = item == null ? string.Empty : item.ToString();
                if (string.IsNullOrEmpty(token)
                    || label.IndexOf(token, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        private void PositionPopup()
        {
            if (_popup == null || _editor == null) return;

            int caret = Math.Min(_editor.CaretIndex, Math.Max(0, _editor.Text.Length));
            Rect rect = _editor.GetRectFromCharacterIndex(caret, false);
            if (rect.IsEmpty) return;

            _popup.HorizontalOffset = rect.X;
            _popup.VerticalOffset = rect.Bottom + 4;
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (_list == null) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (_list.Items.Count > 0)
                        _list.SelectedIndex = Math.Min(_list.SelectedIndex + 1, _list.Items.Count - 1);
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (_list.Items.Count > 0)
                        _list.SelectedIndex = Math.Max(_list.SelectedIndex - 1, 0);
                    e.Handled = true;
                    break;

                case Key.Enter:
                case Key.Tab:
                    if (IsDropDownOpen) { InsertSelection(); e.Handled = true; }
                    break;

                case Key.Escape:
                    SetCurrentValue(IsDropDownOpenProperty, false);
                    e.Handled = true;
                    break;
            }
        }

        private void InsertSelection()
        {
            if (_list == null || _list.SelectedItem == null || _editor == null) return;
            if (_tokenStart < 0) return;

            string label = _list.SelectedItem.ToString();
            string text = _editor.Text;
            string prefix = text.Substring(0, _tokenStart + 1);
            string suffix = text.Substring(Math.Min(_editor.CaretIndex, text.Length));

            string next = prefix + label + " " + suffix;
            SetCurrentValue(IsDropDownOpenProperty, false);

            _editor.Text = next;
            _editor.CaretIndex = prefix.Length + label.Length + 1;
            _editor.Focus();
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool focused = _editor != null && _editor.IsKeyboardFocusWithin;
            string borderKey = !IsEnabled ? "colorBorderDisabled"
                : (focused || IsDropDownOpen ? "colorPrimary" : "colorBorder");

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(t.BorderRadius));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(MinHeightProperty, t.ControlHeight);
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(AntdSize.Middle), 4,
                Metrics.InputPaddingInline(AntdSize.Middle), 4));
        }
    }
}
