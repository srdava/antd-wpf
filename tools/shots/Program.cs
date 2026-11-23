using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Antd.Wpf.Controls;
using Button = Antd.Wpf.Controls.Button;
using DatePicker = Antd.Wpf.Controls.DatePicker;
using Select = Antd.Wpf.Controls.Select;
using Input = Antd.Wpf.Controls.Input;
using Tag = Antd.Wpf.Controls.Tag;
using Checkbox = Antd.Wpf.Controls.Checkbox;
using Switch = Antd.Wpf.Controls.Switch;
using Progress = Antd.Wpf.Controls.Progress;
using Antd.Wpf.Theme;

namespace Shots
{
    /// <summary>
    /// Renders the README screenshots from the live controls. Run from the repo root:
    ///   dotnet run --project tools/shots
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main()
        {
            var app = new Application();

            var framework = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml", UriKind.Absolute)
            };
            app.Resources.MergedDictionaries.Add(framework);

            ThemeManager.Current.Attach(app.Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            AntdLocale.UseCulture("zh-CN");

            string dir = Path.Combine("docs", "assets");
            Directory.CreateDirectory(dir);

            Render(BuildShowcase(), 1180, 830, Path.Combine(dir, "readme-components.png"));
            Console.WriteLine("shots -> " + dir);
            return 0;
        }

        private static FrameworkElement BuildShowcase()
        {
            ThemeSnapshot theme = ThemeManager.Current.Snapshot;

            var root = new Border
            {
                Width = 1180,
                Height = 830,
                Background = theme.Brush("colorBgLayout"),
                Padding = new Thickness(32)
            };

            var column = new StackPanel { Orientation = Orientation.Vertical };

            column.Children.Add(Section("按钮 Button", Row(
                new Button { Content = "主要按钮", Variant = ButtonVariant.Primary },
                new Button { Content = "默认按钮" },
                new Button { Content = "虚线按钮", Variant = ButtonVariant.Dashed },
                new Button { Content = "文本按钮", Variant = ButtonVariant.Text },
                new Button { Content = "链接按钮", Variant = ButtonVariant.Link },
                new Button { Content = "危险按钮", Danger = true },
                new Button { Content = "大号", Size = AntdSize.Large },
                new Button { Content = "小号", Size = AntdSize.Small }
            )));

            var multi = new Select { Width = 300, IsMultiple = true, MaxTagCount = 2, Placeholder = "多选（+N 折叠）" };
            multi.Items.Add("华东一区");
            multi.Items.Add("华北二区");
            multi.Items.Add("华南三区");
            multi.SelectedItems.Add("华东一区");
            multi.SelectedItems.Add("华北二区");
            multi.SelectedItems.Add("华南三区");

            var range = new DatePicker { Width = 300, IsRange = true, RangeStart = new DateTime(2026, 11, 10), RangeEnd = new DateTime(2026, 11, 23) };

            column.Children.Add(Section("输入 Input · 选择 Select · 日期 DatePicker", Row(
                new Input { Width = 240, Placeholder = "请输入名称", AllowClear = true, Text = "web-01" },
                new Select { Width = 200, Placeholder = "选择状态", SelectedIndex = 1, Items = { "运行中", "已停止", "维护中" } },
                multi,
                range
            )));

            column.Children.Add(Section("数据展示 Tag · Progress · Switch · Checkbox", Row(
                new Tag { Content = "运行中" },
                new Tag { Content = "已停止" },
                new Tag { Content = "高优先级" },
                new Progress { Width = 220, Percent = 68 },
                new Switch { IsChecked = true },
                new Checkbox { Content = "自动刷新", IsChecked = true }
            )));

            column.Children.Add(Section("表格 Table（含固定列与状态列）", BuildTable()));

            root.Child = column;
            return root;
        }

        private static Antd.Wpf.Controls.Table BuildTable()
        {
            var table = new Antd.Wpf.Controls.Table
            {
                Width = 800,
                Height = 226,
                HorizontalAlignment = HorizontalAlignment.Left,
                CanUserSortColumns = false,
                IsHitTestVisible = false
            };

            table.Columns.Add(new DataGridTextColumn
            {
                Header = "实例",
                Binding = new System.Windows.Data.Binding("Name"),
                Width = 200
            });
            table.Columns.Add(new DataGridTextColumn
            {
                Header = "状态",
                Binding = new System.Windows.Data.Binding("State"),
                Width = 140
            });
            table.Columns.Add(new DataGridTextColumn
            {
                Header = "可用区",
                Binding = new System.Windows.Data.Binding("Zone"),
                Width = 180
            });
            table.Columns.Add(new DataGridTextColumn
            {
                Header = "创建时间",
                Binding = new System.Windows.Data.Binding("Created"),
                Width = 260
            });

            var rows = new List<RowData>();
            rows.Add(new RowData { Name = "web-01", State = "运行中", Zone = "cn-hangzhou-a", Created = "2026-11-01 09:12" });
            rows.Add(new RowData { Name = "web-02", State = "运行中", Zone = "cn-hangzhou-b", Created = "2026-11-03 14:40" });
            rows.Add(new RowData { Name = "db-01", State = "维护中", Zone = "cn-hangzhou-a", Created = "2026-10-28 20:05" });

            table.ItemsSource = rows;
            return table;
        }

        private sealed class RowData
        {
            public string Name { get; set; }
            public string State { get; set; }
            public string Zone { get; set; }
            public string Created { get; set; }
        }

        private static StackPanel Section(string title, FrameworkElement body)
        {
            ThemeSnapshot theme = ThemeManager.Current.Snapshot;

            var section = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, 22) };

            section.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = theme.Tokens.FontSizeLG,
                FontWeight = FontWeights.SemiBold,
                Foreground = theme.Brush("colorText"),
                Margin = new Thickness(0, 0, 0, 10)
            });

            var card = new Border
            {
                Background = theme.Brush("colorBgContainer"),
                BorderBrush = theme.Brush("colorBorderSecondary"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20)
            };
            card.Child = body;

            section.Children.Add(card);
            return section;
        }

        private static FrameworkElement Row(params FrameworkElement[] children)
        {
            var panel = new WrapPanel { Orientation = Orientation.Horizontal };
            foreach (FrameworkElement child in children)
            {
                child.Margin = new Thickness(0, 0, 12, 8);
                child.VerticalAlignment = VerticalAlignment.Center;
                panel.Children.Add(child);
            }
            return panel;
        }

        private static void Render(FrameworkElement element, int width, int height, string path)
        {
            element.Measure(new Size(width, height));
            element.Arrange(new Rect(0, 0, width, height));
            element.UpdateLayout();
            element.UpdateLayout();

            const double scale = 2.0;
            var bitmap = new RenderTargetBitmap(
                (int)(width * scale), (int)(height * scale), 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            bitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream stream = File.Create(path)) encoder.Save(stream);
            Console.WriteLine("wrote " + path + " (" + (new FileInfo(path).Length / 1024) + " KB)");
        }
    }
}
