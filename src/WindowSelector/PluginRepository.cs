using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowSelector.Common.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using WindowSelector.Common.WPF;

namespace WindowSelector
{
    public class PluginRepository
    {
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;

        private const string pluginPath = "Plugins";
        public DirectoryCatalog Initialize()
        {
            var regBuilder = new RegistrationBuilder();
            regBuilder.ForTypesDerivedFrom<IWindowResultProvider>().Export<IWindowResultProvider>();
            regBuilder.ForTypesDerivedFrom<IRecentWindowRepository>().Export<IRecentWindowRepository>();
            regBuilder.ForTypesDerivedFrom<IWpfViewPlugin>().Export<IWpfViewPlugin>();
            regBuilder.ForTypesDerivedFrom<IPlugin>().Export<IPlugin>();

            var catalog = new AggregateCatalog();
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(Runner).Assembly, regBuilder));
            directoryCatalog = new DirectoryCatalog(pluginPath, regBuilder);
            catalog.Catalogs.Add(directoryCatalog);

            container = new CompositionContainer(catalog);
            //container.ComposeParts(this);;
            container.ComposeExportedValue(container);
            //container.ComposeExportedValue(container);

            //// Get our exports available to the rest of Program.
            //var exports = container.GetExportedValues<IPlugin>();

            return directoryCatalog;
        }
    }
}
