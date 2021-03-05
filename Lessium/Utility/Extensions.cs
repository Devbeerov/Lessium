using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lessium.Utility
{
    public static class Extensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        #region TextBox

        public static double CalculateLineHeight(this TextBox textBox)
        {
            var formattedText = new FormattedText
            (
                "1",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBox.FontFamily, textBox.FontStyle,
                textBox.FontWeight, textBox.FontStretch),
                textBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1
            );

            return formattedText.Height;
        }

        public static Size MeasureText(this TextBlock textBlock)
        {
            var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);

            return MeasureText(textBlock.Text, typeface, textBlock.FontSize);
        }

        public static Size MeasureText(this TextBox textBox)
        {
            var typeface = new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch);

            return MeasureText(textBox.Text, typeface, textBox.FontSize);
        }

        public static Size MeasureText(string text, Typeface typeface, double fontSize)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion
    }
}
