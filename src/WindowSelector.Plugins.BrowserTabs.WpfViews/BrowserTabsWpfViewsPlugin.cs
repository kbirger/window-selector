using System;
using System.Windows;
using WindowSelector.Common.WPF;

namespace WindowSelector.Plugins.BrowserTabs.WpfViews
{
    public class BrowserTabsWpfViewsPlugin : IWpfViewPlugin
    {
        public ResourceDictionary GetViewTemplates()
        {
            var v = new ResourceDictionary()
            {
                Source = new Uri("/WindowSelector.Plugins.BrowserTabs.WpfViews;component/Templates.xaml", UriKind.RelativeOrAbsolute)
            };

            return v;
        }
    }
}
