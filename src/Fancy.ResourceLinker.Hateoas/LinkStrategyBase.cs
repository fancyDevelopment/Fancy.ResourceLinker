using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// Base implementatio of a link strategy.
/// </summary>
/// <typeparam name="T">The type of the resource this strategy links.</typeparam>
/// <seealso cref="Fancy.ResourceLinker.ILinkStrategy" />
public abstract class LinkStrategyBase<T> : ILinkStrategy where T : class
{
    /// <summary>
    /// Determines whether this instance can link the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// True, if this instance can link the specified type; otherwise, false.
    /// </returns>
    public bool CanLinkType(Type type)
    {
        return typeof (T) == type;
    }

    /// <summary>
    /// Links the resource to endpoints.
    /// </summary>
    /// <param name="resource">The resource to link.</param>
    /// <param name="urlHelper">The URL helper.</param>
    public void LinkResource(ResourceBase resource, IUrlHelper urlHelper)
    {
        T? typedResource = resource as T;

        if (typedResource == null)
        {
            throw  new InvalidOperationException("Could not cast resource of type " + resource.GetType().Name + " to type " + typeof(T).Name);
        }

        LinkResourceInternal(typedResource, urlHelper);
    }

    /// <summary>
    /// Links the resource internal.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="urlHelper">The URL helper.</param>
    protected abstract void LinkResourceInternal(T resource, IUrlHelper urlHelper);
}