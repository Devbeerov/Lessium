﻿using Lessium.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class UIElementsDistanceConverter : IMultiValueConverter
    {
        /// <summary>
        /// Calculates distance between TWO given UIElements and parameter coordinate (X or Y)
        /// </summary>
        /// <param name="values">Two UIElements</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">X or Y</param>
        /// <param name="culture"></param>
        /// <returns>Double</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var first = (UIElement)values[0];
            var second = (UIElement)values[1];
            var coordinateString = (string)parameter;

            if (values.Length > 2)
            {
                var warning = new WarningException("Values past first two were ignored.");
                Debug.WriteLine(warning, "Warning");
            }

            var converter = TypeDescriptor.GetConverter(typeof(Coordinate));
            var coordinate = (Coordinate)converter.ConvertFrom(coordinateString);

            return MathHelper.DistanceBetweenElements(first, second, coordinate);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
