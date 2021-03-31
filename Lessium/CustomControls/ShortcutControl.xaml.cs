using Lessium.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ShortcutControl.xaml
    /// </summary>
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
                return (Hotkey)Hotkeys.Current[ShortcutKeyName];
            }
        }

        #endregion

        #region Constructors

        public ShortcutControl()
        {
            InitializeComponent();
            Hotkeys.Current.PropertyChanged += OnHotkeyChanged;
        }

        private void OnHotkeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShortcutKeyName)
            {
                prevHotkeyString = Hotkeys.Current[ShortcutKeyName].ToString();
                RaisePropertyChanged(nameof(Hotkey));
            }
        }

        #endregion

        #region Events

        private void ShortcutBox_KeyDown(object sender, KeyEventArgs e)
        {
            shortcutBox.Clear();

            if (prevKey != Key.None)
            {
                var hotkey = new Hotkey(prevKey, e.Key);
                Hotkeys.Current[ShortcutKeyName] = hotkey;

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
