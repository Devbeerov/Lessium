using Lessium.Interfaces;
using System.Windows.Controls;

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
    }
}
