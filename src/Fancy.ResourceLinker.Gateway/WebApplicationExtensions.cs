using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.AspNetCore.Builder;

namespace Fancy.ResourceLinker.Gateway;

/// <summary>
/// Extension class with helpers to easily add the gateway features to your middleware pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds the gateway anti forgery feature to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    public static void UseGatewayAntiForgery(this WebApplication webApp) => GatewayAntiForgery.UseGatewayAntiForgery(webApp);

    /// <summary>
    /// Adds the gateway authentication feature to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    public static void UseGatewayAuthentication(this WebApplication webApp) => GatewayAuthentication.UseGatewayAuthentication(webApp);

    /// <summary>
    /// Adds the gateway authentication endpoints to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    public static void UseGatewayAuthenticationEndpoints(this WebApplication webApp) => GatewayAuthenticationEndpoints.UseGatewayAuthenticationEndpoints(webApp);

    /// <summary>
    /// Adds the gateway routing feature to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    public static void UseGatewayRouting(this WebApplication webApp) => GatewayRouting.UseGatewayRouting(webApp);
}
