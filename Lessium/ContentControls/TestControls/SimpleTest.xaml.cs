using Lessium.Classes.IO;
using Lessium.ContentControls.MaterialControls;
using Lessium.ContentControls.TestControls.AnswerModels;
using Lessium.CustomControls;
using Lessium.Interfaces;
using Lessium.Services;
using Lessium.UndoableActions.Generic;
using Lessium.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Lessium.ContentControls.TestControls
{
    /// <summary>
    /// Simple test with multiple answers.
    /// </summary>
    [Serializable]
    public partial class SimpleTest : UserControl, ITestControl<SimpleAnswerModel>
    {
        private Guid id;
        private readonly IDispatcher dispatcher;
        private AnswersMappingHelper<SimpleAnswerModel> mappingHelper;

        private bool raiseResizeEvent = true;

        // Serialization

        private StoredTestAnswers<SimpleAnswerModel> storedAnswers;

        #region CLR Properties

        public string Question { get; set; } = Properties.Resources.SimpleTestControl_DefaultText;

        public string GUID
        {
            get { return id.ToString(); }
        }

        private UndoableActionsService ActionsService
        {
            get { return UndoableActionsServiceLocator.GetService(this); }
        }

        #endregion

        #region Constructors

        public SimpleTest()
        {
            this.dispatcher = DispatcherUtility.Dispatcher;

            Initialize();
        }

        public SimpleTest(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            Initialize();
        }

        // For serialization
        protected SimpleTest(SerializationInfo info, StreamingContext context)
        {
            dispatcher = DispatcherUtility.Dispatcher;

            // Initializes component

            Initialize();

            // Serializes properties

            Question = info.GetString(nameof(Question));
            storedAnswers = new StoredTestAnswers<SimpleAnswerModel>(info, nameof(Answers), nameof(TrueAnswers), nameof(SelectedAnswers));
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            mappingHelper = new AnswersMappingHelper<SimpleAnswerModel>(TrueAnswers, SelectedAnswers);

            // Custom control initialization

            id = Guid.NewGuid();
            DataContext = this;

            ValidateAnswersSelectionMode();
            InitializeComponent();
        }

        #endregion

        #region Private

        private DynamicCheckBoxType GetActualCheckBoxType()
        {
            if (IsEditable) { return DynamicCheckBoxType.CheckBox; }

            if (TrueAnswers.Count < 2) return DynamicCheckBoxType.RadioSingle;

            return DynamicCheckBoxType.RadioMultiple;
        }

        private void ValidateAnswersSelectionMode()
        {
            CheckBoxType = GetActualCheckBoxType();
        }

        private SimpleAnswerModel FindAnswerModelFromElement(FrameworkElement element)
        {
            var answerControlContainer = element.FindParentOfName("dataPanel");

            // if dataPanel not found it means either it was because DynamicCheckBoxType was changed
            if (answerControlContainer == null) throw new NullReferenceException("Failed to find parent with name \"dataPanel\".");

            return answerControlContainer.DataContext as SimpleAnswerModel;
        }

        private void UpdateCheckboxes()
        {
            AnswersItemControl.UpdateCheckboxes(mappingHelper.GetCheckBoxTypes(TrueAnswers));
            AnswersItemControl.UpdateCheckboxes(mappingHelper.GetCheckBoxTypes(SelectedAnswers));
        }

        private void UpdateCheckboxesOnceAtLoad()
        {
            RoutedEventHandler updateAction = null;
            Action unsubscribeAction = delegate ()
            {
                AnswersItemControl.Loaded -= updateAction;
            };

            updateAction = new RoutedEventHandler((s, e) =>
            {
                UpdateCheckboxes();
                unsubscribeAction.Invoke();
            });

            AnswersItemControl.Loaded += updateAction;
        }

        #endregion

        #endregion

        #region IContentControl

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set
            {
                SetValue(IsEditableProperty, value);
            }
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(SimpleTest), new PropertyMetadata(false,
                new PropertyChangedCallback(OnEditableChanged)));

        private static void OnEditableChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as SimpleTest;

            control.ValidateAnswersSelectionMode();
        }

        #endregion

        #region ITestControl

        public IList<SimpleAnswerModel> Answers { get; set; } = new ObservableCollection<SimpleAnswerModel>();
        public IList<SimpleAnswerModel> TrueAnswers { get; set; } = new List<SimpleAnswerModel>();
        public IList<SimpleAnswerModel> SelectedAnswers { get; set; } = new List<SimpleAnswerModel>();

        public event NotifyCollectionChangedEventHandler AnswersChanged;

        IList ITestControl.Answers
        {
            get
            {
                return Answers.ToList();
            }
            set { throw new NotSupportedException(); }
        }

        IList ITestControl.TrueAnswers
        {
            get
            {
                return TrueAnswers.ToList();
            }
            set { throw new NotSupportedException(); }
        }

        IList ITestControl.SelectedAnswers
        {
            get
            {
                return SelectedAnswers.ToList();
            }
            set { throw new NotSupportedException(); }
        }

        public bool CheckAnswers()
        {
            return TrueAnswers.SequenceEqual(SelectedAnswers);
        }

        #endregion

        #region Events

        private void AnswersItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!raiseResizeEvent) { return; }

            if (addAnswerButton.Visibility == Visibility.Collapsed) return;
            
            var buttonPos = addAnswerButton.TranslatePoint(default, AnswersItemControl);
            var lineHeight = testQuestion.textBox.CalculateLineHeight();
            var maximumValidPosY = MaxHeight - addAnswerButton.ActualHeight - lineHeight;
            var fits = buttonPos.Y + addAnswerButton.ActualHeight <= maximumValidPosY;

            addAnswerButton.IsEnabled = fits;

            // Sets source to SimpleTest Control, not Border

            e.Source = this;
            raiseResizeEvent = true;
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            var answer = new SimpleAnswerModel();

            ActionsService.ExecuteAction(new AddToCollectionAction<SimpleAnswerModel>(Answers, answer));
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (TextControl)e.Source;

            foreach (var answer in Answers)
            {
                var text = textControl.Text;

                if (ReferenceEquals(answer.Text, text)) // Checks for reference equality, not value!
                {
                    ActionsService.ExecuteAction(new RemoveFromCollectionAction<SimpleAnswerModel>(Answers, answer));

                    return;
                }
            }
        }

        private void TextContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl;

            control.IsEditable = IsEditable;
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl;

            control.IsEditable = IsEditable;
        }

        private void ToggleAnswerTrue_Checked(object sender, RoutedEventArgs e)
        {
            var answerModel = FindAnswerModelFromElement(sender as FrameworkElement);

            TrueAnswers.Add(answerModel);
            AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));

            ValidateAnswersSelectionMode();
        }

        private void ToggleAnswerTrue_Unchecked(object sender, RoutedEventArgs e)
        {
            var answerModel = FindAnswerModelFromElement(sender as FrameworkElement);

            TrueAnswers.Remove(answerModel);
            AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));

            ValidateAnswersSelectionMode();
        }

        private void AnswerSelected(object sender, RoutedEventArgs e)
        {
            var answerModel = FindAnswerModelFromElement(sender as FrameworkElement);

            SelectedAnswers.Add(answerModel);
            AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        private void AnswerUnselected(object sender, RoutedEventArgs e)
        {
            var answerModel = FindAnswerModelFromElement(sender as FrameworkElement);

            SelectedAnswers.Remove(answerModel);
            AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }

        #endregion

        #region Dependency Properties

        private DynamicCheckBoxType CheckBoxType
        {
            get
            {
                var stringValue = (string)GetValue(CheckBoxTypeProperty);
                var parsedDynamicCheckBoxType = (DynamicCheckBoxType)Enum.Parse(typeof(DynamicCheckBoxType), stringValue);

                return parsedDynamicCheckBoxType;
            }
            set { SetValue(CheckBoxTypeProperty, value.ToString()); }
        }

        private static readonly DependencyProperty CheckBoxTypeProperty =
            DependencyProperty.Register("CheckBoxType", typeof(string), typeof(SimpleTest), new PropertyMetadata(null));


        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Question), Question);

            // ITestControl

            info.AddValue(nameof(Answers), Answers.ToList());
            info.AddValue(nameof(TrueAnswers), TrueAnswers.ToList());
            info.AddValue(nameof(SelectedAnswers), SelectedAnswers.ToList());
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            storedAnswers.AddStoredListsTo(Answers, TrueAnswers, SelectedAnswers);
            storedAnswers.Clear();
            storedAnswers = null;

            AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            UpdateCheckboxesOnceAtLoad();
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            #region SimpleTest

            await writer.WriteStartElementAsync(GetType().Name);

            await writer.WriteAttributeStringAsync(nameof(Question), Question);

            foreach (var answer in Answers)
            {
                await answer.WriteXmlAsync(writer, progress, token);
            }

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            // Reads attributes

            Question = reader.GetAttribute(nameof(Question));

            // Reads Answers

            while (await reader.ReadToFollowingAsync("Answer"))
            {
                if (token.HasValue && token.Value.IsCancellationRequested) break;
                if (reader.NodeType != XmlNodeType.Element) continue;

                var answer = new SimpleAnswerModel();
                await answer.ReadXmlAsync(reader, progress, token);

                dispatcher.Invoke(() =>
                {
                    Answers.Add(answer);
                    AnswersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
                });
            }
        }

        #endregion
    } 
}
