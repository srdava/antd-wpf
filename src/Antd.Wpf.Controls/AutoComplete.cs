using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Tokens;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd AutoComplete: a text field that offers filtered suggestions as you type.
    /// Unlike <see cref="Select"/> the value is free text, so it cannot be a ComboBox; the
    /// suggestion list is a ListBox so keyboard navigation and selection come from the framework.
    /// </summary>
    public class AutoComplete : Control
    {
        private TextBox _editor;
        private Popup _popup;
        private ListBox _list;
        private bool _updatingText;

        static AutoComplete()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoComplete), new FrameworkPropertyMetadata(typeof(AutoComplete)));
        }

        public AutoComplete()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(AutoComplete),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(AutoComplete),
            new FrameworkPropertyMetadata(null, OnSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(AutoComplete),
            new FrameworkPropertyMetadata(string.Empty, OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register(
            "FilterMode", typeof(AntdFilterMode), typeof(AutoComplete),
            new FrameworkPropertyMetadata(AntdFilterMode.Contains));

        /// <summary>How typed text narrows <see cref="ItemsSource"/>. None shows every option.</summary>
        public AntdFilterMode FilterMode
        {
            get { return (AntdFilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
            "MaxDropDownHeight", typeof(double), typeof(AutoComplete),
            new FrameworkPropertyMetadata(256.0));

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(AutoComplete),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(AutoComplete),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey HasSuggestionsKey = DependencyProperty.RegisterReadOnly(
            "HasSuggestions", typeof(bool), typeof(AutoComplete), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasSuggestionsProperty = HasSuggestionsKey.DependencyProperty;

        public bool HasSuggestions
        {
            get { return (bool)GetValue(HasSuggestionsProperty); }
            private set { SetValue(HasSuggestionsKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(AutoComplete), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_editor != null) _editor.TextChanged -= OnEditorTextChanged;
            if (_popup != null) _popup.Closed -= OnPopupClosed;

            _editor = GetTemplateChild("PART_EditableTextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _list = GetTemplateChild("PART_Suggestions") as ListBox;

            if (_editor != null)
            {
                _editor.TextChanged += OnEditorTextChanged;
                _editor.PreviewKeyDown += OnEditorKeyDown;
                _editor.GotKeyboardFocus += OnEditorFocusChanged;
                _editor.LostKeyboardFocus += OnEditorFocusChanged;

                _updatingText = true;
                _editor.Text = Text ?? string.Empty;
                _updatingText = false;
            }

            if (_popup != null) _popup.Closed += OnPopupClosed;
            if (_list != null) _list.MouseDoubleClick += OnListDoubleClick;

            Refresh();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var complete = (AutoComplete)d;
            complete.PushTextToEditor();
            complete.Refresh();
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoComplete)d).Refresh();
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var complete = (AutoComplete)d;
            if (complete._popup != null) complete._popup.IsOpen = complete.IsDropDownOpen;
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoComplete)d).UpdateVisualState();
        }

        private void PushTextToEditor()
        {
            if (_editor == null || _editor.Text == (Text ?? string.Empty)) return;

            _updatingText = true;
            _editor.Text = Text ?? string.Empty;
            _updatingText = false;
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingText) return;

            _updatingText = true;
            SetCurrentValue(TextProperty, _editor.Text);
            _updatingText = false;

            Refresh();
        }

        private void OnEditorFocusChanged(object sender, RoutedEventArgs e)
        {
            SetValue(IsFocusedStateKey, _editor != null && _editor.IsKeyboardFocusWithin);

            // Gaining focus opens the suggestion list (provided there is something to show) and
            // losing it closes the list, which is what antd does.
            UpdateVisualState();
            Refresh();
        }

        private static readonly DependencyPropertyKey IsFocusedStateKey = DependencyProperty.RegisterReadOnly(
            "IsFocusedState", typeof(bool), typeof(AutoComplete), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsFocusedStateProperty = IsFocusedStateKey.DependencyProperty;

        public bool IsFocusedState
        {
            get { return (bool)GetValue(IsFocusedStateProperty); }
            private set { SetValue(IsFocusedStateKey, value); }
        }

        private void Refresh()
        {
            SetValue(ShowPlaceholderKey, !string.IsNullOrEmpty(Placeholder) && string.IsNullOrEmpty(Text));

            List<object> matches = Filter();
            if (_list != null)
            {
                _list.ItemsSource = matches;
                _list.SelectedIndex = matches.Count > 0 ? 0 : -1;
            }

            SetValue(HasSuggestionsKey, matches.Count > 0);
            SetCurrentValue(IsDropDownOpenProperty, matches.Count > 0 && _editor != null && _editor.IsKeyboardFocusWithin);
        }

        private List<object> Filter()
        {
            var result = new List<object>();
            IEnumerable source = ItemsSource;
            if (source == null) return result;

            string query = (Text ?? string.Empty).Trim();
            foreach (object item in source)
            {
                string label = item == null ? string.Empty : item.ToString();
                if (query.Length == 0
                    || FilterMode == AntdFilterMode.None
                    || label.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (_list == null) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (!IsDropDownOpen && HasSuggestions) SetCurrentValue(IsDropDownOpenProperty, true);
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
                    if (IsDropDownOpen) CommitSelection();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    SetCurrentValue(IsDropDownOpenProperty, false);
                    e.Handled = true;
                    break;
            }
        }

        private void OnListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommitSelection();
        }

        private void CommitSelection()
        {
            if (_list == null || _list.SelectedItem == null) return;

            string value = _list.SelectedItem.ToString();
            SetCurrentValue(TextProperty, value);
            SetCurrentValue(IsDropDownOpenProperty, false);
            if (_editor != null) _editor.CaretIndex = _editor.Text.Length;
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
                : (focused ? "colorPrimary" : "colorBorder");

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(t.BorderRadius));
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(MinHeightProperty, t.ControlHeight);
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(AntdSize.Middle), 0,
                Metrics.InputPaddingInline(AntdSize.Middle), 0));
        }
    }
}
