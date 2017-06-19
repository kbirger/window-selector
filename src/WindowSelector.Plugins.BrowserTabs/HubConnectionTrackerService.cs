using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace WindowSelector.Plugins.BrowserTabs
{
    [Export(typeof(HubConnectionTrackerService))]
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
        public int GetCount(string key)
        {
            lock (_connections)
            {
                HashSet<string> ret;
                return !_connections.TryGetValue(key, out ret) ? 0 : ret.Count;
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