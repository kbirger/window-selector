using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowSelector.Common.WPF;

namespace WindowSelector.Plugins.Win32.WpfViews
{
    [Export(typeof(IWpfViewPlugin))]
    public class WpfViewPlugin : IWpfViewPlugin
    {
        public ResourceDictionary GetViewTemplates()
        {
            var v = new ResourceDictionary()
            {
                Source = new Uri("/WindowSelector.Plugins.Win32.WpfViews;component/Templates.xaml", UriKind.RelativeOrAbsolute)
            };

            return v;
        }
    }
}
