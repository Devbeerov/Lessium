using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    public class TextBoxNewlineBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (AssociatedObject != null)
            {
                base.OnAttached();
                AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (AssociatedObject == null) { return; }

            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    var selectIndex = AssociatedObject.SelectionStart;

                    AssociatedObject.Text = AssociatedObject.Text.Insert(selectIndex, System.Environment.NewLine);
                    AssociatedObject.SelectionStart = selectIndex + System.Environment.NewLine.Length - 1;
                }

                else
                {
                    // Removes focus

                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(AssociatedObject), null); // Logical focus
                    Keyboard.ClearFocus(); // Keyboard focus
                }
            }
        }
    }
}
