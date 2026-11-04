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

            FrameworkAuto.ItemsSource = new[]
            {
                "WPF", "WinUI 3", "Avalonia", "MAUI", "Uno Platform", "Avalonia XPF"
            };
            FrameworkAuto.Text = string.Empty;

            DemoCalendar.SelectedDate = DateTime.Today;

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
