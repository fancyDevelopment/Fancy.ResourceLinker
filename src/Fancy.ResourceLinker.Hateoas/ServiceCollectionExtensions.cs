using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// Extension class with helper to easily register resource linker to ioc container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the resource linker hateoas feature to the ioc container.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the resource linker to.</param>
    /// <param name="assemblies">The assemblies to search for <see cref="ILinkStrategy"/> implementations to use to link resources.</param>
    /// <returns>
    /// The service collection.
    /// </returns>
    public static IServiceCollection AddHateoas(this IServiceCollection serviceCollection, params Assembly[] assemblies)
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
    /// Adds the resource linker hateoas feature to the ioc container and automatically searches the calling assembly for 
    /// implementation of <see cref="ILinkStrategy"/> to use to link resources.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the resource linker to.</param>
    /// <returns>
    /// The service collection.
    /// </returns>
    public static IServiceCollection AddHateoas(this IServiceCollection serviceCollection)
    {
        // Get the calling assembly and add the resource linker
        Assembly assembly = Assembly.GetCallingAssembly();
        return AddHateoas(serviceCollection, assembly);
    }
}