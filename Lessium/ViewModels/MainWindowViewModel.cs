﻿using Lessium.ContentControls;
using Lessium.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Linq;
using System;
using Lessium.ContentControls.MaterialControls;
using System.Windows.Input;
using Lessium.Interfaces;
using Lessium.ContentControls.Models;
using Lessium.ContentControls.TestControls;
using System.ComponentModel;
using Lessium.Classes;
using Microsoft.Win32;
using System.Diagnostics;
using Lessium.Views;
using System.Windows.Data;
using Lessium.Classes.IO;
using Lessium.Utility;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private fields

        private readonly MainWindowModel model;
        private bool currentPageIsNotFirst = false;
        private bool savingOrLoading = false;

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

        public bool HasChanges
        {
            get { return model.HasChanges; }
            set { SetProperty(ref model.HasChanges, value); }
        }

        public bool ReadOnly
        {
            get { return model.ReadOnly; }
            set
            {
                if(SetProperty(ref model.ReadOnly, value))
                {
                    UpdateSectionsEditable();
                }
            }
        }

        #endregion

        #region Tabs

        public string SelectedTab
        {
            get { return model.SelectedTab; }
            set
            {
                var prevTab = string.Copy(model.SelectedTab); // If we won't copy, it will change on SetProperty.
                var prevSection = model.CurrentSection[prevTab];

                if (SetProperty(ref model.SelectedTab, value)) // Will change CurrentSection property, because it's Get method bound with SelectedTab
                {
                    // Updates previous

                    model.LastSelectedSection[prevTab] = prevSection;
                }
            }
        }

        public bool SelectedTabIsMaterials
        {
            get { return model.SelectedTab == "Materials"; }
        }

        public bool SelectedTabIsTests
        {
            get { return model.SelectedTab == "Tests"; }
        }

        public ObservableCollection<Section> Sections
        {
            get { return model.Sections[SelectedTab]; }
            set
            {
                SetDictionaryProperty(ref model.Sections, SelectedTab, value);
            }
        }

        public Dictionary<string, ObservableCollection<Section>> SectionsByTab
        {
            get { return model.Sections; }
        }

        // Use SelectSection method to "Set" CurrentSection.
        public Section CurrentSection
        {
            get { return model.CurrentSection[SelectedTab]; }

        }

        // Should be used exclusively for binding!
        public int CurrentSectionID
        {
            get
            {
                return model.CurrentSectionID[SelectedTab];
            }
            set
            {
                if(SetDictionaryProperty(ref model.CurrentSectionID, SelectedTab, value))
                {
                    if(value == -1) { return; }

                    SelectSection(Sections[value]);
                }
            }
        }

        #endregion

        #region Pages

        public ObservableCollection<ContentPage> Pages
        {
            get { return CurrentSection?.GetPages(); }
            set
            {
                CurrentSection.SetPages(value);
            }
        }

        public int CurrentPageIndex
        {
            get
            {
                if(CurrentSection == null) { return -1; }

                return CurrentSection.GetSelectedPageIndex();
            }
            set
            {
                if (CurrentPageIndex != value)
                {
                    if (value < 0) { value = 0; }
                    else if (value >= Pages.Count) { value = Pages.Count - 1; }

                    CurrentSection.SetSelectedPageIndex(value);
                    CurrentPage = Pages[value];

                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CurrentPageNumber));
                }
            }
        }

        public int CurrentPageNumber
        {
            get { return CurrentPageIndex + 1; }
            set
            {
                // Validates index

                var number = value;

                if (number <= 0) { number = 1; }
                else if (number > Pages.Count) { number = Pages.Count; }

                var index = number - 1;

                CurrentPageIndex = index;

                RaisePropertyChanged();
            }
        }

        public ContentPage CurrentPage
        {
            get { return CurrentSection?.GetSelectedPage(); }
            set
            {
                if (CurrentPage != value)
                {
                    CurrentSection.SetSelectedPage(value);

                    if (CurrentPage == null)
                    {
                        CurrentPageIndex = -1;
                    }

                    else
                    {
                        CurrentPageIndex = CurrentSection.GetPages().IndexOf(value);
                    }

                    UpdateCurrentPageNotFirst();
                    RaisePropertyChanged(nameof(CurrentPageNumber));
                    RaisePropertyChanged(nameof(CurrentPage));
                }
            }
        }

        public bool CurrentPageIsNotFirst
        {
            get { return currentPageIsNotFirst; }
            set { SetProperty(ref currentPageIsNotFirst, value); }
        }

        #endregion

        #region Buttons

        public string AddSectionText
        {
            get { return model.AddSectionText; }
        }

        #endregion

        #endregion

        #region Methods

        // Constructs ViewModel with Model as parameter.
        public MainWindowViewModel(MainWindowModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if (model == null)
            {
                model = new MainWindowModel();
            }

            this.model = model;
        }

        public ContentType SelectedTabToContentType()
        {
            if (SelectedTab == "Materials")
            {
                return ContentType.Material;
            }

            return ContentType.Test;
        }

        private bool SetDictionaryProperty<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, TKey key, TValue newValue, [CallerMemberName] string name = null)
        {
            var oldValue = dictionary[key];

            if (oldValue == null || !oldValue.Equals(newValue))
            {
                dictionary[key] = newValue;
                RaisePropertyChanged(name);
                return true;
            }

            return false;
        }

        private void AddContentControl(IContentControl control)
        {
            CurrentPage.Add(control);

            HasChanges = true;
        }

        private void UpdateCurrentPageNotFirst()
        {
            bool notFirst = false;

            if (Pages != null && Pages.Count > 0)
            {
                notFirst = CurrentPageIndex != 0;
            }

            CurrentPageIsNotFirst = notFirst;
        }

        private void ClearLesson()
        {
            foreach (var key in model.Sections.Keys)
            {
                model.Sections[key].Clear();
                model.LastSelectedSection[key] = null;
            }
            RaisePropertyChanged(nameof(Sections));
            SelectSection(null);
            HasChanges = true;
        }

        #region Section

        /// <summary>
        /// We put these methods in ViewModel to access it's functionality, instead of only Section's ones.
        /// </summary>

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
            if (CurrentSection != null)
            {
                CollapseSection(CurrentSection);
            }
        }

        private void UpdateSectionsEditable()
        {
            var materialSections = SectionsByTab["Materials"];
            var testSections = SectionsByTab["Tests"];

            foreach (var section in materialSections)
            {
                section.SetEditable(!ReadOnly);
            }

            foreach (var section in testSections)
            {
                section.SetEditable(!ReadOnly);
            }
        }

        /// <summary>
        /// Should be used when changing tabs.
        /// Checks section with manually providen previousSection to avoid wrong comprasion.
        /// </summary>
        /// <param name="section">New Section</param>
        /// <param name="previousSection">Previous Section (to check with new)</param>
        /// <param name="tabChange">Does it selected from tab changing? If so, will clear CurrentSection for this tab.</param>
        private void SelectSection(Section section, Section previousSection = null, bool tabChange = false)
        {
            if(tabChange)
            {
                SetDictionaryProperty(ref model.CurrentSection, SelectedTab, null, nameof(CurrentSection));
            }

            // Sets CurrentSection

            if (SetDictionaryProperty(ref model.CurrentSection, SelectedTab, section, nameof(CurrentSection)))
            {
                // Each section have it's own pages

                 RaisePropertyChanged(nameof(Pages));

                // Updates previous

                if (previousSection != null)
                {
                    model.LastSelectedPage[previousSection] = previousSection.GetSelectedPage();
                    previousSection.PagesChanged -= OnPagesChanged;
                }

                if (section != null)
                {
                    CurrentSectionID = Sections.IndexOf(section);
                    section.PagesChanged += OnPagesChanged;

                    ContentPage storedPage = null;

                    if (model.LastSelectedPage.ContainsKey(section))
                    {
                        storedPage = model.LastSelectedPage[section];
                    }

                    else
                    {
                        model.LastSelectedPage.Add(section, section.GetSelectedPage());
                    }

                    if (storedPage != null)
                    {
                        // For proper update of CurrentPage property

                        CurrentSection.SetSelectedPage(null);

                        // Updates CurrentPage property

                        CurrentPage = storedPage;
                    }
                    else
                    {
                        // Storing selected page

                        var selectedPage = CurrentSection.GetSelectedPage();

                        // For proper setting of property, we need to set Section SelectedPage to null.

                        section.SetSelectedPage(null);

                        CurrentPage = selectedPage;
                    }

                    // We still should call RaisePropertyChanged, because we bind to them in View.
                    // So when changing Sections, Index and Number could be the same.
                    RaisePropertyChanged(nameof(CurrentPageIndex));
                    RaisePropertyChanged(nameof(CurrentPageNumber));

                    ShowSection(section);
                }

                else
                {
                    CurrentSectionID = -1;
                }
            }
        }

        #endregion

        #endregion

        #region Events

        private void OnExceedingContent(object sender, ExceedingContentEventArgs e)
        {
            var content = e.ExceedingItem;
            var oldPage = sender as ContentPage;
            var oldPageIndex = Pages.IndexOf(oldPage);
            
            oldPage.Remove(content);

            if (oldPageIndex != Pages.Count - 1) // Not last Page
            {
                var nextPage = Pages[oldPageIndex + 1];

                // Inserts content into beginning of next Page. Will validate all items forward automatically.

                nextPage.Insert(0, content);

                // WPF is not updating element arrangement (positions) if they not visible at window.
                // To bypass it, we could select next page (make visible) and update it's layout, after that - switch back.

                var pageControl = nextPage.GetPageControl();
                var oldIndex = CurrentPageIndex;

                CurrentPage = nextPage;
                pageControl.InvalidateArrange();
                pageControl.UpdateLayout();

                CurrentPage.ValidatePage();
                CurrentPageIndex = oldIndex;
            }

            else
            {
                // Otherwise, creates a new Page and inserts content to it
                var newPage = ContentPage.CreateWithPageControlInjection(oldPage);
                CurrentSection.Add(newPage);

                newPage.Insert(0, content);
                RaisePropertyChanged(nameof(Pages));
            }
        }

        /// <summary>
        /// Handles CurrentSection.PagesChanged event. CurrentPage should be already updated.
        /// </summary>
        private void OnPagesChanged(object sender, PagesChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Add)
            {
                foreach(var page in e.Pages)
                {
                    page.AddedExceedingContent += OnExceedingContent;
                }
            }

            else if(e.Action == CollectionChangeAction.Remove)
            {
                foreach (var page in e.Pages)
                {
                    page.AddedExceedingContent -= OnExceedingContent;
                }
            }
            
            UpdateCurrentPageNotFirst();
        }

        #endregion

        #region Commands

        #region Lesson Commands

        #region Lesson_Edit

        private DelegateCommand Lesson_EditCommand;
        public DelegateCommand Lesson_Edit =>
            Lesson_EditCommand ?? (Lesson_EditCommand = new DelegateCommand(ExecuteLesson_Edit, CanExecuteLesson_Edit)
            .ObservesProperty(() => ReadOnly)
            );

        void ExecuteLesson_Edit()
        {
            ReadOnly = false;
        }

        bool CanExecuteLesson_Edit()
        {
            return ReadOnly;
        }

        #endregion

        #region Lesson_StopEditing

        private DelegateCommand Lesson_StopEditingCommand;
        public DelegateCommand Lesson_StopEditing =>
            Lesson_StopEditingCommand ?? (Lesson_StopEditingCommand = new DelegateCommand(ExecuteLesson_StopEditing, CanExecuteLesson_StopEditing)
            .ObservesProperty(() => ReadOnly)
            );

        void ExecuteLesson_StopEditing()
        {
            ReadOnly = true;
        }

        bool CanExecuteLesson_StopEditing()
        {
            return !ReadOnly;
        }

        #endregion

        #region Lesson_UndoChanges

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

        #endregion

        #region Lesson_Save

        private DelegateCommand<Window> Lesson_SaveCommand;
        public DelegateCommand<Window> Lesson_Save =>
            Lesson_SaveCommand ?? (Lesson_SaveCommand = new DelegateCommand<Window>(ExecuteLesson_Save, CanExecuteLesson_Save));

        async void ExecuteLesson_Save(Window window) // Yes, async void ICommand. Why? - https://prismlibrary.com/docs/commanding.html
        {
            savingOrLoading = true;
            var saveDialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                AddExtension = true,
                Filter = model.LessonFilter,
                DefaultExt = "lsn",
            };

            if (saveDialog.ShowDialog(window) == true)
            {
                // New window
                var progressView = IOTools.CreateProgressView(window, model.ProgressWindowTitle_Saving, IOType.Write);
                var progress = IOTools.CreateProgressForProgressView(progressView);
               
                progressView.Show();

                // Pauses method until SaveAsync method is completed.
                var result = await LsnWriter.SaveAsync(this, saveDialog.FileName, progress);
                if (result == IOResult.Sucessful)
                {
                    HasChanges = false;
                }

                else if (result != IOResult.Cancelled)
                {
                    MessageBox.Show($"Unexpected behavior in saving process occured. Please contact developers. Result = {result}",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                progressView.Close();
            }
            savingOrLoading = false;
        }

        bool CanExecuteLesson_Save(Window window)
        {
            return !savingOrLoading;
        }

        #endregion

        #region Lesson_Load

        private DelegateCommand<Window> Lesson_LoadCommand;
        public DelegateCommand<Window> Lesson_Load =>
            Lesson_LoadCommand ?? (Lesson_LoadCommand = new DelegateCommand<Window>(ExecuteLesson_Load, CanExecuteLesson_Load));

        async void ExecuteLesson_Load(Window window) // Yes, async void ICommand. Why? - https://prismlibrary.com/docs/commanding.html
        {
            savingOrLoading = true;
            var loadDialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                AddExtension = true,
                Filter = model.LessonFilter,
                DefaultExt = "lsn",
            };

            if (loadDialog.ShowDialog(window) == true)
            {
                // New window
                var progressView = IOTools.CreateProgressView(window, model.ProgressWindowTitle_Loading, IOType.Read);
                var progress = IOTools.CreateProgressForProgressView(progressView);
                progressView.Show();

                // Sets ProgressCount property to total amount of (Section's) pages in file.
                var viewModel = progressView.DataContext as ProgressWindowViewModel;
                viewModel.SetCountData(await LsnReader.CountData(loadDialog.FileName));

                // Pauses method until LoadAsync will be completed.
                var result = await LsnReader.LoadAsync(loadDialog.FileName, progress);
                var resultCode = result.Item1;
                var resultLessonModel = result.Item2;

                if (resultCode == IOResult.Sucessful && resultLessonModel != null)
                {
                    ClearLesson();

                    var materialSections = SectionsByTab["Materials"];
                    var testSections = SectionsByTab["Tests"];

                    materialSections.AddRange(resultLessonModel.MaterialSections);
                    testSections.AddRange(resultLessonModel.TestSections);

                    UpdateSectionsEditable();

                    HasChanges = false;
                }

                else if (resultCode != IOResult.Cancelled)
                {
                    MessageBox.Show($"Unexpected behavior in saving process occured. Please contact developers. Result = {resultCode}",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                progressView.Close();
            }
            savingOrLoading = false;
        }

        bool CanExecuteLesson_Load(Window window)
        {
            return !savingOrLoading;
        }

        #endregion

        #region Lesson_New

        private DelegateCommand Lesson_NewCommand;
        public DelegateCommand Lesson_New =>
            Lesson_NewCommand ?? (Lesson_NewCommand = new DelegateCommand(ExecuteLesson_New));

        void ExecuteLesson_New()
        {
            var result = MessageBox.Show(model.NewLessonMessageText, model.NewLessonMessageHeader, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                ClearLesson();
            }
        }

        #endregion

        #endregion

        #region UI Commands

        #region AddSection

        private DelegateCommand AddSectionCommand;
        public DelegateCommand AddSection =>
            AddSectionCommand ?? (AddSectionCommand = new DelegateCommand(ExecuteAddSection, CanExecuteAddSection)
            .ObservesProperty(() => ReadOnly));

        void ExecuteAddSection()
        {
            int repeatingIndex = 1;
            string sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());

            while (Sections.Any(section => section.GetTitle() == sectionTitle))
            {
                // While Sections contains section with Title equal to sectionTitle

                repeatingIndex++;
                sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());
            }


            var sectionType = SelectedTabToContentType();
            var newSection = new Section(sectionType);
            newSection.SetTitle(sectionTitle);

            // Updates IsEditable state of Section

            newSection.SetEditable(!ReadOnly);

            // Adds newSection key to LastSelectedPage dictionary.

            model.LastSelectedPage.Add(newSection, null);

            // Adds newSection to Sections dictionary.
            Sections.Add(newSection);

            if (CurrentSection == null)
            {
                SelectSection(newSection);

                // Page was added from Section constructor, so we raise handler with this page.
                OnPagesChanged(newSection, new PagesChangedEventArgs(newSection.GetPages(), CollectionChangeAction.Add));
            }

            HasChanges = true;
        }

        bool CanExecuteAddSection()
        {
            return !ReadOnly;
        }

        #endregion

        #region RemoveSection

        private DelegateCommand<Section> RemoveSectionCommand;
        public DelegateCommand<Section> RemoveSection =>
            RemoveSectionCommand ?? (RemoveSectionCommand = new DelegateCommand<Section>(ExecuteRemoveSection));

        void ExecuteRemoveSection(Section section)
        {
            // Unselect

            SelectSection(null);

            // Remove

            Sections.Remove(section);

            // Notify

            HasChanges = true;
        }

        #endregion

        #region TrySelectSection

        private DelegateCommand<Section> TrySelectSectionCommand;
        public DelegateCommand<Section> TrySelectSection =>
            TrySelectSectionCommand ?? (TrySelectSectionCommand = new DelegateCommand<Section>(ExecuteTrySelectSection));

        void ExecuteTrySelectSection(Section newSection)
        {
            SelectSection(newSection);
        }

        #endregion

        #region SectionInput

        private DelegateCommand SectionInputCommand;
        public DelegateCommand SectionInput =>
            SectionInputCommand ?? (SectionInputCommand = new DelegateCommand(ExecuteSectionInput));

        void ExecuteSectionInput()
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                if (Keyboard.IsKeyDown(Key.C))
                {
                    var dataObject = new DataObject(DataFormats.Serializable, CurrentSection);
                    Clipboard.Clear();
                    Clipboard.SetDataObject(dataObject);

                }

                if (Keyboard.IsKeyDown(Key.V))
                {
                    if(ReadOnly) { return; }

                    IDataObject dataObject = Clipboard.GetDataObject();
                    if (dataObject.GetDataPresent(DataFormats.Serializable))
                    {
                        // Serializes Section

                        var section = dataObject.GetData(DataFormats.Serializable) as Section;

                        // Checks if SelectedTab (as ContentType) is same as Section's ContentType

                        if (SelectedTabToContentType() == section.ContentType)
                        {
                            section.SetEditable(!ReadOnly);

                            // Adds to sections

                            Sections.Add(section);

                            // Selects copied Section

                            SelectSection(section);
                        }


                    }
                }

            }
        }

        #endregion

        #region Add/Remove Content Commands

        #region AddMaterial

        private DelegateCommand<string> AddMaterialCommand;
        public DelegateCommand<string> AddMaterial =>
            AddMaterialCommand ?? (AddMaterialCommand = new DelegateCommand<string>(ExecuteAddMaterial));

        void ExecuteAddMaterial(string MaterialName)
        {
            IContentControl control;

            // Instantiation of ContentControl
            switch (MaterialName)
            {
                case "Text":
                    control = new TextControl();
                    break;
                default:
                    throw new NotImplementedException($"{MaterialName} not supported!");
            }

            AddContentControl(control);

        }

        #endregion

        #region AddTest

        private DelegateCommand<string> AddTestCommand;
        public DelegateCommand<string> AddTest =>
            AddTestCommand ?? (AddTestCommand = new DelegateCommand<string>(ExecuteAddTest));

        void ExecuteAddTest(string TestName)
        {
            IContentControl control;

            // Instantiation of ContentControl
            switch (TestName)
            {
                case "SimpleTest":
                    control = new SimpleTest();
                    break;
                default:
                    throw new NotImplementedException($"{TestName} not supported!");
            }

            AddContentControl(control);
        }

        #endregion

        #endregion

        #region Page Commands

        #region RemovePage

        private DelegateCommand RemovePageCommand;
        public DelegateCommand RemovePage =>
            RemovePageCommand ?? (RemovePageCommand = new DelegateCommand(ExecuteRemovePage));

        void ExecuteRemovePage()
        {
            Pages.Remove(CurrentPage);

            // Back to previous page index

            CurrentPageIndex++;

            HasChanges = true;
        }

        #endregion

        #endregion

        #endregion

        #region Event-Commands 

        /*
        * Event-Commands handles WPF Events and execute binded Commands instead of EventHandlers.
        * This is used to avoid code-behind and put event handling at ViewModel.
        */

        #region OnTabChanged

        private DelegateCommand<string> OnTabChangedCommand;
        public DelegateCommand<string> OnTabChanged =>
            OnTabChangedCommand ?? (OnTabChangedCommand = new DelegateCommand<string>(ExecuteOnTabChanged));

        void ExecuteOnTabChanged(string param)
        {
            TryCollapseCurrentSection();

            Section previousSection = null;

            if (CurrentSectionID != -1)
            {
                previousSection = Sections[CurrentSectionID];
            }

            SelectedTab = param;

            RaisePropertyChanged(nameof(Sections)); // Sections[SelectedTab]

            SelectSection(model.LastSelectedSection[param], previousSection, true);

            // We still should call RaisePropertyChanged, because we bind to ID in View, and when changing tabs, ID could be the same.
            RaisePropertyChanged(nameof(CurrentSectionID));
        }

        #endregion

        #endregion

        #endregion

        #region Localisation

        #region Lesson Menu

        public string LessonHeader
        {
            get { return model.LessonHeader; }
        }

        public string EditHeader
        {
            get { return model.EditHeader; }
        }

        public string StopEditingHeader
        {
            get { return model.StopEditingHeader; }
        }

        public string UndoChangesHeader
        {
            get { return model.UndoChangesHeader; }
        }

        public string RecentHeader
        {
            get { return model.RecentHeader; }
        }

        public string NewLessonHeader
        {
            get { return model.NewLessonHeader; }
        }

        public string SaveLessonHeader
        {
            get { return model.SaveLessonHeader; }
        }

        public string LoadLessonHeader
        {
            get { return model.LoadLessonHeader; }
        }

        public string CloseLessonHeader
        {
            get { return model.CloseLessonHeader; }
        }

        public string PrintLessonHeader
        {
            get { return model.PrintLessonHeader; }
        }

        public string ExitHeader
        {
            get { return model.ExitHeader; }
        }

        #endregion

        #region Tabs

        public string MaterialHeader
        {
            get { return model.MaterialHeader; }
        }

        public string TestsHeader
        {
            get { return model.TestsHeader; }
        }

        #endregion

        #region Buttons

        public string ButtonAddHeader
        {
            get { return model.ButtonAddHeader; }
        }

        public string ButtonRemoveHeader
        {
            get { return model.ButtonRemoveHeader; }
        }

        public string AddContentHeader
        {
            get { return model.AddContentHeader; }
        }

        #endregion

        #region Sections

        #region MaterialControls

        public string AudioHeader
        {
            get { return model.AudioHeader; }
        }

        public string ImageHeader
        {
            get { return model.ImageHeader; }
        }

        public string JokeHeader
        {
            get { return model.JokeHeader; }
        }

        public string TextHeader
        {
            get { return model.TextHeader; }
        }
        public string VideoHeader
        {
            get { return model.VideoHeader; }
        }

        #endregion

        #region TestControls

        public string ActionsInCaseHeader
        {
            get { return model.ActionsInCaseHeader; }
        }

        public string CompareHeader
        {
            get { return model.CompareHeader; }
        }

        public string DifferencesHeader
        {
            get { return model.DifferencesHeader; }
        }
        public string LinkTogetherHeader
        {
            get { return model.LinkTogetherHeader; }
        }

        public string MiniGameHeader
        {
            get { return model.MiniGameHeader; }
        }

        public string PrioritiesHeader
        {
            get { return model.PrioritiesHeader; }
        }

        public string SelectCorrectHeader
        {
            get { return model.SelectCorrectHeader; }
        }

        public string SIGameHeader
        {
            get { return model.SIGameHeader; }
        }

        public string SimpleTestHeader
        {
            get { return model.SimpleTestHeader; }
        }

        public string TrickyQuestionHeader
        {
            get { return model.TrickyQuestionHeader; }
        }

        public string TrueFalseHeader
        {
            get { return model.TrueFalseHeader; }
        }

        #endregion

        #endregion

        #region ToolTips

        public string ReadOnlyToolTip
        {
            get { return model.ReadOnlyToolTip; }
        }

        #endregion

        #endregion

        
    }
}
