using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.AspNetCore.Builder;

namespace Fancy.ResourceLinker.Gateway;

public static class WebApplicationExtensions
{
    public static void UseGatewayAntiForgery(this WebApplication app) => GatewayAntiForgery.UseGatewayAntiForgery(app);

    public static void UseGatewayAuthentication(this WebApplication app) => GatewayAuthentication.UseGatewayAuthentication(app);

    public static void UseGatewayAuthenticationEndpoints(this WebApplication app) => GatewayAuthenticationEndpoints.UseGatewayAuthenticationEndpoints(app);

    public static void UseGatewayRouting(this WebApplication app) => GatewayRouting.UseGatewayRouting(app);
}
