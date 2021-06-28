using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Lessium.Utility.Behaviors
{
    public class TextBoxCaretIndexLastBehavior : Behavior<TextBox>
    {
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
            AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
        }
    }
}
