using MPLLib.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.Partial
{
    public class CachedSequencialBufferDynamic<T> : IPartialReader<T>
    {
        private IEnumerator<T[]> e { get; init; }
        public int MaxCacheCount { get; init; }
        private List<T[]> cachedBufferList { get; init; } = new List<T[]>();
        long bufferStartIndex = 0;
        private readonly T falseResult = default;

        public CachedSequencialBufferDynamic(IEnumerable<T[]> bufferGenerator, int MaxCacheCount)
        {
            this.e = bufferGenerator.GetEnumerator();
            this.MaxCacheCount = MaxCacheCount;
        }
        public T this[long index]
        {
            get
            {
                if(TryReadIndex(index, out T result))
                {
                    return result;
                }
                else
                {
                    throw new CacheNotAccesibleException();
                }
            }
        }
        public bool TryReadIndex(long index, out T result)
        {
            if(index < bufferStartIndex)
            {
                throw new PrevCacheNotAccesibleException();
            }
            long addedCachedBufferLength = bufferStartIndex;
            foreach(T[] cachedBuffer in cachedBufferList)
            {
                
                if(addedCachedBufferLength <= index && index < addedCachedBufferLength + cachedBuffer.LongLength)
                {
                    result = cachedBuffer[index-addedCachedBufferLength];
                    return true;
                }
                addedCachedBufferLength += cachedBuffer.Length;
            }
            if(TryCreateCacheUntilIndex(index, out result))
            {
                return true;
            }
            return false;
        }
        public bool TryCreateCacheUntilIndex(long index, out T result)
        {
            while (true)
            {
                if(e.TryMoveNext(out T[] buffer))
                {
                    cachedBufferList.Add(buffer);
                    if (cachedBufferList.Count > MaxCacheCount)
                    {
                        bufferStartIndex += cachedBufferList[0].Length;
                        cachedBufferList.RemoveAt(0);
                    }
                    if (TryReadIndex(index, out result))
                        return true;
                }
                else
                {
                    result = this.falseResult;
                    return false;
                }
            }
        }

        public void Dispose()
        {
            //donothing
        }

        public bool IsReadable(long index)
        {
            try
            {
                return TryReadIndex(index, out _);
            }
            catch(CacheNotAccesibleException)
            {
                return false;
            }
        }
    }
}
