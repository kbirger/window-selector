using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.Win32.Models;
using WindowSelector.Win32;

namespace WindowSelector.Plugins.Win32
{
    [Export(typeof(IWindowManager))]
    public class WindowManager : IWindowManager
    {
        private ConfigurationRoot _configuration;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IWin32ApiWrapper _win32Api;
        private readonly Func<IconRetriever> _iconRetrieverFactoryFunc;

        [ImportingConstructor]
        public WindowManager(IConfigurationProvider configurationProvider, IWin32ApiWrapper win32Api/*, Func<IconRetriever> iconRetrieverFactoryFunc*/)
        {
            _win32Api = win32Api;
            //_iconRetrieverFactoryFunc = iconRetrieverFactoryFunc;
            _configurationProvider = configurationProvider;
            _configuration = configurationProvider.GetConfiguration();
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);
        }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            if (args.ChangedProperties.Any(x => x.In("Blacklist", "Whitelist")))
            {
                _configuration = _configurationProvider.GetConfiguration();
            }
        }

        private bool IsValidWindow(IntPtr wnd, string className)
        {
            WINDOWINFO wi = _win32Api.GetWindowInfo(wnd);
            
            return 
                wi.dwStyle.HasFlag(WindowStyles.WS_CAPTION) && 
                (!wi.dwStyle.HasFlag(WindowStyles.WS_DISABLED) &&
                wi.dwStyle.HasFlag(WindowStyles.WS_VISIBLE));


        }

        // Only for debugging
        [ExcludeFromCodeCoverage]
        private static WindowStyles[] GetAllStyles(WindowStyles input)
        {
            var vals = Enum.GetValues(typeof(WindowStyles));
            var ret = new List<WindowStyles>();
            foreach (var val in vals)
            {
                if (input.HasFlag((WindowStyles) val))
                {
                    ret.Add((WindowStyles)val);
                }
            }

            return ret.ToArray();
        }

        // Only for debugging
        [ExcludeFromCodeCoverage]
        private static WindowStylesEx[] GetAllExStyles(WindowStylesEx input)
        {
            var vals = Enum.GetValues(typeof(WindowStylesEx));
            var ret = new List<WindowStylesEx>();
            foreach (var val in vals)
            {
                if ((uint)val == 0x0) continue;
                if (input.HasFlag((WindowStylesEx)val))
                {
                    ret.Add((WindowStylesEx)val);
                }
            }

            ret.Add(WindowStylesEx.WS_EX_RIGHTSCROLLBAR);
            return ret.ToArray();
        }
        

        public void CenterWindow(IntPtr hWnd)
        {
            RECT rect = _win32Api.GetWindowRect(hWnd);
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var windowWidth = (double)rect.Right - rect.Left;
            var windowHeight = (double)rect.Bottom - rect.Top;
            int x = (int) (screenWidth/2 - windowWidth/2);
            int y = (int) (screenHeight/2 - windowHeight/2);
            _win32Api.MoveWindow(hWnd, x, y);
            
        }

        public void MinimizeWindow(IntPtr hWnd)
        {
            _win32Api.ShowWindow(hWnd, Win32Api.WindowShowStyle.ShowMinNoActivate);
        }

        public void Close(IntPtr hWnd)
        {
            _win32Api.CloseWindow(hWnd);
        }

        public void ShowWindow(IntPtr hWnd)
        {
            var mode = _win32Api.GetWindowPlacement(hWnd).showCmd;
            _win32Api.SetForegroundWindow(hWnd);
            if (mode.In(Win32Api.WindowShowStyle.Hide, Win32Api.WindowShowStyle.ShowMinimized, Win32Api.WindowShowStyle.Minimize))
            {
                _win32Api.ShowWindow(hWnd, Win32Api.WindowShowStyle.Restore);
            }
            else if (!mode.In(Win32Api.WindowShowStyle.ShowNormal, Win32Api.WindowShowStyle.ShowMaximized))
            {
                _win32Api.ShowWindow(hWnd, Win32Api.WindowShowStyle.ShowNormal);
            }

            
        }

        public bool TryGetWindowInfo(IntPtr handle, out INativeWindowInfo windowInfo, bool includeHidden = false,
            bool loadIcon = true)
        {
            try
            {
                int processId = _win32Api.GetWindowProcess(handle);
                var proc = Process.GetProcessById(processId);
                NativeWindowInfo retVal;
                bool retStatus = TryGetWindowInfo(handle, proc, out retVal, includeHidden, loadIcon);
                windowInfo = retVal;
                return retStatus;
            }
            catch (Win32Exception)
            {
                windowInfo = null;
                return false;
            }
            
        }


        private bool TryGetWindowInfo(IntPtr handle, Process owningProcess, out NativeWindowInfo windowInfo, bool includeHidden = false, bool loadIcon = true)
        {
            try
            {
                string windowTitle = _win32Api.GetWindowText(handle);
                var className = _win32Api.GetClassName(handle);

                var ir = new IconRetriever(_win32Api);

                var isWhitelisted = _configuration.Whitelist.Contains(className);
                var isBlacklisted = _configuration.Blacklist.Contains(className);
                if ((!isBlacklisted || includeHidden) &&
                    (includeHidden || isWhitelisted || IsValidWindow(handle, className)))
                {
                    windowInfo = new NativeWindowInfo()
                    {
                        hWnd = handle,
                        PID = owningProcess.Id,
                        ProcessName = owningProcess.ProcessName,
                        Wndclass = className,
                        Title = windowTitle,
                        IsWhiteListed = isWhitelisted,
                        IsBlackListed = isBlacklisted,
                        Icon = ir
                    };
                    if (loadIcon)
                    {
                        ir.LoadIcon(handle);
                    }
                    return true;
                }
                // else, fall through
            }
            catch (Win32Exception)
            {
                // fall through
            }

            windowInfo = null;
            return false;
        }

        public IEnumerable<INativeWindowInfo> FindWindowsWithProcess(string searchString, bool showAll)
        {
            return FindWindowsWithProcessInteral(searchString, showAll);
        }

        internal IEnumerable<NativeWindowInfo> FindWindowsWithProcessInteral(string searchString, bool showAll)
        {
            if (string.IsNullOrWhiteSpace(searchString)) return new NativeWindowInfo[0];
            List<NativeWindowInfo> ret = new List<NativeWindowInfo>();
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.Id == 0) continue;
                bool processNameMatches = proc.ProcessName.ContainsCaseInsensitive(searchString);

                foreach (ProcessThread thread in proc.Threads)
                {
                    //string procName = proc.ProcessName;
                    //bool match = proc.ProcessName.ContainsCaseInsensitive(searchString);
                    _win32Api.EnumThreadWindows((uint)thread.Id, delegate(IntPtr wnd, IntPtr param)
                    {
                        NativeWindowInfo windowInfo;
                        if (TryGetWindowInfo(wnd, proc, out windowInfo, includeHidden: showAll, loadIcon: false)
                            && (processNameMatches || windowInfo.Title.ContainsCaseInsensitive(searchString)))
                        {
                            windowInfo.Icon.LoadIcon(windowInfo.hWnd);
                            ret.Add(windowInfo);
                        }
                        return true;
                    });

                }
            }

            return ret;
        }
    }
}

