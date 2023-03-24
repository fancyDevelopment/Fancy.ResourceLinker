using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker.Gateway.Authentication
{
    public static class GatewayPipeline
    {
        public static void UseGatewayPipeline(this IReverseProxyApplicationBuilder pipeline)
        {
            pipeline.Use(async (context, next) =>
            {
                var proxyFeature = context.GetReverseProxyFeature();
                if (proxyFeature.Route.Config.Metadata.ContainsKey("EnforceAuthentication")
                    && proxyFeature.Route.Config.Metadata["EnforceAuthentication"] == "True")
                {
                    var tokenService = context.RequestServices.GetRequiredService<TokenService>();
                    var accessToken = await tokenService.GetAccessTokenAsync();
                    context.Request.Headers.Add("Authorization", "Bearer " + accessToken);
                }
                await next().ConfigureAwait(false);
            });
        }
    }
}
