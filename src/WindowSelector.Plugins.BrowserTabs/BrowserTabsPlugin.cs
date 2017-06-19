

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.BrowserTabs.ViewModels;
using WindowSelector.Signalr;
using WindowSelector.Signalr.Providers;
using ILogger = NLog.ILogger;

namespace WindowSelector.Plugins.BrowserTabs
{
    public class BrowserTabsPlugin : IPlugin
    {
        private readonly HubConnectionTrackerService _hubConnectionTrackerService;
        private readonly string[] BROWSERS = new[] {"Chrome"};
        public BrowserTabsPlugin(
            [Import(typeof(Func<ILogger>))] Func<ILogger> loggerFactory, 
            [Import(typeof(IConfigurationProvider))] IConfigurationProvider configurationProvider, 
            [Import(typeof(IWindowManager))] IWindowManager windowManager,
            [Import(typeof(HubConnectionTrackerService))] HubConnectionTrackerService hubConnectionTrackerService)
        {
            RegisterTypes(loggerFactory, configurationProvider, windowManager, hubConnectionTrackerService);

            // todo: configurable
            string url = "http://localhost:8084";
            WebApp.Start<Startup>(url);

            ResultServices = new IWindowResultProvider[]
            {
                GlobalHost.DependencyResolver.Resolve<IWindowResultProvider>()
            };

            RecentResultServices = new IRecentWindowRepository[0];
            _hubConnectionTrackerService = hubConnectionTrackerService;
            _hubConnectionTrackerService.CountChanged += HubConnectionTrackerServiceOnCountChanged;
        }

        private void HubConnectionTrackerServiceOnCountChanged(object sender, HubConnectionTrackerService.ConnectionCountChangedEventArgs connectionCountChangedEventArgs)
        {
            PluginStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RegisterTypes(Func<ILogger> loggerFactory, IConfigurationProvider configurationProvider, IWindowManager windowManager, HubConnectionTrackerService hubConnectionTrackerService)
        {
            GlobalHost.DependencyResolver.Register(
                typeof(Func<ILogger>), 
                () => loggerFactory);

            GlobalHost.DependencyResolver.Register(
                typeof(IConfigurationProvider),
                () => configurationProvider);


            var tabsReceiver = new TabsReceiver(
                GlobalHost.DependencyResolver.Resolve<IConnectionManager>(),
                GlobalHost.DependencyResolver.Resolve<Func<ILogger>>());

            
            var tabWindowResultFactory = new TabWindowResultFactory(
                GlobalHost.DependencyResolver.Resolve<IConnectionManager>(), 
                configurationProvider, 
                windowManager, 
                loggerFactory);
            GlobalHost.DependencyResolver.Register
                (typeof(ITabWindowResultFactory), 
                () => tabWindowResultFactory);

            GlobalHost.DependencyResolver.Register(
                typeof(ITabsReceiver),
                () => tabsReceiver);

            GlobalHost.DependencyResolver.Register(
                typeof(HubConnectionTrackerService),
                () => hubConnectionTrackerService);

            GlobalHost.DependencyResolver.Register(
                typeof(ChromeTabsHub),
                () => new ChromeTabsHub(tabsReceiver, hubConnectionTrackerService));

            var tabProvider = new ChromeWindowResultProvider(
                tabsReceiver,
                GlobalHost.DependencyResolver.Resolve<ITabWindowResultFactory>(),
                loggerFactory
                );

            GlobalHost.DependencyResolver.Register(typeof(IWindowResultProvider), ()=> tabProvider);
        }


        public IConnectionManager ConnectionManager => GlobalHost.DependencyResolver.Resolve<IConnectionManager>();

        public IEnumerable<IWindowResultProvider> ResultServices { get; }
        public IEnumerable<IRecentWindowRepository> RecentResultServices { get; }
        public event EventHandler PluginStatusChanged;
        public IEnumerable<PluginStatusInfo> GetPluginStatus()
        {
            return
                BROWSERS.Select(
                    browser =>
                        new PluginStatusInfo(browser, _hubConnectionTrackerService.GetConnectionCount(browser) > 0));
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.UseCors(CorsOptions.AllowAll);
            var signalRConfig = new HubConfiguration();
            signalRConfig.EnableDetailedErrors = true;

            
            //app.MapSignalR();
            app.MapSignalR("/signalr", signalRConfig);


        }
    }
}
