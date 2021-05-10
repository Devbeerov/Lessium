using Lessium.Utility;
using System.Windows;

namespace Lessium.Themes
{
    partial class SettingsViewDictionary : ResourceDictionary
    {
        public SettingsViewDictionary()
        {
            InitializeComponent();
        }

        #region Event Handlers

        private void fontSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFontSizeTextBlock();
        }

        private void fontSizeTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFontSizeTextBlock();
        }

        private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizeTextBlock();
        }

        #endregion

        #region Methods

        private void UpdateFontSizeTextBlock()
        {
            if (fontSizeTextBlock == null) return;

            var thickness = new Thickness();

            /// Math
            /// For example: value = 45, minimum = 10, maximum = 90, ActualWidth = 230
            /// (45-10) * 230 / 80
            /// result = 100.625

            var range = fontSlider.Maximum - fontSlider.Minimum;
            var valueInRange = fontSlider.Value - fontSlider.Minimum;
            var offset = fontSizeTextBlock.MeasureText().Width;

            thickness.Left = (valueInRange * fontSlider.ActualWidth / range) - offset * 0.5;

            var maxLeftThickness = fontSlider.ActualWidth - offset * 1.5;

            if (thickness.Left >= maxLeftThickness)
            {
                thickness.Left = maxLeftThickness;
            }

            // Updates Margin with new Thickness.

            fontSizeTextBlock.Margin = thickness;
        }

        #endregion

    }
}
