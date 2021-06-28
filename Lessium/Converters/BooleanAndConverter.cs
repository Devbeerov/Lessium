using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Lessium.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
