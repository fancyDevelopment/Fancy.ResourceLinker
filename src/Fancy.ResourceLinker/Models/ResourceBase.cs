using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Base class of a resource which can be linked to other resources.
    /// </summary>
    public class ResourceBase : DynamicObject
    {
        /// <summary>
        /// The dynamic properties of this resource.
        /// </summary>
        Dictionary<string, object> _dynamicProperties = new Dictionary<string, object>();

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
            Links.Add(rel, new ResourceLink(href));
        }

        /// <summary>
        /// Adds the action.
        /// </summary>
        /// <param name="rel">The relative.</param>
        /// <param name="method">The method.</param>
        /// <param name="href">The URL to the action.</param>
        public void AddAction(string rel, string method, string href)
        {
            Actions.Add(rel, new ResourceAction(method, href));
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

        /// <summary>
        /// Tries to get a dynamic member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns>true if the member could be retrieved; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dynamicProperties.ContainsKey(binder.Name))
            {
                result = _dynamicProperties[binder.Name];
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to set a dynamic member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="value">the value to set.</param>
        /// <returns>
        /// true if the member could be set; otherwise, false.
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dynamicProperties[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dynamicProperties.Keys;
        }
    }
}