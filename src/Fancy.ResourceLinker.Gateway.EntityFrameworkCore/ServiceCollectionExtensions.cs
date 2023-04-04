using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore
{
    public static class ServiceCollectionExtensions
    {
        private static bool _dbContextAdded = false;
        
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

        public static GatewayAuthenticationBuilder UseDbTokenStore(this GatewayAuthenticationBuilder builder)
        {
            if (!_dbContextAdded)
            {
                throw new InvalidOperationException("Call 'AddDbContext' before configuring options");
            }

            builder.Services.AddScoped<ITokenStore, DbTokenStore>();

            return builder;
        }

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
}
