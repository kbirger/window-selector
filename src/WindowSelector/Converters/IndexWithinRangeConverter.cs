using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WindowSelector.Converters
{
    public class IndexWithinRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var range = int.Parse(parameter.ToString());
            var item = (ListViewItem)value;
            var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

            if (listView == null)
                return false;

            var index = listView.ItemContainerGenerator.IndexFromContainer(item);
            return (index <= range) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
