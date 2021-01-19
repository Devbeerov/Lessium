using System;
using System.Windows;
using System.Windows.Data;

namespace Lessium.Converters
{
    [ValueConversion(typeof(Thickness), typeof(double))]
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var val = (double)value;
            var property = parameter as string;

            switch (property.ToLower())
            {
                case "left": return new Thickness(val, 0, 0, 0);
                case "top": return new Thickness(0, val, 0, 0);
                case "right": return new Thickness(0, 0, val, 0);
                case "bottom": return new Thickness(0, 0, 0, val);
            }

            throw new ArgumentException("Wrong parameter!");
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
