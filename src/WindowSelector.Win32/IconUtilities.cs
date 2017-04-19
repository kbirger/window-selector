using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowSelector.Win32
{
    public static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        public static byte[] ToBytes(this Icon icon)
        {
            if (icon == null) return new byte[0];
            var bmp = icon.ToBitmap();
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
            var imageSource = icon.ToImageSource();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            byte[] bytes = null;
            var bitmapSource = imageSource as InteropBitmap;
            //if (bitmapSource == null) return new byte[0];
            //int height = bitmapSource.PixelHeight;
            //int width = bitmapSource.PixelWidth;
            //int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);

            //byte[] bits = new byte[height * stride];
            //bitmapSource.CopyPixels(bits, stride, 0);
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null) return null;
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            //ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            //    hBitmap,
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());

            //if (!DeleteObject(hBitmap))
            //{
            //    throw new Win32Exception();
            //}

            var iconStream = new MemoryStream();

            using (var bmp = icon.ToBitmap())
            {
                bmp.Save(iconStream, ImageFormat.Png);
            }

            iconStream.Position = 0;
            var decoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.None);
            return decoder.Frames.Last();

            //return wpfBitmap;
        }

        public static ImageSource DataUriToImageSource(this string @string)
        {
            if (@string == null) return null;
            var match = Regex.Match(@string, @"data:image/(?<type>.+?),(?<data>.+)");
            if (match.Success)
            {
                var base64Data = match.Groups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);

                using (var stream = new MemoryStream(binData))
                {
                    Bitmap bitmap = new Bitmap(stream);
                    IntPtr hBitmap = bitmap.GetHbitmap();
                    ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    if (!DeleteObject(hBitmap))
                    {
                        throw new Win32Exception();
                    }

                    return wpfBitmap;

                }
            }
            Debug.Fail("Should be checking this to be a data image first.");
            return null;


        }
    }
}