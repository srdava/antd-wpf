using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Antd.Wpf.Controls;
using Button = Antd.Wpf.Controls.Button;
using Select = Antd.Wpf.Controls.Select;
using Antd.Wpf.Theme;

namespace Antd.Wpf.UiHost
{
    /// <summary>
    /// Minimal window the UI automation tests drive. Each target carries an AutomationId so the
    /// tests address it the way a screen reader or an automation client would.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
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

            app.Run(new HostWindow());
        }
    }

    internal sealed class HostWindow : Window
    {
        private readonly TextBlock _result;
        private readonly TextBlock _statusValue;

        public HostWindow()
        {
            Title = "Antd Ui Host";
            Width = 720;
            Height = 520;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var status = new Select { Width = 240, Placeholder = "请选择状态" };
            status.Items.Add("运行中");
            status.Items.Add("已停止");
            AutomationProperties.SetAutomationId(status, "StatusSelect");

            _statusValue = new TextBlock { Text = "none" };
            AutomationProperties.SetAutomationId(_statusValue, "StatusValue");
            status.SelectionChanged += delegate { _statusValue.Text = status.SelectedItem == null ? "none" : status.SelectedItem.ToString(); };

            var confirm = new Button { Content = "删除实例", Variant = ButtonVariant.Primary };
            AutomationProperties.SetAutomationId(confirm, "ConfirmButton");
            confirm.Click += delegate
            {
                Modal.Confirm(this, "删除确认", "确定要删除这条记录吗？", delegate
                {
                    _result.Text = "ok";
                }, delegate { });
            };

            _result = new TextBlock { Text = "idle" };
            AutomationProperties.SetAutomationId(_result, "ResultText");

            var stack = new StackPanel { Margin = new Thickness(24) };
            stack.Children.Add(status);
            stack.Children.Add(new Border { Height = 8 });
            stack.Children.Add(_statusValue);
            stack.Children.Add(new Border { Height = 16 });
            stack.Children.Add(confirm);
            stack.Children.Add(new Border { Height = 16 });
            stack.Children.Add(_result);

            Content = stack;
        }
    }
}
