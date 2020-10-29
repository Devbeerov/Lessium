using Lessium.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    public partial class Text : UserControl, IMaterialControl
    {
        
        public Text()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            // Handles input

            ProcessInput(key);
        }

        private void ProcessInput(Key key)
        {
            // TODO: Implement method or remove it (same for method which invoked it).
        }

        public void SetEditable(bool editable)
        {
            textBox.IsReadOnly = !editable;
        }
    }
}
