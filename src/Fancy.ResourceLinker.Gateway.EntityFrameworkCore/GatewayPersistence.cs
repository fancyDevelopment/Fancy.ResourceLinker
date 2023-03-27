using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

public static class GatewayPersistence
{
    public static void UseGatewayPersistence(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
