﻿using Lessium.Classes.IO;
using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private IDispatcher dispatcher;

        private bool editable;
        private bool raiseResizeEvent = true;

        // Serialization

        private List<AnswerModel> storedAnswers; // Store Answers, because AnswersControl won't be loaded at time of deserialization.

        #region CLR Properties

        public ObservableCollection<AnswerModel> Answers { get; set; } = new ObservableCollection<AnswerModel>();

        public string GUID
        {
            get { return id.ToString(); }
        }

        public string Question { get; set; } = Properties.Resources.SimpleTestControl_DefaultText;
        public float DisabledAnswerIconOpacity { get; set; } = 0.5f;

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

            var TrueAnswersList = (List<object>)info.GetValue(nameof(TrueAnswers), typeof(List<object>));
            TrueAnswers = TrueAnswersList.ToArray();
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            // Custom control initialization

            id = Guid.NewGuid();
            DataContext = this;

            InitializeComponent();
        }

        #endregion

        #endregion

        #region IContentControl

        public void SetEditable(bool editable)
        {
            this.editable = editable;

            // Text Editable

            testQuestion.SetEditable(editable);

            // Buttons

            removeButton.IsEnabled = editable;

            if (editable)
            {
                removeButton.Visibility = Visibility.Visible;
                AnswersItemControl.ItemTemplate = (DataTemplate)AnswersItemControl.FindResource("EditingTemplate");
            }

            else
            {
                removeButton.Visibility = Visibility.Collapsed;
                AnswersItemControl.ItemTemplate = (DataTemplate)AnswersItemControl.FindResource("ReadOnlyTemplate");
            }

            addAnswerButton.IsEnabled = editable;
            addAnswerButton.Visibility = removeButton.Visibility;

            // Answers Controls

            foreach (var item in AnswersItemControl.Items)
            {
                var contentPresenter = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (contentPresenter != null) // If container is already loaded, otherwise we wait for TextContainer_Loaded method
                {
                    DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                    TextControl textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as TextControl;
                    textContainer.SetEditable(editable);
                }
            }
        }

        public void SetMaxWidth(double width)
        {
            var adjustedWidth = width - removeButton.Width;

            border.MaxHeight = adjustedWidth;
            testQuestion.SetMaxWidth(width);
            AnswersItemControl.MaxWidth = adjustedWidth;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            border.MaxHeight = height;
            testQuestion.SetMaxHeight(height);
            AnswersItemControl.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;
        public event SizeChangedEventHandler Resize;

        #endregion

        #region ITestControl

        public object[] SelectedAnswers { get; set; }
        public object[] TrueAnswers { get; set; }

        public bool CheckAnswers()
        {
            return DataStructuresExtensions.IsSame(SelectedAnswers, TrueAnswers);
        }

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to SimpleTest Control, not Button

            e.Source = this;

            // Invokes event

            RemoveControl?.Invoke(sender, e);
        }

        private void Question_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sets ItemControl MaxHeight to parent border (holder of Question and ItemsControl) MaxHeight minus NewSize.
            AnswersItemControl.MaxHeight = border.MaxHeight - e.NewSize.Height;
        }

        private void AnswersItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!raiseResizeEvent) { return; }

            double? lineHeight = new double?(); // Declares variable here to avoid double-calculating.

            if (AnswersItemControl.Items.Count > 0)
            {
                var lastIndex = AnswersItemControl.Items.Count - 1;
                var last = AnswersItemControl.Items.GetItemAt(lastIndex);
                var lastContainer = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(last) as ContentPresenter;
                if (lastContainer != null)
                {
                    DataTemplate dataTemplate = lastContainer.ContentTemplate;
                    var textContainer = dataTemplate.FindName("TextContainer", lastContainer) as TextControl;

                    var pageControl = this.FindParent<ContentPageControl>();

                    if (textContainer != null && !pageControl.IsElementFits(textContainer))
                    {
                        var textBox = textContainer.textBox;
                        lineHeight = textBox.CalculateLineHeight();
                        var pos = textBox.TranslatePoint(default(Point), pageControl);
                        var maxLineCount = Convert.ToInt32(Math.Floor((pageControl.MaxHeight - pos.Y) / lineHeight.Value));

                        if (textBox.LineCount > maxLineCount)
                        {
                            raiseResizeEvent = false;

                            int prevCaret = textBox.CaretIndex; // Caret before removing everything past MaxLine

                            // Calculates values of MaxLine

                            int MaxLineIndex = maxLineCount - 1;
                            int firstPositionInMaxLine = textBox.GetCharacterIndexFromLineIndex(MaxLineIndex);
                            int lengthOfMaxLine = textBox.GetLineLength(MaxLineIndex);
                            int lastPositionInMaxLine = firstPositionInMaxLine + lengthOfMaxLine;

                            // Removes everything past MaxLine

                            var actualText = textContainer.Text;
                            var newText = actualText.Remove(lastPositionInMaxLine - 1);
                            textContainer.Text = newText;
                            textContainer.InvalidateMeasure();
                            textContainer.UpdateLayout();
                            border.InvalidateMeasure();
                            border.UpdateLayout();

                            // Restores caret

                            if (prevCaret > newText.Length)
                            {
                                prevCaret = newText.Length - 1;
                            }

                            textBox.CaretIndex = prevCaret;

                            e.Handled = true;
                        }
                    }
                }
            }

            double toSubstract = 0d;

            for (int i = 0; i < AnswersItemControl.Items.Count; i++)
            {
                var item = AnswersItemControl.Items.GetItemAt(i);
                var itemContainer = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                toSubstract += itemContainer.ActualHeight;
            }

            var maxHeight = border.MaxHeight - toSubstract;

            if (!lineHeight.HasValue) { lineHeight = testQuestion.textBox.CalculateLineHeight(); }

            testQuestion.textBox.MaxLines = Convert.ToInt32(Math.Floor(maxHeight / lineHeight.Value));
            testQuestion.SetMaxHeight(maxHeight);

            // Sets source to SimpleTest Control, not Border

            e.Source = this;

            // Invokes event

            Resize?.Invoke(sender, e);

            raiseResizeEvent = true;
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var pageControl = this.FindParent<ContentPageControl>();

            bool show = false;

            if (addAnswerButton.Visibility != Visibility.Collapsed)
            {
                show = pageControl.IsElementFits(addAnswerButton);
            }

            addAnswerButton.IsEnabled = show;
            addAnswerButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            Answers.Add(new AnswerModel());
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (TextControl)e.Source;

            foreach (var answer in Answers)
            {
                var text = textControl.Text;

                if (ReferenceEquals(answer.Text, text)) // Checks for string reference, not value!
                {
                    Answers.Remove(answer); // We remove Text string by it's reference, not by value!
                    return;
                }
            }
        }

        private void TextContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl; // TextContainer

            control.RemoveBehavior<TextBoxCutBehavior>();
            control.SetEditable(editable);
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl;

            control.SetEditable(editable);
        }

        private void ToggleAnswerTrue_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            toggleButton.Opacity = 1f;
        }

        private void ToggleAnswerTrue_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            toggleButton.Opacity = 0.5f;
        }

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Question), Question);
            info.AddValue(nameof(Answers), Answers.ToList());

            // ITestControl

            info.AddValue(nameof(TrueAnswers), TrueAnswers.ToList());
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
    }
}
