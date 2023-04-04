namespace Fancy.ResourceLinker.Models;

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
        if(method.Trim().ToLower() == "get")
        {
            throw new ArgumentException("An action may not have the HTTP Verb GET", "method");
        }

        Method = method;
        Href = href;
    }

    /// <summary>
    /// Gets or sets the HTTP method to use for this action.
    /// </summary>
    /// <value>
    /// The method.
    /// </value>
    public string Method { get; set; }

    /// <summary>
    /// Gets or sets the destination URL of the action.
    /// </summary>
    /// <value>
    /// The href.
    /// </value>
    public string Href { get; set; }
}