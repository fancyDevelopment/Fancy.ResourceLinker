using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Forwarder;

namespace Fancy.ResourceLinker.Gateway.Routing;

internal class GatewayForwarderHttpTransformer : HttpTransformer
{
    public static readonly string SendAccessTokenItemKey = "SendAccessTokenItemKey";

    public override async ValueTask TransformRequestAsync(HttpContext httpContext,
    HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        // Copy all request headers
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        // Add access token
        TokenService? tokenService = httpContext.RequestServices.GetService(typeof(TokenService)) as TokenService;

        if (Convert.ToBoolean(httpContext.Items[SendAccessTokenItemKey]))
        {
            proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenService.GetAccessTokenAsync());
        }
        
        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;
    }
}
