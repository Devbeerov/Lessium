using Lessium.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Lessium.Utility;

namespace Lessium.Services
{
    public class HotkeysService
    {
        private Dictionary<Hotkey, Action> boundHotkeys = new Dictionary<Hotkey, Action>();

        public HotkeysService(Window attachTo)
        {
            Keyboard.AddKeyDownHandler(attachTo, OnKeyDown);

            attachTo.Closing += (s, e) =>
            {
                UnregisterHotkeys();
            };
        }

        public void RegisterHotkey(Hotkey hotkey, Action action)
        {
            boundHotkeys.Add(hotkey, action);
        }

        public void UnregisterHotkey(Hotkey hotkey)
        {
            boundHotkeys.Remove(hotkey);
        }

        public void UnregisterHotkeys()
        {
            boundHotkeys.Clear();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var hotkey in boundHotkeys.Keys)
            {
                var modifier = hotkey.Modifier;

                // Checks if both modifier and key are pressed.

                if (!IsModifierDown(modifier)) continue;
                if (!Keyboard.IsKeyDown(hotkey.Key)) continue;

                // Calls assigned action to hotkey.

                boundHotkeys[hotkey].Invoke();
            }
        }

        private bool IsModifierDown(ModifierKeys modifier)
        {
            foreach (var key in modifier.ToKeys())
            {
                if (Keyboard.IsKeyDown(key)) return true;
            }

            return false;
        }
    }
}
