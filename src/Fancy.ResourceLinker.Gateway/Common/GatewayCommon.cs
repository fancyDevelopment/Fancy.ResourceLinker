using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.Common;

/// <summary>
/// Class with helper methods to set up the common services.
/// </summary>
internal class GatewayCommon
{
    /// <summary>
    /// Adds the commmon gateway services.
    /// </summary>
    /// <param name="services">The services.</param>
    internal static void AddGatewayCommonServices(IServiceCollection services)
    {
        services.AddSingleton<DiscoveryDocumentService>();
    }
}
