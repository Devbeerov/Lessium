using Lessium.Utility;
using Lessium.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using static System.Windows.Forms.SystemInformation;
using Lessium.ContentControls;
using System;

namespace Lessium.Views
{
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

        #region TabControl

        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            var newTab = e.AddedItems.OfType<TabItem>().Single();
            var newTabType = GetTabType(newTab);

            if (e.RemovedItems.Count > 0)
            {
                var oldTab = e.RemovedItems.OfType<TabItem>().Single();
                var oldTabType = GetTabType(oldTab);

                viewModel.TabsVerticalScrollOffsets[oldTabType] = sectionsScrollViewer.VerticalOffset;
            }

            var offset = viewModel.TabsVerticalScrollOffsets[newTabType];
            sectionsScrollViewer.ScrollToVerticalOffset(offset);
        }

        private ContentType GetTabType(TabItem tab)
        {
            if (tab == Materials) return ContentType.Material;
            else if (tab == Tests) return ContentType.Test;

            throw new NotSupportedException($"Tab is not supported. You can only get ContentType from Materials or Tests tabs.");
        }

        #endregion

        #region SectionsListBox

        private void SectionsListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0) return;

            bool scrollDown = e.Delta < 0;
            int linesAmount = MouseWheelScrollLines;

            if (linesAmount == -1) // "One screen at time"
            {
                if (scrollDown)
                {
                    sectionsScrollViewer.ScrollToBottom();
                }

                else
                {
                    sectionsScrollViewer.ScrollToTop();
                }
            }

            else
            {
                for (int i = 0; i < MouseWheelScrollLines; i++)
                {
                    if (scrollDown)
                    {
                        sectionsScrollViewer.LineDown();
                    }

                    else
                    {
                        sectionsScrollViewer.LineUp();
                    }
                }
            }
        }

        private void SectionsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && !e.IsRepeat)
            {
                var command = viewModel.RemoveSection;
                var section = viewModel.CurrentSection;

                if (!command.CanExecute(section)) return;

                command.Execute(section);
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

        // Filters input
        private void CurrentPageBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validator.IsOnlyDigits(e.Text); // if not digit, sets event to Handled, so it won't update text.
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

        #region ContentPageControl

        private void ContentPageControl_MouseWheel(object sender, MouseWheelEventArgs e)
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

        #endregion
    }
}