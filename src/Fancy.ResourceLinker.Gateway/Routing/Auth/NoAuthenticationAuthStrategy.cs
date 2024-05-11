using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// An authentication strategy which does not set any authentication.
/// </summary>
public class NoAuthenticationAuthStrategy : IRouteAuthenticationStrategy
{
    /// <summary>
    /// The name of the auth strategy
    /// </summary>
    public const string NAME = "NoAuthentication";

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name => NAME;

    public Task InitializeAsync(GatewayAuthenticationSettings? gatewayAuthenticationSettings, RouteAuthenticationSettings routeAuthenticationSettings)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    /// Sets the authentication to an http context asynchronous.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public Task SetAuthenticationAsync(HttpContext context)
    {
        // Nothing to do here!
        return  Task.CompletedTask;
    }

    /// <summary>
    /// Sets the authentication to an http request message asynchronous.
    /// </summary>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="request">The http request message.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public Task SetAuthenticationAsync(IServiceProvider serviceProvider, HttpRequestMessage request)
    {
        // Nothing to do here!
        return Task.CompletedTask;
    }
}
