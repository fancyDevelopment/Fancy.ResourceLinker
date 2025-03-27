using Fancy.ResourceLinker.Gateway.Routing.Auth;
using Fancy.ResourceLinker.Gateway.Routing.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
    /// The resource proxy item key.
    /// </summary>
    internal static readonly string ResourceProxyItemKey = "ResourceProxyItemKey";

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        string? routeName = httpContext.Items[RouteNameItemKey]?.ToString();
        string? resourceProxy = httpContext.Items[ResourceProxyItemKey]?.ToString();

        if (!string.IsNullOrEmpty(routeName))
        {
            // Add authentication
            RouteAuthenticationManager routeAuthenticationManager = httpContext.RequestServices.GetRequiredService<RouteAuthenticationManager>();
            IRouteAuthenticationStrategy authStrategy = await routeAuthenticationManager.GetAuthStrategyAsync(routeName);
            await authStrategy.SetAuthenticationAsync(httpContext.RequestServices, proxyRequest);
        }

        proxyRequest.RequestUri = new Uri(destinationPrefix);
        proxyRequest.Headers.SetForwardedHeaders(resourceProxy);

        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;
    }
}
