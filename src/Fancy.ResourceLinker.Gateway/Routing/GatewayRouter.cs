using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Models.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// A class to provide routing functionality for manual routing implementations.
/// </summary>
public class GatewayRouter
{
    /// <summary>
    /// The HTTP client.
    /// </summary>
    private readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// The forwarder request configuration.
    /// </summary>
    private readonly ForwarderRequestConfig _forwarderRequestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

    /// <summary>
    /// The forwarder transformer.
    /// </summary>
    private readonly HttpTransformer _forwarderTransformer = new GatewayForwarderHttpTransformer();

    /// <summary>
    /// The serializer options used to deserialize received json.
    /// </summary>
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// The routing settings.
    /// </summary>
    private readonly GatewayRoutingSettings _settings;

    /// <summary>
    /// The forwarder.
    /// </summary>
    private readonly IHttpForwarder _forwarder;

    /// <summary>
    /// The resource cache.
    /// </summary>
    private readonly IResourceCache _resourceCache;

    /// <summary>
    /// The token service.
    /// </summary>
    private readonly TokenService? _tokenService;

    /// <summary>
    /// The token client.
    /// </summary>
    private readonly TokenClient? _tokenClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayRouter"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="forwarder">The forwarder.</param>
    /// <param name="resourceCache">The resource cache.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public GatewayRouter(GatewayRoutingSettings settings, IHttpForwarder forwarder, IResourceCache resourceCache, IServiceProvider serviceProvider)
    {
        _settings = settings;
        _forwarder = forwarder;
        _resourceCache = resourceCache;

        try
        {
            // Get the optional token service
            _tokenService = serviceProvider.GetService<TokenService>();
            _tokenClient = serviceProvider.GetService<TokenClient>();
        }
        catch(InvalidOperationException)
        {
            // The services are not available we assume there is no client scope and work with client credentials
            _tokenService = null;
            _tokenClient = null;
        }

        // Set up serializer options
        _serializerOptions = new JsonSerializerOptions();
        _serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        _serializerOptions.AddResourceConverter();
    }

