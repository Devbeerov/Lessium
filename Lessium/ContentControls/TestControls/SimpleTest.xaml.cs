using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ContentControls.TestControls
{
    /// <summary>
    /// Simple test with multiple answers.
    /// </summary>
    [Serializable]
    public partial class SimpleTest : UserControl, ITestControl
    {

        private ObservableCollection<string> answers = new ObservableCollection<string>();

        #region Properties

        public ObservableCollection<string> Answers
        {
            get { return answers; }
            set { answers = value; }
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

            var answersList = (List<string>) info.GetValue("Answers", typeof(List<string>));

            answers = new ObservableCollection<string>(answersList);
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
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
            answers.Add(Properties.Resources.DefaultAnswerHeader);
        }

        private void RemoveAnswer_Click(object sender, RoutedEventArgs e)
        {
            var textControl = (Text)e.Source;

            answers.Remove(textControl.GetText()); // We remove Text string by it's reference, not by value!
        }

        #endregion
    }
}
