using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace Fancy.ResourceLinker.Gateway.Routing
{
    internal class GatewayForwarderHttpTransformer : HttpTransformer
    {
        public override async ValueTask TransformRequestAsync(HttpContext httpContext,
        HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
        {
            // Copy all request headers
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

            // Add access token
            TokenService? tokenService = httpContext.RequestServices.GetService(typeof(TokenService)) as TokenService;

            if(tokenService == null)
            {
                proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenService.GetAccessTokenAsync());
            }
            
            // Suppress the original request header, use the one from the destination Uri.
            proxyRequest.Headers.Host = null;
        }
    }
}
