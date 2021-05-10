using System;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public partial class DynamicCheckBox : UserControl
    {
        public DynamicCheckBox()
        {
            InitializeComponent();
        }

        #region Events

        public event EventHandler<RoutedEventArgs> CheckBoxChecked;
        public event EventHandler<RoutedEventArgs> CheckBoxUnchecked;

        public event EventHandler<RoutedEventArgs> RadioButtonChecked;
        public event EventHandler<RoutedEventArgs> RadioButtonUnchecked;

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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxChecked?.Invoke(sender, e);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBoxUnchecked?.Invoke(sender, e);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked?.Invoke(sender, e);
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RadioButtonUnchecked?.Invoke(sender, e);
        }

        #endregion
    }

    // NOTE: If you will change DynamicCheckBoxType names, make sure that you will update them in control too.
    public enum DynamicCheckBoxType
    {
        RadioSingle, RadioMultiple, CheckBox
    }
}
