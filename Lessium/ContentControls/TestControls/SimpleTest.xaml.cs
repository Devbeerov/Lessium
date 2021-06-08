﻿using Lessium.Classes.IO;
using Lessium.ContentControls.MaterialControls;
using Lessium.CustomControls;
using Lessium.Interfaces;
using Lessium.Services;
using Lessium.UndoableActions.Generic;
using Lessium.Utility;
using Lessium.Utility.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class SimpleTest : UserControl, ITestControl
    {
        private Guid id;
        private readonly IDispatcher dispatcher;

        private bool raiseResizeEvent = true;

        // Serialization

        private List<AnswerModel> storedAnswers; // Store Answers, because AnswersControl won't be loaded at time of deserialization.

        #region CLR Properties

        public ObservableCollection<AnswerModel> Answers { get; set; } = new ObservableCollection<AnswerModel>();

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
            storedAnswers = (List<AnswerModel>)info.GetValue(nameof(Answers), typeof(List<AnswerModel>));

            // ITestControl

            var storedTrueAnswers = (List<object>)info.GetValue(nameof(TrueAnswers), typeof(List<object>));
            TrueAnswers.AddRange(storedTrueAnswers);

            var storedSelectedAnswers = (List<object>)info.GetValue(nameof(SelectedAnswers), typeof(List<object>));
            SelectedAnswers.AddRange(storedSelectedAnswers);
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            // Custom control initialization

            id = Guid.NewGuid();
            DataContext = this;

            ValidateAnswersSelectionMode();
            InitializeComponent();
        }

        #endregion

        #region Private

        private ContentPresenter GetLastContainer()
        {
            // No items, no problems.

            if (AnswersItemControl.Items.Count == 0) return null;

            var lastIndex = AnswersItemControl.Items.Count - 1;
            var last = AnswersItemControl.Items.GetItemAt(lastIndex);
            return AnswersItemControl.ItemContainerGenerator.ContainerFromItem(last) as ContentPresenter;
        }

        private TextControl GetTextContainer(ContentPresenter contentPresenter)
        {
            // If container does not exist yet, returns.
            if (contentPresenter == null) return null;

            var dataTemplate = contentPresenter.ContentTemplate;

            return dataTemplate.FindName("TextContainer", contentPresenter) as TextControl;
        }

        private int GetMaxLineCount(TextBox textBox, double? lineHeight)
        {
            if (!lineHeight.HasValue) lineHeight = textBox.CalculateLineHeight();

            var pos = ContentPageControlService.TranslatePoint(this);

            return Convert.ToInt32(Math.Floor((ContentPageControlService.MaxHeight - pos.Y) / lineHeight.Value));
        }

        private void ValidateAnswersControl(double? lineHeight, SizeChangedEventArgs e)
        {
            var lastContainer = GetLastContainer();
            var textContainer = GetTextContainer(lastContainer);

            if (textContainer == null || ContentPageControlService.IsControlFits(this)) return;

            var textBox = textContainer.textBox;
            var maxLineCount = GetMaxLineCount(textBox, lineHeight);

            if (textBox.LineCount <= maxLineCount) return;

            var prevCaret = textBox.CaretIndex; // Caret before removing everything past MaxLine
            var lastPositionInMaxLine = CalculateLastPositionInMaxLine(textBox, maxLineCount);

            // Removes all excessing and creates new text.

            var newText = CreateNewText(textContainer, lastPositionInMaxLine);

            UpdateTextContainer(textContainer, newText);
            UpdateBorder();
            UpdateCaret(textBox, prevCaret, newText.Length);

            e.Handled = true;
        }

        private static int CalculateLastPositionInMaxLine(TextBox textBox, int maxLineCount)
        {
            int MaxLineIndex = maxLineCount - 1;
            int firstPositionInMaxLine = textBox.GetCharacterIndexFromLineIndex(MaxLineIndex);
            int lengthOfMaxLine = textBox.GetLineLength(MaxLineIndex);

            return firstPositionInMaxLine + lengthOfMaxLine;
        }

        private string CreateNewText(TextControl textContainer, int lastPositionInMaxLine)
        {
            var actualText = textContainer.Text;

            return actualText.Remove(lastPositionInMaxLine - 1);
        }

        private static void UpdateCaret(TextBox textBox, int prevCaret, int newLength)
        {
            // Restores caret

            if (prevCaret > newLength)
            {
                prevCaret = newLength - 1;
            }

            textBox.CaretIndex = prevCaret;
        }

        private void UpdateBorder()
        {
            border.InvalidateMeasure();
            border.UpdateLayout();
        }

        private static void UpdateTextContainer(TextControl textContainer, string newText)
        {
            textContainer.Text = newText;
            textContainer.InvalidateMeasure();
            textContainer.UpdateLayout();
        }

        private double CalculateAnswersControlMaxHeight()
        {
            double toSubstract = 0d;

            for (int i = 0; i < AnswersItemControl.Items.Count; i++)
            {
                var item = AnswersItemControl.Items.GetItemAt(i);
                var itemContainer = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                toSubstract += itemContainer.ActualHeight;
            }

            return MaxHeight - toSubstract; // new maxHeight
        }

        private void UpdateAnswersControlMaxHeight(double? lineHeight)
        {
            var maxHeight = CalculateAnswersControlMaxHeight();

            if (!lineHeight.HasValue) lineHeight = testQuestion.textBox.CalculateLineHeight();

            testQuestion.textBox.MaxLines = Convert.ToInt32(Math.Floor(maxHeight / lineHeight.Value));
        }

        private DynamicCheckBoxType GetActualCheckBoxType()
        {
            if (!IsEditable) { return DynamicCheckBoxType.CheckBox; }

            if (TrueAnswers.Count < 2) return DynamicCheckBoxType.RadioSingle;

            return DynamicCheckBoxType.RadioMultiple;
        }

        private void ValidateAnswersSelectionMode()
        {
            CheckBoxType = GetActualCheckBoxType();
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
                ValidateAnswersSelectionMode();

                // Text Editable

                testQuestion.IsEditable = value;

                addAnswerButton.IsEnabled = !IsEditable;
                addAnswerButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;

                // Answers Controls

                for (int i = 0; i < AnswersItemControl.Items.Count; i++)
                {
                    // Should be used ContainerFromIndex instead of ContainerFromItem,
                    // Because it will return same ContentPresenter for each element due to overriden AnswerMode.Equals (perhaps GetHashCode involved too)
                    var contentPresenter = AnswersItemControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                    if (contentPresenter != null) // If container is already loaded, otherwise we wait for TextContainer_Loaded method
                    {
                        DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                        TextControl textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as TextControl;
                        textContainer.IsEditable = IsEditable;
                    }
                }
            }
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(SimpleTest), new PropertyMetadata(false));

        #endregion

        #region ITestControl

        public IList<object> SelectedAnswers { get; set; } = new List<object>();
        public IList<object> TrueAnswers { get; set; } = new List<object>();

        public bool CheckAnswers()
        {
            return SelectedAnswers.SequenceEqual(TrueAnswers);
        }

        #endregion

        #region Events

        private void RemoveButtonPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            RequestRemoveButton?.Invoke(this, new RemoveButtonRequestEventArgs(sender as ContentPresenter));
        }

        private void Question_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sets ItemControl MaxHeight to parent border (holder of Question and ItemsControl) MaxHeight minus NewSize.
            AnswersItemControl.MaxHeight = border.MaxHeight - e.NewSize.Height;
        }

        private void AnswersItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!raiseResizeEvent) { return; }

            double? lineHeight = new double?();

            // Performs all validation and fixing, could also set e.Handled = true, to prevent multiple event calls.

            ValidateAnswersControl(lineHeight, e);

            // Updates AnswersControl.MaxHeight properly with calculations.

            UpdateAnswersControlMaxHeight(lineHeight);

            // Sets source to SimpleTest Control, not Border

            e.Source = this;

            raiseResizeEvent = true;
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bool show = false;

            if (addAnswerButton.Visibility != Visibility.Collapsed)
            {
                show = ContentPageControlService.IsControlFits(this);
            }

            addAnswerButton.IsEnabled = show;
            addAnswerButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            var answer = new AnswerModel();

            ActionsService.ExecuteAction(new AddToCollectionAction<AnswerModel>(Answers, answer));
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (TextControl)e.Source;

            foreach (var answer in Answers)
            {
                var text = textControl.Text;

                if (ReferenceEquals(answer.Text, text)) // Checks for string reference equality, not value!
                {
                    ActionsService.ExecuteAction(new RemoveFromCollectionAction<AnswerModel>(Answers, answer));

                    return;
                }
            }
        }

        private void TextContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl; // TextContainer

            control.RemoveBehavior<TextBoxCutBehavior>();
            control.IsEditable = IsEditable;
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl;

            control.IsEditable = IsEditable;
        }

        private void ToggleAnswerTrue_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var answerModel = checkBox.DataContext as AnswerModel;

            TrueAnswers.Add(answerModel);

            ValidateAnswersSelectionMode();
        }

        private void ToggleAnswerTrue_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var answerModel = checkBox.DataContext as AnswerModel;

            TrueAnswers.Remove(answerModel);

            ValidateAnswersSelectionMode();
        }

        private void AnswerSelected(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            var answerModel = control.DataContext as AnswerModel;

            SelectedAnswers.Add(answerModel);
        }

        private void AnswerUnselected(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            var answerModel = control.DataContext as AnswerModel;

            SelectedAnswers.Remove(answerModel);
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
            info.AddValue(nameof(Answers), Answers.ToList());

            // ITestControl

            info.AddValue(nameof(TrueAnswers), TrueAnswers.ToList());
            info.AddValue(nameof(SelectedAnswers), SelectedAnswers.ToList());
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            Answers.AddRange(storedAnswers);

            // Clears and sets to null, so GC will collect List instance.

            storedAnswers.Clear();
            storedAnswers = null;
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

                var answer = new AnswerModel();
                await answer.ReadXmlAsync(reader, progress, token);

                dispatcher.Invoke(() =>
                {
                    Answers.Add(answer);
                });
            }
        }

        #endregion

        #region IRemoveButtonRequestor

        public event RemoveButtonRequestEventHandler RequestRemoveButton;

        #endregion
    }

    [Serializable]
    public class AnswerModel : ILsnSerializable
    {
        private IDispatcher dispatcher;

        public string Text { get; set; } = string.Copy(Properties.Resources.DefaultAnswerHeader);

        public AnswerModel(IDispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;
        }

        public AnswerModel(string text, IDispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            Text = text;
        }

        // For serialization
        protected AnswerModel(SerializationInfo info, StreamingContext context)
        {
            Text = info.GetString(nameof(Text));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dispatcher.Invoke(() =>
            {
                info.AddValue(nameof(Text), Text);
            });
        }

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            #region Answer

            await writer.WriteStartElementAsync("Answer");

            await dispatcher.InvokeAsync(async () =>
            {
                await writer.WriteStringAsync(Text);
            });

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            await dispatcher.InvokeAsync(async () =>
            {
                Text = await reader.ReadElementContentAsStringAsync();
            });
        }

        public override bool Equals(object obj)
        {
            return obj is AnswerModel model &&
                   Text == model.Text;
        }

        // Autogenerated
        public override int GetHashCode()
        {
            return 1249999374 + EqualityComparer<string>.Default.GetHashCode(Text);
        }
    }
}
