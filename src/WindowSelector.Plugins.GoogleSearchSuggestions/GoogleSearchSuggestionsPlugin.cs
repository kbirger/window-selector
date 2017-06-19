using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NLog;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.GoogleSearchSuggestions.Providers;
using WindowSelector.Plugins.GoogleSearchSuggestions.ViewModels;

namespace WindowSelector.Plugins.GoogleSearchSuggestions
{
    public class GoogleSearchSuggestionsPlugin : IPlugin
    {
        public GoogleSearchSuggestionsPlugin(
            [Import(typeof(IConfigurationProvider))] IConfigurationProvider configurationProvider,
            [Import(typeof(Func<ILogger>))] Func<ILogger> loggerFactory)
        {
            ResultServices = new[]
            {
                new GoogleSearchSuggestionProvider(suggestion => new GoogleSuggestionWindowResult(suggestion)), 
            };

            RecentResultServices = new IRecentWindowRepository[0];
        }

        

        public IEnumerable<IWindowResultProvider> ResultServices { get; private set; }
        public IEnumerable<IRecentWindowRepository> RecentResultServices { get; private set; }
        public event EventHandler PluginStatusChanged;
        public IEnumerable<PluginStatusInfo> GetPluginStatus()
        {
            return new PluginStatusInfo[0];
        }
    }
}
