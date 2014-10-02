using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win10TaskbarCleaner
{
    class TaskbarCleaner
    {
        public static void HideButtons(object state = null)
        {
            var hwnd = NativeMethods.GetDesktopWindow();
            NativeMethods.WindowEnumProc proc = EnumChildWindowsCallback;

            // FindWindowEx would have been more elegant, but this was great for debugging so leaving it in here
            NativeMethods.EnumChildWindows(hwnd, proc, (IntPtr)0);
        }

        private static int EnumChildWindowsCallback(IntPtr hwnd, IntPtr lparam)
        {
            var sb = new StringBuilder(1024);

            NativeMethods.GetClassName(hwnd, sb, 1024);
            
            string name = sb.ToString();

            if (name == "Shell_TrayWnd")
            {
                // find the buttons, the callback will handle 
                NativeMethods.EnumChildWindows(hwnd, (NativeMethods.WindowEnumProc)EnumTrayChildWindowsCallback, (IntPtr)(0));

                return 0; // stop enumerating windows
            }

            //System.Diagnostics.Debug.WriteLine("hwnd: " + hwnd + " Class: " + sb.ToString());
            return 1; // return true to continue, false to stop
        }

        private static int EnumTrayChildWindowsCallback(IntPtr hwnd, IntPtr lparam)
        {
            var sb = new StringBuilder(1024);

            NativeMethods.GetClassName(hwnd, sb, 1024);

            string name = sb.ToString();

            if (name == "TrayButton")
            {
                // hide the actual button (search or multi-task)

                NativeMethods.SetWindowPos(hwnd, (IntPtr)1, 0, 0, 0, 0, NativeMethods.SetWindowPosFlags.HideWindow);
            }
            else if (name == "ReBarWindow32")
            {
                // The running apps on the taskbar are in a ReBarWindow32 which is adjusted
                // to account for the two new icons. Let's move it back to where it belongs.
                // I assume that there is a better way to do this, but this works :)
                NativeMethods.RECT rectTaskBar, rectClient;

                NativeMethods.GetClientRect(NativeMethods.GetParent(hwnd), out rectTaskBar);

                NativeMethods.GetClientRect(hwnd, out rectClient);

#if DEBUG
                NativeMethods.RECT rectWindow;

                NativeMethods.GetWindowRect(hwnd, out rectWindow);
#endif

                int xDelta, yDelta;

                if (rectTaskBar.Right > rectTaskBar.Bottom)
                {
                    System.Diagnostics.Debug.WriteLine("Horizontal");
                    xDelta = 36;
                    yDelta = 0;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Vertical");
                    xDelta = 0;
                    yDelta = 36;
                }

                NativeMethods.SetWindowPos(hwnd, (IntPtr)0, rectClient.X + xDelta, rectClient.Y + yDelta, rectClient.Width, rectClient.Height, 
                    NativeMethods.SetWindowPosFlags.IgnoreResize | 
                    NativeMethods.SetWindowPosFlags.IgnoreZOrder | 
                    NativeMethods.SetWindowPosFlags.DrawFrame);

#if DEBUG
                System.Diagnostics.Debug.WriteLine("Pre-Move: " + rectClient + "  Window Rect: " + rectWindow);

                // useful to see how the rects changed
                NativeMethods.GetClientRect(hwnd, out rectClient);
                NativeMethods.GetWindowRect(hwnd, out rectWindow);

                System.Diagnostics.Debug.WriteLine("Post-Move: " + rectClient + "  Window Rect: " + rectWindow);
#endif
            }

            return 1; // always continue iterating so that we can get all of the buttons
        }
    }
}



