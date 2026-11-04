using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Tree. Built on TreeView so expansion, keyboard navigation and selection come from the
    /// framework; the antd look lives in the TreeViewItem template and the item data template.
    /// </summary>
    public class Tree : TreeView
    {
        static Tree()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tree), new FrameworkPropertyMetadata(typeof(Tree)));
        }

        public static readonly DependencyProperty CheckableProperty = DependencyProperty.Register(
            "Checkable", typeof(bool), typeof(Tree),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Shows a tri-state check box on every node.</summary>
        public bool Checkable
        {
            get { return (bool)GetValue(CheckableProperty); }
            set { SetValue(CheckableProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            "ShowIcon", typeof(bool), typeof(Tree), new FrameworkPropertyMetadata(false));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        /// <summary>Nodes whose check box is fully checked, in document order.</summary>
        public IEnumerable<TreeItem> GetCheckedItems()
        {
            var result = new List<TreeItem>();
            CollectChecked(Items, result);
            return result;
        }

        private static void CollectChecked(ItemCollection items, List<TreeItem> result)
        {
            foreach (object entry in items)
            {
                var node = entry as TreeItem;
                if (node == null) continue;
                if (node.IsChecked == true) result.Add(node);
                CollectChecked(node.Children, result);
            }
        }

        private static void CollectChecked(IEnumerable<TreeItem> nodes, List<TreeItem> result)
        {
            foreach (TreeItem node in nodes)
            {
                if (node.IsChecked == true) result.Add(node);
                CollectChecked(node.Children, result);
            }
        }

        /// <summary>Expands or collapses every node that has children.</summary>
        public void ExpandAll(bool expanded)
        {
            foreach (object entry in Items)
            {
                SetExpanded(entry as TreeItem, expanded);
            }
        }

        private static void SetExpanded(TreeItem node, bool expanded)
        {
            if (node == null) return;
            if (node.HasChildren) node.IsExpanded = expanded;
            foreach (TreeItem child in node.Children) SetExpanded(child, expanded);
        }
    }
}
