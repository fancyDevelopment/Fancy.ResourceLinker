using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Forwarder;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// A http transformer used in combination with the http forwarder to add token if necessary.
/// </summary>
/// <seealso cref="Yarp.ReverseProxy.Forwarder.HttpTransformer" />
internal class GatewayForwarderHttpTransformer : HttpTransformer
{
    /// <summary>
    /// The send access token item key.
    /// </summary>
    internal static readonly string RouteNameItemKey = "RouteNameItemKey";

    /// <summary>
    /// The target URL item key.
    /// </summary>
    internal static readonly string TargetUrlItemKey = "TargetUrlItemKey";

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        string? routeName = httpContext.Items[RouteNameItemKey]?.ToString();

        if(!string.IsNullOrEmpty(routeName))
        {
            // Add authentication
            RouteAuthenticationManager routeAuthenticationManager = httpContext.RequestServices.GetRequiredService<RouteAuthenticationManager>();
            IRouteAuthenticationStrategy authStrategy = await routeAuthenticationManager.GetAuthStrategyAsync(routeName);
            await authStrategy.SetAuthenticationAsync(httpContext.RequestServices, proxyRequest);
        }

        proxyRequest.RequestUri = new Uri(destinationPrefix);

        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;
    }
}
