using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker
{
    internal class CachingApiProxyController : ApiProxyController
    {
        private readonly IResourceCache _resourceCache;

        public CachingApiProxyController(Dictionary<string, Uri> baseUris, IResourceCache resourceCache) : base(baseUris)
        {
            _resourceCache = resourceCache;
        }

        public async Task<TResource> GetCachedAsync<TResource>(Uri requestUri, TimeSpan maxDataAge) where TResource : class
        {
            string cacheKey = requestUri.ToString();
            TResource data;
            if(_resourceCache.TryRead(cacheKey, maxDataAge, out data))
            {
                return data;
            }
            else
            {
                data = await GetAsync<TResource>(requestUri);
                _resourceCache.Write(cacheKey, data);
                return data;
            }
        }

        public Task<TResource> GetCachedAsync<TResource>(string baseUriKey, string relativeUrl, TimeSpan maxDataAge) where TResource : class
        {
            Uri requestUri = CombineUris(_baseUris[baseUriKey].AbsoluteUri, relativeUrl);
            return GetCachedAsync<TResource>(requestUri, maxDataAge);
        }
    }
}
