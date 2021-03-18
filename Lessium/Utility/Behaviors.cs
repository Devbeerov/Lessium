﻿using Lessium.ContentControls;
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

                Binding binding = new Binding("IsReadOnly");
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
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            }
        }


        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!raiseEvent) { return; }

            TextBox textBox = sender as TextBox;

            if (textBox == null) { return; }

            if (textBox.LineCount > textBox.MaxLines)
            {
                e.Handled = true;

                int prevCaret = textBox.CaretIndex; // Caret before removing everything past MaxLine

                // Calculates values of MaxLine

                int MaxLineIndex = textBox.MaxLines - 1;
                int firstPositionInMaxLine = textBox.GetCharacterIndexFromLineIndex(MaxLineIndex);
                int lengthOfMaxLine = textBox.GetLineLength(MaxLineIndex);
                int lastPositionInMaxLine = firstPositionInMaxLine + lengthOfMaxLine;

                // Removes everything past MaxLine

                var newText = textBox.Text.Remove(lastPositionInMaxLine);
                UpdateTextWithoutFiring(textBox, newText);

                // Restores caret

                if (prevCaret > newText.Length)
                {
                    prevCaret = newText.Length;
                }

                textBox.CaretIndex = prevCaret;

            }

        }

        private void UpdateTextWithoutFiring(TextBox textBox, string newText)
        {
            raiseEvent = false;

            textBox.Text = newText;
            textBox.UpdateLayout();

            raiseEvent = true;
        }
    }

    #endregion

    #region UshortDigitTextBoxBehavior

    public class UshortDigitTextBoxBehavior : Behavior<TextBox>
    {
        private bool raiseChanged = true;

        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.TextChanged += AssociatedObject_TextChanged;
                this.AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
                this.AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!raiseChanged) return;

            var textBox = sender as TextBox;
            var text = textBox.Text;

            var digitsString = Validator.RemoveNonDigits(text);

            ushort? convertedValue = null;

            if (int.TryParse(digitsString, out int parsedInt))
            {
                if (parsedInt < ushort.MinValue) convertedValue = ushort.MinValue;
                else if (parsedInt > ushort.MaxValue) convertedValue = ushort.MaxValue;
                else convertedValue = (ushort)parsedInt;
            }

            if (convertedValue.HasValue)
            {
                UpdateWithoutFiring(textBox, convertedValue.ToString());
            }
        }

        private void UpdateWithoutFiring(TextBox textBox, string text)
        {
            raiseChanged = false;

            textBox.Text = text;

            raiseChanged = true;
        }

        private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validator.IsOnlyDigits(e.Text); // If not only digits, handled will be true, so Text won't be updated.
        }
    }

    #endregion
}
