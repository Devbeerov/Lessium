using Lessium.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lessium.Utility;

namespace Lessium.CustomControls
{
    public partial class ShortcutControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        private Key prevKey = Key.None;
        private string prevHotkeyString = string.Empty;

        #endregion

        #region Properties

        public Hotkey Hotkey
        {
            get 
            {
                if (ShortcutKeyName == null) return default(Hotkey);

                return (Hotkey)Hotkeys.Current[ShortcutKeyName];
            }

            set
            {
                if (!value.Equals(Hotkeys.Current[ShortcutKeyName]))
                {
                    Hotkeys.Current[ShortcutKeyName] = value;
                    prevHotkeyString = value.ToString();

                    RaisePropertyChanged();
                }
            }
        }

        public string CombinationToolTip
        {
            get { return Properties.Resources.CombinationToolTip; }
        }

        #endregion

        #region Constructors

        public ShortcutControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void ShortcutBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            var modifier = prevKey.ToModifier();

            if (modifier != ModifierKeys.None // Modifier is valid
                && !e.Key.IsModifier() // Current key is not modifier
                && Keyboard.IsKeyDown(prevKey)) // Modifier is still held
            {
                Hotkey = new Hotkey(modifier, e.Key);

                prevKey = Key.None;
            }

            else prevKey = e.Key;
        }

        private void ShortcutBox_GotFocus(object sender, RoutedEventArgs e)
        {
            prevHotkeyString = shortcutBox.Text;

            shortcutBox.Clear();
        }

        private void ShortcutBox_LostFocus(object sender, RoutedEventArgs e)
        {
            shortcutBox.Text = prevHotkeyString;
        }

        private void ShortcutBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // If input length is just one character, then it's not hotkey, so we set Handled to true, which will prevent text change.

            e.Handled = e.Text.Length == 1;
        }

        private void ShortcutBox_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.ContinueRouting = true;
        }

        #endregion

        #region Dependency Properties

        public string ShortcutHeader
        {
            get { return (string)GetValue(ShortcutHeaderProperty); }
            set { SetValue(ShortcutHeaderProperty, value); }
        }

        public string ShortcutKeyName
        {
            get { return (string)GetValue(ShortcutKeyNameProperty); }
            set { SetValue(ShortcutKeyNameProperty, value); }
        }

        public static readonly DependencyProperty ShortcutHeaderProperty =
            DependencyProperty.Register("ShortcutHeader", typeof(string), typeof(ShortcutControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Represents Key in Hotkeys dictionary that shortcut should be assigned to.
        /// </summary>
        public static readonly DependencyProperty ShortcutKeyNameProperty =
            DependencyProperty.Register("ShortcutKeyName", typeof(string), typeof(ShortcutControl), new PropertyMetadata(null));

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
