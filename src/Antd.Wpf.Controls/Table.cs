using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

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
            "EmptyText", typeof(string), typeof(Table), new FrameworkPropertyMetadata("No data"));

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
