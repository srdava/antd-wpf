using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Table.
    ///
    /// WPF already ships a virtualising, sortable, resizable grid in DataGrid, so this control
    /// adapts it instead of rebuilding one: the columns API stays DataGrid's, and the antd look
    /// (header fill, hairline borders, hover and selection colours) lives in the item styles.
    /// What is not here yet: fixed columns, tree rows, expandable rows and editing.
    /// </summary>
    public class Table : DataGrid
    {
        static Table()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Table), new FrameworkPropertyMetadata(typeof(Table)));
        }

        public Table()
        {
            AutoGenerateColumns = false;
            CanUserAddRows = false;
            CanUserDeleteRows = false;
            CanUserResizeRows = false;
            HeadersVisibility = DataGridHeadersVisibility.Column;
            GridLinesVisibility = DataGridGridLinesVisibility.None;
            SelectionMode = DataGridSelectionMode.Single;
            SelectionUnit = DataGridSelectionUnit.FullRow;
            RowHeaderWidth = 0;
            IsReadOnly = true;

            ApplyLocale();
            LocaleLink.Attach(this, ApplyLocale);
            HookTreeSource();


        }

        private string _localizedEmpty;

        /// <summary>Only overwrites the placeholder while the host has not supplied its own.</summary>
        private void ApplyLocale()
        {
            string text = AntdLocale.Current.NoData;
            if (EmptyText == null || EmptyText == _localizedEmpty)
            {
                SetCurrentValue(EmptyTextProperty, text);
            }
            _localizedEmpty = text;
        }
        public static readonly DependencyProperty ShowRowDetailsProperty = DependencyProperty.Register(
            "ShowRowDetails", typeof(bool), typeof(Table),
            new FrameworkPropertyMetadata(false, OnShowRowDetailsChanged));

        /// <summary>
        /// antd's expandable row: shows <see cref="System.Windows.Controls.DataGrid.RowDetailsTemplate"/>
        /// under every row. DataGrid keeps the per-row visibility state, this just drives the mode.
        /// </summary>
        public bool ShowRowDetails
        {
            get { return (bool)GetValue(ShowRowDetailsProperty); }
            set { SetValue(ShowRowDetailsProperty, value); }
        }

        public static readonly DependencyProperty TreeChildrenPathProperty = DependencyProperty.Register(
            "TreeChildrenPath", typeof(string), typeof(Table),
            new FrameworkPropertyMetadata(null, OnTreeChanged));

        /// <summary>
        /// Property that returns each row's children. Setting it turns the table into a tree table:
        /// the rows are flattened into a display list, the first column gets an expander, and
        /// collapsed subtrees are filtered out.
        /// </summary>
        public string TreeChildrenPath
        {
            get { return (string)GetValue(TreeChildrenPathProperty); }
            set { SetValue(TreeChildrenPathProperty, value); }
        }

        public static readonly DependencyProperty TreeTextPathProperty = DependencyProperty.Register(
            "TreeTextPath", typeof(string), typeof(Table),
            new FrameworkPropertyMetadata(null, OnTreeChanged));

        /// <summary>Property used for the tree cell label; falls back to ToString().</summary>
        public string TreeTextPath
        {
            get { return (string)GetValue(TreeTextPathProperty); }
            set { SetValue(TreeTextPathProperty, value); }
        }

        public static readonly DependencyProperty TreeIndentProperty = DependencyProperty.Register(
            "TreeIndent", typeof(double), typeof(Table),
            new FrameworkPropertyMetadata(16.0, OnTreeChanged));

        /// <summary>Pixels of indent per level.</summary>
        public double TreeIndent
        {
            get { return (double)GetValue(TreeIndentProperty); }
            set { SetValue(TreeIndentProperty, value); }
        }

        private readonly Dictionary<object, object> _treeParent = new Dictionary<object, object>();
        private readonly Dictionary<object, int> _treeLevel = new Dictionary<object, int>();
        private readonly Dictionary<object, int> _treeChildCount = new Dictionary<object, int>();
        private readonly HashSet<object> _treeExpanded = new HashSet<object>();
        private readonly List<object> _flatRows = new List<object>();
        private DataGridTemplateColumn _treeColumn;
        private DependencyPropertyDescriptor _itemsSourceDescriptor;
        private IEnumerable _treeSource;
        private bool _rebuildingTree;
        private ICollectionView _treeView;

        private static void OnTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Table)d).RebuildTree();
        }

        private void HookTreeSource()
        {
            if (_itemsSourceDescriptor != null) return;

            _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(Table));
            _itemsSourceDescriptor.AddValueChanged(this, OnItemsSourceChangedForTree);
        }

        private void OnItemsSourceChangedForTree(object sender, EventArgs e)
        {
            if (_rebuildingTree) return;

            _treeSource = ItemsSource;
            RebuildTree();
        }

        /// <summary>Flattens the hierarchy into the grid's item list and re-applies the filter.</summary>
        internal void RebuildTree()
        {
            if (_rebuildingTree) return;

            _rebuildingTree = true;
            try
            {
                _treeParent.Clear();
                _treeLevel.Clear();
                _treeChildCount.Clear();
                _treeExpanded.Clear();
                _flatRows.Clear();

                if (ItemsSource != _flatRows) _treeSource = ItemsSource;

                if (string.IsNullOrEmpty(TreeChildrenPath) || _treeSource == null)
                {
                    RemoveTreeColumn();
                    Items.Filter = null;
                    if (_treeView != null) { _treeView.Filter = null; _treeView.Refresh(); _treeView = null; }
                    ItemsSource = _treeSource;
                    return;
                }

                foreach (object root in _treeSource) IndexNode(root, null, 0);

                EnsureTreeColumn();
                ItemsSource = _flatRows;

                _treeView = CollectionViewSource.GetDefaultView(_flatRows);
                if (_treeView != null)
                {
                    _treeView.Filter = FilterNode;
                    _treeView.Refresh();
                }
            }
            finally
            {
                _rebuildingTree = false;
            }
        }

        private void IndexNode(object item, object parent, int level)
        {
            if (item == null) return;

            _treeParent[item] = parent;
            _treeLevel[item] = level;
            _flatRows.Add(item);

            List<object> children = GetTreeChildren(item);
            _treeChildCount[item] = children.Count;

            for (int i = 0; i < children.Count; i++) IndexNode(children[i], item, level + 1);
        }

        private List<object> GetTreeChildren(object item)
        {
            var list = new List<object>();
            if (item == null || string.IsNullOrEmpty(TreeChildrenPath)) return list;

            PropertyInfo property = item.GetType().GetProperty(TreeChildrenPath);
            if (property == null) return list;

            var children = property.GetValue(item, null) as IEnumerable;
            if (children == null) return list;

            foreach (object child in children) list.Add(child);
            return list;
        }

        private bool FilterNode(object item)
        {
            object parent;
            if (!_treeParent.TryGetValue(item, out parent) || parent == null) return true;

            object cursor = parent;
            while (cursor != null)
            {
                if (!_treeExpanded.Contains(cursor)) return false;

                object up;
                _treeParent.TryGetValue(cursor, out up);
                cursor = up;
            }
            return true;
        }

        /// <summary>Expands or collapses one node.</summary>
        public void ToggleNode(object item)
        {
            if (item == null || !_treeParent.ContainsKey(item)) return;

            if (_treeExpanded.Contains(item)) _treeExpanded.Remove(item);
            else _treeExpanded.Add(item);

            RefreshTreeView();
        }

        /// <summary>Expands every node so the whole hierarchy is visible.</summary>
        public void ExpandAllNodes()
        {
            foreach (object key in _treeParent.Keys) _treeExpanded.Add(key);
            RefreshTreeView();
        }

        /// <summary>Collapses every node back to the roots.</summary>
        public void CollapseAllNodes()
        {
            _treeExpanded.Clear();
            RefreshTreeView();
        }

        internal bool IsTreeNodeExpanded(object item)
        {
            return _treeExpanded.Contains(item);
        }

        internal int GetTreeLevel(object item)
        {
            int level;
            return _treeLevel.TryGetValue(item, out level) ? level : 0;
        }

        internal bool TreeHasChildren(object item)
        {
            int count;
            return _treeChildCount.TryGetValue(item, out count) && count > 0;
        }

        internal string GetTreeText(object item)
        {
            if (item == null) return string.Empty;
            if (string.IsNullOrEmpty(TreeTextPath)) return item.ToString();

            PropertyInfo property = item.GetType().GetProperty(TreeTextPath);
            if (property == null) return item.ToString();

            object value = property.GetValue(item, null);
            return value == null ? string.Empty : value.ToString();
        }

        private void RefreshTreeView()
        {
            try
            {
                if (_treeView != null) _treeView.Refresh();
                else Items.Refresh();
            }
            catch (InvalidOperationException)
            {
                // No collection view yet; the next rebuild handles it.
            }
        }

        private void EnsureTreeColumn()
        {
            if (_treeColumn != null) return;

#pragma warning disable 618
            var factory = new FrameworkElementFactory(typeof(Internal.TreeCell));
#pragma warning restore 618

            var column = new DataGridTemplateColumn
            {
                Header = string.Empty,
                CanUserSort = false,
                CellTemplate = new DataTemplate { VisualTree = factory }
            };

            _treeColumn = column;
            Columns.Insert(0, column);
        }

        private void RemoveTreeColumn()
        {
            if (_treeColumn == null) return;

            Columns.Remove(_treeColumn);
            _treeColumn = null;
        }
        private static void OnShowRowDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var table = (Table)d;
            table.RowDetailsVisibilityMode = table.ShowRowDetails
                ? DataGridRowDetailsVisibilityMode.Visible
                : DataGridRowDetailsVisibilityMode.Collapsed;
        }
        public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
            "Bordered", typeof(bool), typeof(Table), new FrameworkPropertyMetadata(true));

        /// <summary>Draws the outer and horizontal hairlines, antd's default table.</summary>
        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(
            "EmptyText", typeof(string), typeof(Table), new FrameworkPropertyMetadata(null));

        public string EmptyText
        {
            get { return (string)GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        private static readonly DependencyPropertyKey HasRowsKey = DependencyProperty.RegisterReadOnly(
            "HasRows", typeof(bool), typeof(Table), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasRowsProperty = HasRowsKey.DependencyProperty;

        /// <summary>False when the source is empty; the gallery binds an Empty placeholder to it.</summary>
        public bool HasRows
        {
            get { return (bool)GetValue(HasRowsProperty); }
            private set { SetValue(HasRowsKey, value); }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            OnRowsChanged(this, System.EventArgs.Empty);
        }

        private void OnRowsChanged(object sender, System.EventArgs e)
        {
            UpdateHasRows();
            var notifier = ItemsSource as INotifyCollectionChanged;
            if (notifier != null) notifier.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateHasRows();
        }

        private void UpdateHasRows()
        {
            var items = Items;
            HasRows = items != null && items.Count > 0;
        }
    }
}
