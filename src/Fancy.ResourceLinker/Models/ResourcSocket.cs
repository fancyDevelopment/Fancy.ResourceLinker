
namespace Fancy.ResourceLinker.Models
{
    /// <summary>
    /// Contains information regarding a socket which can be used to implement server to client messaging onto a resource.
    /// </summary>
    public class ResourcSocket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcSocket" /> class.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="method">The method.</param>
        /// <param name="token">The token.</param>
        public ResourcSocket(string href, string method, string token)
        {
            Href = href;
            Method = method;
            Token = token;
        }

        /// <summary>
        ///   Gets or sets the hub URL.
        /// </summary>
        /// <value>
        ///   The hub URL.
        /// </value>
        public string Href { get; set; }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }
    }
}
