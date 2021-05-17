using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace Lessium.Services
{
    public static class MathHelper
    {
        public static double DistanceBetweenElements(UIElement first, UIElement second, Coordinate coordinate)
        {
            var point = first.TranslatePoint(new Point(0, 0), second);

            switch (coordinate)
            {
                case Coordinate.X:
                    return Math.Abs(point.X);
                case Coordinate.Y:
                    return Math.Abs(point.Y);

                default: throw new ArgumentException("Invalid coordinate.");
            }
        }
    }

    public enum Coordinate
    {
        X, Y
    }

    [TypeConverter(typeof(Coordinate))]
    public class CoordinateConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string coordinateString)
            {
                switch(coordinateString.ToLower())
                {
                    case "x":
                        return Coordinate.X;
                    case "y":
                        return Coordinate.Y;

                    default: throw new NotSupportedException("Only X and Y coordinates are supported.");
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
