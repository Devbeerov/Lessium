using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    #region TextBoxNewlineBehavior

    public class TextBoxNewlineBehavior : Behavior<TextBox>
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

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if(textBox == null) { return; }

            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    var selectIndex = textBox.SelectionStart;

                    textBox.Text = textBox.Text.Insert(selectIndex, System.Environment.NewLine);
                    textBox.SelectionStart = selectIndex + System.Environment.NewLine.Length - 1;
                }

                else
                {
                    // Removes focus

                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null); // Logical focus
                    Keyboard.ClearFocus(); // Keyboard focus
                }
            }
        }
    }

    #endregion

    #region DoubleClickTextBoxBehavior

    public class DoubleClickTextBoxBehavior : Behavior<TextBox>
    {
        private long _timestamp;

        protected override void OnAttached()
        {
            AssociatedObject.Focusable = false;
            AssociatedObject.Cursor = Cursors.Arrow;

            AssociatedObject.MouseDoubleClick += AssociatedObjectOnMouseDoubleClick;
            AssociatedObject.LostFocus += AssociatedObjectOnLostFocus;
        }

        private void AssociatedObjectOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (AssociatedObject.Focusable)
                return;//fix an issue of selecting all text on double click

            AssociatedObject.Cursor = Cursors.IBeam;
            AssociatedObject.Focusable = true;
            AssociatedObject.Focus();
            AssociatedObject.CaretIndex = AssociatedObject.Text.Length;

            _timestamp = Stopwatch.GetTimestamp();
        }

        private void AssociatedObjectOnLostFocus(object sender, RoutedEventArgs e)
        {
            var delta = Stopwatch.GetTimestamp() - _timestamp;
            var timesp = new TimeSpan(delta);

            if (timesp.TotalSeconds < 1)
                return;

            AssociatedObject.Cursor = Cursors.Arrow;
            AssociatedObject.Focusable = false;
        }
    }

    #endregion
}
