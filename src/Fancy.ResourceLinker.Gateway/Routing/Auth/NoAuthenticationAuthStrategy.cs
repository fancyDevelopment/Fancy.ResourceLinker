using Microsoft.AspNetCore.Http;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// An authentication strategy which does not set any authentication.
/// </summary>
public class NoAuthenticationAuthStrategy : IAuthStrategy
{
    /// <summary>
    /// The name of the auth strategy
    /// </summary>
    public const string NAME = "NoAuthentication";

    /// <summary>
    /// Initializes a new instance of the <see cref="NoAuthenticationAuthStrategy"/> class.
    /// </summary>
    /// <param name="authenticationSettings">The authentication settings.</param>
    public NoAuthenticationAuthStrategy(RouteAuthenticationSettings authenticationSettings)
    {
    }


    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name => NAME;

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
