using MPLLib.ExtensionMethod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace MPLLib.ArrayVariants
{
    internal class IndexArray<T> : ICollection<T>
    {
        public IndexArray(int count)
        {
            this.MaxCount = count;
            values = Enumerable.Repeat<T>(default, count).ToArray();
        }

        T[] values;

        public int Count => _Count;

        public int _Count = 0;

        public int MaxCount { get; init; }

        public bool IsReadOnly => false;

        public void Add(T item) => values[_Count++] = item;

        public void AddRange(IEnumerable<T> item) => item.ForEach(Add);

        public void ClearAfter(int index)
        {
            for (int i = index; i < _Count; i++)
                values[i] = default;
            _Count = index;
        }
        public void Clear()
        {
            for(int i= 0; i < _Count;i++)
                values[i] = default;
            _Count = 0;
        }

        public bool Contains(T item) => ((ICollection<T>)values).Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            for(int i=0; i < _Count;i++)
            {
                array[i+arrayIndex] = values[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(int i=0;i< _Count;i++)
            {
                yield return values[i];
            }
            yield break;
        }

        public bool Remove(T item) => ((ICollection<T>)values).Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
