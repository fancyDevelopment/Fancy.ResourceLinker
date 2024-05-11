using System.Net.Http.Json;
using Fancy.ResourceLinker.Gateway.Authentication;

namespace Fancy.ResourceLinker.Gateway.Common;

/// <summary>
/// A service to retrieve the discovery document from an authorization server.
/// </summary>
public class DiscoveryDocumentService
{
    /// <summary>
    /// The well known discovery URL
    /// </summary>
    private const string DISCOVERY_URL = "/.well-known/openid-configuration";

    /// <summary>
    /// Loads a discovery document asynchronous.
    /// </summary>
    /// <param name="authorityUrl">The authority URL.</param>
    /// <returns>An instance of a class representing the discovery document.</returns>
    public async Task<DiscoveryDocument> LoadDiscoveryDocumentAsync(string authorityUrl)
    {
        HttpClient httpClient = new HttpClient();

        string url = CombineUrls(authorityUrl, DISCOVERY_URL);

        DiscoveryDocument? result = await httpClient.GetFromJsonAsync<DiscoveryDocument>(url);

        if (result == null)
        {
            throw new Exception("Error loading discovery document from " + url);
        }

        return result;
    }

    /// <summary>
    /// Combines the urls.
    /// </summary>
    /// <param name="uri1">The uri1.</param>
    /// <param name="uri2">The uri2.</param>
    /// <returns>The combinde URL.</returns>
    private string CombineUrls(string uri1, string uri2)
    {
        uri1 = uri1.TrimEnd('/');
        uri2 = uri2.TrimStart('/');
        return string.Format("{0}/{1}", uri1, uri2);
    }
}
