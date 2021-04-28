using System;
using System.Windows.Input;

namespace Lessium.Utility
{
    public static class KeyExtensions
    {
        /// <summary>
        /// Converts modifier to relative Key values.
        /// For example: ModifierKeys.Control will return Key.LeftCtrl and Key.RightCtrl.
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns>Array of all relative keys.</returns>
        public static Key[] ToKeys(this ModifierKeys modifier)
        {
            if (modifier == ModifierKeys.Control) return new Key[] { Key.LeftCtrl, Key.RightCtrl };
            if (modifier == ModifierKeys.Shift) return new Key[] { Key.LeftShift, Key.RightShift };
            if (modifier == ModifierKeys.Alt) return new Key[] { Key.LeftAlt, Key.RightAlt };
            if (modifier == ModifierKeys.Windows) return new Key[] { Key.LWin, Key.RWin };

            throw new NotSupportedException($"{modifier} is not supported.");
        }

        public static ModifierKeys ToModifier(this Key modifierKey)
        {
            if (modifierKey == Key.LeftCtrl || modifierKey == Key.RightCtrl) return ModifierKeys.Control;
            if (modifierKey == Key.LeftShift || modifierKey == Key.RightShift) return ModifierKeys.Shift;
            if (modifierKey == Key.LeftAlt || modifierKey == Key.RightAlt) return ModifierKeys.Alt;
            if (modifierKey == Key.LWin || modifierKey == Key.RWin) return ModifierKeys.Windows;

            return ModifierKeys.None;
        }

        public static bool IsModifier(this Key modifierKey)
        {
            return ToModifier(modifierKey) != ModifierKeys.None;
        }

        public static bool IsSpecialKey(this KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.System:
                    return true;

                case Key.ImeProcessed:
                    return true;

                case Key.DeadCharProcessed:
                    return true;

                default:
                    return false;
            }
        }
    }
}
