using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

/// <summary>
/// A database context to hold all information needed to be persisted in a gateway.
/// </summary>
public abstract class GatewayDbContext : DbContext, IDataProtectionKeyContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayDbContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public GatewayDbContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Gets or sets the data protection keys.
    /// </summary>
    /// <value>
    /// The data protection keys.
    /// </value>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token sets.
    /// </summary>
    /// <value>
    /// The token sets.
    /// </value>
    public DbSet<TokenSet> TokenSets { get; set; } = null!;
}