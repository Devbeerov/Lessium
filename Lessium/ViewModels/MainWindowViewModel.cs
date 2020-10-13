﻿using Lessium.ContentControls;
using Lessium.Models;
using Lessium.Utility.Extension;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private properties

        private readonly MainWindowModel model;
        private string selectedTab = "Materials";

        #endregion

        #region Public CLR Properties

        #region Program

        public string Title
        {
            get { return model.title; }
            set { SetProperty(ref model.title, value); }
        }

        #endregion

        #region LessonMenu

        public string LessonHeader
        {
            get { return model.LessonHeader; }
            set { SetProperty(ref model.LessonHeader, value); }
        }

        public string EditHeader
        {
            get { return model.EditHeader; }
            set { SetProperty(ref model.EditHeader, value); }
        }

        public string UndoChangesHeader
        {
            get { return model.UndoChangesHeader; }
            set { SetProperty(ref model.UndoChangesHeader, value); }
        }

        public string RecentHeader
        {
            get { return model.RecentHeader; }
            set { SetProperty(ref model.RecentHeader, value); }
        }

        public string NewLessonHeader
        {
            get { return model.NewLessonHeader; }
            set { SetProperty(ref model.NewLessonHeader, value); }
        }

        public string SaveLessonHeader
        {
            get { return model.SaveLessonHeader; }
            set { SetProperty(ref model.SaveLessonHeader, value); }
        }

        public string LoadLessonHeader
        {
            get { return model.LoadLessonHeader; }
            set { SetProperty(ref model.LoadLessonHeader, value); }
        }

        public string CloseLessonHeader
        {
            get { return model.CloseLessonHeader; }
            set { SetProperty(ref model.CloseLessonHeader, value); }
        }

        public string PrintLessonHeader
        {
            get { return model.PrintLessonHeader; }
            set { SetProperty(ref model.PrintLessonHeader, value); }
        }

        public string ExitHeader
        {
            get { return model.ExitHeader; }
            set { SetProperty(ref model.ExitHeader, value); }
        }

        #region Internal

        public bool HasChanges
        {
            get { return model.HasChanges; }
            set { SetProperty(ref model.HasChanges, value); }
        }

        public bool ReadOnly
        {
            get { return model.ReadOnly; }
            set { SetProperty(ref model.ReadOnly, value); }
        }


        #endregion

        #endregion

        #region Tabs

        public string MaterialHeader
        {
            get { return model.MaterialHeader; }
            set { SetProperty(ref model.MaterialHeader, value); }
        }

        public string TestsHeader
        {
            get { return model.TestsHeader; }
            set { SetProperty(ref model.TestsHeader, value); }
        }

        public ObservableDictionary<string, Section> Sections
        {
            get { return model.Sections[selectedTab]; }
            set
            {
                SetDictionaryProperty(ref model.Sections, selectedTab, value);
            }
        }

        public Section CurrentSection
        {
            get { return model.CurrentSection[selectedTab]; }
            set
            {
                SetDictionaryProperty(ref model.CurrentSection, selectedTab, value);
            }
        }

        public string CurrentSectionTitle
        {
            // If CurrentSection is null, returns null (empty string) without throwing exception.
            get { return CurrentSection?.GetTitle(); }
        }

        public UIElementCollection CurrentSectionItems
        {
            get { return CurrentSection?.GetItems(); }
        }

        public int CurrentSectionID
        {
            get { return model.CurrentSectionID[selectedTab]; }
            set
            {
                SetDictionaryProperty(ref model.CurrentSectionID, selectedTab, value);
            }
        }

        private void SetDictionaryProperty<TValue> (ref Dictionary<string, TValue> dictionary, string key, TValue newValue, [CallerMemberName] string name = null)
        {
            object currentValueObject = dictionary[key];
            object newValueObject = newValue;

            if(currentValueObject != newValueObject)
            {
                dictionary[key] = newValue;
                RaisePropertyChanged(name);
            }
        }

        #endregion

        #region Buttons

        public string ButtonAddHeader
        {
            get { return model.ButtonAddHeader; }
            set { SetProperty(ref model.ButtonAddHeader, value); }
        }

        public string ButtonRemoveHeader
        {
            get { return model.ButtonRemoveHeader; }
            set { SetProperty(ref model.ButtonRemoveHeader, value); }
        }

        public string AddSectionText
        {
            get { return model.AddSectionText; }
            set { SetProperty(ref model.AddSectionText, value); }
        }

        #endregion

        #endregion

        #region Methods

        // Constructs ViewModel with Model as parameter.
        public MainWindowViewModel(MainWindowModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if(model == null)
            {
                model = new MainWindowModel();
            }

            this.model = model;
        }

        #region Section

        /// <summary>
        /// We put methods below in ViewModel to access it's functionality, instead of just Section's own methods.
        /// </summary>
        /// 
        private void SetSectionVisibility(Section section, Visibility visibility)
        {
            section.Visibility = visibility;
        }

        private void ShowSection(Section section)
        {
            SetSectionVisibility(section, Visibility.Visible);
        }

        private void CollapseSection(Section section)
        {
            SetSectionVisibility(section, Visibility.Collapsed);
        }

        private void TryCollapseCurrentSection()
        {
            if (!string.IsNullOrEmpty(CurrentSectionTitle))
            {
                CollapseSection(Sections[CurrentSectionTitle]);
            }
        }

        private void RaiseCurrentSectionChanged()
        {
            RaisePropertyChanged("CurrentSection");
            RaisePropertyChanged("CurrentSectionID");
            RaisePropertyChanged("CurrentSectionTitle");
            RaisePropertyChanged("CurrentSectionItems");
        }

        private void SelectSection(Section section)
        {
            if(CurrentSection == section) { return; }

            TryCollapseCurrentSection();

            CurrentSection = section;

            if (section != null)
            {
                ShowSection(section);
                CurrentSectionID = Sections.GetSectionID(section);
            }

            else
            {
                CurrentSectionID = -1;
            }

            RaiseCurrentSectionChanged();

        }

        #endregion

        #endregion

        #region Commands

        #region Lesson Commands

        // Lesson_EditCommand

        private DelegateCommand Lesson_EditCommand;
        public DelegateCommand Lesson_Edit =>
            Lesson_EditCommand ?? (Lesson_EditCommand = new DelegateCommand(ExecuteLesson_Edit, CanExecuteLesson_Edit)
            .ObservesProperty(() => ReadOnly)
            );

        void ExecuteLesson_Edit()
        {
            ReadOnly = false;
        }

        // Enables editing only when at ReadOnly
        bool CanExecuteLesson_Edit()
        {
            return ReadOnly;
        }

        // Lesson_UndoChangesCommand

        private DelegateCommand Lesson_UndoChangesCommand;
        public DelegateCommand Lesson_UndoChanges =>
            Lesson_UndoChangesCommand ?? (Lesson_UndoChangesCommand = new DelegateCommand(ExecuteLesson_UndoChanges, CanExecuteLesson_UndoChanges)
            .ObservesProperty(() => HasChanges)
            );

        void ExecuteLesson_UndoChanges()
        {
            HasChanges = false;
        }

        bool CanExecuteLesson_UndoChanges()
        {
            return HasChanges;
        }

        // Lesson_SaveCommand

        private DelegateCommand Lesson_SaveCommand;
        public DelegateCommand Lesson_Save =>
            Lesson_SaveCommand ?? (Lesson_SaveCommand = new DelegateCommand(ExecuteLesson_Save, CanExecuteLesson_Save)
            .ObservesProperty(() => ReadOnly) // We don't check for !bool, because we observe property change, not property value.
            .ObservesProperty(() => HasChanges)
            );

        void ExecuteLesson_Save()
        {
            // TODO: Implement save
            HasChanges = false;
        }

        bool CanExecuteLesson_Save()
        {
            return !ReadOnly && HasChanges;
        }

        // Lesson_NewCommand

        private DelegateCommand Lesson_NewCommand;
        public DelegateCommand Lesson_New =>
            Lesson_NewCommand ?? (Lesson_NewCommand = new DelegateCommand(ExecuteLesson_New));

        void ExecuteLesson_New()
        {
            // TODO: Implement new lesson
            HasChanges = true;
        }

        #endregion

        #region UI Commands

        // AddSectionCommand

        private DelegateCommand AddSectionCommand;
        public DelegateCommand AddSection =>
            AddSectionCommand ?? (AddSectionCommand = new DelegateCommand(ExecuteAddSection));
            /* It's not practically to put condition here, because it's hard to imagine that Capacity
            * will grow above highest possible, and checking condition will reduce perfomance slightly. */

        void ExecuteAddSection()
        {
            if(ReadOnly)
            {
                MessageBox.Show(model.Message_NotEnabledInReadOnly);
                return;
            }

            int repeatingIndex = 1;
            string sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());

            while (Sections.ContainsKey(sectionTitle))
            {
                repeatingIndex++;
                sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());
            }

            var newSection = new Section();
            newSection.SetTitle(sectionTitle);

            // For testing purposes
            var textblock = new TextBlock
            {
                Text = "123"
            };
            newSection.Add(textblock);

            // Adds newSection to Sections dictionary using extension method.
            Sections.AddSection(newSection);

            SelectSection(newSection);

            HasChanges = true;
        }

        #endregion

        #region Event-Commands 

        /*
        * Event-Commands handles WPF Events and execute binded Commands instead of EventHandlers.
        * This is used to avoid code-behind and put event handling at ViewModel.
        */

        // OnTabChanged

        private DelegateCommand<string> OnTabChangedCommand;
        public DelegateCommand<string> OnTabChanged =>
            OnTabChangedCommand ?? (OnTabChangedCommand = new DelegateCommand<string>(ExecuteOnTabChanged));

        void ExecuteOnTabChanged(string param)
        {
            selectedTab = param;

            RaisePropertyChanged("Sections");
            RaiseCurrentSectionChanged();
        }

        // OnSectionChanged

        private DelegateCommand<Section> OnSectionChangedCommand;
        public DelegateCommand<Section> OnSectionChanged =>
            OnSectionChangedCommand ?? (OnSectionChangedCommand = new DelegateCommand<Section>(ExecuteOnSectionChanged));

        void ExecuteOnSectionChanged(Section newSection)
        {
            SelectSection(newSection);
        }

        // OnSectionContentUpdated

        private DelegateCommand OnSectionContentUpdatedCommand;
        public DelegateCommand OnSectionContentUpdated =>
            OnSectionContentUpdatedCommand ?? (OnSectionContentUpdatedCommand = new DelegateCommand(ExecuteOnSectionContentUpdated));

        void ExecuteOnSectionContentUpdated()
        {
            SelectSection(CurrentSection);
        }

        #endregion

        #endregion
    }
}
