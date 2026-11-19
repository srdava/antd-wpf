using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;

namespace Antd.Wpf.Controls
{
    /// <summary>One row of a <see cref="Transfer"/>.</summary>
    public class TransferItem
    {
        public TransferItem()
        {
        }

        public TransferItem(string key, string title)
        {
            Key = key;
            Title = title;
        }

        public string Key { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// antd Transfer: two lists with buttons in between.
    /// Items live in the source list until they are moved; <see cref="TargetKeys"/> mirrors the
    /// right hand side so the selection can be persisted or bound.
    /// </summary>
    public class Transfer : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.List, "Transfer");
        }
        private ListBox _source;
        private ListBox _target;

        static Transfer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Transfer), new FrameworkPropertyMetadata(typeof(Transfer)));
        }

        public Transfer()
        {
            SourceItems = new ObservableCollection<TransferItem>();
            TargetItems = new ObservableCollection<TransferItem>();
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public ObservableCollection<TransferItem> SourceItems { get; private set; }
        public ObservableCollection<TransferItem> TargetItems { get; private set; }

        /// <summary>Raised whenever items move between the two sides.</summary>
        public event EventHandler Changed;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(Transfer),
            new FrameworkPropertyMetadata(null, OnItemsChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SourceTitleProperty = DependencyProperty.Register(
            "SourceTitle", typeof(string), typeof(Transfer), new FrameworkPropertyMetadata("Source"));

        public string SourceTitle
        {
            get { return (string)GetValue(SourceTitleProperty); }
            set { SetValue(SourceTitleProperty, value); }
        }

        public static readonly DependencyProperty TargetTitleProperty = DependencyProperty.Register(
            "TargetTitle", typeof(string), typeof(Transfer), new FrameworkPropertyMetadata("Target"));

        public string TargetTitle
        {
            get { return (string)GetValue(TargetTitleProperty); }
            set { SetValue(TargetTitleProperty, value); }
        }

        private static readonly DependencyPropertyKey SourceCountKey = DependencyProperty.RegisterReadOnly(
            "SourceCount", typeof(string), typeof(Transfer), new FrameworkPropertyMetadata("0 items"));

        public static readonly DependencyProperty SourceCountProperty = SourceCountKey.DependencyProperty;

        public string SourceCount
        {
            get { return (string)GetValue(SourceCountProperty); }
            private set { SetValue(SourceCountKey, value); }
        }

        private static readonly DependencyPropertyKey TargetCountKey = DependencyProperty.RegisterReadOnly(
            "TargetCount", typeof(string), typeof(Transfer), new FrameworkPropertyMetadata("0 items"));

        public static readonly DependencyProperty TargetCountProperty = TargetCountKey.DependencyProperty;

        public string TargetCount
        {
            get { return (string)GetValue(TargetCountProperty); }
            private set { SetValue(TargetCountKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _source = GetTemplateChild("PART_SourceList") as ListBox;
            _target = GetTemplateChild("PART_TargetList") as ListBox;
            if (_source != null) _source.ItemsSource = SourceItems;
            if (_target != null) _target.ItemsSource = TargetItems;
            Rebuild();
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Transfer)d).Rebuild();
        }

        private void Rebuild()
        {
            SourceItems.Clear();
            TargetItems.Clear();

            IEnumerable source = ItemsSource;
            if (source != null)
            {
                foreach (object entry in source)
                {
                    var item = entry as TransferItem;
                    if (item != null) SourceItems.Add(item);
                }
            }

            UpdateCounts();
        }

        /// <summary>Moves the given rows from the left list to the right one.</summary>
        public void MoveToTarget(IList selected)
        {
            if (selected == null) return;

            var moving = new List<TransferItem>();
            foreach (object entry in selected)
            {
                var item = entry as TransferItem;
                if (item != null) moving.Add(item);
            }

            foreach (TransferItem item in moving)
            {
                SourceItems.Remove(item);
                TargetItems.Add(item);
            }

            UpdateCounts();
            if (moving.Count > 0) RaiseChanged();
        }

        /// <summary>Moves the given rows from the right list back to the left one.</summary>
        public void MoveToSource(IList selected)
        {
            if (selected == null) return;

            var moving = new List<TransferItem>();
            foreach (object entry in selected)
            {
                var item = entry as TransferItem;
                if (item != null) moving.Add(item);
            }

            foreach (TransferItem item in moving)
            {
                TargetItems.Remove(item);
                SourceItems.Add(item);
            }

            UpdateCounts();
            if (moving.Count > 0) RaiseChanged();
        }

        /// <summary>Convenience wrappers used by the arrow buttons.</summary>
        internal void MoveSelectedToTarget()
        {
            if (_source != null) MoveToTarget(_source.SelectedItems);
        }

        internal void MoveSelectedToSource()
        {
            if (_target != null) MoveToSource(_target.SelectedItems);
        }

        private void UpdateCounts()
        {
            SourceCount = SourceItems.Count + (SourceItems.Count == 1 ? " item" : " items");
            TargetCount = TargetItems.Count + (TargetItems.Count == 1 ? " item" : " items");
        }

        private void RaiseChanged()
        {
            EventHandler handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void UpdateVisualState()
        {
            SetCurrentValue(FontSizeProperty, Antd.Wpf.Theme.ThemeManager.Current.Snapshot.Tokens.FontSize);
        }
    }

    /// <summary>
    /// Arrow button inside a Transfer. A generic.xaml template cannot declare event handlers, so
    /// the button walks up to its owning Transfer and performs the move itself.
    /// </summary>
    public class TransferMoveButton : Button
    {
        static TransferMoveButton()
        {
            // Its own style: the antd Button chrome (15px inline padding, MinHeight 32) would
            // squeeze a 28px square down to nothing and clip the arrow glyph.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransferMoveButton),
                new FrameworkPropertyMetadata(typeof(TransferMoveButton)));
        }

        public static readonly DependencyProperty ToTargetProperty = DependencyProperty.Register(
            "ToTarget", typeof(bool), typeof(TransferMoveButton), new FrameworkPropertyMetadata(true));

        public bool ToTarget
        {
            get { return (bool)GetValue(ToTargetProperty); }
            set { SetValue(ToTargetProperty, value); }
        }

        public TransferMoveButton()
        {
            Click += OnClick;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            DependencyObject node = this;
            while (node != null)
            {
                var transfer = node as Transfer;
                if (transfer != null)
                {
                    if (ToTarget) transfer.MoveSelectedToTarget();
                    else transfer.MoveSelectedToSource();
                    break;
                }
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            e.Handled = true;
        }
    }
}
