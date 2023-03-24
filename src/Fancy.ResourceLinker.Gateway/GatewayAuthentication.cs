using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;


namespace Fancy.ResourceLinker.Gateway;

public static class GatewayAuthentication
{
    public static readonly string AuthenticationPolicyName = "GatewayEnforceAuthentication";

    public static void AddGatewayAuthentication(this IServiceCollection services, GatewayAuthenticationSettings settings)
    {
        services.AddSingleton<DiscoveryDocumentService>();
        services.AddSingleton<TokenRefreshService>();
        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddScoped<TokenService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthenticationPolicyName, new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(setup =>
        {
            setup.ExpireTimeSpan = TimeSpan.FromMinutes(settings.SessionTimeoutInMin);
            setup.SlidingExpiration = true;
        })
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = settings.Authority;
            options.ClientId = settings.ClientId;
            options.UsePkce = true;
            options.ClientSecret = settings.ClientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = settings.QueryUserInfoEndpoint;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.NameClaimType = settings.UniqueIdentifierClaimType;

            foreach (var scope in settings.Scopes)
            {
                options.Scope.Add(scope);
            }

            options.Events.OnTokenValidated = (context) =>
            {
                // Provide new token to token service
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<TokenService>();
                return tokenService.SaveOrUpdateTokensAsync(context);
            };

            options.Events.OnTicketReceived = (context) =>
            {
                // Remove all claims to keep the auth cookie as small as possible
                ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    foreach (var claimType in identity.Claims.Select(c => c.Type).Where(t => t != settings.UniqueIdentifierClaimType).ToList())
                    {
                        identity.RemoveClaim(identity.Claims.First(c => c.Type == claimType));
                    }
                }

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToIdentityProviderForSignOut = (context) =>
            {
                // ToDo: Logout Handler
                // LogoutHandler.HandleLogout(context, config);
                return Task.CompletedTask;
            };
        });
    }

    public static void UseGatewayAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCookiePolicy();

        // Custom Middleware to read current user into token service
        app.Use(async (ctx, next) =>
        {
            TokenService tokenService = ctx.RequestServices.GetService<TokenService>();

            if (tokenService != null)
            {
                tokenService.CurrentUser = ctx.User.Identity.Name;
            }

            await next(ctx);
        });
    }
}
