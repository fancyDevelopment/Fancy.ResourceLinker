namespace Fancy.ResourceLinker.Gateway.Authentication;

public interface ITokenStore
{
    Task SaveOrUpdateTokensAsync(string userId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt);

    Task<TokenRecord?> GetTokensAsync(string userId);
}