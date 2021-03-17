using Lessium.ViewModels;
using System.Windows;
using Lessium.Utility;
using Lessium.Models;

namespace Lessium.Views
{
    public partial class SettingsWindow : Window
    {
        SettingsViewModel viewModel = null;

        public SettingsWindow(SettingsViewModel viewModel = null)
        {
            if (viewModel == null) viewModel = new SettingsViewModel(new SettingsModel());

            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
            InitializeComponent();
        }
    }
}
