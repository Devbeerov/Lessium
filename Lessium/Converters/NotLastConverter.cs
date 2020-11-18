using Lessium.ContentControls.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class NotLastConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemsControl = (ItemsControl)value;

            if (itemsControl.Items.IsEmpty) { return false; }

            var currentItem = itemsControl.Items.CurrentItem;
            var lastItem = itemsControl.Items[itemsControl.Items.Count - 1];

            if (currentItem == lastItem)
            {
                return false;
            }
            return true;
        }

        //public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        //{
        //    var page = (ContentPage)values[0];
        //    var collection = page.Items;

        //    if (collection.Count == 0) { return false; }

        //    var itemsControl = (ItemsControl)values[1];
        //    var lastItem = itemsControl.Items[itemsControl.Items.Count - 1];

        //    if (collection.Last() == lastItem)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
