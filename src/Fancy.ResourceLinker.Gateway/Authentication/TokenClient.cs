using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Gateway.Authentication;

public class TokenRefreshResponse
{
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = "";

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";
    
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
}

public class ClientCredentialsTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
}

public class TokenClient
{
    private readonly GatewayAuthenticationSettings _settings;
    private readonly DiscoveryDocumentService _discoveryDocumentService;
    private DiscoveryDocument _discoveryDocument;
    
    public TokenClient(GatewayAuthenticationSettings settings, DiscoveryDocumentService discoveryDocumentService)
    {
        _settings = settings;
        _discoveryDocumentService = discoveryDocumentService;
    }

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
            string responseContent = await response.Content.ReadAsStringAsync();
            return null;
        }

        TokenRefreshResponse result = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();

        return result;
    }

    public async Task<ClientCredentialsTokenResponse?> GetTokenViaClientCredentialsAsync()
    {
        DiscoveryDocument discoveryDocument = await GetDiscoveryDocumentAsync();

        var payload = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "scope", _settings.ClientCredentialsScope }
        };

        HttpClient httpClient = new HttpClient();

        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(discoveryDocument.TokenEndpoint),
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(payload)
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        var message = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        ClientCredentialsTokenResponse result = await response.Content.ReadFromJsonAsync<ClientCredentialsTokenResponse>();

        return result;
    }

    private async Task<DiscoveryDocument> GetDiscoveryDocumentAsync()
    {
        if (_discoveryDocument == null)
        {
            _discoveryDocument = await _discoveryDocumentService.LoadDiscoveryDocumentAsync(_settings.Authority);
        }

        return _discoveryDocument;
    }
}
