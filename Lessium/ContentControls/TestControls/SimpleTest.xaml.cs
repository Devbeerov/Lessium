using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
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
            id = Guid.NewGuid();

            InitializeComponent();
            this.DataContext = this;
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
                Text textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as Text;
                textContainer.SetEditable(editable);
            }
        }

        public void SetMaxWidth(double width)
        {
            var adjustedWidth = width - removeButton.Width;

            testQuestion.SetMaxWidth(width);

            AnswersItemControl.MaxWidth = adjustedWidth;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

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

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(AnswersItemControl.Items.Count > 0)
            {
                var lastIndex = AnswersItemControl.Items.Count - 1;
                var last = AnswersItemControl.Items.GetItemAt(lastIndex);
                var lastContainer = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(last) as ContentPresenter;
                if  (lastContainer != null)
                {
                    DataTemplate dataTemplate = lastContainer.ContentTemplate;
                    var text = dataTemplate.FindName("TextContainer", lastContainer) as Text;

                    var pageControl = this.FindParent<ContentPageControl>();

                    if (text != null && !pageControl.IsElementFits(text))
                    {
                        var textBox = text.textBox;
                        var formattedText = new FormattedText(
                            "1",
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
                            textBox.FontSize,
                            Brushes.Black,
                            new NumberSubstitution(),
                            1);


                        var lineHeight = formattedText.Height;
                        var pos = textBox.TranslatePoint(default(Point), pageControl);
                        var maxLineCount = Convert.ToInt32(Math.Floor((pageControl.MaxHeight - pos.Y) / lineHeight));

                        if (textBox.LineCount > maxLineCount - 1)
                        {
                            int prevCaret = textBox.CaretIndex; // Caret before removing everything past MaxLine

                            // Calculates values of MaxLine

                            int MaxLineIndex = maxLineCount - 1;
                            int firstPositionInMaxLine = textBox.GetCharacterIndexFromLineIndex(MaxLineIndex);
                            int lengthOfMaxLine = textBox.GetLineLength(MaxLineIndex);
                            int lastPositionInMaxLine = firstPositionInMaxLine + lengthOfMaxLine;

                            // Removes everything past MaxLine

                            var newText = textBox.Text.Remove(lastPositionInMaxLine - 1);
                            textBox.Text = newText;

                            // Restores caret

                            if (prevCaret > newText.Length)
                            {
                                prevCaret = newText.Length - 1;
                            }

                            textBox.CaretIndex = prevCaret;
                        }
                    }
                }
            }

            // Sets source to SimpleTest Control, not Border

            e.Source = this;

            // Invokes event

            Resize?.Invoke(sender, e);
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            answers.Add(new AnswerModel());
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (Text)e.Source;

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
            var control = e.Source as Text; // TextContainer

            control.SetEditable(editable);

            textControls.Add(control);
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as Text;

            control.SetEditable(editable);
            textControls.Remove(control);
        }

        private void SimpleTest_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentPageControl pageControl = addAnswerButton.FindParent<ContentPageControl>();
            addAnswerButton.IsEnabled = pageControl.IsElementFits(addAnswerButton);
        }

        #endregion
    }

    public class AnswerModel
    {
        private string text = string.Copy(Properties.Resources.DefaultAnswerHeader);

        public string Text { get => text; set => text = value; }
    }
}
