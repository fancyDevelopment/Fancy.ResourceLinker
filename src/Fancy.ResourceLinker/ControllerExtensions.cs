using System;
using System.Collections.Generic;
using Fancy.ResourceLinker.Models;
using Microsoft.AspNet.Mvc;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Class with extension methods for a controller to link its resources to related resources.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Links a resource to other resources by using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to link.</typeparam>
        /// <param name="controller">The controller responding to a http call.</param>
        /// <param name="resource">The resource to link.</param>
        public static void LinkResource<TResource>(this Controller controller, TResource resource) where TResource : ResourceBase
        {
            IResourceLinker resourceLinker = controller.HttpContext.ApplicationServices.GetService(typeof (IResourceLinker)) as IResourceLinker;

            if (resourceLinker == null)
            {
                throw new InvalidOperationException("No resource linker was found in the ioc container. Register a class implementing the IResourceLinker interface into the ioc container.");    
            }

            resourceLinker.AddLinks(resource);
        }

        /// <summary>
        /// Links resources to other resources by using a link strategy.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to link.</typeparam>
        /// <param name="controller">The controller responding to a http call.</param>
        /// <param name="resources">The resources to link.</param>
        public static void LinkResources<TResource>(this Controller controller, IEnumerable<TResource> resources) where TResource : ResourceBase
        {
            IResourceLinker resourceLinker = controller.HttpContext.ApplicationServices.GetService(typeof(IResourceLinker)) as IResourceLinker;

            if (resourceLinker == null)
            {
                throw new InvalidOperationException("No resource linker was found in the ioc container. Register a class implementing the IResourceLinker interface into the ioc container.");
            }

            resourceLinker.AddLinks(resources);
        }
    }
}