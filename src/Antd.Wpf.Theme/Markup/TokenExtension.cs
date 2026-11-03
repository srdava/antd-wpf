using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace Antd.Wpf.Theme.Markup
{
    /// <summary>
    /// <c>{antd:Token FontSize}</c> - one way binding to a numeric or string token on the
    /// active snapshot. Use it for element properties (never inside Setter.Value, WPF does
    /// not allow Bindings there - use DynamicResource for styles and triggers).
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class TokenExtension : MarkupExtension
    {
        public TokenExtension()
        {
        }

        public TokenExtension(string path)
        {
            Path = path;
        }

        /// <summary>Token name, for example <c>fontSize</c> or <c>lineHeight</c>.</summary>
        public string Path { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return null;
            }

            var binding = new Binding("Snapshot.Tokens." + Path)
            {
                Source = ThemeManager.Current,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
