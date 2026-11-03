using System;
using System.Windows;
using Antd.Wpf.Theme;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// Keeps a control in sync with the active theme. Subscribes on Loaded and releases on
    /// Unloaded so no control leaks the static theme event.
    /// </summary>
    internal static class ThemeLink
    {
        public static void Attach(FrameworkElement element, Action onThemeChanged)
        {
            if (element == null) throw new ArgumentNullException("element");
            if (onThemeChanged == null) throw new ArgumentNullException("onThemeChanged");

            EventHandler themeHandler = delegate { onThemeChanged(); };

            RoutedEventHandler loadedHandler = null;
            RoutedEventHandler unloadedHandler = null;

            loadedHandler = delegate
            {
                ThemeManager.Current.ThemeChanged += themeHandler;
                onThemeChanged();
            };

            unloadedHandler = delegate
            {
                ThemeManager.Current.ThemeChanged -= themeHandler;
            };

            element.Loaded += loadedHandler;
            element.Unloaded += unloadedHandler;
        }
    }
}
