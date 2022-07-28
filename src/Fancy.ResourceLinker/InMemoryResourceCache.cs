using System;
using System.Collections.Generic;

namespace Fancy.ResourceLinker
{
    public class InMemoryResourceCache : IResourceCache
    {
        private Dictionary<string, Tuple<DateTime, object>> _cache = new Dictionary<string, Tuple<DateTime, object>>();

        public void Write<TResource>(string key, TResource data) where TResource : class
        {
            _cache[key] = new Tuple<DateTime, object>(DateTime.Now, data);
        }

        public bool TryRead<TResource>(string key, TimeSpan maxDataAge, out TResource data) where TResource : class
        {
            data = null;
            if(_cache.ContainsKey(key))
            {
                var cacheEntry = _cache[key];   
                if(DateTime.Now.Subtract(cacheEntry.Item1) < maxDataAge)
                {
                    data = (TResource)cacheEntry.Item2;
                    return true;
                }
            }

            return false;
        }

        
    }
}
