using Fancy.ResourceLinker.Gateway.Routing.Auth;
using Fancy.ResourceLinker.Models.Json;
using Fancy.ResourceLinker.Gateway.Routing.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// A class to provide routing functionality for manual routing and data aggregation implementations.
/// </summary>
public class GatewayRouter
{
    /// <summary>
    /// The HTTP client.
    /// </summary>
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// The forwarder message invoker.
    /// </summary>
    private HttpMessageInvoker _forwarderMessageInvoker;

    /// <summary>
    /// The forwarder request configuration.
    /// </summary>
    private readonly ForwarderRequestConfig _forwarderRequestConfig;

    /// <summary>
    /// The forwarder transformer.
    /// </summary>
    private readonly HttpTransformer _forwarderTransformer;

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
    /// The route authentication manager.
    /// </summary>
    private readonly RouteAuthenticationManager _routeAuthManager;

    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayRouter" /> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="forwarder">The forwarder.</param>
    /// <param name="resourceCache">The resource cache.</param>
    /// <param name="routeAuthManager">The route authentication manager.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public GatewayRouter(GatewayRoutingSettings settings, IHttpForwarder forwarder, IResourceCache resourceCache, RouteAuthenticationManager routeAuthManager, IServiceProvider serviceProvider)
    {
        _settings = settings;
        _forwarder = forwarder;
        _resourceCache = resourceCache;
        _routeAuthManager = routeAuthManager;
        _serviceProvider = serviceProvider;

        // Set up serializer options
        _serializerOptions = new JsonSerializerOptions();
        _serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        _serializerOptions.AddResourceConverter();

        // Set up forwarder assets
        _forwarderMessageInvoker = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15),
        });

        _forwarderRequestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

        _forwarderTransformer = new GatewayForwarderHttpTransformer();
    }

    /// <summary>
    /// Sends a request and deserializes the response into a given type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="routeName">The name of the route to use.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    private async Task<TResource?> SendAsync<TResource>(HttpRequestMessage request, string routeName) where TResource : class
    {
        request.Headers.SetForwardedHeaders(_settings.ResourceProxy);

        // Set authentication to request
        IRouteAuthenticationStrategy authStrategy = await _routeAuthManager.GetAuthStrategyAsync(routeName);
        await authStrategy.SetAuthenticationAsync(_serviceProvider, request);

        // Get data from microservice
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();

        if (responseMessage.Content.Headers.ContentLength > 0)
        {
            string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResource>(jsonResponse, _serializerOptions) ?? throw new Exception("Error on deserialization of result");
        }

        return default;
    }

    /// <summary>
    /// Sends a request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="routeName">The name of the route to use.</param>
    private async Task SendAsync(HttpRequestMessage request, string routeName)
    {
        request.Headers.SetForwardedHeaders(_settings.ResourceProxy);

        // Set authentication to request
        IRouteAuthenticationStrategy authStrategy = await _routeAuthManager.GetAuthStrategyAsync(routeName);
        await authStrategy.SetAuthenticationAsync(_serviceProvider, request);

        // Get data from microservice
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Get data from a microservice specified by its key of a provided route and deserializes it into a given type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public async Task<TResource> GetAsync<TResource>(string routeName, string relativeUrl) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Get,
        };

        var result = await SendAsync<TResource>(request, routeName);

        if (result == null) throw new ApplicationException("No Content was provided by the server");

        return result;
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    public Task PutAsync(string routeName, string relativeUrl, object content)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Put,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync(request, routeName);
    }

    /// <summary>
    /// Puts data to a specific uri.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> PutAsync<TResource>(string routeName, string relativeUrl, object content) where TResource: class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Put,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync<TResource>(request, routeName);
    }

    /// <summary>
    /// Post data to a specific uri.
    /// </summary>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    public Task PostAsync(string routeName, string relativeUrl, object content)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync(request, routeName);
    }

    /// <summary>
    /// Post data to a specific uri and return the result deserialized into the specified resource type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="content">The content to send - will be serialized as json.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> PostAsync<TResource>(string routeName, string relativeUrl, object content) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return SendAsync<TResource>(request, routeName);
    }

    /// <summary>
    /// Delete data from a specific URI
    /// </summary>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    public Task DeleteAsync(string routeName, string relativeUrl)
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Delete,
        };

        return SendAsync(request, routeName);
    }

    /// <summary>
    /// Delete data from a specific URI and return the result deserialized into the specified resource type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <returns>The result deserialized into the specified resource type.</returns>
    public Task<TResource?> DeleteAsync<TResource>(string routeName, string relativeUrl) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        // Set up request
        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Method = HttpMethod.Delete,
        };

        return SendAsync<TResource>(request, routeName);
    }

    /// <summary>
    /// Get data from a microservice specified by its key of a provided endpoint and deserializes it into a given type.
    /// If data is available in the cache and not older as the age specified the data is returned from the chache, if not
    /// data is retrieved from the origin and written to the cache.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="routeName">The name of the route to use.</param>
    /// <param name="relativeUrl">The relative url to the endpoint.</param>
    /// <param name="maxResourceAge">The maximum age of the resource which is acceptable.</param>
    /// <returns>
    /// The result deserialized into the specified resource type.
    /// </returns>
    public async Task<TResource> GetCachedAsync<TResource>(string routeName, string relativeUrl, TimeSpan maxResourceAge) where TResource : class
    {
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

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
            data = await GetAsync<TResource>(routeName, relativeUrl);
            _resourceCache.Write(cacheKey, data);
            return data;
        }
    }

    /// <summary>
    /// Sends the current request to a microservice with the same relative url as the current request.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="routeName">The name of the route to use.</param>
    /// <returns>
    /// The response of the call to the microservice as IActionResult
    /// </returns>
    public async Task<IActionResult> ProxyAsync(HttpContext httpContext, string routeName)
    {
        string relativeUrl = httpContext.Request.Path + httpContext.Request.QueryString;
        Uri requestUri = CombineUris(GetBaseUrl(routeName), relativeUrl);

        HttpRequestMessage proxyRequest = new HttpRequestMessage();

        if (httpContext.Request.ContentLength > 0)
        {
            using (StreamReader reader = new StreamReader(httpContext.Request.Body))
            {
                string content = await reader.ReadToEndAsync();
                proxyRequest.Content = new StringContent(content, Encoding.UTF8, httpContext.Request.ContentType ?? string.Empty);
            }
        }

        proxyRequest.Method = new HttpMethod(httpContext.Request.Method);
        proxyRequest.Headers.SetForwardedHeaders(_settings.ResourceProxy);
        proxyRequest.Headers.Add("Accept", httpContext.Request.Headers["Accept"].ToString());
        proxyRequest.Headers.Host = requestUri.Authority;
        proxyRequest.RequestUri = requestUri;

        // Set authentication to request
        IRouteAuthenticationStrategy authStrategy = await _routeAuthManager.GetAuthStrategyAsync(routeName);
        await authStrategy.SetAuthenticationAsync(httpContext.RequestServices, proxyRequest);

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
    /// Forwards an http context asynchronous.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="routeName">The route name to use.</param>
    /// <param name="relativurl">The relativurl.</param>
    public async Task ForwardAsync(HttpContext httpContext, string routeName, string relativurl)
    {
        string baseUrl = GetBaseUrl(routeName);
        Uri targetUrl = CombineUris(baseUrl, relativurl);

        httpContext.Items[GatewayForwarderHttpTransformer.RouteNameItemKey] = routeName;
        httpContext.Items[GatewayForwarderHttpTransformer.ResourceProxyItemKey] = _settings.ResourceProxy;

        // Forward request to microservice
        ForwarderError error = await _forwarder.SendAsync(httpContext,
                                                          targetUrl.AbsoluteUri, 
                                                          _forwarderMessageInvoker, 
                                                          _forwarderRequestConfig, 
                                                          _forwarderTransformer);

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
