namespace MPLLib.Partial
{
    /// <summary>
    /// Cached array but should read in sequance. might throw exception when index is smaller than previous cache block.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPartialReader<T> : IDisposable
    {
        public T this[long index] { get; }
        public bool IsReadable(long index);
    }

    public interface IPartialWriter<T> : IDisposable
    {
        public T this[long index] { set; }
        public bool IsWriteable(long index);
    }

    public interface IPartialArray<T> : IPartialReader<T>, IPartialWriter<T>, IDisposable
    {

    }

    public class PrevCacheNotAccesibleException : CacheNotAccesibleException
    {

    }
    public class CacheNotAccesibleException : Exception
    {

    }
}
