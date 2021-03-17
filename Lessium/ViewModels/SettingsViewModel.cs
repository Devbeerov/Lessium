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
using System.Resources;
using System.Globalization;
using Microsoft.Extensions.Localization;

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

        public ObservableCollection<LocalizedString> SectionsStrings
        {
            get { return model.SectionsStrings; }
        }

        public string SelectedSectionKey
        {
            get { return model.selectedSectionKey; }
            set { SetProperty(ref model.selectedSectionKey, value); }
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

        private DelegateCommand<RoutedPropertyChangedEventArgs<double>> OnFontSliderChangedCommand;
        public DelegateCommand<RoutedPropertyChangedEventArgs<double>> OnFontSliderChanged =>
            OnFontSliderChangedCommand ?? (OnFontSliderChangedCommand = new DelegateCommand<RoutedPropertyChangedEventArgs<double>>(ExecuteOnFontSliderChanged));

        public void ExecuteOnFontSliderChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            var value = args.NewValue;

            Settings.Default.FontSize = value;
        }

        private DelegateCommand<SelectionChangedEventArgs> OnSectionChangedCommand;
        public DelegateCommand<SelectionChangedEventArgs> OnSectionChanged =>
            OnSectionChangedCommand ?? (OnSectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteOnSectionChanged));

        public void ExecuteOnSectionChanged(SelectionChangedEventArgs e)
        {
            var localizedString = e.AddedItems[0] as LocalizedString;
            
            SelectedSectionKey = Resources.ResourceManager.GetString(localizedString.Name, CultureInfo.InvariantCulture) ?? null;
        }

        #endregion
    }
}
