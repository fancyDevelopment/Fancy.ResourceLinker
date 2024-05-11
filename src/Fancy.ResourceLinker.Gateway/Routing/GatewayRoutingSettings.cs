using Fancy.ResourceLinker.Gateway.Routing.Auth;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// A class to hold all settings required to configure the gateway routing feature.
/// </summary>
public class GatewayRoutingSettings
{
    /// <summary>
    /// Gets or sets the resource proxy.
    /// </summary>
    /// <value>
    /// The resource proxy.
    /// </value>
    public string? ResourceProxy { get; set; }

    /// <summary>
    /// Gets or sets the routes.
    /// </summary>
    /// <value>
    /// The routes.
    /// </value>
    public IDictionary<string, RouteSettings> Routes { get; set; } = new Dictionary<string, RouteSettings>();

    /// <summary>
    /// Validates the settings.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Each route needs to have at least a 'BaseUrl'</exception>
    public void Validate()
    {
        // Check required fields of each route
        foreach(RouteSettings route in Routes.Values) 
        { 
            if(route.BaseUrl == null || string.IsNullOrEmpty(route.BaseUrl.AbsoluteUri))
            {
                throw new InvalidOperationException("Each route needs to have at least a 'BaseUrl'");
            }
        }
    }
}

/// <summary>
/// A class to hold all settings required for each route.
/// </summary>
public class RouteSettings
{
    /// <summary>
    /// Gets or sets the base URL.
    /// </summary>
    /// <value>
    /// The base URL.
    /// </value>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the path match.
    /// </summary>
    /// <value>
    /// The path match.
    /// </value>
    public string? PathMatch { get; set; }

    /// <summary>
    /// Gets or sets the authentication settings.
    /// </summary>
    /// <value>
    /// The authentication settings.
    /// </value>
    public RouteAuthenticationSettings Authentication { get; set; } = new RouteAuthenticationSettings();
}

/// <summary>
/// A class to hold all setting required for authenticating to a backend.
/// </summary>
public class RouteAuthenticationSettings
{
    /// <summary>
    /// Gets or sets the strategy.
    /// </summary>
    /// <value>
    /// The strategy.
    /// </value>
    public string Strategy { get; set; } = "NoAuthentication";

    /// <summary>
    /// Gets or sets the options.
    /// </summary>
    /// <value>
    /// The options.
    /// </value>
    public IDictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
}


