using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowSelector.Common.Properties;
using WindowSelector.Win32;

// todo: move this out of common...
namespace WindowSelector.Common
{
    /// <summary>
    /// Allows asynchronous loading of images from Image sources and Icons
    /// </summary>
    public class AsyncImageSource : INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;
        private ImageSource _image;
        private object _source;
        private BitmapDecoder _decoder = null;

        /// <summary>
        /// Gets the value which represents the image to be displayed
        /// </summary>
        public ImageSource Image
        {
            get { return _image; }
            private set
            {
                if (!_dispatcher.CheckAccess())
                {
                    //_dispatcher.InvokeAsync(() => Image = value);
                    throw new InvalidOperationException("Image should only be set from UI thread.");
                }
                else
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Event raised when property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets or sets the value which represents the image source  
        /// </summary>
        public object Source
        {
            get { return _source; }
            set
            {
                if (Source != null)
                    throw new InvalidOperationException("Source can only be set once");

                if (value is Icon)
                {
                    _source = value;
                    SetIconSource(value as Icon);

                }
                else if (value is Uri)
                {
                    _source = value;
                    SetUriSource(value as Uri);
                }
                else if(value != null)
                {
                    throw new NotSupportedException("Only Icon and Uri sources are supported");
                }
            }
        }

        private void SetIconSource(System.Drawing.Icon icon)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.InvokeAsync(() => SetIconSource(icon));
                return;
            }
            Image = icon.ToImageSource();
        }

        private void SetUriSource(Uri uri)
        {
            // This needs to execute in UI thread
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.InvokeAsync(() => SetUriSource(uri));
                return;
            }

            // Handle data images or handle http urls
            if (uri.Scheme == "data")
            {
                Image = GetImageSourceFromDataUri(uri);
            }
            else if (uri.Scheme.In("http", "https", "file"))
            {
                Image = GetImageSourceFromHttpUri(uri);
            }
            else
            {
                throw new NotSupportedException($"Uri scheme unsupported for uri '{uri.OriginalString}'. Supported schemes: data, http, https, file.");
            }
        }

        private ImageSource GetImageSourceFromHttpUri(Uri uri)
        {
            // Set up decoder and return first frame
            _decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default,
                new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
            _decoder.DownloadCompleted += DecoderOnDownloadCompleted;
            _decoder.DownloadFailed += DecoderOnDownloadFailed;
            return _decoder.Frames[0];
        }

        private ImageSource GetImageSourceFromDataUri(Uri uri)
        {
            return uri.OriginalString.DataUriToImageSource();
            //throw new ArgumentException("Invalid Data Uri");
        }


        private void DecoderOnDownloadFailed(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            DisposeDecoder();
        }

        private void DisposeDecoder()
        {
            _decoder.DownloadFailed -= DecoderOnDownloadFailed;
            _decoder.DownloadCompleted -= DecoderOnDownloadCompleted;
            _decoder = null;
            Image = null;
        }

        void DecoderOnDownloadCompleted(object sender, EventArgs e)
        {
            _decoder.DownloadFailed -= DecoderOnDownloadFailed;
            _decoder.DownloadCompleted -= DecoderOnDownloadCompleted;
            try
            {
                Image = _decoder.Frames[0];
            }
            catch
            {
                Image = null;
            }
            _decoder = null;
        }



        public AsyncImageSource(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }



    }
}
