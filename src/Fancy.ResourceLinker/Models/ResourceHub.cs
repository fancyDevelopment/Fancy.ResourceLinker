using Newtonsoft.Json;

namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Contains information regarding an hub which can be used to implement server to client messaging onto a resource.
    /// </summary>
    public class ResourceHub
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceHub" /> class.
        /// </summary>
        /// <param name="hubUrl">The hub URL.</param>
        /// <param name="token">The token.</param>
        public ResourceHub(string hubUrl, string token)
        {
            HubUrl = hubUrl;
            Token = token;
        }

        /// <summary>
        ///   Gets or sets the hub URL.
        /// </summary>
        /// <value>
        ///   The hub URL.
        /// </value>
        [JsonProperty("hubUrl")]
        public string HubUrl { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
