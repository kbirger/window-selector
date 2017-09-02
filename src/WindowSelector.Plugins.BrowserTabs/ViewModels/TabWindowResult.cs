using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NLog;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.BrowserTabs.Models;

namespace WindowSelector.Plugins.BrowserTabs.ViewModels
{
    public interface ITabWindowResultFactory
    {
        TabWindowResult Create(TabInfo tabInfo);
    }

    public sealed class TabWindowResultFactory : ITabWindowResultFactory
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IWindowManager _windowManager;
        private readonly Func<ILogger> _loggerFactory;

        [ImportingConstructor]
        public TabWindowResultFactory(IConnectionManager connectionManager, IConfigurationProvider configurationProvider, IWindowManager windowManager, [Import(typeof(Func<ILogger>))]Func<ILogger> loggerFactory)
        {
            _connectionManager = connectionManager;
            _configurationProvider = configurationProvider;
            _windowManager = windowManager;
            _loggerFactory = loggerFactory;
        }

        public TabWindowResult Create(TabInfo tabInfo)
        {
            return new TabWindowResult(tabInfo, _connectionManager, _configurationProvider, _windowManager, _loggerFactory);
        }
    }
    public sealed class TabWindowResult : WindowResult
    {
        private readonly IHubContext<IChromeExtension> _context;
        private readonly TabInfo _value;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IWindowManager _windowManager;
        private readonly ILogger _logger;
        private ConfigurationRoot _configuration;

        public TabWindowResult(TabInfo data, IConnectionManager connectionManager, IConfigurationProvider configurationProvider, IWindowManager windowManager, Func<ILogger> loggerFactory)
        {
            // Assign fields
            _logger = loggerFactory();
            _value = data;
            _configurationProvider = configurationProvider;
            _configuration = configurationProvider.GetConfiguration();
            _context = connectionManager.GetHubContext<ChromeTabsHub, IChromeExtension>();
            _windowManager = windowManager;
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);
            
            // set properties
            DisplayText = data.Title;
            Label = data.Url;
            Details = data.Url;
            try
            {
                Icon = data.FavIconUrl != null
                    ? new AsyncImageSource(Dispatcher.CurrentDispatcher) {Source = new Uri(data.FavIconUrl)}
                    : null;
            }
            catch (NotSupportedException ex)
            {
                Icon = null;
                _logger.Warn(ex);
            }
            IsWhiteListed = false;
            IsBlackListed = false;
            HasThumb = !string.IsNullOrWhiteSpace(data.Thumb);
            Priority = 1;
        }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            if (args.ChangedProperties.Any(x => x.In("Whitelist", "Blacklist")))
            {
                _configuration = _configurationProvider.GetConfiguration();
                IsWhiteListed = _configuration.Whitelist.Contains(_value.Url);
                IsBlackListed = _configuration.Blacklist.Contains(_value.Url);
            }
        }

        public override object Value => _value;

        public override void Select(bool centerWindow)
        {
            _context.Clients.All.SetTab(_value.WindowId, _value.Id, centerWindow: centerWindow);

            //Task.Delay(100).ContinueWith(t =>
            //{
            //    var chrome = Process.GetProcessesByName("chrome").FirstOrDefault(p => p.MainWindowTitle.Contains(_value.Title));
            //    if (chrome != null)
            //    {
            //        _windowManager.ShowWindow(chrome.MainWindowHandle);
            //    }
            //    else
            //    {
            //        _logger.Warn($"Selected tab '{_value.Title}', but was unable to find window matching that title to activate.");
            //    }

                
            //});
        }

        public override void Close()
        {
            _context.Clients.All.CloseTabs(_value.Id);
        }

        public override void Minimize()
        {
            _context.Clients.All.SetTab(_value.WindowId, _value.Id, new WindowUpdateInfo()
            {
                State = WindowState.minimized
            });
        }

        public override void Whitelist()
        {
            _configuration.Whitelist.Add(_value.Url);
            _configurationProvider.SaveConfiguration(_configuration);
            IsWhiteListed = true;
        }

        public override void Blacklist()
        {
            _configuration.Blacklist.Add(_value.Url);
            _configurationProvider.SaveConfiguration(_configuration);
            IsBlackListed = true;
        }

        public override void UnWhitelist()
        {
            _configuration.Whitelist.Remove(_value.Url);
            _configurationProvider.SaveConfiguration(_configuration);
            IsWhiteListed = false;
        }

        public override void UnBlacklist()
        {
            _configuration.Blacklist.Remove(_value.Url);
            _configurationProvider.SaveConfiguration(_configuration);
            IsBlackListed = false;
        }

        public override string ToString()
        {
            return $"[Tab Window] Title: {_value.Title}; Url: {_value.Url}";
        }
    }
}