    private async Task SetTokenToRequest(HttpRequestMessage request)
    {
        string accessToken;
        if (_tokenService != null)
        {
            // A user session exists, get token from token service
            accessToken = await _tokenService.GetAccessTokenAsync();
        }
        else if (_tokenClient != null)
        {
            // Fall back to client credentials token directly
            ClientCredentialsTokenResponse? tokenResponse = await _tokenClient.GetTokenViaClientCredentialsAsync();
            if (tokenResponse == null) throw new InvalidOperationException("Could not retrieve token via client credentials.");
            accessToken = tokenResponse.AccessToken;
        }
        else
        {
            throw new InvalidOperationException($"If you want to send access tokens, gateway authentication must be configured.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Sends a request and deserializes the response into a given type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private async Task<TResource?> SendAsync<TResource>(HttpRequestMessage request, bool sendAccessToken) where TResource : class
    {
        if (_settings.ResourceProxy != null)
        {
            request.Headers.Add("X-Forwarded-Host", _settings.ResourceProxy);
        }

        if(sendAccessToken)
        {
            await SetTokenToRequest(request);
        }

        // Get data from microservice
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();

        if (responseMessage.Content.Headers.ContentLength > 0)
        {
            string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResource>(jsonResponse, _serializerOptions) ?? throw new Exception("Error on deserialization of result");
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Sends a request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    private async Task SendAsync(HttpRequestMessage request, bool sendAccessToken)
    {
        if (_settings.ResourceProxy != null)
        {
            request.Headers.Add("X-Forwarded-Host", _settings.ResourceProxy);
        }

        if (sendAccessToken)
        {
            await SetTokenToRequest(request);
        }

        // Get data from microservice
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Gets data from a url and deserializes it into a given type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="requestUri">The uri of the data to get.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private async Task<TResource> GetAsync<TResource>(Uri requestUri, bool sendAccessToken) where TResource : class
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Get,
        };

        var result = await SendAsync<TResource>(request, sendAccessToken);

        if(result == null) throw new ApplicationException("No Content was provided by the server");

        return result;
    }

    /// <summary>
    /// Get data from a microservice specified by its key of a provided route and deserializes it into a given type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource> GetAsync<TResource>(string routeKey, string relativeUrl) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return GetAsync<TResource>(requestUri, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    private Task PutAsync(Uri requestUri, object content, bool sendAccessToken)
    { 
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Put,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync(request, sendAccessToken);
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    public Task PutAsync(string routeKey, string relativeUrl, object content)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return PutAsync(requestUri, content, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private Task<TResource?> PutAsync<TResource>(Uri requestUri, object content, bool sendAccessToken) where TResource: class
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Put,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync<TResource>(request, sendAccessToken);
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> PutAsync<TResource>(string routeKey, string relativeUrl, object content) where TResource: class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return PutAsync<TResource>(requestUri, content, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Post data to a specific uri.
    /// </summary>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    private Task PostAsync(Uri requestUri, object content, bool sendAccessToken)
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync(request, sendAccessToken);
    }
    
    /// <summary>
    /// Post data to a specific uri.
    /// </summary>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    public Task PostAsync(string routeKey, string relativeUrl, object content)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return PostAsync(requestUri, content, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Post data to a specific uri and return the result deserialized into the specified resource type.</returns>.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private Task<TResource?> PostAsync<TResource>(Uri requestUri, object content, bool sendAccessToken) where TResource : class
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync<TResource>(request, sendAccessToken);
    }

    /// <summary>
    /// Post data to a specific uri and return the result deserialized into the specified resource type.</returns>.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> PostAsync<TResource>(string routeKey, string relativeUrl, object content) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return PostAsync<TResource>(requestUri, content, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Delete data from a specific URI
    /// </summary>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    private Task DeleteAsync(Uri requestUri, bool sendAccessToken)
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Delete,
        };

        return SendAsync(request, sendAccessToken);
    }

    /// <summary>
    /// Delete data from a specific URI
    /// </summary>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    public Task DeleteAsync(string routeKey, string relativeUrl)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return DeleteAsync(requestUri, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Delete data from a specific URI and return the result deserialized into the specified resource type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="requestUri">The uri to send to.</param>
    /// <param name="sendAccessToken">If true, the request will be enriched with an access token.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private Task<TResource?> DeleteAsync<TResource>(Uri requestUri, bool sendAccessToken) where TResource : class
    {
        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Delete,
        };

        return SendAsync<TResource>(request, sendAccessToken);
    }

    /// <summary>
    /// Delete data from a specific URI and return the result deserialized into the specified resource type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeKey">The key of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> DeleteAsync<TResource>(string routeKey, string relativeUrl) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return DeleteAsync<TResource>(requestUri, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Gets data from a url and deserializes it into a given type. If data is available in the cache and not older
    /// as the age specified the data is returned from the chache, if not data is retrieved from the origin and written to the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="requestUri">The uri of the data to get.</param>
    /// <param name="maxResourceAge">The maximum age of the resource which is acceptable.</param>
    /// <returns>
    /// The result deserialized into the specified resource type.
    /// </returns>
    public async Task<TResource> GetCachedAsync<TResource>(Uri requestUri, TimeSpan maxResourceAge, bool sendAccessToken) where TResource : class
    {
        string cacheKey = requestUri.ToString();

        // Check if we can get the resource from cache
        TResource? data;
        if (_resourceCache.TryRead(cacheKey, maxResourceAge, out data))
        {
            return data ?? throw new ApplicationException("Error on reading item from cache");
        }
        else
        {
            // Get resource from origin and write it to the cache
            data = await GetAsync<TResource>(requestUri, sendAccessToken);
            _resourceCache.Write(cacheKey, data);
            return data;
        }
    }

    /// <summary>
    /// Get data from a microservice specified by its key of a provided endpoint and deserializes it into a given type.
    /// If data is available in the cache and not older as the age specified the data is returned from the chache, if not
    /// data is retrieved from the origin and written to the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeKey">The key of the route url to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="maxResourceAge">The maximum age of the resource which is acceptable.</param>
    /// <returns>
    /// The result deserialized into the specified resource type.
    /// </returns>
    public Task<TResource> GetCachedAsync<TResource>(string routeKey, string relativeUrl, TimeSpan maxResourceAge) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);
        return GetCachedAsync<TResource>(requestUri, maxResourceAge, _settings.Routes[routeKey].EnforceAuthentication);
    }

