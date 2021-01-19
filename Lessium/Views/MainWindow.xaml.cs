using Lessium.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This should not break MVVM, because Window should know about ViewModel, as it's DataContext.
        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = DataContext as MainWindowViewModel;
        }

        #region UI related code-behind
        // Code in this region affects visual part only, it's not breaking MVVM pattern.

        #region Window-Wide Events

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (viewModel.CurrentPage != null && viewModel.Pages.Count > 1)
            {
                if (e.Delta < 0) // Down one page (index is Top-Down)
                {
                    // Number > index, because of bindings to Number, it will also do all validations with value.
                    viewModel.CurrentPageNumber++;
                }
                else
                {
                    viewModel.CurrentPageNumber--;
                }
            }
        }

        #endregion

        #region SectionsItemControl

        // Focuses ListBox once SelectedItem changed
        private void Sections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Highlights ListBox

            var listbox = sender as ListBox;
            listbox.Focus();

        }

        #endregion

        #region CurrentPage

        private static readonly Regex onlyDigitsRegex = new Regex("\\d");

        // Filters input
        private void CurrentPageBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !onlyDigitsRegex.IsMatch(e.Text); // if not digit, sets event to Handled, so it won't update text.
        }

        private void CurrentPageBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            int page;

            if (!int.TryParse(textBox.Text, out page))
            {
                page = 1;
            }

            textBox.Text = page.ToString();
            
            viewModel.CurrentPageNumber = page;
        }

        #endregion

        #endregion
    }
}