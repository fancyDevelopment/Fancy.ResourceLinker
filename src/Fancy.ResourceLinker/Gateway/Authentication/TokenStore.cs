using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker.Gateway.Authentication
{
    internal class TokenRecord
    {
        public string UserId { get; set; }
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }

    internal class InMemoryTokenStore : ITokenStore
    {
        private Dictionary<string, TokenRecord> _tokens = new Dictionary<string, TokenRecord>();

        public Task SaveOrUpdateTokensAsync(string userId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiration)
        {
            _tokens[userId] = new TokenRecord
            {
                UserId = userId,
                IdToken = idToken,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiration
            };
            return Task.CompletedTask;
        }

        public Task<TokenRecord?> GetTokensAsync(string userId)
        {
            if (_tokens.ContainsKey(userId)) return Task.FromResult(_tokens[userId]);
            else return Task.FromResult<TokenRecord>(null);
        }
    }
}
