using Fancy.ResourceLinker.Gateway;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace Fancy.ResourceLinker.Gateway.Authentication;

// ToDo: think about token exchange

/// <summary>
/// Class with helper methods to set up authentication feature.
/// </summary>
internal static class GatewayAuthentication
{
    /// <summary>
    /// The authentication policy name.
    /// </summary>
    internal static readonly string AuthenticationPolicyName = "GatewayEnforceAuthentication";

    /// <summary>
    /// Adds the required services for the gateway authentication feature to the ioc container.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="settings">The settings.</param>
    internal static void AddGatewayAuthentication(IServiceCollection services, GatewayAuthenticationSettings settings)
    {
        services.AddSingleton<DiscoveryDocumentService>();
        services.AddSingleton<TokenClient>();
        services.AddScoped<TokenService>();

        services.AddAuthorization(options =>
        {
            // Add the authentication policy for routes which are marked with 'EnforceAuthentication'
            options.AddPolicy(AuthenticationPolicyName, new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        });

        // Add required authentication services
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(settings.SessionTimeoutInMin);
            options.SlidingExpiration = true;
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
            //options.TokenValidationParameters.NameClaimType = settings.UniqueIdentifierClaimType;

            foreach (string scope in settings.AuthorizationCodeScopes.Split(' '))
            {
                options.Scope.Add(scope);
            }

            options.Events.OnTokenValidated = async (context) =>
            {
                if (context.TokenEndpointResponse == null) throw new Exception("No token response available");

                // Provide new token to token service
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<TokenService>();
                string sesstionId = await tokenService.SaveTokenForNewSessionAsync(context.TokenEndpointResponse);
                context.HttpContext.Items.Add("TokenSessionId", sesstionId);
            };

            options.Events.OnTicketReceived = (context) =>
            {
                string? tokenSessionId = context.HttpContext.Items["TokenSessionId"] as string;

                if (!string.IsNullOrWhiteSpace(tokenSessionId))
                {
                    // Setup a new default identity which only contians the token session id
                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("TokenSessionId", tokenSessionId) }, context.Principal?.Identity?.AuthenticationType));
                }

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToIdentityProvider = context =>
            {
                // If the request was made 'from Code aka from javascript' return unauthorized instead of the default redirect
                // to make visible to a javascript that the authentication is not valid (anymore)
                if(context.Request.Headers["X-Requested-With"] == "XmlHttpRequest")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.HandleResponse();
                }
                return Task.CompletedTask;
            };
        });
    }

    /// <summary>
    /// Adds the required gateway authentication middlewares to the middleware pipeline for the authentication feature.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    internal static void UseGatewayAuthentication(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        //app.UseCookiePolicy();

        // Custom Middleware to read current user into token service
        app.Use(async (context, next) =>
        {
            TokenService? tokenService = context.RequestServices.GetService<TokenService>();

            if (tokenService != null)
            {
                tokenService.CurrentSessionId = context.User.Claims.SingleOrDefault(c => c.Type == "TokenSessionId")?.Value;
                
                // Add the identity from the current valid token
                context.User.AddIdentity(new ClaimsIdentity(await tokenService.GetIdentityClaimsAsync(), "Gateway"));
            }

            try
            {
                await next(context);
            }
            catch(TokenRefreshException)
            {
                // If a token refresh fails during the following processing of the request, we send the user back a challange result.
                await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            }
            
        });
    }
}
