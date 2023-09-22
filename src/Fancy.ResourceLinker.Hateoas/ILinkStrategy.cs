using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// Interface for a link strategy.
/// </summary>
/// <remarks>
/// A link strategy is responsible to link a specific type of a data transfer object or view model.
/// </remarks>
public interface ILinkStrategy
{
    /// <summary>
    /// Determines whether this instance can link the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>True, if this instance can link the specified type; otherwise, false.</returns>
    bool CanLinkType(Type type);

    /// <summary>
    /// Links the resource to endpoints.
    /// </summary>
    /// <param name="resource">The resource to link.</param>
    /// <param name="urlHelper">The URL helper.</param>
    /// <exception cref="System.ArgumentException">Resource as wrong type;resource</exception>
    void LinkResource(IResource resource, IUrlHelper urlHelper);
}
