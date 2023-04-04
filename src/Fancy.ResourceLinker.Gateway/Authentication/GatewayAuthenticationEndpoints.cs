using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fancy.ResourceLinker.Gateway.Authentication;

public static class GatewayAuthenticationEndpoints
{
    public static void UseGatewayAuthenticationEndpoints(this WebApplication app)
    {
        app.UseUserInfoEndpoint();
        app.UseLoginEndpoint();
        app.UseLogoutEndpoint();
    }

    private static void UseLoginEndpoint(this WebApplication app)
    {
        app.MapGet("/login", (string? redirectUrl, HttpContext ctx) =>
        {

            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "/";
            }

            AuthenticationProperties authProps = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
        });
    }

    private static void UseLogoutEndpoint(this WebApplication app)
    {
        app.MapGet("/logout", (string? redirectUrl, HttpContext ctx) =>
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

    private static void UseUserInfoEndpoint(this WebApplication app)
    {
        app.MapGet("/userinfo", async (TokenService tokenService) =>
        {
            var claims = await tokenService.GetIdentityClaimsAsync();
            var dictionary = new Dictionary<string, string>();

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
