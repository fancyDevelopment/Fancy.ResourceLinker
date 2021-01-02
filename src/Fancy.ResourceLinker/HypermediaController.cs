using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Fancy.ResourceLinker
{
    public class HypermediaController : ControllerBase
    {
        public virtual IActionResult Hypermedia<TResource>(TResource content) where TResource : ResourceBase
        {
            this.LinkResource(content);
            return new ObjectResult(content);
        }

        public virtual IActionResult Hypermedia<TResource>(IEnumerable<TResource> content) where TResource : ResourceBase
        {
            this.LinkResources(content);
            return new ObjectResult(content);
        }
    }
}
