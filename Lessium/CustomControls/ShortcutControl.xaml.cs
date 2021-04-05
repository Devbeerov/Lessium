using Lessium.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lessium.Utility;
using System.Windows.Data;
using System.Globalization;

namespace Lessium.CustomControls
{
    public partial class ShortcutControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        private Key prevModifierKey = Key.None;
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

        #region ShortcutBox

        private void ShortcutBox_KeyDown(object sender, KeyEventArgs e)
        {
            // NOTE: Look at ShortcutControl_PreviewKeyDown, it contains some checks.

            if (prevModifierKey == Key.None) return;

            if (Keyboard.IsKeyDown(prevModifierKey))
            {
                Key key;

                if (e.IsSpecialKey()) key = e.SystemKey;
                else key = e.Key;

                Hotkey = new Hotkey(prevModifierKey.ToModifier(), key);
            }

            prevModifierKey = Key.None;

            e.Handled = true;
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

        #region ShortcutControl

        private void ShortcutControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                e.Handled = true; // Won't further travel to KeyDown
                return;
            }

            if (e.IsSpecialKey() && e.SystemKey.IsModifier())
            {
                e.Handled = true; // Won't further travel to KeyDown
                prevModifierKey = e.SystemKey;
            }

            else if (e.Key.IsModifier())
            {
                e.Handled = true; // Won't further travel to KeyDown
                prevModifierKey = e.Key;
            }
        }

        #endregion

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
            DependencyProperty.Register("ShortcutKeyName", typeof(string), typeof(ShortcutControl), new PropertyMetadata(null, OnShortcutKeyNameChanged));

        private static void OnShortcutKeyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ShortcutControl;

            control.RaisePropertyChanged(nameof(Hotkey));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class UniqueHotkeyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is BindingExpression expression)
            {
                var propertyName = expression.ResolvedSourcePropertyName;
                var source = expression.ResolvedSource;

                // Using reflection to get value of source binding property and update argument with it.

                value = source.GetType().GetProperty(propertyName).GetValue(source, null);
            }

            if (value is Hotkey hotkey)
            {
                // No error if is None or is unique (not present in Hotkeys).
                if (hotkey.Equals(default) || Hotkeys.Current.IsUnique(hotkey)) return ValidationResult.ValidResult;

                return new ValidationResult(false, Resources.HotkeyExistError);
            }

            return new ValidationResult(false, "Value should must have non-empty string type");
        }
    }
}
