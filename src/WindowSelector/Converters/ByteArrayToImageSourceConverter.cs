using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowSelector.Converters
{
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] bytes = value as byte[];
            if (bytes == null)
            {
                return value;
                // todo: some default img? blank maybe? anything but not null
            }

            //BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(bytes);
            BitmapDecoder d = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.Default);

            ImageSource imgSrc = d.Frames.Last();

            return imgSrc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
