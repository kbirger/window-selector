using System.Windows;

namespace WindowSelector.Common.WPF
{
    public interface IWpfViewPlugin
    {
        ResourceDictionary GetViewTemplates();
    }
}
