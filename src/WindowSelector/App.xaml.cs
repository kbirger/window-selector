using System;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using NLog;
using WindowSelector.IoC;
using WindowSelector.Windows;

namespace WindowSelector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger _log;

        protected override void OnStartup(StartupEventArgs e)
        {
            _log = LogManager.GetCurrentClassLogger();
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
    LogUnhandledException((Exception)ex.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, ex) =>
                LogUnhandledException(ex.Exception, "Application.Current.DispatcherUnhandledException");

            Application.Current.DispatcherUnhandledException += (sender, ex) =>
                LogUnhandledException(ex.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, ex) =>
                LogUnhandledException(ex.Exception, "TaskScheduler.UnobservedTaskException");


            IoCSetup.Init();


            //WebApp.Start(url);
            base.OnStartup(e);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            IoCSetup.Container.Resolve<MainWindow>().Show();
        }

        private void LogUnhandledException(Exception exception, string @event)
        {
            // todo: add custom formatter so you can easily add @event here.
            // otherwise nlog stops printing the exception details.
            _log.Fatal(exception);
        }
    }

    //class Startup
    //{
    //    public void Configuration(IAppBuilder app)
    //    {
    //        IoCSetup.Init(app);

    //        app.UseCors(CorsOptions.AllowAll);
    //        app.MapSignalR();
    //    }
    //}
}
