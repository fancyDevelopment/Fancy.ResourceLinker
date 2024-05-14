using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Common;
using Microsoft.Extensions.Logging;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// A route authentication strategy running the OAuth client credential flow only. 
/// </summary>
/// <remarks>
/// In some situations, auth0 requires an audience parameter in the request. This once can be added with this auth strategy.
/// </remarks>
internal class Auth0ClientCredentialOnlyAuthStrategy : ClientCredentialOnlyAuthStrategy
{
    /// <summary>
    /// The name of the auth strategy.
    /// </summary>
    public new const string NAME = "Auth0ClientCredentialOnly";

    // <summary>
    /// The auth0 audience.
    /// </summary>
    private string _audience = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialOnlyAuthStrategy" /> class.
    /// </summary>
    /// <param name="discoveryDocumentService">The discovery document service.</param>
    /// <param name="logger">The logger.</param>
    public Auth0ClientCredentialOnlyAuthStrategy(DiscoveryDocumentService discoveryDocumentService, ILogger<Auth0ClientCredentialOnlyAuthStrategy> logger) : base(discoveryDocumentService, logger)
    {
    }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public override string Name => NAME;

    /// <summary>
    /// Initializes the authentication strategy based on the gateway authentication settings and the route authentication settings asynchronous.
    /// </summary>
    /// <param name="gatewayAuthenticationSettings">The gateway authentication settigns.</param>
    /// <param name="routeAuthenticationSettings">The route authentication settigns.</param>
    public override Task InitializeAsync(GatewayAuthenticationSettings? gatewayAuthenticationSettings, RouteAuthenticationSettings routeAuthenticationSettings)
    {
        if (routeAuthenticationSettings.Options.ContainsKey("Audience"))
        {
            _audience = routeAuthenticationSettings.Options["Audience"];
        }

        return base.InitializeAsync(gatewayAuthenticationSettings, routeAuthenticationSettings);
    }

    /// <summary>
    /// Sets up the token request.
    /// </summary>
    /// <returns>The token request.</returns>
    protected override Dictionary<string, string> SetUpTokenRequest()
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "scope", _scope },
            { "audience", _audience }
        };
    }
}
