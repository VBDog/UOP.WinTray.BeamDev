using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UOP.WinTray.UI.Utilities
{
    /// <summary>
    /// SysMenuUtils
    /// </summary>
    //TODO: Need to remove this exclude attribute 
    [ExcludeFromCodeCoverage]
    public class SysMenuUtils
    {
        private const Int32 GWL_STYLE = -16;
        private const Int32 WS_MAXIMIZEBOX = 0x00010000;
        private const Int32 WS_MINIMIZEBOX = 0x00020000;
        private const Int32 WS_SYSMENU = 0x00080000;
        private const Int32 WS_OVERLAPPED = 0x00000000;

        private const uint MF_REMOVE = 0x00001000;
        private const uint MF_DISABLED = 0x00000002;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_BYPOSITION = 0x00000400;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_ENABLED = 0x00000000;

        private const uint SC_CLOSE = 0x0000F060;
        private const uint SC_SIZE = 0x0000F000; //61440
        private const uint SC_MOVE = 0x0000F010; //61456

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        static extern uint RemoveMenu(IntPtr hMenu, uint nPosition, uint wFlags);

        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
        private extern static Int32 GetWindowLongPtr(IntPtr hWnd, Int32 nIndex);

        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        private extern static Int32 SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

        private SysMenuUtils()
        { }

        /// <summary>
        /// DisableCloseButton
        /// </summary>
        /// <param name="window"></param>
        public static void DisableCloseButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// EnableCloseButton
        /// </summary>
        /// <param name="window"></param>
        public static void EnableCloseButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, true);  // beachte true
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// DisableMaximizeButton
        /// </summary>
        /// <param name="window"></param>
        public static void DisableMaximizeButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MAXIMIZEBOX);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// EnableMaximizeButton
        /// </summary>
        /// <param name="window"></param>
        public static void EnableMaximizeButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MAXIMIZEBOX);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// DisableMinimizeButton
        /// </summary>
        /// <param name="window"></param>
        public static void DisableMinimizeButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MINIMIZEBOX);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// EnableMinimizeButton
        /// </summary>
        /// <param name="window"></param>
        public static void EnableMinimizeButton(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_MINIMIZEBOX);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// HideSysButtons
        /// </summary>
        /// <param name="window"></param>
        public static void HideSysButtons(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_SYSMENU);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// ShowSysButtons
        /// </summary>
        /// <param name="window"></param>
        public static void ShowSysButtons(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
            SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_SYSMENU);
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// DisableMove
        /// </summary>
        /// <param name="window"></param>
        public static void DisableMove(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            RemoveMenu(hMenu, SC_MOVE, MF_BYCOMMAND);
            //oder
            //RemoveMenu(hMenu, menuItemCount-1 , MF_DISABLED | MF_BYPOSITION);
            // es geht nur -1
            DrawMenuBar(hWnd);  // braucht man, wenn man nach OnSourceInitialized arbeitet
        }

        /// <summary>
        /// EnableMove
        /// </summary>
        /// <param name="window"></param>
        public static void EnableMove(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            // Damit wird der Defaultzustand wieder hergestellt
            // für die Veränderungen, die mit RemoveMenu gemacht wurden
            IntPtr hMenu = GetSystemMenu(hWnd, true);  // beachte true
            DrawMenuBar(hWnd);
        }

        /// <summary>
        /// DisableResizing
        /// </summary>
        /// <param name="window"></param>
        public static void DisableResizing(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            RemoveMenu(hMenu, SC_SIZE, MF_BYCOMMAND);
            DrawMenuBar(hWnd);  // braucht man, wenn man nach OnSourceInitialized arbeitet
        }

        /// <summary>
        /// EnableResizing
        /// </summary>
        /// <param name="window"></param>
        public static void EnableResizing(Window window)
        {
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, true);  // beachte true
            DrawMenuBar(hWnd);
        }
    }
}
