//using Autofac;
//using Autofac.Integration.SignalR;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Infrastructure;
//using WindowSelector.Signalr;

//namespace WindowSelector.IoC
//{
//    public class ChromeModule : Module
//    {
//        private readonly HubConfiguration _hubConfiguration;
//        public ChromeModule(HubConfiguration hubConfiguration)
//        {
//            _hubConfiguration = hubConfiguration;
//        }

//        protected override void Load(ContainerBuilder builder)
//        {
//            builder.RegisterHubs(typeof(ChromeTabsHub).Assembly);
//            builder.Register(i => _hubConfiguration.Resolver.Resolve<IConnectionManager>().GetHubContext<ChromeTabsHub, IChromeExtension>()).ExternallyOwned();
//            builder.RegisterType<HubConnectionTrackerService>().As<HubConnectionTrackerService>().SingleInstance();
//            builder.RegisterType<TabsReceiver>().As<ITabsReceiver>();

//        }
//    }
//}
