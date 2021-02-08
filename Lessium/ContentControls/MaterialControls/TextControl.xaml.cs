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

            SetText(info.GetString("Text"));
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

            if (GetShowRemoveButton(this))
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

        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string text)
        {
            obj.SetValue(TextProperty, text);
        }

        public string GetText()
        {
            return GetText(this);
        }

        public void SetText(string text)
        {
            SetText(this, text);
        }

        public string Text
        {
            get { return GetText(); }
            set { SetText(value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextControl), 
                new FrameworkPropertyMetadata(Properties.Resources.TextControl_DefaultText));

        #endregion

        #region ShowRemoveButton

        public static void SetShowRemoveButton(DependencyObject obj, bool show)
        {
            obj.SetValue(ShowRemoveButtonProperty, show);
        }

        public static bool GetShowRemoveButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowRemoveButtonProperty);
        }


        public void SetShowRemoveButton(bool show)
        {
            SetShowRemoveButton(this, show);
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
            #region TextControl

            await writer.WriteStartElementAsync(GetType().Name);

            await writer.WriteStringAsync(Text);

            await writer.WriteEndElementAsync();

            #endregion

            // Reports progress.

            progress.Report(ProgressType.Content);
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Content of TextControl is string. So we just extracts it entirely.

            SetText(await reader.ReadElementContentAsStringAsync());

            // Reports progress.

            progress.Report(ProgressType.Content);
        }

        #endregion
    }
}