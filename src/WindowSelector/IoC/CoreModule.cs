using System.Windows;
using Autofac;
using Autofac.Integration.Mef;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Providers;
//using WindowSelector.Signalr.Providers;
//using WindowSelector.Signalr.ViewModels;
using WindowSelector.ViewModels;
using WindowSelector.Win32;

namespace WindowSelector.IoC
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // SystemInterfaces wrapper setup
            builder.RegisterType<SystemWrapper.IO.FileWrap>().As<SystemInterface.IO.IFile>().SingleInstance();
            builder.RegisterType<SystemWrapper.IO.FileSystemWatcherFactory>()
                .As<SystemInterface.IO.IFileSystemWatcherFactory>();


            //builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            //builder.RegisterType<WindowResultFactory>().As<IWindowResultFactory>();
            //builder.Register<WindowResult>((context, p) =>
            //{
            //    return context.Resolve<WindowResult>(p);
            //}).As<NativeWindowResult>().As<TabWindowResult>();
             builder.RegisterType<MainWindowViewModel>().As<MainWindowViewModel>();
            builder.RegisterType<SettingsWindowViewModel>().As<SettingsWindowViewModel>();
            builder.RegisterType<Win32ApiWrapper>().As<IWin32ApiWrapper>()
                .Exported(x => x.As<IWin32ApiWrapper>())
                .SingleInstance();
            builder.RegisterType<ConfigurationProvider>()
                .As<IConfigurationProvider>()
                .Exported(x => x.As<IConfigurationProvider>())
                .SingleInstance();
            builder.RegisterType<WindowResultsViewModel>().As<WindowResultsViewModel>();

            builder.Register(i => Application.Current.Dispatcher).ExternallyOwned();

            //builder.RegisterType<TabWindowResult>().As<TabWindowResult>();
            //builder.RegisterType<NativeWindowResult>().As<NativeWindowResult>();
            builder.RegisterType<GoogleSuggestionWindowResult>().As<GoogleSuggestionWindowResult>();

            //builder.RegisterType<NativeWindowResultProvider>().As<IWindowResultProvider>().SingleInstance();
            //builder.RegisterType<ChromeWindowResultProvider>().As<IWindowResultProvider>().SingleInstance();
            //builder.RegisterType<GoogleSearchSuggestionProvider>().As<IWindowResultProvider>().SingleInstance();
            //builder.RegisterType<RecentWindowRepository>().As<IRecentWindowRepository>().SingleInstance();
            //builder.RegisterType<IconRetriever>()
            //    .As<IconRetriever>()
            //    .Exported(x => x.As<IconRetriever>());

            //builder.RegisterType<WindowWatcher>().As<IWindowWatcher>();

        }
    }
}
