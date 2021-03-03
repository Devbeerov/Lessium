using Lessium.Properties;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;

namespace Lessium.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        #region CLR Properties

        public string SettingsHeader { get; } = Resources.SettingsHeader;
        public string FontSliderHeader { get; } = Resources.FontSliderHeader;

        #endregion

        #region Methods

        #endregion

        #region Event-Commands

        private DelegateCommand<double?> OnFontSliderChangedCommand;
        public DelegateCommand<double?> OnFontSliderChanged =>
            OnFontSliderChangedCommand ?? (OnFontSliderChangedCommand = new DelegateCommand<double?>(ExecuteOnFontSliderChanged));

        public void ExecuteOnFontSliderChanged(double? value)
        {
            if (!value.HasValue) throw new ArgumentNullException("value");

            Application.Current.Resources["FontSize"] = value.Value;
        }

        #endregion
    }
}
