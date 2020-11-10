using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    #region TextBoxEnterKeyUpdateBehavior

    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && e.Key == Key.Enter)
            {
                // Removes focus

                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null); // Logical focus
                Keyboard.ClearFocus(); // Keyboard focus
            }
        }
    }

    #endregion
}
