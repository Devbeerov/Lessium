using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace Lessium.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressView.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(Dictionary<ContentType, CountData> countDataDictionary)
        {
            this.DataContext = new ProgressWindowViewModel(countDataDictionary);
            InitializeComponent();
        }

        public ProgressWindow(string title, Dictionary<ContentType, CountData> countDataDictionary)
        {
            Title = title;
            this.DataContext = new ProgressWindowViewModel(countDataDictionary);
            InitializeComponent();
        }
    }
}
