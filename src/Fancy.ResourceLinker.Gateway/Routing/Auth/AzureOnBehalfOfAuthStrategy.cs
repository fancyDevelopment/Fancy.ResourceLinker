using Fancy.ResourceLinker.Gateway.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;


class TokenExchangeResponse
{
    public string access_token { get; set; } = "";
    public string refresh_token { get; set; } = "";
    public long expires_in { get; set; }
}

/// <summary>
/// An auth strategy which just passes through the current token
/// </summary>
internal class AzureOnBehalfOfAuthStrategy : IAuthStrategy
{
    /// <summary>
    /// The name of the auth strategy.
    /// </summary>
    public const string NAME = "AzureOnBehalfOf";

    /// <summary>
    /// The HTTP client.
    /// </summary>
    private readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// The route authentication settings.
    /// </summary>
    private readonly RouteAuthenticationSettings _routeAuthenticationSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenPassThroughAuthStrategy"/> class.
    /// </summary>
    /// <param name="tokenService">The token service.</param>
    public AzureOnBehalfOfAuthStrategy(RouteAuthenticationSettings routeAuthenticationSettings)
    {
        _routeAuthenticationSettings = routeAuthenticationSettings;
    }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name => NAME;

    /// <summary>
    /// Sets the authentication to an http context asynchronous.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public async Task SetAuthenticationAsync(HttpContext context)
    {
        TokenService? tokenService = TryGetTokenService(context.RequestServices);

        if (tokenService != null)
        {
            string? accessToken = await tokenService.GetAccessTokenAsync();
            context.Request.Headers.Add("Authorization", "Bearer " + accessToken);
        }
    }

    /// <summary>
    /// Sets the authentication to an http request message asynchronous.
    /// </summary>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="request">The http request message.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public async Task SetAuthenticationAsync(IServiceProvider serviceProvider, HttpRequestMessage request)
    {
        string? accessToken = await TryGetOnBehalfOfTokenAsync(serviceProvider);

        if (accessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private async Task<string?> TryGetOnBehalfOfTokenAsync(IServiceProvider serviceProvider)
    {
        DiscoveryDocumentService discoveryDocumentService = serviceProvider.GetRequiredService<DiscoveryDocumentService>();
        GatewayAuthenticationSettings authSettings = serviceProvider.GetRequiredService<GatewayAuthenticationSettings>();
        var disoveryDocument = await discoveryDocumentService.LoadDiscoveryDocumentAsync(authSettings.Authority);

        TokenService? tokenService = TryGetTokenService(serviceProvider);
        string? accessToken = await tokenService?.GetAccessTokenAsync();

        if (accessToken != null)
        {
            var payload = new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "client_id", _routeAuthenticationSettings.Options["ClientId"] },
                { "client_secret", _routeAuthenticationSettings.Options["ClientSecret"] },
                { "assertion", accessToken },
                { "scope", _routeAuthenticationSettings.Options["Scope"] },
                { "requested_token_use", "on_behalf_of" },
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(payload);
            HttpResponseMessage httpResponse = await _httpClient.PostAsync(disoveryDocument.TokenEndpoint, content);
            
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException)
            {
                ILogger<AzureOnBehalfOfAuthStrategy> logger = serviceProvider.GetRequiredService<ILogger<AzureOnBehalfOfAuthStrategy>>();
                string errorResponse = new StreamReader(httpResponse.Content.ReadAsStream()).ReadToEnd();
                logger.LogError("Error on on behalf of token exchange. Response from server " + errorResponse);
                throw;
            }

            TokenExchangeResponse? response = await httpResponse.Content.ReadFromJsonAsync<TokenExchangeResponse>();
            return response?.access_token;
        }
        return null;
    }

    /// <summary>
    /// Tries to get the token service.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>An instance of the current token service or null if no token service is availalbe.</returns>
    private TokenService? TryGetTokenService(IServiceProvider serviceProvider)
    {
        try
        {
            return serviceProvider.GetService<TokenService>();
        }
        catch (InvalidOperationException)
        {
            // No token service available
            return null;
        }
    }
}

