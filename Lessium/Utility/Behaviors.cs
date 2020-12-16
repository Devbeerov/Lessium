using Lessium.Converters;
using Lessium.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Linq;

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
            TextBox textBox = sender as TextBox;

            if (textBox == null) { return; }

            if (textBox.LineCount > textBox.MaxLines)
            {
                e.Handled = true;

                var pos = 0;
                for (int i = 0; i <= textBox.MaxLines; i++)
                {
                    pos += textBox.GetLineLength(i);
                }

                var totalAddedLength = e.Changes.Sum((change) => Math.Abs(change.AddedLength - change.RemovedLength));

                string sourceText = string.Copy(textBox.Text);

                // Moving offset to get valid wrapping for text.
                for (int i = 0; i < totalAddedLength; i++)
                {
                    string temp = sourceText.Substring(0, pos);

                    UpdateTextWithoutFiring(textBox, temp);

                    textBox.UpdateLayout();

                    if (textBox.LineCount > textBox.MaxLines)
                    {
                        // Decreases pos for next iteration.

                        pos--;
                    }

                    else
                    {
                        break;
                    }
                }

                // pos now should be last valid position before wrap.

                textBox.Text = sourceText.Remove(pos);

                textBox.CaretIndex = pos;
            }

        }

        private void UpdateTextWithoutFiring(TextBox textBox, string newText)
        {
            textBox.TextChanged -= AssociatedObject_TextChanged;

            textBox.Text = newText;

            textBox.TextChanged += AssociatedObject_TextChanged;
        }

        //private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBox = sender as TextBox;

        //    if (textBox == null) { return; }

        //    //var currentSize = MeasureTextBoxSize(textBox, textBox.Text);
        //    //var newSize = MeasureTextBoxSize(textBox, textBox.Text);
        //    //if (currentSize.Width >= 29382)

        //    //if(textBox.ActualHeight == textBox.MaxHeight)
        //    //{
        //    //    // Cut

        //    //    var minPos = e.Changes.Min((change) => change.Offset);
        //    //    var maxPos = textBox.

        //    //    textBox.Text = textBox.Text.Remove(0, pos);
        //    //}
        //    //Console.WriteLine($"{textBox.DesiredSize}, {textBox.RenderSize}");
        //    //29382

        //    //int pos = textBox.meas
        //    //textBox.Text = textBox.Text.Remove(pos);
        //    //if(textBox.)

        //    //if (e.Key == Key.Enter)
        //    //{
        //    //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        //    //    {
        //    //        var selectIndex = textBox.SelectionStart;

        //    //        textBox.Text = textBox.Text.Insert(selectIndex, System.Environment.NewLine);
        //    //        textBox.SelectionStart = selectIndex + System.Environment.NewLine.Length - 1;
        //    //    }

        //    //    else
        //    //    {
        //    //        // Removes focus

        //    //        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null); // Logical focus
        //    //        Keyboard.ClearFocus(); // Keyboard focus
        //    //    }
        //    //}
        //}

        private Size MeasureTextBoxSize(TextBox textBox, string text)
        {
            
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
                textBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }

    #endregion
}