    /// <summary>
    /// Sends the current request to a microservice.
    /// </summary>
    /// <param name="routeKey">The key to the uri of the microservcie to send the request to.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <returns>The response of the call to the microservice as IActionResult</returns>
    public async Task<IActionResult> ProxyAsync(HttpContext httpContext, string routeKey, string relativeUrl)
    {
        HttpRequestMessage proxyRequest = new HttpRequestMessage();

        if (httpContext.Request.ContentLength > 0)
        {
            using (StreamReader reader = new StreamReader(httpContext.Request.Body))
            {
                string content = await reader.ReadToEndAsync();
                proxyRequest.Content = new StringContent(content, Encoding.UTF8, httpContext.Request.ContentType);
            }
        }

        proxyRequest.Method = new HttpMethod(httpContext.Request.Method);

        Uri requestUri = CombineUris(GetBaseUrl(routeKey), relativeUrl);

        if (_settings.ResourceProxy != null)
        {
            proxyRequest.Headers.Add("X-Forwarded-Host", _settings.ResourceProxy);
        }

        proxyRequest.Headers.Add("Accept", httpContext.Request.Headers["Accept"].ToString());
        proxyRequest.Headers.Host = requestUri.Authority;
        proxyRequest.RequestUri = requestUri;

        if (_settings.Routes[routeKey].EnforceAuthentication)
        {
            await SetTokenToRequest(proxyRequest);
        }

        HttpResponseMessage proxyResponse = await _httpClient.SendAsync(proxyRequest);

        if ((int)proxyResponse.StatusCode >= 200 && (int)proxyResponse.StatusCode < 500)
        {
            if (proxyResponse.Content.Headers.ContentLength > 0)
            {
                string content = await proxyResponse.Content.ReadAsStringAsync();
                string? contentType = proxyResponse.Content.Headers.ContentType?.MediaType;
                return new ContentResult { StatusCode = (int)proxyResponse.StatusCode, Content = content, ContentType = contentType };
            }
            else
            {
                return new StatusCodeResult((int)proxyResponse.StatusCode);
            }
        }
        else
        {
            if (proxyResponse.Content.Headers.ContentLength > 0)
            {
                string content = "Internal Error from Microservice \n";
                content += "------------------------------------------\n";
                content += await proxyResponse.Content.ReadAsStringAsync();
                return new ContentResult { StatusCode = 500, Content = content, ContentType = "text" };
            }
            else
            {
                string content = "Internal Error from Microservice with no detailed error message.";
                return new ContentResult { StatusCode = 500, Content = content, ContentType = "text" };

            }
        }
    }

    /// <summary>
    /// Sends the current request to a microservice with the same relative url as the current request.
    /// </summary>
    /// <param name="baseUriKey">The key to the uri of the microservcie to send the request to.</param>
    /// <returns>The response of the call to the microservice as IActionResult</returns>
    public Task<IActionResult> ProxyAsync(HttpContext httpContext, string routeKey)
    {
        return ProxyAsync(httpContext, routeKey, httpContext.Request.Path + httpContext.Request.QueryString);
    }

    /// <summary>
    /// Forwards an http context asynchronous.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="routeKey">The route key.</param>
    public async Task ForwardAsync(HttpContext httpContext, string routeKey)
    {
        string targetUrl = GetBaseUrl(routeKey);
        httpContext.Items[GatewayForwarderHttpTransformer.SendAccessTokenItemKey] = _settings.Routes[routeKey].EnforceAuthentication;

        // Forward request to microservice
        ForwarderError error = await _forwarder.SendAsync(httpContext, targetUrl, _httpClient, _forwarderRequestConfig, _forwarderTransformer);
        
        // Check if the operation was successful
        if (error != ForwarderError.None)
        {
            IForwarderErrorFeature? errorFeature = httpContext.GetForwarderErrorFeature();
            Exception? exception = errorFeature?.Exception;
            throw exception ?? throw new Exception("Error on forwarding");
        }
    }

    /// <summary>
    /// Gets the base URL of a routing key.
    /// </summary>
    /// <param name="routeKey">The route key.</param>
    /// <returns>The base url</returns>
    private string GetBaseUrl(string routeKey)
    {
        Uri? baseUrl = _settings.Routes[routeKey].BaseUrl;
        if (baseUrl == null) throw new InvalidOperationException($"'BaseUrl' is required for route with key '{routeKey}'");
        return baseUrl.AbsoluteUri;
    }

    /// <summary>
    /// Helper method to cobine a base uri with a relative uri.
    /// </summary>
    /// <param name="baseUri">The base URI.</param>
    /// <param name="relativeUri">The relative URI.</param>
    /// <returns>The combined uri.</returns>
    private Uri CombineUris(string baseUri, string relativeUri)
    {
        baseUri = baseUri.Trim();
        relativeUri = relativeUri.Trim();

        if (baseUri.EndsWith("/")) baseUri = baseUri.Substring(0, baseUri.Length - 1);
        if (relativeUri.StartsWith("/")) relativeUri = relativeUri.Substring(1);

        return new Uri(baseUri + "/" + relativeUri);
    }
}
