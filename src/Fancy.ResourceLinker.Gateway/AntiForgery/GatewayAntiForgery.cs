using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.AntiForgery;

internal static class GatewayAntiForgery
{
    public static void AddGatewayAntiForgery(IServiceCollection services)
    {
        services.AddAntiforgery(setup =>
        {
            setup.HeaderName = "X-XSRF-TOKEN";
        });
    }

    public static void UseGatewayAntiForgery(WebApplication app)
    {
        app.UseXsrfCookieCreator();
        app.UseXsrfCookieChecks();
    }

    private static void UseXsrfCookieCreator(this WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            var antiforgery = app.Services.GetService<IAntiforgery>();

            if (antiforgery == null)
            {
                throw new Exception("IAntiforgery service exptected! Call AddGatewayAntiForgery to add required services!");
            }

            var tokens = antiforgery!.GetAndStoreTokens(ctx);

            if (tokens.RequestToken == null)
            {
                throw new Exception("token exptected!");
            }

            ctx.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
                        new CookieOptions() { HttpOnly = false });

            await next(ctx);
        });
    }

    private static void UseXsrfCookieChecks(this WebApplication app)
    {
        //var config = app.Services.GetRequiredService<GatewayConfig>();
        //var apiConfigs = config.ApiConfigs;

        app.Use(async (ctx, next) =>
        {
            var antiforgery = app.Services.GetService<IAntiforgery>();

            if (antiforgery == null)
            {
                throw new Exception("IAntiforgery service exptected!");
            }

            var currentUrl = ctx.Request.Path.ToString().ToLower();
            //if (apiConfigs.Any(c => currentUrl.StartsWith(c.ApiPath)))
            // ToDo: Find a better way when to use antiforgery by using routing configuration and local endpoints
            if (currentUrl.StartsWith("/api"))
            {
                if (!await antiforgery.IsRequestValidAsync(ctx))
                {
                    ctx.Response.StatusCode = 400;
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        Error = "XSRF token validadation failed"
                    });
                    return;
                }
            }

            await next(ctx);
        });
    }
}
