using System;
using System.Collections.Generic;
using System.Reflection;
using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Implements the <see cref="IResourceLinker"/> interface.
    /// </summary>
    public class ResourceLinker : IResourceLinker
    {
        /// <summary>
        /// The link strategies.
        /// </summary>
        private IEnumerable<ILinkStrategy> _linkStrategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLinker"/> class.
        /// </summary>
        /// <param name="linkStrategies">The link strategies to use in this instance.</param>
        public ResourceLinker(IEnumerable<ILinkStrategy> linkStrategies)
        {
            _linkStrategies = linkStrategies;
        }

        /// <summary>
        /// Adds links to a resource using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resource">The resource to add links to.</param>
        /// <param name="urlHelper">The URL helper.</param>
        public void AddLinks<TResource>(TResource resource, IUrlHelper urlHelper) where TResource : ResourceBase
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            LinkObject(resource, urlHelper);

            // Iterate through all properties and link child objects which are also resources
            foreach (PropertyInfo propertyInfo in resource.GetType().GetProperties())
            {
                object propertyValue = propertyInfo.GetValue(resource);

                if (propertyInfo.PropertyType.GetTypeInfo().IsSubclassOf(typeof(ResourceBase)))
                {
                    // Type is a resource -> cast the object of the property and link it
                    AddLinks((ResourceBase)propertyValue, urlHelper);
                }
                else if (propertyValue is IEnumerable<ResourceBase>)
                {
                    // Type is a collection of resources -> cast object and link it
                    IEnumerable<ResourceBase> subResources = propertyValue as IEnumerable<ResourceBase>;
                    AddLinks(subResources, urlHelper);
                }
            }
        }

        /// <summary>
        /// Adds links to a collection of resources using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to add links to.</typeparam>
        /// <param name="resources">The resources to add links to.</param>
        /// <param name="urlHelper">The URL helper.</param>
        public void AddLinks<TResource>(IEnumerable<TResource> resources, IUrlHelper urlHelper) where TResource : ResourceBase
        {
            foreach (ResourceBase resource in resources)
            {
                AddLinks(resource, urlHelper);
            }
        }

        /// <summary>
        /// Links the object.
        /// </summary>
        /// <param name="resource">The resource to link.</param>
        /// <param name="urlHelper">The URL helper to use to build the links.</param>
        private void LinkObject(ResourceBase resource, IUrlHelper urlHelper)
        {
            ILinkStrategy linkStrategy = _linkStrategies.FirstOrDefault(ls => ls.CanLinkType(resource.GetType()));

            if (linkStrategy != null)
            {
                linkStrategy.LinkResource(resource, urlHelper);
            }
        }
    }
}