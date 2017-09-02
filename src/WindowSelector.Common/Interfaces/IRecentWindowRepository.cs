using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowSelector.Common.ViewModels;

namespace WindowSelector.Common.Interfaces
{
    // todo: rename this. should be a service
    public interface IRecentWindowRepository
    {
        // todo: make this a resultwriter interface
        Task<IEnumerable<WindowResult>> GetRecentWindows(int maxWindows);

        // todo: hide this because intpr is an implementation detail and os specific
        void Push(IntPtr windowResult);

        void PushNew(IntPtr handle);
    }
}