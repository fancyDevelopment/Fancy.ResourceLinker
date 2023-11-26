using Fancy.ResourceLinker.Gateway;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace Fancy.ResourceLinker.Gateway.Authentication;

// ToDo: think about token exchange

/// <summary>
/// Class with helper methods to set up authentication feature.
/// </summary>
internal sealed class GatewayAuthentication
{
    /// <summary>
    /// The settings.
    /// </summary>
    private static GatewayAuthenticationSettings _settings = null!;

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
        _settings = settings;
        services.AddSingleton<DiscoveryDocumentService>();
        services.AddSingleton<TokenClient>();
        services.AddScoped<TokenService>();
        services.AddHostedService<TokenCleanupBackgroundService>();

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
            options.TokenValidationParameters.NameClaimType = settings.UniqueIdentifierClaimType;

            options.Scope.Clear();
            foreach (string scope in settings.AuthorizationCodeScopes.Split(' '))
            {
                options.Scope.Add(scope);
            }

            options.Events.OnTokenValidated = async (context) =>
            {
                if (context.TokenEndpointResponse == null) throw new Exception("No token response available");

                IWebHostEnvironment environment = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GatewayAuthentication>>();

                if (environment.IsDevelopment())
                    logger.LogInformation($"Received valid token via authorization flow \n " +
                                          $"IdToken: {context.TokenEndpointResponse.IdToken} \n" +
                                          $"AccessToken: {context.TokenEndpointResponse.AccessToken} \n" +
                                          $"RefreshToken: {context.TokenEndpointResponse.RefreshToken}");
                else
                    logger.LogInformation("Received valid token via authorization flow");

                // Provide new token to token service
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<TokenService>();
                string sesstionId = await tokenService.SaveTokenForNewSessionAsync(context.TokenEndpointResponse);
                context.HttpContext.Items.Add("TokenSessionId", sesstionId);
            };

            options.Events.OnTicketReceived = (context) =>
            {
                string? tokenSessionId = context.HttpContext.Items["TokenSessionId"] as string;

                if (!string.IsNullOrWhiteSpace(tokenSessionId) && context.Principal != null)
                {
                    Claim sessionIdClaim = new Claim("TokenSessionId", tokenSessionId);
                    Claim? uniqueNameClaim = context.Principal.Claims.SingleOrDefault(c => c.Type == settings.UniqueIdentifierClaimType);

                    if(uniqueNameClaim == null)
                    {
                        ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GatewayAuthentication>>();
                        logger.LogError($"No claim was found with the specified unique identifier type '{settings.UniqueIdentifierClaimType}'");
                        throw new Exception($"No claim was found with the specified unique identifier type '{settings.UniqueIdentifierClaimType}'");
                    }

                    // Setup a new default identity which only contians the token session id
                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { sessionIdClaim, uniqueNameClaim }, context.Principal?.Identity?.AuthenticationType, settings.UniqueIdentifierClaimType, null));
                }

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToIdentityProvider = context =>
            {
                ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GatewayAuthentication>>();
                logger.LogInformation("Starting authorization flow for request to: " + context.HttpContext.Request.GetDisplayUrl());
                // If the request was made 'from Code aka from javascript' return unauthorized instead of the default redirect
                // to make visible to a javascript that the authentication is not valid (anymore)
                if(context.Request.Headers["X-Requested-With"] == "XmlHttpRequest")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.HandleResponse();
                }
                return Task.CompletedTask;
            };

            // ToDo: Get this from configuration
            options.Events.OnRedirectToIdentityProviderForSignOut = context =>
            {
                if (settings.IssuerAddressForSignOut != null)
                {
                    context.ProtocolMessage.IssuerAddress = settings.IssuerAddressForSignOut;
                }

                context.ProtocolMessage.ClientId = settings.ClientId;

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
        // Add the default asp.net core middlewares for authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();
        //app.UseCookiePolicy();

        // Custom Middleware to read current user into token service
        app.Use(async (context, next) =>
        {
            TokenService tokenService = context.RequestServices.GetRequiredService<TokenService>();
            string? currentSessionId = context.User.Claims.SingleOrDefault(c => c.Type == "TokenSessionId")?.Value;

            try
            {
                // Add the identity from the token only if we still have a valid session
                if (!string.IsNullOrEmpty(currentSessionId))
                {
                    tokenService.CurrentSessionId = currentSessionId;

                    // Add the identity from the current valid token
                    context.User.AddIdentity(new ClaimsIdentity(await tokenService.GetAccessTokenClaimsAsync(), "Gateway", _settings.UniqueIdentifierClaimType, "roles"));
                }

                await next(context);
            }
            catch(TokenServiceException e)
            {
                ILogger<GatewayAuthentication> logger = context.RequestServices.GetRequiredService<ILogger<GatewayAuthentication>>();
                logger.LogInformation(e.GetType().Name + " during token service operation, rechallenging authorization");

                // If something went wrong with the token service we indicate unauthorized for api clients or challange a new authentication flow
                if (context.Request.Headers["X-Requested-With"] == "XmlHttpRequest")
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                else
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            }
            
        });
    }
}
