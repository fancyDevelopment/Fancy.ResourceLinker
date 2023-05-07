using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// Controller base class for HATEOAS controllers with helper methods. 
/// </summary>
/// <seealso cref="ControllerBase" />
public class HypermediaController : ControllerBase
{
    /// <summary>
    /// Helper method to return a hypermedia result for the specified content.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>A linked object result.</returns>
    public virtual IActionResult Hypermedia<TResource>(TResource content) where TResource : ResourceBase
    {
        this.LinkResource(content);
        return new ObjectResult(content);
    }

    /// <summary>
    /// Helper method to return a hypermedia result for the specified list of content elements.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>A linked object result.</returns>
    public virtual IActionResult Hypermedia<TResource>(IEnumerable<TResource> content) where TResource : ResourceBase
    {
        this.LinkResources(content);
        return new ObjectResult(content);
    }
}
