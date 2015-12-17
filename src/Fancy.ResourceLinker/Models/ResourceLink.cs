using Newtonsoft.Json;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Contains information regarding a link which can be performed onto a resource.
    /// </summary>
    public class ResourceLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLink"/> class.
        /// </summary>
        /// <param name="rel">The relative.</param>
        /// <param name="href">The href.</param>
        public ResourceLink(string rel, string href)
        {
            Rel = rel;
            Href = href;
        }

        /// <summary>
        /// Gets or sets the relation of the link.
        /// </summary>
        /// <value>
        /// The relative.
        /// </value>
        [JsonProperty("rel")]
        public string Rel { get; set; }

        /// <summary>
        /// Gets or sets the destination URL of the link.
        /// </summary>
        /// <value>
        /// The href.
        /// </value>
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}