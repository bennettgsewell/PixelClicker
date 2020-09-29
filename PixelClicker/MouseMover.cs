using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelClicker
{
    /// <summary>
    /// Handles mouse movement and clicking.
    /// </summary>
    class MouseMover
    {
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void MouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// Returns the current position of the mouse pointer.
        /// </summary>
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        /// <summary>
        /// Clicks the screen at a certain point.
        /// </summary>
        public static void ClickScreen(int x, int y)
        {
            SetCursorPos(x, y);
            //MouseEvent(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            //MouseEvent(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
    }
}
