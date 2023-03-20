using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Forwarder;

namespace Fancy.ResourceLinker.Gateway.Routing
{
    public class GatewayRouter
    {
        /// <summary>
        /// The HTTP client.
        /// </summary>
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly ForwarderRequestConfig _forwarderRequestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
        private readonly HttpTransformer _forwarderTransformer = new GatewayForwarderHttpTransformer();

        /// <summary>
        /// The serializer options used to deserialize received json.
        /// </summary>
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions();

        private readonly GatewayRoutingSettings _settings;
        private readonly IHttpForwarder _forwarder;
        private readonly TokenService? _tokenService;

        public GatewayRouter(IOptions<GatewayRoutingSettings> settings, IHttpForwarder forwarder, IServiceProvider serviceProvider)
        {
            _settings = settings.Value;
            _forwarder = forwarder;

            _serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            _serializerOptions.AddResourceConverter();

            // Check if a token service is available
            _tokenService = serviceProvider.GetService<TokenService>();
        }

        /// <summary>
        /// Gets data from a url and deserializes it into a given type.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="requestUri">The uri of the data to get.</param>
        /// <returns>The result deserialized into the specified resource type.</returns>
        public async Task<TResource> GetAsync<TResource>(Uri requestUri) where TResource : class
        {
            // Set up request
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = requestUri,
                Method = HttpMethod.Get
            };

            request.Headers.Add("ResourceProxy", "http://localhost:5101");

            if(_tokenService != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenService.GetAccessTokenAsync());
            }

            // Get data from microservice
            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResource>(jsonResponse, _serializerOptions);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Get data from a microservice specified by its key of a provided endpoint and deserializes it into a given type.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="microserviceKey">The key of the microservice url to use.</param>
        /// <param name="relativeUrl">The relative url to the endpoint.</param>
        /// <returns>The result deserialized into the specified resource type.</returns>
        public Task<TResource> GetAsync<TResource>(string microserviceKey, string relativeUrl) where TResource : class
        {
            Uri requestUri = CombineUris(_settings.Microservices[microserviceKey].BaseUrl.AbsoluteUri, relativeUrl);
            return GetAsync<TResource>(requestUri);
        }

        /// <summary>
        /// Sends the current request to a microservice.
        /// </summary>
        /// <param name="baseUriKey">The key to the uri of the microservcie to send the request to.</param>
        /// <param name="microserviceKey">The relative url to the endpoint.</param>
        /// <returns>The response of the call to the microservice as IActionResult</returns>
        public async Task<IActionResult> ProxyAsync(HttpContext httpContext, string microserviceKey, string relativeUrl)
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

            Uri requestUri = CombineUris(_settings.Microservices[microserviceKey].BaseUrl.AbsoluteUri, relativeUrl);

            proxyRequest.Headers.Add("Accept", httpContext.Request.Headers["Accept"].ToString());
            proxyRequest.Headers.Host = requestUri.Authority;
            proxyRequest.RequestUri = requestUri;

            if (_tokenService != null)
            {
                proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenService.GetAccessTokenAsync());
            }

            HttpResponseMessage proxyResponse = await _httpClient.SendAsync(proxyRequest);

            if ((int)proxyResponse.StatusCode >= 200 && (int)proxyResponse.StatusCode < 500)
            {
                if (proxyResponse.Content.Headers.ContentLength > 0)
                {
                    string content = await proxyResponse.Content.ReadAsStringAsync();
                    string contentType = proxyResponse.Content.Headers.ContentType?.MediaType;
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
        public Task<IActionResult> ProxyAsync(HttpContext httpContext, string microserviceKey)
        {
            return ProxyAsync(httpContext, microserviceKey, httpContext.Request.Path + httpContext.Request.QueryString);
        }

        public async Task ForwardAsync(HttpContext httpContext, string microserviceKey)
        {
            string targetUrl = _settings.Microservices[microserviceKey].BaseUrl.AbsoluteUri;

            // Forward request to microservice
            ForwarderError error = await _forwarder.SendAsync(httpContext, targetUrl, _httpClient, _forwarderRequestConfig, _forwarderTransformer);
            
            // Check if the operation was successful
            if (error != ForwarderError.None)
            {
                IForwarderErrorFeature errorFeature = httpContext.GetForwarderErrorFeature();
                Exception exception = errorFeature.Exception;
                throw exception;
            }
        }

        /// <summary>
        /// Helper method to cobine a base uri with a relative uri.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="relativeUri">The relative URI.</param>
        /// <returns>The combined uri.</returns>
        internal Uri CombineUris(string baseUri, string relativeUri)
        {
            baseUri = baseUri.Trim();
            relativeUri = relativeUri.Trim();

            if (baseUri.EndsWith("/")) baseUri = baseUri.Substring(0, baseUri.Length - 1);
            if (relativeUri.StartsWith("/")) relativeUri = relativeUri.Substring(1);

            return new Uri(baseUri + "/" + relativeUri);
        }
    }
}
