using Lessium.Converters;
using Lessium.Properties;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Lessium.Utility.Behaviors
{
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
            if (tooltip == null)
            {
                tooltip = new ToolTip
                {
                    Content = Resources.DoubleClickToEditTooltip
                };

                AssociatedObject.ToolTip = tooltip;

                // Binding

                Binding binding = new Binding("IsReadOnly") // TextBox.IsReadOnly!
                {
                    Source = AssociatedObject,
                    Converter = converter
                };

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
}
