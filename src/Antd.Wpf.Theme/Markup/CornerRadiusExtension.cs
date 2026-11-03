using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Antd.Wpf.Theme.Markup
{
    /// <summary>
    /// Multiplies a bound value, used by the badge to offset itself by 50% of its own size
    /// (antd's translate(50%, -50%)) without hard coding the pixel distance.
    /// </summary>
    public sealed class RatioConverter : IValueConverter
    {
        /// <summary>Multiplier applied to the incoming value.</summary>
        public double Ratio { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            if (value is double) number = (double)value;
            else if (!double.TryParse(System.Convert.ToString(value, culture), NumberStyles.Any, culture, out number))
                return 0.0;

            return number * Ratio;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Turns a numeric token into a uniform <see cref="CornerRadius"/>.</summary>
    public sealed class DoubleToCornerRadiusConverter : IValueConverter
    {
        public static readonly DoubleToCornerRadiusConverter Instance = new DoubleToCornerRadiusConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double radius = (double)value;
                return new CornerRadius(radius < 0 ? 0 : radius);
            }
            return new CornerRadius(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// <c>CornerRadius="{antd:CornerRadius borderRadiusSM}"</c> - avoids a read-only property per
    /// control just to expose a radius token as a CornerRadius.
    /// </summary>
    [MarkupExtensionReturnType(typeof(CornerRadius))]
    public class CornerRadiusExtension : MarkupExtension
    {
        public CornerRadiusExtension()
        {
        }

        public CornerRadiusExtension(string token)
        {
            Token = token;
        }

        /// <summary>Numeric token name, for example <c>borderRadiusSM</c>.</summary>
        public string Token { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding("Snapshot.Tokens." + (Token ?? "borderRadius"))
            {
                Source = ThemeManager.Current,
                Mode = BindingMode.OneWay,
                Converter = DoubleToCornerRadiusConverter.Instance
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
