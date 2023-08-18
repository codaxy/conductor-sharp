using ConductorSharp.Client.Exceptions;
using ConductorSharp.Client.Model.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public class ConductorClient : IConductorClient
    {
        private readonly ILogger<ConductorClient> _logger;
        private readonly RestConfig _restConfig;
        private readonly RestClient _restClient;

        public ConductorClient(ILogger<ConductorClient> logger, RestConfig restConfig)
        {
            _logger = logger;
            _restConfig = restConfig;

            if (_restConfig?.CreateClient != null)
                _restClient = _restConfig.CreateClient();
            else
                _restClient = new RestClient();

            // TODO: Make this not required, as we want to allow the user to init client as he likes
            _restClient.UseNewtonsoftJson();
        }

        private void CheckResponse(RestResponse response)
        {
            var bodyParameter = response.Request.Parameters.FirstOrDefault(a => a.Type == ParameterType.RequestBody);
            var body = bodyParameter?.Value == null ? "" : JsonConvert.SerializeObject(bodyParameter.Value);

            if (!response.IsSuccessful)
            {
                ConductorErrorResponse error = default;

                _logger.LogInformation("Received {@Response} with status code {@StatusCode}", response.Content, (int)response.StatusCode);

                error = JsonConvert.DeserializeObject<ConductorErrorResponse>(response.Content);

                if (error == null || string.IsNullOrEmpty(error.Message))
                    throw new Exception("Unable to deserialize error");

                _logger.LogError("{@conductorError}", error);

                if (!_restConfig.IgnoreValidationErrors && error?.Message?.Contains("Validation failed") == true)
                    throw new Exception(response.Content);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    throw new Exception(response.Content);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException(error.Message);
            }
        }

        private RestRequest CreateRequest(Uri relativeUrl, HttpMethod method, object body = null)
        {
            Uri uri;

            if (relativeUrl.IsAbsoluteUri)
                uri = relativeUrl;
            else
                uri = new Uri($"{_restConfig.BaseUrl}/{_restConfig.ApiPath}/{relativeUrl.OriginalString}");

            var request = new RestRequest(uri, (Method)Enum.Parse(typeof(Method), method.Method, true))
            {
                // TODO: This does not work with the latest version of restsharp
                //OnBeforeDeserialization = resp =>
                //{
                //    resp.ContentType = "";
                //}
            };

            if (body != null)
                request.AddJsonBody(body);

            return request;
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            var response = await _restClient.ExecuteAsync<T>(request);

            CheckResponse(response);

            return response.Data;
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method)
        {
            var request = CreateRequest(relativeUrl, method);

            var response = await _restClient.ExecuteAsync(request);

            CheckResponse(response);

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            var response = await _restClient.ExecuteAsync(request);

            CheckResponse(response);
        }

        public async Task ExecuteRequestAsync(Uri relativeUrl, HttpMethod method)
        {
            var request = CreateRequest(relativeUrl, method);
            var response = await _restClient.ExecuteAsync(request);

            CheckResponse(response);
        }

        public async Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            request.AddHeaders(headers);
            var response = await _restClient.ExecuteAsync<T>(request);

            CheckResponse(response);

            return response.Data;
        }

        public async Task<string> ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers)
        {
            var request = CreateRequest(relativeUrl, method, resource);
            request.AddHeaders(headers);
            var response = await _restClient.ExecuteAsync(request);

            CheckResponse(response);

            return response.Content;
        }
    }
}
