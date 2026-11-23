using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Select. Built on ComboBox so ItemsSource, selection, keyboard handling and
    /// popup placement come from the framework; only the visual tree is antd's.
    /// Multi-select and remote search are not covered yet (see M2 notes).
    /// </summary>
    public class Select : ComboBox
    {
        static Select()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Select), new FrameworkPropertyMetadata(typeof(Select)));
        }

        private readonly DependencyPropertyDescriptor _openDescriptor;

        public Select()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();

            // DropDownClosed is not raised when the dropdown is closed programmatically, so the
            // filter is reset from the property itself. Same trick Button uses for IsPressed.
            _openDescriptor = DependencyPropertyDescriptor.FromProperty(IsDropDownOpenProperty, typeof(Select));
            _openDescriptor.AddValueChanged(this, OnOpenStateChanged);

            var selected = new ObservableCollection<object>();
            selected.CollectionChanged += OnSelectedItemsChanged;
            SetValue(SelectedItemsKey, selected);
        }

        private void OnOpenStateChanged(object sender, EventArgs e)
        {
            if (!IsDropDownOpen) ResetSearch();
        }

        private void ResetSearch()
        {
            if (_searchBox != null && _searchBox.Text.Length > 0) _searchBox.Text = string.Empty;
            ApplyFilter(string.Empty);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(CornerRadius), typeof(Select),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

        /// <summary>
        /// Same radius the Input uses. Driving it from a DP (instead of a markup extension in
        /// the template) keeps every bordered field on one code path, so they cannot drift apart.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(AntdSize), typeof(Select),
            new FrameworkPropertyMetadata(AntdSize.Middle, OnAppearanceChanged));

        public AntdSize Size
        {
            get { return (AntdSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(AntdStatus), typeof(Select),
            new FrameworkPropertyMetadata(AntdStatus.None, OnAppearanceChanged));

        public AntdStatus Status
        {
            get { return (AntdStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(Select),
            new FrameworkPropertyMetadata("Please select"));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        public bool AllowClear
        {
            get { return (bool)GetValue(AllowClearProperty); }
            set { SetValue(AllowClearProperty, value); }
        }

        private static readonly DependencyPropertyKey HasSelectionKey = DependencyProperty.RegisterReadOnly(
            "HasSelection", typeof(bool), typeof(Select), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasSelectionProperty = HasSelectionKey.DependencyProperty;

        public bool HasSelection
        {
            get { return (bool)GetValue(HasSelectionProperty); }
            private set { SetValue(HasSelectionKey, value); }
        }

        private static readonly DependencyPropertyKey CanClearSelectionKey = DependencyProperty.RegisterReadOnly(
            "CanClearSelection", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CanClearSelectionProperty = CanClearSelectionKey.DependencyProperty;

        public bool CanClearSelection
        {
            get { return (bool)GetValue(CanClearSelectionProperty); }
            private set { SetValue(CanClearSelectionKey, value); }
        }

        private static readonly DependencyPropertyKey PlaceholderBrushKey = DependencyProperty.RegisterReadOnly(
            "PlaceholderBrush", typeof(Brush), typeof(Select), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty PlaceholderBrushProperty = PlaceholderBrushKey.DependencyProperty;

        public Brush PlaceholderBrush
        {
            get { return (Brush)GetValue(PlaceholderBrushProperty); }
            private set { SetValue(PlaceholderBrushKey, value); }
        }

        private static readonly DependencyPropertyKey ArrowBrushKey = DependencyProperty.RegisterReadOnly(
            "ArrowBrush", typeof(Brush), typeof(Select), new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ArrowBrushProperty = ArrowBrushKey.DependencyProperty;

        public Brush ArrowBrush
        {
            get { return (Brush)GetValue(ArrowBrushProperty); }
            private set { SetValue(ArrowBrushKey, value); }
        }

        public static readonly DependencyProperty FilterableProperty = DependencyProperty.Register(
            "Filterable", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false, OnAppearanceChanged));

        /// <summary>
        /// antd's showSearch: a search box at the top of the dropdown filters the items by their
        /// display text (or <see cref="System.Windows.Controls.TextSearch.TextPath"/> when set).
        /// </summary>
        public bool Filterable
        {
            get { return (bool)GetValue(FilterableProperty); }
            set { SetValue(FilterableProperty, value); }
        }

        public static readonly DependencyProperty IsMultipleProperty = DependencyProperty.Register(
            "IsMultiple", typeof(bool), typeof(Select),
            new FrameworkPropertyMetadata(false, OnMultipleChanged));

        /// <summary>antd's mode="multiple": picks accumulate as tags and the dropdown stays open.</summary>
        public bool IsMultiple
        {
            get { return (bool)GetValue(IsMultipleProperty); }
            set { SetValue(IsMultipleProperty, value); }
        }

        public static readonly DependencyProperty MaxTagCountProperty = DependencyProperty.Register(
            "MaxTagCount", typeof(int), typeof(Select),
            new FrameworkPropertyMetadata(0, OnMultipleChanged));

        /// <summary>How many tags stay visible before folding into "+N". 0 shows them all.</summary>
        public int MaxTagCount
        {
            get { return (int)GetValue(MaxTagCountProperty); }
            set { SetValue(MaxTagCountProperty, value); }
        }

        private static readonly DependencyPropertyKey SelectedItemsKey = DependencyProperty.RegisterReadOnly(
            "SelectedItems", typeof(ObservableCollection<object>), typeof(Select),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsKey.DependencyProperty;

        /// <summary>Tags currently selected in multiple mode, in click order.</summary>
        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
        }

        private System.Windows.Controls.Panel _tagHost;
        private string _tagSignature;
        private bool _suppressSelection;
        public static readonly DependencyProperty FilterLocallyProperty = DependencyProperty.Register(
            "FilterLocally", typeof(bool), typeof(Select), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// When false the search box only reports text: the host listens to
        /// <see cref="SearchTextChanged"/> and swaps ItemsSource itself (remote search).
        /// </summary>
        public bool FilterLocally
        {
            get { return (bool)GetValue(FilterLocallyProperty); }
            set { SetValue(FilterLocallyProperty, value); }
        }

        /// <summary>Raised on every keystroke in the search box (remote search hook).</summary>
        public event EventHandler<string> SearchTextChanged;
        private System.Windows.Controls.TextBox _searchBox;
        private FrameworkElement _searchHost;
        private System.Windows.Controls.TextBlock _searchWatermark;
        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Select)d).UpdateVisualState();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            // Programmatic or keyboard selection still lands in SelectedItem; in multiple mode it
            // becomes a tag and the single-selection slot is cleared again.
            if (IsMultiple && !_suppressSelection && SelectedItem != null)
            {
                object picked = SelectedItem;
                _suppressSelection = true;
                SetCurrentValue(SelectedIndexProperty, -1);
                _suppressSelection = false;

                if (!SelectedItems.Contains(picked)) SelectedItems.Add(picked);
            }

            UpdateVisualState();
        }

        protected override void OnDropDownOpened(System.EventArgs e)
        {
            base.OnDropDownOpened(e);
            UpdateVisualState();

            if (!Filterable || _searchBox == null) return;

            _searchBox.Text = string.Empty;
            Dispatcher.BeginInvoke(
                new Action(delegate { if (IsDropDownOpen) _searchBox.Focus(); }),
                System.Windows.Threading.DispatcherPriority.Input);
        }

        protected override void OnDropDownClosed(System.EventArgs e)
        {
            base.OnDropDownClosed(e);

            if (_searchBox != null) _searchBox.Text = string.Empty;
            ApplyFilter(string.Empty);
        }

        private System.Windows.Controls.Primitives.Popup _popup;

        private void OnPopupClosedReset(object sender, EventArgs e)
        {
            if (_searchBox != null && _searchBox.Text.Length > 0) _searchBox.Text = string.Empty;
            ApplyFilter(string.Empty);
        }
        private static void OnMultipleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var select = (Select)d;

            if (select.IsMultiple && select.SelectedItem != null)
            {
                select.ToggleItem(select.SelectedItem);
            }

            select.RebuildTags();
            select.UpdateVisualState();
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildTags();
            UpdateVisualState();
        }

        private void ToggleItem(object item)
        {
            if (item == null) return;

            if (SelectedItems.Contains(item)) SelectedItems.Remove(item);
            else SelectedItems.Add(item);
        }

        private void RebuildTags()
        {
            if (_tagHost == null) return;

            var snapshot = ThemeManager.Current.Snapshot;
            string signature = SelectedItems.Count + "|" + MaxTagCount + "|" + IsMultiple + "|"
                               + snapshot.GetHashCode() + "|" + Metrics.FontSize(snapshot.Tokens, Size);

            if (signature == _tagSignature) return;
            _tagSignature = signature;

            TagStrip.Build(_tagHost, SelectedItems, MaxTagCount, RemoveTag, snapshot, Metrics.FontSize(snapshot.Tokens, Size));
        }

        private void RemoveTag(object item)
        {
            if (item != null && SelectedItems.Contains(item)) SelectedItems.Remove(item);
        }

        /// <summary>Clicking a row toggles it and keeps the dropdown open, like antd's multiple mode.</summary>
        /// <summary>Hooks the popup content once it exists (Child is not ready during ApplyTemplate).</summary>
        private void OnPopupOpenedHookClicks(object sender, EventArgs e)
        {
            if (_popup == null) return;

            var child = _popup.Child as UIElement;
            if (child == null) return;

            child.PreviewMouseLeftButtonDown -= OnPopupItemMouseDown;
            child.PreviewMouseLeftButtonDown += OnPopupItemMouseDown;
        }

        private void OnPopupItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMultiple) return;

            var container = FindComboBoxItem(e.OriginalSource as DependencyObject);
            if (container == null) return;

            ToggleItem(container.Content ?? container.DataContext);
            e.Handled = true;
        }

        private static ComboBoxItem FindComboBoxItem(DependencyObject node)
        {
            while (node != null)
            {
                var item = node as ComboBoxItem;
                if (item != null) return item;
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            return null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsMultiple)
            {
                if ((e.Key == Key.Enter || e.Key == Key.Space) && IsDropDownOpen)
                {
                    int index = SelectedIndex;
                    if (index >= 0 && index < Items.Count)
                    {
                        ToggleItem(Items[index]);
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.Back && SelectedItems.Count > 0)
                {
                    SelectedItems.RemoveAt(SelectedItems.Count - 1);
                    e.Handled = true;
                    return;
                }
            }

            base.OnKeyDown(e);
        }
        private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter(_searchBox.Text);

            EventHandler<string> handler = SearchTextChanged;
            if (handler != null) handler(this, _searchBox.Text);

            if (_searchWatermark != null)
            {
                _searchWatermark.Visibility = string.IsNullOrEmpty(_searchBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            if (!IsDropDownOpen && !string.IsNullOrEmpty(_searchBox.Text)) IsDropDownOpen = true;
        }

        /// <summary>Keeps keyboard operation while focus sits in the search box.</summary>
        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    MoveSelection(1);
                    break;
                case Key.Up:
                    MoveSelection(-1);
                    break;
                case Key.Enter:
                    SetCurrentValue(IsDropDownOpenProperty, false);
                    break;
                case Key.Escape:
                    _searchBox.Text = string.Empty;
                    SetCurrentValue(IsDropDownOpenProperty, false);
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void MoveSelection(int delta)
        {
            int count = Items.Count;
            if (count == 0) return;

            int next = SelectedIndex + delta;
            if (next < 0) next = 0;
            if (next >= count) next = count - 1;

            SetCurrentValue(SelectedIndexProperty, next);
        }

        private void ApplyFilter(string text)
        {
            if (!Filterable)
            {
                Items.Filter = null;
                return;
            }

            string needle = text == null ? string.Empty : text.Trim();
            if (!FilterLocally) { Items.Filter = null; return; }
            if (needle.Length == 0)
            {
                Items.Filter = null;
                return;
            }

            string path = System.Windows.Controls.TextSearch.GetTextPath(this);
            Items.Filter = delegate(object item)
            {
                string value = DescribeItem(item, path);
                return value != null && value.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };
        }

        private static string DescribeItem(object item, string path)
        {
            if (item == null) return null;
            if (string.IsNullOrEmpty(path)) return item.ToString();

            var property = item.GetType().GetProperty(path);
            if (property == null) return item.ToString();
            object value = property.GetValue(item, null);
            return value == null ? string.Empty : value.ToString();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var popup = AntdMotion.Configure(GetTemplateChild("PART_Popup") as System.Windows.Controls.Primitives.Popup);
            PopupAnchor.Attach(popup, this);

            // The popup closes through several paths (selection, click-away, programmatic). Clearing
            // the filter on the popup itself is the only hook all of them go through.
            if (_popup != null) _popup.Closed -= OnPopupClosedReset;
            _popup = popup;
            if (_popup != null) _popup.Closed += OnPopupClosedReset;

            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpenedHookClicks;
                _popup.Opened += OnPopupOpenedHookClicks;
            }

            _tagHost = GetTemplateChild("PART_TagHost") as System.Windows.Controls.Panel;
            RebuildTags();

            if (_searchBox != null)
            {
                _searchBox.TextChanged -= OnSearchTextChanged;
                _searchBox.PreviewKeyDown -= OnSearchKeyDown;
            }

            _searchHost = GetTemplateChild("SearchHost") as FrameworkElement;
            _searchWatermark = GetTemplateChild("PART_SearchWatermark") as System.Windows.Controls.TextBlock;
            _searchBox = GetTemplateChild("PART_SearchBox") as System.Windows.Controls.TextBox;
            if (_searchBox != null)
            {
                _searchBox.TextChanged += OnSearchTextChanged;
                _searchBox.PreviewKeyDown += OnSearchKeyDown;
                _searchBox.Text = string.Empty;
            }

            ApplyFilter(string.Empty);

            var clear = GetTemplateChild("PART_ClearButton") as System.Windows.Controls.Button;
            if (clear != null)
            {
                clear.Click -= OnClearClicked;
                clear.Click += OnClearClicked;
            }
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            if (IsMultiple)
            {
                SelectedItems.Clear();
                SetCurrentValue(SelectedIndexProperty, -1);
            }
            else
            {
                SetCurrentValue(SelectedIndexProperty, -1);
            }

            e.Handled = true;
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            bool focused = IsKeyboardFocusWithin || IsDropDownOpen;
            string borderKey;
            if (!IsEnabled) borderKey = "colorBorderDisabled";
            else if (Status == AntdStatus.Error) borderKey = "colorError";
            else if (Status == AntdStatus.Warning) borderKey = "colorWarning";
            else if (focused) borderKey = "colorPrimary";
            else if (IsMouseOver) borderKey = "colorPrimaryHover";
            else borderKey = "colorBorder";

            bool hasSelection = IsMultiple ? SelectedItems.Count > 0 : SelectedIndex >= 0;

            SetCurrentValue(BackgroundProperty, snapshot.Brush(IsEnabled ? "colorBgContainer" : "colorBgContainerDisabled"));
            SetCurrentValue(ForegroundProperty, snapshot.Brush(IsEnabled ? "colorText" : "colorTextDisabled"));
            SetCurrentValue(BorderBrushProperty, snapshot.Brush(borderKey));
            SetCurrentValue(BorderThicknessProperty, new Thickness(t.LineWidth));
            SetCurrentValue(FontSizeProperty, Metrics.FontSize(t, Size));
            SetCurrentValue(MinHeightProperty, Metrics.ControlHeight(t, Size));

            if (_searchBox != null)
            {
                _searchBox.FontSize = t.FontSize;
                _searchBox.Foreground = snapshot.Brush("colorText");
                _searchBox.CaretBrush = snapshot.Brush("colorPrimary");
                _searchBox.Background = System.Windows.Media.Brushes.Transparent;
                _searchBox.BorderThickness = new Thickness(0);
                _searchBox.Padding = new Thickness(4, 6, 4, 6);
            }

            if (_searchWatermark != null)
            {
                _searchWatermark.Foreground = snapshot.Brush("colorTextPlaceholder");
                _searchWatermark.FontSize = t.FontSize;
                _searchWatermark.Text = AntdLocale.Current.Search;
            }
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(Metrics.Radius(t, Size)));
            double block = Size == AntdSize.Large ? 6.5 : (Size == AntdSize.Small ? 0 : 4);
            SetCurrentValue(PaddingProperty, new Thickness(Metrics.InputPaddingInline(Size), block,
                Metrics.InputPaddingInline(Size), block));

            RebuildTags();
            SetValue(HasSelectionKey, hasSelection);
            SetValue(CanClearSelectionKey, AllowClear && hasSelection && IsEnabled);
            SetValue(PlaceholderBrushKey, snapshot.Brush("colorTextPlaceholder"));
            SetValue(ArrowBrushKey, snapshot.Brush(IsEnabled ? "colorTextTertiary" : "colorTextDisabled"));
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }
    }
}
