using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowSelector.Win32
{
    public interface IWin32ApiWrapper
    {
        void ShowWindow(IntPtr handle, Win32Api.WindowShowStyle cmdShow);
        void SetForegroundWindow(IntPtr hWnd);
        IntPtr GetForegroundWindow();
        Win32Api.WINDOWPLACEMENT GetWindowPlacement(IntPtr hWnd);
        string GetWindowText(IntPtr hWnd);
        bool EnumThreadWindows(uint dwThreadId, Win32Api.EnumThreadDelegate lpfn);
        WINDOWINFO GetWindowInfo(IntPtr hWnd);
        RECT GetWindowRect(IntPtr hWnd);
        void MoveWindow(IntPtr hWnd, int x, int y);
        string GetClassName(IntPtr hWnd);
        System.Drawing.Icon GetAppIcon(IntPtr hwnd);
        void SendMessageAsync(IntPtr hWnd, uint Msg, int wParam, Win32Api.SendMessageDelegate lpCallBack);
        void CloseWindow(IntPtr hWnd);
        int GetWindowProcess(IntPtr hWnd);

        System.Drawing.Icon GetAppClassIcon(IntPtr hWnd);
    }

    [ExcludeFromCodeCoverage]
    public class Win32ApiWrapper : IWin32ApiWrapper
    {
        public void ShowWindow(IntPtr handle, Win32Api.WindowShowStyle cmdShow)
        {
            if (!Win32Api.ShowWindow(handle, cmdShow))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void SetForegroundWindow(IntPtr hWnd)
        {
            if (!Win32Api.SetForegroundWindow(hWnd))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public IntPtr GetForegroundWindow()
        {
            return Win32Api.GetForegroundWindow();
        }

        public int GetWindowProcess(IntPtr hWnd)
        {
            Win32Api.SetLastError(0);
            int processId;

            Win32Api.GetWindowThreadProcessId(hWnd, out processId);
            int err = Marshal.GetLastWin32Error();
            if (err > 0)
                throw new Win32Exception(err);

            return processId;
        }

        public Win32Api.WINDOWPLACEMENT GetWindowPlacement(IntPtr hWnd)
        {
            Win32Api.WINDOWPLACEMENT placement = new Win32Api.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            if(!Win32Api.GetWindowPlacement(hWnd, ref placement))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return placement;
        }

        public string GetWindowText(IntPtr hWnd)
        {
            // GetWindowText doesn't reset last error to 0 on success, so set it ourselves.
            Win32Api.SetLastError(0);
            int length = Win32Api.GetWindowTextLength(hWnd);
            int err = Marshal.GetLastWin32Error();
            if (err > 0)
                throw new Win32Exception(err);

            if (length == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(length + 1);


            if (Win32Api.GetWindowText(hWnd, sb, sb.Capacity) != length)
            {
                err = Marshal.GetLastWin32Error();
                if (err > 0)
                    throw new Win32Exception(err);
            }
                
            return sb.ToString();
        }

        public bool EnumThreadWindows(uint dwThreadId, Win32Api.EnumThreadDelegate lpfn)
        {
            return Win32Api.EnumThreadWindows(dwThreadId, lpfn, IntPtr.Zero);
        }

        public WINDOWINFO GetWindowInfo(IntPtr hWnd)
        {
            WINDOWINFO wi = new WINDOWINFO();
            if (!Win32Api.GetWindowInfo(hWnd, ref wi))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return wi;
        }

        public RECT GetWindowRect(IntPtr hWnd)
        {
            RECT lpRect;
            if(!Win32Api.GetWindowRect(hWnd, out lpRect))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return lpRect;
        }

        public void CloseWindow(IntPtr hWnd)
        {
            Win32Api.SendMessage(hWnd, Win32Api.WM_CLOSE, 0, 0);
        }

        public void MoveWindow(IntPtr hWnd, int x, int y)
        {
            if(!Win32Api.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0,0, SWP.NOSIZE | SWP.NOZORDER | SWP.SHOWWINDOW))
                throw new Win32Exception(Marshal.GetLastWin32Error());

        }

        public string GetClassName(IntPtr hWnd)
        {
            // Pre-allocate 256 characters, since this is the maximum class name length.
            StringBuilder className = new StringBuilder(256);
            //Get the window class name
            if(!Win32Api.GetClassName(hWnd, className, className.Capacity))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return className.ToString();
        }

        /// <summary>
        /// Gets the Icon of the application executable (not necessarily of the current instance represented by handle.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public System.Drawing.Icon GetAppClassIcon(IntPtr hWnd)
        {
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                iconHandle = Win32Api.GetClassLongPtr(hWnd, Win32Api.GCL_HICON);
            }
            catch { }
            try
            {
                if (iconHandle == IntPtr.Zero)
                    iconHandle = Win32Api.GetClassLongPtr(hWnd, Win32Api.GCL_HICONSM);
            }
            catch { }

            if (iconHandle == IntPtr.Zero)
                return null;

            return System.Drawing.Icon.FromHandle(iconHandle);
        }

        public System.Drawing.Icon GetAppIcon(IntPtr hwnd)
        {
            IntPtr iconHandle = Win32Api.SendMessage(hwnd, Win32Api.WM_GETICON, Win32Api.ICON_SMALL2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = Win32Api.SendMessage(hwnd, Win32Api.WM_GETICON, Win32Api.ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = Win32Api.SendMessage(hwnd, Win32Api.WM_GETICON, Win32Api.ICON_BIG, 0);

            if (iconHandle == IntPtr.Zero)
            {
                return GetAppClassIcon(hwnd);
            }

            System.Drawing.Icon icn = System.Drawing.Icon.FromHandle(iconHandle);

            return icn;
        }

        public void SendMessageAsync(IntPtr hWnd, uint Msg, int wParam, Win32Api.SendMessageDelegate lpCallBack)
        {
            if (!Win32Api.SendMessageCallback(hWnd, Msg, wParam, 0, lpCallBack, UIntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

    }


}
