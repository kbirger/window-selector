using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowSelector.Common
{
    public interface INativeWindowInfo
    {
         string ProcessName { get; set; }
         bool IsWhiteListed { get; set; }
         bool IsBlackListed { get; set; }
         int PID { get; set; }

        string Title { get; set; }

    }
}
