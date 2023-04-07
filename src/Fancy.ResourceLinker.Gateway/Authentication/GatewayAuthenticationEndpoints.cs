using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// Class to add endpoints to the web application specific for the authentication feature.
/// </summary>
internal static class GatewayAuthenticationEndpoints
{
    /// <summary>
    /// Adds the required gateway authentication endpoints to the middleware pipeline for the authentication feature.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    internal static void UseGatewayAuthenticationEndpoints(this WebApplication webApp)
    {
        webApp.UseUserInfoEndpoint();
        webApp.UseLoginEndpoint();
        webApp.UseLogoutEndpoint();
    }

    /// <summary>
    /// Adds the login endpoint to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    private static void UseLoginEndpoint(this WebApplication webApp)
    {
        webApp.MapGet("/login", (string? redirectUrl, HttpContext context) =>
        {

            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "/";
            }

            AuthenticationProperties authProps = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
        });
    }

    /// <summary>
    /// Adds the logout endpoint to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    private static void UseLogoutEndpoint(this WebApplication webApp)
    {
        webApp.MapGet("/logout", (string? redirectUrl, HttpContext context) =>
        {
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "/";
            }

            var authProps = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            var authSchemes = new string[] {
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            };

            return Results.SignOut(authProps, authSchemes);
        });
    }

    /// <summary>
    /// Adds the user info endpoint to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    private static void UseUserInfoEndpoint(this WebApplication webApp)
    {
        webApp.MapGet("/userinfo", async (TokenService tokenService) =>
        {
            IEnumerable<Claim>? claims = await tokenService.GetIdentityClaimsAsync();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if(claims == null)
            {
                return Results.Unauthorized();
            }

            foreach (var entry in claims)
            {
                dictionary[entry.Type] = entry.Value;
            }

            return Results.Ok(dictionary);
        });
    }
}
