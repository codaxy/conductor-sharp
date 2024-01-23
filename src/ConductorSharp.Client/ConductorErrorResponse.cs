using Newtonsoft.Json;

namespace ConductorSharp.Client
{
    public class ConductorValidationError
    {
        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class ConductorErrorResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("retryable")]
        public bool Retryable { get; set; }

        [JsonProperty("validationErrors")]
        public List<ConductorValidationError> ValidationErrors { get; set; } = [];
    }
}
