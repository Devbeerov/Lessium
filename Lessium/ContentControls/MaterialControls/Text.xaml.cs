using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
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

        #region Events

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            // Handles input

            ProcessInput(key);
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

        #region Private

        private void ProcessInput(Key key)
        {
            // TODO: Implement method or remove it (same for method which invoked it).
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
            textBox.IsReadOnly = !editable;
        }

        #endregion
    }
}
