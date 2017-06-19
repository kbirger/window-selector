using System;
using System.ComponentModel.Composition;
using System.Threading;
using NLog;
using WindowSelector.Common.Interfaces;
using WindowSelector.Win32;

namespace WindowSelector.Plugins.Win32
{
    public interface IWindowWatcher
    {
        void Start();
        void Stop();
        void Dispose();
    }

    [Export(typeof(IWindowWatcher))]
    public class WindowWatcher : IDisposable, IWindowWatcher
    {
        private const int INTERVAL = 500;
        private bool _stopRequested = false;
        private readonly Thread _workerThread;

        private readonly IRecentWindowRepository _recentWindowRepository;
        private readonly IWin32ApiWrapper _win32ApiWrapper;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public WindowWatcher(IRecentWindowRepository recentWindowRepository, IWin32ApiWrapper win32ApiWrapper, Func<ILogger> loggerFactory)
        {
            _recentWindowRepository = recentWindowRepository;
            _win32ApiWrapper = win32ApiWrapper;
            _logger = loggerFactory();
            _workerThread = new Thread(Run);
        }

        public void Start()
        {
            _stopRequested = false;
            _workerThread.Start();
        }

        public void Stop()
        {
            _stopRequested = true;
            _workerThread.Join(100);
        }

        private void Run()
        {
            while (!_stopRequested)
            {
                try
                {
                    IntPtr handle = _win32ApiWrapper.GetForegroundWindow();
                    if (handle != IntPtr.Zero)
                    {
                        _recentWindowRepository.PushNew(handle);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error getting foreground window");
                }
                Thread.Sleep(INTERVAL);
            }
        }

        
        public void Dispose()
        {
            Stop();
        }
    }
}
