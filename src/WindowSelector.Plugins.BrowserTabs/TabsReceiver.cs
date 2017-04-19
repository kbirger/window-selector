using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NLog;
using WindowSelector.Signalr.Models;

namespace WindowSelector.Signalr
{
    public interface ITabsReceiver
    {
        Task<IEnumerable<TabInfo>> RequestTabs(string substring, CancellationToken token);
        void SendTabs(string query, IEnumerable<TabInfo> tabs);
    }

    public class TabsReceiver : ITabsReceiver
    {
        private readonly IHubContext<IChromeExtension> _context;

        private static readonly ConcurrentDictionary<string, TaskCompletionSource<IEnumerable<TabInfo>>> _responses =
            new ConcurrentDictionary<string, TaskCompletionSource<IEnumerable<TabInfo>>>();

        private readonly ILogger _logger;

        public TabsReceiver(IConnectionManager connectionManager, Func<ILogger> loggerFactory)
        {

            _context = connectionManager.GetHubContext<ChromeTabsHub, IChromeExtension>();
            _logger = loggerFactory();
        }

        public Task<IEnumerable<TabInfo>> RequestTabs(string substring, CancellationToken token)
        {
            TaskCompletionSource<IEnumerable<TabInfo>> tsc = new TaskCompletionSource<IEnumerable<TabInfo>>(substring);
            _responses[substring] = tsc;

            _context.Clients.All.QueryTabs(substring);
            Task.Delay(1000, token).ContinueWith((t, state) =>
            {
                if (!tsc.Task.IsCompleted)
                {
                    _logger.Info($"Search for '{state}' timed out.");
                    tsc.SetCanceled();
                    TaskCompletionSource<IEnumerable<TabInfo>> temp;
                    _responses.TryRemove((string) state, out temp);
                }
            }, substring);
            return tsc.Task;
        }

        public void SendTabs(string query, IEnumerable<TabInfo> tabs)
        {
            TaskCompletionSource<IEnumerable<TabInfo>> ret;
            if (_responses.TryRemove(query, out ret))
            {
                ret.TrySetResult(tabs);
            }
        }
    }
}