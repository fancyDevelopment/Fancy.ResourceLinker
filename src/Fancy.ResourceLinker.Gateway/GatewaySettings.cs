using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;

namespace Fancy.ResourceLinker.Gateway;

/// <summary>
/// A class to describe the required settings for setting up a gateway.
/// </summary>
public class GatewaySettings
{
    /// <summary>
    /// Gets or sets the authentication settings.
    /// </summary>
    /// <value>
    /// The authentication settings.
    /// </value>
    public GatewayAuthenticationSettings? Authentication { get; set; }

    /// <summary>
    /// Gets or sets the routing settings.
    /// </summary>
    /// <value>
    /// The routing settings.
    /// </value>
    public GatewayRoutingSettings? Routing { get; set; }
}
