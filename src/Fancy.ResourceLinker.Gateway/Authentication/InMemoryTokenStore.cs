using System.Collections.Concurrent;

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A simple implementation of <see cref="ITokenStore"/> to store tokens in memory.
/// </summary>
/// <seealso cref="Fancy.ResourceLinker.Gateway.Authentication.ITokenStore" />
internal class InMemoryTokenStore : ITokenStore
{
    /// <summary>
    /// A dictionary mapping tokens to sessions.
    /// </summary>
    private ConcurrentDictionary<string, TokenRecord> _tokenStore = new ConcurrentDictionary<string, TokenRecord>();

    /// <summary>
    /// Saves the or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiration">The expiration.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation.
    /// </returns>
    public Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiration)
    {
        if (_tokenStore.ContainsKey(sessionId))
        {
            // Update existing token
            _tokenStore[sessionId].IdToken = idToken;
            _tokenStore[sessionId].IdToken = idToken;
            _tokenStore[sessionId].AccessToken = accessToken;
            _tokenStore[sessionId].RefreshToken = refreshToken;
            _tokenStore[sessionId].ExpiresAt = expiration;
        }
        else
        {
            _tokenStore[sessionId] = new TokenRecord(sessionId, idToken, accessToken, refreshToken, expiration);
        }
  
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the or update userinfo claims asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userinfoClaims">The userinfo object as json string.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation.
    /// </returns>
    public Task SaveOrUpdateUserinfoClaimsAsync(string sessionId, string userinfoClaims)
    {
        if(!_tokenStore.ContainsKey(sessionId))
        {
            throw new InvalidOperationException("The specified session Id is not valid");
        }

        _tokenStore[sessionId].UserinfoClaims = userinfoClaims;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the token record for a provided session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A token record if available.
    /// </returns>
    public Task<TokenRecord?> GetTokenRecordAsync(string sessionId)
    {
        if (_tokenStore.ContainsKey(sessionId)) return Task.FromResult<TokenRecord?>(_tokenStore[sessionId]);
        else return Task.FromResult<TokenRecord?>(null);
    }

    /// <summary>
    /// Cleans up the expired token records asynchronous.
    /// </summary>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task CleanupExpiredTokenRecordsAsync()
    {
        ConcurrentDictionary<string, TokenRecord> validRecords = new ConcurrentDictionary<string, TokenRecord> ();

        foreach(var record in _tokenStore.Values)
        {
            if(record.ExpiresAt > DateTime.UtcNow) validRecords[record.SessionId] = record;
        }

        _tokenStore = validRecords;

        return Task.CompletedTask;
    }
}
