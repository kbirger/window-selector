using System;
using System.Windows;
using SystemInterface.IO;
using NLog;

namespace WindowSelector.Common.Configuration
{
    public interface IConfigurationProvider
    {
        ConfigurationRoot GetConfiguration();
        void SaveConfiguration(ConfigurationRoot configurationRoot);
        event EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs> ConfigurationUpdated;
        void AddConfigurationUpdatedHandler(EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs> h);
        void RemoveConfigurationUpdatedHandler(EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs> h);
    }

    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly object _lock = new object();
        private readonly IFile _fileSystem;
        private readonly ILogger _logger;
        private readonly IFileSystemWatcher _fileSystemWatcher;
        private ConfigurationRoot _cached;
        public ConfigurationProvider(IFile fileSystem, ILogger logger, IFileSystemWatcherFactory fileSystemWatcherFactory)
        {
            _fileSystemWatcher = fileSystemWatcherFactory.Create(".", "config.json");
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        private void _fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            _cached = null;
            _logger.Info("configuration file updated. clearing cache.");
        }

        public ConfigurationRoot GetConfiguration()
        {
            lock (_lock)
            {
                if (_cached == null)
                {
                    _cached = ConfigurationRoot.Load(_fileSystem);
                }
                // return copy
                return new ConfigurationRoot(_cached);
            }
        }

        public void SaveConfiguration(ConfigurationRoot configurationRoot)
        {
            lock (_lock)
            {
                var changed = configurationRoot.Save(_fileSystem);
                _cached = configurationRoot;
                _logger.Debug("Saving configuration. Changed properties: {0}", string.Join(", ", changed));
                RaiseConfigurationUpdated(changed);
            }
        }

        private void RaiseConfigurationUpdated(string [] changed)
        {
            ConfigurationUpdated?.Invoke(this, new ConfigurationUpdateEventArgs(changed));
        }

        public event EventHandler<ConfigurationUpdateEventArgs> ConfigurationUpdated;
        public void AddConfigurationUpdatedHandler(EventHandler<ConfigurationUpdateEventArgs> h)
        {
            WeakEventManager<ConfigurationProvider, ConfigurationUpdateEventArgs>.AddHandler(this, "ConfigurationUpdated", h);
        }

        public void RemoveConfigurationUpdatedHandler(EventHandler<ConfigurationUpdateEventArgs> h)
        {
            WeakEventManager<ConfigurationProvider, ConfigurationUpdateEventArgs>.RemoveHandler(this, "ConfigurationUpdated", h);

        }

        public sealed class ConfigurationUpdateEventArgs : EventArgs
        {
            public ConfigurationUpdateEventArgs(string[] changed)
                : base()
            {
                ChangedProperties = changed;
            }

            public string[] ChangedProperties { get; private set; }
        }
    }
}
