using System;
using System.Globalization;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class ValidDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                bool isValid = !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue);
                return isValid;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
