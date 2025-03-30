using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// A class to create an hold instances of route authentication strategies for specific routes.
/// </summary>
public class RouteAuthenticationManager
{
    /// <summary>
    /// The route authentication strategy instances.
    /// </summary>
    private Dictionary<string, IRouteAuthenticationStrategy> _authStrategyInstances = new Dictionary<string, IRouteAuthenticationStrategy>();

    /// <summary>
    /// The routing settings.
    /// </summary>
    private readonly GatewayRoutingSettings _routingSettings;

    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The gateway authentication settings if available.
    /// </summary>
    private readonly GatewayAuthenticationSettings? _gatewayAuthenticationSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteAuthenticationManager" /> class.
    /// </summary>
    /// <param name="routingSettings">The routing settings.</param>
    /// <param name="serviceProvider">The service provider to use to get instances of auth strategies.</param>
    public RouteAuthenticationManager(GatewayRoutingSettings routingSettings, IServiceProvider serviceProvider)
    {
        _routingSettings = routingSettings;
        _serviceProvider = serviceProvider;
        _gatewayAuthenticationSettings = GetGatewayAuthenticationSettings();
    }

    /// <summary>
    /// Gets an authentication strategy for a specific route asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>An instance of the auth strategy.</returns>
    public async Task<IRouteAuthenticationStrategy> GetAuthStrategyAsync(string route)
    {
        if(!_authStrategyInstances.ContainsKey(route))
        {
            return await CreateAuthStrategyAsync(route);
        }

        return _authStrategyInstances[route];
    }

    /// <summary>
    /// Creates an authentication strategy asynchronous.
    /// </summary>
    /// <param name="route">The route.</param>
    /// <returns>
    /// The authentication strategy.
    /// </returns>
    private async Task<IRouteAuthenticationStrategy> CreateAuthStrategyAsync(string route)
    {
        RouteAuthenticationSettings routeAuthSettings = _routingSettings.Routes[route].Authentication;

        IRouteAuthenticationStrategy authStrategy;

        try
        {
            authStrategy = _serviceProvider.GetRequiredKeyedService<IRouteAuthenticationStrategy>(routeAuthSettings.Strategy);
        }
        catch(InvalidOperationException e)
        {
            throw new InvalidOperationException($"Could not retrieve an IRouteAuthenticationStrategy with strategy name '{routeAuthSettings.Strategy}'", e);
        }
        
        await authStrategy.InitializeAsync(_gatewayAuthenticationSettings, routeAuthSettings);

        _authStrategyInstances[route] = authStrategy;

        return authStrategy;
    }

    /// <summary>
    /// Gets the gateway authentication settings.
    /// </summary>
    /// <returns>The gateway authentication settings or null if none are set.</returns>
    private GatewayAuthenticationSettings? GetGatewayAuthenticationSettings()
    {
        try
        {
            return _serviceProvider.GetService<GatewayAuthenticationSettings>();
        } 
        catch 
        { 
            return null; 
        }
    }
}
