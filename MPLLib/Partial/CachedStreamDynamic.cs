using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.Partial
{
    public class CachedStreamAsync
    {

    }
    public class CachedStreamFixed// : IPartialReader<byte>, IDisposable
    {
        Stream s;
        
        void asdf()
        {
            
        }
    }

    public class CachedStreamDynamic : IPartialReader<byte>, IDisposable
    {
#if DEBUG
        public int cacheHitSuccess = 0;
        public int cacheHitFailure = 0;
        public double cacheHitRaito => (double)cacheHitSuccess / (cacheHitSuccess+cacheHitFailure);
#endif


        public Stream stream { get; init; }

        List<(long, byte[])> cachedArrayList = new List<(long, byte[])>();
        public int MaxCacheCount;
        public int MaxCacheSize;
        public int CachePrevOffset;

        private readonly (long, byte[]) defaultCache = (0, new byte[0]);

        public CachedStreamDynamic(Stream stream, int MaxCacheCount=4, int MaxCacheSize =4096, int CachePrevOffset = 0)
        {
            if(!stream.CanRead)
            {
                throw new Exception("Cannot read stream");
            }

            this.stream = stream;
            this.MaxCacheCount = MaxCacheCount;
            this.MaxCacheSize = MaxCacheSize;
            this.CachePrevOffset = CachePrevOffset;
        }

        public byte this[long index]
        {
            get
            {
                if (!TryFindCache(index, out var cache))
                {
                    cache = (index, CreateCache(index));
                    cachedArrayList.Add(cache);
                    if (cachedArrayList.Count > MaxCacheCount)
                        cachedArrayList.RemoveAt(0);
#if DEBUG
                    cacheHitFailure++;
                }
                else
                {
                    cacheHitSuccess++;
#endif
                }
                if(cache.array.Length < index - cache.index)
                {
                    throw new CacheNotAccesibleException();
                }
                return cache.array[index - cache.index];
            }
        }
        public byte[] this[Range range] => ReadRange(range.Start.Value, range.End.Value);

        public byte[] ReadRange(long offset, int length)
        {
            byte[] buffer = new byte[length];
            ReadRangeBuffer(buffer, 0, offset, length);
            return buffer;
        }
        public void ReadRangeBuffer(byte[] buffer, int bufferOffset, long offset, int length)
        {
            throw new Exception();
        }

        public bool TryFindCache(long index, out (long index, byte[] array) cache)
        {
            foreach((long offset, byte[] array) cache2 in cachedArrayList)
            {
                if(cache2.offset <= index && index < cache2.offset + cache2.array.Length)
                {
                    cache = cache2;
                    return true;
                }
            }
            cache = defaultCache;
            return false;
        }

        public byte[] CreateCache(long index)
        {
            var endcuts = cachedArrayList.Select(x=>x.Item1).Append(stream.Length);
            var startcuts = cachedArrayList.Select(x => x.Item1+x.Item2.LongLength).Append(0L);

            long endindex = index + MaxCacheSize - CachePrevOffset;
            long startindex = index - CachePrevOffset;
            foreach (var endcut in endcuts)
            {
                if (index < endcut && endcut < endindex)
                {
                    endindex = endcut;
                    break;
                }
            }
            foreach(var startcut in startcuts)
            {
                if(startcut < index && index < startcut)
                {
                    startindex = startcut;
                    break;
                }
            }
            int length = (int)(endindex - startindex);

            byte[] buffer = new byte[length];
            stream.Position = startindex;
            stream.Read(buffer, 0, length);
            return buffer;
        }

        public void Dispose() => stream.Dispose();

        public bool IsReadable(long index)
        {
            if (index < 0) return false;
            if (index > stream.Length) return false;
            return true;
        }
    }
}
