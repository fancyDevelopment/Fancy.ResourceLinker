﻿using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// Helper class to set up the pipeline for the yarp proxy.
/// </summary>
internal static class GatewayPipeline
{
    /// <summary>
    /// Adds required middleware to the yarp proxy pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    internal static void UseGatewayPipeline(this IReverseProxyApplicationBuilder pipeline)
    {
        pipeline.Use(async (context, next) =>
        {
            // Check if token shall be added
            var proxyFeature = context.GetReverseProxyFeature();
            if (proxyFeature.Route.Config.Metadata != null
                && proxyFeature.Route.Config.Metadata.ContainsKey("EnforceAuthentication")
                && proxyFeature.Route.Config.Metadata["EnforceAuthentication"] == "True")
            {
                // Add access token to request
                var tokenService = context.RequestServices.GetRequiredService<TokenService>();
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Fancy.ResourceLinker.Gateway.Routing.GatewayPipeline");
                logger.LogDebug("Adding Authorization header and token into request to " + context.Request.GetDisplayUrl());
                var accessToken = await tokenService.GetAccessTokenAsync();
                context.Request.Headers.Add("Authorization", "Bearer " + accessToken);
            }
            await next().ConfigureAwait(false);
        });
    }
}
