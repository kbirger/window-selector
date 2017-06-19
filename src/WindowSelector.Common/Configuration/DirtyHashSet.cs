using System.Collections;
using System.Collections.Generic;

namespace WindowSelector.Common.Configuration
{
    public class DirtyHashSet : ICollection<string>
    {
        private readonly IDirtyHandle _dirtyHandle;
        private readonly HashSet<string> _base;
        public DirtyHashSet(IDirtyHandle dirtyHandle)
            : this(dirtyHandle, new string[0])
        {
        }
        public DirtyHashSet(IDirtyHandle dirtyHandle, IEnumerable<string> original)
        {
            _dirtyHandle = dirtyHandle;
            _base = new HashSet<string>(original);
        }

        public IEnumerator<string> GetEnumerator() => _base.GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_base).GetEnumerator();

        public void Add(string item)
        {
            if (_base.Add(item))
            {
                _dirtyHandle.SetDirty();
            }
        }

        public void Clear()
        {
            if (_base.Count > 0)
            {
                _dirtyHandle.SetDirty();
            }
            _base.Clear();
        }

        public bool Contains(string item) => _base.Contains(item);


        public void CopyTo(string[] array, int arrayIndex) => _base.CopyTo(array, arrayIndex);

        public bool Remove(string item)
        {
            var ret = _base.Remove(item);
            if (ret)
            {
                _dirtyHandle.SetDirty();
            }

            return ret;
        }

        public int Count => _base.Count;

        public bool IsReadOnly => ((ICollection<string>) _base).IsReadOnly;
    }
}
