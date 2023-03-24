using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

internal class GatewayDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<TokenSet> TokenSets { get; set; }
}
