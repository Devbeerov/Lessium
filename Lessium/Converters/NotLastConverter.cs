using Lessium.ContentControls.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class NotLastConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var page = (ContentPage)values[0];
            var collection = page.Items;

            if(collection.Count == 0) { return false; }

            var item = values[1];

            if(collection.Last() == item)
            {
                return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


    }
}
