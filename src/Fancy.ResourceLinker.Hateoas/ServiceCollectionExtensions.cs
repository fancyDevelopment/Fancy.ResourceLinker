using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fancy.ResourceLinker.Hateoas;

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
}