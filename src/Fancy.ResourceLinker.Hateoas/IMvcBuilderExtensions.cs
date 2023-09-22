using Fancy.ResourceLinker.Models.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// Extension class with helper to easily register resource linker to ioc controller service.
/// </summary>
public static class IMvcBuilderExtensions
{
    /// <summary>
    /// Adds the resource linker hateoas feature to the ioc container.
    /// </summary>
    /// <param name="mvcBuilder">The mvc builder to add the resource linker to.</param>
    /// <param name="assemblies">The assemblies to search for <see cref="ILinkStrategy"/> implementations to use to link resources.</param>
    /// <returns>
    /// The service collection.
    /// </returns>
    public static IMvcBuilder AddHateoas(this IMvcBuilder mvcBuilder, params Assembly[] assemblies)
    {
        // Add required services
        mvcBuilder.Services.AddTransient(typeof(IResourceLinker), typeof(ResourceLinker));

        // Add resource converter
        mvcBuilder.AddJsonOptions(options => options.JsonSerializerOptions.AddResourceConverter(true, true));

        // Find all link strategies in provided assemblies and register them
        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> linkStrategies = assembly.GetTypes().Where(x => typeof(ILinkStrategy).IsAssignableFrom(x));

            foreach (Type linkStrategy in linkStrategies)
            {
                mvcBuilder.Services.AddTransient(typeof(ILinkStrategy), linkStrategy);
            }
        }

        return mvcBuilder;
    }

    /// <summary>
    /// Adds the resource linker hateoas feature to the ioc container and automatically searches the calling assembly for 
    /// implementation of <see cref="ILinkStrategy"/> to use to link resources.
    /// </summary>
    /// <param name="mvcBuilder">The mvc builder to add the resource linker to.</param>
    /// <returns>
    /// The service collection.
    /// </returns>
    public static IMvcBuilder AddHateoas(this IMvcBuilder mvcBuilder)
    {
        // Get the calling assembly and add the resource linker
        Assembly assembly = Assembly.GetCallingAssembly();
        return AddHateoas(mvcBuilder, assembly);
    }
}