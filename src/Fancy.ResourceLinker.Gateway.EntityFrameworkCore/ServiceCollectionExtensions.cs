using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
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
    /// Adds the database context to the gateway.
    /// </summary>
    /// <typeparam name="T">A type of a gateway builder.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="optionsAction">The options action.</param>
    /// <returns>A type of a gateway builder.</returns>
    public static T AddDbContext<T>(this T builder, Action<DbContextOptionsBuilder> optionsAction) where T : GatewayBuilder
    {
        if(_dbContextAdded)
        {
            throw new InvalidOperationException("DbContext can be added only once");
        }

        builder.Services.AddDbContext<GatewayDbContext>(optionsAction);

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
            throw new InvalidOperationException("Call 'AddDbContext' before configuring options");
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
            throw new InvalidOperationException("Call 'AddDbContext' before configuring options");
        }

        builder.Services.AddDataProtection().PersistKeysToDbContext<GatewayDbContext>();

        return builder;
    }
}
