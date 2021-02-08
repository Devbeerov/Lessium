using Lessium.ViewModels;
using System.Windows;

namespace Lessium.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressView.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private ProgressWindowViewModel viewModel;

        public ProgressWindow()
        {
            InitializeComponent();
            viewModel = DataContext as ProgressWindowViewModel;
        }

        public ProgressWindow(string title)
        {
            InitializeComponent();
            viewModel = DataContext as ProgressWindowViewModel;
            Title = title;
        }
    }
}
