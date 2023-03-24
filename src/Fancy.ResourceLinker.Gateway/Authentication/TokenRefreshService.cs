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

internal class TokenRefreshService
{
    private readonly GatewayAuthenticationSettings _settings;
    private readonly DiscoveryDocumentService _discoveryDocumentService;
    private DiscoveryDocument _discoveryDocument;
    
    public TokenRefreshService(IOptions<GatewayAuthenticationSettings> settings, DiscoveryDocumentService discoveryDocumentService)
    {
        _settings = settings.Value;
        _discoveryDocumentService = discoveryDocumentService;
    }

    public async Task<TokenRefreshResponse?> RefreshAsync(string refreshToken)
    {
        if(_discoveryDocument == null)
        {
            _discoveryDocument = await _discoveryDocumentService.LoadDiscoveryDocumentAsync(_settings.Authority);
        }

        var payload = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", _settings.ClientId },
            //{ "client_secret", config.ClientSecret }
        };

        HttpClient httpClient = new HttpClient();

        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(_discoveryDocument.TokenEndpoint),
            Method = HttpMethod.Post,
            Content = new FormUrlEncodedContent(payload)
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        TokenRefreshResponse result = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();

        return result;

    }
}
