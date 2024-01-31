using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;
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
    internal static readonly string SendAccessTokenItemKey = "SendAccessTokenItemKey";

    /// <summary>
    /// The target URL item key.
    /// </summary>
    internal static readonly string TargetUrlItemKey = "TargetUrlItemKey";

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        if (Convert.ToBoolean(httpContext.Items[SendAccessTokenItemKey]))
        {
            // Add access token
            TokenService? tokenService = httpContext.RequestServices.GetService(typeof(TokenService)) as TokenService;

            if (tokenService == null) throw new InvalidOperationException($"If 'EnforceAuthentication' is 'true', gateway authentication must be configured.");

            proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenService.GetAccessTokenAsync());
        }

        if(httpContext.Items.ContainsKey(TargetUrlItemKey))
        {
            // If an alternative target url ist provided use it
            string? targetUrl = Convert.ToString(httpContext.Items[TargetUrlItemKey]);
            if(targetUrl != null) proxyRequest.RequestUri = new Uri(targetUrl);
        }
        
        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;
    }
}
