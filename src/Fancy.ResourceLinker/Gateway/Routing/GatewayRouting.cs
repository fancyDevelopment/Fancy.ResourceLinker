using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
