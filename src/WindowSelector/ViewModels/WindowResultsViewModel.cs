using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NLog;
using WindowSelector.Common.Interfaces;

using WindowSelector.Common.ViewModels;
using WindowSelector.Properties;

namespace WindowSelector.ViewModels
{
    public class WindowResultsViewModel : INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyList<WindowResult>, IList<WindowResult>
    {
        private readonly IEnumerable<IWindowResultProvider> _providers;
        private readonly Dispatcher _dispatcher;
        private readonly CancellationTokenSource _cts;
        private readonly List<WindowResult> _results = new List<WindowResult>();
        private readonly ILogger _logger;
        private readonly IEnumerable<IRecentWindowRepository> _recentWindowRepositories;
        private bool _searched = false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler FirstResultReceived;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WindowResultsViewModel(IEnumerable<IWindowResultProvider> providers, Dispatcher dispatcher, ILogger logger, IEnumerable<IRecentWindowRepository> recentWindowRepositories)
        {
            _providers = providers;
            _dispatcher = dispatcher;
            _logger = logger;
            _recentWindowRepositories = recentWindowRepositories;
            _cts = new CancellationTokenSource();
            
        }

        public async Task<WindowResultsViewModel> GetDefault()
        {
            int maxResults = 20;
            //var recentWindows  = (await Task.WhenAll(_recentWindowRepositories.Select(p => p.GetRecentWindows(maxResults)))).SelectMany(x => x); ;
            var recentWindows = await Task.Run(() =>
            {
                return Task.WhenAll(_recentWindowRepositories.Select(p => p.GetRecentWindows(maxResults)));
            });

            var v = recentWindows.SelectMany(x => x);
            var recentWindowsTrimmed = v.Take(maxResults);

            WriteResults(recentWindowsTrimmed.ToList());

            return this;
        }

        public Task<WindowResultsViewModel> Search(string search, IEnumerable<string> commands)
        {
            if (_searched)
            {
                throw new InvalidOperationException("Cannot search more than once");
            }
            _searched = true;
            //var rx = @"^([^\s])(?= )";
            //var keyword = Regex.Match(search, rx).Value;
            var keyword = commands.FirstOrDefault();
            var query = search.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult(this);
            }

            var supportedProviders = _providers.Where(p => p.SupportsKeyword(keyword));
            var resultsWriter = new SearchResultsWriter(_dispatcher, WriteResults, _cts.Token);
            var tasks = supportedProviders.Select(p => p.GetResultsAsync(keyword, query, _cts.Token, resultsWriter));
            TaskCompletionSource<WindowResultsViewModel> handle = new TaskCompletionSource<WindowResultsViewModel>();

            Task.WhenAny(tasks).ContinueWith((t) =>
            {
                    handle.SetResult(this);
            });

            //toco: flag when all are complete by using whenall
            return handle.Task;
        }

        private void WriteResults(List<WindowResult> items)
        {
            LogItems(items);
            items.ToList().ForEach(_results.Add);
            Update(items, NotifyCollectionChangedAction.Add);
        }

        private void LogItems(IList<WindowResult> items)
        {
            if (items.Count == 0)
            {
                _logger.Debug("No items returned");
            }
            else
            {
                _logger.Debug("Received items");
                if (_logger.IsTraceEnabled)
                {
                    StringBuilder sb = new StringBuilder("Received items: ");
                    foreach (var item in items)
                    {
                        sb.Append($"\t{item}\n");
                    }

                    _logger.Trace(sb.ToString());
                }
            }
        }

        public bool IsCanceled => _cts.IsCancellationRequested;

        public void Cancel() => _cts.Cancel();

        private void Update(List<WindowResult> items, NotifyCollectionChangedAction action)
        {
            _dispatcher.Invoke(() =>
            {
                OnPropertyChanged("");
                OnCollectionChanged(items, action);
            });

        }

        private void UpdateRemove(List<WindowResult> removedItems, int index)
        {
            _dispatcher.Invoke(() =>
            {
                OnPropertyChanged("Results");
                OnCollectionChanged(removedItems, index, NotifyCollectionChangedAction.Remove);
            });
        }

        private void OnCollectionChanged(IList items, int index, NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, items, index));
        }

        protected virtual void OnCollectionChanged(IList items, NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, items));
        }

        public void Reset()
        {
            //_results.Clear();
            //Update(null, NotifyCollectionChangedAction.Reset);
        }

        protected virtual void OnFirstResultReceived()
        {
            FirstResultReceived?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerator<WindowResult> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(WindowResult item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(WindowResult item)
        {
            return _results.Contains(item);
        }

        public void CopyTo(WindowResult[] array, int arrayIndex)
        {
            _results.CopyTo(array, arrayIndex);
        }

        public bool Remove(WindowResult item)
        {
            int index = _results.IndexOf(item);
            if (index >= 0)
            {
                _results.RemoveAt(index);
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                        new List<WindowResult>() {item}, index));

                return true;
            }
            return false;
        }

        public int Count => _results.Count;
        public bool IsReadOnly => true;

        public int IndexOf(WindowResult item)
        {
            return _results.IndexOf(item);
        }

        public void Insert(int index, WindowResult item)
        {
            _results.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<WindowResult>() { item},  index));

        }

        public void RemoveAt(int index)
        {
            var item = _results[index];
            _results.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<WindowResult>() { item }, index));

        }

        WindowResult IList<WindowResult>.this[int index]
        {
            get { return _results[index]; }
            set { _results[index] = value; }
        }

        public WindowResult this[int index] => _results[index];
    }
}
