using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace TrayIconBuster
{
    /// <summary>
    /// I started with https://www.codeproject.com/Articles/19620/LP-TrayIconBuster as a base, but changed it to use the icon removal method from this script:
    /// https://techforpassion.blogspot.com/2014/04/refresh-tray-icons-how-to-remove-dead-tray-icons-after-killing-program.html (he isn't sure where it came
    /// from, and I was unable to find an original source either). Using Shell_NotifyIcon(NIM_DELETE,...) seems to work better for me - the old
    /// SendMessage(hWnd, TB_DELETEBUTTON,...) call would leave a blank spot where the icon used to be, but this does not.
    /// </summary>
    public static class TrayIconBuster
    {
        private const uint TB_BUTTONCOUNT = 0x0418; // WM_USER+24
        private const uint TB_GETBUTTON = 0x0417;   // WM_USER+23

        /// <summary>
        /// List of nested window class hierarchies leading to the toolbar windows. Slashes separate each class name
        /// from least to most specific (they should end on ToolbarWindow32). Each of these toolbars will have its
        /// phantom icons removed.
        /// </summary>
        private static readonly List<string> TrayWindowSearchList = new List<string>
        {
            //The regular toolbar window
            "Shell_TrayWnd/TrayNotifyWnd/SysPager/ToolbarWindow32",

            //The overflow window
            "NotifyIconOverflowWindow/ToolbarWindow32",
        };

        /// <summary>
        /// Removes any zombie icons (those whose processes no longer exist) from the notification tray.
        /// </summary>
        /// <returns>The number of tray icons removed.</returns>
        public static uint RemoveZombieIcons()
        {
            var toolbarButton = new ToolbarButton();
            var extraData = new ExtraButtonData();
            uint totalRemovedCount = 0;
            uint totalItemCount = 0;

            foreach (var windowList in TrayWindowSearchList)
            {
                //Get a handle to the toolbar window
                IntPtr toolbarHandle = FindNestedWindow(windowList);
                if (toolbarHandle == IntPtr.Zero)
                {
                    Utilities.Log($"No window found for list: {windowList}");
                    continue;
                }

                //Use that handle to open the toolbar's process
                using (LP_Process process = new LP_Process(toolbarHandle))
                {
                    //Allocate shared memory in the process to store toolbar button data for us to read
                    IntPtr remoteButtonPtr = process.Allocate(toolbarButton);
                    process.Allocate(extraData);

                    //Ask the window how many buttons it contains so we can iterate over them
                    uint itemCount = (uint)SendMessage(toolbarHandle, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
                    totalItemCount += itemCount;
                    Utilities.Log($"Found {itemCount} tray icons (some may be hidden)");
                    uint removedCount = 0;
                    for (uint item = 0; item < itemCount; item++)
                    {
                        //We remove items starting from the leftmost (#0), so for each item we have removed, the remaining items' indices
                        // will have 1 subtracted from them (since they all just got shifted left by 1).
                        uint index = item - removedCount;

                        //Get info for this toolbar button
                        if ((uint)SendMessage(toolbarHandle, TB_GETBUTTON, new IntPtr(index), remoteButtonPtr) == 0) throw new ApplicationException("TB_GETBUTTON failed");
                        process.Read(toolbarButton, remoteButtonPtr);
                        process.Read(extraData, toolbarButton.dwData);

                        //Open the handle for this button to see if its parent process exists or not. If it has no parent, it's a zombie - kill it!
                        IntPtr buttonHandle = extraData.hWnd;
                        if (buttonHandle == IntPtr.Zero) throw new ApplicationException("Invalid handle in tray button data");
                        using (LP_Process proc = new LP_Process(buttonHandle))
                        {
                            if (proc.ownerProcessID == 0)
                            {
                                RemoveIcon(extraData);
                                removedCount++;
                                totalRemovedCount++;
                            }
                        }
                    }
                }
            }

            Utilities.Log($"Done. {totalItemCount} icons found, {totalRemovedCount} icons removed.");
            return totalRemovedCount;
        }

        private static IntPtr FindNestedWindow(string windows)
        {
            var windowList = windows.Split('/');
            IntPtr hWnd = IntPtr.Zero;
            foreach (var name in windowList)
            {
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = FindWindow(name, null);
                }
                else
                {
                    hWnd = FindWindowEx(hWnd, IntPtr.Zero, name, null);
                }

                //If we get down here and haven't found a window, there's no point trying to continue - return null.
                if (hWnd == IntPtr.Zero) break;
            }
            return hWnd;
        }

        //See Shell_NotifyIcon function: https://msdn.microsoft.com/en-us/library/windows/desktop/bb762159(v=vs.85).aspx
        private const uint NIM_DELETE = 0x00000002;

        /// <summary>
        /// Removes the given tray icon using the TrayData we've fetched for it.
        /// </summary>
        private static void RemoveIcon(ExtraButtonData td)
        {
            var data = new NotifyIconData()
            {
                hWnd = td.hWnd,
                uID = td.uID
            };

            //Try to remove the icon. Throw the last Win32 error if this fails.
            if (!Shell_NotifyIcon(NIM_DELETE, data)) throw new Win32Exception();
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr SendMessage(IntPtr Hdc, uint Msg_Const, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIcon", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool Shell_NotifyIcon(uint dwMessage, NotifyIconData pnid);

        /// <summary>
        /// ToolBarButton struct used for TB_GETBUTTON message.
        /// </summary>
        /// <remarks>
        /// We use a class so LP_Process.Read can fill this.
        /// See TBBUTTON struct: http://msdn.microsoft.com/en-us/library/windows/desktop/bb760476(v=vs.85).aspx
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private class ToolbarButton
        {
            public uint iBitmap;        //  0
            public uint idCommand;      //  4
            public byte fsState;        //  8
            public byte fsStyle;        //  9
            public IntPtr dwData;       // 12 or 16: points to tray data
            public uint iString;        // 16 or 24
        }

        /// <summary>
        /// Struct used for extra info for ToolBarButton.
        /// </summary>
        /// <remarks>
        /// We use a class so LP_Process.Read can fill this. This struct apparently is undocumented since it's application-specific.
        /// There is likely more to it, but these are the only fields we care about.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private class ExtraButtonData
        {
            public IntPtr hWnd;             //  0
            public uint uID;                //  4 or 8
        }

        /// <summary>
        /// Struct used to make Shell_NotifyIcon calls.
        /// See NOTIFYICONDATA struct: https://msdn.microsoft.com/en-us/library/windows/desktop/bb773352(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class NotifyIconData
        {
            public uint cbSize = (uint)Marshal.SizeOf(typeof(NotifyIconData));
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] //This size should be 64 if running on something earlier than Win2k
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }
    }
}