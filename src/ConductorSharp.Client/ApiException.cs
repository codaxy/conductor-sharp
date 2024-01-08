using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ConductorSharp.Client.Generated
{
    internal class ApiException : Exception
    {
        public ConductorErrorResponse Errors { get; private set; }
        public int StatusCode { get; private set; }
        public string ResponseData { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

        public ApiException(
            string message,
            int statusCode,
            string responseData,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            JsonException exception
        ) : base(message, exception)
        {
            Errors = JsonConvert.DeserializeObject<ConductorErrorResponse>(responseData);
            StatusCode = statusCode;
            Headers = headers;
            ResponseData = responseData;
        }
    }
}
