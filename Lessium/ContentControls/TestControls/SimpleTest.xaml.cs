using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Answers", answers.ToList());
        }

        #endregion

        #region IContentControl

        public void SetEditable(bool editable)
        {
            this.editable = editable;

            // ReadOnly

            testQuestion.IsReadOnly = !editable;

            // Border

            var converter = new ThicknessConverter();
            var thickness = (Thickness)converter.ConvertFrom(editable);

            testQuestion.BorderThickness = thickness;

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

            foreach(IContentControl answer in AnswersItemControl.Items)
            {
                answer.SetEditable(editable);
            }

            // Tooltip

            ToolTipService.SetIsEnabled(testQuestion, editable);
        }

        public void SetMaxWidth(double width)
        {
            testQuestion.Width = width - removeButton.Width;
            testQuestion.MaxWidth = testQuestion.Width;
        }

        public void SetMaxHeight(double height)
        {
            testQuestion.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to SimpleTest Control, not Button

            e.Source = this;

            // Invokes event

            RemoveControl?.Invoke(sender, e);
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
            var control = e.Source as Text;

            control.SetEditable(editable);
            textControls.Add(control);
        }

        private void TextContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            var control = e.Source as Text;

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
