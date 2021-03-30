using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lessium.Properties
{
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Z")]
        public Key UndoHotkey
        {
            get
            {
                return ((Key)(this["UndoHotkey"]));
            }
            set
            {
                this["UndoHotkey"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Y")]
        public Key RedoHotkey
        {
            get
            {
                return ((Key)(this["RedoHotkey"]));
            }
            set
            {
                this["RedoHotkey"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LShift")]
        public Key UndoHotkeyModifier
        {
            get
            {
                return ((Key)(this["UndoHotkeyModifier"]));
            }
            set
            {
                this["UndoHotkeyModifier"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LShift")]
        public Key RedoHotkeyModifier
        {
            get
            {
                return ((Key)(this["RedoHotkeyModifier"]));
            }
            set
            {
                this["RedoHotkeyModifier"] = value;
            }
        }
    }
}
