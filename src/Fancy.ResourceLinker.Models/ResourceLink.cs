namespace Fancy.ResourceLinker.Models;

/// <summary>
/// Contains information regarding a link which can be performed onto a resource.
/// </summary>
public class ResourceLink
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceLink"/> class.
    /// </summary>
    /// <param name="href">The href.</param>
    public ResourceLink(string href)
    {
        Href = href;
    }

    /// <summary>
    /// Gets or sets the destination URL of the link.
    /// </summary>
    /// <value>
    /// The href.
    /// </value>
    public string Href { get; set; }
}