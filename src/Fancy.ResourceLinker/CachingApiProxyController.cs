using Fancy.ResourceLinker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Controller base class for API proxy controllers to to forward requests to microservices
    /// which provides additionally caching mechanism for resources.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="ServiceCollectionExtensions.AddResourceCache(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/> method
    /// to register a cache implementation to IoC container at program startup.
    /// </remarks>
    /// <seealso cref="Fancy.ResourceLinker.ApiProxyController" />
    public class CachingApiProxyController : ApiProxyController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachingApiProxyController"/> class.
        /// </summary>
        /// <param name="baseUris">The base uris of the microservices each mapped to a unique key.</param>
        public CachingApiProxyController(Dictionary<string, Uri> baseUris) : base(baseUris)
        {
        }

        /// <summary>
        /// Gets data from a url and deserializes it into a given type. If data is available in the cache and not older
        /// as the age specified the data is returned from the chache, if not data is retrieved from the origin and written to the cache.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="requestUri">The uri of the data to get.</param>
        /// <param name="maxResourceAge">The maximum age of the resource which is acceptable.</param>
        /// <returns>
        /// The result deserialized into the specified resource type.
        /// </returns>
        public async Task<TResource> GetCachedAsync<TResource>(Uri requestUri, TimeSpan maxResourceAge) where TResource : ResourceBase
        {
            IResourceCache resourceCache = (IResourceCache)HttpContext.RequestServices.GetService(typeof(IResourceCache));
            string cacheKey = requestUri.ToString();

            // Check if we can get the resource from cache
            TResource data;
            if (resourceCache.TryRead(cacheKey, maxResourceAge, out data))
            {
                return data;
            }
            else
            {
                // Get resource from origin and write it to the cache
                data = await GetAsync<TResource>(requestUri);
                resourceCache.Write(cacheKey, data);
                return data;
            }
        }

        /// <summary>
        /// Get data from a microservice specified by its key of a provided endpoint and deserializes it into a given type.
        /// If data is available in the cache and not older as the age specified the data is returned from the chache, if not
        /// data is retrieved from the origin and written to the cache.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="baseUriKey">The key of the microservice url to use.</param>
        /// <param name="relativeUrl">The relative url to the endpoint.</param>
        /// <param name="maxResourceAge">The maximum age of the resource which is acceptable.</param>
        /// <returns>
        /// The result deserialized into the specified resource type.
        /// </returns>
        public Task<TResource> GetCachedAsync<TResource>(string baseUriKey, string relativeUrl, TimeSpan maxResourceAge) where TResource : ResourceBase
        {
            Uri requestUri = CombineUris(_baseUris[baseUriKey].AbsoluteUri, relativeUrl);
            return GetCachedAsync<TResource>(requestUri, maxResourceAge);
        }
    }
}