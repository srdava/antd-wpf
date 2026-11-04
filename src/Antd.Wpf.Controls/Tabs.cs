using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Tabs (line type). Keyboard navigation, selection and content hosting come from
    /// <see cref="TabControl"/>; only the chrome is ours. Measured against real antd: the strip
    /// is 46 tall (12 + 22 + 12 at 14px), labels are 32 apart and the ink bar is 2 thick.
    /// </summary>
    public class Tabs : TabControl
    {
        static Tabs()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tabs), new FrameworkPropertyMetadata(typeof(Tabs)));
        }

        public Tabs()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ItemGutterProperty = DependencyProperty.Register(
            "ItemGutter", typeof(double), typeof(Tabs), new FrameworkPropertyMetadata(32.0));

        /// <summary>Space after each tab label; antd uses 32.</summary>
        public double ItemGutter
        {
            get { return (double)GetValue(ItemGutterProperty); }
            set { SetValue(ItemGutterProperty, value); }
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
        }
    }

    /// <summary>
    /// One tab label. It has to be its own type: a theme dictionary can only restyle the
    /// assembly's own controls, so the stock <see cref="TabItem"/> would keep the system look.
    /// Colours come from the owning <see cref="Tabs"/> via the resource dictionary, so this
    /// class needs no state of its own.
    /// </summary>
    public class TabsItem : TabItem
    {
        static TabsItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabsItem), new FrameworkPropertyMetadata(typeof(TabsItem)));
        }
    }
}
