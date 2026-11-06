using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Integration
{
    /// <summary>
    /// Entry point for the integration sample. Besides the normal window it understands two
    /// switches used by the acceptance pass: --selftest drives the real page and exits with a
    /// non-zero code on failure, --shot renders the window to a PNG.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.Current.Attach(Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

            if (e.Args.Any(a => a == "--dark")) ThemeManager.Current.Apply(ThemeConfig.CreateDark());

            var window = new MainWindow();
            MainWindow = window;
            window.Show();

            bool selfTest = e.Args.Any(a => a == "--selftest");
            string shotPath = ValueOf(e.Args, "--shot");

            if (selfTest)
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    int failures = window.RunSelfTest();
                    Shutdown(failures == 0 ? 0 : 1);
                }), DispatcherPriority.ApplicationIdle);
                return;
            }

            if (e.Args.Any(a => a == "--editor")) window.BeginCreate();

            if (e.Args.Any(a => a == "--toast"))
            {
                Antd.Wpf.Controls.Message.Success("实例已创建：edge-cache");
                Antd.Wpf.Controls.Notification.Info("批量操作完成", "2 个实例已停止。");
            }

            if (shotPath != null)
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    window.CaptureTo(shotPath);
                    Shutdown(0);
                }), DispatcherPriority.ApplicationIdle);
            }
        }

        private static string ValueOf(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            return null;
        }


    }
}
