using System;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker.Gateway.Authentication
{
    internal interface ITokenStore
    {
        Task SaveOrUpdateTokensAsync(string userId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt);

        Task<TokenRecord?> GetTokensAsync(string userId);
    }
}