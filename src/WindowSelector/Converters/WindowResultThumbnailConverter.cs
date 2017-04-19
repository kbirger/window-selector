using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WindowSelector.Common;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
//using WindowSelector.Signalr.Models;
//using WindowSelector.Signalr.ViewModels;

namespace WindowSelector.Converters
{
    public class WindowResultThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var windowResult = value as WindowResult;
            if (windowResult == null)
            {
                return null;
            }
            //if (windowResult.HasThumb)
            //{
            //    if (windowResult is INativeWindowResult)
            //    {
            //        var searchData = ((INativeWindowInfo)windowResult.Value);
            //        return new Controls.Thumbnail()
            //        {
            //            Source = searchData.hw
            //        };
            //    }
            //    else if (windowResult is TabWindowResult)
            //    {
            //        var tabInfo = ((TabInfo)windowResult.Value);
            //        return new System.Windows.Controls.Image()
            //        {
            //            Source = tabInfo.Thumb.DataUriToImageSource()
            //        };
            //    }
            //}
            return new TextBlock()
            {
                Foreground = Brushes.White,
                FontSize = 120.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "?"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
