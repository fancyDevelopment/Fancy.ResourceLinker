using Newtonsoft.Json;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Contains information regarding an action which can be performed onto a resource.
    /// </summary>
    public class ResourceAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAction" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="href">The href.</param>
        public ResourceAction(string method, string href)
        {
            Method = method;
            Href = href;
        }

        /// <summary>
        /// Gets or sets the HTTP method to use for this action.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        [JsonProperty("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the destination URL of the action.
        /// </summary>
        /// <value>
        /// The href.
        /// </value>
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}