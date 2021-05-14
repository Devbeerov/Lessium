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
            var operationString = (string)parameter;
            var operation = ConvertToOperation(operationString);

            return ApplyOperation(source, values, operation);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private ArithmeticOperation ConvertToOperation(string operationString)
        {
            switch (operationString)
            {
                case "+":
                    return ArithmeticOperation.Add;

                case "-":
                    return ArithmeticOperation.Substract;

                case "*":
                    return ArithmeticOperation.Multiply;

                case "/":
                    return ArithmeticOperation.Divide;

                case "%":
                    return ArithmeticOperation.DivideWithRemainer;

                default: throw new ArgumentException("Parameter should be one of five operations!");
            }
        }

        private double ApplyOperation(double source, object[] values, ArithmeticOperation operation)
        {
            for (int i = 1; i < values.Length; i++)
            {
                var value = (double)values[i];

                switch (operation)
                {
                    case ArithmeticOperation.Add:
                        source += value;
                        break;

                    case ArithmeticOperation.Substract:
                        source -= value;
                        break;

                    case ArithmeticOperation.Multiply:
                        source *= value;
                        break;

                    case ArithmeticOperation.Divide:
                        source /= value;
                        break;

                    case ArithmeticOperation.DivideWithRemainer:
                        source %= value;
                        break;
                }
            }

            return source;
        }

        private enum ArithmeticOperation
        {
            Add, Substract, Multiply, Divide, DivideWithRemainer
        }
    }
}
