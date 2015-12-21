using System.Collections.Generic;
using Fancy.ResourceLinker.Models;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Implements the <see cref="IResourceLinker"/> interface.
    /// </summary>
    public class ResourceLinker : IResourceLinker
    {
        /// <summary>
        /// Adds links to a resource using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resource">The resource to add links to.</param>
        public void AddLinks<TResource>(TResource resource) where TResource : ResourceBase
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Adds links to a collection of resources using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resources">The resources to add links to.</param>
        public void AddLinks<TResource>(IEnumerable<TResource> resources) where TResource : ResourceBase
        {
            throw new System.NotImplementedException();
        }
    }
}