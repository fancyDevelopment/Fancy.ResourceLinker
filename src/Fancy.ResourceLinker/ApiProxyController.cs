using Fancy.ResourceLinker.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Controller base class for API proxy controllers to to forward requests to microservices.
    /// </summary>
    public class ApiProxyController : HypermediaController
    {
        /// <summary>
        /// The HTTP client.
        /// </summary>
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// The serializer options used to deserialize received json.
        /// </summary>
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions();

        /// <summary>
        /// The base urls of the microservices mapped to a unique key.
        /// </summary>
        protected readonly Dictionary<string, Uri> _baseUris;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProxyController"/> class.
        /// </summary>
        /// <param name="baseUris">The base uris of the microservices each mapped to a unique key.</param>
        public ApiProxyController(Dictionary<string, Uri> baseUris)
        {
            _baseUris = baseUris;
            _serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            _serializerOptions.AddResourceConverter();
        }

        /// <summary>
        /// Gets data from a url and deserializes it into a given type.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="requestUri">The uri of the data to get.</param>
        /// <returns>The result deserialized into the specified resource type.</returns>
        protected async Task<TResource> GetAsync<TResource>(Uri requestUri) where TResource : class
        {
            // Get data from microservice
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(requestUri);
            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResource>(jsonResponse, _serializerOptions);
            }
            else
            {
                return default(TResource);
            }
        }

        /// <summary>
        /// Get data from a microservice specified by its key of a provided endpoint and deserializes it into a given type.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="baseUriKey">The key of the microservice url to use.</param>
        /// <param name="relativeUrl">The relative url to the endpoint.</param>
        /// <returns>The result deserialized into the specified resource type.</returns>
        protected Task<TResource> GetAsync<TResource>(string baseUriKey, string relativeUrl) where TResource : class
        {
            Uri requestUri = CombineUris(_baseUris[baseUriKey].AbsoluteUri, relativeUrl);
            return GetAsync<TResource>(requestUri);
        }

        /// <summary>
        /// Sends the current request to a microservice.
        /// </summary>
        /// <param name="baseUriKey">The key to the uri of the microservcie to send the request to.</param>
        /// <param name="relativeUrl">The relative url to the endpoint.</param>
        /// <returns>The response of the call to the microservice as IActionResult</returns>
        protected async Task<IActionResult> ProxyAsync(string baseUriKey, string relativeUrl)
        {
            HttpRequestMessage proxyRequest = new HttpRequestMessage();

            if (HttpContext.Request.ContentLength > 0)
            {
                using (StreamReader reader = new StreamReader(HttpContext.Request.Body))
                {
                    string content = await reader.ReadToEndAsync();
                    proxyRequest.Content = new StringContent(content, Encoding.UTF8, HttpContext.Request.ContentType);
                }
            }

            proxyRequest.Method = new HttpMethod(HttpContext.Request.Method);

            Uri requestUri = CombineUris(_baseUris[baseUriKey].AbsoluteUri, relativeUrl);

            proxyRequest.Headers.Add("Accept", HttpContext.Request.Headers["Accept"].ToString());
            proxyRequest.Headers.Host = requestUri.Authority;
            proxyRequest.RequestUri = requestUri;

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
        protected Task<IActionResult> ProxyAsync(string baseUriKey)
        {
            return ProxyAsync(baseUriKey, HttpContext.Request.Path + HttpContext.Request.QueryString);
        }

        protected async Task ProxyBinaryAsync(string baseUriKey, string relativeUri)
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });

            Uri uri = CombineUris(_baseUris[baseUriKey].AbsoluteUri, relativeUri);
            using (HttpRequestMessage proxyRequestMessage = CreateProxyHttpRequest(uri))
            {
                using (HttpResponseMessage proxyResponseMessage = await httpClient.SendAsync(proxyRequestMessage))
                {
                    await CopyProxyHttpResponse(proxyResponseMessage);
                }
            }
        }

        protected Task ProxyBinaryAsync(string baseUriKey)
        {
            return ProxyBinaryAsync(baseUriKey, HttpContext.Request.Path + HttpContext.Request.QueryString);
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

        private HttpRequestMessage CreateProxyHttpRequest(Uri uri)
        {
            var request = HttpContext.Request;

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                request.Body.Position = 0;
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }

        public async Task CopyProxyHttpResponse(HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                throw new ArgumentNullException(nameof(responseMessage));
            }

            var response = HttpContext.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");

            if (!responseMessage.Content.Headers.ContentLength.HasValue || responseMessage.Content.Headers.ContentLength.Value == 0)
            {
                response.StatusCode = (int)responseMessage.StatusCode;
                return;
            }

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(response.Body, HttpContext.RequestAborted);
            }
        }
    }
}
