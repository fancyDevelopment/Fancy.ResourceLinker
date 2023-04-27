using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.AspNetCore.Builder;

namespace Fancy.ResourceLinker.Gateway;

/// <summary>
/// A builder to set up an anti forgery policy.
/// </summary>
public class AntiForgeryBuilder
{
    /// <summary>
    /// Gets or sets the exclusions.
    /// </summary>
    /// <value>
    /// The exclusions.
    /// </value>
    internal List<string> Exclusions { get; set; } = new List<string>();

    /// <summary>
    /// Excludes the specified path start.
    /// </summary>
    /// <param name="pathStart">The path start.</param>
    /// <returns>An instance to the same instance.</returns>
    public AntiForgeryBuilder Exclude(string pathStart)
    {
        Exclusions.Add(pathStart);
        return this;
    }
}

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
    /// Adds the gateway anti forgery feature to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    /// <param name="configurePolicy">An action to configure the anti forgery policy.</param>
    public static void UseGatewayAntiForgery(this WebApplication webApp, Action<AntiForgeryBuilder> configurePolicy) => GatewayAntiForgery.UseGatewayAntiForgery(webApp, configurePolicy);

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
