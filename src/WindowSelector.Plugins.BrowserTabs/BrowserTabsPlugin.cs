

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Logging;
using Owin;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.BrowserTabs;
using WindowSelector.Signalr;
using WindowSelector.Signalr.Providers;
using WindowSelector.Signalr.ViewModels;
using ILogger = NLog.ILogger;

namespace WindowSelector.Plugins.BrowserTabs
{
    public class BrowserTabsPlugin : IPlugin
    {
        public BrowserTabsPlugin(
            [Import(typeof(Func<ILogger>))] Func<ILogger> loggerFactory, 
            [Import(typeof(IConfigurationProvider))] IConfigurationProvider configurationProvider, 
            [Import(typeof(IWindowManager))] IWindowManager windowManager)
        {
            RegisterTypes(loggerFactory, configurationProvider, windowManager);

            // todo: configurable
            string url = "http://localhost:8084";
            WebApp.Start<Startup>(url);

            ResultServices = new IWindowResultProvider[]
            {
                GlobalHost.DependencyResolver.Resolve<IWindowResultProvider>()
            };

            RecentResultServices = new IRecentWindowRepository[0];
        }

        private void RegisterTypes(Func<ILogger> loggerFactory, IConfigurationProvider configurationProvider, IWindowManager windowManager)
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

            var connectionTrackingService = new HubConnectionTrackerService();
            
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
                () => connectionTrackingService);

            GlobalHost.DependencyResolver.Register(
                typeof(ChromeTabsHub),
                () => new ChromeTabsHub(tabsReceiver, connectionTrackingService));

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
