﻿using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// An auth strategy which just passes through the current token
/// </summary>
internal class TokenPassThroughAuthStrategy : IRouteAuthenticationStrategy
{
    /// <summary>
    /// The name of the auth strategy.
    /// </summary>
    public const string NAME = "TokenPassThrough";

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name => NAME;

    /// <summary>Initializes the authentication strategy based on the gateway authentication settings and the route authentication settings asynchronous.</summary>
    /// <param name="gatewayAuthenticationSettings">The gateway authentication settigns.</param>
    /// <param name="routeAuthenticationSettings">The route authentication settigns.</param>
    /// <returns>
    ///   A task indicating the completion of the asyncrhonous operation.
    /// </returns>
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
    public async Task SetAuthenticationAsync(HttpContext context)
    {
        TokenService tokenService = GetTokenService(context.RequestServices);
        string accessToken = await tokenService.GetAccessTokenAsync();
        context.Request.Headers.Authorization = new StringValues("Bearer " + accessToken);
    }

    /// <summary>
    /// Sets the authentication to an http request message asynchronous.
    /// </summary>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="request">The http request message.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public async Task SetAuthenticationAsync(IServiceProvider serviceProvider, HttpRequestMessage request)
    {
        TokenService? tokenService = GetTokenService(serviceProvider);
        string accessToken = await tokenService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Gets the token service.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>An instance of the current token service.</returns>
    private TokenService GetTokenService(IServiceProvider serviceProvider)
    {
        try
        {
            return serviceProvider.GetRequiredService<TokenService>();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("A token service in a scope is needed to use the token pass through auth strategy");
        }
    }
}