using Lessium.ContentControls;
using Lessium.Converters;
using Lessium.Properties;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    #region TextBoxNewlineBehavior

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

    #endregion

    #region DoubleClickTextBoxBehavior

    public class DoubleClickTextBoxBehavior : Behavior<TextBox>
    {
        private static readonly InverseBooleanConverter converter = new InverseBooleanConverter();

        private long _timestamp;

        protected override void OnAttached()
        {
            // Properties

            AssociatedObject.Focusable = false;
            AssociatedObject.Cursor = Cursors.Arrow;

            // Tooltip

            var tooltip = AssociatedObject.ToolTip as ToolTip;
            if(tooltip == null)
            {
                tooltip = new ToolTip();
                tooltip.Content = Resources.DoubleClickToEditTooltip;

                AssociatedObject.ToolTip = tooltip;

                // Binding

                Binding binding = new Binding("IsEditable");
                binding.Source = AssociatedObject;
                binding.Converter = converter;

                AssociatedObject.SetBinding(ToolTipService.IsEnabledProperty, binding);
            }

            // Events

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

    #region TextBoxCutBehavior

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

    #endregion

    #region TextBoxCaretIndexLastBehavior

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

    #endregion
}
