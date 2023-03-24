using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

public static class GatewayPersistence
{
    public static void AddGatewayPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> dbOptionsAction)
    {
        services.AddDbContext<GatewayDbContext>(dbOptionsAction);

        // Use database for storing encryption keys
        services.AddDataProtection().PersistKeysToDbContext<GatewayDbContext>();

        // Replace the default in memory token store with the db token store
        ServiceDescriptor serviceDescriptor = new ServiceDescriptor(typeof(ITokenStore), typeof(DbTokenStore), ServiceLifetime.Scoped);
        services.Replace(serviceDescriptor);
    }

    public static void AddGatewayPersistence(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
