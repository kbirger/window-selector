using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WindowSelector.Common.Configuration;
using WindowSelector.Properties;


namespace WindowSelector.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private readonly IConfigurationProvider _configurationProvider;
        private ConfigurationRoot _configuration;
        public HotkeyConfiguration Hotkeys => _configuration.Hotkeys;
        public SettingsWindowViewModel(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            _configuration = configurationProvider.GetConfiguration();
            _configurationProvider.AddConfigurationUpdatedHandler(OnConfigurationUpdated);

        }

        private void OnConfigurationUpdated(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args)
        {
            if (args.ChangedProperties.Any(x => x.Contains("Hotkeys")))
            {
                _configuration = _configurationProvider.GetConfiguration();
                foreach (var changed in args.ChangedProperties)
                {
                    OnPropertyChanged(changed);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            _configurationProvider.SaveConfiguration(_configuration);
        }
    }
}
