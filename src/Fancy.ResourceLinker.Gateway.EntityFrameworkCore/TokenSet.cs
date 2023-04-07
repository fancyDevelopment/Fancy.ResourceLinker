using System.ComponentModel.DataAnnotations;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

/// <summary>
/// A entity object to save token sets to database.
/// </summary>
internal class TokenSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenSet"/> class.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiresAt">The expires at.</param>
    public TokenSet(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiresAt)
    {
        SessionId = sessionId;
        IdToken = idToken;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    [Key]
    [MaxLength(40)]
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
    /// Gets or sets the expires at.
    /// </summary>
    /// <value>
    /// The expires at.
    /// </value>
    public DateTime ExpiresAt { get; set; }
}
