using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// One node of an antd Tree. Holds the presentation state (expanded / checked) and keeps the
    /// tri-state check box consistent across parents and children the way antd does.
    /// </summary>
    public class TreeItem : INotifyPropertyChanged
    {
        private bool _suppressCascade;
        private bool? _isChecked = false;
        private bool _isExpanded;
        private bool _isEnabled = true;

        public TreeItem()
        {
            Children = new ObservableCollection<TreeItem>();
            Children.CollectionChanged += OnChildrenChanged;
        }

        public TreeItem(object header) : this()
        {
            Header = header;
        }

        public object Header { get; set; }

        /// <summary>Optional glyph shown before the label.</summary>
        public object Icon { get; set; }

        /// <summary>Optional stable identity for lookups.</summary>
        public string Key { get; set; }

        public ObservableCollection<TreeItem> Children { get; private set; }

        /// <summary>Set by the collection so check cascades can walk upwards.</summary>
        public TreeItem Parent { get; internal set; }

        public bool HasChildren
        {
            get { return Children.Count > 0; }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                Raise("IsExpanded");
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                Raise("IsEnabled");
            }
        }

        /// <summary>False / true / null (indeterminate) like antd's checkable tree.</summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                Raise("IsChecked");

                if (_suppressCascade) return;
                CascadeDown(value);
                CascadeUp();
            }
        }

        internal void SetCheckedSilently(bool value)
        {
            _suppressCascade = true;
            IsChecked = value;
            _suppressCascade = false;
        }

        private void CascadeDown(bool? value)
        {
            if (!value.HasValue) return;

            foreach (TreeItem child in Children)
            {
                child._suppressCascade = true;
                child.IsChecked = value;
                child._suppressCascade = false;
                child.CascadeDown(value);
            }
        }

        private void CascadeUp()
        {
            TreeItem parent = Parent;
            if (parent == null) return;

            bool allChecked = true;
            bool noneChecked = true;
            foreach (TreeItem child in parent.Children)
            {
                if (child.IsChecked == true) noneChecked = false;
                else if (child.IsChecked == false) allChecked = false;
                else { allChecked = false; noneChecked = false; }
            }

            bool? next = allChecked ? true : (noneChecked ? (bool?)false : null);

            parent._suppressCascade = true;
            parent.IsChecked = next;
            parent._suppressCascade = false;
            parent.CascadeUp();
        }

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TreeItem child in e.NewItems) child.Parent = this;
            }
            if (e.OldItems != null)
            {
                foreach (TreeItem child in e.OldItems) child.Parent = null;
            }

            Raise("HasChildren");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Raise(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
