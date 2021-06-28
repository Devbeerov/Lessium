using Lessium.Models;
using Lessium.Properties;
using Lessium.Utility;
using Microsoft.Extensions.Localization;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private SettingsModel model = null;

        private ushort prevUndoLimit = Settings.Default.UndoLimit;

        #region CLR Properties

        public string SettingsHeader
        {
            get { return model.SettingsHeader; }
        }

        public string FontSliderHeader
        {
            get { return model.FontSliderHeader; }
        }

        public string UndoLimitHeader
        {
            get { return model.UndoLimitHeader; }
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

        public ushort UndoLimit
        {
            get { return Settings.Default.UndoLimit; }
            set
            {
                if (value != UndoLimit)
                {
                    Settings.Default.UndoLimit = value;
                }

                // Raises because it's intended to be source for binding target TextBox.Text,
                // Doesn't matter if it same value, it should update TextBox.Text to avoid invalid input.
                RaisePropertyChanged();
            }
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

        private ushort ValidateUndoLimitInput(string text)
        {
            int parsedInt;

            // If text isn't valid numeric or not convertable to int, returns previous
            if (!Validator.IsValidNumeric(text) || !int.TryParse(text, out parsedInt))
            {
                return prevUndoLimit;
            }

            // Otherwise checking bounds and update if needed
            else
            {
                if (parsedInt < ushort.MinValue) return ushort.MinValue;
                if (parsedInt > ushort.MaxValue) return ushort.MaxValue;

                var converted = Convert.ToUInt16(parsedInt);
                prevUndoLimit = converted;

                return converted;
            }
        }

        #endregion

        #region Event-Commands

        #region OnFontSliderChanged

        private DelegateCommand<RoutedPropertyChangedEventArgs<double>> OnFontSliderChangedCommand;
        public DelegateCommand<RoutedPropertyChangedEventArgs<double>> OnFontSliderChanged =>
            OnFontSliderChangedCommand ?? (OnFontSliderChangedCommand = new DelegateCommand<RoutedPropertyChangedEventArgs<double>>(ExecuteOnFontSliderChanged));

        public void ExecuteOnFontSliderChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            var value = args.NewValue;

            Settings.Default.FontSize = value;
        }

        #endregion

        #region OnSectionChanged

        private DelegateCommand<SelectionChangedEventArgs> OnSectionChangedCommand;
        public DelegateCommand<SelectionChangedEventArgs> OnSectionChanged =>
            OnSectionChangedCommand ?? (OnSectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteOnSectionChanged));

        public void ExecuteOnSectionChanged(SelectionChangedEventArgs e)
        {
            var localizedString = e.AddedItems[0] as LocalizedString;

            SelectedSectionKey = Resources.ResourceManager.GetString(localizedString.Name, CultureInfo.InvariantCulture) ?? null;
        }

        #endregion

        #region OnUndoLimitChanged

        private DelegateCommand<string> OnUndoLimitChangedCommand;
        public DelegateCommand<string> OnUndoLimitChanged =>
            OnUndoLimitChangedCommand ?? (OnUndoLimitChangedCommand = new DelegateCommand<string>(ExecuteOnUndoLimitChanged));

        public void ExecuteOnUndoLimitChanged(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                UndoLimit = ValidateUndoLimitInput(text);
            }

            else
            {
                UndoLimit = 1;
            }
        }

        #endregion

        #endregion
    }
}
