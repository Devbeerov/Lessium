using Lessium.Utility;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lessium.CustomControls
{
    public partial class DynamicCheckBox : UserControl
    {
        private bool fireEvents = true;

        public DynamicCheckBox()
        {
            InitializeComponent();
        }

        #region Methods

        /// <summary>
        /// Updates specified CheckBox.IsChecked state, will not fire Checked events.
        /// </summary>
        public void UpdateIsChecked(bool isChecked, DynamicCheckBoxType checkBoxType)
        {
            var typeString = checkBoxType.ToString();

            foreach (DictionaryEntry entry in currentContentControl.Resources)
            {
                if ((string)entry.Key != typeString) continue;

                var toggleContainer = entry.Value as ContentControl;
                var toggle = toggleContainer.Content as ToggleButton;

                UpdateIsCheckedWithoutEvent(toggle, isChecked);

                return;
            }
        }

        private void UpdateIsCheckedWithoutEvent(ToggleButton toggle, bool isChecked)
        {
            fireEvents = false;

            toggle.IsChecked = isChecked;

            fireEvents = true;
        }

        private void SynchronizeContent(ContentControl oldControl, ContentControl newControl)
        {
            if (oldControl == null) return;

            var oldToggle = oldControl.Content as ToggleButton;
            var newToggle = newControl.Content as ToggleButton;

            // For example, if old is CheckBox and new is RadioButton, then it won't synchronize.
            if (oldToggle as RadioButton != null && newToggle as RadioButton != null)
            {
                newToggle.IsChecked = oldToggle.IsChecked;
                // TODO: find place where  DynamicCheckBoxTypeKey changes
            }
        }

        #endregion

        #region Events

        public event EventHandler<RoutedEventArgs> CheckBoxChecked;
        public event EventHandler<RoutedEventArgs> CheckBoxUnchecked;

        public event EventHandler<RoutedEventArgs> RadioButtonChecked;
        public event EventHandler<RoutedEventArgs> RadioButtonUnchecked;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!fireEvents)
            {
                e.Handled = true;
                return;
            }

            CheckBoxChecked?.Invoke(sender, e);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!fireEvents)
            {
                e.Handled = true;
                return;
            }

            CheckBoxUnchecked?.Invoke(sender, e);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!fireEvents)
            {
                e.Handled = true;
                return;
            }

            RadioButtonChecked?.Invoke(sender, e);
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!fireEvents)
            {
                e.Handled = true;
                return;
            }

            RadioButtonUnchecked?.Invoke(sender, e);
        }

        private void OnDynamicContentChanged(object sender, DynamicContentChangedArgs e)
        {
            SynchronizeContent(e.OldContent as ContentControl, e.NewContent as ContentControl);
        }

        #endregion

        #region Dependency Properties

        public DynamicCheckBoxType DynamicCheckBoxTypeKey
        {
            get
            {
                var stringValue = (string)GetValue(DynamicCheckBoxTypeKeyProperty);
                var parsedDynamicCheckBoxType = (DynamicCheckBoxType)Enum.Parse(typeof(DynamicCheckBoxType), stringValue);

                return parsedDynamicCheckBoxType;
            }
            set { SetValue(DynamicCheckBoxTypeKeyProperty, value.ToString()); }
        }

        public string RadioButtonGroupName
        {
            get { return (string)GetValue(RadioButtonGroupNameProperty); }
            set { SetValue(RadioButtonGroupNameProperty, value); }
        }

        public static readonly DependencyProperty DynamicCheckBoxTypeKeyProperty =
            DependencyProperty.Register("DynamicCheckBoxTypeKey", typeof(string), typeof(DynamicCheckBox), new PropertyMetadata(null));

        public static readonly DependencyProperty RadioButtonGroupNameProperty =
            DependencyProperty.Register("RadioButtonGroupName", typeof(string), typeof(DynamicCheckBox), new PropertyMetadata(null));

        #endregion
    }

    // NOTE: If you will change DynamicCheckBoxType names, make sure that you will update them in control too.
    public enum DynamicCheckBoxType
    {
        RadioSingle, RadioMultiple, CheckBox
    }
}
