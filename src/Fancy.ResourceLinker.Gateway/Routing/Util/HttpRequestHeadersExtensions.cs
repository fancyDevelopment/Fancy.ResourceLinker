using System.Net.Http.Headers;

namespace Fancy.ResourceLinker.Gateway.Routing.Util;

internal static class HttpRequestHeadersExtensions
{
    /// <summary>
    /// Sets the X-Forwarded headers.
    /// </summary>
    /// <param name="headers">The headers.</param>
    /// <param name="origin">The origin to set the headers for.</param>
    public static void SetForwardedHeaders(this HttpRequestHeaders headers, string? origin)
    {
        if (origin != null)
        {
            string[] proxyParts = origin.Split("://");
            string proto = proxyParts[0];
            string host = proxyParts[1];
            headers.Add("X-Forwarded-Proto", proto);
            headers.Add("X-Forwarded-Host", host);
        }
    }
}
