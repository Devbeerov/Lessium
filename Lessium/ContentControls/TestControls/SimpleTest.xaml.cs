using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Lessium.ContentControls.TestControls
{
    /// <summary>
    /// Simple test with multiple answers.
    /// </summary>
    [Serializable]
    public partial class SimpleTest : UserControl, ITestControl
    {
        private Guid id;

        private bool editable;
        private bool raiseResizeEvent = true;

        private ObservableCollection<AnswerModel> answers = new ObservableCollection<AnswerModel>();

        private readonly Collection<IContentControl> textControls = new Collection<IContentControl>();

        #region Properties

        public ObservableCollection<AnswerModel> Answers
        {
            get { return answers; }
            set { answers = value; }
        }

        public string GUID
        {
            get { return id.ToString(); }
        }

        public string Question { get; set; } = Properties.Resources.SimpleTestControl_DefaultText;

        #endregion

        #region Constructors

        public SimpleTest()
        {
            Initialize();
        }

        // For serialization
        protected SimpleTest(SerializationInfo info, StreamingContext context)
        {
            // Initializes component

            Initialize();

            // Serializes properties

            var answersList = (List<AnswerModel>) info.GetValue("Answers", typeof(List<AnswerModel>));

            answers = new ObservableCollection<AnswerModel>(answersList);
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            // Custom control initialization

            id = Guid.NewGuid();

            this.DataContext = this;

            InitializeComponent();
        }

        #endregion

        #endregion


        #region ISerializable

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Answers", answers.ToList());
        }

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
            }

            else
            {
                removeButton.Visibility = Visibility.Collapsed;
            }

            addAnswerButton.IsEnabled = editable;
            addAnswerButton.Visibility = removeButton.Visibility;

            // Answers Controls

            foreach (var item in AnswersItemControl.Items)
            {
                var contentPresenter = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                TextControl textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as TextControl;
                textContainer.SetEditable(editable);
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

            double? lineHeight = new Nullable<double>(); // Declares variable here to avoid double-calculating.

            if (AnswersItemControl.Items.Count > 0)
            {
                var lastIndex = AnswersItemControl.Items.Count - 1;
                var last = AnswersItemControl.Items.GetItemAt(lastIndex);
                var lastContainer = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(last) as ContentPresenter;
                if (lastContainer != null)
                {
                    DataTemplate dataTemplate = lastContainer.ContentTemplate;
                    var text = dataTemplate.FindName("TextContainer", lastContainer) as TextControl;

                    var pageControl = this.FindParent<ContentPageControl>();

                    if (text != null && !pageControl.IsElementFits(text))
                    {
                        var textBox = text.textBox;
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

                            var actualText = text.GetText();
                            var newText = actualText.Remove(lastPositionInMaxLine - 1);
                            text.SetText(newText);
                            text.InvalidateMeasure();
                            text.UpdateLayout();
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
            answers.Add(new AnswerModel());
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (TextControl)e.Source;

            foreach (var answer in answers)
            {
                var text = textControl.GetText();

                if (ReferenceEquals(answer.Text, text)) // Checks for string reference, not value!
                {
                    answers.Remove(answer); // We remove Text string by it's reference, not by value!
                    return;
                }
            }
        }

        private void TextContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl; // TextContainer

            control.RemoveBehavior<TextBoxCutBehavior>();
            control.SetEditable(editable);

            textControls.Add(control);
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as TextControl;

            control.SetEditable(editable);
            textControls.Remove(control);
        }

        #endregion
    }

    public class AnswerModel
    {
        private string text = string.Copy(Properties.Resources.DefaultAnswerHeader);

        public string Text { get => text; set => text = value; }
    }
}
