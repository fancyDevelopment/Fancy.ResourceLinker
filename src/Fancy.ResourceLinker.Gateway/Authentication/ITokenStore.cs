namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// Interface to a token store.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Saves the or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiration">The expiration.</param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt);

    /// <summary>
    /// Gets the token record for a provided session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A token record if available.
    /// </returns>
    Task<TokenRecord?> GetTokenRecordAsync(string sessionId);
}