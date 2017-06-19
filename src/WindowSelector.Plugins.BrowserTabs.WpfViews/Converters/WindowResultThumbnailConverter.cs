using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WindowSelector.Plugins.BrowserTabs.ViewModels;
using WindowSelector.Signalr.Models;
using WindowSelector.Win32;

//using WindowSelector.Signalr.Models;
//using WindowSelector.Signalr.ViewModels;

namespace WindowSelector.Plugins.BrowserTabs.WpfViews.Converters
{
    public class WindowResultThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var windowResult = value as TabWindowResult;
            if (windowResult == null)
            {
                return null;
            }
            if (!windowResult.HasThumb) return null;

            var tabInfo = ((TabInfo)windowResult.Value);
            return tabInfo.Thumb.DataUriToImageSource();
            //    return new TextBlock()
            //{
            //    Foreground = Brushes.White,
            //    FontSize = 120.0,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    Text = "?"
            //};
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
