using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WindowSelector.Signalr.Models;

namespace WindowSelector.Signalr
{
    [HubName("chromeTabsHub")]
    public class ChromeTabsHub : Hub
    {
        public const string HubName = "chromeTabsHub";
        private ITabsReceiver _tabsReceiver;
        private HubConnectionTrackerService _connections;

        public ChromeTabsHub(ITabsReceiver receiver, HubConnectionTrackerService connectionTrackerService)
        {
            _connections = connectionTrackerService;
            _tabsReceiver = receiver;
        }
        public void SendTabs(string query, IEnumerable<TabInfo> tabs)
        {
            _tabsReceiver.SendTabs(query, tabs);
        }


        public override Task OnConnected()
        {
            _connections.Add(HubName, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _connections.Remove(HubName, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _connections.Add(HubName, Context.ConnectionId);
            return base.OnReconnected();
        }
    }


    public class HubConnectionTrackerService
    {

        public sealed class ConnectionCountChangedEventArgs : EventArgs
        {
            public ConnectionCountChangedEventArgs(string name)
            {
                HubName = name;
            }

            public string HubName { get; private set; }
        }
        private readonly Dictionary<string, HashSet<string>> _connections =
            new Dictionary<string, HashSet<string>>();

        public event EventHandler<ConnectionCountChangedEventArgs> CountChanged;

        private void OnCountChanged(string name)
        {
            CountChanged?.Invoke(this, new ConnectionCountChangedEventArgs(name));
        }
        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(string key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                    OnCountChanged(key);
                }
            }
        }

        public int GetConnectionCount(string key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections.Count;
            }

            return 0;
        }

        public void Remove(string key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                    OnCountChanged(key);
                }
            }
        }
    }
}
