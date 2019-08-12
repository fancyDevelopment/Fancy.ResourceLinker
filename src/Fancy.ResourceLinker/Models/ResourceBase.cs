using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Base class of a resource which can be linked to other resources.
    /// </summary>
    public class ResourceBase : DynamicObject
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
        /// Gets or sets the links of this resource.
        /// </summary>
        /// <value>
        /// The links.
        /// </value>
        [JsonProperty("_links")]
        public Dictionary<string, ResourceLink> Links { get; private set; }

        /// <summary>
        /// Gets or sets the actions of this resource.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [JsonProperty("_actions")]
        public Dictionary<string, ResourceAction> Actions { get; private set; }

        /// <summary>
        /// Gets the hubs.
        /// </summary>
        /// <value>
        /// The hubs.
        /// </value>
        [JsonProperty("_hubs")]
        public Dictionary<string, ResourceHub> Hubs { get; private set; }

        /// <summary>
        /// Gets the dynamic properties.
        /// </summary>
        /// <value>
        /// The dynamic properties.
        /// </value>
        internal Dictionary<string, object> DynamicProperties { get; } = new Dictionary<string, object>();

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
        /// Adds the hub.
        /// </summary>
        /// <param name="rel">The relative.</param>
        /// <param name="hubUrl">The hub URL.</param>
        /// <param name="token">The token.</param>
        public void AddHub(string rel, string hubUrl, string token)
        {
            Hubs.Add(rel, new ResourceHub(hubUrl, token));
        }

        /// <summary>
        /// Tries to get a dynamic member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns>true if the member could be retrieved; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (DynamicProperties.ContainsKey(binder.Name))
            {
                result = DynamicProperties[binder.Name];
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
            if(binder.Name == "_links")
            {
                Links = ((JObject) value).ToObject<Dictionary<string, ResourceLink>>();
                return true;
            }

            if (binder.Name == "_actions")
            {
                Actions = ((JObject)value).ToObject<Dictionary<string, ResourceAction>>();
                return true;
            }

            if (binder.Name == "_hubs")
            {
                Hubs = ((JObject)value).ToObject<Dictionary<string, ResourceHub>>();
                return true;
            }

            DynamicProperties[binder.Name] = value;
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
            return DynamicProperties.Keys;
        }
    }
}