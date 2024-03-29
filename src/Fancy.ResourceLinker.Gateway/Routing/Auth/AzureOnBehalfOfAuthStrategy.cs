using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// A model to deserialize the on behalf of token response.
/// </summary>
class OnBehalfOfTokenResponse
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    /// <value>
    /// The access token.
    /// </value>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    /// <value>
    /// The refresh token.
    /// </value>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    /// <summary>
    /// Gets or sets the expires in.
    /// </summary>
    /// <value>
    /// The expires in.
    /// </value>
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the expires at.
    /// </summary>
    /// <value>
    /// The expires at.
    /// </value>
    [JsonIgnore]
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// An auth strategy which just passes through the current token
/// </summary>
internal class AzureOnBehalfOfAuthStrategy : IRouteAuthenticationStrategy
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
    /// The discovery document service.
    /// </summary>
    private readonly DiscoveryDocumentService _discoveryDocumentService;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<AzureOnBehalfOfAuthStrategy> _logger;

    /// <summary>
    /// The gateway authentication settings.
    /// </summary>
    private GatewayAuthenticationSettings? _gatewayAuthenticationSettings;

    /// <summary>
    /// The route authentication settings.
    /// </summary>
    private RouteAuthenticationSettings? _routeAuthenticationSettings;

    /// <summary>
    /// The discovery document.
    /// </summary>
    private DiscoveryDocument? _discoveryDocument;

    /// <summary>
    /// The current token response.
    /// </summary>
    private OnBehalfOfTokenResponse? _currentTokenResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOnBehalfOfAuthStrategy"/> class.
    /// </summary>
    /// <param name="discoveryDocumentService">The discovery document service.</param>
    /// <param name="logger">The logger.</param>
    public AzureOnBehalfOfAuthStrategy(DiscoveryDocumentService discoveryDocumentService, ILogger<AzureOnBehalfOfAuthStrategy> logger)
    {
        _discoveryDocumentService = discoveryDocumentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name => NAME;

    /// <summary>
    /// Initializes the authentication strategy based on the gateway authentication settings and the route authentication settings asynchronous.
    /// </summary>
    /// <param name="gatewayAuthenticationSettings">The gateway authentication settigns.</param>
    /// <param name="routeAuthenticationSettings">The route authentication settigns.</param>
    public async Task InitializeAsync(GatewayAuthenticationSettings? gatewayAuthenticationSettings, RouteAuthenticationSettings routeAuthenticationSettings)
    {
        _gatewayAuthenticationSettings = gatewayAuthenticationSettings;

        if(_gatewayAuthenticationSettings == null)
        {
            throw new InvalidOperationException($"The {NAME} route authentication strategy needs to have the gateway authenticaion configured");
        }

        if(string.IsNullOrEmpty(_gatewayAuthenticationSettings.ClientId) || string.IsNullOrEmpty(_gatewayAuthenticationSettings.ClientSecret))
        {
            throw new InvalidOperationException($"The {NAME} route authentication strategy needs to have set the 'ClientId' and 'Client Secret' settings at the gateway authentication settings.");
        }

        _routeAuthenticationSettings = routeAuthenticationSettings;
        _discoveryDocument = await _discoveryDocumentService.LoadDiscoveryDocumentAsync(_gatewayAuthenticationSettings.Authority);
    }

    /// <summary>
    /// Sets the authentication to an http context asynchronous.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation
    /// </returns>
    public async Task SetAuthenticationAsync(HttpContext context)
    {
        string accessToken = await GetAccessTokenAsync(context.RequestServices);
        context.Request.Headers.Authorization =  new StringValues("Bearer " + accessToken);
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
        string accessToken = await GetAccessTokenAsync(serviceProvider);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Gets the access token asynchronous.
    /// </summary>
    /// <returns>The access token.</returns>
    private async Task<string> GetAccessTokenAsync(IServiceProvider serviceProvider)
    {
        if (_currentTokenResponse == null || IsExpired(_currentTokenResponse))
        {
            TokenService? tokenService = TryGetTokenService(serviceProvider);

            if (tokenService == null)
            {
                throw new InvalidOperationException("A token service in a scope is needed to use the azure on behalf of flow");
            }

            _currentTokenResponse = await GetOnBehalfOfTokenAsync(tokenService);
        }

        return _currentTokenResponse.AccessToken;
    }

    /// <summary>
    /// Gets a token via an on behalf of flow asynchronous.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The exchanged token.</returns>
    private async Task<OnBehalfOfTokenResponse> GetOnBehalfOfTokenAsync(TokenService tokenService)
    {
        if(_gatewayAuthenticationSettings == null || _routeAuthenticationSettings == null)
        {
            throw new InvalidOperationException("Initialize the auth strategy first before using it");
        }

        string accessToken = await tokenService.GetAccessTokenAsync();

        var payload = new Dictionary<string, string>
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
            { "client_id", _gatewayAuthenticationSettings.ClientId },
            { "client_secret", _gatewayAuthenticationSettings.ClientSecret },
            { "assertion", accessToken },
            { "scope", _routeAuthenticationSettings.Options["Scope"] },
            { "requested_token_use", "on_behalf_of" },
        };

        FormUrlEncodedContent content = new FormUrlEncodedContent(payload);
        HttpResponseMessage httpResponse = await _httpClient.PostAsync(_discoveryDocument?.TokenEndpoint, content);
            
        try
        {
            httpResponse.EnsureSuccessStatusCode();
        }
        catch(HttpRequestException)
        {
            string errorResponse = new StreamReader(httpResponse.Content.ReadAsStream()).ReadToEnd();
            _logger.LogError("Error on on behalf of token exchange. Response from server " + errorResponse);
            throw;
        }

        OnBehalfOfTokenResponse? result = await httpResponse.Content.ReadFromJsonAsync<OnBehalfOfTokenResponse>();

        if(result == null) 
        {
            throw new InvalidOperationException("Could not deserialize token response from azure on behalf of flow");
        }

        result.ExpiresAt = DateTime.UtcNow.AddSeconds(Convert.ToInt32(result.ExpiresIn));

        return result;
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

    /// Determines whether the specified token response is expired.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>
    ///   <c>true</c> if the specified response is expired; otherwise, <c>false</c>.
    /// </returns>
    private bool IsExpired(OnBehalfOfTokenResponse response)
    {
        return response.ExpiresAt.Subtract(DateTime.UtcNow).TotalSeconds < 30;
    }
}
