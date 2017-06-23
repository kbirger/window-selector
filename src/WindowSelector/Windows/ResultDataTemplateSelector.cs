using System.Windows;
using System.Windows.Controls;
using WindowSelector.Common.Interfaces;

namespace WindowSelector.Windows
{
    public class ResultDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null) return null;

            if (item is INativeWindowResult)
            {
                return element.FindResource("NativeWindowTemplate") as DataTemplate;
            }
            else 
                return element.FindResource("TabTemplate") as DataTemplate;
            
            return base.SelectTemplate(item, container);
        }
    }
}
