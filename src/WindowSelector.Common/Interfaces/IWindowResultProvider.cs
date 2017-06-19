using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowSelector.Common.ViewModels;

namespace WindowSelector.Common.Interfaces
{
    public interface INativeWindowResult
    {
    }
    // todo: service is more accurate here
    public interface IWindowResultProvider
    {
        Task<IEnumerable<WindowResult>> GetResultsAsync(string keyword, string query, CancellationToken cancellationToken, SearchResultsWriter writer);
        bool SupportsKeyword(string keyword);
    }
}
