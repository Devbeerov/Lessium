using Lessium.ViewModels;
using System.Windows;
using Lessium.Utility;
using Lessium.Models;

namespace Lessium.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel = null)
        {
            if (viewModel == null) viewModel = new SettingsViewModel(new SettingsModel());

            this.DataContext = viewModel;
            InitializeComponent();
        }

        #region UI related code-behind

        #region Event Handlers

        private void fontSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFontSizeTextBlock();
        }

        private void fontSizeTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFontSizeTextBlock();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizeTextBlock();
        }

        #endregion

        private void UpdateFontSizeTextBlock()
        {
            if (fontSizeTextBlock == null) return;

            var thickness = new Thickness();

            var range = fontSlider.Maximum - fontSlider.Minimum;
            var valueInRange = fontSlider.Value - fontSlider.Minimum;
            var offset = fontSizeTextBlock.MeasureText().Width;

            thickness.Left = (valueInRange * fontSlider.ActualWidth / range) - offset * 0.5;

            var maxLeftThickness = fontSlider.ActualWidth - offset * 1.5;
            if (thickness.Left >= maxLeftThickness)
            {
                thickness.Left = maxLeftThickness;
            }

            /// For example: value = 45, minimum = 10, maximum = 90, ActualWidth = 230
            /// (45-10) * 230 / 80
            /// result = 100.625

            fontSizeTextBlock.Margin = thickness;
        }

        #endregion

        
    }
}
