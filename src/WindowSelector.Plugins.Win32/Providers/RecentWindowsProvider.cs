using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Exceptions;
using WindowSelector.Common.Interfaces;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.Win32;
using WindowSelector.Plugins.Win32.Models;
using WindowSelector.Plugins.Win32.ViewModels;
using WindowSelector.Win32;

//namespace WindowSelector.Plugins.Win32.Providers
//{
//    public class RecentWindowsProvider : IWindowResultProvider
//    {
//        private readonly IRecentWindowRepository _recentWindowRepository;

//        public RecentWindowsProvider(IRecentWindowRepository recentWindowRepository)
//        {
//            _recentWindowRepository = recentWindowRepository;
//        }
//        public Task<IEnumerable<WindowResult>> GetResultsAsync(string keyword, string query, CancellationToken cancellationToken, SearchResultsWriter writer)
//        {
//            return _recentWindowRepository.GetRecentWindows(20);
//        }

//        public bool SupportsKeyword(string keyword)
//        {
//            return false;
//        }
//    }

    [Export(typeof(IRecentWindowRepository))]
public class RecentWindowRepository : IRecentWindowRepository
{
    private readonly IWindowManager _windowManager;
    private readonly Func<INativeWindowInfo, NativeWindowResult> _nativeWindowResultFactoryFunc;
    private IntPtr _lastHandle = IntPtr.Zero;
    public RecentWindowRepository(IWindowManager windowManager, IConfigurationProvider configurationProvider/*, Func<NativeWindowInfo, NativeWindowResult> nativeWindowResultFactoryFunc*/)
    {
        _windowManager = windowManager;
        _nativeWindowResultFactoryFunc = (data) => new NativeWindowResult((NativeWindowInfo) data, windowManager, configurationProvider, this);
    }

    private sealed class Record
    {
        private readonly object _lock = new object();

        public Record()
        {
            Count = 1;
            LastModified = DateTime.Now;
        }

        private Record(int count)
        {
            Count = count;
            LastModified = DateTime.Now;
        }
        public Record Touch()
        {
            return new Record(Count + 1);
        }

        public int Count { get; private set; }
        public DateTime LastModified { get; private set; }
    }
    private readonly ConcurrentDictionary<IntPtr, Record> _store = new ConcurrentDictionary<IntPtr, Record>();

    public Task<IEnumerable<WindowResult>> GetRecentWindows(int i)
    {
        var orderedWindows = _store
            .OrderByDescending((p) => p.Value.LastModified)
            .Skip(1)
            .Select(p => p.Key);
        List<WindowResult> results = new List<WindowResult>(i);
        foreach (var handle in orderedWindows)
        {
            if (results.Count == i)
            {
                break;
            }

            try
            {
                INativeWindowInfo windowInfo;
                if (_windowManager.TryGetWindowInfo(handle, out windowInfo))
                {
                    if (windowInfo.ProcessName != Process.GetCurrentProcess().ProcessName)
                    {
                        var result = _nativeWindowResultFactoryFunc(windowInfo);
                        results.Add(result);
                    }
                }
                else
                {
                    Record temp;
                    _store.TryRemove(handle, out temp);
                }
            }
            catch (WindowNotFoundException)
            {
                // skip this
            }
        }
        return Task.FromResult((IEnumerable<WindowResult>)results);
    }

    /// <summary>
    /// Pushes a window handle to the repository store, but only if it is different than the last handle to be pushed using PushNew
    /// </summary>
    /// <param name="handle"></param>
    public void PushNew(IntPtr handle)
    {
        if (handle != _lastHandle)
        {
            Push(handle);
            _lastHandle = handle;
        }
    }

    /// <summary>
    /// Pushes a window handle to the repository store
    /// </summary>
    /// <param name="handle"></param>
    public void Push(IntPtr handle)
    {
        _store.AddOrUpdate(handle, new Record(), (i, record) => record.Touch());
        if (_store.Count > 100)
        {
            var staleItems = _store
                .OrderBy((p) => p.Value.LastModified)
                .Select(p => p.Key)
                .Take(_store.Count - 50);

            Record temp;
            foreach (var item in staleItems)
            {
                _store.TryRemove(item, out temp);
            }
        }
    }

}



//}
