using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// Chip strip used by multi-select. Built in code rather than XAML: the number of chips is
    /// dynamic, each one needs a remove affordance, and antd folds the overflow into a "+N" chip.
    /// </summary>
    internal static class TagStrip
    {
        public static void Build(Panel host, ObservableCollection<object> items, int maxTagCount,
            Action<object> onRemove, ThemeSnapshot brushes, double fontSize)
        {
            if (host == null) return;

            host.Children.Clear();
            if (items == null) return;

            int visible = maxTagCount <= 0 ? items.Count : Math.Min(maxTagCount, items.Count);
            for (int i = 0; i < visible; i++)
            {
                host.Children.Add(CreateChip(items[i], onRemove, brushes, fontSize));
            }

            int overflow = items.Count - visible;
            if (overflow > 0)
            {
                host.Children.Add(CreateOverflowChip(overflow, brushes, fontSize));
            }
        }

        private static FrameworkElement CreateChip(object item, Action<object> onRemove,
            ThemeSnapshot brushes, double fontSize)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock
            {
                Text = Describe(item),
                Foreground = brushes.Brush("colorTextHeading"),
                FontSize = fontSize,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 160,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            var close = new TextBlock
            {
                Text = "\u00D7",
                Margin = new Thickness(6, 0, 0, 0),
                Foreground = brushes.Brush("colorTextTertiary"),
                FontSize = fontSize,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            close.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
            {
                e.Handled = true;
                if (onRemove != null) onRemove(item);
            };
            panel.Children.Add(close);

            var chip = new Border
            {
                Background = brushes.Brush("colorFillSecondary"),
                BorderBrush = brushes.Brush("colorBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 0, 6, 0),
                Margin = new Thickness(0, 0, 4, 0),
                Height = fontSize + 8,
                Child = panel
            };
            return chip;
        }

        private static FrameworkElement CreateOverflowChip(int count, ThemeSnapshot brushes, double fontSize)
        {
            return new Border
            {
                Background = brushes.Brush("colorFillSecondary"),
                BorderBrush = brushes.Brush("colorBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 0, 6, 0),
                Margin = new Thickness(0, 0, 4, 0),
                Height = fontSize + 8,
                Child = new TextBlock
                {
                    Text = "+" + count,
                    Foreground = brushes.Brush("colorTextSecondary"),
                    FontSize = fontSize,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        public static string Describe(object item)
        {
            if (item == null) return string.Empty;
            string text = item as string;
            return text ?? item.ToString();
        }
    }
}
