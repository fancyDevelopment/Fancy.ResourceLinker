namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A class to hold all token information needed for a specific user session.
/// </summary>
public class TokenRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRecord"/> class.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiresAt">The expires at.</param>
    public TokenRecord(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiresAt)
    {
        SessionId = sessionId;
        IdToken = idToken;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRecord" /> class.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="userinfoClaims">The userinfo claims.</param>
    /// <param name="expiresAt">The expires at.</param>
    public TokenRecord(string sessionId, string idToken, string accessToken, string refreshToken, string userinfoClaims, DateTime expiresAt)
    {
        SessionId = sessionId;
        IdToken = idToken;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserinfoClaims = userinfoClaims;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    public string SessionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier token.
    /// </summary>
    /// <value>
    /// The identifier token.
    /// </value>
    public string IdToken { get; set; }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    /// <value>
    /// The access token.
    /// </value>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    /// <value>
    /// The refresh token.
    /// </value>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the userinfo claims.
    /// </summary>
    /// <value>
    /// The userinfo claims.
    /// </value>
    public string? UserinfoClaims { get; set; }

    /// <summary>
    /// Gets or sets the expires at.
    /// </summary>
    /// <value>
    /// The expires at.
    /// </value>
    public DateTime ExpiresAt { get; set; }
}
