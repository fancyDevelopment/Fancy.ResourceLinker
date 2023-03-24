using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace Fancy.ResourceLinker.Gateway.Routing
{
    public static class GatewayRouting
    {
        public static void AddGatewayRouting(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpForwarder();
            services.Configure<GatewayRoutingSettings>(config);
            services.AddScoped<GatewayRouter>();
        }

        public static void AddGatewayRoutes(this IReverseProxyBuilder reverseProxyBuilder, IConfiguration config)
        {
            GatewayRoutingSettings settings = config.Get<GatewayRoutingSettings>();

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
    }
}
