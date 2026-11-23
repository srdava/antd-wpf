using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// First-column cell of a tree table: indent, expander and the item's display text.
    /// Built in code so the Table can create the column without shipping a DataTemplate resource.
    /// </summary>
    internal sealed class TreeCell : Grid
    {
        private readonly TextBlock _chevron;
        private readonly TextBlock _text;

        public TreeCell()
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _chevron = new TextBlock
            {
                Text = "\u25B8",
                Width = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                Visibility = Visibility.Collapsed
            };
            _chevron.MouseLeftButtonUp += OnChevronClick;

            _text = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            SetColumn(_chevron, 0);
            SetColumn(_text, 1);
            Children.Add(_chevron);
            Children.Add(_text);

            DataContextChanged += delegate { Refresh(); };
            Loaded += delegate { Refresh(); };
        }

        private Antd.Wpf.Controls.Table Owner
        {
            get
            {
                DependencyObject node = VisualTreeHelper.GetParent(this);
                while (node != null)
                {
                    var table = node as Antd.Wpf.Controls.Table;
                    if (table != null) return table;
                    node = VisualTreeHelper.GetParent(node);
                }
                return null;
            }
        }

        private void Refresh()
        {
            Antd.Wpf.Controls.Table table = Owner;
            object item = DataContext;

            if (table == null || item == null)
            {
                _text.Text = item == null ? string.Empty : item.ToString();
                return;
            }

            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            int level = table.GetTreeLevel(item);
            bool hasChildren = table.TreeHasChildren(item);
            bool expanded = table.IsTreeNodeExpanded(item);

            Margin = new Thickness(level * table.TreeIndent, 0, 0, 0);
            _chevron.Visibility = hasChildren ? Visibility.Visible : Visibility.Collapsed;
            _chevron.Text = expanded ? "\u25BE" : "\u25B8";
            _chevron.Foreground = snapshot.Brush("colorTextTertiary");
            _text.Text = table.GetTreeText(item);
            _text.Foreground = snapshot.Brush("colorText");
        }

        private void OnChevronClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            Antd.Wpf.Controls.Table table = Owner;
            if (table != null) table.ToggleNode(DataContext);
        }
    }
}
