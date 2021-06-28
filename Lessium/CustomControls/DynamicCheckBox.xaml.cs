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
        private static readonly Array checkBoxTypesArray = Enum.GetValues(typeof(DynamicCheckBoxType));
        private bool fireEvents = true;

        public DynamicCheckBox()
        {
            InitializeComponent();
        }

        #region Methods

        /// <summary>
        /// Updates specified CheckBox.IsChecked state.
        /// </summary>
        public void UpdateIsChecked(bool isChecked, DynamicCheckBoxType checkBoxType, bool fireEvents = false)
        {
            var toggle = FindCheckBoxTypeResource(checkBoxType.ToString());

            if (toggle == null) return;

            if (fireEvents)
                toggle.IsChecked = isChecked;
            else
                UpdateIsCheckedWithoutEvent(toggle, isChecked);
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
            }
        }

        private ToggleButton FindCheckBoxTypeResource(string typeKey)
        {
            foreach (DictionaryEntry entry in currentContentControl.Resources)
            {
                if ((string)entry.Key != typeKey) continue;

                var toggleContainer = entry.Value as ContentControl;

                return toggleContainer.Content as ToggleButton;
            }

            throw new NotSupportedException($"\"{typeKey}\" is not supported.");
        }

        private void SynchronizeRadioButtons(bool isChecked)
        {
            foreach (DynamicCheckBoxType type in checkBoxTypesArray)
            {
                if (type != DynamicCheckBoxType.CheckBox)
                {
                    UpdateIsChecked(isChecked, type);
                }
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

            SynchronizeRadioButtons(true);
            RadioButtonChecked?.Invoke(sender, e);
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!fireEvents)
            {
                e.Handled = true;
                return;
            }

            SynchronizeRadioButtons(false);
            RadioButtonUnchecked?.Invoke(sender, e);
        }

        // There's a reference to that method in DynamicCheckBox.xaml, however it might not appear due to Visual Studio bug.
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
