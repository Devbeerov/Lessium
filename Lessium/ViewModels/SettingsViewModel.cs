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

        public string UndoCountHeader
        {
            get { return model.UndoCountHeader; }
        }

        public string RedoCountHeader
        {
            get { return model.RedoCountHeader; }
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

        public ushort UndoCount
        {
            get { return Settings.Default.UndoCount; }
            set
            {
                if (value != UndoCount)
                {
                    Settings.Default.UndoCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ushort RedoCount
        {
            get { return Settings.Default.RedoCount; }
            set 
            {
                if (value != RedoCount)
                {
                    Settings.Default.RedoCount = value;
                    RaisePropertyChanged(nameof(RedoCount));
                }
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

        #region OnUndoCountChanged

        private DelegateCommand<string> OnUndoCountChangedCommand;
        public DelegateCommand<string> OnUndoCountChanged =>
            OnUndoCountChangedCommand ?? (OnUndoCountChangedCommand = new DelegateCommand<string>(ExecuteOnUndoCountChanged));

        public void ExecuteOnUndoCountChanged(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                UndoCount = ushort.Parse(text);
            }
        }

        #endregion

        #region OnRedoCountChanged

        private DelegateCommand<string> OnRedoCountChangedCommand;
        public DelegateCommand<string> OnRedoCountChanged =>
            OnRedoCountChangedCommand ?? (OnRedoCountChangedCommand = new DelegateCommand<string>(ExecuteOnRedoCountChanged));

        public void ExecuteOnRedoCountChanged(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                RedoCount = ushort.Parse(text);
            }
        }

        #endregion

        #endregion
    }
}
