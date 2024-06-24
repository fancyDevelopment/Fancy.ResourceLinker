using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Fancy.ResourceLinker.Gateway.Common;

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// Class to hold the result of a token refresh response.
/// </summary>
public class TokenRefreshResponse
{
    /// <summary>
    /// Gets or sets the identifier token.
    /// </summary>
    /// <value>
    /// The identifier token.
    /// </value>
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = "";

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
}

/// <summary>
/// Class to hold the result of a client credentials token response.
/// </summary>
public class ClientCredentialsTokenResponse
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
}

/// <summary>
/// A token client with implementation of typical token logic.
/// </summary>
public class TokenClient
{
    /// <summary>
    /// The authentication settings.
    /// </summary>
    private readonly GatewayAuthenticationSettings _settings;

    /// <summary>
    /// The discovery document service.
    /// </summary>
    private readonly DiscoveryDocumentService _discoveryDocumentService;

    /// <summary>
    /// The discovery document.
    /// </summary>
    private DiscoveryDocument? _discoveryDocument;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenClient"/> class.
    /// </summary>
    /// <param name="settings">The authentication settings.</param>
    /// <param name="discoveryDocumentService">The discovery document service.</param>
    public TokenClient(GatewayAuthenticationSettings settings, DiscoveryDocumentService discoveryDocumentService)
    {
        _settings = settings;
        _discoveryDocumentService = discoveryDocumentService;
    }

    /// <summary>
    /// Executes a token refresh the asynchronous.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>The token refresh response.</returns>
    public async Task<TokenRefreshResponse?> RefreshAsync(string refreshToken)
    {
        DiscoveryDocument discoveryDocument = await GetDiscoveryDocumentAsync();

        var payload = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        };

        HttpClient httpClient = new HttpClient();

        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(discoveryDocument.TokenEndpoint),
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(payload)
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
    }

    /// <summary>
    /// Gets the discovery document asynchronous.
    /// </summary>
    /// <returns>The discovery document.</returns>
    private async Task<DiscoveryDocument> GetDiscoveryDocumentAsync()
    {
        if (_discoveryDocument == null)
        {
            _discoveryDocument = await _discoveryDocumentService.LoadDiscoveryDocumentAsync(_settings.Authority);
        }

        return _discoveryDocument;
    }
}
