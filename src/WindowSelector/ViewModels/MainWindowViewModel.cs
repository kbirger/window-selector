using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using WindowSelector.Commands;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.ViewModels;
//using WindowSelector.Signalr;

namespace WindowSelector.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public PluginStatusViewModel PluginStatuses { get; private set; }

        public sealed class WindowEventArgs : EventArgs
        {
            public WindowResult WindowResult { get; private set; }

            public WindowEventArgs(WindowResult windowResult)
            {
                WindowResult = windowResult;
            }
        }

        private WindowResultsViewModel _results;
        private CancellationTokenSource _currentTasks;
        //private readonly HubConnectionTrackerService _connectionTrackerService;
        private bool _chromeConnected = false;
        private WindowResult _selectedItem;
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly Func<WindowResultsViewModel> _windowResultViewFactory;
        private HotkeySetting _minimizeKey;
        private HotkeySetting _closeKey;

        public MainWindowViewModel(/*HubConnectionTrackerService connectionTrackerService, */ILogger logger, IConfigurationProvider configurationProvider, Func<WindowResultsViewModel> windowResultViewFactory, PluginStatusViewModel pluginStatuses)
        {
            PluginStatuses = pluginStatuses;
            _results = windowResultViewFactory();
            _configurationProvider = configurationProvider;
            _windowResultViewFactory = windowResultViewFactory;
            //_connectionTrackerService = connectionTrackerService;
            _logger = logger;
            //_connectionTrackerService.CountChanged += ConnectionTrackerServiceOnCountChanged;
            //ChromeConnected = IsHubConnected(ChromeTabsHub.HubName);
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);

            InitializeCommands();

            var config = _configurationProvider.GetConfiguration();
            MinimizeKey = config.Hotkeys["minimize_key"];
            CloseKey = config.Hotkeys["close_key"];
        }

        private void InitializeCommands()
        {
            HighlightCommand = new RelayCommand((o) =>
            {
                IsHighlighting = true;
            });
            ItemDownCommand = new RelayCommand((o) =>
            {
                if (Results.Count == 0) return;
                int index = Results.IndexOf(SelectedItem);
                SelectedItem = Results[(index + 1)%Results.Count];
            });

            ItemUpCommand = new RelayCommand((o) =>
            {
                if (Results.Count == 0) return;
                int index = Results.IndexOf(SelectedItem);
                SelectedItem = Results[(--index) >= 0 ? index : Results.Count - 1];
            });

            ItemSelectCommand = new RelayCommand((o) =>
            {
                if (SelectedItem == null) return;
                _logger.Info($"Selecting window {SelectedItem}");
                SelectedItem.Select((bool) o);
            });

            ItemMinimizeCommand = new RelayCommand((o) =>
            {
                if (SelectedItem == null) return;
                _logger.Info($"Minimizing window {SelectedItem}");
                SelectedItem.Minimize();
            });

            ItemCloseCommand = new RelayCommand((o) =>
            {
                if (SelectedItem == null) return;
                _logger.Info($"Closing window {SelectedItem}");
                OnWindowClosing(SelectedItem);
                SelectedItem.Select(false);
                _logger.Debug("Bringing window to foreground in case save dialog appears");
                SelectedItem.Close();
                _logger.Debug("WindowResult close finished");
                OnWindowClosed(SelectedItem);
                var index = Results.IndexOf(SelectedItem);
                Results.RemoveAt(index);
                
            });
        }

        private bool _isHighlighting = false;

        public bool IsHighlighting
        {
            get { return _isHighlighting; }
            set { _isHighlighting = value; OnPropertyChanged(); }
            
        }

        public ICommand HighlightCommand { get; set; }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            var newconfig = _configurationProvider.GetConfiguration();
            foreach (var propertyName in args.ChangedProperties)
            {
                switch (propertyName)
                {
                    case "Hotkeys[\"minimize_key\"]":
                        MinimizeKey = newconfig.Hotkeys["minimize_key"];
                        break;
                    case "Hotkeys[\"close_key\"]":
                        CloseKey = newconfig.Hotkeys["close_key"];
                        break;
                    default:
                        break;
                }
            }
        }


        public event EventHandler<WindowEventArgs> WindowClosing;
        public event EventHandler<WindowEventArgs> WindowClosed;
        public event EventHandler<WindowEventArgs> WindowSelected; 
        
        public HotkeySetting MinimizeKey
        {
            get
            {
                return _minimizeKey;
            }
            private set
            {
                _minimizeKey = value;
                OnPropertyChanged();
            }
        }

        public HotkeySetting CloseKey {
            get
            {
                return _closeKey;
            }
            set
            {
                _closeKey = value;
                OnPropertyChanged();
            }
        }

        public ICommand ItemCloseCommand { get; private set; }

        public ICommand ItemMinimizeCommand { get; private set; }

        public ICommand ItemSelectCommand { get; private set; }

        public ICommand ItemUpCommand { get; private set; }

        public ICommand ItemDownCommand { get; private set; }

        public WindowResult SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        //private void ConnectionTrackerServiceOnCountChanged(object sender, HubConnectionTrackerService.ConnectionCountChangedEventArgs connectionCountChangedEventArgs)
        //{
        //    // todo: check hub name and stuff
        //    ChromeConnected = IsHubConnected(ChromeTabsHub.HubName);
        //}

        //private bool IsHubConnected(string hubName)
        //{
        //    return _connectionTrackerService.GetConnectionCount(hubName) > 0;
        //}

        public bool ChromeConnected
        {
            get { return _chromeConnected; }
            set
            {
                _chromeConnected = value;
                OnPropertyChanged();
            }
        }

        public async Task Default()
        {
            Reset();
            Results = await Results.GetDefault();
        }
        public void Reset()
        {
            Results = _windowResultViewFactory();
            //Update(null, NotifyCollectionChangedAction.Reset);
        }

        public async Task Search(string search, bool showAll)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                Results = _windowResultViewFactory();
                return;
            }

            // wait for new results to come in before updating
            // this is to prevent extra flicker
            var newResults = await _windowResultViewFactory().Search(search);
            // once the new results are in, cancel old ones to conserve resources
            Results?.Cancel();
            Results = newResults;

            SelectedItem = SelectedItem ?? Results.FirstOrDefault();

        }



        public WindowResultsViewModel Results
        {
            get { return _results; }
            set
            {
                if (_results != null)
                {
                    _results.CollectionChanged -= Results_CollectionChanged;
                }
                _results = value;
                _results.CollectionChanged += Results_CollectionChanged;
                OnPropertyChanged();
            }
        }

        private void Results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedItem = SelectedItem ?? _results.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnWindowClosing(WindowResult result)
        {
            WindowClosing?.Invoke(this, new WindowEventArgs(result));
        }

        protected virtual void OnWindowClosed(WindowResult result)
        {
            WindowClosed?.Invoke(this, new WindowEventArgs(result));
        }

        protected virtual void OnWindowSelected(WindowResult result)
        {
            WindowSelected?.Invoke(this, new WindowEventArgs(result));
        }
    }
}
