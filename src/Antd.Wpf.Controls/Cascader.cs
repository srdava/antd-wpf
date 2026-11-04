using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>One node of a <see cref="Cascader"/>.</summary>
    public class CascaderNode
    {
        public CascaderNode()
        {
            Children = new ObservableCollection<CascaderNode>();
        }

        public CascaderNode(string label) : this()
        {
            Label = label;
        }

        public string Label { get; set; }
        public object Value { get; set; }
        public ObservableCollection<CascaderNode> Children { get; private set; }

        public override string ToString()
        {
            return Label;
        }
    }

    /// <summary>
    /// antd Cascader: one column per level, each selection opens the next column until a leaf is
    /// picked. Columns are built in code because their count depends on how deep the user goes.
    /// </summary>
    public class Cascader : Control
    {
        private Border _field;
        private Popup _popup;
        private StackPanel _columns;
        private Button _clear;

        static Cascader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cascader), new FrameworkPropertyMetadata(typeof(Cascader)));
        }

        public Cascader()
        {
            SelectedPath = new ObservableCollection<CascaderNode>();
            SelectedPath.CollectionChanged += delegate { SyncText(); };
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public ObservableCollection<CascaderNode> SelectedPath { get; private set; }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(Cascader),
            new FrameworkPropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(Cascader),
            new FrameworkPropertyMetadata("Select", OnAppearanceChanged));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty SeparatorProperty = DependencyProperty.Register(
            "Separator", typeof(string), typeof(Cascader),
            new FrameworkPropertyMetadata(" / ", OnAppearanceChanged));

        public string Separator
        {
            get { return (string)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(Cascader),
            new FrameworkPropertyMetadata(true, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(Cascader),
            new FrameworkPropertyMetadata(false, OnDropDownChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Cascader),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static readonly DependencyPropertyKey DisplayTextKey = DependencyProperty.RegisterReadOnly(
            "DisplayText", typeof(string), typeof(Cascader), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            private set { SetValue(DisplayTextKey, value); }
        }

        private static readonly DependencyPropertyKey ShowPlaceholderKey = DependencyProperty.RegisterReadOnly(
            "ShowPlaceholder", typeof(bool), typeof(Cascader), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderKey.DependencyProperty;

        public bool ShowPlaceholder
        {
            get { return (bool)GetValue(ShowPlaceholderProperty); }
            private set { SetValue(ShowPlaceholderKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearKey = DependencyProperty.RegisterReadOnly(
            "CanClear", typeof(bool), typeof(Cascader), new FrameworkPropertyMetadata(false));

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

            _field = GetTemplateChild("PART_Field") as Border;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            PopupAnchor.Attach(_popup, this);
            _columns = GetTemplateChild("PART_Columns") as StackPanel;
            _clear = GetTemplateChild("PART_ClearButton") as Button;

            if (_field != null) _field.MouseLeftButtonUp += OnFieldClick;
            if (_clear != null) _clear.Click += OnClearClick;
            if (_popup != null) _popup.Closed += OnPopupClosed;

            BuildColumns();
        }

        private void OnFieldClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            SelectedPath.Clear();
            e.Handled = true;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cascader = (Cascader)d;
            if (cascader._popup != null) cascader._popup.IsOpen = cascader.IsDropDownOpen;
            if (cascader.IsDropDownOpen) cascader.BuildColumns();
            cascader.UpdateVisualState();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cascader = (Cascader)d;
            cascader.SyncText();
            cascader.UpdateVisualState();
        }

        /// <summary>Rebuilds the column stack from the first level down to the last picked node.</summary>
        private void BuildColumns()
        {
            if (_columns == null) return;

            _columns.Children.Clear();
            AddColumn(ItemsSource, 0);

            for (int level = 0; level < SelectedPath.Count; level++)
            {
                CascaderNode node = SelectedPath[level];
                if (node.Children.Count == 0) break;
                AddColumn(node.Children, level + 1);
            }
        }

        private void AddColumn(IEnumerable source, int level)
        {
            var list = new ListBox
            {
                Width = 132,
                Height = 190,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, level == 0 ? 0 : 1, 0),
                ItemsSource = source
            };

            if (level < SelectedPath.Count) list.SelectedItem = SelectedPath[level];

            list.SelectionChanged += delegate
            {
                var node = list.SelectedItem as CascaderNode;
                if (node == null) return;
                OnNodePicked(node, level);
            };

            _columns.Children.Add(list);
        }

        private void OnNodePicked(CascaderNode node, int level)
        {
            while (SelectedPath.Count > level) SelectedPath.RemoveAt(SelectedPath.Count - 1);
            SelectedPath.Add(node);

            if (node.Children.Count > 0)
            {
                BuildColumns();
                return;
            }

            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private void SyncText()
        {
            var parts = new List<string>();
            foreach (CascaderNode node in SelectedPath) parts.Add(node.Label);

            string text = string.Join(Separator ?? " / ", parts.ToArray());
            SetValue(DisplayTextKey, text);
            SetValue(ShowPlaceholderKey, string.IsNullOrEmpty(text));
            SetValue(CanClearKey, AllowClear && parts.Count > 0 && IsEnabled);
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
