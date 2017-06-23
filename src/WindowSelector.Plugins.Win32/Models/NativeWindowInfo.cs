using System;
using System.ComponentModel.Composition;
using WindowSelector.Common;

namespace WindowSelector.Plugins.Win32.Models
{
    [Export(typeof(INativeWindowInfo))]
    public class NativeWindowInfo : INativeWindowInfo
    {
        public string ProcessName { get; set; }
        public bool IsWhiteListed { get; set; }
        public bool IsBlackListed { get; set; }
        public int PID { get; set; }

        public string Wndclass { get; set; }
        public IntPtr hWnd { get; set; }
        public string Title { get; set; }
        
        public IconRetriever Icon { get; set; }
    }

}
