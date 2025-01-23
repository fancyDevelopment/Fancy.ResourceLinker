using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

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
    /// The cached tokens.
    /// </summary>
    ConcurrentDictionary<string, TokenSet?> _cachedTokens = new ConcurrentDictionary<string, TokenSet?>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DbTokenStore"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public DbTokenStore(GatewayDbContext dbContext) 
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Saves or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiresAt">The expires at.</param>
    public async Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiresAt)
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

        _cachedTokens[sessionId] = tokenSet;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Saves the or update userinfo claims asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userinfoClaims">The userinfo object as json string.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation.
    /// </returns>
    public async Task SaveOrUpdateUserinfoClaimsAsync(string sessionId, string userinfoClaims)
    {
        TokenSet? tokenSet = await _dbContext.TokenSets.SingleOrDefaultAsync(ts => ts.SessionId == sessionId);

        if (tokenSet == null)
        {
            throw new InvalidOperationException("The specified session Id is not valid");
        }
        else
        {
            tokenSet.UserinfoClaims = userinfoClaims;
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the token record for a provided session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A token record if available.
    /// </returns>
    /// <remarks>
    /// This method is thread save to enable paralell calls from the gateway to backends.
    /// </remarks>
    public Task<TokenRecord?> GetTokenRecordAsync(string sessionId)
    {
        TokenSet? tokenSet;

        lock (_dbContext)
        {
            if (!_cachedTokens.ContainsKey(sessionId))
            {
                tokenSet = _dbContext.TokenSets.SingleOrDefault(ts => ts.SessionId == sessionId);
                _cachedTokens[sessionId] = tokenSet;
            }
            else
            {
                tokenSet = _cachedTokens[sessionId];
            }
        }

        if (tokenSet == null) { return Task.FromResult<TokenRecord?>(null); }

        return Task.FromResult<TokenRecord?>(new TokenRecord(sessionId, tokenSet.IdToken, tokenSet.AccessToken, tokenSet.RefreshToken, tokenSet.ExpiresAt));
    }

    /// <summary>
    /// Cleans up the expired token records asynchronous.
    /// </summary>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task CleanupExpiredTokenRecordsAsync()
    {
        return _dbContext.TokenSets.Where(ts => ts.ExpiresAt < DateTime.UtcNow).ExecuteDeleteAsync();
    }
}
