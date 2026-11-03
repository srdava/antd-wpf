using System.Windows;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Gallery
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Current.Attach(Resources);
        }
    }
}
