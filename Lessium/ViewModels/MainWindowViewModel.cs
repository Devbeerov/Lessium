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
using Lessium.ContentControls.TestControls;
using System.ComponentModel;
using Microsoft.Win32;
using Lessium.Classes.IO;
using Lessium.Utility;
using System.Diagnostics;
using Lessium.Views;
using System.Text;
using Lessium.Properties;
using System.Threading.Tasks;
using Lessium.UndoableActions;
using System.Configuration;
using Lessium.Services;
using Lessium.UndoableActions.Generic;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private fields

        // Model

        private readonly MainModel model;

        // Services

        private readonly UndoableActionsService actionsService;
        private readonly HotkeysService hotkeysService;

        // Logical variables

        private bool currentPageIsNotFirst = false;
        private bool savingOrLoading = false;
        private bool currentSectionUpdating = false;

        // Windows

        private Window mainWindow = null;
        private Window settingsWindow = null;
        private Window aboutWindow = null;

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
            get { return actionsService.ExecutedActionsCount > 0; }
        }

        public bool IsEditable
        {
            get { return model.IsEditable; }
            set 
            {
                if (SetProperty(ref model.IsEditable, value))
                    CheckPageTests.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Tabs

        public ContentType SelectedContentType
        {
            get { return model.SelectedContentType; }
            set
            {
                var prevTab = model.SelectedContentType;
                var prevSection = model.CurrentSection[prevTab];

                if (SetProperty(ref model.SelectedContentType, value)) // Will change CurrentSection property, because it's Get method bound with SelectedContentType
                {
                    // Updates previous

                    model.CurrentSection[prevTab] = prevSection;
                }
            }
        }

        public bool SelectedTabIsMaterials
        {
            get { return model.SelectedContentType == ContentType.Material; }
        }

        public bool SelectedTabIsTests
        {
            get { return model.SelectedContentType == ContentType.Test; }
        }

        public ObservableCollection<Section> Sections
        {
            get { return model.Sections[SelectedContentType]; }
            set
            {
                SetDictionaryProperty(ref model.Sections, SelectedContentType, value);
            }
        }

        public Dictionary<ContentType, ObservableCollection<Section>> SectionsByType
        {
            get { return model.Sections; }
        }

        public Dictionary<ContentType, double> TabsVerticalScrollOffsets
        {
            get { return model.TabsVerticalScrollOffsets; }
        }

        // Use SelectSection method to "Set" CurrentSection.
        public Section CurrentSection
        {
            get { return model.CurrentSection[SelectedContentType]; }
        }

        // Should be used exclusively for binding!
        public int CurrentSectionID
        {
            get
            {
                return model.CurrentSectionID[SelectedContentType];
            }

            set
            {
                if (SetDictionaryProperty(ref model.CurrentSectionID, SelectedContentType, value))
                {
                    // Returns if there is no Section.

                    if (value == -1) { return; }

                    // Otherwise selects Section.

                    SelectSection(Sections[value]);
                }
            }
        }

        #endregion

        #region Pages

        public ObservableCollection<ContentPageModel> Pages
        {
            get { return CurrentSection?.Pages; }
            set
            {
                // Not SetProperty! We don't check Collection for equality, and we don't need.
                // It's dependency property anyway. So we can do it just in single line like that.
                CurrentSection.Pages = value;
            }
        }

        public int CurrentPageIndex
        {
            get
            {
                if (CurrentSection == null) { return -1; }

                return CurrentSection.SelectedPageIndex;
            }
            set
            {
                if (CurrentPageIndex != value)
                {
                    if (value < 0) { value = 0; }
                    else if (value >= Pages.Count) { value = Pages.Count - 1; }

                    CurrentSection.SelectedPageIndex = value;
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

                if (number <= 0) number = 1;
                else if (number > Pages.Count) number = Pages.Count;

                var index = number - 1;

                CurrentPageIndex = index;

                RaisePropertyChanged();
            }
        }

        public ContentPageModel CurrentPage
        {
            get { return CurrentSection?.SelectedPage; }
            set
            {
                if (CurrentPage == value) return;

                // Update value

                CurrentSection.SelectedPage = value;

                if (CurrentPage == null)
                {
                    CurrentPageIndex = -1;
                }

                else
                {
                    CurrentPageIndex = CurrentSection.Pages.IndexOf(value);
                }

                UpdateCurrentPageNotFirst();
                RaisePropertyCurrentPageChanged();
            }
        }

        public bool CurrentPageIsNotFirst
        {
            get { return currentPageIsNotFirst; }
            set { SetProperty(ref currentPageIsNotFirst, value); }
        }

        public int TotalTestsCount
        {
            get { return model.TotalTestsCount; }
            set { SetProperty(ref model.TotalTestsCount, value); }
        }

        public int CorrectTestsCount
        {
            get { return model.CorrectTestsCount; }
            set { SetProperty(ref model.CorrectTestsCount, value); }
        }

        #endregion

        #region Buttons

        public string AddSectionText
        {
            get { return model.AddSectionText; }
        }

        #endregion

        #endregion

        #region Constructors

        public MainWindowViewModel(MainModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if (model == null)
            {
                model = new MainModel();
            }

            this.model = model;

            // Retrieves MainWindow

            mainWindow = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault();

            // Creates service

            actionsService = new UndoableActionsService(mainWindow, () => RaiseHasChanges());
            hotkeysService = new HotkeysService(mainWindow);

            // Setup hotkeys

            SetupHotkeys();

            // Attach handlers to hotkeys changes

            Hotkeys.Current.PropertyChanged += OnHotkeyChanged;
            Hotkeys.Current.SettingChanging += OnHotkeyChanging;
        }

        #endregion

        #region Methods

        #region Public

        public SendActionEventHandler GetSendActionEventHandler()
        {
            return OnContentReceived;
        }

        #endregion

        #region Private

        /// <param name="name">If explicitly provided null, then won't call RaisePropertyChanged.</param>
        private bool SetDictionaryProperty<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, TKey key, TValue newValue, [CallerMemberName] string name = null)
        {
            var oldValue = dictionary[key];

            if (oldValue == null || !ReferenceEquals(oldValue, newValue))
            {
                dictionary[key] = newValue;

                if (name != null) { RaisePropertyChanged(name); }

                return true;
            }

            return false;
        }

        private void AddContentControl(IContentControl control)
        {
            actionsService.ExecuteAction(new AddContentAction(CurrentPage, control));
        }

        private void InsertContentControl(IContentControl control, int position)
        {
            actionsService.ExecuteAction(new InsertContentAction(CurrentPage, control, position));
        }

        private void RemoveContentControl(IContentControl control)
        {
            actionsService.ExecuteAction(new RemoveContentAction(CurrentPage, control));
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
                model.CurrentSection[key] = null;
            }

            // Clears changes as well.

            ClearChanges();

            // Raises PropertyChanged

            RaisePropertyChanged(nameof(Sections));

            // Selects no section

            SelectSection(null);
        }

        private LessonModel GenerateLessonModel()
        {
            var lessonModel = new LessonModel();

            lessonModel.MaterialSections.AddRange(SectionsByType[ContentType.Material]);
            lessonModel.TestSections.AddRange(SectionsByType[ContentType.Test]);

            return lessonModel;
        }

        /// <summary>
        /// Clears all undo and redo actions in actionsSerivce, also raises PropertyChanged of HasChanges.
        /// </summary>
        private void ClearChanges()
        {
            actionsService.Clear();

            // Updates CanExecute

            Lesson_UndoChanges.RaiseCanExecuteChanged();
            Lesson_RedoChanges.RaiseCanExecuteChanged();
        }

        private void AddSectionToSections(Section section)
        {
            actionsService.ExecuteAction(new AddToCollectionAction<Section>(Sections, section));
        }

        private void RaiseHasChanges()
        {
            RaisePropertyChanged(nameof(HasChanges));
        }

        private void SwitchEditable(bool editable)
        {
            IsEditable = editable;
        }

        private void SwitchEditable()
        {
            SwitchEditable(!IsEditable);
        }

        private string BuildHeaderWithHotkey(string defaultHeader, Hotkey hotkey)
        {
            var sb = new StringBuilder();

            sb.Append(defaultHeader);
            sb.Append($" ({hotkey})");

            return sb.ToString();
        }

        #endregion

        #region Hotkeys

        private void SetupHotkeys()
        {
            RegisterEditHotkey();
            RegisterUndoHotkey();
            RegisterRedoHotkey();
        }

        private bool TryUndo()
        {
            if (!IsEditable) return false;

            return actionsService.TryUndo();
        }

        private bool TryRedo()
        {
            if (!IsEditable) return false;

            return actionsService.TryRedo();
        }

        private void RegisterEditHotkey()
        {
            hotkeysService.RegisterHotkey(Hotkeys.Current.EditHotkey, () => SwitchEditable());
        }

        private void RegisterUndoHotkey()
        {
            hotkeysService.RegisterHotkey(Hotkeys.Current.UndoHotkey, () => TryUndo());
        }

        private void RegisterRedoHotkey()
        {
            hotkeysService.RegisterHotkey(Hotkeys.Current.RedoHotkey, () => TryRedo());
        }

        #endregion

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

        /// <summary>
        /// Should be used when changing tabs.
        /// Checks section with manually providen previousSection to avoid wrong comparison.
        /// </summary>
        /// <param name="section">New Section</param>
        /// <param name="previousSection">Previous Section, if provided - will also unsubscribe PagesChanged.</param>
        private void SelectSection(Section section, Section previousSection = null, bool tabChange = false)
        {
            // If not tabChange and failed to SetDictionaryProperty (new and old are same), then just returns.

            if (!tabChange && !SetDictionaryProperty(ref model.CurrentSection, SelectedContentType, section, null))
            {
                return;
            }

            // Using logical bool because of SelectionChanged event, which will be fired with old section, due to UI won't be updated yet.

            currentSectionUpdating = true;

            RaisePropertyCurrentSectionChanged();

            if (previousSection != null) 
                previousSection.PagesChanged -= OnPagesChanged;

            if (section == null) 
                return;

            section.PagesChanged += OnPagesChanged;

            CheckPageTests.RaiseCanExecuteChanged();

            ShowSection(section);

            currentSectionUpdating = false;
        }

        /// <summary>
        /// Will also call RaisePropertyChanged of CurrentPageIndex and CurrentPageNumber properties.
        /// </summary>
        private void RaisePropertyCurrentPageChanged()
        {
            RaisePropertyChanged(nameof(CurrentPage));
            RaisePropertyChanged(nameof(CurrentPageIndex));
            RaisePropertyChanged(nameof(CurrentPageNumber));
        }

        /// <summary>
        /// Will execute all RaisePropertyChanged to make all bindings updated.
        /// </summary>
        private void RaisePropertyCurrentSectionChanged()
        {
            RaisePropertyChanged(nameof(CurrentSection));
            RaisePropertyChanged(nameof(CurrentSectionID));
            RaisePropertyChanged(nameof(Pages));

            CurrentSectionID = Sections.IndexOf(CurrentSection);

            RaisePropertyCurrentPageChanged();
        }

        #endregion

        #endregion

        #region Events

        private void OnExceedingContent(object sender, ExceedingContentEventArgs e)
        {
            var content = e.ExceedingItem;
            var oldPage = sender as ContentPageModel;

            // If there is just only one item, will not move it to next Page.
            if (oldPage.Items.Count == 1) return;

            var oldPageIndex = Pages.IndexOf(oldPage);
            
            oldPage.Remove(content);

            if (oldPageIndex != Pages.Count - 1) // Not last Page
            {
                var nextPage = Pages[oldPageIndex + 1];

                // Inserts content into beginning of next Page. Will validate all items forward automatically.

                nextPage.Insert(0, content);

                // WPF is not updating element arrangement (positions) if they not visible at window.
                // To bypass it, we could select next page (make visible) and update it's layout, after that - switch back.

                var oldIndex = CurrentPageIndex;

                CurrentPage = nextPage;
                CurrentPage.ValidatePage();

                CurrentPageIndex = oldIndex;
            }

            else
            {
                // Otherwise, creates a new Page and inserts content to it
                var newPage = new ContentPageModel(oldPage.ContentType, DispatcherUtility.Dispatcher);

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
                foreach (var page in e.Pages)
                {
                    page.AddedExceedingContent += OnExceedingContent;
                }
            }

            else if (e.Action == CollectionChangeAction.Remove)
            {
                foreach (var page in e.Pages)
                {
                    page.AddedExceedingContent -= OnExceedingContent;
                }
            }
            
            UpdateCurrentPageNotFirst();
        }

        private void OnContentReceived(object sender, SendActionEventArgs e)
        {
            switch (e.Action)
            {
                case SendActionEventArgs.ContentAction.Add:
                    AddContentControl(e.Content);
                    break;

                case SendActionEventArgs.ContentAction.Remove:
                    RemoveContentControl(e.Content);
                    break;

                case SendActionEventArgs.ContentAction.Insert:
                    InsertContentControl(e.Content, e.Position);
                    break;
            }
        }

        private void OnHotkeyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Hotkeys.EditHotkey):
                    RegisterEditHotkey();
                    RaisePropertyChanged(nameof(EditHeader));
                    break;

                case nameof(Hotkeys.UndoHotkey):
                    RegisterUndoHotkey();
                    RaisePropertyChanged(nameof(UndoChangesHeader));
                    break;

                case nameof(Hotkeys.RedoHotkey):
                    RegisterRedoHotkey();
                    RaisePropertyChanged(nameof(RedoChangesHeader));
                    break;
            }
        }

        private void OnHotkeyChanging(object sender, SettingChangingEventArgs e)
        {
            if (e.Cancel) return;

            var hotkey = (Hotkey)Hotkeys.Current[e.SettingName];
            hotkeysService.UnregisterHotkey(hotkey);
        }

        #endregion

        #region Commands

        #region Lesson Commands

        #region Lesson_Edit

        private DelegateCommand Lesson_EditCommand;
        public DelegateCommand Lesson_Edit =>
            Lesson_EditCommand ?? (Lesson_EditCommand = new DelegateCommand(ExecuteLesson_Edit, CanExecuteLesson_Edit)
            .ObservesProperty(() => IsEditable)
            );

        void ExecuteLesson_Edit()
        {
            SwitchEditable(true);
        }

        bool CanExecuteLesson_Edit()
        {
            return !IsEditable;
        }

        #endregion

        #region Lesson_StopEditing

        private DelegateCommand Lesson_StopEditingCommand;
        public DelegateCommand Lesson_StopEditing =>
            Lesson_StopEditingCommand ?? (Lesson_StopEditingCommand = new DelegateCommand(ExecuteLesson_StopEditing, CanExecuteLesson_StopEditing)
            .ObservesProperty(() => IsEditable)
            );

        void ExecuteLesson_StopEditing()
        {
            SwitchEditable(false);
        }

        bool CanExecuteLesson_StopEditing()
        {
            return IsEditable;
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
            TryUndo();
        }

        bool CanExecuteLesson_UndoChanges()
        {
            return HasChanges;
        }

        #endregion

        #region Lesson_RedoChanges

        private DelegateCommand Lesson_RedoChangesCommand;
        public DelegateCommand Lesson_RedoChanges =>
            Lesson_RedoChangesCommand ?? (Lesson_RedoChangesCommand = new DelegateCommand(ExecuteLesson_RedoChanges, CanExecuteLesson_RedoChanges)
            .ObservesProperty(() => HasChanges) // Observes HasChanges, because it is callback of actionsService.
            );

        void ExecuteLesson_RedoChanges()
        {
            TryRedo();
        }

        bool CanExecuteLesson_RedoChanges()
        {
            return actionsService.UndoneActionsCount > 0; // But we check UndoneActionsCount instead of HasChanges!
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
                var fileName = saveDialog.FileName;
                var lessonModel = GenerateLessonModel();

                // New window
                var progressView = IOTools.CreateProgressView(window, model.ProgressWindowTitle_Saving,
                    await LsnWriter.CountDataAsync(lessonModel, fileName), IOType.Write);
                var progress = IOTools.CreateProgressForProgressView(progressView);
               
                progressView.Show();

                // Awaits until SaveAsync method is completed asynchronously.

                var result = await Task.Run(async () => await LsnWriter.SaveAsync(lessonModel, fileName, progress));

                if (result != IOResult.Cancelled)
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
                Dictionary<ContentType, CountData> countData = null;

                try
                {
                    countData = await Task.Run(async () => await LsnReader.CountDataAsync(loadDialog.FileName));
                }

                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    // CountDataAsync failed, whatever by XmlSchema validation or by some other exception, we just returns.
                    savingOrLoading = false;
                    return;
                }

                // New window

                var progressView = IOTools.CreateProgressView(window, model.ProgressWindowTitle_Loading,
                    countData, IOType.Read);
                var progress = IOTools.CreateProgressForProgressView(progressView);

                progressView.Show();

                (IOResult, LessonModel) result = (IOResult.Null, null);
                try
                {
                    result = await Task.Run(async () => await LsnReader.LoadAsync(loadDialog.FileName, progress));
                }

                finally
                {
                    // Will be executed whatever LoadAsync was successful or not.

                    progressView.Close();
                }

                // Code below will be executed only if no exceptions occured during LoadAsync.

                var resultCode = result.Item1;
                var resultLessonModel = result.Item2;

                if (resultCode == IOResult.successful && resultLessonModel != null)
                {
                    ClearLesson();

                    var materialSections = SectionsByType[ContentType.Material];
                    var testSections = SectionsByType[ContentType.Test];

                    materialSections.AddRange(resultLessonModel.MaterialSections);
                    testSections.AddRange(resultLessonModel.TestSections);
                }

                else if (resultCode != IOResult.Cancelled)
                {
                    MessageBox.Show($"Unexpected behavior in saving process occured. Please contact developers. Result = {resultCode}",
                        "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            .ObservesProperty(() => IsEditable));

        void ExecuteAddSection()
        {
            int repeatingIndex = 1;
            string sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());

            while (Sections.Any(section => section.Title == sectionTitle))
            {
                // While Sections contains section with Title equal to sectionTitle

                repeatingIndex++;
                sectionTitle = string.Format("{0} {1}", model.NewSection, repeatingIndex.ToString());
            }


            var newSection = new Section(SelectedContentType)
            {
                Title = sectionTitle
            };

            // Adds newSection to Sections dictionary.

            AddSectionToSections(newSection);

            if (CurrentSection == null)
            {
                SelectSection(newSection);

                // Page was added from Section constructor, so we raise handler with this page.

                OnPagesChanged(newSection, new PagesChangedEventArgs(newSection.Pages, CollectionChangeAction.Add));
            }
        }

        bool CanExecuteAddSection()
        {
            return IsEditable;
        }

        #endregion

        #region RemoveSection

        private DelegateCommand<Section> RemoveSectionCommand;
        public DelegateCommand<Section> RemoveSection =>
            RemoveSectionCommand ?? (RemoveSectionCommand = new DelegateCommand<Section>(ExecuteRemoveSection, CanExecuteRemoveSection));

        void ExecuteRemoveSection(Section section)
        {
            // Unselect

            SelectSection(null);

            // Remove

            actionsService.ExecuteAction(new RemoveFromCollectionAction<Section>(Sections, section));
        }

        bool CanExecuteRemoveSection(Section _) // Discard Section, because we don't check it, we check only IsEditable state.
        {
            return IsEditable;
        }

        #endregion

        #region TrySelectSection

        private DelegateCommand<Section> TrySelectSectionCommand;
        public DelegateCommand<Section> TrySelectSection =>
            TrySelectSectionCommand ?? (TrySelectSectionCommand = new DelegateCommand<Section>(ExecuteTrySelectSection));

        void ExecuteTrySelectSection(Section newSection)
        {
            if (currentSectionUpdating) return;

            SelectSection(newSection);
        }

        #endregion

        #region SectionInput

        private DelegateCommand SectionInputCommand;
        public DelegateCommand SectionInput =>
            SectionInputCommand ?? (SectionInputCommand = new DelegateCommand(ExecuteSectionInput));

        void ExecuteSectionInput()
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;

            if (Keyboard.IsKeyDown(Key.C))
            {
                if (CurrentSection == null) return;

                ClipboardService.CopySerializable(CurrentSection);

                return;
            }

            if (Keyboard.IsKeyDown(Key.V))
            {
                // We don't paste if not IsEditable

                if (!IsEditable) { return; }

                var section = ClipboardService.GetStoredSerializable() as Section;

                // Checks if SelectedContentType is same as Section's ContentType

                if (SelectedContentType != section.ContentType) return;

                // Adds to Sections

                AddSectionToSections(section);

                // Selects copied Section

                SelectSection(section);
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

            switch (MaterialName)
            {
                case nameof(TextControl):
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

            switch (TestName)
            {
                case nameof(SimpleTest):
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
            actionsService.ExecuteAction(new RemoveFromCollectionAction<ContentPageModel>(Pages, CurrentPage)
            {
                Callback = () =>
                {
                    CurrentPageIndex--;
                },
            });
        }

        #endregion

        #region CheckPageTests

        private DelegateCommand CheckPageTestsCommand;
        public DelegateCommand CheckPageTests =>
            CheckPageTestsCommand ?? (CheckPageTestsCommand = new DelegateCommand(ExecuteCheckPageTests, CanExecuteCheckPageTests));

        void ExecuteCheckPageTests()
        {
            int correctTests = 0;

            foreach (var item in CurrentPage.Items)
            {
                var testControl = item as ITestControl;
                var correct = testControl.CheckAnswers();

                if (correct)
                    correctTests++;
            }
        }

        bool CanExecuteCheckPageTests()
        {
            return !IsEditable &&
                CurrentPage != null &&
                CurrentPage.ContentType == ContentType.Test;
        }

        #endregion

        #endregion

        #region ShowSettings

        private DelegateCommand ShowSettingsCommand;
        public DelegateCommand ShowSettings =>
            ShowSettingsCommand ?? (ShowSettingsCommand = new DelegateCommand(ExecuteShowSettings, CanExecuteShowSettings));

        void ExecuteShowSettings()
        {
            // New window

            settingsWindow = new SettingsWindow()
            {
                Owner = mainWindow
            };

            settingsWindow.Closed += OnSettingsClosed;
            settingsWindow.Show();
        }

        bool CanExecuteShowSettings()
        {
            return settingsWindow == null;
        }

        #endregion

        #region ShowAbout

        private DelegateCommand ShowAboutCommand;
        public DelegateCommand ShowAbout =>
            ShowAboutCommand ?? (ShowAboutCommand = new DelegateCommand(ExecuteShowAbout, CanExecuteShowAbout));

        void ExecuteShowAbout()
        {
            // New window

            aboutWindow = new AboutView()
            {
                Owner = mainWindow
            };

            aboutWindow.Closed += OnAboutClosed;
            aboutWindow.Show();
        }

        bool CanExecuteShowAbout()
        {
            return aboutWindow == null;
        }

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

            SelectedContentType = (ContentType) Enum.Parse(typeof(ContentType), param);

            RaisePropertyChanged(nameof(Sections)); // Sections[SelectedContentType]

            // SelectSection should be used here, see implementation for details.

            SelectSection(model.CurrentSection[SelectedContentType], previousSection, true);
        }

        #endregion

        #region OnSettingsClosed

        private void OnSettingsClosed(object sender, EventArgs e)
        {
            settingsWindow = null;
        }

        #endregion

        #region OnAboutClosed

        private void OnAboutClosed(object sender, EventArgs e)
        {
            aboutWindow = null;
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
            get { return BuildHeaderWithHotkey(model.EditHeader, Hotkeys.Current.EditHotkey); }
        }

        public string StopEditingHeader
        {
            get { return BuildHeaderWithHotkey(model.StopEditingHeader, Hotkeys.Current.EditHotkey); }
        }

        public string UndoChangesHeader
        {
            get { return BuildHeaderWithHotkey(model.UndoChangesHeader, Hotkeys.Current.UndoHotkey); }
        }

        public string RedoChangesHeader
        {
            get { return BuildHeaderWithHotkey(model.RedoChangesHeader, Hotkeys.Current.RedoHotkey); }
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

        public string ExitHeader
        {
            get { return model.ExitHeader; }
        }

        #endregion

        #region Tabs

        public string Materials
        {
            get { return model.Materials; }
        }

        public string Tests
        {
            get { return model.Tests; }
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

        public string TextHeader
        {
            get { return model.TextHeader; }
        }

        #endregion

        #region TestControls

        public string SimpleTestHeader
        {
            get { return model.SimpleTestHeader; }
        }

        #endregion

        #endregion

        #region ToolTips

        public string ReadOnlyToolTip
        {
            get { return model.ReadOnlyToolTip; }
        }

        public string CheckAnswersToolTip
        {
            get { return model.CheckAnswersToolTip; }
        }

        #endregion

        #endregion


    }
}
