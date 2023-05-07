using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

/// <summary>
/// Class with helper methods to set up the ef core feature.
/// </summary>
public static class GatewayEntityFrameworkCore
{
    /// <summary>
    /// Ensures that the gateway database has been created.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    public static void EnsureGatewayDbCreated(this WebApplication webApp)
    {
        using IServiceScope scope = webApp.Services.CreateScope();
        GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
