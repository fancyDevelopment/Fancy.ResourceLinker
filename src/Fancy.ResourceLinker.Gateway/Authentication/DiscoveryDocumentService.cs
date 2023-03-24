using System.Net.Http.Json;

namespace Fancy.ResourceLinker.Gateway.Authentication;

internal class DiscoveryDocumentService
{
    private const string DISCOVERY_URL = "/.well-known/openid-configuration";

    public async Task<DiscoveryDocument> LoadDiscoveryDocumentAsync(string authority)
    {
        var httpClient = new HttpClient();

        string url = CombineUrls(authority, DISCOVERY_URL);

        DiscoveryDocument result = await httpClient.GetFromJsonAsync<DiscoveryDocument>(url);

        if (result == null)
        {
            throw new Exception("Error loading discovery document from " + url);
        }

        return result;
    }

    private string CombineUrls(string uri1, string uri2)
    {
        uri1 = uri1.TrimEnd('/');
        uri2 = uri2.TrimStart('/');
        return string.Format("{0}/{1}", uri1, uri2);
    }
}
