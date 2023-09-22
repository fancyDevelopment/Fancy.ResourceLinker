using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Models;

/// <summary>
/// Interface for a resource which container embedded metadata as hypermedia document.
/// </summary>
public interface IResource
{
    /// <summary>
    /// Gets or sets the links of this resource.
    /// </summary>
    /// <value>
    /// The links.
    /// </value>
    [JsonPropertyName("_links")]
    [NotMapped]
    Dictionary<string, ResourceLink> Links { get; }

    /// <summary>
    /// Gets or sets the actions of this resource.
    /// </summary>
    /// <value>
    /// The actions.
    /// </value>
    [JsonPropertyName("_actions")]
    [NotMapped]
    Dictionary<string, ResourceAction> Actions { get; }

    /// <summary>
    /// Gets the sockets.
    /// </summary>
    /// <value>
    /// The sockets.
    /// </value>
    [JsonPropertyName("_sockets")]
    [NotMapped]
    Dictionary<string, ResourcSocket> Sockets { get; }

    /// <summary>
    /// Adds a link.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    void AddLink(string rel, string href)
    {
        Links[rel] = new ResourceLink(href);
    }

    /// <summary>
    /// Adds an action.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="method">The method.</param>
    /// <param name="href">The URL to the action.</param>
    void AddAction(string rel, string method, string href);

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    /// <param name="token">The token.</param>
    void AddSocket(string rel, string href, string method, string token);

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    void AddSocket(string rel, string href, string method);

    /// <summary>
    /// Removes the metadata of links, actions and sockets completely from this instance.
    /// </summary>
    void ClearMetadata();

    /// <summary>
    /// Gets a collection containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// Gets the static keys.
    /// </summary>
    /// <value>
    /// The static keys.
    /// </value>
    internal ICollection<string> StaticKeys { get; }

    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> with the specified key.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="key">The key.</param>
    /// <returns>The object.</returns>
    object? this[string key] { get; set; }
}