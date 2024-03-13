using Fancy.ResourceLinker.Gateway.Routing.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
                && proxyFeature.Route.Config.Metadata.ContainsKey("RouteName"))
            {
                string routeName = proxyFeature.Route.Config.Metadata["RouteName"];
                AuthStrategyFactory authStrategyFactory = context.RequestServices.GetRequiredService<AuthStrategyFactory>();
                IAuthStrategy authStrategy = authStrategyFactory.GetAuthStrategy(routeName);
                await authStrategy.SetAuthenticationAsync(context);
            }
            await next().ConfigureAwait(false);
        });
    }
}
