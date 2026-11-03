using System;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Antd.Wpf.Theme.Markup
{
    /// <summary>
    /// <c>{antd:Brush colorPrimary}</c> - frozen brush for a color token. Cheaper than
    /// DynamicResource when the value is consumed by an element property rather than a setter.
    /// </summary>
    [MarkupExtensionReturnType(typeof(SolidColorBrush))]
    public class BrushExtension : MarkupExtension
    {
        public BrushExtension()
        {
        }

        public BrushExtension(string name)
        {
            Name = name;
        }

        /// <summary>Color token name, for example <c>colorBgContainer</c>.</summary>
        public string Name { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Brushes.Transparent;
            }

            var binding = new Binding("Snapshot.Brushes[" + Name + "]")
            {
                Source = ThemeManager.Current,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }
    }

    /// <summary><c>{antd:FontFamily}</c> - the WPF font stack derived from the font token.</summary>
    [MarkupExtensionReturnType(typeof(FontFamily))]
    public class FontFamilyExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding("Snapshot.FontFamily")
            {
                Source = ThemeManager.Current,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
