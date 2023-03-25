using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

internal class DbTokenStore : ITokenStore
{
    private readonly GatewayDbContext _dbContext;

    public DbTokenStore(GatewayDbContext dbContext) 
    {
        _dbContext = dbContext;
    }

    public async Task<TokenRecord?> GetTokensAsync(string userId)
    {
        TokenSet tokenSet = await _dbContext.TokenSets.SingleOrDefaultAsync(ts => ts.UserId == userId);

        if (tokenSet == null) { return null; }

        return new TokenRecord
        {
            UserId = userId,
            IdToken = tokenSet.IdToken,
            AccessToken = tokenSet.AccessToken,
            RefreshToken = tokenSet.RefreshToken,
            ExpiresAt = tokenSet.ExpiresAt
        };
    }

    public async Task SaveOrUpdateTokensAsync(string userId, string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        TokenSet tokenSet = await _dbContext.TokenSets.SingleOrDefaultAsync(ts => ts.UserId == userId);

        if(tokenSet == null) 
        {
            tokenSet.UserId = userId;
            _dbContext.TokenSets.Add(tokenSet);
        }

        tokenSet.IdToken = idToken;
        tokenSet.AccessToken = accessToken;
        tokenSet.RefreshToken = refreshToken;

        await _dbContext.SaveChangesAsync();
    }
}
