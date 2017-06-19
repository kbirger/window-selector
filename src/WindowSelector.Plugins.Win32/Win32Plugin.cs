using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.Win32.Providers;
using WindowSelector.Plugins.Win32.ViewModels;
using WindowSelector.Win32;

namespace WindowSelector.Plugins.Win32
{
    public class Win32Plugin : IPlugin
    {
        public Win32Plugin(
            [Import(typeof(IConfigurationProvider))] IConfigurationProvider configurationProvider,
            [Import(typeof(Func<ILogger>))] Func<ILogger> loggerFactory)
        {
            var win32Api = new Win32ApiWrapper();
            var windowManager = new WindowManager(configurationProvider, win32Api);
            var recentWindowService = new RecentWindowRepository(windowManager, configurationProvider);

            IWindowWatcher windowWatcher = new WindowWatcher(
                recentWindowService, 
                new Win32ApiWrapper(),
                loggerFactory);
            windowWatcher.Start();


            ResultServices = new[]
            {
                new NativeWindowResultProvider(
                    windowManager, 
                    (data) => new  NativeWindowResult(data, windowManager, configurationProvider, recentWindowService), 
                    loggerFactory)
            };

            RecentResultServices = new[] {recentWindowService};
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
