using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Linq;
using Lessium.Utility;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Generic;
using Lessium.Classes.IO;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    [Serializable]
    public partial class TextControl : UserControl, IMaterialControl
    {
        #region Constructors

        public TextControl()
        {
            Initialize();
        }

        // For serialization
        protected TextControl(SerializationInfo info, StreamingContext context)
        {
            // Initializes component

            Initialize();

            // Serializes properties

            Text = info.GetString("Text");
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void RemoveBehavior<T>() where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(textBox);
            var behavior = GetBehavior<T>();
            if (behavior != null)
            {
                behaviors.Remove(behavior);
            }
        }

        public T GetBehavior<T>() where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(textBox);
            return behaviors.OfType<T>().FirstOrDefault();
        }

        #endregion

        #endregion

        #region IContentControl

        public void SetEditable(bool editable)
        {
            // ReadOnly

            textBox.IsReadOnly = !editable;

            // Border

            var converter = new ThicknessConverter();

            // Size 0 if not editable, size 1 if editable.

            var thickness = (Thickness)converter.ConvertFrom(editable);

            textBox.BorderThickness = thickness;

            // Button

            if (ShowRemoveButton)
            {
                removeButton.IsEnabled = editable;
            }

            // Tooltip

            ToolTipService.SetIsEnabled(textBox, editable);
        }

        public void SetMaxWidth(double width)
        {
            var adjusted = width - removeButton.Width;

            raiseResizeEvent = false;

            this.MaxWidth = width;
            textBox.MaxWidth = adjusted;
            textBox.Width = adjusted;

            raiseResizeEvent = true;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            this.MaxHeight = height;
            textBox.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;
        public event SizeChangedEventHandler Resize;

        private bool raiseResizeEvent = true;

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to TextControl, not Button

            e.Source = this;

            // Invokes event

            RemoveControl?.Invoke(sender, e);
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!raiseResizeEvent)
            {
                e.Handled = true;
                return;
            }

            // Sets source to TextControl, not Border

            e.Source = this;

            // Invokes event

            Resize?.Invoke(sender, e);
        }

        #endregion

        #region Dependency Properties

        #region Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextControl), 
                new FrameworkPropertyMetadata(Properties.Resources.TextControl_DefaultText));

        #endregion

        #region ShowRemoveButton

        public bool ShowRemoveButton
        {
            get { return (bool)GetValue(ShowRemoveButtonProperty); }
            set { SetValue(ShowRemoveButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowRemoveButtonProperty =
            DependencyProperty.Register("ShowRemoveButton", typeof(bool), typeof(TextControl), new PropertyMetadata(true));

        #endregion

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Text", Text);
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            #region TextControl

            await writer.WriteStartElementAsync(GetType().Name);

            await writer.WriteStringAsync(Text);

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            // Content of TextControl is string. So we just extracts it entirely.

            Text = await reader.ReadElementContentAsStringAsync();
        }

        #endregion
    }
}