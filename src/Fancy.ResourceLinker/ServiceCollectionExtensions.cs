using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Extension class with helper to easily register resource linker to ioc container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the resource linker to the ioc container.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the resource linker to.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddResourceLinker(this IServiceCollection serviceCollection, params Assembly[] assemblies)
        {
            serviceCollection.AddTransient(typeof(IResourceLinker), typeof(ResourceLinker));

            // Find all link strategies in provided assemblies and register them
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> linkStrategies = assembly.GetTypes().Where(x => typeof(ILinkStrategy).IsAssignableFrom(x));

                foreach (Type linkStrategy in linkStrategies)
                {
                    serviceCollection.AddTransient(typeof(ILinkStrategy), linkStrategy);
                }
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds a resource cache instance to the IoC container.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="cacheInstance">The cache instance or null if you would like to use the default implementation.</param>
        /// <returns>
        /// The service collection.
        /// </returns>
        /// <remarks>
        /// The <see cref="InMemoryResourceCache" /> is registered as implementation by default.
        /// </remarks>
        public static IServiceCollection AddResourceCache(this IServiceCollection serviceCollection, IResourceCache cacheInstance = null)
        {
            if(cacheInstance != null)
            {
                serviceCollection.AddSingleton<IResourceCache>(cacheInstance);
            }
            else
            {
                serviceCollection.AddSingleton<IResourceCache, InMemoryResourceCache>();
            }

            return serviceCollection;
        }
    }
}