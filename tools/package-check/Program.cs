using System;
using System.Linq;
using System.Windows;
using Antd.Wpf.Controls;
using Antd.Wpf.Icons;
using Antd.Wpf.Theme;

namespace Verify
{
    /// <summary>
    /// Installs the packed Antd.Wpf.Controls 1.0.0, loads its resource dictionary through the pack
    /// URI and touches the theme, a control and the icon set. If any of that is missing from the
    /// package, this fails instead of the bug reaching a consumer.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main()
        {
            var app = new Application();

            var framework = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml",
                    UriKind.Absolute)
            };
            app.Resources.MergedDictionaries.Add(framework);

            ThemeManager.Current.Attach(app.Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

            var button = new Button { Content = "ok", Variant = ButtonVariant.Primary };
            var modal = new Modal { Title = "t" };
            var icons = AntdIcons.Names.Count();

            Console.WriteLine("resources: " + app.Resources.MergedDictionaries.Count);
            Console.WriteLine("colorPrimary: " + ThemeManager.Current.Snapshot.Tokens.ColorPrimary);
            Console.WriteLine("button background: " + Describe(button.Background));
            Console.WriteLine("modal ok text: " + modal.OkText);
            Console.WriteLine("icons: " + icons);
            Console.WriteLine("icons star-filled: " + (AntdIcons.ByName("star-filled") != null));

            bool ok = app.Resources.MergedDictionaries.Count >= 1
                      && !string.IsNullOrEmpty(ThemeManager.Current.Snapshot.Tokens.ColorPrimary.ToString())
                      && button.Background != null
                      && icons >= 800
                      && AntdIcons.ByName("star-filled") != null;

            Console.WriteLine(ok ? "VERIFY: PASS" : "VERIFY: FAIL");
            return ok ? 0 : 1;
        }

        private static string Describe(System.Windows.Media.Brush brush)
        {
            var solid = brush as System.Windows.Media.SolidColorBrush;
            return solid == null ? "null" : solid.Color.ToString();
        }
    }
}
