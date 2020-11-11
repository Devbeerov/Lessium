using Lessium.ContentControls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        // Focuses ListBox once SelectedItem changed
        private void Sections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Highlights ListBox

            var listbox = sender as ListBox;
            listbox.Focus();

        }

        private static readonly Regex onlyDigitsRegex = new Regex("\\d");

        // Filters input
        private void CurrentPage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !onlyDigitsRegex.IsMatch(e.Text); // if not digit, sets event to Handled, so it won't update text.
        }

        private void CurrentPage_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var scroll = FindName("ContentScrollViewer") as ScrollViewer;

            int page;

            if (!int.TryParse(textBox.Text, out page))
            {
                textBox.Text = "1";
                page = 1;
            }

            scroll.ScrollToVerticalOffset(ContentPage.PageHeight * (page - 1));

        }

        #endregion
    }
}