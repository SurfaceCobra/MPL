using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib
{

    public interface ICachedEnumerable<T> : IEnumerable<T>
    {
        public int CacheSize { get; set; }
    }

    public interface ICachedEnumerator<T> : IEnumerator<T>
    {
        public int CacheSize { get; set; }
        public IEnumerable<T> Peek(int length);

        public bool TryPeek(int length, out IEnumerable<T> output);

        public List<T> CachedValues { get; }

        public void MoveNextSilent();
        public new void Reset();
    }

    public class CachedEnumerable<T> : ICachedEnumerable<T>
    {
        public CachedEnumerable(IEnumerable<T> enumerable) : this(enumerable, 4) { }
        public CachedEnumerable(IEnumerable<T> enumerable, int CacheSize)
        {
            this.enumerable = enumerable;
            this.CacheSize = CacheSize;
        }
        private IEnumerable<T> enumerable;
        public int CacheSize { get; set; }
        ICachedEnumerator<T> GetCachedEnumerator() => new CachedEnumerator<T>(enumerable.GetEnumerator(), CacheSize);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => enumerable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => enumerable.GetEnumerator();
    }

    public class CachedEnumerator<T> : ICachedEnumerator<T>
    {
        public CachedEnumerator(IEnumerator<T> enumerator) : this(enumerator, 4) { }
        public CachedEnumerator(IEnumerator<T> enumerator, int CacheSize)
        {
            this.enumerator = enumerator;
            this.CacheSize = CacheSize;
        }
        private IEnumerator<T> enumerator;
        public int CacheSize { get; set; }
        public List<T> CachedValues { get; init; } = new List<T>();

        public IEnumerable<T> Peek(int length)
        {
            TryPeek(length, out var output);
            return output;
        }
        public bool TryPeek(int length, out IEnumerable<T> output)
        {
            if (Load(length))
            {
                output = this.CachedValues.Take(length);
                return true;
            }
            else
            {
                output = this.CachedValues;
                return false;
            }
        }

        public T Current => CachedValues.First();
        T IEnumerator<T>.Current => this.Current;
        object IEnumerator.Current => this.Current;
        void IDisposable.Dispose()
        {
            enumerator.Dispose();
            enumerator = null;
            this.CachedValues.Clear();
        }
        public bool MoveNext()
        {
            if(CachedValues.Count>0) CachedValues.RemoveAt(0);
            Load(CacheSize);
            return CachedValues.Count != 0;
        }
        bool IEnumerator.MoveNext() => this.MoveNext();

        public void Reset()
        {
            enumerator.Reset();
            this.CachedValues.Clear();
        }

        private bool Load(int size)
        {
            while (CachedValues.Count < size)
                if (enumerator.MoveNext())
                    CachedValues.Add(enumerator.Current);
                else
                    break;
            return !(CachedValues.Count < size);
        }
        public void MoveNextSilent() => this.enumerator.MoveNext();
        public static implicit operator CachedEnumerator<T>(List<T> srclist) => new CachedEnumerator<T>(srclist.GetEnumerator());
        public static implicit operator CachedEnumerator<T>(T[] srcarray) => new CachedEnumerator<T>(srcarray.ToList().GetEnumerator());
    }


    public static class ExtensionMethodICachedEnumerator
    {
        public static ICachedEnumerator<T> GetCachedEnumerator<T>(this IEnumerable<T> enumerable, int CacheSize) => new CachedEnumerator<T>(enumerable.GetEnumerator(), CacheSize);
    }
}
