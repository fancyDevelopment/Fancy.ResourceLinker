using Fancy.ResourceLinker.Models;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// Implements the <see cref="IResourceCache"/> interface with an im memory cache which caches the resources 
/// directly in the working memory.
/// </summary>
/// <seealso cref="IResourceCache" />
public class InMemoryResourceCache : IResourceCache
{
    /// <summary>
    /// The cache dictonary which holds all keys and accoarding cache entries.
    /// </summary>
    private Dictionary<string, Tuple<DateTime, object?>> _cache = new Dictionary<string, Tuple<DateTime, object?>>();

    /// <summary>
    /// Writes a resource with the specified key to the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="key">The key to save the resource under.</param>
    /// <param name="resource">The resource instance to save.</param>
    public void Write<TResource>(string key, TResource? resource) where TResource : ResourceBase
    {
        _cache[key] = new Tuple<DateTime, object?>(DateTime.Now, resource);
    }

    /// <summary>
    /// Tries to read a resource from the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="key">The key of the resource to get.</param>
    /// <param name="maxResourceAge">The maximum age of the resource.</param>
    /// <param name="resource">The resource.</param>
    /// <returns>
    /// True if the cache was able to read and provide a valid resource instance; otherwise, false.
    /// </returns>
    public bool TryRead<TResource>(string key, TimeSpan maxResourceAge, out TResource? resource) where TResource : ResourceBase
    {
        resource = null;

        // Check if the key exists within the cache
        if (_cache.ContainsKey(key))
        {
            var cacheEntry = _cache[key];

            // Check if the item within the cahe is not too old
            if (DateTime.Now.Subtract(cacheEntry.Item1) < maxResourceAge)
            {
                resource = (TResource?)cacheEntry.Item2;
                return true;
            }
        }

        return false;
    }
}