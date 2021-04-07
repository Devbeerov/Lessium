using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lessium.Utility
{
    public static class SystemSettingsService
    {
        #region Action Codes

        private static readonly int SPI_GETWHEELSCROLLLINES = 0x0068;

        #endregion

        [DllImport("user32.dll")]
        private static extern int SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        public static int GetSetting(int settingID, int parameter, IntPtr structurePointer)
        {
            return SystemParametersInfo(settingID, parameter, structurePointer, 0);
        }

        public static uint GetMouseWheelScrollingLines()
        {
            try
            {
                var retrievedValue = GetSetting(SPI_GETWHEELSCROLLLINES, 0, IntPtr.Zero);
                var retrievedLineCount = (uint)retrievedValue;
                return retrievedLineCount;
            }

            catch (Win32Exception)
            {
                return 3; // Default line count
            }
        }

        private class StructWrapper : IDisposable
        {
            public IntPtr Ptr { get; private set; }

            public StructWrapper(object obj)
            {
                if (Ptr != null)
                {
                    Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
                    Marshal.StructureToPtr(obj, Ptr, false);
                }
                else
                {
                    Ptr = IntPtr.Zero;
                }
            }

            ~StructWrapper()
            {
                if (Ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Ptr);
                    Ptr = IntPtr.Zero;
                }
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
                GC.SuppressFinalize(this);
            }

            public static implicit operator IntPtr(StructWrapper w)
            {
                return w.Ptr;
            }
        }
    }
}
