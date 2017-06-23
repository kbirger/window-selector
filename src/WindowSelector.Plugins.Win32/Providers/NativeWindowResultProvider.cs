using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WindowSelector.Common;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.Win32.Models;
using WindowSelector.Plugins.Win32.ViewModels;

namespace WindowSelector.Plugins.Win32.Providers
{
    public class NativeWindowResultProvider : IWindowResultProvider
    {
        private readonly WindowManager _windowManager;
        private readonly Func<NativeWindowInfo, WindowResult> _nativeWindowResultFactoryFunc;
        private readonly ILogger _logger;

        public NativeWindowResultProvider(WindowManager windowManager, Func<NativeWindowInfo, NativeWindowResult> nativeWindowResultFactoryFunc, Func<ILogger> loggerFactory)
        {
            _windowManager = windowManager;
            _nativeWindowResultFactoryFunc = nativeWindowResultFactoryFunc;
            _logger = loggerFactory();
        }


        public async Task<IEnumerable<WindowResult>> GetResultsAsync(string keyword, string query, CancellationToken cancellationToken, SearchResultsWriter writer)
        {
            try
            {
                _logger.Debug($"Searching [{keyword}] for '{query}'");
                var results = await Task.Run(() => _windowManager.FindWindowsWithProcessInteral(query, keyword == "!"), cancellationToken);
                //var results = _windowManager.FindWindowsWithProcess(query, keyword == "!");
                _logger.Debug("Received results");
                var x = results.Select(_nativeWindowResultFactoryFunc).OrderBy(r => r.DisplayText);
                writer.AddResults(x.ToList());

                return x;
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Search canceled or timed out");
                return new List<WindowResult>();
            }
        }

        public bool SupportsKeyword(string keyword)
        {
            return keyword.In("!", ".", null, "");
        }
    }
}
