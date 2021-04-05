using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Windows.Input;
using Lessium.Utility;
using System.Linq;

namespace Lessium.Properties
{
    public sealed class Hotkeys : ApplicationSettingsBase
    {
        private static Hotkeys instance = (Hotkeys)Synchronized(new Hotkeys());

        public static Hotkeys Current { get; } = instance;

        public bool IsUnique(Hotkey hotkey)
        {
            return !PropertyValues.OfType<Hotkey>().Contains(hotkey);
        }

        #region Setting Properties

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Control + Z")]
        public Hotkey UndoHotkey
        {
            get
            {
                return ((Hotkey)(this["UndoHotkey"]));
            }
            set
            {
                this["UndoHotkey"] = value;
                var g = ApplicationCommands.Undo.InputGestures;
            }
        }


        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Control + Y")]
        public Hotkey RedoHotkey
        {
            get
            {
                return ((Hotkey)(this["RedoHotkey"]));
            }
            set
            {
                this["RedoHotkey"] = value;
                var g = ApplicationCommands.Undo.InputGestures;
            }
        }

        #endregion
    }

    [TypeConverterAttribute(typeof(HotkeyConverter))]
    public struct Hotkey
    {

        #region Properties

        public ModifierKeys Modifier { get; set; }
        public Key Key { get; set; }

        #endregion

        #region Constructors

        public Hotkey(ModifierKeys modifier, Key key)
        {
            Modifier = modifier;
            Key = key;
        }

        public Hotkey(Key modifierKey, Key key)
        {
            Modifier = modifierKey.ToModifier();
            Key = key;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Modifier == ModifierKeys.None)
            {
                return Key.ToString();
            }

            return $"{Modifier} + {Key}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Hotkey other)
            {
                return Modifier == other.Modifier && Key == other.Key;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Modifier, Key).GetHashCode();
        }

        #endregion

    }

    public class HotkeyConverter : TypeConverter
    {
        private static char separator = '+';
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var hotkeyString = value as string;

            if (hotkeyString != null)
            {
                string text = hotkeyString.Trim();

                if (text.Length == 0)
                {
                    return null;
                }

                else
                {
                    string[] tokens = text.Split(separator);

                    if (tokens.Length != 2)
                    {
                        throw new ArgumentException($"Hotkey \"{hotkeyString}\" is not completed. Hotkey should contain modifier and key.");
                    }

                    bool modifierGood = Enum.TryParse(tokens[0], true, out ModifierKeys modifier);
                    bool keyGood = Enum.TryParse(tokens[1], true, out Key key);

                    if (modifierGood && keyGood)
                    {
                        return new Hotkey(modifier, key);
                    }

                    else
                    {
                        throw new ArgumentException($"Failed to convert {hotkeyString} to Hotkey type.");
                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
