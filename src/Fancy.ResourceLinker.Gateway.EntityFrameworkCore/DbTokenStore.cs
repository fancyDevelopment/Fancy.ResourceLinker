using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

/// <summary>
/// Implements the <see cref="ITokenStore"/> interface to store all tokens into a database.
/// </summary>
/// <seealso cref="ITokenStore" />
internal class DbTokenStore : ITokenStore
{
    /// <summary>
    /// The database context
    /// </summary>
    private readonly GatewayDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbTokenStore"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public DbTokenStore(GatewayDbContext dbContext) 
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the token record for a provided session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A token record if available.
    /// </returns>
    public async Task<TokenRecord?> GetTokenRecordAsync(string sessionId)
    {
        TokenSet? tokenSet = await _dbContext.TokenSets.SingleOrDefaultAsync(ts => ts.SessionId == sessionId);

        if (tokenSet == null) { return null; }

        return new TokenRecord(sessionId, tokenSet.IdToken, tokenSet.AccessToken, tokenSet.RefreshToken, tokenSet.ExpiresAt);
    }

    /// <summary>
    /// Saves the or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiresAt">The expires at.</param>
    public async Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        TokenSet? tokenSet = await _dbContext.TokenSets.SingleOrDefaultAsync(ts => ts.SessionId == sessionId);

        if (tokenSet == null)
        {
            tokenSet = new TokenSet(sessionId, idToken, accessToken, refreshToken, expiresAt);
            _dbContext.TokenSets.Add(tokenSet);
        }
        else
        {
            tokenSet.IdToken = idToken;
            tokenSet.AccessToken = accessToken;
            tokenSet.RefreshToken = refreshToken;
            tokenSet.ExpiresAt = expiresAt;
        }

        await _dbContext.SaveChangesAsync();
    }
}
