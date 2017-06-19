using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WindowSelector.Common;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.BrowserTabs.ViewModels;

namespace WindowSelector.Plugins.BrowserTabs.Providers
{
    public class ChromeWindowResultProvider : IWindowResultProvider
    {
        private readonly ITabsReceiver _tabsReceiver;
        private readonly ITabWindowResultFactory _tabWindowResultFactory;
        private readonly ILogger _logger;


        public ChromeWindowResultProvider(ITabsReceiver tabsReceiver, ITabWindowResultFactory tabWindowResultFactory, Func<ILogger> loggerFactory)
        {
            _tabsReceiver = tabsReceiver;
            _tabWindowResultFactory = tabWindowResultFactory;
            _logger = loggerFactory();
        }

        public async Task<IEnumerable<WindowResult>> GetResultsAsync(string keyword, string query, CancellationToken cancellationToken, SearchResultsWriter writer)
        {
            try
            {
                _logger.Debug($"Searching [{keyword}] for '{query}'");
                var results = await _tabsReceiver.RequestTabs(query, cancellationToken);
                _logger.Debug("Received results");
                writer.AddResults(results.Select(_tabWindowResultFactory.Create).OrderBy(r => r.DisplayText).ToList<WindowResult>());

                return new List<WindowResult>();
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Search canceled or timed out");
                return new List<WindowResult>();
            }
        }

        public bool SupportsKeyword(string keyword)
        {
            return keyword.In("c", null, "");
        }
    }
}
