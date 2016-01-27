using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Base class of a resource which can be linked to other resources.
    /// </summary>
    public class ResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBase"/> class.
        /// </summary>
        public ResourceBase()
        {
            Links = new Dictionary<string, ResourceLink>();
            Actions = new Dictionary<string, ResourceAction>();
        }

        /// <summary>
        /// Adds the link.
        /// </summary>
        /// <param name="rel">The rel.</param>
        /// <param name="href">The href.</param>
        public void AddLink(string rel, string href)
        {
            Links.Add(rel, new ResourceLink(rel, href));
        }

        /// <summary>
        /// Adds the action.
        /// </summary>
        /// <param name="rel">The relative.</param>
        /// <param name="method">The method.</param>
        /// <param name="href">The URL to the action.</param>
        public void AddAction(string rel, string method, string href)
        {
            Actions.Add(rel, new ResourceAction(rel, method, href));
        }

        /// <summary>
        /// Gets or sets the links of this resource.
        /// </summary>
        /// <value>
        /// The links.
        /// </value>
        [JsonProperty("_links")]
        public Dictionary<string, ResourceLink> Links { get; }

        /// <summary>
        /// Gets or sets the actions of this resource.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [JsonProperty("_actions")]
        public Dictionary<string, ResourceAction> Actions { get; }
    }
}