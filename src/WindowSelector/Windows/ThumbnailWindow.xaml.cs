using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WindowSelector.Common.WPF;

namespace WindowSelector.Windows
{
    /// <summary>
    /// Interaction logic for ThumbnailWindow.xaml
    /// </summary>
    public partial class ThumbnailWindow : Window
    {
        public ThumbnailWindow(IEnumerable<IWpfViewPlugin> viewPlugins)
        {
            InitializeComponent();


            var templates = viewPlugins.Select(v => v.GetViewTemplates())
                .Aggregate(new ResourceDictionary(), (accum, source) =>
                {
                    accum.MergedDictionaries.Add(source);
                    return accum;
                });

            //ThumbContent.Resources = templates;
            Resources.MergedDictionaries.Add(templates);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            MaxWidth = SystemParameters.PrimaryScreenWidth - Left;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 20;
            Top = Math.Max(0, (SystemParameters.MaximizedPrimaryScreenHeight - 20) / 2 - sizeInfo.NewSize.Height / 2);
            //Top = 0;
        }

        private void ThumbContent_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
