using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace WindowSelector.Win32
{
    [ExcludeFromCodeCoverage]
    public static class DWM
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr source, out IntPtr hthumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr HThumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr HThumbnail, ref ThumbnailProperties props);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr HThumbnail, out Size size);

        public struct Point
        {
            public int x;
            public int y;
        }

        public struct Size
        {
            public int Width, Height;
        }

        
        public struct ThumbnailProperties
        {
            public ThumbnailFlags Flags;
            public Rect Destination;
            public Rect Source;
            public Byte Opacity;
            public bool Visible;
            public bool SourceClientAreaOnly;
        }

        public struct Rect
        {
            public Rect(int x, int y, int x1, int y1)
            {
                this.Left = x;
                this.Top = y;
                this.Right = x1;
                this.Bottom = y1;
            }

            public int Left, Top, Right, Bottom;
        }

        [Flags]
        public enum ThumbnailFlags : int
        {
            RectDetination = 1,
            RectSource = 2,
            Opacity = 4,
            Visible = 8,
            SourceClientAreaOnly = 16
        }

        
    }
}