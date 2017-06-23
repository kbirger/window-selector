using System;
using System.Collections.Generic;

namespace WindowSelector.Common.Interfaces
{
    public interface IPlugin
    {
        /// <summary>
        /// Gets a set of services that allow getting of results by query
        /// </summary>
        IEnumerable<IWindowResultProvider> ResultServices { get; }

        /// <summary>
        /// Gets a set of services that retrieve recent results
        /// </summary>
        IEnumerable<IRecentWindowRepository> RecentResultServices { get; }

        event EventHandler PluginStatusChanged;

        IEnumerable<PluginStatusInfo> GetPluginStatus();
    }
}
