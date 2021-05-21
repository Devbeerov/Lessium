using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    /// <summary>
    /// Prevents TextBox from growing beyond MaxHeight. Behavior cuts exceeding text.
    /// </summary>
    public class TextBoxCutBehavior : Behavior<TextBox>
    {
        private bool raiseEvent = true;

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
