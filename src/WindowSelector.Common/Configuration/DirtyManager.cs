using System.Collections.Generic;
using System.Linq;

namespace WindowSelector.Common.Configuration
{
    public class DirtyManager
    {
        public DirtyManager()
            : this(string.Empty)
        {

        }

        public DirtyManager(string root)
        {
            _format = string.IsNullOrEmpty(root) ? "{0}" : root + "[\"{0}\"]";
        }
        private readonly IDictionary<string, bool> _items = new Dictionary<string, bool>();
        private readonly string _format;

        public IEnumerator<KeyValuePair<string, bool>> Items => _items.GetEnumerator();


        public IEnumerable<string> GetDirty()
        {
            return from item in _items
                   where item.Value
                   select string.Format(_format, item.Key);
        }

        public void SetDirty(string property)
        {
            _items[property] = true;
        }
        
        public void ResetDirty()
        {
            _items.Clear();
        }

        public IDirtyHandle GetDirtyHandle(string property)
        {
            return new DirtyHandle(this, property);
        }

        private sealed class DirtyHandle : IDirtyHandle
        {
            private readonly DirtyManager _manager;
            private readonly string _property;

            public DirtyHandle(DirtyManager manager, string property)
            {
                _manager = manager;
                _property = property;
            }

            public void SetDirty()
            {
                _manager.SetDirty(_property);
            }
        }
    }

    public interface IDirtyHandle
    {
        void SetDirty();
    }



}