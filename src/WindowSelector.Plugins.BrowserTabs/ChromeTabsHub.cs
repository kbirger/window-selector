using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WindowSelector.Plugins.BrowserTabs.Models;

namespace WindowSelector.Plugins.BrowserTabs
{
    [HubName("chromeTabsHub")]
    public class ChromeTabsHub : Hub
    {
        public const string ChromeClient = "Chrome";
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
            // todo: groups?
            //Groups.Add(Context.ConnectionId, ChromeClient);
            _connections.Add(ChromeClient, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // todo: groups?
            //Groups.Remove(Context.ConnectionId, ChromeClient);
            _connections.Remove(ChromeClient, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _connections.Add(ChromeClient, Context.ConnectionId);
            return base.OnReconnected();
        }
    }
}
