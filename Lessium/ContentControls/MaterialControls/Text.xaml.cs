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

        public event RoutedEventHandler RemoveControl;

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to Text Control, not Button

            e.Source = this;

            // Invokes event

            RemoveControl.Invoke(sender, e);
        }

        #endregion
    }
}