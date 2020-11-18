using Lessium.ContentControls;
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

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private properties

        private readonly MainWindowModel model;

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
                    foreach (var section in Sections)
                    {
                        section.SetEditable(!model.ReadOnly);
                    }
                }
            }
        }

        #endregion

        #region Tabs

        public string SelectedTab
        {
            get { return model.SelectedTab; }
            set { SetProperty(ref model.SelectedTab, value); }
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

        public Section CurrentSection
        {
            get { return model.CurrentSection[SelectedTab]; }
            set
            {
                if (SetDictionaryProperty(ref model.CurrentSection, SelectedTab, value))
                {
                    if (CurrentSection == null)
                    {
                        CurrentSectionID = -1;
                    }

                    else
                    {
                        CurrentSectionID = Sections.IndexOf(value);
                    }
                }
            }
        }

        public bool CurrentPageIsEmpty
        {
            get { return CurrentPage?.Items.Count == 0; }
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
                SetDictionaryProperty(ref model.CurrentSectionID, SelectedTab, value);
            }
        }

        #endregion

        #region Pages

        public ObservableCollection<ContentPage> Pages
        {
            get { return CurrentSection.GetPages(); }
            set
            {
                CurrentSection.SetPages(value);
            }
        }

        public int CurrentPageIndex
        {
            get { return model.CurrentPageIndex; }
            set
            {
                CurrentSection.SetSelectedPageIndex(value);
                SetProperty(ref model.CurrentPageIndex, value);
            }
        }

        public int CurrentPageNumber
        {
            get { return model.CurrentPageIndex + 1; }
            set
            {
                // Validates index

                var number = value;
                if (number <= 0) { number = 1; }
                else if (number > Pages.Count) { number = Pages.Count; }

                var index = number - 1;

                CurrentSection.SetSelectedPageIndex(index);
                SetProperty(ref model.CurrentPageIndex, index);
            }
        }

        public ContentPage CurrentPage
        {
            get { return model.CurrentPage; }
            set
            {
                CurrentSection.SetSelectedPage(value);
                SetProperty(ref model.CurrentPage, value);
            }
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

        public SectionType SelectedTabToSectionType()
        {
            if (SelectedTab == "Materials")
            {
                return SectionType.MaterialSection;
            }

            return SectionType.TestSection;
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
            if (CurrentSection != null)
            {
                CollapseSection(CurrentSection);
            }
        }

        private void SelectSection(Section section)
        {
            if (CurrentSection == section) { return; }

            TryCollapseCurrentSection(); // colapses old selected section

            CurrentSection = section;

            if (section != null)
            {
                // Updates CurrentPage

                CurrentPageIndex = section.GetSelectedPageIndex();
                CurrentPage = section.GetSelectedPage();

                // Shows Section

                ShowSection(section);
            }

            
        }

        #endregion

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

        // Enables editing only when at ReadOnly
        bool CanExecuteLesson_Edit()
        {
            return ReadOnly;
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

        #endregion

        #region Lesson_New

        private DelegateCommand Lesson_NewCommand;
        public DelegateCommand Lesson_New =>
            Lesson_NewCommand ?? (Lesson_NewCommand = new DelegateCommand(ExecuteLesson_New));

        void ExecuteLesson_New()
        {
            // TODO: Implement new lesson
            HasChanges = true;
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


            var sectionType = SelectedTabToSectionType();
            var newSection = new Section(sectionType);
            newSection.SetTitle(sectionTitle);

            // Updates IsEditable state of Section

            newSection.SetEditable(!ReadOnly);

            // Adds newSection to Sections dictionary.
            Sections.Add(newSection);

            SelectSection(newSection);

            HasChanges = true;
        }

        bool CanExecuteAddSection()
        {
            return !ReadOnly;
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
                    var serializationInfo = CurrentSection.GetSerializationInfo();
                    Clipboard.Clear();
                    Clipboard.SetDataObject(serializationInfo);

                }

                if (Keyboard.IsKeyDown(Key.V))
                {
                    IDataObject dataObject = Clipboard.GetDataObject();
                    var dataType = typeof(SectionSerializationInfo);
                    if (dataObject.GetDataPresent(dataType))
                    {
                        // Retrieves SerializationInfo

                        var info = dataObject.GetData(dataType) as SectionSerializationInfo;

                        // Checks if SelectedTab (as SectionType) is same as SectionType

                        if (SelectedTabToSectionType() == info.sectionType)
                        {
                            // Constructs section from SerializationInfo

                            var section = new Section(info);

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
                    control = new Text();
                    break;
                default:
                    throw new NotImplementedException($"{MaterialName} not supported!");
            }

            CurrentPage.Add(control);

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

            CurrentPage.Add(control);
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

            SelectedTab = param;

            RaisePropertyChanged("Sections"); // Sections[SelectedTab]
            RaisePropertyChanged("CurrentSection");
            RaisePropertyChanged("CurrentSectionID"); // Binds to (new) CurrentSectionID based on tab.

            if (CurrentSection != null)
            {
                ShowSection(CurrentSection); // Shows (new) CurrentSection based on tab.
            }
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
