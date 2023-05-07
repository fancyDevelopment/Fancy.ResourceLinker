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
    private Dictionary<string, TokenRecord> _tokens = new Dictionary<string, TokenRecord>();

    /// <summary>
    /// Saves the or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiration">The expiration.</param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiration)
    {
        _tokens[sessionId] = new TokenRecord(sessionId, idToken, accessToken, refreshToken, expiration );
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
        if (_tokens.ContainsKey(sessionId)) return Task.FromResult<TokenRecord?>(_tokens[sessionId]);
        else return Task.FromResult<TokenRecord?>(null);
    }

    /// <summary>
    /// Cleans up the expired token records asynchronous.
    /// </summary>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    public Task CleanupExpiredTokenRecordsAsync()
    {
        Dictionary<string, TokenRecord> validRecords = new Dictionary<string, TokenRecord> ();

        foreach(var record in _tokens.Values)
        {
            if(record.ExpiresAt > DateTime.UtcNow) validRecords.Add(record.SessionId, record);
        }

        _tokens = validRecords;

        return Task.CompletedTask;
    }
}
