using System.Collections.Generic;
using Fancy.ResourceLinker.Models;
using Microsoft.AspNet.Mvc;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Interface for a service which can be used to link resources.
    /// </summary>
    public interface IResourceLinker
    {
        /// <summary>
        /// Adds links to a resource using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resource">The resource to add links to.</param>
        /// <param name="urlHelper">The URL helper.</param>
        void AddLinks<TResource>(TResource resource, IUrlHelper urlHelper) where TResource : ResourceBase;

        /// <summary>
        /// Adds links to a collection of resources using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resources">The resources to add links to.</param>
        /// <param name="urlHelper">The URL helper.</param>
        void AddLinks<TResource>(IEnumerable<TResource> resources, IUrlHelper urlHelper) where TResource : ResourceBase;
    }
}