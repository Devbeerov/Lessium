using Lessium.ContentControls.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class NotLastToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var page = (ContentPage)values[0];
            var collection = page.Items;

            if(collection.Count == 0) { return Visibility.Collapsed; }

            var item = values[1];

            if(collection.Last() == item)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }


    }
}
