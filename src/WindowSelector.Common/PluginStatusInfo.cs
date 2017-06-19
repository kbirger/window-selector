using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowSelector.Common
{
    /// <summary>
    /// Reports the status of a plugin component
    /// </summary>
    public class PluginStatusInfo
    {

        public  PluginStatusInfo(string item, bool isActive)
        {
            Item = item;
            IsActive = isActive;
        }

        /// <summary>
        /// Gets the value indicating the name of the item.
        /// </summary>
        public string Item { get; private set; }

        /// <summary>
        /// Gets the value indicating whether this item is currently active.
        /// </summary>
        public bool IsActive { get; private set; }


    }
}
