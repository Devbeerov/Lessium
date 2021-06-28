using System;
using System.Windows;
using System.Windows.Data;

namespace Lessium.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class VisibilityAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visible = true;

            for (int i = 0; i < values.Length; i++)
            {
                var value = System.Convert.ToBoolean(values[i]);
                if (!value)
                {
                    visible = false;
                }
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
