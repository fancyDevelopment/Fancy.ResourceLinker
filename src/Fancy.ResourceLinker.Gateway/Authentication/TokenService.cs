using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A token service with handling logic for tokens needed by the gateway authentication feature.
/// </summary>
public class TokenService
{
    /// <summary>
    /// The token store.
    /// </summary>
    private readonly ITokenStore _tokenStore;

    /// <summary>
    /// The token client.
    /// </summary>
    private readonly TokenClient _tokenClient;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<TokenService> _logger;

    /// <summary>
    /// The environment.
    /// </summary>
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// The authentication settings.
    /// </summary>
    private readonly GatewayAuthenticationSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="tokenClient">The token client.</param>
    public TokenService(ITokenStore tokenStore, 
                        TokenClient tokenClient, 
                        ILogger<TokenService> logger, 
                        IWebHostEnvironment environment, 
                        GatewayAuthenticationSettings settings) 
    {
        _tokenStore = tokenStore;
        _tokenClient = tokenClient;
        _logger = logger;
        _environment = environment;
        _settings = settings;
    }

    /// <summary>
    /// Gets or sets the current session identifier.
    /// </summary>
    /// <value>
    /// The current session identifier.
    /// </value>
    internal string? CurrentSessionId { get; set; }

    /// <summary>
    /// Saves a token for a new session and generates a new unique session id asynchronous.
    /// </summary>
    /// <param name="tokenResponse">The token response from authorization server.</param>
    /// <returns>
    /// A new unique session id.
    /// </returns>
    internal async Task<string> SaveTokenForNewSessionAsync(OpenIdConnectMessage tokenResponse)
    {
        // Create a new guid for the new session
        string sessionId = Guid.NewGuid().ToString();
        await SaveOrUpdateTokenAsync(sessionId, tokenResponse);

        if(_settings.QueryUserInfoEndpoint)
        {
            string userinfoClaims = await _tokenClient.QueryUserInfoEndpoint(tokenResponse.AccessToken);
            await _tokenStore.SaveOrUpdateUserinfoClaimsAsync(sessionId, userinfoClaims);
        }

        return sessionId;
    }

    /// <summary>
    /// Saves or updates a token for an existing session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="tokenResponse">The token response from authorisation server.</param>
    internal async Task SaveOrUpdateTokenAsync(string sessionId, OpenIdConnectMessage tokenResponse)
    {
        string idToken = tokenResponse.IdToken;
        string accessToken = tokenResponse.AccessToken;
        string refreshToken = tokenResponse.RefreshToken;
        DateTime expiresAt = DateTime.UtcNow.AddSeconds(Convert.ToInt32(tokenResponse.ExpiresIn));

        await _tokenStore.SaveOrUpdateTokensAsync(sessionId, idToken, accessToken, refreshToken, expiresAt);
    }

    /// <summary>
    /// Gets the access token of the current session asynchronous.
    /// </summary>
    /// <returns>The access token.</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        if(CurrentSessionId == null)
        {
            throw new NoSessionIdException();
        }

        TokenRecord? tokenRecord = await _tokenStore.GetTokenRecordAsync(CurrentSessionId);

        if(tokenRecord == null)
        {
            throw new NoTokenForCurrentSessionIdException();
        }

        if(IsExpired(tokenRecord))
        {
            _logger.LogInformation("Access Token expired, executing token refresh");

            // Refresh the token
            TokenRefreshResponse? tokenRefresh = await _tokenClient.RefreshAsync(tokenRecord.RefreshToken);

            if(tokenRefresh == null)
            {
                throw new TokenRefreshException();
            }

            if (_environment.IsDevelopment())
                _logger.LogInformation($"Received new tokens via token refresh \n " +
                                      $"IdToken: {tokenRefresh.IdToken} \n" +
                                      $"AccessToken: {tokenRefresh.AccessToken} \n" +
                                      $"RefreshToken: {tokenRefresh.RefreshToken}");
            else
                _logger.LogInformation("Received new tokens via token refresh");

            DateTime expiresAt = DateTime.UtcNow.AddSeconds(Convert.ToInt32(tokenRefresh.ExpiresIn));
            await _tokenStore.SaveOrUpdateTokensAsync(CurrentSessionId, tokenRefresh.IdToken, tokenRefresh.AccessToken, tokenRefresh.RefreshToken, expiresAt);
            return tokenRefresh.AccessToken;
        }
        else
        {
            return tokenRecord.AccessToken;
        }
    }

    /// <summary>
    /// Gets the access token claims of the current session asynchronous.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Claim>?> GetAccessTokenClaimsAsync()
    {
        if (CurrentSessionId == null) return null;

        TokenRecord? tokenRecord = await _tokenStore.GetTokenRecordAsync(CurrentSessionId);

        if (tokenRecord == null)
        {
            throw new NoTokenForCurrentSessionIdException();
        }

        JwtSecurityToken accessToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenRecord.AccessToken);

        return accessToken.Claims;
    }

    /// <summary>
    /// Gets the identity claims of the current session asynchronous.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Claim>?> GetIdentityClaimsAsync()
    {
        if (CurrentSessionId == null) return null;

        TokenRecord? tokenRecord = await _tokenStore.GetTokenRecordAsync(CurrentSessionId);

        if (tokenRecord == null)
        {
            throw new NoTokenForCurrentSessionIdException();
        }

        JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenRecord.IdToken);

        return idToken.Claims;
    }

    public async Task<IEnumerable<Claim>?> GetUserinfoClaimsAsync()
    {
        if (CurrentSessionId == null) return null;

        TokenRecord? tokenRecord = await _tokenStore.GetTokenRecordAsync(CurrentSessionId);

        if (tokenRecord == null)
        {
            throw new NoTokenForCurrentSessionIdException();
        }

        if(tokenRecord.UserinfoClaims == null)
        {
            return null;
        }

        Dictionary<string, JsonElement>? userinfos = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tokenRecord.UserinfoClaims);
        return userinfos?.Select(userinfo => new Claim(userinfo.Key, userinfo.Value.ToString())).ToList();
    }

    /// <summary>
    /// Determines whether the specified tokent record is expired.
    /// </summary>
    /// <param name="tokentRecord">The tokent record.</param>
    /// <returns>
    ///   <c>true</c> if the specified tokent record is expired; otherwise, <c>false</c>.
    /// </returns>
    private bool IsExpired(TokenRecord tokentRecord)
    {
        return tokentRecord.ExpiresAt.Subtract(DateTime.UtcNow).TotalSeconds < 30;
    }
}
