using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

/// <summary>
/// Extension class with helpers to easily register the gateway ef core to ioc container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// A database context added flag.
    /// </summary>
    private static bool _dbContextAdded = false;

    /// <summary>
    /// Uses the provided database context in the gateway.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// A type of a gateway builder.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">DbContext can be added only once</exception>
    public static GatewayBuilder UseDbContext<TDbContext>(this GatewayBuilder builder) where TDbContext : GatewayDbContext
    {
        if(_dbContextAdded)
        {
            throw new InvalidOperationException("DbContext can be added only once");
        }

        builder.Services.AddScoped<GatewayDbContext>(services => services.GetRequiredService<TDbContext>());

        _dbContextAdded = true;

        return builder;
    }

    /// <summary>
    /// Adds the database token store for the authentication feature.
    /// </summary>
    /// <param name="builder">The gateway authentication builder.</param>
    /// <returns>The gateway authentication builder.</returns>
    public static GatewayAuthenticationBuilder UseDbTokenStore(this GatewayAuthenticationBuilder builder)
    {
        if (!_dbContextAdded)
        {
            throw new InvalidOperationException("Call 'AddDbContext' before configuring options using the db context");
        }

        builder.Services.AddScoped<ITokenStore, DbTokenStore>();

        return builder;
    }

    /// <summary>
    /// Adds the database anti forgery key store for the anti forgery feature.
    /// </summary>
    /// <param name="builder">The gateway anti forgery builder.</param>
    /// <returns>The gateway anti forgery builder.</returns>
    public static GatewayAntiForgeryBuilder UseDbKeyStore(this GatewayAntiForgeryBuilder builder)
    {
        if (!_dbContextAdded)
        {
            throw new InvalidOperationException("Call 'AddDbContext' before configuring options using the db context");
        }

        builder.Services.AddDataProtection().PersistKeysToDbContext<GatewayDbContext>();

        return builder;
    }
}
