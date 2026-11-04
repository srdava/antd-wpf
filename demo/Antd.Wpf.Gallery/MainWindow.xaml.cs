using System;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Gallery
{
    public partial class MainWindow : Window
    {
        private static readonly string[] SwatchTokens =
        {
            "colorPrimary", "colorPrimaryHover", "colorPrimaryActive", "colorPrimaryBg", "colorPrimaryBorder",
            "colorSuccess", "colorWarning", "colorError", "colorInfo",
            "colorText", "colorTextSecondary", "colorTextTertiary", "colorTextQuaternary",
            "colorBgContainer", "colorBgLayout", "colorBgElevated",
            "colorBorder", "colorBorderSecondary", "colorFillTertiary"
        };

        public MainWindow()
        {
            InitializeComponent();

            RangeSegmented.ItemsSource = new[] { "Daily", "Weekly", "Monthly" };
            RangeSegmented.SelectedIndex = 0;

            DemoTimePicker.SelectedTime = new TimeSpan(9, 30, 0);
            DemoColorPicker.SelectedColor = System.Windows.Media.Color.FromRgb(0x16, 0x77, 0xFF);
            DemoMentions.ItemsSource = new[]
            {
                "Alice Chen", "Bob Li", "Carol Wang", "David Zhao", "Eve Sun"
            };

            DemoUpload.Files.Add(new Antd.Wpf.Controls.UploadFile { Name = "token-audit.png", Size = 184320, Status = Antd.Wpf.Controls.AntdUploadStatus.Done });
            DemoUpload.Files.Add(new Antd.Wpf.Controls.UploadFile { Name = "spec.pdf", Size = 2400000, Status = Antd.Wpf.Controls.AntdUploadStatus.Uploading, Progress = 0.4 });

            DemoTransfer.ItemsSource = new[]
            {
                new Antd.Wpf.Controls.TransferItem("1", "Colour"),
                new Antd.Wpf.Controls.TransferItem("2", "Typography"),
                new Antd.Wpf.Controls.TransferItem("3", "Spacing"),
                new Antd.Wpf.Controls.TransferItem("4", "Motion"),
                new Antd.Wpf.Controls.TransferItem("5", "Iconography")
            };

            BuildDemoTree();
            BuildDemoTable();
            BuildDemoSelectors();

            FrameworkAuto.ItemsSource = new[]
            {
                "WPF", "WinUI 3", "Avalonia", "MAUI", "Uno Platform", "Avalonia XPF"
            };
            FrameworkAuto.Text = string.Empty;

            DemoCalendar.SelectedDate = DateTime.Today;
            DemoDatePicker.SelectedDate = DateTime.Today.AddDays(3);

            FrameworkSelect.ItemsSource = new[] { "WPF", "Avalonia", "WinUI", "MAUI" };
            FrameworkSelect.SelectedIndex = 0;

            Loaded += delegate { RefreshTokens(); };
            ThemeManager.Current.ThemeChanged += OnThemeChanged;
            Closed += delegate { ThemeManager.Current.ThemeChanged -= OnThemeChanged; };
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            RefreshTokens();
            UpdateThemeButton();
        }

        public sealed class TableRow
        {
            public string Name { get; set; }
            public string Role { get; set; }
            public string Status { get; set; }
            public string Updated { get; set; }
        }

        private void BuildDemoTable()
        {
            DemoTable.ItemsSource = new[]
            {
                new TableRow { Name = "Button", Role = "Atomic", Status = "Stable", Updated = "2 days ago" },
                new TableRow { Name = "Select", Role = "Data entry", Status = "Stable", Updated = "5 days ago" },
                new TableRow { Name = "DatePicker", Role = "Data entry", Status = "Beta", Updated = "yesterday" },
                new TableRow { Name = "Tree", Role = "Navigation", Status = "Beta", Updated = "today" },
                new TableRow { Name = "Table", Role = "Display", Status = "Beta", Updated = "today" },
                new TableRow { Name = "Upload", Role = "Data entry", Status = "Draft", Updated = "3 days ago" }
            };
        }

        private void BuildDemoSelectors()
        {
            var components = new Antd.Wpf.Controls.TreeItem("Components") { IsExpanded = true };
            var entry = new Antd.Wpf.Controls.TreeItem("Data entry") { IsExpanded = true };
            entry.Children.Add(new Antd.Wpf.Controls.TreeItem("Button"));
            entry.Children.Add(new Antd.Wpf.Controls.TreeItem("Input"));
            components.Children.Add(entry);
            components.Children.Add(new Antd.Wpf.Controls.TreeItem("Table"));
            DemoTreeSelect.ItemsSource = new[] { components };
            DemoTreeSelect.SelectedItem = entry.Children[1];

            var zhejiang = new Antd.Wpf.Controls.CascaderNode("Zhejiang");
            var hangzhou = new Antd.Wpf.Controls.CascaderNode("Hangzhou");
            hangzhou.Children.Add(new Antd.Wpf.Controls.CascaderNode("Xihu"));
            hangzhou.Children.Add(new Antd.Wpf.Controls.CascaderNode("Binjiang"));
            zhejiang.Children.Add(hangzhou);
            var jiangsu = new Antd.Wpf.Controls.CascaderNode("Jiangsu");
            var nanjing = new Antd.Wpf.Controls.CascaderNode("Nanjing");
            nanjing.Children.Add(new Antd.Wpf.Controls.CascaderNode("Xuanwu"));
            jiangsu.Children.Add(nanjing);
            DemoCascader.ItemsSource = new[] { zhejiang, jiangsu };
        }

        private void BuildDemoTree()
        {
            var root = new Antd.Wpf.Controls.TreeItem("Design system") { IsExpanded = true };
            var foundations = new Antd.Wpf.Controls.TreeItem("Foundations") { IsExpanded = true };
            foundations.Children.Add(new Antd.Wpf.Controls.TreeItem("Colour"));
            foundations.Children.Add(new Antd.Wpf.Controls.TreeItem("Typography"));
            foundations.Children.Add(new Antd.Wpf.Controls.TreeItem("Spacing"));

            var components = new Antd.Wpf.Controls.TreeItem("Components") { IsExpanded = true };
            var inputs = new Antd.Wpf.Controls.TreeItem("Data entry") { IsExpanded = true };
            inputs.Children.Add(new Antd.Wpf.Controls.TreeItem("Button"));
            inputs.Children.Add(new Antd.Wpf.Controls.TreeItem("Input"));
            inputs.Children.Add(new Antd.Wpf.Controls.TreeItem("DatePicker"));
            components.Children.Add(inputs);
            components.Children.Add(new Antd.Wpf.Controls.TreeItem("Tree"));
            components.Children.Add(new Antd.Wpf.Controls.TreeItem("Table"));

            root.Children.Add(foundations);
            root.Children.Add(components);
            root.Children.Add(new Antd.Wpf.Controls.TreeItem("Changelog"));

            DemoTree.ItemsSource = new[] { root };
        }

        private void OnSubmitForm(object sender, RoutedEventArgs e)
        {
            DemoForm.Validate();
        }

        private void OnResetForm(object sender, RoutedEventArgs e)
        {
            DemoForm.ResetErrors();
            NameInput.Text = string.Empty;
        }

        private void OnToggleTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ToggleDark();
        }

        private void OnResetTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
        }

        private void UpdateThemeButton()
        {
            if (ThemeButton == null) return;
            ThemeButton.Content = ThemeManager.Current.Snapshot.IsDark ? "Switch to light" : "Switch to dark";
        }

        private void RefreshTokens()
        {
            if (TokensPanel == null) return;

            UpdateThemeButton();
            TokensPanel.Children.Clear();

            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken tokens = snapshot.Tokens;

            var grid = new UniformGrid { Columns = 3 };
            foreach (string name in SwatchTokens)
            {
                grid.Children.Add(BuildSwatch(snapshot, tokens, name));
            }
            TokensPanel.Children.Add(grid);

            TokensPanel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Foreground = snapshot.Brush("colorTextSecondary"),
                FontSize = 13,
                Text = string.Format(
                    "fontSize {0} · lineHeight {1:0.###} · controlHeight {2} · borderRadius {3} · lineWidth {4} · algorithm {5}",
                    tokens.FontSize, tokens.LineHeight, tokens.ControlHeight, tokens.BorderRadius, tokens.LineWidth,
                    snapshot.IsDark ? "dark" : "default")
            });
        }

        private static FrameworkElement BuildSwatch(ThemeSnapshot snapshot, AliasToken tokens, string name)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 10) };

            var chip = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = snapshot.Brush("colorBorderSecondary"),
                Background = snapshot.Brush(name),
                VerticalAlignment = VerticalAlignment.Center
            };
            row.Children.Add(chip);

            var text = new StackPanel { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            text.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = snapshot.Brush("colorText")
            });

            AntdColor color = tokens.Color(name);
            text.Children.Add(new TextBlock
            {
                Text = color.Alpha >= 1.0 ? color.ToHex() : color.ToRgbString(),
                FontSize = 11,
                Foreground = snapshot.Brush("colorTextTertiary")
            });

            row.Children.Add(text);
            return row;
        }
    }
}
