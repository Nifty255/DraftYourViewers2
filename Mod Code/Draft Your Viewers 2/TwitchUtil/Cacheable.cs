using System;

namespace CodeNifty.DraftYourViewers2.TwitchUtil
{
    public class Cacheable<T>
    {
        public T Data;
        public DateTime CachedAt;
        public double CacheSeconds;

        public Cacheable(T data, double cacheSeconds)
        {
            Data = data;
            CachedAt = DateTime.UtcNow;
            CacheSeconds = cacheSeconds;
        }
    }
}
