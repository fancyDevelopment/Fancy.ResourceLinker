using Fancy.ResourceLinker.Models;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// Interface for a service which can be used to cache resources.
/// </summary>
public interface IResourceCache
{
    /// <summary>
    /// Writes a resource with the specified key to the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="key">The key to save the resource under.</param>
    /// <param name="resource">The resource instance to save.</param>
    void Write<TResource>(string key, TResource resource) where TResource : class;

    /// <summary>
    /// Tries to read a resource from the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="key">The key of the resource to get.</param>
    /// <param name="maxResourceAge">The maximum age of the resource.</param>
    /// <param name="resource">The resource.</param>
    /// <returns>True if the cache was able to read and provide a valid resource instance; otherwise, false.</returns>
    bool TryRead<TResource>(string key, TimeSpan maxResourceAge, out TResource? resource) where TResource : class;
}
