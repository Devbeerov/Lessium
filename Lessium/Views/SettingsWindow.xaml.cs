using Lessium.ViewModels;
using System.Windows;

namespace Lessium.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel = null)
        {
            if (viewModel == null) viewModel = new SettingsViewModel();

            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
