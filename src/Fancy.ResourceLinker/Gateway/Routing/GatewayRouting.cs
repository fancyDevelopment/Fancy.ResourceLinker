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

        public static void AddMicroserviceRoutes(this IReverseProxyBuilder reverseProxyBuilder, GatewayRoutingSettings settings)
        {
            List<RouteConfig> routes = new List<RouteConfig>();
            List<ClusterConfig> clusters = new List<ClusterConfig>();

            // Add for each microservcie a route and a cluster
            foreach (KeyValuePair<string, MicroserviceSettings> microserviceSettings in settings.Microservices)
            {
                routes.Add(new RouteConfig { ClusterId = microserviceSettings.Key });
            }

            reverseProxyBuilder.LoadFromMemory(routes, clusters);
        }
    }
}
