using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public class UnselectableRadioButton : RadioButton
    {
        protected override void OnClick()
        {
            bool? wasChecked = this.IsChecked;

            base.OnClick();

            if (wasChecked == true) this.IsChecked = false;
        }
    }
}
