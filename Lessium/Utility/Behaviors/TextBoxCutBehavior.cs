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

                Settings.Default.PropertyChanged += OnFontSizeChanged;
                AssociatedObject.Loaded += AssociatedObject_Loaded;
                AssociatedObject.TextChanged += AssociatedObject_TextChanged;
                
            }
        }

        private void OnFontSizeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Settings.FontSize)) return;

            UpdateMaxLineCount();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMaxLineCount();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                Settings.Default.PropertyChanged -= OnFontSizeChanged;
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
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
            if (double.IsInfinity(AssociatedObject.MaxHeight) || double.IsNaN(AssociatedObject.MaxHeight)) return ContentPageModel.PageHeight;

            return AssociatedObject.MaxHeight;
        }

        private double CalculateMaxHeight()
        {
            var maxHeight = FindProperMaxHeight();

            if (offset == 0d) CalculateOffset();

            return maxHeight - offset * 2; // For fixing border we substract two offsets
        }

        private void UpdateMaxLineCount()
        {
            var maxHeight = CalculateMaxHeight();
            var lineHeight = AssociatedObject.CalculateLineHeight();

            AssociatedObject.MaxLines = (int)Math.Floor(maxHeight / lineHeight);
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!raiseEvent) { return; }

            if (AssociatedObject == null) { return; }

            if (AssociatedObject.LineCount > AssociatedObject.MaxLines)
            {
                e.Handled = true;

                int prevCaret = AssociatedObject.CaretIndex; // Caret before removing everything past MaxLine

                // Calculates values of MaxLine

                int MaxLineIndex = AssociatedObject.MaxLines - 1;
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
