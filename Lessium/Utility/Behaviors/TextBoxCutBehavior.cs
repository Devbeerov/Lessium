using Lessium.ContentControls;
using Lessium.Converters;
using Lessium.Models;
using Lessium.Properties;
using Lessium.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Lessium.Utility.Behaviors
{
    /// <summary>
    /// Prevents TextBox from growing beyond MaxHeight. Behavior cuts exceeding text.
    /// Will change MaxLines property automatically on demand.
    /// </summary>
    public class TextBoxCutBehavior : Behavior<TextBox>
    {
        private bool raiseEvent = true;
        private static readonly UIElementsDistanceConverter distanceConverter = new UIElementsDistanceConverter();
        private double offset = 0d;

        protected override void OnAttached()
        {
            if (AssociatedObject != null)
            {
                base.OnAttached();

                AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObject_TextChanged;

                base.OnDetaching();
            }
        }

        private void CalculateOffset()
        {
            var inputElements = new object[] { AssociatedObject.FindParent<ContentPageControl>(), AssociatedObject };

            offset = (double)distanceConverter.Convert(inputElements, AssociatedType, Coordinate.Y.ToString(), CultureInfo.InvariantCulture);
        }

        private double FindProperMaxHeight()
        {
            if (double.IsInfinity(AssociatedObject.MaxHeight) || double.IsNaN(AssociatedObject.MaxHeight)) throw new NotSupportedException("Behavior can only work with specified MaxHeight.");

            return AssociatedObject.MaxHeight;
        }

        private double CalculateMaxHeight()
        {
            var maxHeight = FindProperMaxHeight();

            if (offset == 0d) CalculateOffset();

            return maxHeight - offset * 2; // For fixing border we substract two offsets
        }

        private bool IsFitMaxHeight()
        {
            var maxHeight = CalculateMaxHeight();
            var lineHeight = AssociatedObject.CalculateLineHeight();

            return lineHeight * AssociatedObject.LineCount <= maxHeight;
        }

        private int CalculateMaxValidLineCount()
        {
            var maxHeight = CalculateMaxHeight();
            var lineHeight = AssociatedObject.CalculateLineHeight();

            return (int) Math.Floor(maxHeight / lineHeight);
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!raiseEvent) return;
            if (AssociatedObject == null) return;
            if (IsFitMaxHeight()) return;
            
            e.Handled = true;

            int prevCaret = AssociatedObject.CaretIndex; // Caret before removing everything past MaxLine

            // Calculates values of MaxLine

            int MaxLineIndex = CalculateMaxValidLineCount() - 1;
            int firstPositionInMaxLine = AssociatedObject.GetCharacterIndexFromLineIndex(MaxLineIndex);
            int lengthOfMaxLine = AssociatedObject.GetLineLength(MaxLineIndex);
            int lastPositionInMaxLine = firstPositionInMaxLine + lengthOfMaxLine;

            // Removes everything past MaxLine

            var newText = AssociatedObject.Text.Remove(lastPositionInMaxLine);
            UpdateTextWithoutFiring(newText);

            // Restores caret

            if (prevCaret > newText.Length)
            {
                prevCaret = newText.Length;
            }

            AssociatedObject.CaretIndex = prevCaret;
        }

        private void UpdateTextWithoutFiring(string newText)
        {
            raiseEvent = false;

            AssociatedObject.Text = newText;
            AssociatedObject.UpdateLayout();

            raiseEvent = true;
        }
    }
}
