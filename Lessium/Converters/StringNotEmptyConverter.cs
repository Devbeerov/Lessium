using System;
using System.Windows.Data;

namespace Lessium.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var targetString = (string)value; // Will get exception if not string.

            return targetString != string.Empty; // No need for String.Equals
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
