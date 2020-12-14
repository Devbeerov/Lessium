using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    [Serializable]
    public partial class Text : UserControl, IMaterialControl
    {
        #region Constructors

        public Text()
        {
            Initialize();
        }

        // For serialization
        protected Text(SerializationInfo info, StreamingContext context)
        {
            // Initializes component

            Initialize();

            // Serializes properties

            textBox.Text = info.GetString("Text");
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
            info.AddValue("Text", textBox.Text);
        }

        #endregion

        #region IContentControl

        public void SetEditable(bool editable)
        {
            // ReadOnly

            textBox.IsReadOnly = !editable;

            // Border

            var converter = new ThicknessConverter();
            var thickness = (Thickness)converter.ConvertFrom(editable);

            textBox.BorderThickness = thickness;

            // Button

            removeButton.IsEnabled = editable;

            if (editable)
            {
                removeButton.Visibility = Visibility.Visible;
            }

            else
            {
                removeButton.Visibility = Visibility.Collapsed;
            }

            // Tooltip

            ToolTipService.SetIsEnabled(textBox, editable);
        }

        public void SetMaxWidth(double width)
        {
            var adjusted = width - removeButton.Width;
            textBox.Width = adjusted;
            textBox.MaxWidth = adjusted;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            textBox.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;
        public event SizeChangedEventHandler Resize;

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to Text Control, not Button

            e.Source = this;

            // Invokes event

            RemoveControl?.Invoke(sender, e);
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sets source to Text Control, not Border

            e.Source = this;

            // Invokes event

            Resize?.Invoke(sender, e);
        }

        #endregion

        #region Dependency Properties

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

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Text), new PropertyMetadata(Properties.Resources.TextControl_DefaultText));

        #endregion
    }
}