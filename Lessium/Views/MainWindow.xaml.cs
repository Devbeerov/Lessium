using System.Windows;
using System.Windows.Controls;

namespace Lessium.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region UI related code-behind
        // Code in this region affects visual part only, it's not breaking MVVM pattern.

        // Focuses ListBox once SelectedItem changed (to highlight it)
        private void Sections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            listbox.Focus();
        }

        #endregion
    }
}