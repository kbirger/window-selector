using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mef;
using Microsoft.AspNet.SignalR;
using NLog;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.WPF;

namespace WindowSelector.IoC
{
    public static class IoCSetup
    {
        public static IContainer Container { get; set; }
        public static IContainer Init()
        {
            var signalRConfig = new HubConfiguration();
            var builder = new ContainerBuilder();
            var plugs = new PluginRepository();
            var cat = plugs.Initialize();

            // looks up assemblies and their dependencies from 
            // <probing> element in config.
            // also consider:
            /* AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolder);

            static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
            {
                string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath)) return null;
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }*/
            // https://stackoverflow.com/questions/1373100/how-to-add-folder-to-assembly-search-path-at-runtime-in-net
            builder.RegisterComposablePartCatalog(cat,
                //new TypedService(typeof(IWindowResultProvider)),
                new TypedService(typeof(IPlugin)),
                new TypedService(typeof(IWpfViewPlugin))
                //,new TypedService(typeof(IRecentWindowRepository))
                );

            builder.RegisterModule(new NLogModule());
            //builder.RegisterModule(new ChromeModule(signalRConfig));
            builder.RegisterModule(new CoreModule());
            builder.RegisterModule(new WindowModule());


            builder.Register<Func<ILogger>>(logger => () => LogManager.GetLogger(GetClassFullName()))
                .As<Func<ILogger>>()
                .Exported(x => x.As<Func<ILogger>>());


            // SignalR Dependency Resolver
            //signalRConfig.Resolver = new AutofacDependencyResolver(container);
            //var plug = container.Resolve<System.Collections.Generic.IEnumerable<IPlugin>>();

            //var prov = plug.SelectMany(p => p.ResultServices ?? new IWindowResultProvider[0]);

            builder.Register((c) => c.Resolve<IEnumerable<IPlugin>>().SelectMany(p => p.ResultServices ?? new IWindowResultProvider[0])).SingleInstance();
            builder.Register((c) => c.Resolve<IEnumerable<IPlugin>>().SelectMany(p => p.RecentResultServices ?? new IRecentWindowRepository [0])).SingleInstance();
            //app.UseAutofacMiddleware(container);
            //app.MapSignalR("/signalr", signalRConfig);

            var container = builder.Build();

            Container = container;

            //var prov = container.Resolve<System.Collections.Generic.IEnumerable<IWindowResultProvider>>();
            return container;
        }

        private static string GetClassFullName()
        {
            int num = 2;
            MethodBase method;
            string result;
            while (true)
            {
                StackFrame stackFrame = new StackFrame(num, false);
                method = stackFrame.GetMethod();
                Type declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    break;
                }
                num++;
                result = declaringType.FullName;
                if (!declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
            }
            result = method.Name;
            return result;
        }
    }
}
