using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.Partial
{
    public class PartialArray<T> : IPartialReader<T>
    {
        T[] values;
        public PartialArray(T[] values)
        {
            this.values = values;
        }
        public PartialArray(IEnumerable<T> values)
        {
            this.values = values.ToArray();
        }

        public T this[long index] => values[index];

        public void Dispose() => values = null;

        public bool IsReadable(long index)
        {
            if (index < 0 || index >= values.Length) return false;
            return true;
        }

        public static implicit operator PartialArray<T>(T[] array) {  return new PartialArray<T>(array); }
    }
}
