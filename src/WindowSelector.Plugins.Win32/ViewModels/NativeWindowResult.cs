using System.Linq;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.Win32.Models;

namespace WindowSelector.Plugins.Win32.ViewModels
{
    

    public sealed class NativeWindowResult : WindowResult, INativeWindowResult
    {
        private readonly NativeWindowInfo _value;
        private readonly IWindowManager _windowManager;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IRecentWindowRepository _recentWindowRepository;
        private ConfigurationRoot _configuration;

        
        public NativeWindowResult(NativeWindowInfo data, IWindowManager windowManager, IConfigurationProvider configurationProvider, IRecentWindowRepository recentWindowRepository)
        {
            
            _windowManager = windowManager;
            _value = data;
            _configurationProvider = configurationProvider;
            _recentWindowRepository = recentWindowRepository;
            _configuration = configurationProvider.GetConfiguration();
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);
            DisplayText = data.Title;
            Label = data.ProcessName;
            Details = data.Wndclass?.ToString() ?? "";
            Icon = data.Icon;
            IsWhiteListed = data.IsWhiteListed;
            IsBlackListed = data.IsBlackListed;
            HasThumb = true;
            Priority = 2;
        }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            if (args.ChangedProperties.Any(x => x.In("Whitelist", "Blacklist")))
            {
                _configuration = _configurationProvider.GetConfiguration();
                IsWhiteListed = _configuration.Whitelist.Contains(_value.Wndclass);
                IsBlackListed = _configuration.Blacklist.Contains(_value.Wndclass);
            }
        }

        //public override bool IsWhiteListed => _configuration.Whitelist.Contains(_value.Wndclass);

        public override object Value => _value;


        public override void Select(bool centerWindow)
        {

            _windowManager.ShowWindow(_value.hWnd);
            _recentWindowRepository.Push(_value.hWnd);
            if (centerWindow)
            {
                _windowManager.CenterWindow(_value.hWnd);
            }
        }

        public override void Close()
        {
            _windowManager.Close(_value.hWnd);
        }
        public override void Minimize()
        {
            _windowManager.MinimizeWindow(_value.hWnd);
        }


        public override void Whitelist()
        {
            _configuration.Whitelist.Add(_value.Wndclass);
            _configurationProvider.SaveConfiguration(_configuration);
            IsWhiteListed = true;
        }

        public override void Blacklist()
        {
            _configuration.Blacklist.Add(_value.Wndclass);
            _configurationProvider.SaveConfiguration(_configuration);
            IsBlackListed = true;
        }

        public override void UnWhitelist()
        {
            _configuration.Whitelist.Remove(_value.Wndclass);
            _configurationProvider.SaveConfiguration(_configuration);
            IsWhiteListed = false;
        }

        public override void UnBlacklist()
        {
            _configuration.Blacklist.Remove(_value.Wndclass);
            _configurationProvider.SaveConfiguration(_configuration);
            IsBlackListed = false;
        }

        public override string ToString()
        {
            return $"[Native Window] Handle: {_value.hWnd}; Title: {_value.Title}; Class: {_value.Wndclass}";
        }
    }
}