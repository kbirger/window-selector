using Autofac;
using WindowSelector.Windows;

namespace WindowSelector.IoC
{
    public class WindowModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>().As<MainWindow>();
            builder.RegisterType<SettingsWindow>().As<SettingsWindow>();
            builder.RegisterType<ThumbnailWindow>().As<ThumbnailWindow>();
        }
    }
}
