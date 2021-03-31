using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Windows.Input;

namespace Lessium.Properties
{
    public sealed class Hotkeys : ApplicationSettingsBase
    {
        private static Hotkeys instance = (Hotkeys)Synchronized(new Hotkeys());

        public static Hotkeys Current { get; } = instance;

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("LeftShift + Z")]
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
        [DefaultSettingValueAttribute("LeftShift + Y")]
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
    }
    
    [TypeConverterAttribute(typeof(HotkeyConverter))]
    public struct Hotkey
    {
        public Key Modifier { get; set; }
        public Key Key { get; set; }

        public Hotkey (Key modifier, Key key)
        {
            Modifier = modifier;
            Key = key;
        }

        public override string ToString()
        {
            if (Modifier == Key.None)
            {
                return Key.ToString();
            }

            return $"{Modifier} + {Key}";
        }
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
                    Key[] values = new Key[tokens.Length];

                    TypeConverter keyConverter = TypeDescriptor.GetConverter(typeof(Key));
                    for (int i = 0; i < values.Length; i++)
                    {
                        // Note: ConvertFromString will raise exception if value cannot be converted.
                        values[i] = (Key)keyConverter.ConvertFromString(context, culture, tokens[i]);
                    }

                    if (values.Length == 2)
                    {
                        return new Hotkey(values[0], values[1]);
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
