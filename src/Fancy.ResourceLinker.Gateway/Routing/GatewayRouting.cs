using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace Fancy.ResourceLinker.Gateway.Routing;

public static class GatewayRouting
{
    internal static void AddGatewayRouting(IServiceCollection services, GatewayRoutingSettings settings)
    {
        services.AddHttpForwarder();
        services.AddSingleton(settings);
        services.AddScoped<GatewayRouter>();
        services.AddReverseProxy().AddGatewayRoutes(settings);
    }

    internal static void AddGatewayRoutes(this IReverseProxyBuilder reverseProxyBuilder, GatewayRoutingSettings settings)
    {
        List<RouteConfig> routes = new List<RouteConfig>();
        List<ClusterConfig> clusters = new List<ClusterConfig>();

        // Add for each microservcie a route and a cluster
        foreach (KeyValuePair<string, RouteSettings> routeSettings in settings.Routes)
        {
            routes.Add(new RouteConfig
            {
                RouteId = routeSettings.Key,
                ClusterId = routeSettings.Key,
                AuthorizationPolicy = routeSettings.Value.EnforceAuthentication ? GatewayAuthentication.AuthenticationPolicyName : null,
                Match = new RouteMatch { Path = routeSettings.Value.PathMatch },
                Metadata = new Dictionary<string, string> { { "EnforceAuthentication", routeSettings.Value.EnforceAuthentication.ToString() } }
            });

            clusters.Add(new ClusterConfig
            {
                ClusterId = routeSettings.Key,
                Destinations = new Dictionary<string, DestinationConfig> { { "default", new DestinationConfig { Address = routeSettings.Value.BaseUrl.AbsoluteUri } } },
            });
        }

        reverseProxyBuilder.LoadFromMemory(routes, clusters);
    }

    public static void UseGatewayRouting(WebApplication app)
    {
        app.MapReverseProxy(pipeline =>
        {
            pipeline.UseGatewayPipeline();
        });
    }
}
