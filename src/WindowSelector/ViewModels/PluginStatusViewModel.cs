using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WindowSelector.Common;
using WindowSelector.Common.Interfaces;

namespace WindowSelector.ViewModels
{
    /// <summary>
    /// Reports plugin status to the UI
    /// </summary>
    public class PluginStatusViewModel : INotifyPropertyChanged
    {
        private readonly IEnumerable<IPlugin> _plugins;

        public PluginStatusViewModel(IEnumerable<IPlugin> plugins)
        {
            _plugins = plugins;
            foreach (var plugin in plugins)
            {
                plugin.PluginStatusChanged += OnPluginStatusChanged;
            }
        }

        /// <summary>
        /// Gets the aggregated status of all plugins
        /// </summary>
        public IEnumerable<PluginStatusInfo> PluginStatuses
        {
            get { return _plugins.SelectMany(p => p.GetPluginStatus()); }
        }

        private void OnPluginStatusChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("PluginStatuses");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
