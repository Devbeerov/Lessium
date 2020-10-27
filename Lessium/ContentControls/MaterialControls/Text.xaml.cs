using Lessium.Interfaces;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    public partial class Text : UserControl, IMaterialControl
    {
        private Key prevKey = Key.None;
        private Key prevKeyHold = Key.None;

        public Text()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            // Handles input

            ProcessInput(key, sender as TextBox);

            // Updates previous Keys

            prevKey = key;

            // If hold
            if (e.IsRepeat)
            {
                prevKeyHold = key;
            }
        }

        private void ProcessInput(Key key, TextBox textBox)
        {
            switch(key)
            {
                case Key.Enter:

                    if(prevKeyHold == Key.LeftShift)
                    {
                        textBox.AppendText(Environment.NewLine);
                        int index = textBox.Text.Length - 1;
                        textBox.Select(index,1);
                        return;
                    }

                    break;
            }
        }
    }
}
