using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowSelector.Common.ViewModels;

namespace WindowSelector.Common.WPF
{
    public interface IWpfViewPlugin
    {
        ResourceDictionary GetViewTemplates();
    }
}
