using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowSelector.Plugins.Win32.Properties;
using WindowSelector.Win32;

namespace WindowSelector.Plugins.Win32
{
    public class IconRetriever : INotifyPropertyChanged
    {
        private readonly IWin32ApiWrapper _win32ApiWrapper;
        //private readonly Dispatcher _dispatcher;
        private Icon _icon;
        private readonly Queue<int> _remaining = new Queue<int>();
        private ImageSource _imageSource;

        private Win32Api.SendMessageDelegate del;
        private byte[] _imageBytes;

        public IconRetriever(IWin32ApiWrapper win32ApiWrapper/*, Dispatcher dispatcher*/)
        {
            _win32ApiWrapper = win32ApiWrapper;
            //_dispatcher = dispatcher;
            del = new Win32Api.SendMessageDelegate(Callback);
            _remaining.Enqueue(Win32Api.ICON_BIG);
            _remaining.Enqueue(Win32Api.ICON_SMALL2);
            _remaining.Enqueue(Win32Api.ICON_SMALL);
        }

        public byte[] ImageBytes
        {
            get { return _imageBytes; }
            set
            {
                _imageBytes = value;
                OnPropertyChanged();
            }
        }

        public Icon Icon
        {
            get { return _icon; }
            private set
            {
                if (value == null) return;
                _icon = value;
                OnPropertyChanged();
                //ImageBytes = _icon.ToBytes();
                var i = _icon.ToImageSource();
                i.Freeze();
                
                Image = i;
                //_dispatcher.Invoke(() => Image = _icon.ToImageSource());
            }
        }

        public ImageSource Image
        {
            get { return _imageSource; }
            private set
            {
                _imageSource = value; 
                OnPropertyChanged();
            }
        }

        public void LoadIcon(IntPtr hWnd)
        {
            if (Icon != null)
            {
                throw new InvalidOperationException("Cannot call LoadIcon more than once.");
            }

            Icon = _win32ApiWrapper.GetAppClassIcon(hWnd);
            var nextAttempt = _remaining.Dequeue();
            //_dispatcher.Invoke(
            //() =>
            //_win32ApiWrapper.SendMessageAsync(hWnd, Win32Api.WM_GETICON, nextAttempt, del);
            //IntPtr outt;
            //Win32Api.SendMessageTimeout(hWnd, Win32Api.WM_GETICON, nextAttempt, IntPtr.Zero, 0x0002, 50, out outt);
            IntPtr outt = _win32ApiWrapper.SendMessageTimeout(hWnd, Win32Api.WM_GETICON, nextAttempt, IntPtr.Zero, 50);
            Callback(hWnd, Win32Api.WM_GETICON, UIntPtr.Zero, outt);
            //);
        }
        // cleanup to make async again
        public void Callback(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult)
        {

            if (lResult == IntPtr.Zero)
            {
                if (_remaining.Count > 0)
                {
                    var nextAttempt = _remaining.Dequeue();
                    //IntPtr outt;
                    //_win32ApiWrapper.SendMessageAsync(hWnd, Win32Api.WM_GETICON, nextAttempt, del);
                    IntPtr outt = _win32ApiWrapper.SendMessageTimeout(hWnd, Win32Api.WM_GETICON, nextAttempt, IntPtr.Zero, 50);

                    Callback(hWnd, Win32Api.WM_GETICON, UIntPtr.Zero, outt);

                }
                else
                {
                    del = null;
                }
            }
            else
            {
                Icon = Icon.FromHandle(lResult);
                del = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private Process GetUWPAppHost(int pid, IntPtr handle)
        {
            
            var children = _win32ApiWrapper.GetChildWindows(handle);
            foreach (var childHwnd in children)
            {
                int childPid = _win32ApiWrapper.GetWindowProcess(childHwnd);
                if (childPid != pid)
                {
                    // here we are
                    Process childProc = Process.GetProcessById(childPid);
                    return childProc;
                }
            }

            throw new Exception("No host process found for supposed UWP app.");
        }

        public void LoadUWPIcon(IntPtr handle)
        {
            int pid = _win32ApiWrapper.GetWindowProcess(handle);
            try
            {
                var hostProcess = GetUWPAppHost(pid, handle);
                var package = AppxPackage.FromProcess(hostProcess);

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                string finalFileName = Path.GetFileNameWithoutExtension(package.Logo) + ".scale-200" +
                                   Path.GetExtension(package.Logo);
                string finalPath = Path.Combine(Path.GetDirectoryName(package.Logo), finalFileName);
                bmp.UriSource = new Uri(Path.Combine(package.Path, finalPath));
                bmp.EndInit();
                bmp.Freeze();

                Image = bmp;
            }
            catch
            {
                
            }
        }
    }


}
