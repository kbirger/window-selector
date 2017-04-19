using System;
using System.Collections.Generic;

namespace WindowSelector.Common.Interfaces
{
    public interface IWindowManager
    {
        IEnumerable<INativeWindowInfo> FindWindowsWithProcess(string searchString, bool showAll);
        void ShowWindow(IntPtr hWnd);
        void CenterWindow(IntPtr hWnd);
        void MinimizeWindow(IntPtr hWnd);
        void Close(IntPtr hWnd);

        bool TryGetWindowInfo(IntPtr handle, out INativeWindowInfo windowInfo, bool includeHidden = false,
            bool loadIcon = true);
    }
}