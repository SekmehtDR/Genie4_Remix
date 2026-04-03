using System;
using System.Runtime.InteropServices;

namespace GenieClient
{
    /// <summary>
    /// Drives Windows OS-level dark mode for the entire application.
    /// Uses uxtheme.dll and dwmapi.dll APIs introduced in Windows 10 1903.
    /// All methods are no-ops on older OS versions (exceptions are swallowed).
    /// </summary>
    internal static class DarkModeManager
    {
        private static bool _isDark = true;

        // uxtheme.dll — accessed by ordinal because these are undocumented exports
        [DllImport("uxtheme.dll", EntryPoint = "#135")]
        private static extern int SetPreferredAppMode(int mode);

        [DllImport("uxtheme.dll", EntryPoint = "#104")]
        private static extern void RefreshImmersiveColorPolicyState();

        [DllImport("uxtheme.dll", EntryPoint = "#133")]
        private static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool allow);

        [DllImport("uxtheme.dll", EntryPoint = "#136")]
        private static extern void FlushMenuThemes();

        // dwmapi.dll — DWMWA_USE_IMMERSIVE_DARK_MODE (attr 20) darkens the window caption/border
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        // SetPreferredAppMode values
        private const int AppModeDefault   = 0;
        private const int AppModeAllowDark = 1;
        private const int AppModeForceDark = 2;
        private const int AppModeForceLight = 3;

        /// <summary>
        /// Call once at application startup, before any UI is created.
        /// Tells Windows to render all native controls (menus, scrollbars, etc.) in dark mode.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                SetPreferredAppMode(AppModeForceDark);
                RefreshImmersiveColorPolicyState();
                _isDark = true;
            }
            catch { /* Windows version does not support dark mode APIs */ }
        }

        /// <summary>
        /// Apply the current dark/light mode to a specific window.
        /// Call this from OnHandleCreated for each top-level window.
        /// </summary>
        public static void ApplyToWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                AllowDarkModeForWindow(hwnd, _isDark);
                int value = _isDark ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
            }
            catch { }
        }

        /// <summary>
        /// Toggle dark/light mode at runtime.
        /// Pass all currently open window handles so they refresh immediately.
        /// </summary>
        public static void SetDarkMode(bool dark, IntPtr[] windowHandles)
        {
            _isDark = dark;
            try
            {
                SetPreferredAppMode(dark ? AppModeForceDark : AppModeForceLight);
                RefreshImmersiveColorPolicyState();
                foreach (var hwnd in windowHandles)
                    ApplyToWindow(hwnd);
                FlushMenuThemes();
            }
            catch { }
        }

        public static bool IsDark => _isDark;
    }
}
