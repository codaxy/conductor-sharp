using ConductorSharp.Client.Exceptions;
using ConductorSharp.Client.Model.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ConductorSharp.Client.Service
{
    public class ConductorHttpClient : IConductorClient
    {
        private readonly ILogger<ConductorClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly RestConfig _restConfig;

        public ConductorHttpClient(ILogger<ConductorClient> logger, HttpClient httpClient, RestConfig restConfig)
        {
            _logger = logger;
            _httpClient = httpClient;
            _restConfig = restConfig;
        }

        private async void CheckResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var request = await response.RequestMessage.Content.ReadAsStringAsync();
                var responseContent = await response.Content.ReadAsStringAsync();
                ConductorErrorResponse error = default;

                _logger.LogDebug("Received {@response}", response.Content);

                error = JsonConvert.DeserializeObject<ConductorErrorResponse>(responseContent);

                if (error == null || string.IsNullOrEmpty(error.Message))
                    throw new Exception("Unable to deserialize error");

                _logger.LogError("{@conductorError}", error);

                if (!_restConfig.IgnoreValidationErrors && error?.Message?.Contains("Validation failed") == true)
                    throw new Exception(responseContent);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    throw new Exception(responseContent);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException(error.Message);
            }
        }

        private HttpRequestMessage CreateRequest(Uri relativeUrl, HttpMethod method, object body = null)
        {
            Uri uri;

            if (relativeUrl.IsAbsoluteUri)
                uri = relativeUrl;
            else
                uri = new Uri($"{_restConfig.BaseUrl}/{_restConfig.ApiPath}/{relativeUrl.OriginalString}");

            var httpRequestMessage = new HttpRequestMessage(method, uri);

            if (body != null)
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return httpRequestMessage;
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method)
        {
            var request = CreateRequest(relativeUrl, method);

            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);
        }

        public async Task ExecuteRequestAsync(Uri relativeUrl, HttpMethod method)
        {
            var request = CreateRequest(relativeUrl, method);
            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers)
        {
            var request = CreateRequest(relativeUrl, method, resource);

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers)
        {
            var request = CreateRequest(relativeUrl, method, resource);

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            var response = await _httpClient.SendAsync(request);

            CheckResponse(response);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
