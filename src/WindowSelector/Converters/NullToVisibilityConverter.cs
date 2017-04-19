using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WindowSelector.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool negate = (parameter as bool?).GetValueOrDefault();
            return ((value != null) ^ negate) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
