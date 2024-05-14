using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// Interface for an authorization strategy.
/// </summary>
public interface IRouteAuthenticationStrategy
{
    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Initializes the authentication strategy based on the gateway authentication settings and the route authentication settings asynchronous.
    /// </summary>
    /// <param name="gatewayAuthenticationSettings">The gateway authentication settigns.</param>
    /// <param name="routeAuthenticationSettings">The route authentication settigns.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation.
    /// </returns>
    Task InitializeAsync(GatewayAuthenticationSettings? gatewayAuthenticationSettings, RouteAuthenticationSettings routeAuthenticationSettings);

    /// <summary>
    /// Sets the authentication to an http context asynchronous.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <returns>A task indicating the completion of the asynchronous operation</returns>
    Task SetAuthenticationAsync(HttpContext context);

    /// <summary>
    /// Sets the authentication to an http request message asynchronous.
    /// </summary>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="request">The http request message.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    Task SetAuthenticationAsync(IServiceProvider serviceProvider, HttpRequestMessage request);
}
