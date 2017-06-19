using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace WindowSelector.Common.ViewModels
{
    public class SearchResultsWriter
    {
        private readonly Dispatcher _dispatcher;
        private readonly Action<List<WindowResult>> _writeResults;
        private readonly object _lock = new object();
        private readonly CancellationToken _cancellationToken;

        public SearchResultsWriter(Dispatcher dispatcher, Action<List<WindowResult>> writeResults, CancellationToken cancellationToken)
        {
            _dispatcher = dispatcher;
            _writeResults = writeResults;
            _cancellationToken = cancellationToken;
        }


        public void AddResults(List<WindowResult> results)
        {
            if(!_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(() => AddResults(results));
                return;
            }

            if(!_cancellationToken.IsCancellationRequested)
            {
                _writeResults.Invoke(results);
            }
        }



    }
}