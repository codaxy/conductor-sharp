using Newtonsoft.Json;

namespace ConductorSharp.Client.Generated
{
    public class ApiException(
        string message,
        int statusCode,
        string responseData,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        JsonException exception
    ) : Exception(message, exception)
    {
        public ConductorErrorResponse? Errors { get; private set; } = JsonConvert.DeserializeObject<ConductorErrorResponse>(responseData);
        public int StatusCode { get; private set; } = statusCode;
        public string ResponseData { get; private set; } = responseData;
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; } = headers;
    }
}
