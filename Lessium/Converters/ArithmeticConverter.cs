using System;
using System.Linq;
using System.Windows.Data;

namespace Lessium.Converters
{
    public class ArithmeticConverter : IMultiValueConverter
    {
        /// <summary>
        /// Performs specified operation on given values. All values should be convertible to double!
        /// </summary>
        /// <param name="values">First - Source value, which all operations will be applied to.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Operation parameter.
        /// + = addition
        /// - = substracton
        /// * = multiplication
        /// / = division
        /// % = modulus</param>
        /// <param name="culture"></param>
        /// <returns>Double.</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (double)values[0];
            var operation = (string)parameter;

            // Performs operation with values next to first(zero-index) value.
            switch(operation)
            {
                case "+":

                    for(int i = 1; i < values.Length; i++)
                    {
                        var value = (double)values[i];
                        source += value;
                    }

                    break;

                case "-":

                    for (int i = 1; i < values.Length; i++)
                    {
                        var value = (double)values[i];
                        source -= value;
                    }

                    break;

                case "*":

                    for (int i = 1; i < values.Length; i++)
                    {
                        var value = (double)values[i];
                        source *= value;
                    }

                    break;

                case "/":

                    for (int i = 1; i < values.Length; i++)
                    {
                        var value = (double)values[i];
                        source /= value;
                    }

                    break;

                case "%":

                    for (int i = 1; i < values.Length; i++)
                    {
                        var value = (double)values[i];
                        source %= value;
                    }

                    break;

                default: throw new ArgumentException("Parameter should be one of five operations!");
            }

            return source;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
