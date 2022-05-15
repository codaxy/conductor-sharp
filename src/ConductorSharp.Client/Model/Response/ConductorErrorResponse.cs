using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Response
{

    public class ConductorErrorResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("retryable")]
        public bool Retryable { get; set; }
        [JsonProperty("validationErrors")]
        public List<ConductorValidationError> ValidationErrors { get; set; }
    }
}