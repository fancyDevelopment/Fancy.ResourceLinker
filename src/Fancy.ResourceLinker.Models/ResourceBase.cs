using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Models;

/// <summary>
/// Base class of a resource which can be linked to other resources.
/// </summary>
public abstract class ResourceBase : IResource
{
    /// <summary>
    /// The static keys.
    /// </summary>
    private ICollection<string> _staticKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceBase"/> class.
    /// </summary>
    public ResourceBase()
    {
        // Initialize metadata dictionaries
        Links = new Dictionary<string, ResourceLink>();
        Actions = new Dictionary<string, ResourceAction>();
        Sockets = new Dictionary<string, ResourceSocket>();

        // Get all static (compile time) properties of this type
        _staticKeys = GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.GetIndexParameters().Length == 0)
                     .Select(p => p.Name)
                     .ToList();

        // Remove the control keys of the dictionary interface
        _staticKeys.Remove("Keys");
    }

    /// <summary>
    /// Gets or sets the links of this resource.
    /// </summary>
    /// <value>
    /// The links.
    /// </value>
    [JsonPropertyName("_links")]
    [NotMapped]
    public Dictionary<string, ResourceLink> Links { get; internal set; }

    /// <summary>
    /// Gets or sets the actions of this resource.
    /// </summary>
    /// <value>
    /// The actions.
    /// </value>
    [JsonPropertyName("_actions")]
    [NotMapped]
    public Dictionary<string, ResourceAction> Actions { get; internal set; }

    /// <summary>
    /// Gets the sockets.
    /// </summary>
    /// <value>
    /// The sockets.
    /// </value>
    [JsonPropertyName("_sockets")]
    [NotMapped]
    public Dictionary<string, ResourceSocket> Sockets { get; internal set; }

    /// <summary>
    /// Adds a link.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    public void AddLink(string rel, string href)
    {
        Links[rel] = new ResourceLink(href);
    }

    /// <summary>
    /// Adds an action.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="method">The method.</param>
    /// <param name="href">The URL to the action.</param>
    public void AddAction(string rel, string method, string href)
    {
        Actions[rel] = new ResourceAction(method, href);
    }

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    /// <param name="token">The token.</param>
    public void AddSocket(string rel, string href, string method, string token)
    {
        Sockets[rel] = new ResourceSocket(href, method, token);
    }

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    public void AddSocket(string rel, string href, string method)
    {
        Sockets.Add(rel, new ResourceSocket(href, method));
    }

    /// <summary>
    /// Removes the metadata of links, actions and sockets completely from this instance.
    /// </summary>
    public void ClearMetadata()
    {
        Links.Clear();
        Actions.Clear();
        Sockets.Clear();
    }

    /// <summary>
    /// Gets a collection containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
    /// </summary>
    public ICollection<string> Keys { get => _staticKeys; }

    /// <summary>
    /// Gets the static keys.
    /// </summary>
    /// <value>
    /// The static keys.
    /// </value>
    ICollection<string> IResource.StaticKeys { get => _staticKeys; }

    // The couterpart to the explicit interface implementation to make it accessible within the class
    private ICollection<string> StaticKeys { get => _staticKeys; }

    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> with the specified key.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="key">The key.</param>
    /// <returns>The object.</returns>
    public object? this[string key]
    {
        get
        {
            if (StaticKeys.Contains(key))
            {
                return GetType().GetProperty(key)!.GetValue(this);
            }
            else
            {
                throw new KeyNotFoundException($"Key {key} does not exist on this statically typed resource. If you want to work with dynamic data structures use 'DynamicResourceBase'.");
            }
        }
        set
        {
            if (StaticKeys.Contains(key))
            {
                GetType().GetProperty(key)!.SetValue(this, value);
            }
            else
            {
                throw new KeyNotFoundException($"Key {key} does not exist on this statically typed resource. If you want to work with dynamic data structures use 'DynamicResourceBase'.");
            }
        }
    }
}