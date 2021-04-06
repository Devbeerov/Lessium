using Lessium.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lessium.Utility;
using System.Windows.Data;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lessium.CustomControls
{
    public partial class ShortcutControl : UserControl, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        private Key prevModifierKey = Key.None;
        private string prevHotkeyString = string.Empty;
        private readonly Dictionary<string, List<string>> dataErrors = new Dictionary<string, List<string>>();

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

            Hotkeys.Current.PropertyChanged += OnHotkeysUpdated;
        }

        #endregion

        #region Methods

        private void ValidateHotkeyUnique()
        {
            // Clears all errors linked to Hotkey, if validation won't fail, therefore it won't have errors.

            ClearErrors(nameof(Hotkey));

            // No error if is None or is unique (not present in Hotkeys).

            if (Hotkeys.Current.IsUnique(Hotkey, ShortcutKeyName)) return;

            TryAddError(nameof(Hotkey), Properties.Resources.HotkeyExistError);
        }

        private void TryAddError(string propertyName, string error)
        {
            // If property is not exist is dictionary, adds new List for it.

            if (!dataErrors.ContainsKey(propertyName)) dataErrors.Add(propertyName, new List<string>());

            // If property errors List already contains error, returns.

            else if (dataErrors[propertyName].Contains(error)) return;

            // Now we can be sure that dictionary contains propertyName key and it's list not already have this error, so we add it.

            dataErrors[propertyName].Add(error);
            
        }

        private void ClearErrors(string propertyName)
        {
            if (!dataErrors.ContainsKey(propertyName)) return;

            dataErrors[propertyName].Clear();
            RaiseErrorsChanged(propertyName);
        }

        #endregion

        #region Events

        private void OnHotkeysUpdated(object sender, PropertyChangedEventArgs e)
        {
            // If any hotkey is updated, we check if Hotkey is still unique. Also if this hotkey updated by itself, it will be checked too.
            ValidateHotkeyUnique();
        }

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
            if (prevHotkeyString != string.Empty)
            {
                prevHotkeyString = shortcutBox.Text;
            }

            shortcutBox.Clear();
        }

        private void ShortcutBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // IMPORTANT NOTE: Don't set Text property to any value! It will detach binding. Instead, use replace method.
            shortcutBox.Text.Replace(shortcutBox.Text, prevHotkeyString);
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

            control.ValidateHotkeyUnique();
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

        #region INotifyDataErrorInfo

        public bool HasErrors => dataErrors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null) return null;

            if(dataErrors.ContainsKey(propertyName)) return dataErrors[propertyName];

            return null;
        }

        private void RaiseErrorsChanged([CallerMemberName] string propertyName = "")
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}
