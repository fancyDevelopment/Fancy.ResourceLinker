using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway.AntiForgery;

/// <summary>
/// Class with helper methods to set up anti forgery feature.
/// </summary>
internal static class GatewayAntiForgery
{
    /// <summary>
    /// Adds the gateway anti forgery service to the ioc container.
    /// </summary>
    /// <param name="services">The services.</param>
    internal static void AddGatewayAntiForgery(IServiceCollection services)
    {
        services.AddAntiforgery(setup =>
        {
            setup.HeaderName = "X-XSRF-TOKEN";
        });
    }

    /// <summary>
    /// Adds the gateway anti forgery middleware to the middleware pipeline.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    internal static void UseGatewayAntiForgery(WebApplication webApp)
    {
        webApp.UseXsrfCookieCreator();
        webApp.UseXsrfHeaderChecks();
    }

    /// <summary>
    /// Adds a middleware to create XSRF cookies.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    private static void UseXsrfCookieCreator(this WebApplication webApp)
    {
        webApp.Use((context, next) =>
        {
            IAntiforgery? antiforgery = webApp.Services.GetService<IAntiforgery>();

            if (antiforgery == null)
            {
                throw new InvalidOperationException("IAntiforgery service exptected! Call 'AddAntiForgery' on the gateway builder to add required services!");
            }

            AntiforgeryTokenSet tokens = antiforgery!.GetAndStoreTokens(context);

            if (tokens.RequestToken == null)
            {
                throw new InvalidOperationException("Antiforgery request token exptected!");
            }

            // Add a XSRF Cookie to the response
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });

            return next(context);
        });
    }

    /// <summary>
    /// Adds a middleware to check XSRF headers.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    private static void UseXsrfHeaderChecks(this WebApplication webApp)
    {
        webApp.Use(async (context, next) =>
        {
            IAntiforgery? antiforgery = webApp.Services.GetService<IAntiforgery>();

            if (antiforgery == null)
            {
                throw new InvalidOperationException("IAntiforgery service exptected! Call 'AddAntiForgery' on the gateway builder to add required services!");
            }

            if (!await antiforgery.IsRequestValidAsync(context))
            {
                // Return an error response
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { Error = "XSRF token validadation failed" });
                return;
            }

            await next(context);
        });
    }
}
