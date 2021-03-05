using Lessium.Properties;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Linq;
using Lessium.Utility;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Lessium.Models;

namespace Lessium.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private SettingsModel model = null;

        #region CLR Properties

        public string SettingsHeader
        {
            get { return model.SettingsHeader; }
        }

        public string FontSliderHeader
        {
            get { return model.FontSliderHeader; }
        }

        public ObservableCollection<string> SectionHeaders
        {
            get { return model.SectionHeaders; }
        }

        #endregion

        #region Constructors

        public SettingsViewModel(SettingsModel model)
        {
            // Dependency injection

            this.model = model;
        }

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

            Settings.Default.FontSize = value.Value;
        }

        #endregion
    }
}
