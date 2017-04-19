using System;
using System.Drawing;
using System.IO;
using System.Windows;

namespace WindowSelector.Tests
{
    internal static class ResourceUtil
    {
        public static Icon GetIcon()
        {
            Icon icon;
            using (var imageStream = GetStream("TestIco.ico"))
            {
                icon = new Icon(imageStream);
            }
            return icon;
        }

        public static Stream GetStream(string resourceName)
        {
            if (!UriParser.IsKnownScheme("pack")) new System.Windows.Application();
            var imgUri = new Uri("pack://application:,,,/WindowSelector.Tests;component/" + resourceName);
            var streamInfo = Application.GetResourceStream(imgUri);
            var stream = streamInfo.Stream;
            return stream;
        }

        public static void LoadTestIconToPath(string imagePath)
        {
            using (var imageStream = GetStream("TestIco.ico"))
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Write))
            {
                imageStream.CopyTo(fs);
            }
        }
    }
}