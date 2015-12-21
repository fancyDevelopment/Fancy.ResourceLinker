using Microsoft.Extensions.DependencyInjection;

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
        public static IServiceCollection AddResourceLinker(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof (IResourceLinker), typeof (ResourceLinker));
            return serviceCollection;
        }
    }
}