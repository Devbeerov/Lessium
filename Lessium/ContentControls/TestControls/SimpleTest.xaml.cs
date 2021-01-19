﻿using Lessium.ContentControls.MaterialControls;
using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
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

            foreach (var item in AnswersItemControl.Items)
            {
                var contentPresenter = AnswersItemControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                Text textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as Text;
                textContainer.SetEditable(editable);
            }

            // Tooltip

            ToolTipService.SetIsEnabled(testQuestion, editable);
        }

        public void SetMaxWidth(double width)
        {
            var adjustedWidth = width - removeButton.Width;

            testQuestion.Width = adjustedWidth;
            testQuestion.MaxWidth = adjustedWidth;

            AnswersItemControl.MaxWidth = adjustedWidth;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            testQuestion.MaxHeight = height;
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
