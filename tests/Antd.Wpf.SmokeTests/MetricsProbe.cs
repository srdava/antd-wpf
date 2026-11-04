using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Antd.Wpf.Controls;
using Antd.Wpf.Theme;
using AntdButton = Antd.Wpf.Controls.Button;
using AntdInput = Antd.Wpf.Controls.Input;

namespace Antd.Wpf.SmokeTests
{
    /// <summary>
    /// Measures our controls the same way tools/visual-ref measures real antd in Chrome, so the
    /// two metric dumps can be diffed field by field instead of eyeballed from screenshots.
    /// </summary>
    internal static class MetricsProbe
    {
        public static void Run(string path)
        {
            var host = new StackPanel { Width = 814 };
            var items = new List<Tuple<string, FrameworkElement>>();

            Action<string, FrameworkElement> add = (name, element) =>
            {
                element.Margin = new Thickness(0, 0, 0, 8);
                host.Children.Add(element);
                items.Add(Tuple.Create(name, element));
            };

            add("btn-primary", new AntdButton { Content = "Primary", Variant = ButtonVariant.Primary });
            add("btn-default", new AntdButton { Content = "Default" });
            add("btn-dashed", new AntdButton { Content = "Dashed", Variant = ButtonVariant.Dashed });
            add("btn-text", new AntdButton { Content = "Text", Variant = ButtonVariant.Text });
            add("btn-link", new AntdButton { Content = "Link", Variant = ButtonVariant.Link });
            add("btn-primary-danger", new AntdButton { Content = "Primary Danger", Variant = ButtonVariant.Primary, Danger = true });
            add("btn-danger", new AntdButton { Content = "Danger", Danger = true });
            add("btn-disabled", new AntdButton { Content = "Disabled", IsEnabled = false });
            add("btn-large", new AntdButton { Content = "Large", Size = AntdSize.Large });
            add("btn-middle", new AntdButton { Content = "Middle" });
            add("btn-small", new AntdButton { Content = "Small", Size = AntdSize.Small });
            add("btn-block", new AntdButton { Content = "Block button", Block = true });

            add("input-basic", new AntdInput { Width = 220, Placeholder = "Basic input" });
            add("input-error", new AntdInput { Width = 220, Text = "Error state", Status = AntdStatus.Error });
            add("input-large", new AntdInput { Width = 160, Size = AntdSize.Large, Placeholder = "Large" });
            add("input-small", new AntdInput { Width = 160, Size = AntdSize.Small, Placeholder = "Small" });
            add("input-number", new InputNumber { Width = 140, Value = 3, Minimum = 1, Maximum = 20 });
            add("select", new Select { Width = 200, Placeholder = "Pick one" });
            add("autocomplete", new AutoComplete { Width = 200, Placeholder = "Type to filter" });

            add("tag-default", new Tag { Content = "Default" });
            add("tag-processing", new Tag { Content = "Processing", Color = AntdTagColor.Processing });
            add("tag-success", new Tag { Content = "Success", Color = AntdTagColor.Success });
            add("tag-warning", new Tag { Content = "Warning", Color = AntdTagColor.Warning });
            add("tag-error", new Tag { Content = "Error", Color = AntdTagColor.Error });

            add("badge-5", new Badge { Count = 5, Content = new TextBlock { Text = "Inbox", FontSize = 14 } });
            add("avatar-default", new Avatar { Content = "A" });

            add("divider-text", new Divider { Text = "Space sizes" });

            add("progress-30", new Progress { Width = 814, Percent = 30 });

            var segmented = new Segmented { ItemsSource = new[] { "Daily", "Weekly", "Monthly" }, SelectedIndex = 0 };
            add("segmented", segmented);

            add("spin", new Spin { Spinning = true });

            add("statistic", new Statistic { Title = "Active users", Value = 112893 });
            add("title-1", new Title { Text = "Heading 1", Level = 1 });

            var window = new Window
            {
                Width = 900,
                Height = 1400,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Left = -6000,
                Top = 0,
                Content = new ScrollViewer { Content = host }
            };

            window.Show();
            DoEvents();
            window.UpdateLayout();

            var builder = new StringBuilder();
            builder.Append("{\n \"metrics\": [\n");
            for (int i = 0; i < items.Count; i++)
            {
                Tuple<string, FrameworkElement> item = items[i];
                FrameworkElement element = item.Item2;
                builder.Append("  {");
                builder.Append("\"name\": ").Append(Json(item.Item1)).Append(", ");
                builder.Append("\"w\": ").Append(Round(element.ActualWidth)).Append(", ");
                builder.Append("\"h\": ").Append(Round(element.ActualHeight)).Append(", ");
                var control = element as Control;
                builder.Append("\"fontSize\": ").Append(Round(control != null ? control.FontSize : TextElement.GetFontSize(element))).Append(", ");
                builder.Append("\"fontWeight\": ").Append(TextElement.GetFontWeight(element).ToOpenTypeWeight().ToString(CultureInfo.InvariantCulture)).Append(", ");

                if (control != null)
                {
                    builder.Append("\"paddingLeft\": ").Append(Round(control.Padding.Left)).Append(", ");
                    builder.Append("\"paddingRight\": ").Append(Round(control.Padding.Right)).Append(", ");
                    builder.Append("\"borderLeft\": ").Append(Round(control.BorderThickness.Left)).Append(", ");
                }
                else
                {
                    builder.Append("\"paddingLeft\": 0, \"paddingRight\": 0, \"borderLeft\": 0, ");
                }

                builder.Append("\"radius\": ").Append(Json(RadiusOf(element))).Append(", ");
                Brush foreground = control != null ? control.Foreground : TextElement.GetForeground(element);
                Brush background = control != null ? control.Background : null;
                builder.Append("\"foreground\": ").Append(Json(ColorOf(foreground))).Append(", ");
                builder.Append("\"background\": ").Append(Json(ColorOf(background)));
                builder.Append("}");
                if (i < items.Count - 1) builder.Append(",");
                builder.Append("\n");
            }
            builder.Append(" ]\n}\n");

            File.WriteAllText(path, builder.ToString());
            Console.WriteLine("metrics written: " + path + " (" + items.Count + " entries)");
            window.Close();
        }

        private static string RadiusOf(FrameworkElement element)
        {
            var button = element as AntdButton;
            if (button != null) return Round(button.CornerRadius.TopLeft);

            var tag = element as Tag;
            if (tag != null) return Round(tag.TagCornerRadius.TopLeft);

            var avatar = element as Avatar;
            if (avatar != null) return Round(avatar.AvatarCornerRadius.TopLeft);

            var progress = element as Progress;
            if (progress != null) return "-";

            return "-";
        }

        private static string ColorOf(Brush brush)
        {
            var solid = brush as SolidColorBrush;
            if (solid == null) return brush == null ? "none" : brush.ToString();
            Color c = solid.Color;
            // antd reports colours as rgba(), so fold the colour's own alpha channel in.
            double alpha = (c.A / 255.0) * solid.Opacity;
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}@{3:0.##}", c.R, c.G, c.B, alpha);
        }

        private static string Round(double value)
        {
            return Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);
        }

        private static string Json(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate { frame.Continue = false; }));
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }
    }
}
