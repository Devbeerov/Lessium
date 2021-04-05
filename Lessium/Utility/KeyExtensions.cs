using System.Windows.Input;

namespace Lessium.Utility
{
    public static class KeyExtensions
    {
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
