using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fancy.ResourceLinker.Gateway.Authentication;

internal class TokenRefreshException : Exception
{
}

internal class TokenService
{
    private readonly ITokenStore _tokenStore;
    private readonly TokenClient _tokenClient;

    public TokenService(ITokenStore tokenStore, TokenClient tokenClient) 
    {
        _tokenStore = tokenStore;
        _tokenClient = tokenClient;
    }

    public string? CurrentSessionId { get; set; }

    public async Task<string> SaveTokenForNewSessionAsync(TokenValidatedContext tokenContext)
    {
        // Create a new guid for the new session
        string sessionId = Guid.NewGuid().ToString();
        await SaveOrUpdateTokenAsync (sessionId, tokenContext);
        return sessionId;
    }

    public async Task SaveOrUpdateTokenAsync(string sessionId, TokenValidatedContext tokenContext)
    {
        string idToken = tokenContext.TokenEndpointResponse.IdToken;
        string accessToken = tokenContext.TokenEndpointResponse.AccessToken;
        string refreshToken = tokenContext.TokenEndpointResponse.RefreshToken;
        DateTimeOffset expiresAt = new DateTimeOffset(DateTime.Now).AddSeconds(Convert.ToInt32(tokenContext.TokenEndpointResponse.ExpiresIn));

        await _tokenStore.SaveOrUpdateTokensAsync(sessionId, idToken, accessToken, refreshToken, expiresAt);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if(CurrentSessionId == null)
        {
            var tokenResponse = await _tokenClient.GetTokenViaClientCredentialsAsync();
            return tokenResponse.AccessToken;
        }

        TokenRecord? tokenRecord = await _tokenStore.GetTokensAsync(CurrentSessionId);

        if(tokenRecord == null)
        {
            throw new InvalidOperationException("No token for session with id  " + CurrentSessionId + " available");
        }

        if(IsExpired(tokenRecord))
        {
            // Refresh the token
            TokenRefreshResponse tokenRefresh = await _tokenClient.RefreshAsync(tokenRecord.RefreshToken);

            if(tokenRefresh == null)
            {
                throw new TokenRefreshException();
            }

            DateTimeOffset expiresAt = new DateTimeOffset(DateTime.Now).AddSeconds(Convert.ToInt32(tokenRefresh.ExpiresIn));
            await _tokenStore.SaveOrUpdateTokensAsync(CurrentSessionId, tokenRefresh.IdToken, tokenRefresh.AccessToken, tokenRefresh.RefreshToken, expiresAt);
            return tokenRefresh.AccessToken;
        }
        else
        {
            return tokenRecord.AccessToken;
        }
    }

    public async Task<IEnumerable<Claim>> GetIdentityClaimsAsync()
    {
        if (CurrentSessionId == null) return null;

        TokenRecord? tokenRecord = await _tokenStore.GetTokensAsync(CurrentSessionId);

        if (tokenRecord == null)
        {
            throw new InvalidOperationException("No token for user " + CurrentSessionId + " available");
        }

        JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenRecord.IdToken);

        return idToken.Claims;
    }

    private bool IsExpired(TokenRecord tokentRecord)
    {
        return tokentRecord.ExpiresAt.Subtract(DateTimeOffset.UtcNow).TotalSeconds < 30;
    }
}
