using System;
using System.Collections;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd TreeSelect: a select field whose drop down is a tree.
    /// Reuses <see cref="Tree"/> for the hierarchy, so check boxes and keyboard navigation there
    /// apply here too when Checkable is on.
    /// </summary>
    public class TreeSelect : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.ComboBox, "TreeSelect");
        }
        private Border _field;
        private Popup _popup;
        private Tree _tree;
        private Button _clear;

        static TreeSelect()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeSelect), new FrameworkPropertyMetadata(typeof(TreeSelect)));
        }

        public TreeSelect()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(TreeSelect),
            new FrameworkPropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(TreeItem), typeof(TreeSelect),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedChanged));

        public TreeItem SelectedItem
        {
            get { return (TreeItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(TreeSelect),
            new FrameworkPropertyMetadata("Select", OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(TreeSelect),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(TreeSelect),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(TreeSelect),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey DisplayTextKey = DependencyProperty.RegisterReadOnly(
            "DisplayText", typeof(string), typeof(TreeSelect), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            private set { SetValue(DisplayTextKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(TreeSelect), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(TreeSelect), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearProperty = CanClearKey.DependencyProperty;

        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            private set { SetValue(CanClearKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_field != null) _field.MouseLeftButtonUp -= OnFieldClick;
            if (_tree != null) _tree.SelectedItemChanged -= OnTreeSelection;

            _field = GetTemplateChild("PART_Field") as Border;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            PopupAnchor.Attach(_popup, this);
            _tree = GetTemplateChild("PART_Tree") as Tree;
            _clear = GetTemplateChild("PART_ClearButton") as Button;

            if (_field != null) _field.MouseLeftButtonUp += OnFieldClick;
            if (_tree != null)
            {
                _tree.SelectedItemChanged += OnTreeSelection;
                _tree.ItemsSource = ItemsSource;
            }
            if (_clear != null) _clear.Click += OnClearClick;
            if (_popup != null) _popup.Closed += OnPopupClosed;
        }

        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedItemProperty, null);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }

        private void OnTreeSelection(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as TreeItem;
            if (node == null) return;

            SetCurrentValue(SelectedItemProperty, node);
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var select = (TreeSelect)d;
            if (select._popup != null) select._popup.IsOpen = select.IsDropDownOpen;
            select.UpdateVisualState();
        }

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeSelect)d).SyncText();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var select = (TreeSelect)d;
            select.SyncText();
            select.UpdateVisualState();
        }

        private void SyncText()
        {
            TreeItem item = SelectedItem;
            string text = item == null || item.Header == null ? string.Empty : item.Header.ToString();

            SetValue(DisplayTextKey, text);
            SetValue(ShowPlaceholderKey, string.IsNullOrEmpty(text));
            SetValue(CanClearKey, AllowClear && item != null && IsEnabled);
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            string borderKey = !IsEnabled ? "colorBorderDisabled"
                : (IsDropDownOpen ? "colorPrimary" : "colorBorder");

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
