using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// Class to hold the result of a client credentials token response.
/// </summary>
class ClientCredentialsTokenResponse
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
/// A route authentication strategy running the client credential flow. 
/// </summary>
internal class ClientCredentialsAuthStrategy : IRouteAuthenticationStrategy
{
    /// <summary>
    /// The name of the auth strategy.
    /// </summary>
    public const string NAME = "ClientCredentials";

    /// <summary>
    /// The discovery document service.
    /// </summary>
    private readonly DiscoveryDocumentService _discoveryDocumentService;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<ClientCredentialsAuthStrategy> _logger;

    /// <summary>
    /// The discovery document.
    /// </summary>
    protected DiscoveryDocument? _discoveryDocument;

    /// <summary>
    /// The client identifier.
    /// </summary>
    protected string _clientId = string.Empty;

    /// <summary>
    /// The client secret.
    /// </summary>
    protected string _clientSecret = string.Empty;

    /// <summary>
    /// The scope.
    /// </summary>
    protected string _scope = string.Empty;

    /// <summary>
    /// The is initialized.
    /// </summary>
    protected bool _isInitialized = false;

    /// <summary>
    /// The current token response.
    /// </summary>
    protected ClientCredentialsTokenResponse? _currentTokenResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsAuthStrategy" /> class.
    /// </summary>
    /// <param name="discoveryDocumentService">The discovery document service.</param>
    /// <param name="logger">The logger.</param>
    public ClientCredentialsAuthStrategy(DiscoveryDocumentService discoveryDocumentService, ILogger<ClientCredentialsAuthStrategy> logger)
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
    public virtual string Name => NAME;

    /// <summary>
    /// Initializes the authentication strategy based on the gateway authentication settings and the route authentication settings asynchronous.
    /// </summary>
    /// <param name="gatewayAuthenticationSettings">The gateway authentication settigns.</param>
    /// <param name="routeAuthenticationSettings">The route authentication settigns.</param>
    public virtual async Task InitializeAsync(GatewayAuthenticationSettings? gatewayAuthenticationSettings, RouteAuthenticationSettings routeAuthenticationSettings)
    {
        string authority;

        if(routeAuthenticationSettings.Options.ContainsKey("Authority"))
        {
            authority = routeAuthenticationSettings.Options["Authority"];
        } 
        else if(!string.IsNullOrEmpty(gatewayAuthenticationSettings?.Authority))
        {
            authority = gatewayAuthenticationSettings.Authority;
        }
        else
        {
            throw new InvalidOperationException("Either the route authentication settings or the gateway authentication settings need to have an 'Authority' set");
        }

        if (routeAuthenticationSettings.Options.ContainsKey("ClientId"))
        {
            _clientId = routeAuthenticationSettings.Options["ClientId"];
        }
        else if (!string.IsNullOrEmpty(gatewayAuthenticationSettings?.ClientId))
        {
            _clientId = gatewayAuthenticationSettings.ClientId;
        }
        else
        {
            throw new InvalidOperationException("Either the route authentication settings or the gateway authentication settings need to have a 'ClientId' set");
        }

        if (routeAuthenticationSettings.Options.ContainsKey("ClientSecret"))
        {
            _clientSecret = routeAuthenticationSettings.Options["ClientSecret"];
        }
        else if (!string.IsNullOrEmpty(gatewayAuthenticationSettings?.ClientSecret))
        {
            _clientSecret = gatewayAuthenticationSettings.ClientSecret;
        }
        else
        {
            throw new InvalidOperationException("Either the route authentication settings or the gateway authentication settings need to have a 'ClientId' set");
        }

        if (routeAuthenticationSettings.Options.ContainsKey("Scope"))
        {
            _scope = routeAuthenticationSettings.Options["Scope"];
        }
        else
        {
            throw new InvalidOperationException("The scope needs to be set at the route authentication settings");
        }

        _discoveryDocument = await _discoveryDocumentService.LoadDiscoveryDocumentAsync(authority);

        _isInitialized = true;
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
        if(!_isInitialized)
        {
            throw new InvalidOperationException("Call initialize first before using this instance to set authorization headers");
        }

        string accessToken = await GetAccessTokenAsync();
        context.Request.Headers.Authorization = new StringValues("Bearer " + accessToken);
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
        if(!_isInitialized)
        {
            throw new InvalidOperationException("Call initialize first before using this instance to set authorization headers");
        }

        string accessToken = await GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Sets up the token request.
    /// </summary>
    /// <returns>The token request.</returns>
    protected virtual Dictionary<string, string> SetUpTokenRequest()
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "scope", _scope }
        };
    }

    /// <summary>
    /// Gets the access token asynchronous.
    /// </summary>
    /// <returns>The access token.</returns>
    private async Task<string> GetAccessTokenAsync()
    {
        if(_currentTokenResponse == null || IsExpired(_currentTokenResponse))
        {
            _currentTokenResponse = await GetTokenViaClientCredentialsAsync();
        }

        return _currentTokenResponse.AccessToken;
    }

    /// <summary>
    /// Gets the token via client credentials asynchronous.
    /// </summary>
    /// <returns></returns>
    private async Task<ClientCredentialsTokenResponse> GetTokenViaClientCredentialsAsync()
    {
        Dictionary<string, string> payload = SetUpTokenRequest();

        HttpClient httpClient = new HttpClient();

        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(_discoveryDocument!.TokenEndpoint),
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(payload)
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            string errorResponse = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            _logger.LogError("Error on client credential flow token request. Response from server " + errorResponse);
            throw;
        }

        ClientCredentialsTokenResponse? result = await response.Content.ReadFromJsonAsync<ClientCredentialsTokenResponse>();

        if (result == null)
        {
            throw new InvalidOperationException("Could not deserialize client credential token response");
        }

        result.ExpiresAt = DateTime.UtcNow.AddSeconds(Convert.ToInt32(result.ExpiresIn));

        return result;
    }

    /// <summary>
    /// Determines whether the specified token response is expired.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>
    ///   <c>true</c> if the specified response is expired; otherwise, <c>false</c>.
    /// </returns>
    private bool IsExpired(ClientCredentialsTokenResponse response)
    {
        return response.ExpiresAt.Subtract(DateTime.UtcNow).TotalSeconds < 30;
    }
}
